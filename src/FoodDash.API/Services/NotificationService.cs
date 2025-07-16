using FoodDash.Shared.DTOs.Common;

namespace FoodDash.API.Services;

public class NotificationService : INotificationService
{
    public async Task<ApiResponse> SendNotificationAsync(int userId, string message)
    {
        await Task.CompletedTask;
        return ApiResponse.ErrorResponse("Not implemented");
    }

    public async Task<ApiResponse> SendOrderUpdateAsync(int orderId, string status)
    {
        await Task.CompletedTask;
        return ApiResponse.ErrorResponse("Not implemented");
    }
}