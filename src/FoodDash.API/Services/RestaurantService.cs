using AutoMapper;
using Microsoft.EntityFrameworkCore;
using FoodDash.Database;
using FoodDash.Database.Entities;
using FoodDash.Shared.DTOs.Common;
using FoodDash.Shared.DTOs.Restaurant;
using FoodDash.Shared.Enums;

namespace FoodDash.API.Services;

public class RestaurantService : IRestaurantService
{
    private readonly FoodDashDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<RestaurantService> _logger;

    public RestaurantService(FoodDashDbContext context, IMapper mapper, ILogger<RestaurantService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<RestaurantDto>>> GetRestaurantsAsync(int page = 1, int pageSize = 10, string? searchTerm = null)
    {
        try
        {
            var query = _context.Restaurants
                .Where(r => r.Status == RestaurantStatus.Approved)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(r => r.Name.Contains(searchTerm) || r.CuisineType!.Contains(searchTerm));
            }

            var totalItems = await query.CountAsync();
            var restaurants = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var restaurantDtos = _mapper.Map<List<RestaurantDto>>(restaurants);

            var result = new PagedResult<RestaurantDto>
            {
                Items = restaurantDtos,
                TotalItems = totalItems,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                HasNextPage = page * pageSize < totalItems,
                HasPreviousPage = page > 1
            };

            return ApiResponse<PagedResult<RestaurantDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting restaurants");
            return ApiResponse<PagedResult<RestaurantDto>>.ErrorResponse("Failed to get restaurants");
        }
    }

    public async Task<ApiResponse<RestaurantDto>> GetRestaurantByIdAsync(int id)
    {
        try
        {
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
            {
                return ApiResponse<RestaurantDto>.ErrorResponse("Restaurant not found");
            }

            var restaurantDto = _mapper.Map<RestaurantDto>(restaurant);
            return ApiResponse<RestaurantDto>.SuccessResponse(restaurantDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting restaurant {Id}", id);
            return ApiResponse<RestaurantDto>.ErrorResponse("Failed to get restaurant");
        }
    }

    public async Task<ApiResponse<RestaurantDto>> CreateRestaurantAsync(RestaurantDto restaurant, int ownerId)
    {
        try
        {
            var restaurantEntity = _mapper.Map<Restaurant>(restaurant);
            restaurantEntity.OwnerId = ownerId;
            restaurantEntity.Status = RestaurantStatus.Pending;
            restaurantEntity.CreatedAt = DateTime.UtcNow;
            restaurantEntity.UpdatedAt = DateTime.UtcNow;

            _context.Restaurants.Add(restaurantEntity);
            await _context.SaveChangesAsync();

            var result = _mapper.Map<RestaurantDto>(restaurantEntity);
            return ApiResponse<RestaurantDto>.SuccessResponse(result, "Restaurant created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating restaurant");
            return ApiResponse<RestaurantDto>.ErrorResponse("Failed to create restaurant");
        }
    }

    public async Task<ApiResponse<RestaurantDto>> UpdateRestaurantAsync(int id, RestaurantDto restaurant, int ownerId)
    {
        try
        {
            var existingRestaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Id == id && r.OwnerId == ownerId);

            if (existingRestaurant == null)
            {
                return ApiResponse<RestaurantDto>.ErrorResponse("Restaurant not found or access denied");
            }

            _mapper.Map(restaurant, existingRestaurant);
            existingRestaurant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = _mapper.Map<RestaurantDto>(existingRestaurant);
            return ApiResponse<RestaurantDto>.SuccessResponse(result, "Restaurant updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating restaurant {Id}", id);
            return ApiResponse<RestaurantDto>.ErrorResponse("Failed to update restaurant");
        }
    }

    public async Task<ApiResponse> DeleteRestaurantAsync(int id, int ownerId)
    {
        try
        {
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Id == id && r.OwnerId == ownerId);

            if (restaurant == null)
            {
                return ApiResponse.ErrorResponse("Restaurant not found or access denied");
            }

            _context.Restaurants.Remove(restaurant);
            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResponse("Restaurant deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting restaurant {Id}", id);
            return ApiResponse.ErrorResponse("Failed to delete restaurant");
        }
    }

    public async Task<ApiResponse<List<RestaurantDto>>> GetRestaurantsByOwnerAsync(int ownerId)
    {
        try
        {
            var restaurants = await _context.Restaurants
                .Where(r => r.OwnerId == ownerId)
                .ToListAsync();

            var restaurantDtos = _mapper.Map<List<RestaurantDto>>(restaurants);
            return ApiResponse<List<RestaurantDto>>.SuccessResponse(restaurantDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting restaurants for owner {OwnerId}", ownerId);
            return ApiResponse<List<RestaurantDto>>.ErrorResponse("Failed to get restaurants");
        }
    }
}