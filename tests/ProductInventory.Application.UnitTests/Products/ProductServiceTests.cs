using FluentAssertions;
using NSubstitute;
using ProductInventory.Application.Abstractions;
using ProductInventory.Application.Products;
using ProductInventory.Application.Products.Dtos;
using ProductInventory.Domain.Entities;
using ProductInventory.Domain.Exceptions;
using Xunit;

namespace ProductInventory.Application.UnitTests.Products;

public class ProductServiceTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _sut = new ProductService(_repository);
    }

    private static Product CreateProduct(int id = 100001, int stock = 10) => new()
    {
        Id = id,
        Name = "Test Product",
        Description = "A product used for testing",
        Price = 9.99m,
        Stock = stock,
        CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        RowVersion = [1, 2, 3, 4, 5, 6, 7, 8],
    };

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ReturnsMappedResponse()
    {
        var product = CreateProduct();
        _repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        var result = await _sut.GetByIdAsync(product.Id, CancellationToken.None);

        result.Id.Should().Be(product.Id);
        result.Name.Should().Be(product.Name);
        result.Stock.Should().Be(product.Stock);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductDoesNotExist_ThrowsProductNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((Product?)null);

        var act = () => _sut.GetByIdAsync(999999, CancellationToken.None);

        await act.Should().ThrowAsync<ProductNotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_AddsProductAndReturnsResponse()
    {
        var request = new CreateProductRequest { Name = "New Product", Price = 12.5m, Stock = 5 };

        var result = await _sut.CreateAsync(request, CancellationToken.None);

        await _repository.Received(1).AddAsync(Arg.Is<Product>(p => p.Name == request.Name && p.Stock == request.Stock), Arg.Any<CancellationToken>());
        result.Name.Should().Be(request.Name);
        result.Stock.Should().Be(request.Stock);
    }

    [Fact]
    public async Task UpdateAsync_WhenProductExists_AppliesChangesAndPersists()
    {
        var product = CreateProduct();
        _repository.GetTrackedByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        var request = new UpdateProductRequest
        {
            Name = "Updated Name",
            Price = 20m,
            Stock = 30,
            RowVersion = Convert.ToBase64String(product.RowVersion),
        };

        var result = await _sut.UpdateAsync(product.Id, request, CancellationToken.None);

        result.Name.Should().Be("Updated Name");
        result.Stock.Should().Be(30);
        await _repository.Received(1).UpdateAsync(
            product, Arg.Is<byte[]>(rv => rv.SequenceEqual(product.RowVersion)), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WhenProductDoesNotExist_ThrowsProductNotFoundException()
    {
        _repository.GetTrackedByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((Product?)null);
        var request = new UpdateProductRequest { Name = "X", Price = 1, Stock = 1, RowVersion = Convert.ToBase64String([1]) };

        var act = () => _sut.UpdateAsync(999999, request, CancellationToken.None);

        await act.Should().ThrowAsync<ProductNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenProductExists_RemovesProduct()
    {
        var product = CreateProduct();
        _repository.GetTrackedByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        await _sut.DeleteAsync(product.Id, CancellationToken.None);

        await _repository.Received(1).DeleteAsync(product, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_WhenProductDoesNotExist_ThrowsProductNotFoundException()
    {
        _repository.GetTrackedByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((Product?)null);

        var act = () => _sut.DeleteAsync(999999, CancellationToken.None);

        await act.Should().ThrowAsync<ProductNotFoundException>();
    }

    [Fact]
    public async Task DecrementStockAsync_WhenSuccessful_ReturnsUpdatedProduct()
    {
        var product = CreateProduct(stock: 5);
        _repository.TryDecrementStockAsync(product.Id, 3, Arg.Any<CancellationToken>())
            .Returns(StockAdjustmentResult.Success);
        _repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        var result = await _sut.DecrementStockAsync(product.Id, 3, CancellationToken.None);

        result.Id.Should().Be(product.Id);
    }

    [Fact]
    public async Task DecrementStockAsync_WhenInsufficientStock_ThrowsInsufficientStockException()
    {
        var product = CreateProduct(stock: 2);
        _repository.TryDecrementStockAsync(product.Id, 10, Arg.Any<CancellationToken>())
            .Returns(StockAdjustmentResult.InsufficientStock);
        _repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        var act = () => _sut.DecrementStockAsync(product.Id, 10, CancellationToken.None);

        (await act.Should().ThrowAsync<InsufficientStockException>())
            .Which.RequestedQuantity.Should().Be(10);
    }

    [Fact]
    public async Task DecrementStockAsync_WhenProductNotFound_ThrowsProductNotFoundException()
    {
        _repository.TryDecrementStockAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(StockAdjustmentResult.ProductNotFound);

        var act = () => _sut.DecrementStockAsync(999999, 1, CancellationToken.None);

        await act.Should().ThrowAsync<ProductNotFoundException>();
    }

    [Fact]
    public async Task AddToStockAsync_WhenSuccessful_ReturnsUpdatedProduct()
    {
        var product = CreateProduct(stock: 15);
        _repository.TryIncrementStockAsync(product.Id, 5, Arg.Any<CancellationToken>())
            .Returns(StockAdjustmentResult.Success);
        _repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        var result = await _sut.AddToStockAsync(product.Id, 5, CancellationToken.None);

        result.Stock.Should().Be(15);
    }

    [Fact]
    public async Task AddToStockAsync_WhenProductNotFound_ThrowsProductNotFoundException()
    {
        _repository.TryIncrementStockAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(StockAdjustmentResult.ProductNotFound);

        var act = () => _sut.AddToStockAsync(999999, 5, CancellationToken.None);

        await act.Should().ThrowAsync<ProductNotFoundException>();
    }

    [Fact]
    public async Task AddToStockAsync_WhenStockWouldOverflow_ThrowsStockOverflowException()
    {
        var product = CreateProduct(stock: int.MaxValue - 1);
        _repository.TryIncrementStockAsync(product.Id, 10, Arg.Any<CancellationToken>())
            .Returns(StockAdjustmentResult.StockOverflow);
        _repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        var act = () => _sut.AddToStockAsync(product.Id, 10, CancellationToken.None);

        (await act.Should().ThrowAsync<StockOverflowException>())
            .Which.RequestedQuantity.Should().Be(10);
    }

    [Theory]
    [InlineData(0, 0, 1, 20)]
    [InlineData(-5, -5, 1, 20)]
    [InlineData(2, 500, 2, 100)]
    public async Task GetProductsAsync_ClampsPageAndPageSizeToValidRange(
        int requestedPage, int requestedPageSize, int expectedPage, int expectedPageSize)
    {
        _repository.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((new List<Product>(), 0));

        await _sut.GetProductsAsync(requestedPage, requestedPageSize, CancellationToken.None);

        await _repository.Received(1).GetPagedAsync(expectedPage, expectedPageSize, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchAsync_ReturnsMappedResults()
    {
        var products = new List<Product> { CreateProduct(), CreateProduct(100002) };
        _repository.SearchByNameAsync("Test", Arg.Any<CancellationToken>()).Returns(products);

        var result = await _sut.SearchAsync(new SearchProductsQuery { Name = "Test" }, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByStockRangeAsync_ReturnsMappedResults()
    {
        var products = new List<Product> { CreateProduct(stock: 5) };
        _repository.GetByStockRangeAsync(0, 10, Arg.Any<CancellationToken>()).Returns(products);

        var result = await _sut.GetByStockRangeAsync(new StockLevelQuery { Min = 0, Max = 10 }, CancellationToken.None);

        result.Should().ContainSingle();
    }
}
