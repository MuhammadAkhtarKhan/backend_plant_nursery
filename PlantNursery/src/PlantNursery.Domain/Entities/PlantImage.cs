namespace PlantNursery.Domain.Entities;

public class PlantImage : BaseEntity
{
    public Guid PlantId { get; set; }

    public Plant Plant { get; set; } = null!;

    public string ImageUrl { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }
}