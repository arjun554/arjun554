using AutoMapper;
using Microsoft.EntityFrameworkCore;
using FoodDash.Database;
using FoodDash.Database.Entities;
using FoodDash.Shared.DTOs.Common;
using FoodDash.Shared.DTOs.Order;
using FoodDash.Shared.Enums;

namespace FoodDash.API.Services;

public class OrderService : IOrderService
{
    private readonly FoodDashDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;
    private readonly INotificationService _notificationService;

    public OrderService(
        FoodDashDbContext context, 
        IMapper mapper, 
        ILogger<OrderService> logger,
        INotificationService notificationService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderDto orderDto, int customerId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Validate customer
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == customerId);

            if (customer == null)
            {
                return ApiResponse<OrderDto>.ErrorResponse("Customer not found");
            }

            // Validate restaurant
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Id == orderDto.RestaurantId && r.Status == RestaurantStatus.Approved);

            if (restaurant == null)
            {
                return ApiResponse<OrderDto>.ErrorResponse("Restaurant not found or not available");
            }

            if (!restaurant.IsOpen)
            {
                return ApiResponse<OrderDto>.ErrorResponse("Restaurant is currently closed");
            }

            // Validate menu items and calculate totals
            var orderItems = new List<OrderItem>();
            decimal subTotal = 0;

            foreach (var itemDto in orderDto.OrderItems)
            {
                var menuItem = await _context.MenuItems
                    .FirstOrDefaultAsync(mi => mi.Id == itemDto.MenuItemId && mi.RestaurantId == orderDto.RestaurantId);

                if (menuItem == null)
                {
                    return ApiResponse<OrderDto>.ErrorResponse($"Menu item with ID {itemDto.MenuItemId} not found");
                }

                if (!menuItem.IsAvailable)
                {
                    return ApiResponse<OrderDto>.ErrorResponse($"Menu item '{menuItem.Name}' is not available");
                }

                var orderItem = new OrderItem
                {
                    MenuItemId = menuItem.Id,
                    Quantity = itemDto.Quantity,
                    UnitPrice = menuItem.Price,
                    TotalPrice = menuItem.Price * itemDto.Quantity,
                    SpecialInstructions = itemDto.SpecialInstructions
                };

                orderItems.Add(orderItem);
                subTotal += orderItem.TotalPrice;
            }

            // Check minimum order amount
            if (subTotal < restaurant.MinimumOrderAmount)
            {
                return ApiResponse<OrderDto>.ErrorResponse($"Minimum order amount is {restaurant.MinimumOrderAmount:C}");
            }

            // Calculate taxes and fees
            var taxRate = 0.13m; // 13% VAT in Nepal
            var taxAmount = subTotal * taxRate;
            var deliveryFee = restaurant.DeliveryFee;
            var discountAmount = 0m;

            // Apply coupon if provided
            if (!string.IsNullOrEmpty(orderDto.CouponCode))
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.Code == orderDto.CouponCode && c.IsActive && 
                                            c.ExpiryDate > DateTime.UtcNow);

                if (coupon != null)
                {
                    if (coupon.MinimumOrderAmount <= subTotal)
                    {
                        discountAmount = coupon.DiscountType == "Percentage" 
                            ? subTotal * (coupon.DiscountValue / 100)
                            : coupon.DiscountValue;
                        
                        if (coupon.MaxDiscountAmount.HasValue && discountAmount > coupon.MaxDiscountAmount)
                        {
                            discountAmount = coupon.MaxDiscountAmount.Value;
                        }
                    }
                }
            }

            var totalAmount = subTotal + taxAmount + deliveryFee - discountAmount;

            // Create order
            var order = new Order
            {
                OrderNumber = await GenerateOrderNumberAsync(),
                CustomerId = customerId,
                RestaurantId = orderDto.RestaurantId,
                Status = OrderStatus.Pending,
                DeliveryAddress = orderDto.DeliveryAddress,
                DeliveryInstructions = orderDto.DeliveryInstructions,
                SubTotal = subTotal,
                TaxAmount = taxAmount,
                DeliveryFee = deliveryFee,
                DiscountAmount = discountAmount,
                TotalAmount = totalAmount,
                PaymentMethod = orderDto.PaymentMethod,
                IsPaid = orderDto.PaymentMethod == PaymentMethod.CashOnDelivery ? false : false, // Will be updated after payment
                EstimatedDeliveryTime = restaurant.EstimatedDeliveryTime,
                CouponCode = orderDto.CouponCode,
                OrderItems = orderItems
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Update customer stats
            customer.TotalOrders++;
            customer.LastOrderDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            // Reload order with related data
            var createdOrder = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            var result = _mapper.Map<OrderDto>(createdOrder);

            // Send notifications
            await _notificationService.SendOrderUpdateAsync(order.Id, "Order placed successfully");

            return ApiResponse<OrderDto>.SuccessResponse(result, "Order created successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating order for customer {CustomerId}", customerId);
            return ApiResponse<OrderDto>.ErrorResponse("Failed to create order");
        }
    }

    public async Task<ApiResponse<OrderDto>> GetOrderByIdAsync(int id)
    {
        try
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Restaurant)
                .Include(o => o.DeliveryPartner)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return ApiResponse<OrderDto>.ErrorResponse("Order not found");
            }

            var result = _mapper.Map<OrderDto>(order);
            return ApiResponse<OrderDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order {Id}", id);
            return ApiResponse<OrderDto>.ErrorResponse("Failed to get order");
        }
    }

    public async Task<ApiResponse<List<OrderDto>>> GetOrdersByCustomerAsync(int customerId)
    {
        try
        {
            var orders = await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.DeliveryPartner)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var result = _mapper.Map<List<OrderDto>>(orders);
            return ApiResponse<List<OrderDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for customer {CustomerId}", customerId);
            return ApiResponse<List<OrderDto>>.ErrorResponse("Failed to get orders");
        }
    }

    public async Task<ApiResponse<List<OrderDto>>> GetOrdersByRestaurantAsync(int restaurantId)
    {
        try
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.DeliveryPartner)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.RestaurantId == restaurantId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var result = _mapper.Map<List<OrderDto>>(orders);
            return ApiResponse<List<OrderDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for restaurant {RestaurantId}", restaurantId);
            return ApiResponse<List<OrderDto>>.ErrorResponse("Failed to get orders");
        }
    }

    public async Task<ApiResponse<OrderDto>> UpdateOrderStatusAsync(int orderId, OrderStatus status, int userId, UserRole userRole)
    {
        try
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Restaurant)
                .Include(o => o.DeliveryPartner)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return ApiResponse<OrderDto>.ErrorResponse("Order not found");
            }

            // Validate permissions
            if (!await CanUpdateOrderStatus(order, userId, userRole, status))
            {
                return ApiResponse<OrderDto>.ErrorResponse("Not authorized to update this order status");
            }

            var previousStatus = order.Status;
            order.Status = status;

            // Update timestamps based on status
            switch (status)
            {
                case OrderStatus.Confirmed:
                    order.ConfirmedAt = DateTime.UtcNow;
                    break;
                case OrderStatus.ReadyForPickup:
                    order.ReadyAt = DateTime.UtcNow;
                    break;
                case OrderStatus.Delivered:
                    order.DeliveredAt = DateTime.UtcNow;
                    // Update customer total spent
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == order.CustomerId);
                    if (customer != null)
                    {
                        customer.TotalSpent += order.TotalAmount;
                        customer.LoyaltyPoints += (int)(order.TotalAmount / 100); // 1 point per $1
                    }
                    // Update delivery partner stats
                    if (order.DeliveryPartnerId.HasValue)
                    {
                        var deliveryPartner = await _context.DeliveryPartners
                            .FirstOrDefaultAsync(dp => dp.UserId == order.DeliveryPartnerId.Value);
                        if (deliveryPartner != null)
                        {
                            deliveryPartner.TotalDeliveries++;
                            deliveryPartner.TotalEarnings += order.DeliveryFee * 0.8m; // 80% commission
                        }
                    }
                    break;
            }

            await _context.SaveChangesAsync();

            // Send notifications
            await _notificationService.SendOrderUpdateAsync(orderId, $"Order status updated to {status}");

            var result = _mapper.Map<OrderDto>(order);
            return ApiResponse<OrderDto>.SuccessResponse(result, "Order status updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status for order {OrderId}", orderId);
            return ApiResponse<OrderDto>.ErrorResponse("Failed to update order status");
        }
    }

    public async Task<ApiResponse<OrderDto>> AssignDeliveryPartnerAsync(int orderId, int deliveryPartnerId)
    {
        try
        {
            var order = await _context.Orders
                .Include(o => o.Restaurant)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return ApiResponse<OrderDto>.ErrorResponse("Order not found");
            }

            var deliveryPartner = await _context.DeliveryPartners
                .Include(dp => dp.User)
                .FirstOrDefaultAsync(dp => dp.UserId == deliveryPartnerId && dp.IsAvailable);

            if (deliveryPartner == null)
            {
                return ApiResponse<OrderDto>.ErrorResponse("Delivery partner not found or not available");
            }

            order.DeliveryPartnerId = deliveryPartnerId;
            if (order.Status == OrderStatus.ReadyForPickup)
            {
                order.Status = OrderStatus.OutForDelivery;
            }

            deliveryPartner.Status = DeliveryStatus.OnDelivery;

            await _context.SaveChangesAsync();

            await _notificationService.SendOrderUpdateAsync(orderId, "Delivery partner assigned");

            var result = _mapper.Map<OrderDto>(order);
            return ApiResponse<OrderDto>.SuccessResponse(result, "Delivery partner assigned successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning delivery partner to order {OrderId}", orderId);
            return ApiResponse<OrderDto>.ErrorResponse("Failed to assign delivery partner");
        }
    }

    private async Task<string> GenerateOrderNumberAsync()
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var dailyCount = await _context.Orders
            .CountAsync(o => o.CreatedAt.Date == DateTime.UtcNow.Date);
        
        return $"ORD-{today}-{(dailyCount + 1):D3}";
    }

    private async Task<bool> CanUpdateOrderStatus(Order order, int userId, UserRole userRole, OrderStatus newStatus)
    {
        switch (userRole)
        {
            case UserRole.Admin:
                return true;

            case UserRole.RestaurantOwner:
                var restaurant = await _context.Restaurants
                    .FirstOrDefaultAsync(r => r.Id == order.RestaurantId && r.OwnerId == userId);
                return restaurant != null && CanRestaurantUpdateToStatus(order.Status, newStatus);

            case UserRole.DeliveryPartner:
                return order.DeliveryPartnerId == userId && CanDeliveryPartnerUpdateToStatus(order.Status, newStatus);

            case UserRole.Customer:
                return order.CustomerId == userId && newStatus == OrderStatus.Cancelled && 
                       order.Status == OrderStatus.Pending;

            default:
                return false;
        }
    }

    private static bool CanRestaurantUpdateToStatus(OrderStatus currentStatus, OrderStatus newStatus)
    {
        return (currentStatus, newStatus) switch
        {
            (OrderStatus.Pending, OrderStatus.Confirmed) => true,
            (OrderStatus.Pending, OrderStatus.Rejected) => true,
            (OrderStatus.Confirmed, OrderStatus.Preparing) => true,
            (OrderStatus.Preparing, OrderStatus.ReadyForPickup) => true,
            _ => false
        };
    }

    private static bool CanDeliveryPartnerUpdateToStatus(OrderStatus currentStatus, OrderStatus newStatus)
    {
        return (currentStatus, newStatus) switch
        {
            (OrderStatus.ReadyForPickup, OrderStatus.OutForDelivery) => true,
            (OrderStatus.OutForDelivery, OrderStatus.Delivered) => true,
            _ => false
        };
    }
}