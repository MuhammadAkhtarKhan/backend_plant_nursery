namespace PlantNursery.Application.DTOs.Plants;

public class PlantImageDto
{
    public Guid Id { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }
}