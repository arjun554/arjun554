using FoodDash.Shared.DTOs.Common;

namespace FoodDash.API.Services;

public interface IPaymentService
{
    Task<ApiResponse<string>> ProcessPaymentAsync(int orderId, string paymentMethod);
}