using System.ComponentModel.DataAnnotations;
using PlantNursery.Domain.Enums;

namespace PlantNursery.Application.DTOs.Orders;

public class CreateOrderRequest
{
    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}
