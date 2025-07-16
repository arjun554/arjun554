using FoodDash.Database.Entities;
using FoodDash.Shared.Enums;

namespace FoodDash.OrderService.Services;

public interface IOrderBusinessService
{
    Task<OrderTotals> CalculateOrderTotalsAsync(decimal subTotal, decimal deliveryFee, string? couponCode, int restaurantId);
    Task<string> GenerateOrderNumberAsync();
    Task<bool> CanUpdateOrderStatusAsync(Order order, int userId, UserRole userRole, OrderStatus newStatus);
    Task UpdateOrderStatusAsync(Order order, OrderStatus newStatus);
    Task<(bool Success, string Message)> AssignDeliveryPartnerAsync(int orderId, int deliveryPartnerId);
}

public record OrderTotals(
    decimal SubTotal,
    decimal TaxAmount,
    decimal DeliveryFee,
    decimal DiscountAmount,
    decimal TotalAmount);