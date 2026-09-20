using ProductInventory.Application.Common;
using ProductInventory.Application.Products.Dtos;

namespace ProductInventory.Application.Products;

public interface IProductService
{
    Task<PagedResult<ProductResponse>> GetProductsAsync(int page, int pageSize, CancellationToken cancellationToken);

    Task<ProductResponse> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken);

    Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(int id, CancellationToken cancellationToken);

    Task<ProductResponse> DecrementStockAsync(int id, int quantity, CancellationToken cancellationToken);

    Task<ProductResponse> AddToStockAsync(int id, int quantity, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductResponse>> SearchAsync(SearchProductsQuery query, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductResponse>> GetByStockRangeAsync(StockLevelQuery query, CancellationToken cancellationToken);
}
