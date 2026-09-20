namespace ProductInventory.Domain.Exceptions;

/// <summary>Raised when an update targets a product that was concurrently modified (RowVersion mismatch).</summary>
public class ProductConcurrencyConflictException : DomainException
{
    public ProductConcurrencyConflictException(int productId)
        : base($"Product '{productId}' was modified by another request. Reload and try again.")
    {
        ProductId = productId;
    }

    public int ProductId { get; }
}
