using FoodDash.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodDash.Database.Entities;

public class Order
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int RestaurantId { get; set; }

    public int? DeliveryPartnerId { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Required]
    public string DeliveryAddress { get; set; } = string.Empty;

    public string? DeliveryInstructions { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal TaxAmount { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal DeliveryFee { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal DiscountAmount { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalAmount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }
    public bool IsPaid { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? ReadyAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public int EstimatedDeliveryTime { get; set; } // in minutes

    public string? CouponCode { get; set; }

    // Navigation properties
    [ForeignKey(nameof(CustomerId))]
    public virtual User Customer { get; set; } = null!;

    [ForeignKey(nameof(RestaurantId))]
    public virtual Restaurant Restaurant { get; set; } = null!;

    [ForeignKey(nameof(DeliveryPartnerId))]
    public virtual User? DeliveryPartner { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual Payment? Payment { get; set; }
}