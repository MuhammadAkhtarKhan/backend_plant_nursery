using System.ComponentModel.DataAnnotations;
using PlantNursery.Domain.Enums;

namespace PlantNursery.Application.DTOs.Orders;

public class UpdateOrderStatusRequest
{
    [Required]
    public OrderStatus Status { get; set; }
}