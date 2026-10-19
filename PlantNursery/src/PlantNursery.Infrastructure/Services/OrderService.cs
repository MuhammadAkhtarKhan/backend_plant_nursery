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

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();

            throw new ConflictException( "The stock was changed by another transaction. Please refresh the plant stock and try again.");
        }
        catch
        {
            await transaction.RollbackAsync();

            throw;
        }

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
    public async Task<OrderDto?> UpdateStatusAsync( Guid orderId, OrderStatus newStatus)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(x => x.Id == orderId);

        if (order == null)
        {
            return null;
        }

        bool isValidTransition =
            (order.OrderStatus == OrderStatus.Pending &&
             newStatus == OrderStatus.Confirmed)
            ||
            (order.OrderStatus == OrderStatus.Confirmed &&
             newStatus == OrderStatus.Processing)
            ||
            (order.OrderStatus == OrderStatus.Processing &&
             newStatus == OrderStatus.Completed);

        if (!isValidTransition)
        {
            throw new ValidationException(
                $"Cannot change order status from " +
                $"{order.OrderStatus} to {newStatus}.");
        }

        order.OrderStatus = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(  orderId, order.UserId, isAdmin: true);
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
    public async Task<bool> CancelAsync(Guid orderId)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var order = await _context.Orders
                .Include(x => x.Items)
                .ThenInclude(x => x.Plant)
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order == null)
            {
                return false;
            }

            if (order.OrderStatus == OrderStatus.Cancelled)
            {
                throw new ConflictException(
                    "This order has already been cancelled.");
            }

            if (order.OrderStatus != OrderStatus.Pending &&
                order.OrderStatus != OrderStatus.Confirmed &&
                order.OrderStatus != OrderStatus.Processing)
            {
                throw new ValidationException(
                    "Only pending, confirmed, or processing orders can be cancelled.");
            }

            foreach (var item in order.Items)
            {
                item.Plant.StockQuantity += item.Quantity;
                item.Plant.UpdatedAt = DateTime.UtcNow;
            }

            order.OrderStatus = OrderStatus.Cancelled;
            order.PaymentStatus = order.PaymentStatus == PaymentStatus.Paid
                ? PaymentStatus.Refunded
                : order.PaymentStatus;

            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();

            throw new ConflictException(
                "The order or inventory changed during cancellation. Refresh and try again.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}