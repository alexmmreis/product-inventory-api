namespace ProductInventory.Application.Products.Dtos;

public class CreateProductRequest
{
    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public decimal Price { get; init; }

    public int Stock { get; init; }
}
