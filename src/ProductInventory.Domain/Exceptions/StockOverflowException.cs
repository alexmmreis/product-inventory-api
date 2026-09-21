namespace ProductInventory.Domain.Exceptions;

public class StockOverflowException : DomainException
{
    public StockOverflowException(int productId, int currentStock, int requestedQuantity)
        : base($"Product '{productId}' stock cannot be increased by {requestedQuantity}: current stock {currentStock} would exceed the maximum allowed value.")
    {
        ProductId = productId;
        CurrentStock = currentStock;
        RequestedQuantity = requestedQuantity;
    }

    public int ProductId { get; }

    public int CurrentStock { get; }

    public int RequestedQuantity { get; }
}
