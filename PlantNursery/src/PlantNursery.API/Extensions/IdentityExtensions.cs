using Microsoft.AspNetCore.Identity;
using PlantNursery.Infrastructure.Identity;

namespace PlantNursery.API.Extensions;

public static class IdentityExtensions
{
    public static async Task SeedIdentityAsync(
        this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var roleManager =
            scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        await IdentitySeeder.SeedRolesAsync(roleManager);
        await IdentitySeeder.SeedAdminAsync(userManager);
    }
}