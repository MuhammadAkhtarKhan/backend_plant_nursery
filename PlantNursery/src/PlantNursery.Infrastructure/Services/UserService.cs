using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlantNursery.Application.Common.Exceptions;
using PlantNursery.Application.DTOs.Users;
using PlantNursery.Application.Interfaces;
using PlantNursery.Infrastructure.Identity;

namespace PlantNursery.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .OrderBy(x => x.Email)
            .ToListAsync();

        var result = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            result.Add(MapToDto(user, roles));
        }

        return result;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        return MapToDto(user, roles);
    }

    public async Task<UserDto> CreateAsync(
        CreateUserRequest request)
    {
        var email = request.Email.Trim();

        var existingUser =
            await _userManager.FindByEmailAsync(email);

        if (existingUser != null)
        {
            throw new ConflictException(
                "A user with this email already exists.");
        }

        var allowedRoles = new[]
        {
            Roles.Admin,
            Roles.Cashier,
            Roles.Customer
        };

        var role = request.Role.Trim();

        if (!allowedRoles.Contains(
                role,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new ValidationException(
                "Invalid role. Allowed roles are Admin, Cashier and Customer.");
        }

        // Normalize role to the actual application role name.
        role = allowedRoles.First(
            x => x.Equals(
                role,
                StringComparison.OrdinalIgnoreCase));

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(
            user,
            request.Password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                createResult.Errors.Select(
                    x => x.Description));

            throw new ValidationException(errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(
            user,
            role);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            var errors = string.Join(
                "; ",
                roleResult.Errors.Select(
                    x => x.Description));

            throw new ValidationException(errors);
        }

        return MapToDto(
            user,
            new List<string> { role });
    }

    public async Task<UserDto?> UpdateAsync(
        Guid id,
        UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(
            id.ToString());

        if (user == null)
        {
            return null;
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(
                    x => x.Description));

            throw new ValidationException(errors);
        }

        var roles = await _userManager.GetRolesAsync(user);

        return MapToDto(user, roles);
    }

    public async Task<bool> DeactivateAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(
            id.ToString());

        if (user == null)
        {
            return false;
        }

        // Identity does not have an IsActive property by default.
        // Lockout is being used to deactivate the account.
        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(
                    x => x.Description));

            throw new ValidationException(errors);
        }

        return true;
    }

    private static UserDto MapToDto(
        ApplicationUser user,
        IList<string> roles)
    {
        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            IsActive = !user.LockoutEnd.HasValue ||
                       user.LockoutEnd <= DateTimeOffset.UtcNow,
            Roles = roles
        };
    }
}