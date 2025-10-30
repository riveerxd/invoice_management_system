namespace InvoiceManagement.Models.Entities;

public class BusinessPartner
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Identifier { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
