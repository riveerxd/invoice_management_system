using InvoiceManagement.Models;

namespace InvoiceManagement.Models.DTOs;

public class InvoiceFilterRequest
{
    /// <summary>
    /// Filter by invoice number (partial match)
    /// </summary>
    public string? InvoiceNumber { get; set; }

    /// <summary>
    /// Filter by issue date from (inclusive)
    /// </summary>
    public DateTime? IssueDateFrom { get; set; }

    /// <summary>
    /// Filter by issue date to (inclusive)
    /// </summary>
    public DateTime? IssueDateTo { get; set; }

    /// <summary>
    /// Filter by due date from (inclusive)
    /// </summary>
    public DateTime? DueDateFrom { get; set; }

    /// <summary>
    /// Filter by due date to (inclusive)
    /// </summary>
    public DateTime? DueDateTo { get; set; }

    /// <summary>
    /// Filter by invoice type
    /// </summary>
    public InvoiceType? Type { get; set; }

    /// <summary>
    /// Filter by payment status
    /// </summary>
    public PaymentStatus? PaymentStatus { get; set; }

    /// <summary>
    /// Filter by business partner name (partial match)
    /// </summary>
    public string? PartnerName { get; set; }

    /// <summary>
    /// Filter overdue invoices only
    /// </summary>
    public bool? IsOverdue { get; set; }

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size (max 1000)
    /// </summary>
    public int PageSize { get; set; } = 50;
}
