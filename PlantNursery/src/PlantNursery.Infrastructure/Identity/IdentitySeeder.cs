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
            Roles.Customer,
            Roles.Cashier
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
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}