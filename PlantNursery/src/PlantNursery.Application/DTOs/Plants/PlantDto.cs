namespace PlantNursery.Application.DTOs.Plants;

public class PlantDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public string? ScientificName { get; set; }

    public string? CareInstructions { get; set; }

    public string? SunlightRequirement { get; set; }

    public string? WateringFrequency { get; set; }

    public string? Size { get; set; }

    public bool IsActive { get; set; }

    public Guid CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public List<PlantImageDto> Images { get; set; } = new();

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
