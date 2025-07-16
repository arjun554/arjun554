using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FoodDash.API.Services;
using FoodDash.Shared.DTOs.Common;
using FoodDash.Shared.DTOs.Order;
using FoodDash.Shared.Enums;

namespace FoodDash.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder([FromBody] CreateOrderDto orderDto)
    {
        try
        {
            var customerId = GetCurrentUserId();
            var result = await _orderService.CreateOrderAsync(orderDto, customerId);
            
            if (result.Success)
            {
                return CreatedAtAction(nameof(GetOrder), new { id = result.Data!.Id }, result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, ApiResponse<OrderDto>.ErrorResponse("Internal server error"));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrder(int id)
    {
        try
        {
            var result = await _orderService.GetOrderByIdAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            // Verify access permissions
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            
            if (!CanAccessOrder(result.Data!, userId, userRole))
            {
                return Forbid();
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order {Id}", id);
            return StatusCode(500, ApiResponse<OrderDto>.ErrorResponse("Internal server error"));
        }
    }

    [HttpGet("my-orders")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetMyOrders()
    {
        try
        {
            var customerId = GetCurrentUserId();
            var result = await _orderService.GetOrdersByCustomerAsync(customerId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer orders");
            return StatusCode(500, ApiResponse<List<OrderDto>>.ErrorResponse("Internal server error"));
        }
    }

    [HttpGet("restaurant/{restaurantId}")]
    [Authorize(Roles = "RestaurantOwner,Admin")]
    public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetRestaurantOrders(int restaurantId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            
            // Verify restaurant ownership for restaurant owners
            if (userRole == UserRole.RestaurantOwner)
            {
                // Additional verification would be needed here to check restaurant ownership
                // For simplicity, we'll allow it for now
            }

            var result = await _orderService.GetOrdersByRestaurantAsync(restaurantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting restaurant orders for restaurant {RestaurantId}", restaurantId);
            return StatusCode(500, ApiResponse<List<OrderDto>>.ErrorResponse("Internal server error"));
        }
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            
            var result = await _orderService.UpdateOrderStatusAsync(id, request.Status, userId, userRole);
            
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status for order {Id}", id);
            return StatusCode(500, ApiResponse<OrderDto>.ErrorResponse("Internal server error"));
        }
    }

    [HttpPut("{id}/assign-delivery")]
    [Authorize(Roles = "Admin,RestaurantOwner")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> AssignDeliveryPartner(int id, [FromBody] AssignDeliveryPartnerRequest request)
    {
        try
        {
            var result = await _orderService.AssignDeliveryPartnerAsync(id, request.DeliveryPartnerId);
            
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning delivery partner to order {Id}", id);
            return StatusCode(500, ApiResponse<OrderDto>.ErrorResponse("Internal server error"));
        }
    }

    [HttpGet("available-for-delivery")]
    [Authorize(Roles = "DeliveryPartner")]
    public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetAvailableOrdersForDelivery()
    {
        try
        {
            // This would need implementation in OrderService
            // For now, return empty list
            var result = ApiResponse<List<OrderDto>>.SuccessResponse(new List<OrderDto>());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available orders for delivery");
            return StatusCode(500, ApiResponse<List<OrderDto>>.ErrorResponse("Internal server error"));
        }
    }

    [HttpGet("my-deliveries")]
    [Authorize(Roles = "DeliveryPartner")]
    public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetMyDeliveries()
    {
        try
        {
            var deliveryPartnerId = GetCurrentUserId();
            // This would need implementation in OrderService
            // For now, return empty list
            var result = ApiResponse<List<OrderDto>>.SuccessResponse(new List<OrderDto>());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting delivery partner orders");
            return StatusCode(500, ApiResponse<List<OrderDto>>.ErrorResponse("Internal server error"));
        }
    }

    [HttpPost("{id}/accept-delivery")]
    [Authorize(Roles = "DeliveryPartner")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> AcceptDelivery(int id)
    {
        try
        {
            var deliveryPartnerId = GetCurrentUserId();
            var result = await _orderService.AssignDeliveryPartnerAsync(id, deliveryPartnerId);
            
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting delivery for order {Id}", id);
            return StatusCode(500, ApiResponse<OrderDto>.ErrorResponse("Internal server error"));
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user token");
        }
        return userId;
    }

    private UserRole GetCurrentUserRole()
    {
        var roleClaim = User.FindFirst("Role");
        if (roleClaim == null || !Enum.TryParse<UserRole>(roleClaim.Value, out var role))
        {
            throw new UnauthorizedAccessException("Invalid user role");
        }
        return role;
    }

    private static bool CanAccessOrder(OrderDto order, int userId, UserRole userRole)
    {
        return userRole switch
        {
            UserRole.Admin => true,
            UserRole.Customer => order.CustomerId == userId,
            UserRole.RestaurantOwner => true, // Would need restaurant ownership verification
            UserRole.DeliveryPartner => order.DeliveryPartnerId == userId,
            _ => false
        };
    }
}

// Request DTOs
public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}

public class AssignDeliveryPartnerRequest
{
    public int DeliveryPartnerId { get; set; }
}