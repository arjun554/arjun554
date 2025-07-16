using FoodDash.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodDash.Database.Entities;

public class Restaurant
{
    [Key]
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
    public string PhoneNumber { get; set; } = string.Empty;

    public string? Email { get; set; }
    public string? ImageUrl { get; set; }

    [Column(TypeName = "decimal(10,8)")]
    public decimal Latitude { get; set; }

    [Column(TypeName = "decimal(11,8)")]
    public decimal Longitude { get; set; }

    public RestaurantStatus Status { get; set; } = RestaurantStatus.Pending;

    [Column(TypeName = "decimal(10,2)")]
    public decimal DeliveryFee { get; set; }

    public int EstimatedDeliveryTime { get; set; } // in minutes

    [Column(TypeName = "decimal(10,2)")]
    public decimal MinimumOrderAmount { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal Rating { get; set; }

    public int ReviewCount { get; set; }

    public bool IsOpen { get; set; } = true;
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }

    public string? CuisineType { get; set; }

    [Required]
    public int OwnerId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(OwnerId))]
    public virtual User Owner { get; set; } = null!;

    public virtual ICollection<MenuCategory> MenuCategories { get; set; } = new List<MenuCategory>();
    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}