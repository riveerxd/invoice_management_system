using Microsoft.AspNetCore.Identity;

namespace InvoiceManagement.Models.Entities;

public class User : IdentityUser<int>
{
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
