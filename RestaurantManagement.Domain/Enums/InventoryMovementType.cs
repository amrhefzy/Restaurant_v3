namespace RestaurantManagement.Domain.Enums;

public enum InventoryMovementType
{
    PurchaseReceipt = 1,
    SaleIssue = 2,
    SalesReturn = 3,
    PurchaseReturn = 4,
    ManualAdjustment = 5,
    Wastage = 6
}
