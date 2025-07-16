namespace FoodDash.Shared.Enums;

public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    Preparing = 3,
    ReadyForPickup = 4,
    OutForDelivery = 5,
    Delivered = 6,
    Cancelled = 7,
    Rejected = 8
}