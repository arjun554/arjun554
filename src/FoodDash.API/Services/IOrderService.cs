using FoodDash.Shared.DTOs.Common;
using FoodDash.Shared.DTOs.Order;

namespace FoodDash.API.Services;

public interface IOrderService
{
    Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderDto order, int customerId);
    Task<ApiResponse<OrderDto>> GetOrderByIdAsync(int id);
    Task<ApiResponse<List<OrderDto>>> GetOrdersByCustomerAsync(int customerId);
    Task<ApiResponse<List<OrderDto>>> GetOrdersByRestaurantAsync(int restaurantId);
}