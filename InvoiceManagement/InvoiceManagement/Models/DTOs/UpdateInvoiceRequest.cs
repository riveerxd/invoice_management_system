using InvoiceManagement.Models;

namespace InvoiceManagement.Models.DTOs;

public class UpdateInvoiceRequest
{
    /// <summary>
    /// Invoice number (must be unique)
    /// </summary>
    public string? InvoiceNumber { get; set; }

    /// <summary>
    /// Date invoice was issued
    /// </summary>
    public DateTime? IssueDate { get; set; }

    /// <summary>
    /// Payment due date
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Invoice type (Received or Issued)
    /// </summary>
    public InvoiceType? Type { get; set; }

    /// <summary>
    /// Business partner name
    /// </summary>
    public string? PartnerName { get; set; }

    /// <summary>
    /// Business partner identifier (tax ID, etc.)
    /// </summary>
    public string? PartnerIdentifier { get; set; }

    /// <summary>
    /// Amount in cents
    /// </summary>
    public long? AmountCents { get; set; }

    /// <summary>
    /// Payment status
    /// </summary>
    public PaymentStatus? PaymentStatus { get; set; }

    /// <summary>
    /// Date payment was made (required if status is Paid)
    /// </summary>
    public DateTime? PaymentDate { get; set; }
}
