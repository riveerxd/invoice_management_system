# Quickstart Guide: Invoice Management System

**Feature**: 001-invoice-management-system
**Date**: 2025-10-22
**Purpose**: Get the invoice management API running locally in under 10 minutes

## Prerequisites

- Docker and Docker Compose installed
- Git repository cloned
- No .NET SDK required (Docker-first development)

## Quick Start (5 Steps)

### 1. Navigate to Docker Directory

```bash
cd dotnet-docker
```

### 2. Initialize the WebAPI Project (First Time Only)

```bash
./init-webapi.sh InvoiceManagement
```

This creates:
- `InvoiceManagement/` - Main API project
- `InvoiceManagement.Tests/` - Test project
- Required NuGet packages (EF Core, Npgsql, Swagger, xUnit)

### 3. Start the Services

```bash
docker-compose up -d
```

This starts:
- PostgreSQL database (port 5432)
- Invoice Management API (port 5000)

**Wait for services to be healthy** (~10 seconds):
```bash
docker-compose ps
```

### 4. Create Database Schema

```bash
./dotnet ef database update --project ../InvoiceManagement
```

This applies EF Core migrations to create tables:
- `users` (with seeded admin account)
- `business_partners`
- `invoices`
- `invoice_locks`
- `audit_log_entries`

### 5. Verify API is Running

Open in browser:
```
http://localhost:5000/swagger
```

You should see the Swagger UI with all API endpoints.

---

## Default Credentials

**Administrator Account** (seeded in migration):
- Username: `admin`
- Password: `Admin@123` (change after first login)
- Role: Administrator

---

## Common Commands

### Run Migrations

Create new migration:
```bash
./dotnet ef migrations add MigrationName --project ../InvoiceManagement
```

Apply migrations:
```bash
./dotnet ef database update --project ../InvoiceManagement
```

Rollback last migration:
```bash
./dotnet ef database update PreviousMigrationName --project ../InvoiceManagement
```

### Run Tests

All tests:
```bash
./dotnet test ../InvoiceManagement.Tests
```

Integration tests only:
```bash
./dotnet test ../InvoiceManagement.Tests --filter Category=Integration
```

Unit tests only:
```bash
./dotnet test ../InvoiceManagement.Tests --filter Category=Unit
```

### View Logs

API logs:
```bash
docker-compose logs -f invoice-api
```

Database logs:
```bash
docker-compose logs -f postgres
```

### Database Access

Connect to PostgreSQL:
```bash
docker exec -it postgres psql -U invoice_user -d invoice_db
```

Useful queries:
```sql
-- List all invoices
SELECT * FROM invoices;

-- Check admin user
SELECT * FROM users WHERE username = 'admin';

-- View audit log
SELECT * FROM audit_log_entries ORDER BY timestamp DESC;

-- Check for locked invoices
SELECT * FROM invoice_locks;
```

### Reset Database

Stop services and remove volumes:
```bash
docker-compose down -v
```

Then restart (step 3) and re-run migrations (step 4).

---

## API Usage Examples

### 1. Login

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin@123"
  }' \
  -c cookies.txt
```

### 2. Create an Invoice

```bash
curl -X POST http://localhost:5000/api/invoices \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "invoiceNumber": "INV-2025-001",
    "issueDate": "2025-01-15",
    "dueDate": "2025-02-15",
    "type": "Issued",
    "partnerName": "Acme Corporation",
    "partnerIdentifier": "TAX-123456",
    "amountCents": 125000,
    "paymentStatus": "Unpaid"
  }'
```

### 3. List Invoices

All invoices:
```bash
curl -X GET http://localhost:5000/api/invoices \
  -b cookies.txt
```

With filters:
```bash
curl -X GET "http://localhost:5000/api/invoices?paymentStatus=Unpaid&issueDateFrom=2025-01-01" \
  -b cookies.txt
```

### 4. Get Invoice Details

```bash
curl -X GET http://localhost:5000/api/invoices/1 \
  -b cookies.txt
```

### 5. Update Invoice (with Pessimistic Lock)

Acquire lock:
```bash
curl -X POST http://localhost:5000/api/invoices/1/lock \
  -b cookies.txt
```

Update invoice:
```bash
curl -X PUT http://localhost:5000/api/invoices/1 \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "amountCents": 130000,
    "paymentStatus": "Paid",
    "paymentDate": "2025-02-10"
  }'
```

Release lock:
```bash
curl -X DELETE http://localhost:5000/api/invoices/1/lock \
  -b cookies.txt
```

### 6. Export to CSV

```bash
curl -X GET "http://localhost:5000/api/invoices/export?paymentStatus=Paid" \
  -b cookies.txt \
  -o invoices.csv
```

### 7. Create User (Admin Only)

```bash
curl -X POST http://localhost:5000/api/users \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "username": "accountant1",
    "email": "accountant1@example.com",
    "password": "SecurePass123",
    "role": "Accountant"
  }'
```

---

## Project Structure

```
InvoiceManagement/
├── Controllers/
│   ├── AuthController.cs       # Login/logout endpoints
│   ├── InvoicesController.cs   # Invoice CRUD + export
│   └── UsersController.cs      # User management
├── Models/
│   ├── Entities/               # EF Core entities
│   │   ├── Invoice.cs
│   │   ├── BusinessPartner.cs
│   │   ├── User.cs
│   │   ├── InvoiceLock.cs
│   │   └── AuditLogEntry.cs
│   └── DTOs/                   # Request/response contracts
│       ├── InvoiceDto.cs
│       ├── CreateInvoiceRequest.cs
│       └── ...
├── Services/
│   ├── InvoiceService.cs       # Business logic
│   ├── AuthService.cs          # Authentication
│   └── LockService.cs          # Pessimistic locking
├── Data/
│   ├── InvoiceDbContext.cs     # EF Core context
│   └── Migrations/             # Database migrations
├── Middleware/
│   ├── SessionTimeoutMiddleware.cs
│   └── ExceptionHandlingMiddleware.cs
└── Program.cs                  # Application entry point

InvoiceManagement.Tests/
├── Integration/
│   ├── InvoiceEndpointsTests.cs
│   └── ConcurrencyTests.cs
└── Unit/
    ├── InvoiceServiceTests.cs
    └── LockServiceTests.cs
```

---

## Environment Configuration

Edit `dotnet-docker/.env` to customize:

```env
# PostgreSQL Configuration
POSTGRES_USER=invoice_user
POSTGRES_PASSWORD=secure_password_here
POSTGRES_DB=invoice_db
DB_PORT=5432

# API Configuration
API_PORT=5000
ASPNETCORE_ENVIRONMENT=Development

# Session Configuration
SESSION_TIMEOUT_MINUTES=30
LOCK_TIMEOUT_MINUTES=5
```

---

## Troubleshooting

### API won't start

Check if PostgreSQL is healthy:
```bash
docker-compose ps
```

If unhealthy, check logs:
```bash
docker-compose logs postgres
```

### Migration errors

Remove database and start fresh:
```bash
docker-compose down -v
docker-compose up -d
./dotnet ef database update --project ../InvoiceManagement
```

### Port already in use

Change ports in `.env`:
```env
API_PORT=5001
DB_PORT=5433
```

Then restart:
```bash
docker-compose down
docker-compose up -d
```

### Session timeout issues

Check middleware registration in `Program.cs`:
```csharp
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
```

### Lock not released

Locks expire after 5 minutes automatically. To manually clear:
```sql
docker exec -it postgres psql -U invoice_user -d invoice_db
DELETE FROM invoice_locks WHERE lock_expires_at < NOW();
```

---

## Next Steps

1. **Read the API docs**: Open http://localhost:5000/swagger
2. **Run tests**: `./dotnet test ../InvoiceManagement.Tests`
3. **Review data model**: See `specs/001-invoice-management-system/data-model.md`
4. **Check contracts**: See `specs/001-invoice-management-system/contracts/openapi.yaml`
5. **Start implementing**: Follow TDD workflow in `specs/001-invoice-management-system/tasks.md` (generated by `/speckit.tasks`)

---

## Development Workflow

### Adding a New Feature

1. Write integration test (Red):
```csharp
[Fact]
public async Task CreateInvoice_WithValidData_Returns201()
{
    // Arrange
    var request = new CreateInvoiceRequest { ... };

    // Act
    var response = await _client.PostAsJsonAsync("/api/invoices", request);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
}
```

2. Implement controller action (Green):
```csharp
[HttpPost]
[Authorize(Roles = "Accountant,Administrator")]
public async Task<ActionResult<InvoiceResponse>> CreateInvoice(CreateInvoiceRequest request)
{
    var invoice = await _invoiceService.CreateAsync(request);
    return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoice);
}
```

3. Refactor for clarity

4. Verify tests pass:
```bash
./dotnet test
```

### Making Schema Changes

1. Update entity class:
```csharp
public class Invoice
{
    // Add new property
    public string? Notes { get; set; }
}
```

2. Create migration:
```bash
./dotnet ef migrations add AddNotesToInvoice --project ../InvoiceManagement
```

3. Review generated migration in `Data/Migrations/`

4. Apply to database:
```bash
./dotnet ef database update --project ../InvoiceManagement
```

---

## Production Deployment Notes

### Environment Variables (Required)

```env
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=prod-db;Database=invoice_db;...
SessionTimeout=30
LockTimeout=5
```

### Security Checklist

- [ ] Change default admin password
- [ ] Use HTTPS (TLS certificates configured)
- [ ] Set secure session cookie flags
- [ ] Configure CORS for allowed origins
- [ ] Enable rate limiting
- [ ] Set up monitoring/logging (e.g., Serilog + ELK)
- [ ] Database backups configured
- [ ] Regular security updates (dependabot)

### Performance Tuning

- Enable response compression
- Configure caching for read-heavy endpoints
- Database connection pooling (default in Npgsql)
- Consider Redis for distributed sessions (multi-instance)

---

## Support

For issues or questions:
- Review `specs/001-invoice-management-system/plan.md`
- Check constitution: `.specify/memory/constitution.md`
- Run tests: `./dotnet test --verbosity detailed`
