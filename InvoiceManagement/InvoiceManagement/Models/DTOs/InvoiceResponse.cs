namespace InvoiceManagement.Models.DTOs;

public class InvoiceResponse
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Type { get; set; } = string.Empty;
    public int BusinessPartnerId { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public string? PartnerIdentifier { get; set; }
    public long AmountCents { get; set; }
    public decimal AmountDecimal => AmountCents / 100.0m;
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime? PaymentDate { get; set; }
    public bool IsOverdue { get; set; }
    public int CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public int? ModifiedById { get; set; }
    public string? ModifiedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
