namespace InvoiceManagement.Models.Entities;

public class InvoiceLock
{
    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public int LockedByUserId { get; set; }
    public User? LockedByUser { get; set; }
    public string LockedByUserName { get; set; } = string.Empty;
    public DateTime LockAcquiredAt { get; set; } = DateTime.UtcNow;
    public DateTime LockExpiresAt { get; set; }
}
