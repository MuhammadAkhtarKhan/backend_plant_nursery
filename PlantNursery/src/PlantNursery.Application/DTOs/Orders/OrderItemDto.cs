namespace PlantNursery.Application.DTOs.Orders;

public class OrderItemDto
{
    public Guid Id { get; set; }

    public Guid PlantId { get; set; }

    public string PlantName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }
}
