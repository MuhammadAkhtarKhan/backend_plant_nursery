using Microsoft.EntityFrameworkCore;
using PlantNursery.Application.Common.Exceptions;
using PlantNursery.Application.DTOs.Orders;
using PlantNursery.Application.Interfaces;
using PlantNursery.Domain.Entities;
using PlantNursery.Domain.Enums;
using PlantNursery.Infrastructure.Persistence;

namespace PlantNursery.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto> CreateAsync(
        Guid userId,
        CreateOrderRequest request,
        bool isPos)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            throw new ValidationException(
                "An order must contain at least one item.");
        }

        // Prevent duplicate plants in the same order.
        var duplicatePlantIds = request.Items
            .GroupBy(x => x.PlantId)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

        if (duplicatePlantIds.Count > 0)
        {
            throw new ValidationException(
                "The same plant cannot appear more than once in an order.");
        }

        // Load all requested plants in one query.
        var plantIds = request.Items
            .Select(x => x.PlantId)
            .ToList();

        var plants = await _context.Plants
            .Where(x =>
                plantIds.Contains(x.Id) &&
                x.IsActive)
            .ToDictionaryAsync(x => x.Id);

        // Make sure every requested plant exists.
        if (plants.Count != plantIds.Count)
        {
            var missingPlantIds = plantIds
                .Where(id => !plants.ContainsKey(id))
                .ToList();

            throw new NotFoundException(
                $"One or more plants were not found: " +
                string.Join(", ", missingPlantIds));
        }

        // Validate stock before modifying anything.
        foreach (var item in request.Items)
        {
            var plant = plants[item.PlantId];

            if (plant.StockQuantity < item.Quantity)
            {
                throw new ValidationException(
                    $"Insufficient stock for plant '{plant.Name}'. " +
                    $"Available: {plant.StockQuantity}, " +
                    $"requested: {item.Quantity}.");
            }
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),

            OrderNumber = GenerateOrderNumber(),

            OrderType = isPos
                ? OrderType.POS
                : OrderType.Online,

            UserId = userId,

            PaymentMethod = request.PaymentMethod,

            PaymentStatus = PaymentStatus.Paid,

            OrderStatus = isPos
                ? OrderStatus.Completed
                : OrderStatus.Confirmed,

            CreatedAt = DateTime.UtcNow
        };

        decimal totalAmount = 0;

        foreach (var item in request.Items)
        {
            var plant = plants[item.PlantId];

            var unitPrice = plant.Price;

            var totalPrice =
                unitPrice * item.Quantity;

            totalAmount += totalPrice;

            // Reduce shared inventory.
            plant.StockQuantity -= item.Quantity;
            plant.UpdatedAt = DateTime.UtcNow;

            order.Items.Add(
                new OrderItem
                {
                    Id = Guid.NewGuid(),

                    PlantId = plant.Id,

                    Quantity = item.Quantity,

                    UnitPrice = unitPrice,

                    TotalPrice = totalPrice,

                    CreatedAt = DateTime.UtcNow
                });
        }

        order.TotalAmount = totalAmount;

        _context.Orders.Add(order);

        await _context.SaveChangesAsync();

        return (await GetOrderDtoAsync(order.Id))!;
    }

    public async Task<OrderDto?> GetByIdAsync(
        Guid orderId,
        Guid userId,
        bool isAdmin)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Where(x => x.Id == orderId);

        // Admin can see any order.
        // Other users can only see their own order.
        if (!isAdmin)
        {
            query = query.Where(x => x.UserId == userId);
        }

        return await ProjectToDto(query)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<OrderDto>> GetMyOrdersAsync(
        Guid userId)
    {
        return await ProjectToDto(
                _context.Orders
                    .AsNoTracking()
                    .Where(x => x.UserId == userId))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrderDto>> GetAllAsync()
    {
        return await ProjectToDto(
                _context.Orders.AsNoTracking())
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    private async Task<OrderDto?> GetOrderDtoAsync(
        Guid orderId)
    {
        return await ProjectToDto(
                _context.Orders
                    .AsNoTracking()
                    .Where(x => x.Id == orderId))
            .FirstOrDefaultAsync();
    }

    private static IQueryable<OrderDto> ProjectToDto(
        IQueryable<Order> query)
    {
        return query.Select(order => new OrderDto
        {
            Id = order.Id,

            OrderNumber = order.OrderNumber,

            OrderType = order.OrderType,

            UserId = order.UserId,

            TotalAmount = order.TotalAmount,

            PaymentMethod = order.PaymentMethod,

            PaymentStatus = order.PaymentStatus,

            OrderStatus = order.OrderStatus,

            CreatedAt = order.CreatedAt,

            Items = order.Items
                .Select(item => new OrderItemDto
                {
                    Id = item.Id,

                    PlantId = item.PlantId,

                    PlantName = item.Plant.Name,

                    Quantity = item.Quantity,

                    UnitPrice = item.UnitPrice,

                    TotalPrice = item.TotalPrice
                })
                .ToList()
        });
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmssfff}-" +
               Random.Shared.Next(100, 999);
    }
}