# Research: Company Invoice Management System

**Feature**: 001-invoice-management-system
**Date**: 2025-10-22
**Purpose**: Resolve technical unknowns and establish implementation patterns

## Overview

This document captures research findings for implementing the invoice management system using ASP.NET Core WebAPI with PostgreSQL. All decisions align with the project constitution's Docker-first, API-centric approach.

## Research Areas

### 1. Pessimistic Locking Strategy

**Decision**: Implement database-level row locking with EF Core pessimistic concurrency control

**Rationale**:
- Invoice data is financial and requires strict consistency
- Prevents lost updates and ensures data integrity
- User feedback on locked resources is critical for UX
- PostgreSQL supports `SELECT FOR UPDATE` natively

**Implementation Approach**:
- Use `IsolationLevel.ReadCommitted` with explicit row locking
- Store lock metadata (user ID, timestamp) in separate `InvoiceLock` table
- Implement lock timeout (5 minutes) to prevent indefinite locks from abandoned sessions
- Release locks on save/cancel/session timeout

**Alternatives Considered**:
- Optimistic concurrency (EF Core `RowVersion`): Rejected - spec explicitly requires pessimistic locking
- Application-level locks (in-memory): Rejected - doesn't survive API restarts, not suitable for distributed systems

**References**:
- EF Core concurrency control: https://learn.microsoft.com/en-us/ef/core/saving/concurrency
- PostgreSQL row locking: https://www.postgresql.org/docs/current/explicit-locking.html

---

### 2. Session Management and Timeout

**Decision**: Use ASP.NET Core built-in session middleware with 30-minute sliding expiration

**Rationale**:
- Sliding expiration resets on activity, providing good UX
- Built-in support for distributed cache (Redis) for future scaling
- Integrates with authentication middleware
- Automatic cleanup of expired sessions

**Implementation Approach**:
- Configure `AddSession()` with `IdleTimeout = TimeSpan.FromMinutes(30)`
- Use `SlidingExpiration = true` for activity-based renewal
- Store user identity and role claims in session
- Implement session check middleware for API endpoints

**Alternatives Considered**:
- JWT with fixed expiration: Rejected - doesn't support 30-min inactivity timeout (would require refresh token complexity)
- Cookie-only authentication: Rejected - doesn't provide programmatic session timeout control

**References**:
- ASP.NET Core session state: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state

---

### 3. Integer-Based Currency Storage

**Decision**: Store amounts as `long` (Int64) representing cents, with custom converter for EF Core

**Rationale**:
- Avoids floating-point precision errors in financial calculations
- Standard practice in financial systems (Stripe, payment gateways)
- Supports amounts up to $92 quadrillion (sufficient for invoice management)
- Simple arithmetic operations without rounding errors

**Implementation Approach**:
- Database column: `BIGINT` (PostgreSQL native 64-bit integer)
- C# model: `long Amount` property
- DTO conversion: `decimal` for API contracts (user-friendly), convert to/from cents in service layer
- Example: $123.45 → 12345 (cents)

**Alternatives Considered**:
- `decimal` type: Rejected - spec requires integer storage
- `int` (32-bit): Rejected - insufficient range for large invoices (max ~$21 million)
- String representation: Rejected - complicates arithmetic operations and queries

**References**:
- Stripe money representation: https://stripe.com/docs/api/balance_transactions/object
- PostgreSQL numeric types: https://www.postgresql.org/docs/current/datatype-numeric.html

---

### 4. Global Unique Invoice Number Enforcement

**Decision**: Use unique constraint on `InvoiceNumber` column with database index

**Rationale**:
- Database-level constraint provides ACID guarantees
- Prevents race conditions when creating invoices concurrently
- Fast lookup performance via index for validation
- Clear error message on constraint violation (409 Conflict)

**Implementation Approach**:
- EF Core migration: `builder.HasIndex(i => i.InvoiceNumber).IsUnique()`
- PostgreSQL generates: `CREATE UNIQUE INDEX idx_invoice_number ON invoices(invoice_number)`
- Service layer catches `DbUpdateException` with unique constraint violation
- Return HTTP 409 Conflict with clear message: "Invoice number {number} already exists"

**Alternatives Considered**:
- Application-level validation only: Rejected - race conditions possible
- GUID/auto-increment IDs: Rejected - spec requires user-provided invoice numbers
- Composite key (number + type): Rejected - spec requires global uniqueness

**References**:
- EF Core indexes: https://learn.microsoft.com/en-us/ef/core/modeling/indexes
- PostgreSQL unique constraints: https://www.postgresql.org/docs/current/ddl-constraints.html#DDL-CONSTRAINTS-UNIQUE-CONSTRAINTS

---

### 5. Role-Based Access Control (RBAC)

**Decision**: Use ASP.NET Core Identity with custom roles and policy-based authorization

**Rationale**:
- Built-in support for user management, password hashing, role assignment
- Declarative authorization via `[Authorize(Roles = "...")]` attributes
- Extensible for future claims-based permissions
- Integrates with EF Core for user/role storage

**Implementation Approach**:
- Define roles: `Accountant`, `Manager`, `Administrator`
- Seed admin user in EF Core migration
- Use `[Authorize(Roles = "Accountant,Administrator")]` on create/edit endpoints
- Use `[Authorize(Roles = "Administrator")]` on delete endpoints
- All authenticated users can view/filter (read-only for Managers)

**Alternatives Considered**:
- Custom JWT claims: Rejected - adds complexity, Identity provides all needed features
- Hardcoded user table: Rejected - reinvents password hashing and security features
- External auth (OAuth2): Deferred - spec allows session-based auth for MVP

**References**:
- ASP.NET Core Identity: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity
- Authorization policies: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies

---

### 6. CSV Export Implementation

**Decision**: Use CsvHelper library with streaming for large datasets

**Rationale**:
- Industry-standard library with 50M+ downloads
- Supports streaming for memory efficiency (10K+ records)
- Configurable field mapping and formatting
- Handles edge cases (special characters, quotes, newlines)

**Implementation Approach**:
- Endpoint: `GET /api/invoices/export?{filters}`
- Apply same filters as list endpoint
- Stream results to CSV using `CsvWriter`
- Return `FileStreamResult` with `text/csv` content type
- Filename: `invoices_{timestamp}.csv`

**CSV Schema**:
```
InvoiceNumber,IssueDate,DueDate,Type,PartnerName,AmountCents,PaymentStatus,PaymentDate
INV-001,2025-01-15,2025-02-15,Issued,Acme Corp,125000,Paid,2025-02-10
```

**Alternatives Considered**:
- Manual CSV generation: Rejected - error-prone, doesn't handle edge cases
- Excel (XLSX): Deferred - spec says "CSV or equivalent", CSV is simpler
- JSON export: Possible addition but CSV is primary for accounting software

**References**:
- CsvHelper: https://joshclose.github.io/CsvHelper/
- ASP.NET Core file results: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/file-results

---

### 7. Audit Logging for Deletions

**Decision**: EF Core `SaveChanges` interceptor with dedicated `AuditLog` table

**Rationale**:
- Centralized audit logic (no code duplication across controllers)
- Automatic tracking of all delete operations
- Immutable audit records (append-only table)
- Queryable for compliance reports

**Implementation Approach**:
- Create `AuditLogEntry` entity: `Id`, `Timestamp`, `UserId`, `UserName`, `Action`, `InvoiceId`, `InvoiceNumber`, `Details`
- Implement `SaveChangesInterceptor` to detect `EntityState.Deleted` for invoices
- Capture current user from `IHttpContextAccessor`
- Write audit entry before delete commits

**Alternatives Considered**:
- Manual logging in controller: Rejected - easy to forget, not DRY
- Trigger-based logging (PostgreSQL): Rejected - violates "migrations as code" principle
- External audit service: Rejected - adds complexity for MVP

**References**:
- EF Core interceptors: https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors
- Audit logging patterns: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/

---

### 8. Performance: Indexing Strategy

**Decision**: Create indexes on frequently queried columns

**Primary Indexes** (via EF Core migrations):
- `InvoiceNumber` (unique) - for duplicate checking and search
- `IssueDate` - for date range filtering
- `DueDate` - for overdue invoice queries
- `PaymentStatus` - for paid/unpaid filtering
- `PartnerName` - for partner search (consider full-text index for partial matching)

**Composite Indexes**:
- `(PaymentStatus, DueDate)` - for "overdue unpaid" queries
- `(PartnerName, IssueDate)` - for partner-specific date filtering

**Rationale**:
- Supports <2s query time for 50K records (spec SC-004)
- Minimal overhead on inserts (invoices are created less frequently than queried)
- PostgreSQL B-tree indexes are optimal for range queries (dates)

**Implementation**: Add in EF Core migration after entities are defined

**References**:
- EF Core indexes: https://learn.microsoft.com/en-us/ef/core/modeling/indexes
- PostgreSQL index types: https://www.postgresql.org/docs/current/indexes-types.html

---

### 9. Docker Compose Configuration

**Decision**: Extend existing `dotnet-docker/docker-compose.yml` for invoice API service

**Key Configuration**:
```yaml
services:
  postgres:
    # (already configured in project)
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER}"]
      interval: 5s
      timeout: 5s
      retries: 5

  invoice-api:
    build:
      context: ../InvoiceManagement
      dockerfile: Dockerfile
    ports:
      - "${API_PORT:-5000}:8080"
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
      - ASPNETCORE_ENVIRONMENT=Development
    depends_on:
      postgres:
        condition: service_healthy
    volumes:
      - ../InvoiceManagement:/app
    command: dotnet watch run
```

**Rationale**:
- Health check ensures database is ready before API starts
- Volume mounting enables hot reload during development
- Environment variables externalized to `.env` file
- API depends on database health (constitution requirement)

**References**:
- Docker Compose health checks: https://docs.docker.com/compose/compose-file/05-services/#healthcheck
- ASP.NET Core Docker: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/

---

## Summary of Decisions

| Area | Decision | Key Benefit |
|------|----------|-------------|
| Pessimistic Locking | EF Core + InvoiceLock table | Data consistency for financial records |
| Session Management | ASP.NET Core session (30-min sliding) | Built-in timeout with activity renewal |
| Currency Storage | `long` (cents) | Eliminates floating-point errors |
| Unique Invoices | Database unique constraint + index | ACID guarantees, fast validation |
| RBAC | ASP.NET Core Identity | Built-in security, password hashing, roles |
| CSV Export | CsvHelper with streaming | Memory-efficient for 10K+ records |
| Audit Logging | EF Core interceptor | Automatic, centralized, queryable |
| Performance | Strategic indexes on query columns | <2s response for 50K records |
| Docker Compose | Health checks + volume mounts | Constitution compliance, hot reload |

## Next Steps

Proceed to Phase 1: Design & Contracts
- Create `data-model.md` with entity definitions
- Generate OpenAPI contracts in `/contracts/`
- Write `quickstart.md` for running the system
