using Microsoft.EntityFrameworkCore;
using InvoiceManagement.Data;
using InvoiceManagement.Models;
using InvoiceManagement.Models.DTOs;
using InvoiceManagement.Models.Entities;

namespace InvoiceManagement.Services;

public class InvoiceService : IInvoiceService
{
    private readonly InvoiceDbContext _context;

    public InvoiceService(InvoiceDbContext context)
    {
        _context = context;
    }

    public async Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest request, int userId)
    {
        // Validate dates
        if (request.DueDate < request.IssueDate)
        {
            throw new ArgumentException("Due date must be greater than or equal to issue date.");
        }

        if (request.IssueDate > DateTime.UtcNow)
        {
            throw new ArgumentException("Issue date cannot be in the future.");
        }

        // Validate payment date if provided
        if (request.PaymentDate.HasValue)
        {
            if (request.PaymentDate.Value < request.IssueDate)
            {
                throw new ArgumentException("Payment date must be greater than or equal to issue date.");
            }

            if (request.PaymentStatus != PaymentStatus.Paid)
            {
                throw new ArgumentException("Payment date can only be set when payment status is Paid.");
            }
        }

        if (request.PaymentStatus == PaymentStatus.Paid && !request.PaymentDate.HasValue)
        {
            throw new ArgumentException("Payment date is required when payment status is Paid.");
        }

        // Check for duplicate invoice number
        var existingInvoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.InvoiceNumber == request.InvoiceNumber);

        if (existingInvoice != null)
        {
            throw new InvalidOperationException($"Invoice number {request.InvoiceNumber} already exists.");
        }

        // Find or create business partner
        var partner = await _context.BusinessPartners
            .FirstOrDefaultAsync(bp => bp.Name == request.PartnerName);

        if (partner == null)
        {
            partner = new BusinessPartner
            {
                Name = request.PartnerName,
                Identifier = request.PartnerIdentifier,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.BusinessPartners.Add(partner);
            await _context.SaveChangesAsync();
        }

        // Create invoice
        var invoice = new Invoice
        {
            InvoiceNumber = request.InvoiceNumber,
            IssueDate = request.IssueDate,
            DueDate = request.DueDate,
            Type = request.Type,
            BusinessPartnerId = partner.Id,
            AmountCents = request.AmountCents,
            PaymentStatus = request.PaymentStatus,
            PaymentDate = request.PaymentDate,
            CreatedById = userId,
            ModifiedById = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(invoice.Id)
            ?? throw new InvalidOperationException("Failed to retrieve created invoice.");
    }

    public async Task<InvoiceResponse?> GetByIdAsync(int id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.BusinessPartner)
            .Include(i => i.CreatedBy)
            .Include(i => i.ModifiedBy)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
        {
            return null;
        }

        return MapToResponse(invoice);
    }

    public async Task<InvoiceResponse> UpdateAsync(int id, UpdateInvoiceRequest request, int userId)
    {
        // Find the invoice
        var invoice = await _context.Invoices
            .Include(i => i.BusinessPartner)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
        {
            throw new ArgumentException($"Invoice with ID {id} not found.");
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.InvoiceNumber) && request.InvoiceNumber != invoice.InvoiceNumber)
        {
            // Check for duplicate invoice number
            var existingInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceNumber == request.InvoiceNumber && i.Id != id);

            if (existingInvoice != null)
            {
                throw new InvalidOperationException($"Invoice number {request.InvoiceNumber} already exists.");
            }

            invoice.InvoiceNumber = request.InvoiceNumber;
        }

        if (request.IssueDate.HasValue)
        {
            if (request.IssueDate.Value > DateTime.UtcNow)
            {
                throw new ArgumentException("Issue date cannot be in the future.");
            }
            invoice.IssueDate = request.IssueDate.Value;
        }

        if (request.DueDate.HasValue)
        {
            if (request.DueDate.Value < invoice.IssueDate)
            {
                throw new ArgumentException("Due date must be greater than or equal to issue date.");
            }
            invoice.DueDate = request.DueDate.Value;
        }

        if (request.Type.HasValue)
        {
            invoice.Type = request.Type.Value;
        }

        if (request.AmountCents.HasValue)
        {
            if (request.AmountCents.Value < 0)
            {
                throw new ArgumentException("Amount cannot be negative.");
            }
            invoice.AmountCents = request.AmountCents.Value;
        }

        // Handle payment status and date
        if (request.PaymentStatus.HasValue)
        {
            invoice.PaymentStatus = request.PaymentStatus.Value;

            if (request.PaymentStatus.Value == PaymentStatus.Paid)
            {
                if (!request.PaymentDate.HasValue)
                {
                    throw new ArgumentException("Payment date is required when payment status is Paid.");
                }

                if (request.PaymentDate.Value < invoice.IssueDate)
                {
                    throw new ArgumentException("Payment date must be greater than or equal to issue date.");
                }

                invoice.PaymentDate = request.PaymentDate.Value;
            }
            else
            {
                // Clear payment date if status is Unpaid
                invoice.PaymentDate = null;
            }
        }

        // Handle business partner update
        if (!string.IsNullOrWhiteSpace(request.PartnerName) && request.PartnerName != invoice.BusinessPartner?.Name)
        {
            var partner = await _context.BusinessPartners
                .FirstOrDefaultAsync(bp => bp.Name == request.PartnerName);

            if (partner == null)
            {
                partner = new BusinessPartner
                {
                    Name = request.PartnerName,
                    Identifier = request.PartnerIdentifier,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.BusinessPartners.Add(partner);
                await _context.SaveChangesAsync();
            }

            invoice.BusinessPartnerId = partner.Id;
        }

        // Update metadata
        invoice.ModifiedById = userId;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id)
            ?? throw new InvalidOperationException("Failed to retrieve updated invoice.");
    }

    public async Task<InvoiceListResponse> GetFilteredAsync(InvoiceFilterRequest filter)
    {
        // Start with base query
        var query = _context.Invoices
            .Include(i => i.BusinessPartner)
            .Include(i => i.CreatedBy)
            .Include(i => i.ModifiedBy)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.InvoiceNumber))
        {
            query = query.Where(i => i.InvoiceNumber.Contains(filter.InvoiceNumber));
        }

        if (filter.IssueDateFrom.HasValue)
        {
            query = query.Where(i => i.IssueDate >= filter.IssueDateFrom.Value);
        }

        if (filter.IssueDateTo.HasValue)
        {
            query = query.Where(i => i.IssueDate <= filter.IssueDateTo.Value);
        }

        if (filter.DueDateFrom.HasValue)
        {
            query = query.Where(i => i.DueDate >= filter.DueDateFrom.Value);
        }

        if (filter.DueDateTo.HasValue)
        {
            query = query.Where(i => i.DueDate <= filter.DueDateTo.Value);
        }

        if (filter.Type.HasValue)
        {
            query = query.Where(i => i.Type == filter.Type.Value);
        }

        if (filter.PaymentStatus.HasValue)
        {
            query = query.Where(i => i.PaymentStatus == filter.PaymentStatus.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.PartnerName))
        {
            query = query.Where(i => i.BusinessPartner != null && i.BusinessPartner.Name.Contains(filter.PartnerName));
        }

        if (filter.IsOverdue.HasValue && filter.IsOverdue.Value)
        {
            var now = DateTime.UtcNow;
            query = query.Where(i => i.PaymentStatus == Models.PaymentStatus.Unpaid && i.DueDate < now);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Calculate summary statistics
        var allInvoices = await query.ToListAsync();
        var summary = new InvoiceSummary
        {
            TotalPaidCents = allInvoices.Where(i => i.PaymentStatus == Models.PaymentStatus.Paid).Sum(i => i.AmountCents),
            TotalUnpaidCents = allInvoices.Where(i => i.PaymentStatus == Models.PaymentStatus.Unpaid).Sum(i => i.AmountCents),
            PaidCount = allInvoices.Count(i => i.PaymentStatus == Models.PaymentStatus.Paid),
            UnpaidCount = allInvoices.Count(i => i.PaymentStatus == Models.PaymentStatus.Unpaid),
            OverdueCount = allInvoices.Count(i => i.PaymentStatus == Models.PaymentStatus.Unpaid && i.DueDate < DateTime.UtcNow)
        };

        // Apply pagination
        var pageSize = Math.Min(filter.PageSize, 1000); // Max 1000 items per page
        var page = Math.Max(filter.Page, 1); // Minimum page 1
        var skip = (page - 1) * pageSize;

        var invoices = allInvoices
            .OrderByDescending(i => i.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(MapToResponse)
            .ToList();

        return new InvoiceListResponse
        {
            Items = invoices,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Summary = summary
        };
    }

    public async Task<Stream> ExportToCsvAsync(InvoiceFilterRequest filter)
    {
        // Get filtered invoices
        var listResponse = await GetFilteredAsync(filter);

        // Create memory stream
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream, System.Text.Encoding.UTF8);
        var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);

        // Write CSV records
        var records = listResponse.Items.Select(invoice => new InvoiceCsvRecord
        {
            InvoiceNumber = invoice.InvoiceNumber,
            IssueDate = invoice.IssueDate.ToString("yyyy-MM-dd"),
            DueDate = invoice.DueDate.ToString("yyyy-MM-dd"),
            Type = invoice.Type,
            PartnerName = invoice.PartnerName,
            PartnerIdentifier = invoice.PartnerIdentifier ?? string.Empty,
            AmountCents = invoice.AmountCents.ToString(),
            PaymentStatus = invoice.PaymentStatus,
            PaymentDate = invoice.PaymentDate?.ToString("yyyy-MM-dd") ?? string.Empty,
            IsOverdue = invoice.IsOverdue ? "Yes" : "No",
            CreatedBy = invoice.CreatedByName,
            CreatedAt = invoice.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
        });

        csv.WriteRecords(records);
        await writer.FlushAsync();
        stream.Position = 0;

        return stream;
    }

    private static InvoiceResponse MapToResponse(Invoice invoice)
    {
        return new InvoiceResponse
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            IssueDate = invoice.IssueDate,
            DueDate = invoice.DueDate,
            Type = invoice.Type.ToString(),
            BusinessPartnerId = invoice.BusinessPartnerId,
            PartnerName = invoice.BusinessPartner?.Name ?? string.Empty,
            PartnerIdentifier = invoice.BusinessPartner?.Identifier,
            AmountCents = invoice.AmountCents,
            PaymentStatus = invoice.PaymentStatus.ToString(),
            PaymentDate = invoice.PaymentDate,
            IsOverdue = invoice.PaymentStatus == Models.PaymentStatus.Unpaid && invoice.DueDate < DateTime.UtcNow,
            CreatedById = invoice.CreatedById,
            CreatedByName = invoice.CreatedBy?.UserName ?? string.Empty,
            ModifiedById = invoice.ModifiedById,
            ModifiedByName = invoice.ModifiedBy?.UserName,
            CreatedAt = invoice.CreatedAt,
            UpdatedAt = invoice.UpdatedAt
        };
    }
}
