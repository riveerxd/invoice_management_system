using System.ComponentModel.DataAnnotations;

namespace InvoiceManagement.Models.DTOs;

public class CreateInvoiceRequest
{
    [Required]
    [StringLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required]
    public DateTime IssueDate { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    [Required]
    public InvoiceType Type { get; set; }

    [Required]
    [StringLength(200)]
    public string PartnerName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? PartnerIdentifier { get; set; }

    [Required]
    [Range(0, long.MaxValue)]
    public long AmountCents { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    public DateTime? PaymentDate { get; set; }
}
