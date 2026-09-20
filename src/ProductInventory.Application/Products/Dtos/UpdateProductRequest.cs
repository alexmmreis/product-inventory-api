namespace ProductInventory.Application.Products.Dtos;

public class UpdateProductRequest
{
    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public decimal Price { get; init; }

    public int Stock { get; init; }

    /// <summary>Opaque concurrency token (base64) from a previous ProductResponse; required to detect conflicting updates.</summary>
    public string RowVersion { get; init; } = string.Empty;
}
