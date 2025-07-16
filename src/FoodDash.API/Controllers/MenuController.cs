using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FoodDash.API.Services;
using FoodDash.Shared.DTOs.Common;
using FoodDash.Shared.DTOs.Menu;
using FoodDash.Shared.Enums;

namespace FoodDash.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;
    private readonly IRestaurantService _restaurantService;
    private readonly ILogger<MenuController> _logger;

    public MenuController(
        IMenuService menuService, 
        IRestaurantService restaurantService,
        ILogger<MenuController> logger)
    {
        _menuService = menuService;
        _restaurantService = restaurantService;
        _logger = logger;
    }

    [HttpGet("restaurant/{restaurantId}")]
    public async Task<ActionResult<ApiResponse<List<MenuCategoryDto>>>> GetMenuByRestaurant(int restaurantId)
    {
        try
        {
            var result = await _menuService.GetMenuByRestaurantIdAsync(restaurantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting menu for restaurant {RestaurantId}", restaurantId);
            return StatusCode(500, ApiResponse<List<MenuCategoryDto>>.ErrorResponse("Internal server error"));
        }
    }

    [HttpGet("item/{id}")]
    public async Task<ActionResult<ApiResponse<MenuItemDto>>> GetMenuItem(int id)
    {
        try
        {
            var result = await _menuService.GetMenuItemByIdAsync(id);
            if (result.Success)
            {
                return Ok(result);
            }
            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting menu item {Id}", id);
            return StatusCode(500, ApiResponse<MenuItemDto>.ErrorResponse("Internal server error"));
        }
    }

    // Category Management
    [HttpPost("category")]
    [Authorize(Roles = "RestaurantOwner")]
    public async Task<ActionResult<ApiResponse<MenuCategoryDto>>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Verify restaurant ownership
            if (!await IsRestaurantOwner(request.RestaurantId, userId))
            {
                return Forbid();
            }

            var result = await _menuService.CreateCategoryAsync(request.Category, request.RestaurantId);
            
            if (result.Success)
            {
                return CreatedAtAction(nameof(GetMenuByRestaurant), 
                    new { restaurantId = request.RestaurantId }, result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating menu category");
            return StatusCode(500, ApiResponse<MenuCategoryDto>.ErrorResponse("Internal server error"));
        }
    }

    [HttpPut("category/{id}")]
    [Authorize(Roles = "RestaurantOwner")]
    public async Task<ActionResult<ApiResponse<MenuCategoryDto>>> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Verify restaurant ownership
            if (!await IsRestaurantOwner(request.RestaurantId, userId))
            {
                return Forbid();
            }

            var result = await _menuService.UpdateCategoryAsync(id, request.Category, request.RestaurantId);
            
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating menu category {Id}", id);
            return StatusCode(500, ApiResponse<MenuCategoryDto>.ErrorResponse("Internal server error"));
        }
    }

    [HttpDelete("category/{id}")]
    [Authorize(Roles = "RestaurantOwner")]
    public async Task<ActionResult<ApiResponse>> DeleteCategory(int id, [FromQuery] int restaurantId)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Verify restaurant ownership
            if (!await IsRestaurantOwner(restaurantId, userId))
            {
                return Forbid();
            }

            var result = await _menuService.DeleteCategoryAsync(id, restaurantId);
            
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting menu category {Id}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
        }
    }

    // Menu Item Management
    [HttpPost("item")]
    [Authorize(Roles = "RestaurantOwner")]
    public async Task<ActionResult<ApiResponse<MenuItemDto>>> CreateMenuItem([FromBody] CreateMenuItemRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Verify restaurant ownership
            if (!await IsRestaurantOwner(request.RestaurantId, userId))
            {
                return Forbid();
            }

            var result = await _menuService.CreateMenuItemAsync(request.MenuItem, request.RestaurantId);
            
            if (result.Success)
            {
                return CreatedAtAction(nameof(GetMenuItem), new { id = result.Data!.Id }, result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating menu item");
            return StatusCode(500, ApiResponse<MenuItemDto>.ErrorResponse("Internal server error"));
        }
    }

    [HttpPut("item/{id}")]
    [Authorize(Roles = "RestaurantOwner")]
    public async Task<ActionResult<ApiResponse<MenuItemDto>>> UpdateMenuItem(int id, [FromBody] UpdateMenuItemRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Verify restaurant ownership
            if (!await IsRestaurantOwner(request.RestaurantId, userId))
            {
                return Forbid();
            }

            var result = await _menuService.UpdateMenuItemAsync(id, request.MenuItem, request.RestaurantId);
            
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating menu item {Id}", id);
            return StatusCode(500, ApiResponse<MenuItemDto>.ErrorResponse("Internal server error"));
        }
    }

    [HttpDelete("item/{id}")]
    [Authorize(Roles = "RestaurantOwner")]
    public async Task<ActionResult<ApiResponse>> DeleteMenuItem(int id, [FromQuery] int restaurantId)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Verify restaurant ownership
            if (!await IsRestaurantOwner(restaurantId, userId))
            {
                return Forbid();
            }

            var result = await _menuService.DeleteMenuItemAsync(id, restaurantId);
            
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting menu item {Id}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user token");
        }
        return userId;
    }

    private async Task<bool> IsRestaurantOwner(int restaurantId, int userId)
    {
        var restaurant = await _restaurantService.GetRestaurantByIdAsync(restaurantId);
        return restaurant.Success && restaurant.Data?.OwnerId == userId;
    }
}

// Request DTOs
public class CreateCategoryRequest
{
    public int RestaurantId { get; set; }
    public MenuCategoryDto Category { get; set; } = new();
}

public class UpdateCategoryRequest
{
    public int RestaurantId { get; set; }
    public MenuCategoryDto Category { get; set; } = new();
}

public class CreateMenuItemRequest
{
    public int RestaurantId { get; set; }
    public MenuItemDto MenuItem { get; set; } = new();
}

public class UpdateMenuItemRequest
{
    public int RestaurantId { get; set; }
    public MenuItemDto MenuItem { get; set; } = new();
}