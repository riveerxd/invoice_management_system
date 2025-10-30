namespace InvoiceManagement.Models.DTOs;

public class InvoiceListResponse
{
    /// <summary>
    /// List of invoices matching the filter
    /// </summary>
    public List<InvoiceResponse> Items { get; set; } = new();

    /// <summary>
    /// Total count of invoices matching the filter (before pagination)
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Summary statistics
    /// </summary>
    public InvoiceSummary Summary { get; set; } = new();
}

public class InvoiceSummary
{
    /// <summary>
    /// Total amount of paid invoices in cents
    /// </summary>
    public long TotalPaidCents { get; set; }

    /// <summary>
    /// Total amount of unpaid invoices in cents
    /// </summary>
    public long TotalUnpaidCents { get; set; }

    /// <summary>
    /// Count of paid invoices
    /// </summary>
    public int PaidCount { get; set; }

    /// <summary>
    /// Count of unpaid invoices
    /// </summary>
    public int UnpaidCount { get; set; }

    /// <summary>
    /// Count of overdue invoices
    /// </summary>
    public int OverdueCount { get; set; }
}
