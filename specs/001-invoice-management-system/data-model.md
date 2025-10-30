# Data Model: Company Invoice Management System

**Feature**: 001-invoice-management-system
**Date**: 2025-10-22
**Purpose**: Define entities, relationships, and validation rules for EF Core implementation

## Entity Relationship Diagram (Conceptual)

```
┌─────────────────┐       ┌──────────────────┐
│     User        │       │ BusinessPartner  │
├─────────────────┤       ├──────────────────┤
│ Id (PK)         │       │ Id (PK)          │
│ Username        │       │ Name             │
│ Email           │       │ Identifier       │
│ PasswordHash    │       │ CreatedAt        │
│ Role            │       │ UpdatedAt        │
│ IsActive        │       └──────────────────┘
│ CreatedAt       │                │
│ UpdatedAt       │                │
└────────┬────────┘                │
         │                         │
         │ Created/Modified By     │ References
         │                         │
         ▼                         ▼
┌────────────────────────────────────────┐
│             Invoice                    │
├────────────────────────────────────────┤
│ Id (PK)                                │
│ InvoiceNumber (UNIQUE)                 │
│ IssueDate                              │
│ DueDate                                │
│ Type (Received/Issued)                 │
│ BusinessPartnerId (FK)                 │
│ AmountCents (long)                     │
│ PaymentStatus (Paid/Unpaid)            │
│ PaymentDate (nullable)                 │
│ CreatedById (FK → User)                │
│ ModifiedById (FK → User)               │
│ CreatedAt                              │
│ UpdatedAt                              │
└─────────────┬──────────────────────────┘
              │
              │ Audit Trail
              ▼
┌────────────────────────────────────────┐
│          AuditLogEntry                 │
├────────────────────────────────────────┤
│ Id (PK)                                │
│ Timestamp                              │
│ UserId (FK → User)                     │
│ UserName                               │
│ Action (e.g., "DELETE")                │
│ InvoiceId                              │
│ InvoiceNumber                          │
│ Details (JSON)                         │
└────────────────────────────────────────┘

┌────────────────────────────────────────┐
│          InvoiceLock                   │
├────────────────────────────────────────┤
│ InvoiceId (PK, FK → Invoice)           │
│ LockedByUserId (FK → User)             │
│ LockedByUserName                       │
│ LockAcquiredAt                         │
│ LockExpiresAt                          │
└────────────────────────────────────────┘
```

## Entity Definitions

### 1. User

**Purpose**: Represents system users with authentication and authorization

**Fields**:

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | `int` | PK, Auto-increment | Primary key |
| `Username` | `string(50)` | NOT NULL, UNIQUE | Login username |
| `Email` | `string(100)` | NOT NULL, UNIQUE | Email address |
| `PasswordHash` | `string(255)` | NOT NULL | Hashed password (ASP.NET Core Identity) |
| `Role` | `enum` | NOT NULL | `Accountant`, `Manager`, `Administrator` |
| `IsActive` | `bool` | NOT NULL, DEFAULT true | Account active status |
| `CreatedAt` | `DateTime` | NOT NULL, DEFAULT CURRENT_TIMESTAMP | Creation timestamp |
| `UpdatedAt` | `DateTime` | NOT NULL, DEFAULT CURRENT_TIMESTAMP | Last update timestamp |

**Indexes**:
- Unique: `Username`, `Email`
- Non-unique: `Role` (for role-based queries)

**Validation Rules**:
- Username: 3-50 characters, alphanumeric + underscore
- Email: Valid email format
- Role: Must be one of three defined values
- Password: Min 8 characters (enforced by Identity)

**Relationships**:
- One-to-many with `Invoice` (created invoices)
- One-to-many with `Invoice` (modified invoices)
- One-to-many with `AuditLogEntry`
- One-to-one with `InvoiceLock` (currently locked invoice)

---

### 2. BusinessPartner

**Purpose**: Represents suppliers (for received invoices) and customers (for issued invoices)

**Fields**:

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | `int` | PK, Auto-increment | Primary key |
| `Name` | `string(200)` | NOT NULL | Company/organization name |
| `Identifier` | `string(50)` | NULL | Tax ID, registration number, etc. |
| `CreatedAt` | `DateTime` | NOT NULL, DEFAULT CURRENT_TIMESTAMP | Creation timestamp |
| `UpdatedAt` | `DateTime` | NOT NULL, DEFAULT CURRENT_TIMESTAMP | Last update timestamp |

**Indexes**:
- Non-unique: `Name` (for partner search/filtering)

**Validation Rules**:
- Name: 1-200 characters, required
- Identifier: Optional, max 50 characters

**Relationships**:
- One-to-many with `Invoice`

---

### 3. Invoice

**Purpose**: Core entity representing received or issued invoices

**Fields**:

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | `int` | PK, Auto-increment | Primary key |
| `InvoiceNumber` | `string(50)` | NOT NULL, UNIQUE | Globally unique invoice number |
| `IssueDate` | `Date` | NOT NULL | Date invoice was issued |
| `DueDate` | `Date` | NOT NULL | Payment due date |
| `Type` | `enum` | NOT NULL | `Received` or `Issued` |
| `BusinessPartnerId` | `int` | FK, NOT NULL | Reference to BusinessPartner |
| `AmountCents` | `long` | NOT NULL, CHECK >= 0 | Amount in smallest currency unit (cents) |
| `PaymentStatus` | `enum` | NOT NULL, DEFAULT Unpaid | `Paid` or `Unpaid` |
| `PaymentDate` | `Date` | NULL | Date payment was received/made |
| `CreatedById` | `int` | FK, NOT NULL | User who created invoice |
| `ModifiedById` | `int` | FK, NULL | User who last modified invoice |
| `CreatedAt` | `DateTime` | NOT NULL, DEFAULT CURRENT_TIMESTAMP | Creation timestamp |
| `UpdatedAt` | `DateTime` | NOT NULL, DEFAULT CURRENT_TIMESTAMP | Last update timestamp |

**Indexes**:
- Unique: `InvoiceNumber`
- Non-unique: `IssueDate`, `DueDate`, `PaymentStatus`, `BusinessPartnerId`
- Composite: `(PaymentStatus, DueDate)` - for overdue queries
- Composite: `(BusinessPartnerId, IssueDate)` - for partner filtering

**Validation Rules**:
- InvoiceNumber: 1-50 characters, required, globally unique
- IssueDate: Required, cannot be in future (validation in service layer)
- DueDate: Required, must be >= IssueDate
- AmountCents: Must be >= 0 (no negative invoices)
- PaymentDate: Required if PaymentStatus = Paid, NULL if Unpaid
- PaymentDate: Must be >= IssueDate if present

**Relationships**:
- Many-to-one with `BusinessPartner`
- Many-to-one with `User` (CreatedBy)
- Many-to-one with `User` (ModifiedBy)
- One-to-one with `InvoiceLock` (optional)
- One-to-many with `AuditLogEntry` (historical)

**Enums**:

```csharp
public enum InvoiceType
{
    Received = 0,  // Invoice from supplier
    Issued = 1     // Invoice to customer
}

public enum PaymentStatus
{
    Unpaid = 0,
    Paid = 1
}
```

---

### 4. InvoiceLock

**Purpose**: Implements pessimistic locking for concurrent edit control

**Fields**:

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `InvoiceId` | `int` | PK, FK → Invoice | Invoice being locked |
| `LockedByUserId` | `int` | FK → User, NOT NULL | User holding the lock |
| `LockedByUserName` | `string(50)` | NOT NULL | Username (denormalized for display) |
| `LockAcquiredAt` | `DateTime` | NOT NULL, DEFAULT CURRENT_TIMESTAMP | When lock was acquired |
| `LockExpiresAt` | `DateTime` | NOT NULL | Lock expiration time (5 min default) |

**Indexes**:
- Primary key: `InvoiceId`
- Non-unique: `LockExpiresAt` (for cleanup of expired locks)

**Validation Rules**:
- LockExpiresAt must be > LockAcquiredAt
- Locks expire after 5 minutes of inactivity (enforced by service)

**Relationships**:
- One-to-one with `Invoice`
- Many-to-one with `User`

**Lifecycle**:
- Created when user opens invoice for editing
- Updated when user performs actions (extends expiration)
- Deleted when user saves/cancels or lock expires

---

### 5. AuditLogEntry

**Purpose**: Immutable audit trail for invoice deletions (compliance requirement)

**Fields**:

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | `int` | PK, Auto-increment | Primary key |
| `Timestamp` | `DateTime` | NOT NULL, DEFAULT CURRENT_TIMESTAMP | When action occurred |
| `UserId` | `int` | FK → User, NOT NULL | User who performed action |
| `UserName` | `string(50)` | NOT NULL | Username (denormalized) |
| `Action` | `string(20)` | NOT NULL | Action type (e.g., "DELETE") |
| `InvoiceId` | `int` | NOT NULL | ID of affected invoice |
| `InvoiceNumber` | `string(50)` | NOT NULL | Invoice number (denormalized) |
| `Details` | `jsonb` | NULL | Additional context (snapshot of invoice) |

**Indexes**:
- Non-unique: `Timestamp` (for time-based queries)
- Non-unique: `UserId` (for user-specific audit reports)
- Non-unique: `InvoiceNumber` (for invoice history)

**Validation Rules**:
- All fields except Details are required
- Details contains JSON snapshot of invoice before deletion

**Relationships**:
- Many-to-one with `User`
- Historical reference to deleted Invoice (no FK constraint)

**Note**: This table is append-only. No updates or deletes permitted.

---

## State Transitions

### Invoice Payment Status Lifecycle

```
┌─────────┐
│ Unpaid  │ ◄─── Initial state when invoice created
└────┬────┘
     │
     │ markAsPaid() with PaymentDate
     ▼
┌─────────┐
│  Paid   │
└─────────┘
```

**Allowed Transitions**:
- `Unpaid → Paid`: Set PaymentDate, update PaymentStatus
- `Paid → Unpaid`: Clear PaymentDate (error correction only)

**Business Rules**:
- Cannot mark as paid without PaymentDate
- PaymentDate must be >= IssueDate
- PaymentDate cannot be in future

### Invoice Lock Lifecycle

```
┌──────────────┐
│  Unlocked    │ ◄─── Default state
└──────┬───────┘
       │
       │ User clicks "Edit"
       ▼
┌──────────────┐
│   Locked     │
└──────┬───────┘
       │
       ├──► User saves → Release lock
       ├──► User cancels → Release lock
       ├──► Lock expires (5 min) → Auto-release
       └──► Session timeout (30 min) → Auto-release
```

---

## Data Validation Summary

### Database-Level Constraints (via EF Core Migrations)

```sql
-- Unique constraints
ALTER TABLE Users ADD CONSTRAINT UQ_Users_Username UNIQUE (Username);
ALTER TABLE Users ADD CONSTRAINT UQ_Users_Email UNIQUE (Email);
ALTER TABLE Invoices ADD CONSTRAINT UQ_Invoices_InvoiceNumber UNIQUE (InvoiceNumber);

-- Check constraints
ALTER TABLE Invoices ADD CONSTRAINT CK_Invoices_AmountCents CHECK (AmountCents >= 0);
ALTER TABLE Invoices ADD CONSTRAINT CK_Invoices_Dates CHECK (DueDate >= IssueDate);

-- Foreign keys (auto-generated by EF Core)
ALTER TABLE Invoices ADD CONSTRAINT FK_Invoices_BusinessPartners FOREIGN KEY (BusinessPartnerId) REFERENCES BusinessPartners(Id);
ALTER TABLE Invoices ADD CONSTRAINT FK_Invoices_Users_CreatedBy FOREIGN KEY (CreatedById) REFERENCES Users(Id);
-- (etc.)
```

### Application-Level Validation (Service Layer)

- IssueDate cannot be in future
- PaymentDate required when PaymentStatus = Paid
- PaymentDate must be >= IssueDate and <= current date
- DueDate must be >= IssueDate
- Invoice number format validation (business-specific)
- User must have appropriate role for operation

---

## Performance Considerations

### Index Strategy

**High-Priority Indexes** (created in initial migration):
1. `Invoices.InvoiceNumber` (UNIQUE) - primary lookup
2. `Invoices.IssueDate` - date range filtering
3. `Invoices.DueDate` - overdue calculations
4. `Invoices.PaymentStatus` - paid/unpaid filtering
5. `Invoices.(PaymentStatus, DueDate)` - composite for "overdue unpaid" queries

**Medium-Priority Indexes** (add if performance testing requires):
6. `BusinessPartners.Name` - partner search
7. `Invoices.BusinessPartnerId` - partner-based filtering

### Query Optimization

- Use `AsNoTracking()` for read-only queries (invoice lists)
- Implement pagination for invoice lists (default 50 items, max 1000)
- Consider materialized view for aggregate reports (total paid/unpaid)
- Cache frequently accessed data (business partners, user roles)

### Scaling Considerations

- Current design supports 50K invoices per spec (SC-004)
- For >100K invoices: partition table by year
- For >1M invoices: consider archival strategy (move old invoices to separate table)

---

## Migration Plan

### Initial Migration: `CreateInvoiceSchema`

1. Create tables: Users, BusinessPartners, Invoices, InvoiceLocks, AuditLogEntries
2. Create indexes (unique + performance)
3. Create check constraints
4. Seed data:
   - Administrator user (username: `admin`, email: `admin@example.com`)
   - Sample business partners (optional, for testing)

### Future Migrations

- Add columns as features expand
- Create additional indexes based on production query patterns
- Add full-text search index on BusinessPartner.Name if needed

---

## EF Core Configuration Notes

### Conventions to Override

- Use `[Table("invoice")]` for lowercase table names (PostgreSQL convention)
- Configure `long AmountCents` with `.HasColumnType("bigint")`
- Use `.HasPrecision(19, 0)` for any additional numeric fields
- Configure enums with `.HasConversion<string>()` for readability in DB
- Use `DateTime.UtcNow` for all timestamps (avoid timezone issues)

### Example Entity Configuration

```csharp
public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(i => i.InvoiceNumber)
            .IsUnique();

        builder.Property(i => i.AmountCents)
            .IsRequired()
            .HasColumnType("bigint");

        builder.Property(i => i.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(i => i.PaymentStatus)
            .IsRequired()
            .HasConversion<string>();

        builder.HasOne(i => i.BusinessPartner)
            .WithMany()
            .HasForeignKey(i => i.BusinessPartnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.CreatedBy)
            .WithMany()
            .HasForeignKey(i => i.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.IssueDate);
        builder.HasIndex(i => i.DueDate);
        builder.HasIndex(i => i.PaymentStatus);
        builder.HasIndex(i => new { i.PaymentStatus, i.DueDate });
    }
}
```

---

## Next Steps

1. Generate EF Core migration: `dotnet-docker/dotnet ef migrations add CreateInvoiceSchema`
2. Review migration files
3. Apply to development database: `dotnet-docker/dotnet ef database update`
4. Define API contracts in `/contracts/` directory
