using FoodDash.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodDash.Database.Entities;

public class DeliveryPartner
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    public string? VehicleType { get; set; }
    public string? VehicleNumber { get; set; }
    public string? LicenseNumber { get; set; }

    public DeliveryStatus Status { get; set; } = DeliveryStatus.Available;

    [Column(TypeName = "decimal(10,8)")]
    public decimal? CurrentLatitude { get; set; }

    [Column(TypeName = "decimal(11,8)")]
    public decimal? CurrentLongitude { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal Rating { get; set; }

    public int ReviewCount { get; set; }
    public int TotalDeliveries { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalEarnings { get; set; }

    public bool IsAvailable { get; set; } = true;
    public bool IsVerified { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}