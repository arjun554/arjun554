using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using FoodDash.Database;
using FoodDash.Database.Entities;
using FoodDash.Shared.Protos;
using FoodDash.Shared.Enums;

namespace FoodDash.RestaurantService.Services;

public class RestaurantGrpcService : Restaurant.RestaurantServiceBase
{
    private readonly FoodDashDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<RestaurantGrpcService> _logger;

    public RestaurantGrpcService(
        FoodDashDbContext context,
        IMapper mapper,
        ILogger<RestaurantGrpcService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public override async Task<GetRestaurantsResponse> GetRestaurants(GetRestaurantsRequest request, ServerCallContext context)
    {
        try
        {
            var query = _context.Restaurants.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(r => r.Name.Contains(request.SearchTerm) || r.Description.Contains(request.SearchTerm));
            }

            var totalCount = await query.CountAsync();
            var restaurants = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var response = new GetRestaurantsResponse
            {
                Success = true,
                Message = "Restaurants retrieved successfully",
                Data = new PagedRestaurantResult
                {
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
                }
            };

            foreach (var restaurant in restaurants)
            {
                response.Data.Items.Add(MapToRestaurantDto(restaurant));
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting restaurants");
            return new GetRestaurantsResponse
            {
                Success = false,
                Message = "An error occurred while retrieving restaurants"
            };
        }
    }

    public override async Task<GetRestaurantByIdResponse> GetRestaurantById(GetRestaurantByIdRequest request, ServerCallContext context)
    {
        try
        {
            var restaurant = await _context.Restaurants.FindAsync(request.RestaurantId);
            if (restaurant == null)
            {
                return new GetRestaurantByIdResponse
                {
                    Success = false,
                    Message = "Restaurant not found"
                };
            }

            return new GetRestaurantByIdResponse
            {
                Success = true,
                Message = "Restaurant retrieved successfully",
                Data = MapToRestaurantDto(restaurant)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting restaurant {RestaurantId}", request.RestaurantId);
            return new GetRestaurantByIdResponse
            {
                Success = false,
                Message = "An error occurred while retrieving restaurant"
            };
        }
    }

    public override async Task<CreateRestaurantResponse> CreateRestaurant(CreateRestaurantRequest request, ServerCallContext context)
    {
        try
        {
            var restaurant = new Database.Entities.Restaurant
            {
                Name = request.Name,
                Description = request.Description,
                Address = request.Address,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                OpeningHours = request.OpeningHours,
                ImageUrl = request.ImageUrl,
                IsActive = request.IsActive,
                DeliveryFee = (decimal)request.DeliveryFee,
                MinimumOrderAmount = request.MinimumOrderAmount,
                EstimatedDeliveryTime = request.EstimatedDeliveryTime,
                OwnerId = int.Parse(request.OwnerId),
                CreatedAt = DateTime.UtcNow
            };

            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            return new CreateRestaurantResponse
            {
                Success = true,
                Message = "Restaurant created successfully",
                Data = MapToRestaurantDto(restaurant)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating restaurant");
            return new CreateRestaurantResponse
            {
                Success = false,
                Message = "An error occurred while creating restaurant"
            };
        }
    }

    public override async Task<UpdateRestaurantResponse> UpdateRestaurant(UpdateRestaurantRequest request, ServerCallContext context)
    {
        try
        {
            var restaurant = await _context.Restaurants.FindAsync(request.RestaurantId);
            if (restaurant == null)
            {
                return new UpdateRestaurantResponse
                {
                    Success = false,
                    Message = "Restaurant not found"
                };
            }

            // Check ownership
            if (restaurant.OwnerId != int.Parse(request.OwnerId))
            {
                return new UpdateRestaurantResponse
                {
                    Success = false,
                    Message = "Access denied"
                };
            }

            restaurant.Name = request.Name;
            restaurant.Description = request.Description;
            restaurant.Address = request.Address;
            restaurant.PhoneNumber = request.PhoneNumber;
            restaurant.Email = request.Email;
            restaurant.OpeningHours = request.OpeningHours;
            restaurant.ImageUrl = request.ImageUrl;
            restaurant.IsActive = request.IsActive;
            restaurant.DeliveryFee = (decimal)request.DeliveryFee;
            restaurant.MinimumOrderAmount = request.MinimumOrderAmount;
            restaurant.EstimatedDeliveryTime = request.EstimatedDeliveryTime;
            restaurant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new UpdateRestaurantResponse
            {
                Success = true,
                Message = "Restaurant updated successfully",
                Data = MapToRestaurantDto(restaurant)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating restaurant {RestaurantId}", request.RestaurantId);
            return new UpdateRestaurantResponse
            {
                Success = false,
                Message = "An error occurred while updating restaurant"
            };
        }
    }

    public override async Task<DeleteRestaurantResponse> DeleteRestaurant(DeleteRestaurantRequest request, ServerCallContext context)
    {
        try
        {
            var restaurant = await _context.Restaurants.FindAsync(request.RestaurantId);
            if (restaurant == null)
            {
                return new DeleteRestaurantResponse
                {
                    Success = false,
                    Message = "Restaurant not found"
                };
            }

            // Check ownership
            if (restaurant.OwnerId != int.Parse(request.OwnerId))
            {
                return new DeleteRestaurantResponse
                {
                    Success = false,
                    Message = "Access denied"
                };
            }

            _context.Restaurants.Remove(restaurant);
            await _context.SaveChangesAsync();

            return new DeleteRestaurantResponse
            {
                Success = true,
                Message = "Restaurant deleted successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting restaurant {RestaurantId}", request.RestaurantId);
            return new DeleteRestaurantResponse
            {
                Success = false,
                Message = "An error occurred while deleting restaurant"
            };
        }
    }

    public override async Task<GetMyRestaurantResponse> GetMyRestaurant(GetMyRestaurantRequest request, ServerCallContext context)
    {
        try
        {
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.OwnerId == int.Parse(request.OwnerId));

            if (restaurant == null)
            {
                return new GetMyRestaurantResponse
                {
                    Success = false,
                    Message = "Restaurant not found"
                };
            }

            return new GetMyRestaurantResponse
            {
                Success = true,
                Message = "Restaurant retrieved successfully",
                Data = MapToRestaurantDto(restaurant)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting restaurant for owner {OwnerId}", request.OwnerId);
            return new GetMyRestaurantResponse
            {
                Success = false,
                Message = "An error occurred while retrieving restaurant"
            };
        }
    }

    public override async Task<UpdateRestaurantStatusResponse> UpdateRestaurantStatus(UpdateRestaurantStatusRequest request, ServerCallContext context)
    {
        try
        {
            var restaurant = await _context.Restaurants.FindAsync(request.RestaurantId);
            if (restaurant == null)
            {
                return new UpdateRestaurantStatusResponse
                {
                    Success = false,
                    Message = "Restaurant not found"
                };
            }

            // Check ownership
            if (restaurant.OwnerId != int.Parse(request.OwnerId))
            {
                return new UpdateRestaurantStatusResponse
                {
                    Success = false,
                    Message = "Access denied"
                };
            }

            restaurant.IsActive = request.IsActive;
            restaurant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new UpdateRestaurantStatusResponse
            {
                Success = true,
                Message = "Restaurant status updated successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating restaurant status {RestaurantId}", request.RestaurantId);
            return new UpdateRestaurantStatusResponse
            {
                Success = false,
                Message = "An error occurred while updating restaurant status"
            };
        }
    }

    private static RestaurantDto MapToRestaurantDto(Database.Entities.Restaurant restaurant)
    {
        return new RestaurantDto
        {
            Id = restaurant.Id,
            Name = restaurant.Name,
            Description = restaurant.Description ?? "",
            Address = restaurant.Address ?? "",
            PhoneNumber = restaurant.PhoneNumber ?? "",
            Email = restaurant.Email ?? "",
            OpeningHours = restaurant.OpeningHours ?? "",
            ImageUrl = restaurant.ImageUrl ?? "",
            IsActive = restaurant.IsActive,
            DeliveryFee = (double)restaurant.DeliveryFee,
            MinimumOrderAmount = restaurant.MinimumOrderAmount,
            EstimatedDeliveryTime = restaurant.EstimatedDeliveryTime,
            Rating = (double)restaurant.Rating,
            TotalReviews = restaurant.TotalReviews,
            OwnerId = restaurant.OwnerId.ToString(),
            CreatedAt = ((DateTimeOffset)restaurant.CreatedAt).ToUnixTimeSeconds(),
            UpdatedAt = restaurant.UpdatedAt.HasValue ? ((DateTimeOffset)restaurant.UpdatedAt.Value).ToUnixTimeSeconds() : 0
        };
    }
}