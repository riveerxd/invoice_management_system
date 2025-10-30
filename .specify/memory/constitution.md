<!--
SYNC IMPACT REPORT
==================
Version Change: 0.0.0 → 1.0.0
Rationale: Initial constitution establishment for .NET WebAPI project

Added Principles:
- I. Docker-First Development
- II. API-Centric Design
- III. Database Migrations as Code
- IV. Test-Driven Development
- V. Container Orchestration

Added Sections:
- Technology Stack
- Development Workflow
- Governance

Templates Requiring Updates:
✅ .specify/templates/plan-template.md - Constitution Check section aligned
✅ .specify/templates/spec-template.md - Requirements align with API-first principle
✅ .specify/templates/tasks-template.md - Task categorization supports principles
✅ .claude/commands/speckit.constitution.md - Generic guidance maintained

Follow-up TODOs:
- None - all placeholders filled
-->

# OpenAPI Swagger Project Constitution

## Core Principles

### I. Docker-First Development

All development tooling MUST run inside Docker containers. The .NET SDK is NOT required
on the host machine. Every dotnet command MUST be executed via the wrapper script
`dotnet-docker/dotnet {command}`.

**Rationale**: Ensures consistent development environment across all machines, eliminates
"works on my machine" issues, and simplifies onboarding. The PostgreSQL database runs in
a container alongside the API, maintaining environment parity from development through
production.

**Non-negotiable rules**:
- NEVER invoke `dotnet` directly from the host
- ALWAYS use `dotnet-docker/dotnet` wrapper for CLI operations
- Database connections MUST use container networking (service name `postgres`)
- All dependencies MUST be containerized

### II. API-Centric Design

Features MUST be designed as RESTful API endpoints first. Every capability is exposed
via HTTP contracts with clear request/response schemas.

**Rationale**: API-first design ensures loose coupling, enables multiple client types
(web, mobile, CLI), and makes integration testing straightforward. OpenAPI/Swagger
documentation becomes the single source of truth for contracts.

**Non-negotiable rules**:
- All endpoints MUST have OpenAPI/Swagger documentation
- Request/response DTOs MUST be explicitly defined (no anonymous types)
- HTTP status codes MUST follow REST conventions (200, 201, 400, 404, 500, etc.)
- API versioning MUST be implemented for breaking changes

### III. Database Migrations as Code

All database schema changes MUST be expressed as Entity Framework Core migrations.
Direct SQL modifications to the database are prohibited.

**Rationale**: Migration files provide version control for the database schema, enable
rollback capabilities, and ensure schema changes are reproducible across environments.

**Non-negotiable rules**:
- Schema changes MUST use `dotnet-docker/dotnet ef migrations add {Name}`
- Migrations MUST be reviewed in pull requests before merge
- `dotnet-docker/dotnet ef database update` MUST succeed before deployment
- Seed data SHOULD be included in migrations when appropriate

### IV. Test-Driven Development

Tests are written BEFORE implementation. Tests MUST fail initially, then pass after
the feature is implemented (Red-Green-Refactor cycle).

**Rationale**: TDD ensures code is testable, reduces defects, and provides living
documentation of expected behavior. For APIs, contract tests validate endpoint
behavior independently.

**Non-negotiable rules**:
- Integration tests MUST cover API endpoint contracts
- Tests MUST use the containerized environment (not mocks for database)
- Test execution MUST use `dotnet-docker/dotnet test`
- Tests MUST pass before PR merge

### V. Container Orchestration

The application stack (WebAPI + PostgreSQL) MUST be orchestrated via Docker Compose.
Services MUST be defined with health checks, dependencies, and environment configuration.

**Rationale**: Docker Compose provides a declarative way to manage multi-container
applications, ensures services start in correct order, and simplifies local development
and CI/CD pipelines.

**Non-negotiable rules**:
- Services MUST define health checks (e.g., `pg_isready` for PostgreSQL)
- API service MUST depend on PostgreSQL health check
- Environment variables MUST be externalized (use `.env` file)
- Volumes MUST be used for database persistence

## Technology Stack

**Language**: C# / .NET 8.0+
**Framework**: ASP.NET Core WebAPI
**Database**: PostgreSQL 16 (Alpine)
**ORM**: Entity Framework Core
**Containerization**: Docker + Docker Compose
**Documentation**: Swagger/OpenAPI
**Testing**: xUnit or NUnit (via `dotnet-docker/dotnet test`)

**Key Dependencies**:
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Design
- Npgsql.EntityFrameworkCore.PostgreSQL
- Swashbuckle.AspNetCore (Swagger)

**Development Tools**:
- `dotnet-docker/dotnet` CLI wrapper
- `dotnet-docker/run.sh` for project execution
- `dotnet-docker/init-webapi.sh` for project scaffolding

## Development Workflow

### Local Development

1. Use `dotnet-docker/dotnet {command}` for all .NET operations
2. Start stack with `docker-compose up` from `dotnet-docker/` directory
3. API available at `http://localhost:5000` (or configured `API_PORT`)
4. PostgreSQL available at `localhost:5432` (or configured `DB_PORT`)

### Making Schema Changes

1. Create migration: `dotnet-docker/dotnet ef migrations add {MigrationName}`
2. Review generated migration files in pull request
3. Apply migration: `dotnet-docker/dotnet ef database update`
4. Verify schema changes in PostgreSQL container

### Feature Development

1. Write integration tests for API endpoints (Red)
2. Implement endpoint, service, and data layers (Green)
3. Refactor for clarity and performance (Refactor)
4. Update Swagger documentation (auto-generated + annotations)
5. Run full test suite: `dotnet-docker/dotnet test`

### Pull Request Requirements

- All tests passing (`dotnet-docker/dotnet test`)
- Migrations reviewed and applied successfully
- Swagger documentation reflects new/changed endpoints
- Docker Compose stack starts cleanly (`docker-compose up`)

## Governance

This constitution is the authoritative source of development standards for the project.
All contributors MUST adhere to these principles.

**Amendment Procedure**:
- Proposed changes MUST be documented with rationale
- Breaking changes require MAJOR version bump
- New principles or expanded guidance require MINOR version bump
- Clarifications and typo fixes require PATCH version bump
- Amendments MUST be approved via pull request review

**Compliance Review**:
- Pull requests MUST reference compliance with relevant principles
- Code reviews MUST verify adherence to Docker-first, API-centric, and migration rules
- CI pipelines SHOULD enforce test execution and migration validation

**Complexity Justification**:
- Violations of principles (e.g., bypassing Docker, direct SQL) MUST be justified
- Simpler alternatives MUST be documented as rejected with reasoning
- Complexity must provide measurable value (performance, security, scalability)

**Version**: 1.0.0 | **Ratified**: 2025-10-22 | **Last Amended**: 2025-10-22
