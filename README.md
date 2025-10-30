# 📊 Invoice Management System

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![OpenAPI](https://img.shields.io/badge/OpenAPI-6BA539?style=for-the-badge&logo=openapi-initiative&logoColor=white)

A modern, RESTful API for managing company invoices with role-based access control, real-time locking, and comprehensive financial tracking.

[Features](#-features) • [Getting Started](#-getting-started) • [API Documentation](#-api-documentation) • [Development](#-development)

</div>

---

## ✨ Features

### 🔐 Authentication & Authorization
- **Role-Based Access Control** - Administrator, Accountant, and Manager roles
- **Session Management** - 30-minute sliding session expiration
- **Secure Authentication** - ASP.NET Core Identity with password policies

### 📝 Invoice Management
- **CRUD Operations** - Create, read, update, and manage invoices
- **Dual Invoice Types** - Track both received (supplier) and issued (customer) invoices
- **Payment Tracking** - Monitor payment status and overdue invoices
- **Business Partner Management** - Automatic partner creation and tracking

### 🔒 Concurrent Editing Protection
- **Real-Time Locking** - Prevent simultaneous edits with 5-minute auto-expiring locks
- **Lock Status Visibility** - See who is currently editing an invoice
- **Automatic Cleanup** - Background service clears expired locks

### 🔍 Advanced Filtering & Pagination
- Filter by invoice type, payment status, partner name, and date ranges
- Paginated results with configurable page sizes
- Real-time summary statistics (total paid/unpaid, overdue count)

### 📤 Data Export
- **CSV Export** - Export filtered invoice data to CSV format
- **Custom Timestamps** - Timestamped file names for easy organization

---

## 🛠️ Tech Stack

| Category | Technology |
|----------|-----------|
| **Framework** | ASP.NET Core 8.0 WebAPI |
| **Database** | PostgreSQL 16+ |
| **ORM** | Entity Framework Core 9.0 |
| **Authentication** | ASP.NET Core Identity |
| **API Documentation** | Swagger/OpenAPI (Swashbuckle) |
| **Data Export** | CsvHelper |
| **Containerization** | Docker & Docker Compose |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher
- [PostgreSQL 16+](https://www.postgresql.org/download/) or Docker
- [Docker](https://www.docker.com/get-started) (optional, for containerized setup)

### 📦 Installation

#### Option 1: Docker Compose (Recommended)

The easiest way to get started is using Docker Compose, which starts both the PostgreSQL database and the API in containers:

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd openapi_swagger
   ```

2. **Start the entire stack**
   ```bash
   docker compose up -d
   ```

3. **Access the API**
   - API: http://localhost:8080
   - Swagger UI: http://localhost:8080/swagger

4. **Stop the stack**
   ```bash
   docker compose down
   ```

   To remove all data (database volumes):
   ```bash
   docker compose down -v
   ```

#### Option 2: Manual Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd openapi_swagger
   ```

2. **Configure PostgreSQL**

   Update `appsettings.json` with your database connection:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=invoicedb;Username=postgres;Password=yourpassword"
     }
   }
   ```

3. **Apply database migrations**
   ```bash
   cd InvoiceManagement/InvoiceManagement
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the API**
   - API: http://localhost:8080
   - Swagger UI: http://localhost:8080/swagger

---

## 📚 API Documentation

### Default Users

The system comes pre-seeded with test users:

| Username | Password | Role | Email |
|----------|----------|------|-------|
| `admin` | `Admin@123` | Administrator | admin@invoice.local |
| `accountant` | `Accountant@123` | Accountant | accountant@invoice.local |

### Authentication Endpoints

> **Authentication Method:** This API uses **JWT Bearer tokens** for authentication. Include the token in the `Authorization` header for all protected endpoints.

#### 🔑 Login
```bash
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}
```

**Response (200 OK):**
```json
{
  "userId": 1,
  "username": "admin",
  "email": "admin@invoice.local",
  "role": "Administrator",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-10-30T14:30:00Z",
  "message": "Login successful"
}
```

**Save the `token` value** - you'll need it for all subsequent requests!

#### 👤 Get Current User
```bash
GET /api/auth/me
Authorization: Bearer <your-token-here>
```

#### 🚪 Logout
```bash
POST /api/auth/logout
Authorization: Bearer <your-token-here>
```

> **Note:** With JWT tokens, logout is client-side. Simply delete/discard the token.

---

### Invoice Endpoints

#### 📝 Create Invoice
```bash
POST /api/invoices
Content-Type: application/json
Authorization: Bearer <your-token-here>

{
  "invoiceNumber": "INV-2025-001",
  "issueDate": "2025-10-30T00:00:00Z",
  "dueDate": "2025-11-30T00:00:00Z",
  "type": 1,
  "partnerName": "Acme Corporation",
  "partnerIdentifier": "ACM-001",
  "amountCents": 610000,
  "paymentStatus": 0
}
```

**Invoice Types:**
- `0` - Received (from supplier)
- `1` - Issued (to customer)

**Payment Status:**
- `0` - Unpaid
- `1` - Paid

#### 📄 Get Invoice by ID
```bash
GET /api/invoices/{id}
Authorization: Bearer <your-token-here>
```

#### 📋 List Invoices (with filtering)
```bash
GET /api/invoices?type=1&paymentStatus=0&partnerName=Acme&page=1&pageSize=50
Authorization: Bearer <your-token-here>
```

**Query Parameters:**
- `type` - Filter by invoice type (0 or 1)
- `paymentStatus` - Filter by payment status (0 or 1)
- `partnerName` - Filter by partner name (partial match)
- `invoiceNumber` - Filter by invoice number (partial match)
- `issueDateFrom` / `issueDateTo` - Filter by issue date range
- `dueDateFrom` / `dueDateTo` - Filter by due date range
- `page` - Page number (default: 1)
- `pageSize` - Items per page (default: 50)

**Response:**
```json
{
  "items": [...],
  "totalCount": 3,
  "page": 1,
  "pageSize": 50,
  "totalPages": 1,
  "summary": {
    "totalPaidCents": 750000,
    "totalUnpaidCents": 275000,
    "paidCount": 1,
    "unpaidCount": 2,
    "overdueCount": 1
  }
}
```

#### ✏️ Update Invoice
```bash
PUT /api/invoices/{id}
Content-Type: application/json
Authorization: Bearer <your-token-here>

{
  "issueDate": "2025-10-30T00:00:00Z",
  "dueDate": "2025-12-15T00:00:00Z",
  "type": 1,
  "partnerName": "Acme Corporation",
  "partnerIdentifier": "ACM-001",
  "amountCents": 750000,
  "paymentStatus": 1,
  "paymentDate": "2025-10-30T12:00:00Z"
}
```

---

### Lock Management Endpoints

#### 🔒 Acquire Lock
```bash
POST /api/invoices/{id}/lock
Authorization: Bearer <your-token-here>
```

**Response (200 OK):**
```json
{
  "invoiceId": 3,
  "lockedByUserId": 2,
  "lockedByUserName": "accountant",
  "lockAcquiredAt": "2025-10-30T13:14:19.917507Z",
  "lockExpiresAt": "2025-10-30T13:19:19.917507Z",
  "isActive": true,
  "message": "Lock acquired successfully."
}
```

**Response (409 Conflict) - Already Locked:**
```json
{
  "invoiceId": 3,
  "lockedByUserId": 2,
  "lockedByUserName": "accountant",
  "lockAcquiredAt": "2025-10-30T13:14:19.917507Z",
  "lockExpiresAt": "2025-10-30T13:19:19.917507Z",
  "isActive": true,
  "message": "Invoice is locked by accountant until 2025-10-30 13:19:19 UTC."
}
```

#### 🔓 Release Lock
```bash
DELETE /api/invoices/{id}/lock
Authorization: Bearer <your-token-here>
```

> **Note:** Locks are automatically released after successful invoice updates.

---

### Export Endpoints

#### 📤 Export to CSV
```bash
GET /api/invoices/export?type=1&paymentStatus=1
Authorization: Bearer <your-token-here>
```

Downloads a CSV file with format: `invoices_YYYYMMDD_HHmmss.csv`

**Sample CSV Output:**
```csv
InvoiceNumber,IssueDate,DueDate,Type,PartnerName,PartnerIdentifier,AmountCents,PaymentStatus,PaymentDate,IsOverdue,CreatedBy,CreatedAt
INV-2025-002,2025-10-30,2025-12-15,Issued,Acme Corporation,TAX-123456,750000,Paid,2025-10-30,No,admin,2025-10-30 13:13:49
```

---

## 🧪 Testing

### Run Unit Tests
```bash
cd InvoiceManagement/InvoiceManagement.Tests
dotnet test
```

### Manual API Testing with cURL

**1. Login and get JWT token:**
```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123"}'
```

Response will include a `token` field. Copy it!

**2. Set token as environment variable:**
```bash
export TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**3. Create an invoice:**
```bash
curl -X POST http://localhost:8080/api/invoices \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "invoiceNumber": "INV-2025-003",
    "issueDate": "2025-10-30T00:00:00Z",
    "dueDate": "2025-11-30T00:00:00Z",
    "type": 1,
    "partnerName": "Tech Solutions Ltd",
    "partnerIdentifier": "TSL-100",
    "amountCents": 500000,
    "paymentStatus": 0
  }'
```

**4. List all invoices:**
```bash
curl -X GET http://localhost:8080/api/invoices \
  -H "Authorization: Bearer $TOKEN"
```

---

## 📁 Project Structure

```
openapi_swagger/
├── InvoiceManagement/
│   ├── InvoiceManagement/              # Main API project
│   │   ├── Controllers/                # API endpoints
│   │   ├── Models/                     # Data models
│   │   │   ├── DTOs/                   # Data Transfer Objects
│   │   │   └── Entities/               # Database entities
│   │   ├── Services/                   # Business logic
│   │   ├── Data/                       # Database context & seeder
│   │   ├── Middleware/                 # Custom middleware
│   │   └── Program.cs                  # Application entry point
│   └── InvoiceManagement.Tests/        # Unit tests
├── db-init/                            # Database initialization scripts
├── specs/                              # Project specifications
├── Dockerfile.postgres                 # PostgreSQL Docker config
├── run-postgres.sh                     # Database startup script
└── README.md                           # This file
```

---

## 🔧 Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=invoicedb;Username=postgres;Password=postgres"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment name | Development |
| `ConnectionStrings__DefaultConnection` | Database connection string | See above |

---

## 🐳 Docker Deployment

### Using Docker Compose

The project includes a complete `docker-compose.yml` configuration that runs both the PostgreSQL database and the Invoice Management API:

**Features:**
- PostgreSQL 16 with health checks
- API with automatic database connection
- Pre-configured JWT authentication
- Network isolation with bridge networking
- Persistent database volumes
- Automatic dependency management (API waits for DB)

**Run the stack:**
```bash
docker compose up -d
```

**View logs:**
```bash
docker compose logs -f
```

**Stop the stack:**
```bash
docker compose down
```

**Rebuild after code changes:**
```bash
docker compose up -d --build
```

---

## 🔐 Security Features

- ✅ **JWT Bearer Token Authentication** - Stateless, scalable authentication
- ✅ **Password Requirements** - 8+ characters, uppercase, lowercase, digit, special character
- ✅ **Role-Based Authorization** - Fine-grained access control on endpoints
- ✅ **Token Expiration** - 30-minute token lifetime with configurable expiry
- ✅ **SQL Injection Prevention** - Entity Framework parameterization
- ✅ **Concurrent Editing Prevention** - Locking mechanism for data integrity
- ✅ **User Account Status** - Active/inactive account checking
- ✅ **HTTPS Support** - Production-ready secure communication

---

## 📊 Database Schema

### Core Entities

- **User** - System users with roles and authentication
- **Invoice** - Invoice records with payment tracking
- **BusinessPartner** - Customer/supplier information
- **InvoiceLock** - Active locks for concurrent editing control

---
