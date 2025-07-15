using FoodDash.Shared.DTOs.Common;
using FoodDash.Shared.DTOs.Auth;

namespace FoodDash.API.Services;

public class UserService : IUserService
{
    public async Task<ApiResponse<UserDto>> GetUserByIdAsync(int id)
    {
        await Task.CompletedTask;
        return ApiResponse<UserDto>.ErrorResponse("Not implemented");
    }

    public async Task<ApiResponse<UserDto>> UpdateUserAsync(int id, UserDto user)
    {
        await Task.CompletedTask;
        return ApiResponse<UserDto>.ErrorResponse("Not implemented");
    }
}