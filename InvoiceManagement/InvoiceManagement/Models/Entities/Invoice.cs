namespace InvoiceManagement.Models.Entities;

public class Invoice
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceType Type { get; set; }
    public int BusinessPartnerId { get; set; }
    public BusinessPartner? BusinessPartner { get; set; }
    public long AmountCents { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    public DateTime? PaymentDate { get; set; }
    public int CreatedById { get; set; }
    public User? CreatedBy { get; set; }
    public int? ModifiedById { get; set; }
    public User? ModifiedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
