using Microsoft.AspNetCore.Identity;
using FoodDash.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace FoodDash.Database.Entities;

public class User : IdentityUser<int>
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? ProfilePicture { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Restaurant>? OwnedRestaurants { get; set; }
    public virtual ICollection<Order>? CustomerOrders { get; set; }
    public virtual ICollection<Order>? DeliveredOrders { get; set; }
    public virtual DeliveryPartner? DeliveryPartner { get; set; }
    public virtual Customer? Customer { get; set; }
}