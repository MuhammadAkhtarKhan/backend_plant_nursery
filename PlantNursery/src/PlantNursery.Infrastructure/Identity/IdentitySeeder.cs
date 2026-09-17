using Microsoft.AspNetCore.Identity;

namespace PlantNursery.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        string[] roles =
        {
            Roles.Admin,
            Roles.Cashier,
            Roles.Customer
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole<Guid>(role));

                if (!result.Succeeded)
                {
                    throw new Exception(
                        $"Failed to create role: {role}. " +
                        string.Join(
                            ", ",
                            result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    public static async Task SeedAdminAsync(
        UserManager<ApplicationUser> userManager)
    {
        const string adminEmail = "admin@plantnursery.com";
        const string adminPassword = "Admin@12345";

        var existingAdmin =
            await userManager.FindByEmailAsync(adminEmail);

        if (existingAdmin != null)
        {
            return;
        }

        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "System",
            LastName = "Administrator",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(
            admin,
            adminPassword);

        if (!result.Succeeded)
        {
            throw new Exception(
                "Failed to create default Admin user. " +
                string.Join(
                    "; ",
                    result.Errors.Select(e => e.Description)));
        }

        var roleResult = await userManager.AddToRoleAsync(
            admin,
            Roles.Admin);

        if (!roleResult.Succeeded)
        {
            throw new Exception(
                "Failed to assign Admin role. " +
                string.Join(
                    "; ",
                    roleResult.Errors.Select(e => e.Description)));
        }
    }
}