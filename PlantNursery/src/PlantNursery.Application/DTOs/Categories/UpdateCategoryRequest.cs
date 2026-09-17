using System.ComponentModel.DataAnnotations;

namespace PlantNursery.Application.DTOs.Categories;

public class UpdateCategoryRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; } = null;

    [Url]   
    public string? ImageUrl { get; set; }= null;
}