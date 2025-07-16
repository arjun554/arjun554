using FoodDash.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace FoodDash.Shared.DTOs.Restaurant;

public class RestaurantDto
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    public string City { get; set; } = string.Empty;
    
    [Required]
    public string State { get; set; } = string.Empty;
    
    [Required]
    public string ZipCode { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [EmailAddress]
    public string? Email { get; set; }
    
    public string? ImageUrl { get; set; }
    
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    
    public RestaurantStatus Status { get; set; }
    
    public decimal DeliveryFee { get; set; }
    public int EstimatedDeliveryTime { get; set; } // in minutes
    
    public decimal MinimumOrderAmount { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    
    public bool IsOpen { get; set; }
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    
    public string? CuisineType { get; set; }
    
    public int OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}