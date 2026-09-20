namespace ProductInventory.Domain.Exceptions;

public class InsufficientStockException : DomainException
{
    public InsufficientStockException(int productId, int currentStock, int requestedQuantity)
        : base($"Product '{productId}' has insufficient stock: requested {requestedQuantity}, available {currentStock}.")
    {
        ProductId = productId;
        CurrentStock = currentStock;
        RequestedQuantity = requestedQuantity;
    }

    public int ProductId { get; }

    public int CurrentStock { get; }

    public int RequestedQuantity { get; }
}
