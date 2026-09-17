using Microsoft.EntityFrameworkCore;
using PlantNursery.Application.DTOs.Plants;
using PlantNursery.Application.Interfaces;
using PlantNursery.Domain.Entities;
using PlantNursery.Infrastructure.Persistence;

namespace PlantNursery.Infrastructure.Services;

public class PlantService : IPlantService
{
    private readonly ApplicationDbContext _context;

    public PlantService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PlantDto>> GetAllAsync()
    {
        return await _context.Plants
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new PlantDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                StockQuantity = x.StockQuantity,
                ScientificName = x.ScientificName,
                CareInstructions = x.CareInstructions,
                SunlightRequirement = x.SunlightRequirement,
                WateringFrequency = x.WateringFrequency,
                Size = x.Size,
                IsActive = x.IsActive,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,

                Images = x.Images
                    .Select(i => new PlantImageDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        IsPrimary = i.IsPrimary
                    })
                    .ToList(),

                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<PlantDto?> GetByIdAsync(Guid id)
    {
        return await _context.Plants
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.IsActive)
            .Select(x => new PlantDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                StockQuantity = x.StockQuantity,
                ScientificName = x.ScientificName,
                CareInstructions = x.CareInstructions,
                SunlightRequirement = x.SunlightRequirement,
                WateringFrequency = x.WateringFrequency,
                Size = x.Size,
                IsActive = x.IsActive,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,

                Images = x.Images
                    .Select(i => new PlantImageDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        IsPrimary = i.IsPrimary
                    })
                    .ToList(),

                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PlantDto> CreateAsync(
        CreatePlantRequest request)
    {
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Plant name is required.");
        }

        // Validate Category
        var categoryExists = await _context.Categories
            .AnyAsync(x =>
                x.Id == request.CategoryId &&
                x.IsActive);

        if (!categoryExists)
        {
            throw new ArgumentException(
                "The selected category does not exist or is inactive.");
        }

        // Prevent duplicate plant names
        var duplicateName = await _context.Plants
            .AnyAsync(x =>
                x.Name == name &&
                x.IsActive);

        if (duplicateName)
        {
            throw new InvalidOperationException(
                "A plant with this name already exists.");
        }

        var plant = new Plant
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = request.Description?.Trim(),
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            ScientificName = request.ScientificName?.Trim(),
            CareInstructions = request.CareInstructions?.Trim(),
            SunlightRequirement = request.SunlightRequirement?.Trim(),
            WateringFrequency = request.WateringFrequency?.Trim(),
            Size = request.Size?.Trim(),
            CategoryId = request.CategoryId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var imageRequest in request.Images)
        {
            plant.Images.Add(new PlantImage
            {
                Id = Guid.NewGuid(),
                ImageUrl = imageRequest.ImageUrl.Trim(),
                IsPrimary = imageRequest.IsPrimary,
                CreatedAt = DateTime.UtcNow
            });
        }

        _context.Plants.Add(plant);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(plant.Id))!;
    }

    public async Task<PlantDto?> UpdateAsync(
        Guid id,
        UpdatePlantRequest request)
    {
        var plant = await _context.Plants
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (plant == null || !plant.IsActive)
        {
            return null;
        }

        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Plant name is required.");
        }

        // Validate Category
        var categoryExists = await _context.Categories
            .AnyAsync(x =>
                x.Id == request.CategoryId &&
                x.IsActive);

        if (!categoryExists)
        {
            throw new ArgumentException(
                "The selected category does not exist or is inactive.");
        }

        // Prevent duplicate names
        var duplicateName = await _context.Plants
            .AnyAsync(x =>
                x.Id != id &&
                x.Name == name &&
                x.IsActive);

        if (duplicateName)
        {
            throw new InvalidOperationException(
                "A plant with this name already exists.");
        }

        plant.Name = name;
        plant.Description = request.Description?.Trim();
        plant.Price = request.Price;
        plant.StockQuantity = request.StockQuantity;
        plant.ScientificName = request.ScientificName?.Trim();
        plant.CareInstructions = request.CareInstructions?.Trim();
        plant.SunlightRequirement =
            request.SunlightRequirement?.Trim();
        plant.WateringFrequency =
            request.WateringFrequency?.Trim();
        plant.Size = request.Size?.Trim();
        plant.CategoryId = request.CategoryId;
        plant.UpdatedAt = DateTime.UtcNow;


        // Remove existing images
        var existingImages = await _context.PlantImages
            .Where(x => x.PlantId == plant.Id)
            .ToListAsync();

        _context.PlantImages.RemoveRange(existingImages);       

        // Add new images
        foreach (var imageRequest in request.Images)
        {
            var image = new PlantImage
            {
                Id = Guid.NewGuid(),
                PlantId = plant.Id,
                ImageUrl = imageRequest.ImageUrl.Trim(),
                IsPrimary = imageRequest.IsPrimary,
                CreatedAt = DateTime.UtcNow
            };

            _context.PlantImages.Add(image);
        }

        await _context.SaveChangesAsync();       

        return await GetByIdAsync(plant.Id);
    }

    public async Task<bool> DeactivateAsync(Guid id)
    {
        var plant = await _context.Plants
            .FirstOrDefaultAsync(x => x.Id == id);

        if (plant == null || !plant.IsActive)
        {
            return false;
        }

        plant.IsActive = false;
        plant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}