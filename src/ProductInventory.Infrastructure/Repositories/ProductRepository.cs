using Microsoft.EntityFrameworkCore;
using ProductInventory.Application.Abstractions;
using ProductInventory.Domain.Entities;
using ProductInventory.Domain.Exceptions;
using ProductInventory.Infrastructure.Persistence;

namespace ProductInventory.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _context.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public Task<Product?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _context.Products
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Products.AsNoTracking().OrderBy(p => p.Id);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Product>> SearchByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.Name.Contains(name))
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByStockRangeAsync(int min, int max, CancellationToken cancellationToken)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.Stock >= min && p.Stock <= max)
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Product product, byte[] originalRowVersion, CancellationToken cancellationToken)
    {
        _context.Entry(product).Property(p => p.RowVersion).OriginalValue = originalRowVersion;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ProductConcurrencyConflictException(product.Id);
        }
    }

    public async Task DeleteAsync(Product product, CancellationToken cancellationToken)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<StockAdjustmentResult> TryDecrementStockAsync(int id, int quantity, CancellationToken cancellationToken)
    {
        var rowsAffected = await _context.Products
            .Where(p => p.Id == id && p.Stock >= quantity)
            .ExecuteUpdateAsync(
                s => s.SetProperty(p => p.Stock, p => p.Stock - quantity)
                      .SetProperty(p => p.UpdatedAtUtc, _ => DateTime.UtcNow),
                cancellationToken);

        if (rowsAffected > 0)
        {
            return StockAdjustmentResult.Success;
        }

        var exists = await _context.Products.AsNoTracking().AnyAsync(p => p.Id == id, cancellationToken);
        return exists ? StockAdjustmentResult.InsufficientStock : StockAdjustmentResult.ProductNotFound;
    }

    public async Task<StockAdjustmentResult> TryIncrementStockAsync(int id, int quantity, CancellationToken cancellationToken)
    {
        var rowsAffected = await _context.Products
            .Where(p => p.Id == id && p.Stock <= int.MaxValue - quantity)
            .ExecuteUpdateAsync(
                s => s.SetProperty(p => p.Stock, p => p.Stock + quantity)
                      .SetProperty(p => p.UpdatedAtUtc, _ => DateTime.UtcNow),
                cancellationToken);

        if (rowsAffected > 0)
        {
            return StockAdjustmentResult.Success;
        }

        var existing = await _context.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (existing is null)
        {
            return StockAdjustmentResult.ProductNotFound;
        }

        return existing.Stock <= int.MaxValue - quantity ? StockAdjustmentResult.Success : StockAdjustmentResult.StockOverflow;
    }
}
