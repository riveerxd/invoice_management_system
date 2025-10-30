namespace InvoiceManagement.Models.DTOs;

public class InvoiceCsvRecord
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public string IssueDate { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string PartnerName { get; set; } = string.Empty;
    public string PartnerIdentifier { get; set; } = string.Empty;
    public string AmountCents { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string PaymentDate { get; set; } = string.Empty;
    public string IsOverdue { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}
