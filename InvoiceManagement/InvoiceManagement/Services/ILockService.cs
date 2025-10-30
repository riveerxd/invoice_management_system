using InvoiceManagement.Models.DTOs;

namespace InvoiceManagement.Services;

public interface ILockService
{
    /// <summary>
    /// Acquire a lock on an invoice for editing
    /// </summary>
    /// <param name="invoiceId">Invoice ID to lock</param>
    /// <param name="userId">User ID requesting the lock</param>
    /// <param name="userName">Username requesting the lock</param>
    /// <returns>Lock response with lock details</returns>
    Task<LockResponse> AcquireLockAsync(int invoiceId, int userId, string userName);

    /// <summary>
    /// Release a lock on an invoice
    /// </summary>
    /// <param name="invoiceId">Invoice ID to unlock</param>
    /// <param name="userId">User ID releasing the lock</param>
    /// <returns>True if lock was released, false if no lock existed or user doesn't own the lock</returns>
    Task<bool> ReleaseLockAsync(int invoiceId, int userId);

    /// <summary>
    /// Check if an invoice is currently locked
    /// </summary>
    /// <param name="invoiceId">Invoice ID to check</param>
    /// <returns>Lock response if locked, null if not locked</returns>
    Task<LockResponse?> IsLockedAsync(int invoiceId);

    /// <summary>
    /// Clean up expired locks
    /// </summary>
    Task CleanupExpiredLocksAsync();
}
