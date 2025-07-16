using FoodDash.Shared.DTOs.Common;

namespace FoodDash.API.Services;

public interface INotificationService
{
    Task<ApiResponse> SendNotificationAsync(int userId, string message);
    Task<ApiResponse> SendOrderUpdateAsync(int orderId, string status);
}