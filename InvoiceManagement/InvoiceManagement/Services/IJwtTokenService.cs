using InvoiceManagement.Models.Entities;

namespace InvoiceManagement.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
