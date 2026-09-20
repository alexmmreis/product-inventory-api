namespace ProductInventory.Application.Products.Dtos;

public class ProductResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public decimal Price { get; init; }

    public int Stock { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }

    /// <summary>Opaque concurrency token (base64); pass back unchanged on PUT to detect conflicting updates.</summary>
    public string RowVersion { get; init; } = string.Empty;
}
