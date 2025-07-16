using FoodDash.Shared.DTOs.Auth;
using FoodDash.Shared.DTOs.Common;

namespace FoodDash.API.Services;

public interface IAuthService
{
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request);
    Task<ApiResponse<LoginResponseDto>> RegisterAsync(RegisterRequestDto request);
    Task<ApiResponse> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<ApiResponse> ForgotPasswordAsync(string email);
    Task<ApiResponse> ResetPasswordAsync(string email, string token, string newPassword);
    Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(string refreshToken);
    Task<ApiResponse> LogoutAsync(int userId);
    Task<string> GenerateJwtTokenAsync(int userId);
}