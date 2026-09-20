using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantNursery.Application.Common;
using PlantNursery.Application.DTOs.Orders;
using PlantNursery.Application.Interfaces;

namespace PlantNursery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // ============================================================
    // Customer - Create Online Order
    // ============================================================

    [Authorize(Roles = "Customer")]
    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        CreateOrderRequest request)
    {
        var userId = GetCurrentUserId();

        var order = await _orderService.CreateAsync(
            userId,
            request,
            isPos: false);

        return CreatedAtAction(
            nameof(GetById),
            new { id = order.Id },
            ApiResponse<OrderDto>.Ok(
                order,
                "Order created successfully."));
    }


    // ============================================================
    // Cashier - Create POS Sale
    // ============================================================

    [Authorize(Roles = "Cashier")]
    [HttpPost("pos")]
    public async Task<IActionResult> CreatePosSale(
        CreateOrderRequest request)
    {
        var userId = GetCurrentUserId();

        var order = await _orderService.CreateAsync(
            userId,
            request,
            isPos: true);

        return CreatedAtAction(
            nameof(GetById),
            new { id = order.Id },
            ApiResponse<OrderDto>.Ok(
                order,
                "POS sale created successfully."));
    }


    // ============================================================
    // Customer/Cashier - Get My Orders/Sales
    // ============================================================

    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = GetCurrentUserId();

        var orders = await _orderService.GetMyOrdersAsync(
            userId);

        return Ok(
            ApiResponse<IEnumerable<OrderDto>>.Ok(
                orders,
                "Orders retrieved successfully."));
    }


    // ============================================================
    // Get single order
    // ============================================================

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetCurrentUserId();

        var isAdmin = User.IsInRole("Admin");

        var order = await _orderService.GetByIdAsync(
            id,
            userId,
            isAdmin);

        if (order == null)
        {
            return NotFound(
                ApiResponse.Fail(
                    "Order not found."));
        }

        return Ok(
            ApiResponse<OrderDto>.Ok(
                order,
                "Order retrieved successfully."));
    }


    // ============================================================
    // Admin - Get All Orders
    // ============================================================

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _orderService.GetAllAsync();

        return Ok(
            ApiResponse<IEnumerable<OrderDto>>.Ok(
                orders,
                "Orders retrieved successfully."));
    }


    // ============================================================
    // Helper
    // ============================================================

    private Guid GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            throw new UnauthorizedAccessException(
                "User identity is invalid.");
        }

        return userId;
    }
}