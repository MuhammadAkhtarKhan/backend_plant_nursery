using System.ComponentModel.DataAnnotations;

namespace PlantNursery.Application.DTOs.Plants;

public class CreatePlantImageRequest
{
    [Required]
    [Url]
    [StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }
}