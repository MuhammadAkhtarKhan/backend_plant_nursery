using System.ComponentModel.DataAnnotations;

namespace PlantNursery.Application.DTOs.Plants;

public class CreatePlantRequest
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(0.01, 999999999)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [StringLength(200)]
    public string? ScientificName { get; set; }

    [StringLength(2000)]
    public string? CareInstructions { get; set; }

    [StringLength(200)]
    public string? SunlightRequirement { get; set; }

    [StringLength(200)]
    public string? WateringFrequency { get; set; }

    [StringLength(100)]
    public string? Size { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    public List<CreatePlantImageRequest> Images { get; set; } = new();
}