using PlantNursery.Domain.Enums;

namespace PlantNursery.Application.DTOs.Orders;

public class OrderDto
{
    public Guid Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public OrderType OrderType { get; set; }

    public Guid UserId { get; set; }

    public decimal TotalAmount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public OrderStatus OrderStatus { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<OrderItemDto> Items { get; set; } = new();
}