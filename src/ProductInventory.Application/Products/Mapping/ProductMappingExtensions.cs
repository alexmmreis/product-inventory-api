using ProductInventory.Application.Products.Dtos;
using ProductInventory.Domain.Entities;

namespace ProductInventory.Application.Products.Mapping;

public static class ProductMappingExtensions
{
    public static ProductResponse ToResponse(this Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CreatedAtUtc = product.CreatedAtUtc,
            UpdatedAtUtc = product.UpdatedAtUtc,
            RowVersion = Convert.ToBase64String(product.RowVersion),
        };
    }

    public static Product ToEntity(this CreateProductRequest request)
    {
        return new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    /// <summary>Applies request values onto a tracked entity in place (preserves Id/CreatedAtUtc).</summary>
    public static void ApplyUpdate(this Product product, UpdateProductRequest request)
    {
        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.UpdatedAtUtc = DateTime.UtcNow;
    }
}
