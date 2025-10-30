# Tasks: Company Invoice Management System

**Input**: Design documents from `/specs/001-invoice-management-system/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/openapi.yaml

**Tests**: Following TDD principles per constitution - tests are REQUIRED and written BEFORE implementation

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Project structure follows ASP.NET Core WebAPI conventions:
- Main project: `InvoiceManagement/`
- Test project: `InvoiceManagement.Tests/`
- Docker tooling: `dotnet-docker/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic Docker-based development environment

- [X] T001 Initialize WebAPI project using dotnet-docker/dotnet new webapi -n InvoiceManagement (via docker wrapper per constitution)
- [X] T002 [P] Add NuGet packages: Microsoft.EntityFrameworkCore, Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.Design, Swashbuckle.AspNetCore in InvoiceManagement/InvoiceManagement.csproj
- [X] T003 [P] Add NuGet packages: Microsoft.AspNetCore.Identity.EntityFrameworkCore, CsvHelper in InvoiceManagement/InvoiceManagement.csproj
- [X] T004 [P] Configure Docker Compose for API + PostgreSQL in dotnet-docker/docker-compose.yml with health checks
- [X] T005 [P] Create .env file in dotnet-docker/ with DB credentials, API port, session/lock timeouts
- [X] T006 [P] Initialize test project using dotnet-docker/dotnet new xunit -n InvoiceManagement.Tests
- [X] T007 [P] Add test dependencies: Microsoft.AspNetCore.Mvc.Testing, Microsoft.EntityFrameworkCore.InMemory in InvoiceManagement.Tests/InvoiceManagement.Tests.csproj

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T008 Create enums in InvoiceManagement/Models/InvoiceType.cs (Received, Issued)
- [X] T009 [P] Create enums in InvoiceManagement/Models/PaymentStatus.cs (Paid, Unpaid)
- [X] T010 [P] Create enums in InvoiceManagement/Models/UserRole.cs (Accountant, Manager, Administrator)
- [X] T011 Create User entity in InvoiceManagement/Models/Entities/User.cs (Id, Username, Email, PasswordHash, Role, IsActive, CreatedAt, UpdatedAt)
- [X] T012 [P] Create BusinessPartner entity in InvoiceManagement/Models/Entities/BusinessPartner.cs (Id, Name, Identifier, CreatedAt, UpdatedAt)
- [X] T013 [P] Create Invoice entity in InvoiceManagement/Models/Entities/Invoice.cs (Id, InvoiceNumber, IssueDate, DueDate, Type, BusinessPartnerId, AmountCents, PaymentStatus, PaymentDate, CreatedById, ModifiedById, CreatedAt, UpdatedAt)
- [X] T014 [P] Create InvoiceLock entity in InvoiceManagement/Models/Entities/InvoiceLock.cs (InvoiceId, LockedByUserId, LockedByUserName, LockAcquiredAt, LockExpiresAt)
- [X] T015 [P] Create AuditLogEntry entity in InvoiceManagement/Models/Entities/AuditLogEntry.cs (Id, Timestamp, UserId, UserName, Action, InvoiceId, InvoiceNumber, Details)
- [X] T016 Create InvoiceDbContext in InvoiceManagement/Data/InvoiceDbContext.cs with DbSets for all entities
- [X] T017 Configure entity relationships and constraints in InvoiceManagement/Data/InvoiceDbContext.cs (OnModelCreating: unique constraints, indexes, foreign keys)
- [X] T018 Create EF Core migration: dotnet-docker/dotnet ef migrations add CreateInvoiceSchema --project InvoiceManagement
- [X] T019 Review and validate migration files in InvoiceManagement/Data/Migrations/
- [X] T020 Add seed data for admin user in migration (username: admin, role: Administrator)
- [X] T021 Apply migration: dotnet-docker/dotnet ef database update --project InvoiceManagement
- [X] T022 Configure session management in InvoiceManagement/Program.cs (AddSession with 30-min IdleTimeout)
- [ ] T022a [P] [US6] Integration test: Verify session timeout after 30 minutes of inactivity in InvoiceManagement.Tests/Integration/SessionTimeoutTests.cs
- [X] T023 [P] Configure ASP.NET Core Identity in InvoiceManagement/Program.cs (AddIdentity with User entity)
- [X] T024 [P] Add connection string configuration in InvoiceManagement/appsettings.json using environment variable substitution pattern ${DB_CONNECTION_STRING}
- [X] T025 [P] Create SessionTimeoutMiddleware in InvoiceManagement/Middleware/SessionTimeoutMiddleware.cs
- [X] T026 [P] Create ExceptionHandlingMiddleware in InvoiceManagement/Middleware/ExceptionHandlingMiddleware.cs
- [X] T027 Register middleware in InvoiceManagement/Program.cs (UseSession, UseAuthentication, UseAuthorization, custom middleware)
- [X] T028 Configure Swagger in InvoiceManagement/Program.cs with security definitions for session auth
- [X] T029 Create WebApplicationFactory for integration tests in InvoiceManagement.Tests/TestWebApplicationFactory.cs
- [X] T030 Verify stack starts cleanly: docker-compose up in dotnet-docker/ and check health

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Basic Invoice Entry (Priority: P1) 🎯 MVP

**Goal**: Enable accountants to create and view invoices with validation

**Independent Test**: Create invoice via API, retrieve it, verify all fields match. Try creating with missing fields and verify validation errors.

### Tests for User Story 1 ⚠️

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T031 [P] [US1] Integration test: POST /api/invoices with valid data returns 201 in InvoiceManagement.Tests/Integration/InvoiceCreationTests.cs
- [X] T032 [P] [US1] Integration test: POST /api/invoices with missing required fields returns 400 in InvoiceManagement.Tests/Integration/InvoiceCreationTests.cs
- [X] T033 [P] [US1] Integration test: GET /api/invoices/{id} returns invoice details in InvoiceManagement.Tests/Integration/InvoiceRetrievalTests.cs
- [X] T034 [P] [US1] Integration test: POST /api/invoices with duplicate invoice number returns 409 in InvoiceManagement.Tests/Integration/InvoiceCreationTests.cs
- [X] T034a [P] [US2] Integration test: GET /api/invoices with no matching filters returns 200 with empty array in InvoiceManagement.Tests/Integration/InvoiceFilteringTests.cs

### Implementation for User Story 1

- [X] T035 [P] [US1] Create CreateInvoiceRequest DTO in InvoiceManagement/Models/DTOs/CreateInvoiceRequest.cs
- [X] T036 [P] [US1] Create InvoiceResponse DTO in InvoiceManagement/Models/DTOs/InvoiceResponse.cs
- [X] T037 [P] [US1] Create IInvoiceService interface in InvoiceManagement/Services/IInvoiceService.cs (CreateAsync, GetByIdAsync methods)
- [X] T038 [US1] Implement InvoiceService.CreateAsync in InvoiceManagement/Services/InvoiceService.cs (validate, check duplicates, create invoice, create/link business partner)
- [X] T039 [US1] Implement InvoiceService.GetByIdAsync in InvoiceManagement/Services/InvoiceService.cs (retrieve with includes)
- [X] T040 [US1] Create InvoicesController in InvoiceManagement/Controllers/InvoicesController.cs with POST and GET {id} endpoints
- [X] T041 [US1] Add [Authorize(Roles = "Accountant,Administrator")] attribute to POST endpoint in InvoicesController
- [X] T042 [US1] Add validation logic in CreateAsync: required fields, invoice number format, date validations in InvoiceService.cs
- [X] T043 [US1] Add error handling: catch DbUpdateException for unique constraint violations (duplicate invoice numbers) in InvoiceService.cs
- [ ] T044 [US1] Run tests: dotnet-docker/dotnet test InvoiceManagement.Tests --filter Category=US1
- [ ] T045 [US1] Verify all US1 tests pass and invoice creation works via Swagger UI

**Checkpoint**: At this point, User Story 1 should be fully functional - can create and retrieve invoices with proper validation

---

## Phase 4: User Story 2 - Invoice Listing and Filtering (Priority: P1)

**Goal**: Enable users to query invoices with multiple filter criteria and pagination

**Independent Test**: Create 10+ invoices with varying attributes, apply filters (date range, payment status, partner name), verify correct results returned. Test pagination.

### Tests for User Story 2 ⚠️

- [X] T046 [P] [US2] Integration test: GET /api/invoices returns all invoices with pagination in InvoiceManagement.Tests/Integration/InvoiceListingTests.cs
- [X] T047 [P] [US2] Integration test: Filter by date range returns correct invoices in InvoiceManagement.Tests/Integration/InvoiceFilteringTests.cs
- [X] T048 [P] [US2] Integration test: Filter by payment status (Paid/Unpaid) returns correct invoices in InvoiceManagement.Tests/Integration/InvoiceFilteringTests.cs
- [X] T049 [P] [US2] Integration test: Filter by partner name returns correct invoices in InvoiceManagement.Tests/Integration/InvoiceFilteringTests.cs
- [X] T050 [P] [US2] Integration test: Combine multiple filters returns correct invoices in InvoiceManagement.Tests/Integration/InvoiceFilteringTests.cs

### Implementation for User Story 2

- [X] T051 [P] [US2] Create InvoiceFilterRequest DTO in InvoiceManagement/Models/DTOs/InvoiceFilterRequest.cs (all filter fields + pagination)
- [X] T052 [P] [US2] Create InvoiceListResponse DTO in InvoiceManagement/Models/DTOs/InvoiceListResponse.cs (items, pagination, summary)
- [X] T053 [US2] Implement IInvoiceService.GetFilteredAsync in IInvoiceService interface
- [X] T054 [US2] Implement InvoiceService.GetFilteredAsync in InvoiceService.cs (apply filters with IQueryable, pagination, calculate summary, handle empty results)
- [X] T055 [US2] Add GET /api/invoices endpoint in InvoicesController with filter parameters
- [X] T056 [US2] Add overdue calculation logic (isOverdue field in response) in InvoiceService.cs
- [X] T057 [US2] Add summary aggregation (totalPaidCents, totalUnpaidCents, counts) in InvoiceService.cs
- [ ] T058 [US2] Run tests: dotnet-docker/dotnet test InvoiceManagement.Tests --filter Category=US2
- [ ] T059 [US2] Verify filtering and pagination work via Swagger UI with sample data

**Checkpoint**: User Stories 1 AND 2 work independently - can create invoices and query them with filters

---

## Phase 5: User Story 3 - Invoice Modification (Priority: P2)

**Goal**: Enable accountants to edit existing invoices with pessimistic locking

**Independent Test**: Create invoice, acquire lock, update fields, save. Verify another user cannot edit while locked. Verify lock releases on save/cancel/timeout.

### Tests for User Story 3 ⚠️

- [ ] T060 [P] [US3] Integration test: PUT /api/invoices/{id} updates invoice successfully in InvoiceManagement.Tests/Integration/InvoiceUpdateTests.cs
- [ ] T061 [P] [US3] Integration test: PUT with invalid data returns 400 in InvoiceManagement.Tests/Integration/InvoiceUpdateTests.cs
- [ ] T062 [P] [US3] Integration test: Concurrent edit attempt returns 409 (locked) in InvoiceManagement.Tests/Integration/ConcurrencyTests.cs
- [ ] T063 [P] [US3] Integration test: Lock expires automatically after 5 minutes (FR-004b) in InvoiceManagement.Tests/Integration/ConcurrencyTests.cs
- [ ] T064 [P] [US3] Integration test: User without edit permission (Manager) gets 403 in InvoiceManagement.Tests/Integration/AuthorizationTests.cs

### Implementation for User Story 3

- [X] T065 [P] [US3] Create UpdateInvoiceRequest DTO in InvoiceManagement/Models/DTOs/UpdateInvoiceRequest.cs
- [X] T066 [P] [US3] Create LockResponse DTO in InvoiceManagement/Models/DTOs/LockResponse.cs (include lockedBy, lockedByUserName, lockExpiresAt per FR-031a)
- [X] T067 [P] [US3] Create ILockService interface in InvoiceManagement/Services/ILockService.cs (AcquireLockAsync, ReleaseLockAsync, IsLockedAsync)
- [X] T068 [US3] Implement LockService in InvoiceManagement/Services/LockService.cs with pessimistic locking logic (5-minute expiration per FR-004b)
- [X] T069 [US3] Implement IInvoiceService.UpdateAsync in IInvoiceService interface
- [X] T070 [US3] Implement InvoiceService.UpdateAsync in InvoiceService.cs (check lock, validate, update, track ModifiedBy)
- [X] T071 [US3] Add PUT /api/invoices/{id} endpoint in InvoicesController
- [X] T072 [US3] Add POST /api/invoices/{id}/lock endpoint in InvoicesController
- [X] T073 [US3] Add DELETE /api/invoices/{id}/lock endpoint in InvoicesController
- [X] T073a [P] [US3] Return 409 with LockResponse details when invoice is locked (FR-031a) in InvoicesController PUT endpoint
- [X] T074 [US3] Add [Authorize(Roles = "Accountant,Administrator")] to edit endpoints in InvoicesController
- [X] T075 [US3] Implement lock cleanup background task for expired locks using IHostedService with timer in InvoiceManagement/Services/LockCleanupService.cs
- [ ] T076 [US3] Run tests: dotnet-docker/dotnet test InvoiceManagement.Tests --filter Category=US3
- [ ] T077 [US3] Verify pessimistic locking works via Swagger UI (two browser sessions)

**Checkpoint**: User Stories 1, 2, AND 3 all work independently - full CRUD with locking

---

## Phase 6: User Story 4 - Payment Status Tracking (Priority: P2)

**Goal**: Track which invoices are paid/unpaid and identify overdue invoices

**Independent Test**: Create unpaid invoice, mark as paid with payment date. Filter for paid/unpaid invoices. Verify overdue calculation for unpaid past-due invoices.

### Tests for User Story 4 ⚠️

- [ ] T078 [P] [US4] Integration test: Update invoice to mark as Paid with payment date in InvoiceManagement.Tests/Integration/PaymentStatusTests.cs
- [ ] T079 [P] [US4] Integration test: Filter by paymentStatus=Unpaid returns only unpaid in InvoiceManagement.Tests/Integration/PaymentStatusTests.cs
- [ ] T080 [P] [US4] Integration test: Overdue invoices flagged correctly (unpaid + past due date) in InvoiceManagement.Tests/Integration/PaymentStatusTests.cs
- [ ] T081 [P] [US4] Integration test: Summary totals calculate correctly for paid/unpaid in InvoiceManagement.Tests/Integration/PaymentStatusTests.cs

### Implementation for User Story 4

- [X] T082 [US4] Extend UpdateInvoiceRequest to include paymentStatus and paymentDate fields in UpdateInvoiceRequest.cs
- [X] T083 [US4] Add validation in InvoiceService.UpdateAsync: paymentDate required when Paid, must be >= issueDate
- [X] T084 [US4] Enhance GetFilteredAsync to handle paymentStatus filter in InvoiceService.cs
- [X] T085 [US4] Add isOverdue calculation in InvoiceResponse DTO mapping (unpaid && dueDate < today) in InvoiceService.cs
- [X] T086 [US4] Update summary calculation to include overdueCount in InvoiceListResponse in InvoiceService.cs
- [ ] T087 [US4] Add visual indication for overdue in Swagger documentation (update OpenAPI annotations)
- [ ] T088 [US4] Run tests: dotnet-docker/dotnet test InvoiceManagement.Tests --filter Category=US4
- [ ] T089 [US4] Verify payment tracking works via Swagger UI

**Checkpoint**: Payment status tracking complete - can mark paid, filter, see overdue

---

## Phase 7: User Story 5 - Data Export for Accounting (Priority: P2)

**Goal**: Export filtered invoices to CSV for external accounting software

**Independent Test**: Apply filters, export to CSV, open in Excel/LibreOffice, verify all fields present and correct.

### Tests for User Story 5 ⚠️

- [ ] T090 [P] [US5] Integration test: GET /api/invoices/export returns CSV file in InvoiceManagement.Tests/Integration/ExportTests.cs
- [ ] T091 [P] [US5] Integration test: CSV export respects filters (only exports filtered invoices) in InvoiceManagement.Tests/Integration/ExportTests.cs
- [ ] T092 [P] [US5] Integration test: CSV contains all required fields in correct format in InvoiceManagement.Tests/Integration/ExportTests.cs
- [ ] T093 [P] [US5] Integration test: Large dataset export (10,000+ invoices) completes in <30s per SC-006 in InvoiceManagement.Tests/Integration/ExportTests.cs

### Implementation for User Story 5

- [X] T094 [US5] Add CsvHelper dependency to InvoiceManagement.csproj (already in T003 but verify)
- [X] T095 [US5] Create InvoiceCsvRecord class in InvoiceManagement/Models/DTOs/InvoiceCsvRecord.cs (flat structure for CSV)
- [X] T096 [US5] Implement IInvoiceService.ExportToCsvAsync in IInvoiceService interface
- [X] T097 [US5] Implement InvoiceService.ExportToCsvAsync in InvoiceService.cs with streaming using CsvWriter
- [X] T098 [US5] Add GET /api/invoices/export endpoint in InvoicesController returning FileStreamResult
- [X] T099 [US5] Configure CSV mapping with proper field names and formatting in ExportToCsvAsync
- [X] T100 [US5] Add Content-Disposition header with filename: invoices_{timestamp}.csv in InvoicesController
- [ ] T101 [US5] Run tests: dotnet-docker/dotnet test InvoiceManagement.Tests --filter Category=US5
- [ ] T102 [US5] Verify CSV export works via Swagger UI and opens correctly in spreadsheet software

**Checkpoint**: Export functionality complete - can export filtered data to CSV

---

## Phase 8: User Story 6 - Role-Based Access Control (Priority: P3)

**Goal**: Enforce role-based permissions (Accountant, Manager, Administrator)

**Independent Test**: Create users with different roles, attempt operations, verify Manager can only read, Accountant can create/edit, Administrator can delete.

### Tests for User Story 6 ⚠️

- [ ] T103 [P] [US6] Integration test: Manager role cannot POST /api/invoices (403) in InvoiceManagement.Tests/Integration/RoleTests.cs
- [ ] T104 [P] [US6] Integration test: Manager role can GET /api/invoices (200) in InvoiceManagement.Tests/Integration/RoleTests.cs
- [ ] T105 [P] [US6] Integration test: Accountant role cannot DELETE /api/invoices/{id} (403) in InvoiceManagement.Tests/Integration/RoleTests.cs
- [ ] T106 [P] [US6] Integration test: Administrator can manage users in InvoiceManagement.Tests/Integration/UserManagementTests.cs

### Implementation for User Story 6

- [ ] T107 [P] [US6] Create UserResponse DTO in InvoiceManagement/Models/DTOs/UserResponse.cs
- [ ] T108 [P] [US6] Create CreateUserRequest DTO in InvoiceManagement/Models/DTOs/CreateUserRequest.cs
- [ ] T109 [P] [US6] Create UpdateUserRequest DTO in InvoiceManagement/Models/DTOs/UpdateUserRequest.cs
- [ ] T110 [P] [US6] Create LoginRequest DTO in InvoiceManagement/Models/DTOs/LoginRequest.cs
- [ ] T111 [P] [US6] Create LoginResponse DTO in InvoiceManagement/Models/DTOs/LoginResponse.cs
- [ ] T112 [P] [US6] Create IAuthService interface in InvoiceManagement/Services/IAuthService.cs (LoginAsync, LogoutAsync)
- [ ] T113 [US6] Implement AuthService in InvoiceManagement/Services/AuthService.cs using ASP.NET Core Identity
- [ ] T114 [P] [US6] Create IUserService interface in InvoiceManagement/Services/IUserService.cs (CreateAsync, UpdateAsync, GetAllAsync)
- [ ] T115 [US6] Implement UserService in InvoiceManagement/Services/UserService.cs
- [ ] T116 [US6] Create AuthController in InvoiceManagement/Controllers/AuthController.cs with POST /api/auth/login and POST /api/auth/logout
- [ ] T117 [US6] Create UsersController in InvoiceManagement/Controllers/UsersController.cs with GET, POST, PUT endpoints
- [ ] T117a [P] [US6] Add PATCH /api/users/{id}/status endpoint for user activation/deactivation toggle (FR-026) in UsersController
- [ ] T118 [US6] Add [Authorize(Roles = "Administrator")] to user management endpoints in UsersController
- [ ] T119 [US6] Verify existing invoice endpoints have correct role restrictions (Accountant/Admin for create/edit, Manager can read)
- [ ] T120 [US6] Run tests: dotnet-docker/dotnet test InvoiceManagement.Tests --filter Category=US6
- [ ] T121 [US6] Verify role-based access via Swagger UI with different user accounts

**Checkpoint**: RBAC complete - all roles enforced correctly

---

## Phase 9: User Story 7 - Invoice Deletion (Priority: P3)

**Goal**: Allow administrators to delete invoices with automatic audit logging

**Independent Test**: Login as admin, delete invoice, verify it's removed from GET requests. Check audit log contains deletion record with who/when/what.

### Tests for User Story 7 ⚠️

- [ ] T122 [P] [US7] Integration test: DELETE /api/invoices/{id} as Administrator returns 204 in InvoiceManagement.Tests/Integration/InvoiceDeletionTests.cs
- [ ] T123 [P] [US7] Integration test: DELETE as non-Administrator returns 403 in InvoiceManagement.Tests/Integration/InvoiceDeletionTests.cs
- [ ] T124 [P] [US7] Integration test: Deleted invoice creates audit log entry in InvoiceManagement.Tests/Integration/AuditLogTests.cs
- [ ] T125 [P] [US7] Integration test: Audit log contains userId, userName, invoiceId, invoiceNumber in InvoiceManagement.Tests/Integration/AuditLogTests.cs

### Implementation for User Story 7

- [ ] T126 [US7] Create SaveChangesInterceptor in InvoiceManagement/Data/AuditInterceptor.cs to detect EntityState.Deleted
- [ ] T127 [US7] Implement audit logging logic in AuditInterceptor to create AuditLogEntry before delete commits
- [ ] T128 [US7] Register AuditInterceptor in InvoiceDbContext.OnConfiguring
- [ ] T129 [US7] Implement IInvoiceService.DeleteAsync in IInvoiceService interface
- [ ] T130 [US7] Implement InvoiceService.DeleteAsync in InvoiceService.cs
- [ ] T131 [US7] Add DELETE /api/invoices/{id} endpoint in InvoicesController
- [ ] T132 [US7] Add [Authorize(Roles = "Administrator")] to DELETE endpoint in InvoicesController
- [ ] T133 [US7] Add IHttpContextAccessor injection to capture current user in AuditInterceptor
- [ ] T134 [US7] Run tests: dotnet-docker/dotnet test InvoiceManagement.Tests --filter Category=US7
- [ ] T135 [US7] Verify deletion and audit logging via Swagger UI and database query
- [ ] T135a [P] [US7] Create AuditLogsController with GET /api/audit-logs endpoint (FR-027a) in InvoiceManagement/Controllers/AuditLogsController.cs
- [ ] T135b [P] [US7] Add [Authorize(Roles = "Administrator")] to audit logs endpoint and integration test in InvoiceManagement.Tests/Integration/AuditLogTests.cs

**Checkpoint**: All user stories complete - full invoice management system functional

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T136 [P] Add XML documentation comments to all public APIs for Swagger in InvoiceManagement/Controllers/
- [ ] T137 [P] Configure Swagger examples and descriptions using OpenAPI attributes
- [ ] T138 [P] Add response compression in InvoiceManagement/Program.cs (AddResponseCompression)
- [ ] T139 [P] Implement rate limiting middleware in InvoiceManagement/Middleware/RateLimitingMiddleware.cs
- [ ] T140 [P] Add health check endpoint in InvoiceManagement/Program.cs (MapHealthChecks)
- [ ] T141 [P] Configure CORS policy in InvoiceManagement/Program.cs for allowed origins
- [ ] T142 [P] Add logging configuration (Serilog or built-in) in InvoiceManagement/Program.cs
- [ ] T143 [P] Add unit tests for InvoiceService business logic in InvoiceManagement.Tests/Unit/InvoiceServiceTests.cs
- [ ] T144 [P] Add unit tests for LockService in InvoiceManagement.Tests/Unit/LockServiceTests.cs
- [ ] T145 Code review and refactoring for clarity across all services
- [ ] T146 Performance optimization: review and optimize database queries with AsNoTracking where appropriate
- [ ] T147 Security hardening: validate all inputs, check for SQL injection risks, review authorization
- [ ] T148 Run full test suite: dotnet-docker/dotnet test InvoiceManagement.Tests
- [ ] T149 Verify quickstart.md instructions work end-to-end (fresh clone to running system)
- [ ] T150 Update CLAUDE.md with any implementation notes or gotchas discovered

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phases 3-9)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P1 → P2 → P2 → P2 → P3 → P3)
- **Polish (Phase 10)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Basic Invoice Entry - No dependencies on other stories
- **User Story 2 (P1)**: Invoice Listing - Independent (uses invoices from US1 for testing)
- **User Story 3 (P2)**: Invoice Modification - Independent (extends US1 endpoints)
- **User Story 4 (P2)**: Payment Status Tracking - Independent (extends Invoice entity from Foundation)
- **User Story 5 (P2)**: Data Export - Independent (uses filtering from US2)
- **User Story 6 (P3)**: RBAC - Independent (adds auth to existing endpoints)
- **User Story 7 (P3)**: Invoice Deletion - Independent (adds delete endpoint + audit)

### Within Each User Story

1. Tests MUST be written and FAIL before implementation
2. DTOs before services
3. Service interfaces before implementations
4. Services before controllers
5. Controllers before authorization attributes
6. Core implementation before integration with other stories
7. Story complete before moving to next priority

### Parallel Opportunities

**Phase 1 (Setup)**:
- T002, T003, T004, T005, T006, T007 can all run in parallel

**Phase 2 (Foundational)**:
- T008, T009, T010 (enums) can run in parallel
- T011, T012, T013, T014, T015 (entities) can run in parallel after enums
- T023, T024, T025, T026 can run in parallel after migrations

**Within Each User Story**:
- All tests for a story can run in parallel (marked [P])
- All DTOs for a story can run in parallel (marked [P])
- All parallel-marked tasks within a story

**Across User Stories** (after Foundation complete):
- Different team members can work on US1, US2, US3, etc. simultaneously
- Each story is independently testable

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together:
Parallel Tasks:
- T031: Integration test POST /api/invoices returns 201
- T032: Integration test POST with missing fields returns 400
- T033: Integration test GET /api/invoices/{id} returns details
- T034: Integration test duplicate invoice number returns 409

# Launch all DTOs for User Story 1 together:
Parallel Tasks:
- T035: Create CreateInvoiceRequest DTO
- T036: Create InvoiceResponse DTO
```

---

## Parallel Example: Foundational Phase

```bash
# Enums (can all run in parallel):
- T008: InvoiceType enum
- T009: PaymentStatus enum
- T010: UserRole enum

# Entities (can all run in parallel after enums):
- T011: User entity
- T012: BusinessPartner entity
- T013: Invoice entity
- T014: InvoiceLock entity
- T015: AuditLogEntry entity
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (Basic Invoice Entry)
4. Complete Phase 4: User Story 2 (Invoice Listing/Filtering)
5. **STOP and VALIDATE**: Test US1 + US2 independently
6. Deploy/demo MVP (create and query invoices)

**Rationale**: US1 + US2 together provide core value - accountants can enter invoices and find them. Both are P1 priority.

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo
3. Add User Story 2 → Test independently → Deploy/Demo (MVP complete)
4. Add User Story 3 → Test independently → Deploy/Demo (edit capability)
5. Add User Story 4 → Test independently → Deploy/Demo (payment tracking)
6. Add User Story 5 → Test independently → Deploy/Demo (export)
7. Add User Story 6 → Test independently → Deploy/Demo (RBAC)
8. Add User Story 7 → Test independently → Deploy/Demo (deletion + audit)
9. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. **Team** completes Setup + Foundational together
2. Once Foundational is done (after T030):
   - **Developer A**: User Story 1 (T031-T045)
   - **Developer B**: User Story 2 (T046-T059)
   - **Developer C**: User Story 6 (T103-T121) - can start early as it adds auth to existing endpoints
3. Stories complete and integrate independently
4. Continue with remaining stories (US3, US4, US5, US7) in parallel or sequence

---

## Notes

- [P] tasks = different files, no dependencies within that phase
- [Story] label (US1, US2, etc.) maps task to specific user story for traceability
- Each user story should be independently completable and testable
- **TDD CRITICAL**: Verify tests fail (Red) before implementing (Green), then refactor
- Commit after each logical task group or checkpoint
- Use `dotnet-docker/dotnet` wrapper for ALL .NET operations (per constitution)
- Stop at any checkpoint to validate story independently
- All tests must pass before merging: `dotnet-docker/dotnet test`
- Swagger documentation auto-generated but enhance with XML comments and examples

---

## Task Summary

**Total Tasks**: 157 (was 150, added 7 tasks for analysis fixes)
**Setup Phase**: 7 tasks
**Foundational Phase**: 24 tasks (was 23, +1 for session timeout test T022a)
**User Story 1 (P1)**: 15 tasks (4 tests + 11 implementation)
**User Story 2 (P1)**: 15 tasks (was 14, +1 for empty results test T034a)
**User Story 3 (P2)**: 20 tasks (was 18, +2 for lock details T073a and cleanup spec T075)
**User Story 4 (P2)**: 12 tasks (4 tests + 8 implementation)
**User Story 5 (P2)**: 13 tasks (4 tests + 9 implementation)
**User Story 6 (P3)**: 20 tasks (was 19, +1 for user status toggle T117a)
**User Story 7 (P3)**: 16 tasks (was 14, +2 for audit log endpoint T135a-b)
**Polish Phase**: 15 tasks

**Parallel Opportunities Identified**: 54 tasks marked [P] (was 47, +7 new parallel tasks)

**MVP Scope** (Recommended): Phase 1 + Phase 2 + US1 + US2 = 61 tasks (was 59)
**Full Feature**: All 157 tasks

**Estimated Timeline**:
- MVP (US1 + US2): ~12-16 hours
- + US3 (Edit): +6-8 hours
- + US4 (Payment): +4-6 hours
- + US5 (Export): +4-6 hours
- + US6 (RBAC): +6-8 hours
- + US7 (Delete): +4-6 hours
- Polish: +4-6 hours
- **Total**: 40-56 hours (1-1.5 weeks full-time)
