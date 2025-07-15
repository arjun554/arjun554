using FoodDash.Shared.DTOs.Common;
using FoodDash.Shared.DTOs.Restaurant;

namespace FoodDash.API.Services;

public interface IRestaurantService
{
    Task<ApiResponse<PagedResult<RestaurantDto>>> GetRestaurantsAsync(int page = 1, int pageSize = 10, string? searchTerm = null);
    Task<ApiResponse<RestaurantDto>> GetRestaurantByIdAsync(int id);
    Task<ApiResponse<RestaurantDto>> CreateRestaurantAsync(RestaurantDto restaurant, int ownerId);
    Task<ApiResponse<RestaurantDto>> UpdateRestaurantAsync(int id, RestaurantDto restaurant, int ownerId);
    Task<ApiResponse> DeleteRestaurantAsync(int id, int ownerId);
    Task<ApiResponse<List<RestaurantDto>>> GetRestaurantsByOwnerAsync(int ownerId);
}