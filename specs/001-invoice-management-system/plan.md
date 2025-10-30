# Implementation Plan: Company Invoice Management System

**Branch**: `001-invoice-management-system` | **Date**: 2025-10-22 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-invoice-management-system/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Build a RESTful API-based invoice management system for tracking received and issued invoices with role-based access control, payment status tracking, and data export capabilities. The system will use ASP.NET Core WebAPI with PostgreSQL for persistence, following Docker-first development principles with Entity Framework Core migrations for schema management.

## Technical Context

**Language/Version**: C# / .NET 8.0+
**Primary Dependencies**: ASP.NET Core WebAPI, Entity Framework Core, Npgsql.EntityFrameworkCore.PostgreSQL, Swashbuckle.AspNetCore
**Storage**: PostgreSQL 16 (Alpine container)
**Testing**: xUnit with integration tests against containerized database
**Target Platform**: Linux Docker containers (API + PostgreSQL)
**Project Type**: Web API (backend only, API-centric)
**Performance Goals**: Support 100+ concurrent users, <2s response time for list/filter queries on databases with up to 50K invoice records
**Constraints**: All operations via Docker containers, 30-min session timeout, pessimistic locking for edits
**Scale/Scope**: ~50K invoice records, 3 user roles, 8+ API endpoints, single-tenant deployment

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Principle I: Docker-First Development
- ✅ **Compliant**: All development uses `dotnet-docker/dotnet` wrapper
- ✅ **Compliant**: PostgreSQL runs in container with service networking
- ✅ **Compliant**: No host .NET SDK required

### Principle II: API-Centric Design
- ✅ **Compliant**: RESTful API endpoints for all invoice operations
- ✅ **Compliant**: OpenAPI/Swagger documentation required for all endpoints
- ✅ **Compliant**: Explicit DTOs for request/response contracts
- ✅ **Compliant**: REST status codes (200, 201, 400, 404, 409, 500)

### Principle III: Database Migrations as Code
- ✅ **Compliant**: EF Core migrations for all schema changes
- ✅ **Compliant**: Invoice, BusinessPartner, User, AuditLog entities via migrations
- ✅ **Compliant**: Seed data for initial admin user in migration

### Principle IV: Test-Driven Development
- ✅ **Compliant**: Integration tests for API contracts before implementation
- ✅ **Compliant**: Tests use containerized PostgreSQL (no mocks)
- ✅ **Compliant**: Red-Green-Refactor workflow for each endpoint

### Principle V: Container Orchestration
- ✅ **Compliant**: Docker Compose for WebAPI + PostgreSQL
- ✅ **Compliant**: Health checks for PostgreSQL (`pg_isready`)
- ✅ **Compliant**: API depends on database health
- ✅ **Compliant**: Environment variables externalized (`.env` file)

**Result**: ✅ All constitutional principles satisfied. No violations to justify.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
InvoiceManagement/
├── Controllers/
│   ├── InvoicesController.cs
│   ├── UsersController.cs
│   └── AuthController.cs
├── Models/
│   ├── Entities/
│   │   ├── Invoice.cs
│   │   ├── BusinessPartner.cs
│   │   ├── User.cs
│   │   └── AuditLogEntry.cs
│   └── DTOs/
│       ├── InvoiceDto.cs
│       ├── CreateInvoiceRequest.cs
│       ├── UpdateInvoiceRequest.cs
│       ├── InvoiceFilterRequest.cs
│       └── UserDto.cs
├── Services/
│   ├── IInvoiceService.cs
│   ├── InvoiceService.cs
│   ├── IAuthService.cs
│   ├── AuthService.cs
│   └── ILockService.cs (pessimistic locking)
│   └── LockService.cs
├── Data/
│   ├── InvoiceDbContext.cs
│   └── Migrations/
│       └── (EF Core generated migrations)
├── Middleware/
│   ├── SessionTimeoutMiddleware.cs
│   └── ExceptionHandlingMiddleware.cs
└── Program.cs

InvoiceManagement.Tests/
├── Integration/
│   ├── InvoiceEndpointsTests.cs
│   ├── AuthenticationTests.cs
│   └── ConcurrencyTests.cs
└── Unit/
    ├── InvoiceServiceTests.cs
    └── LockServiceTests.cs

dotnet-docker/
├── dotnet (wrapper script)
├── run.sh
├── init-webapi.sh
└── docker-compose.yml

db-init/
└── init.sql (PostgreSQL initialization)
```

**Structure Decision**: Web API project structure following ASP.NET Core conventions. The main `InvoiceManagement` project contains Controllers (API endpoints), Models (entities + DTOs), Services (business logic), and Data (EF Core context). Tests are organized by type (integration tests for API contracts, unit tests for services). Docker tooling remains in `dotnet-docker/` directory per constitution.

## Complexity Tracking

> No constitutional violations - this section is empty.

---

## Phase 0: Research ✅ Complete

**Status**: All technical unknowns resolved
**Output**: [research.md](./research.md)

**Key Decisions**:
1. Pessimistic locking via EF Core + InvoiceLock table
2. ASP.NET Core session management (30-min sliding timeout)
3. Integer storage for amounts (cents) to avoid floating-point errors
4. Database unique constraint for invoice numbers
5. ASP.NET Core Identity for RBAC
6. CsvHelper for streaming CSV exports
7. EF Core SaveChanges interceptor for audit logging
8. Strategic database indexes for query performance
9. Docker Compose with health checks

---

## Phase 1: Design & Contracts ✅ Complete

**Status**: Data model, API contracts, and quickstart guide created
**Outputs**:
- [data-model.md](./data-model.md) - Entity definitions and relationships
- [contracts/openapi.yaml](./contracts/openapi.yaml) - OpenAPI 3.0 specification
- [quickstart.md](./quickstart.md) - Developer getting started guide
- Agent context updated (CLAUDE.md)

**Entities Defined**:
1. User (authentication + roles)
2. BusinessPartner (business partners: suppliers/customers as companies only)
3. Invoice (core entity with 13 fields)
4. InvoiceLock (pessimistic concurrency control with 5-minute expiration)
5. AuditLogEntry (compliance/audit trail)

**API Endpoints** (13 total):
- `POST /api/auth/login` - Authenticate user (session-based)
- `POST /api/auth/logout` - End session
- `GET /api/invoices` - List/filter invoices
- `POST /api/invoices` - Create invoice
- `GET /api/invoices/{id}` - Get invoice details
- `PUT /api/invoices/{id}` - Update invoice
- `DELETE /api/invoices/{id}` - Delete invoice (admin only)
- `POST /api/invoices/{id}/lock` - Acquire edit lock (5-min expiration)
- `DELETE /api/invoices/{id}/lock` - Release edit lock
- `GET /api/invoices/export` - Export to CSV
- `GET /api/users` - List users (admin only)
- `POST /api/users` - Create user (admin only)
- `PUT /api/users/{id}` - Update user (admin only)
- `GET /api/audit-logs` - Retrieve audit log entries (admin only)

**Constitution Re-Check**: ✅ All principles satisfied (no violations)

---

## Next Steps

**Command**: `/speckit.tasks`

This will generate `tasks.md` with dependency-ordered implementation tasks based on:
- User stories prioritization (P1 → P2 → P3)
- TDD workflow (test → implement → refactor)
- Database migrations first, then services, then controllers
- Integration testing for API contracts

**Estimated Implementation**:
- Setup & migrations: 2-4 hours
- Core invoice CRUD (P1): 8-12 hours
- Locking + auth (P2-P3): 6-10 hours
- Testing & refinement: 4-6 hours
- **Total**: ~20-32 hours
