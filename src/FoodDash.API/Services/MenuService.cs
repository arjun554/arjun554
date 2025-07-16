using AutoMapper;
using Microsoft.EntityFrameworkCore;
using FoodDash.Database;
using FoodDash.Database.Entities;
using FoodDash.Shared.DTOs.Common;
using FoodDash.Shared.DTOs.Menu;

namespace FoodDash.API.Services;

public class MenuService : IMenuService
{
    private readonly FoodDashDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<MenuService> _logger;

    public MenuService(FoodDashDbContext context, IMapper mapper, ILogger<MenuService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<List<MenuCategoryDto>>> GetMenuByRestaurantIdAsync(int restaurantId)
    {
        try
        {
            var categories = await _context.MenuCategories
                .Include(c => c.MenuItems.Where(mi => mi.IsAvailable))
                .Where(c => c.RestaurantId == restaurantId && c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            var categoryDtos = _mapper.Map<List<MenuCategoryDto>>(categories);
            return ApiResponse<List<MenuCategoryDto>>.SuccessResponse(categoryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting menu for restaurant {RestaurantId}", restaurantId);
            return ApiResponse<List<MenuCategoryDto>>.ErrorResponse("Failed to get menu");
        }
    }

    public async Task<ApiResponse<MenuItemDto>> GetMenuItemByIdAsync(int id)
    {
        try
        {
            var menuItem = await _context.MenuItems
                .Include(mi => mi.Category)
                .FirstOrDefaultAsync(mi => mi.Id == id);

            if (menuItem == null)
            {
                return ApiResponse<MenuItemDto>.ErrorResponse("Menu item not found");
            }

            var menuItemDto = _mapper.Map<MenuItemDto>(menuItem);
            return ApiResponse<MenuItemDto>.SuccessResponse(menuItemDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting menu item {Id}", id);
            return ApiResponse<MenuItemDto>.ErrorResponse("Failed to get menu item");
        }
    }

    public async Task<ApiResponse<MenuItemDto>> CreateMenuItemAsync(MenuItemDto menuItem, int restaurantId)
    {
        try
        {
            // Verify restaurant ownership
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Id == restaurantId);

            if (restaurant == null)
            {
                return ApiResponse<MenuItemDto>.ErrorResponse("Restaurant not found");
            }

            // Verify category belongs to restaurant
            var category = await _context.MenuCategories
                .FirstOrDefaultAsync(c => c.Id == menuItem.CategoryId && c.RestaurantId == restaurantId);

            if (category == null)
            {
                return ApiResponse<MenuItemDto>.ErrorResponse("Category not found or doesn't belong to restaurant");
            }

            var menuItemEntity = _mapper.Map<MenuItem>(menuItem);
            menuItemEntity.RestaurantId = restaurantId;
            menuItemEntity.CreatedAt = DateTime.UtcNow;
            menuItemEntity.UpdatedAt = DateTime.UtcNow;

            _context.MenuItems.Add(menuItemEntity);
            await _context.SaveChangesAsync();

            // Reload with category info
            await _context.Entry(menuItemEntity)
                .Reference(mi => mi.Category)
                .LoadAsync();

            var result = _mapper.Map<MenuItemDto>(menuItemEntity);
            return ApiResponse<MenuItemDto>.SuccessResponse(result, "Menu item created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating menu item");
            return ApiResponse<MenuItemDto>.ErrorResponse("Failed to create menu item");
        }
    }

    public async Task<ApiResponse<MenuItemDto>> UpdateMenuItemAsync(int id, MenuItemDto menuItem, int restaurantId)
    {
        try
        {
            var existingMenuItem = await _context.MenuItems
                .Include(mi => mi.Category)
                .FirstOrDefaultAsync(mi => mi.Id == id && mi.RestaurantId == restaurantId);

            if (existingMenuItem == null)
            {
                return ApiResponse<MenuItemDto>.ErrorResponse("Menu item not found or access denied");
            }

            // Verify category belongs to restaurant if category is being changed
            if (existingMenuItem.CategoryId != menuItem.CategoryId)
            {
                var category = await _context.MenuCategories
                    .FirstOrDefaultAsync(c => c.Id == menuItem.CategoryId && c.RestaurantId == restaurantId);

                if (category == null)
                {
                    return ApiResponse<MenuItemDto>.ErrorResponse("Category not found or doesn't belong to restaurant");
                }
            }

            _mapper.Map(menuItem, existingMenuItem);
            existingMenuItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = _mapper.Map<MenuItemDto>(existingMenuItem);
            return ApiResponse<MenuItemDto>.SuccessResponse(result, "Menu item updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating menu item {Id}", id);
            return ApiResponse<MenuItemDto>.ErrorResponse("Failed to update menu item");
        }
    }

    public async Task<ApiResponse> DeleteMenuItemAsync(int id, int restaurantId)
    {
        try
        {
            var menuItem = await _context.MenuItems
                .FirstOrDefaultAsync(mi => mi.Id == id && mi.RestaurantId == restaurantId);

            if (menuItem == null)
            {
                return ApiResponse.ErrorResponse("Menu item not found or access denied");
            }

            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResponse("Menu item deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting menu item {Id}", id);
            return ApiResponse.ErrorResponse("Failed to delete menu item");
        }
    }

    public async Task<ApiResponse<MenuCategoryDto>> CreateCategoryAsync(MenuCategoryDto category, int restaurantId)
    {
        try
        {
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Id == restaurantId);

            if (restaurant == null)
            {
                return ApiResponse<MenuCategoryDto>.ErrorResponse("Restaurant not found");
            }

            var categoryEntity = _mapper.Map<MenuCategory>(category);
            categoryEntity.RestaurantId = restaurantId;
            categoryEntity.CreatedAt = DateTime.UtcNow;
            categoryEntity.UpdatedAt = DateTime.UtcNow;

            _context.MenuCategories.Add(categoryEntity);
            await _context.SaveChangesAsync();

            var result = _mapper.Map<MenuCategoryDto>(categoryEntity);
            return ApiResponse<MenuCategoryDto>.SuccessResponse(result, "Category created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating menu category");
            return ApiResponse<MenuCategoryDto>.ErrorResponse("Failed to create category");
        }
    }

    public async Task<ApiResponse<MenuCategoryDto>> UpdateCategoryAsync(int id, MenuCategoryDto category, int restaurantId)
    {
        try
        {
            var existingCategory = await _context.MenuCategories
                .FirstOrDefaultAsync(c => c.Id == id && c.RestaurantId == restaurantId);

            if (existingCategory == null)
            {
                return ApiResponse<MenuCategoryDto>.ErrorResponse("Category not found or access denied");
            }

            _mapper.Map(category, existingCategory);
            existingCategory.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = _mapper.Map<MenuCategoryDto>(existingCategory);
            return ApiResponse<MenuCategoryDto>.SuccessResponse(result, "Category updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating menu category {Id}", id);
            return ApiResponse<MenuCategoryDto>.ErrorResponse("Failed to update category");
        }
    }

    public async Task<ApiResponse> DeleteCategoryAsync(int id, int restaurantId)
    {
        try
        {
            var category = await _context.MenuCategories
                .Include(c => c.MenuItems)
                .FirstOrDefaultAsync(c => c.Id == id && c.RestaurantId == restaurantId);

            if (category == null)
            {
                return ApiResponse.ErrorResponse("Category not found or access denied");
            }

            if (category.MenuItems.Any())
            {
                return ApiResponse.ErrorResponse("Cannot delete category with menu items. Please delete or move the menu items first.");
            }

            _context.MenuCategories.Remove(category);
            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResponse("Category deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting menu category {Id}", id);
            return ApiResponse.ErrorResponse("Failed to delete category");
        }
    }
}