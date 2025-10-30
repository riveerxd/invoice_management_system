using Microsoft.EntityFrameworkCore;
using InvoiceManagement.Data;
using InvoiceManagement.Models.DTOs;
using InvoiceManagement.Models.Entities;

namespace InvoiceManagement.Services;

public class LockService : ILockService
{
    private readonly InvoiceDbContext _context;
    private readonly ILogger<LockService> _logger;
    private readonly int _lockTimeoutMinutes;

    public LockService(InvoiceDbContext context, ILogger<LockService> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _lockTimeoutMinutes = configuration.GetValue<int>("LockTimeout", 5);
    }

    public async Task<LockResponse> AcquireLockAsync(int invoiceId, int userId, string userName)
    {
        // Check if invoice exists
        var invoice = await _context.Invoices.FindAsync(invoiceId);
        if (invoice == null)
        {
            throw new ArgumentException($"Invoice with ID {invoiceId} not found.");
        }

        // Check for existing lock
        var existingLock = await _context.InvoiceLocks
            .FirstOrDefaultAsync(l => l.InvoiceId == invoiceId);

        var now = DateTime.UtcNow;

        // If lock exists and hasn't expired
        if (existingLock != null && existingLock.LockExpiresAt > now)
        {
            // If the same user already holds the lock, extend it
            if (existingLock.LockedByUserId == userId)
            {
                existingLock.LockExpiresAt = now.AddMinutes(_lockTimeoutMinutes);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Extended lock on invoice {InvoiceId} for user {UserId}", invoiceId, userId);

                return MapToLockResponse(existingLock, "Lock extended successfully.");
            }

            // Lock is held by another user
            _logger.LogWarning("Invoice {InvoiceId} is locked by user {LockedByUserId}", invoiceId, existingLock.LockedByUserId);

            return MapToLockResponse(existingLock, $"Invoice is locked by {existingLock.LockedByUserName} until {existingLock.LockExpiresAt:yyyy-MM-dd HH:mm:ss} UTC.");
        }

        // Create or update lock
        if (existingLock != null)
        {
            // Lock expired, update it
            existingLock.LockedByUserId = userId;
            existingLock.LockedByUserName = userName;
            existingLock.LockAcquiredAt = now;
            existingLock.LockExpiresAt = now.AddMinutes(_lockTimeoutMinutes);
        }
        else
        {
            // Create new lock
            existingLock = new InvoiceLock
            {
                InvoiceId = invoiceId,
                LockedByUserId = userId,
                LockedByUserName = userName,
                LockAcquiredAt = now,
                LockExpiresAt = now.AddMinutes(_lockTimeoutMinutes)
            };
            _context.InvoiceLocks.Add(existingLock);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Acquired lock on invoice {InvoiceId} for user {UserId}", invoiceId, userId);

        return MapToLockResponse(existingLock, "Lock acquired successfully.");
    }

    public async Task<bool> ReleaseLockAsync(int invoiceId, int userId)
    {
        var existingLock = await _context.InvoiceLocks
            .FirstOrDefaultAsync(l => l.InvoiceId == invoiceId);

        if (existingLock == null)
        {
            _logger.LogWarning("No lock found for invoice {InvoiceId}", invoiceId);
            return false;
        }

        // Only the lock owner can release it (or if it's expired)
        if (existingLock.LockedByUserId != userId && existingLock.LockExpiresAt > DateTime.UtcNow)
        {
            _logger.LogWarning("User {UserId} attempted to release lock on invoice {InvoiceId} owned by user {LockedByUserId}",
                userId, invoiceId, existingLock.LockedByUserId);
            return false;
        }

        _context.InvoiceLocks.Remove(existingLock);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Released lock on invoice {InvoiceId}", invoiceId);

        return true;
    }

    public async Task<LockResponse?> IsLockedAsync(int invoiceId)
    {
        var existingLock = await _context.InvoiceLocks
            .FirstOrDefaultAsync(l => l.InvoiceId == invoiceId);

        if (existingLock == null)
        {
            return null;
        }

        var now = DateTime.UtcNow;

        // If lock has expired, remove it and return null
        if (existingLock.LockExpiresAt <= now)
        {
            _context.InvoiceLocks.Remove(existingLock);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Removed expired lock on invoice {InvoiceId}", invoiceId);

            return null;
        }

        return MapToLockResponse(existingLock, "Invoice is currently locked.");
    }

    public async Task CleanupExpiredLocksAsync()
    {
        var now = DateTime.UtcNow;
        var expiredLocks = await _context.InvoiceLocks
            .Where(l => l.LockExpiresAt <= now)
            .ToListAsync();

        if (expiredLocks.Any())
        {
            _context.InvoiceLocks.RemoveRange(expiredLocks);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} expired locks", expiredLocks.Count);
        }
    }

    private static LockResponse MapToLockResponse(InvoiceLock lockEntity, string? message = null)
    {
        return new LockResponse
        {
            InvoiceId = lockEntity.InvoiceId,
            LockedByUserId = lockEntity.LockedByUserId,
            LockedByUserName = lockEntity.LockedByUserName,
            LockAcquiredAt = lockEntity.LockAcquiredAt,
            LockExpiresAt = lockEntity.LockExpiresAt,
            Message = message
        };
    }
}
