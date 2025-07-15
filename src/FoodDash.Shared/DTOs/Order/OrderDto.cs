using FoodDash.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace FoodDash.Shared.DTOs.Order;

public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    
    [Required]
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    
    [Required]
    public int RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    
    public int? DeliveryPartnerId { get; set; }
    public string? DeliveryPartnerName { get; set; }
    
    public OrderStatus Status { get; set; }
    
    [Required]
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? DeliveryInstructions { get; set; }
    
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    
    public PaymentMethod PaymentMethod { get; set; }
    public bool IsPaid { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? ReadyAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    
    public int EstimatedDeliveryTime { get; set; } // in minutes
    
    public string? CouponCode { get; set; }
    
    public List<OrderItemDto> OrderItems { get; set; } = new();
}

public class OrderItemDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    
    [Required]
    public int MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
    
    [Required]
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    
    public string? SpecialInstructions { get; set; }
}

public class CreateOrderDto
{
    [Required]
    public int RestaurantId { get; set; }
    
    [Required]
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? DeliveryInstructions { get; set; }
    
    [Required]
    public PaymentMethod PaymentMethod { get; set; }
    
    public string? CouponCode { get; set; }
    
    [Required]
    public List<CreateOrderItemDto> OrderItems { get; set; } = new();
}

public class CreateOrderItemDto
{
    [Required]
    public int MenuItemId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
    
    public string? SpecialInstructions { get; set; }
}