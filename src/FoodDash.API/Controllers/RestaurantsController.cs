using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FoodDash.API.Services;
using FoodDash.Shared.DTOs.Common;
using FoodDash.Shared.DTOs.Restaurant;
using FoodDash.Shared.Enums;

namespace FoodDash.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RestaurantsController : ControllerBase
{
    private readonly IRestaurantService _restaurantService;
    private readonly ILogger<RestaurantsController> _logger;

    public RestaurantsController(IRestaurantService restaurantService, ILogger<RestaurantsController> logger)
    {
        _restaurantService = restaurantService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<RestaurantDto>>>> GetRestaurants(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        try
        {
            var result = await _restaurantService.GetRestaurantsAsync(page, pageSize, searchTerm);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting restaurants");
            return StatusCode(500, ApiResponse<PagedResult<RestaurantDto>>.ErrorResponse("Internal server error"));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<RestaurantDto>>> GetRestaurant(int id)
    {
        try
        {
            var result = await _restaurantService.GetRestaurantByIdAsync(id);
            if (result.Success)
            {
                return Ok(result);
            }
            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting restaurant {Id}", id);
            return StatusCode(500, ApiResponse<RestaurantDto>.ErrorResponse("Internal server error"));
        }
    }

    [HttpPost]
    [Authorize(Roles = "RestaurantOwner")]
    public async Task<ActionResult<ApiResponse<RestaurantDto>>> CreateRestaurant([FromBody] RestaurantDto restaurant)
    {
        try
        {
            var ownerId = GetCurrentUserId();
            var result = await _restaurantService.CreateRestaurantAsync(restaurant, ownerId);
            
            if (result.Success)
            {
                return CreatedAtAction(nameof(GetRestaurant), new { id = result.Data!.Id }, result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating restaurant");
            return StatusCode(500, ApiResponse<RestaurantDto>.ErrorResponse("Internal server error"));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "RestaurantOwner")]
    public async Task<ActionResult<ApiResponse<RestaurantDto>>> UpdateRestaurant(int id, [FromBody] RestaurantDto restaurant)
    {
        try
        {
            var ownerId = GetCurrentUserId();
            var result = await _restaurantService.UpdateRestaurantAsync(id, restaurant, ownerId);
            
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating restaurant {Id}", id);
            return StatusCode(500, ApiResponse<RestaurantDto>.ErrorResponse("Internal server error"));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "RestaurantOwner")]
    public async Task<ActionResult<ApiResponse>> DeleteRestaurant(int id)
    {
        try
        {
            var ownerId = GetCurrentUserId();
            var result = await _restaurantService.DeleteRestaurantAsync(id, ownerId);
            
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting restaurant {Id}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
        }
    }

    [HttpGet("owner/{ownerId}")]
    [Authorize(Roles = "RestaurantOwner,Admin")]
    public async Task<ActionResult<ApiResponse<List<RestaurantDto>>>> GetRestaurantsByOwner(int ownerId)
    {
        try
        {
            // Verify ownership or admin role
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();
            
            if (currentUserRole != UserRole.Admin && currentUserId != ownerId)
            {
                return Forbid();
            }

            var result = await _restaurantService.GetRestaurantsByOwnerAsync(ownerId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting restaurants for owner {OwnerId}", ownerId);
            return StatusCode(500, ApiResponse<List<RestaurantDto>>.ErrorResponse("Internal server error"));
        }
    }

    [HttpGet("my-restaurants")]
    [Authorize(Roles = "RestaurantOwner")]
    public async Task<ActionResult<ApiResponse<List<RestaurantDto>>>> GetMyRestaurants()
    {
        try
        {
            var ownerId = GetCurrentUserId();
            var result = await _restaurantService.GetRestaurantsByOwnerAsync(ownerId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting restaurants for current owner");
            return StatusCode(500, ApiResponse<List<RestaurantDto>>.ErrorResponse("Internal server error"));
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

    private UserRole GetCurrentUserRole()
    {
        var roleClaim = User.FindFirst("Role");
        if (roleClaim == null || !Enum.TryParse<UserRole>(roleClaim.Value, out var role))
        {
            throw new UnauthorizedAccessException("Invalid user role");
        }
        return role;
    }
}