using FoodDash.Shared.DTOs.Common;
using FoodDash.Shared.DTOs.Auth;

namespace FoodDash.API.Services;

public interface IUserService
{
    Task<ApiResponse<UserDto>> GetUserByIdAsync(int id);
    Task<ApiResponse<UserDto>> UpdateUserAsync(int id, UserDto user);
}