using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodDash.Database.Entities;

public class Coupon
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string DiscountType { get; set; } = string.Empty; // "Percentage" or "Amount"

    [Column(TypeName = "decimal(10,2)")]
    public decimal DiscountValue { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? MaxDiscountAmount { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal MinimumOrderAmount { get; set; }

    public int? MaxUses { get; set; }
    public int UsedCount { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime ExpiryDate { get; set; }

    public bool IsActive { get; set; } = true;

    public int? RestaurantId { get; set; } // Null for global coupons

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(RestaurantId))]
    public virtual Restaurant? Restaurant { get; set; }
}