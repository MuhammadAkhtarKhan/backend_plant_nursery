using PlantNursery.Application.DTOs.Categories;

namespace PlantNursery.Application.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync();

    Task<CategoryDto?> GetByIdAsync(Guid id);

    Task<CategoryDto> CreateAsync(CreateCategoryRequest request);

    Task<CategoryDto?> UpdateAsync(Guid id, UpdateCategoryRequest request);

    Task<bool> DeactivateAsync(Guid id);
}