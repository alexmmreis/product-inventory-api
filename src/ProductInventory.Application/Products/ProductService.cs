using ProductInventory.Application.Abstractions;
using ProductInventory.Application.Common;
using ProductInventory.Application.Products.Dtos;
using ProductInventory.Application.Products.Mapping;
using ProductInventory.Domain.Exceptions;

namespace ProductInventory.Application.Products;

public class ProductService : IProductService
{
    private const int MaxPageSize = 100;

    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<ProductResponse>> GetProductsAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, MaxPageSize);

        var (items, totalCount) = await _repository.GetPagedAsync(page, pageSize, cancellationToken);

        return new PagedResult<ProductResponse>
        {
            Items = items.Select(p => p.ToResponse()).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<ProductResponse> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new ProductNotFoundException(id);

        return product.ToResponse();
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = request.ToEntity();

        await _repository.AddAsync(product, cancellationToken);

        return product.ToResponse();
    }

    public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetTrackedByIdAsync(id, cancellationToken)
            ?? throw new ProductNotFoundException(id);

        product.ApplyUpdate(request);

        var originalRowVersion = Convert.FromBase64String(request.RowVersion);
        await _repository.UpdateAsync(product, originalRowVersion, cancellationToken);

        return product.ToResponse();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var product = await _repository.GetTrackedByIdAsync(id, cancellationToken)
            ?? throw new ProductNotFoundException(id);

        await _repository.DeleteAsync(product, cancellationToken);
    }

    public async Task<ProductResponse> DecrementStockAsync(int id, int quantity, CancellationToken cancellationToken)
    {
        var result = await _repository.TryDecrementStockAsync(id, quantity, cancellationToken);
        return await HandleStockAdjustmentResultAsync(id, quantity, result, cancellationToken);
    }

    public async Task<ProductResponse> AddToStockAsync(int id, int quantity, CancellationToken cancellationToken)
    {
        var result = await _repository.TryIncrementStockAsync(id, quantity, cancellationToken);
        return await HandleStockAdjustmentResultAsync(id, quantity, result, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductResponse>> SearchAsync(SearchProductsQuery query, CancellationToken cancellationToken)
    {
        var products = await _repository.SearchByNameAsync(query.Name, cancellationToken);
        return products.Select(p => p.ToResponse()).ToList();
    }

    public async Task<IReadOnlyList<ProductResponse>> GetByStockRangeAsync(StockLevelQuery query, CancellationToken cancellationToken)
    {
        var products = await _repository.GetByStockRangeAsync(query.Min, query.Max, cancellationToken);
        return products.Select(p => p.ToResponse()).ToList();
    }

    private async Task<ProductResponse> HandleStockAdjustmentResultAsync(
        int id, int quantity, StockAdjustmentResult result, CancellationToken cancellationToken)
    {
        if (result == StockAdjustmentResult.ProductNotFound)
        {
            throw new ProductNotFoundException(id);
        }

        if (result == StockAdjustmentResult.InsufficientStock)
        {
            var current = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new ProductNotFoundException(id);

            throw new InsufficientStockException(id, current.Stock, quantity);
        }

        if (result == StockAdjustmentResult.StockOverflow)
        {
            var current = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new ProductNotFoundException(id);

            throw new StockOverflowException(id, current.Stock, quantity);
        }

        var updated = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new ProductNotFoundException(id);

        return updated.ToResponse();
    }
}
