using PlantNursery.Application.DTOs.Plants;

namespace PlantNursery.Application.Interfaces;

public interface IPlantService
{
    Task<IEnumerable<PlantDto>> GetAllAsync();

    Task<PlantDto?> GetByIdAsync(Guid id);

    Task<PlantDto> CreateAsync(CreatePlantRequest request);

    Task<PlantDto?> UpdateAsync(
        Guid id,
        UpdatePlantRequest request);

    Task<bool> DeactivateAsync(Guid id);
}