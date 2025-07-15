using FoodDash.Shared.DTOs.Common;
using FoodDash.Shared.DTOs.Menu;

namespace FoodDash.API.Services;

public interface IMenuService
{
    Task<ApiResponse<List<MenuCategoryDto>>> GetMenuByRestaurantIdAsync(int restaurantId);
    Task<ApiResponse<MenuItemDto>> GetMenuItemByIdAsync(int id);
    Task<ApiResponse<MenuItemDto>> CreateMenuItemAsync(MenuItemDto menuItem, int restaurantId);
    Task<ApiResponse<MenuItemDto>> UpdateMenuItemAsync(int id, MenuItemDto menuItem, int restaurantId);
    Task<ApiResponse> DeleteMenuItemAsync(int id, int restaurantId);
}