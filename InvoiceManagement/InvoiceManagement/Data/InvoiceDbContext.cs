using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using InvoiceManagement.Models;
using InvoiceManagement.Models.Entities;

namespace InvoiceManagement.Data;

public class InvoiceDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public InvoiceDbContext(DbContextOptions<InvoiceDbContext> options) : base(options)
    {
    }

    public DbSet<BusinessPartner> BusinessPartners => Set<BusinessPartner>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLock> InvoiceLocks => Set<InvoiceLock>();
    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure table names (PostgreSQL convention: lowercase)
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<IdentityRole<int>>().ToTable("roles");
        modelBuilder.Entity<IdentityUserRole<int>>().ToTable("user_roles");
        modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("user_claims");
        modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("user_logins");
        modelBuilder.Entity<IdentityUserToken<int>>().ToTable("user_tokens");
        modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("role_claims");

        modelBuilder.Entity<BusinessPartner>().ToTable("business_partners");
        modelBuilder.Entity<Invoice>().ToTable("invoices");
        modelBuilder.Entity<InvoiceLock>().ToTable("invoice_locks");
        modelBuilder.Entity<AuditLogEntry>().ToTable("audit_log_entries");

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Role)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Role);
        });

        // BusinessPartner configuration
        modelBuilder.Entity<BusinessPartner>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Identifier)
                .HasMaxLength(50);

            entity.HasIndex(e => e.Name);
        });

        // Invoice configuration
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.InvoiceNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(e => e.InvoiceNumber).IsUnique();

            entity.Property(e => e.Type)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(e => e.PaymentStatus)
                .IsRequired()
                .HasConversion<string>()
                .HasDefaultValue(PaymentStatus.Unpaid);

            entity.Property(e => e.AmountCents)
                .IsRequired()
                .HasColumnType("bigint");

            // Indexes for performance
            entity.HasIndex(e => e.IssueDate);
            entity.HasIndex(e => e.DueDate);
            entity.HasIndex(e => e.PaymentStatus);
            entity.HasIndex(e => new { e.PaymentStatus, e.DueDate });
            entity.HasIndex(e => e.BusinessPartnerId);

            // Relationships
            entity.HasOne(e => e.BusinessPartner)
                .WithMany()
                .HasForeignKey(e => e.BusinessPartnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ModifiedBy)
                .WithMany()
                .HasForeignKey(e => e.ModifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Check constraints
            entity.ToTable(t => t.HasCheckConstraint("CK_Invoices_AmountCents", "\"AmountCents\" >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_Invoices_Dates", "\"DueDate\" >= \"IssueDate\""));
        });

        // InvoiceLock configuration
        modelBuilder.Entity<InvoiceLock>(entity =>
        {
            entity.HasKey(e => e.InvoiceId);

            entity.Property(e => e.LockedByUserName)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(e => e.Invoice)
                .WithOne()
                .HasForeignKey<InvoiceLock>(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.LockedByUser)
                .WithMany()
                .HasForeignKey(e => e.LockedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.LockExpiresAt);
        });

        // AuditLogEntry configuration
        modelBuilder.Entity<AuditLogEntry>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Action)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.InvoiceNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Details)
                .HasColumnType("jsonb");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.InvoiceNumber);
        });
    }
}
