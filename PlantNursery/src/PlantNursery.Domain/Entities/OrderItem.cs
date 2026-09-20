namespace PlantNursery.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }

    public Order Order { get; set; } = null!;

    public Guid PlantId { get; set; }

    public Plant Plant { get; set; } = null!;

    public int Quantity { get; set; }

    // Price at the time of sale.
    // This should NOT depend on the current Plant.Price.
    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }
}