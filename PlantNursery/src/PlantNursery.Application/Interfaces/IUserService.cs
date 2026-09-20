using PlantNursery.Application.DTOs.Users;

namespace PlantNursery.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync();

    Task<UserDto?> GetByIdAsync(Guid id);

    Task<UserDto> CreateAsync(CreateUserRequest request);

    Task<UserDto?> UpdateAsync(
        Guid id,
        UpdateUserRequest request);

    Task<bool> DeactivateAsync(Guid id);
}