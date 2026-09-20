using PlantNursery.Domain.Enums;

namespace PlantNursery.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;

    public OrderType OrderType { get; set; }

    // User who created the order.
    // Customer for online orders.
    // Cashier for POS orders.
    public Guid UserId { get; set; }

    public decimal TotalAmount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public OrderStatus OrderStatus { get; set; }

    public ICollection<OrderItem> Items { get; set; }
        = new List<OrderItem>();
}
