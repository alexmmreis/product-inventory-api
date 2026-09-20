namespace ProductInventory.Domain.Exceptions;

public class ProductNotFoundException : DomainException
{
    public ProductNotFoundException(int productId)
        : base($"Product with id '{productId}' was not found.")
    {
        ProductId = productId;
    }

    public int ProductId { get; }
}
