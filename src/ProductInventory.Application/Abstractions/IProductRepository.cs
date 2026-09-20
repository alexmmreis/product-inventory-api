using ProductInventory.Domain.Entities;

namespace ProductInventory.Application.Abstractions;

public interface IProductRepository
{
    /// <summary>Read-only lookup (no change tracking) for display endpoints.</summary>
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>Tracked lookup for flows that mutate and persist the entity (update/delete).</summary>
    Task<Product?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken);

    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);

    Task<IReadOnlyList<Product>> SearchByNameAsync(string name, CancellationToken cancellationToken);

    Task<IReadOnlyList<Product>> GetByStockRangeAsync(int min, int max, CancellationToken cancellationToken);

    Task AddAsync(Product product, CancellationToken cancellationToken);

    /// <summary>Persists changes to a tracked product, checking <paramref name="originalRowVersion"/> against the current DB value.
    /// Throws ProductConcurrencyConflictException on a mismatch (disconnected optimistic-concurrency pattern).</summary>
    Task UpdateAsync(Product product, byte[] originalRowVersion, CancellationToken cancellationToken);

    Task DeleteAsync(Product product, CancellationToken cancellationToken);

    /// <summary>Atomically decrements stock in a single round trip (no read-modify-write race).</summary>
    Task<StockAdjustmentResult> TryDecrementStockAsync(int id, int quantity, CancellationToken cancellationToken);

    /// <summary>Atomically increments stock in a single round trip (no read-modify-write race).</summary>
    Task<StockAdjustmentResult> TryIncrementStockAsync(int id, int quantity, CancellationToken cancellationToken);
}
