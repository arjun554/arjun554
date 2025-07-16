using FoodDash.Shared.DTOs.Common;

namespace FoodDash.API.Services;

public class PaymentService : IPaymentService
{
    public async Task<ApiResponse<string>> ProcessPaymentAsync(int orderId, string paymentMethod)
    {
        await Task.CompletedTask;
        return ApiResponse<string>.ErrorResponse("Not implemented");
    }
}