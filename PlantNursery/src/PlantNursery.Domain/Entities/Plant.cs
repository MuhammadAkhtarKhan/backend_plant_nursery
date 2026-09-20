

namespace PlantNursery.Domain.Entities;

public class Plant : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public string? ScientificName { get; set; }

    public string? CareInstructions { get; set; }

    public string? SunlightRequirement { get; set; }

    public string? WateringFrequency { get; set; }

    public string? Size { get; set; }

    public bool IsActive { get; set; } = true;

    public Guid CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public ICollection<PlantImage> Images { get; set; } = new List<PlantImage>();
}
