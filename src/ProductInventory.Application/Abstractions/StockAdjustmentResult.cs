namespace ProductInventory.Application.Abstractions;

public enum StockAdjustmentResult
{
    Success,
    ProductNotFound,
    InsufficientStock,
    StockOverflow,
}
