using Microsoft.AspNetCore.Identity;
using InvoiceManagement.Models;
using InvoiceManagement.Models.Entities;

namespace InvoiceManagement.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

        // Create roles
        string[] roleNames = { "Administrator", "Accountant", "Manager" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(roleName));
            }
        }

        // Create admin user
        var adminEmail = "admin@invoice.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = "admin",
                Email = adminEmail,
                EmailConfirmed = true,
                Role = UserRole.Administrator,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrator");
            }
        }

        // Create accountant user for testing
        var accountantEmail = "accountant@invoice.local";
        var accountantUser = await userManager.FindByEmailAsync(accountantEmail);

        if (accountantUser == null)
        {
            accountantUser = new User
            {
                UserName = "accountant",
                Email = accountantEmail,
                EmailConfirmed = true,
                Role = UserRole.Accountant,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(accountantUser, "Accountant@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(accountantUser, "Accountant");
            }
        }
    }
}
