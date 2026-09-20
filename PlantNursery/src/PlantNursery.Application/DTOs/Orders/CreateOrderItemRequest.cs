using System.ComponentModel.DataAnnotations;

namespace PlantNursery.Application.DTOs.Orders;

public class CreateOrderItemRequest
{
    [Required]
    public Guid PlantId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}