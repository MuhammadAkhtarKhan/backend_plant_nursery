using Microsoft.EntityFrameworkCore;
using PlantNursery.Application.DTOs.Categories;
using PlantNursery.Application.Interfaces;
using PlantNursery.Domain.Entities;
using PlantNursery.Infrastructure.Persistence;

namespace PlantNursery.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(x => x.Id == id && x.IsActive)
            .Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CategoryDto> CreateAsync(
        CreateCategoryRequest request)
    {
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Category name is required.");
        }

        var exists = await _context.Categories
            .AnyAsync(x => x.Name == name);

        if (exists)
        {
            throw new InvalidOperationException(
                "A category with this name already exists.");
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = request.Description?.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);

        await _context.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<CategoryDto?> UpdateAsync(
        Guid id,
        UpdateCategoryRequest request)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == id);

        if (category == null || !category.IsActive)
        {
            return null;
        }

        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Category name is required.");
        }

        var duplicateName = await _context.Categories
            .AnyAsync(x =>
                x.Id != id &&
                x.Name == name);

        if (duplicateName)
        {
            throw new InvalidOperationException(
                "A category with this name already exists.");
        }

        category.Name = name;
        category.Description = request.Description?.Trim();
        category.ImageUrl = request.ImageUrl?.Trim();
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<bool> DeactivateAsync(Guid id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == id);

        if (category == null || !category.IsActive)
        {
            return false;
        }

        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }
}