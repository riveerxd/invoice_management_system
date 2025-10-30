namespace InvoiceManagement.Models.DTOs;

public class LockResponse
{
    /// <summary>
    /// Invoice ID that is locked
    /// </summary>
    public int InvoiceId { get; set; }

    /// <summary>
    /// User ID who holds the lock
    /// </summary>
    public int LockedByUserId { get; set; }

    /// <summary>
    /// Username of the user who holds the lock
    /// </summary>
    public string LockedByUserName { get; set; } = string.Empty;

    /// <summary>
    /// When the lock was acquired
    /// </summary>
    public DateTime LockAcquiredAt { get; set; }

    /// <summary>
    /// When the lock will expire
    /// </summary>
    public DateTime LockExpiresAt { get; set; }

    /// <summary>
    /// Whether the lock is currently active
    /// </summary>
    public bool IsActive => DateTime.UtcNow < LockExpiresAt;

    /// <summary>
    /// Message describing the lock status
    /// </summary>
    public string? Message { get; set; }
}
