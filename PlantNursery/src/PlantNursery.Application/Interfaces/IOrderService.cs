using PlantNursery.Application.DTOs.Orders;

namespace PlantNursery.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateAsync(
        Guid userId,
        CreateOrderRequest request,
        bool isPos);

    Task<OrderDto?> GetByIdAsync(
        Guid orderId,
        Guid userId,
        bool isAdmin);

    Task<IEnumerable<OrderDto>> GetMyOrdersAsync(
        Guid userId);

    Task<IEnumerable<OrderDto>> GetAllAsync();
}