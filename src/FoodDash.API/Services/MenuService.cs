using FoodDash.Shared.DTOs.Common;
using FoodDash.Shared.DTOs.Menu;

namespace FoodDash.API.Services;

public class MenuService : IMenuService
{
    public async Task<ApiResponse<List<MenuCategoryDto>>> GetMenuByRestaurantIdAsync(int restaurantId)
    {
        await Task.CompletedTask;
        return ApiResponse<List<MenuCategoryDto>>.SuccessResponse(new List<MenuCategoryDto>());
    }

    public async Task<ApiResponse<MenuItemDto>> GetMenuItemByIdAsync(int id)
    {
        await Task.CompletedTask;
        return ApiResponse<MenuItemDto>.ErrorResponse("Not implemented");
    }

    public async Task<ApiResponse<MenuItemDto>> CreateMenuItemAsync(MenuItemDto menuItem, int restaurantId)
    {
        await Task.CompletedTask;
        return ApiResponse<MenuItemDto>.ErrorResponse("Not implemented");
    }

    public async Task<ApiResponse<MenuItemDto>> UpdateMenuItemAsync(int id, MenuItemDto menuItem, int restaurantId)
    {
        await Task.CompletedTask;
        return ApiResponse<MenuItemDto>.ErrorResponse("Not implemented");
    }

    public async Task<ApiResponse> DeleteMenuItemAsync(int id, int restaurantId)
    {
        await Task.CompletedTask;
        return ApiResponse.ErrorResponse("Not implemented");
    }
}