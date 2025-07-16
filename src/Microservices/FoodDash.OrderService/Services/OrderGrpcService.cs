using Grpc.Core;
using AutoMapper;
using FoodDash.Shared.Grpc;
using FoodDash.Database;
using Microsoft.EntityFrameworkCore;
using FoodDash.Database.Entities;
using FoodDash.Shared.Enums;

namespace FoodDash.OrderService.Services;

public class OrderGrpcService : FoodDash.Shared.Grpc.OrderService.OrderServiceBase
{
    private readonly FoodDashDbContext _context;
    private readonly IMapper _mapper;
    private readonly IOrderBusinessService _orderBusinessService;
    private readonly ILogger<OrderGrpcService> _logger;

    public OrderGrpcService(
        FoodDashDbContext context,
        IMapper mapper,
        IOrderBusinessService orderBusinessService,
        ILogger<OrderGrpcService> logger)
    {
        _context = context;
        _mapper = mapper;
        _orderBusinessService = orderBusinessService;
        _logger = logger;
    }

    public override async Task<OrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Creating order for customer {CustomerId} at restaurant {RestaurantId}", 
                request.CustomerId, request.RestaurantId);

            using var transaction = await _context.Database.BeginTransactionAsync();

            // Validate customer
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == request.CustomerId);

            if (customer == null)
            {
                return new OrderResponse
                {
                    Success = false,
                    Message = "Customer not found"
                };
            }

            // Validate restaurant
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Id == request.RestaurantId && r.Status == RestaurantStatus.Approved);

            if (restaurant == null)
            {
                return new OrderResponse
                {
                    Success = false,
                    Message = "Restaurant not found or not available"
                };
            }

            if (!restaurant.IsOpen)
            {
                return new OrderResponse
                {
                    Success = false,
                    Message = "Restaurant is currently closed"
                };
            }

            // Process order items and calculate totals
            var orderItems = new List<OrderItem>();
            decimal subTotal = 0;

            foreach (var itemRequest in request.OrderItems)
            {
                var menuItem = await _context.MenuItems
                    .FirstOrDefaultAsync(mi => mi.Id == itemRequest.MenuItemId && mi.RestaurantId == request.RestaurantId);

                if (menuItem == null)
                {
                    return new OrderResponse
                    {
                        Success = false,
                        Message = $"Menu item with ID {itemRequest.MenuItemId} not found"
                    };
                }

                if (!menuItem.IsAvailable)
                {
                    return new OrderResponse
                    {
                        Success = false,
                        Message = $"Menu item '{menuItem.Name}' is not available"
                    };
                }

                var orderItem = new OrderItem
                {
                    MenuItemId = menuItem.Id,
                    Quantity = itemRequest.Quantity,
                    UnitPrice = menuItem.Price,
                    TotalPrice = menuItem.Price * itemRequest.Quantity,
                    SpecialInstructions = itemRequest.SpecialInstructions
                };

                orderItems.Add(orderItem);
                subTotal += orderItem.TotalPrice;
            }

            // Check minimum order amount
            if (subTotal < restaurant.MinimumOrderAmount)
            {
                return new OrderResponse
                {
                    Success = false,
                    Message = $"Minimum order amount is {restaurant.MinimumOrderAmount:C}"
                };
            }

            // Calculate totals
            var totals = await _orderBusinessService.CalculateOrderTotalsAsync(
                subTotal, restaurant.DeliveryFee, request.CouponCode, request.RestaurantId);

            // Create order
            var order = new Order
            {
                OrderNumber = await _orderBusinessService.GenerateOrderNumberAsync(),
                CustomerId = request.CustomerId,
                RestaurantId = request.RestaurantId,
                Status = OrderStatus.Pending,
                DeliveryAddress = request.DeliveryAddress,
                DeliveryInstructions = request.DeliveryInstructions,
                SubTotal = subTotal,
                TaxAmount = totals.TaxAmount,
                DeliveryFee = totals.DeliveryFee,
                DiscountAmount = totals.DiscountAmount,
                TotalAmount = totals.TotalAmount,
                PaymentMethod = Enum.Parse<PaymentMethod>(request.PaymentMethod),
                IsPaid = false,
                EstimatedDeliveryTime = restaurant.EstimatedDeliveryTime,
                CouponCode = request.CouponCode,
                OrderItems = orderItems
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Update customer stats
            customer.TotalOrders++;
            customer.LastOrderDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            // Reload with related data
            var createdOrder = await LoadOrderWithRelatedDataAsync(order.Id);

            _logger.LogInformation("Order {OrderId} created successfully", order.Id);

            return new OrderResponse
            {
                Success = true,
                Message = "Order created successfully",
                Order = MapToOrderData(createdOrder!)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return new OrderResponse
            {
                Success = false,
                Message = "Failed to create order"
            };
        }
    }

    public override async Task<OrderResponse> GetOrder(GetOrderRequest request, ServerCallContext context)
    {
        try
        {
            var order = await LoadOrderWithRelatedDataAsync(request.OrderId);

            if (order == null)
            {
                return new OrderResponse
                {
                    Success = false,
                    Message = "Order not found"
                };
            }

            return new OrderResponse
            {
                Success = true,
                Message = "Order retrieved successfully",
                Order = MapToOrderData(order)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order {OrderId}", request.OrderId);
            return new OrderResponse
            {
                Success = false,
                Message = "Failed to get order"
            };
        }
    }

    public override async Task<OrderResponse> UpdateOrderStatus(UpdateOrderStatusRequest request, ServerCallContext context)
    {
        try
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Restaurant)
                .Include(o => o.DeliveryPartner)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId);

            if (order == null)
            {
                return new OrderResponse
                {
                    Success = false,
                    Message = "Order not found"
                };
            }

            var newStatus = Enum.Parse<OrderStatus>(request.Status);
            var userRole = Enum.Parse<UserRole>(request.UserRole);

            // Validate permissions
            if (!await _orderBusinessService.CanUpdateOrderStatusAsync(order, request.UserId, userRole, newStatus))
            {
                return new OrderResponse
                {
                    Success = false,
                    Message = "Not authorized to update this order status"
                };
            }

            // Update status and related data
            await _orderBusinessService.UpdateOrderStatusAsync(order, newStatus);

            var updatedOrder = await LoadOrderWithRelatedDataAsync(order.Id);

            _logger.LogInformation("Order {OrderId} status updated to {Status}", order.Id, newStatus);

            return new OrderResponse
            {
                Success = true,
                Message = "Order status updated successfully",
                Order = MapToOrderData(updatedOrder!)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status for order {OrderId}", request.OrderId);
            return new OrderResponse
            {
                Success = false,
                Message = "Failed to update order status"
            };
        }
    }

    public override async Task<OrderListResponse> GetOrdersByRestaurant(GetOrdersByRestaurantRequest request, ServerCallContext context)
    {
        try
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.DeliveryPartner)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.RestaurantId == request.RestaurantId)
                .OrderByDescending(o => o.CreatedAt);

            var totalCount = await query.CountAsync();
            var orders = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new OrderListResponse
            {
                Success = true,
                Message = "Orders retrieved successfully",
                Orders = { orders.Select(MapToOrderData) },
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for restaurant {RestaurantId}", request.RestaurantId);
            return new OrderListResponse
            {
                Success = false,
                Message = "Failed to get orders"
            };
        }
    }

    public override async Task<OrderListResponse> GetOrdersByCustomer(GetOrdersByCustomerRequest request, ServerCallContext context)
    {
        try
        {
            var query = _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.DeliveryPartner)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.CustomerId == request.CustomerId)
                .OrderByDescending(o => o.CreatedAt);

            var totalCount = await query.CountAsync();
            var orders = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new OrderListResponse
            {
                Success = true,
                Message = "Orders retrieved successfully",
                Orders = { orders.Select(MapToOrderData) },
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for customer {CustomerId}", request.CustomerId);
            return new OrderListResponse
            {
                Success = false,
                Message = "Failed to get orders"
            };
        }
    }

    public override async Task<OrderResponse> AssignDeliveryPartner(AssignDeliveryPartnerRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _orderBusinessService.AssignDeliveryPartnerAsync(request.OrderId, request.DeliveryPartnerId);

            if (result.Success)
            {
                var order = await LoadOrderWithRelatedDataAsync(request.OrderId);
                return new OrderResponse
                {
                    Success = true,
                    Message = "Delivery partner assigned successfully",
                    Order = MapToOrderData(order!)
                };
            }

            return new OrderResponse
            {
                Success = false,
                Message = result.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning delivery partner to order {OrderId}", request.OrderId);
            return new OrderResponse
            {
                Success = false,
                Message = "Failed to assign delivery partner"
            };
        }
    }

    private async Task<Order?> LoadOrderWithRelatedDataAsync(int orderId)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Restaurant)
            .Include(o => o.DeliveryPartner)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    private static OrderData MapToOrderData(Order order)
    {
        var orderData = new OrderData
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CustomerName = $"{order.Customer.FirstName} {order.Customer.LastName}",
            RestaurantId = order.RestaurantId,
            RestaurantName = order.Restaurant.Name,
            DeliveryPartnerId = order.DeliveryPartnerId ?? 0,
            DeliveryPartnerName = order.DeliveryPartner != null ? $"{order.DeliveryPartner.FirstName} {order.DeliveryPartner.LastName}" : "",
            Status = order.Status.ToString(),
            DeliveryAddress = order.DeliveryAddress,
            TotalAmount = (double)order.TotalAmount,
            PaymentMethod = order.PaymentMethod.ToString(),
            IsPaid = order.IsPaid,
            CreatedAt = order.CreatedAt.ToString("O")
        };

        orderData.OrderItems.AddRange(order.OrderItems.Select(oi => new OrderItemData
        {
            Id = oi.Id,
            MenuItemId = oi.MenuItemId,
            MenuItemName = oi.MenuItem.Name,
            Quantity = oi.Quantity,
            UnitPrice = (double)oi.UnitPrice,
            TotalPrice = (double)oi.TotalPrice,
            SpecialInstructions = oi.SpecialInstructions ?? ""
        }));

        return orderData;
    }
}