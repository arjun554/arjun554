using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodDash.Database.Entities;

public class Customer
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    public string? PreferredPaymentMethod { get; set; }
    public string? DefaultDeliveryAddress { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal LoyaltyPoints { get; set; }

    public DateTime? LastOrderDate { get; set; }
    public int TotalOrders { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalSpent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}