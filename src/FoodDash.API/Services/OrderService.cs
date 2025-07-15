using FoodDash.Shared.DTOs.Common;
using FoodDash.Shared.DTOs.Order;

namespace FoodDash.API.Services;

public class OrderService : IOrderService
{
    public async Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderDto order, int customerId)
    {
        await Task.CompletedTask;
        return ApiResponse<OrderDto>.ErrorResponse("Not implemented");
    }

    public async Task<ApiResponse<OrderDto>> GetOrderByIdAsync(int id)
    {
        await Task.CompletedTask;
        return ApiResponse<OrderDto>.ErrorResponse("Not implemented");
    }

    public async Task<ApiResponse<List<OrderDto>>> GetOrdersByCustomerAsync(int customerId)
    {
        await Task.CompletedTask;
        return ApiResponse<List<OrderDto>>.SuccessResponse(new List<OrderDto>());
    }

    public async Task<ApiResponse<List<OrderDto>>> GetOrdersByRestaurantAsync(int restaurantId)
    {
        await Task.CompletedTask;
        return ApiResponse<List<OrderDto>>.SuccessResponse(new List<OrderDto>());
    }
}