using InvoiceManagement.Models.DTOs;

namespace InvoiceManagement.Services;

public interface IInvoiceService
{
    Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest request, int userId);
    Task<InvoiceResponse?> GetByIdAsync(int id);
    Task<InvoiceListResponse> GetFilteredAsync(InvoiceFilterRequest filter);
    Task<InvoiceResponse> UpdateAsync(int id, UpdateInvoiceRequest request, int userId);
    Task<Stream> ExportToCsvAsync(InvoiceFilterRequest filter);
}
