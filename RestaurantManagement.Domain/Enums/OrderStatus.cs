namespace RestaurantManagement.Domain.Enums;

public enum OrderStatus
{
    New = 1,
    InKitchen = 2,
    Ready = 3,
    Served = 4,
    Closed = 5,
    Cancelled = 6,
    Held = 7
}
