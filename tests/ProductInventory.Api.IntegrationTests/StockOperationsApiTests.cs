using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProductInventory.Application.Products.Dtos;
using Xunit;

namespace ProductInventory.Api.IntegrationTests;

[Collection(ProductApiCollection.Name)]
public class StockOperationsApiTests
{
    private readonly HttpClient _client;

    public StockOperationsApiTests(ProductApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<ProductResponse> CreateProductAsync(int stock)
    {
        var request = new CreateProductRequest { Name = "Stock Test Product", Price = 10m, Stock = stock };
        var response = await _client.PostAsJsonAsync("/api/products", request);
        return (await response.Content.ReadFromJsonAsync<ProductResponse>())!;
    }

    [Fact]
    public async Task DecrementStock_SufficientStock_ReturnsOkWithReducedStock()
    {
        var product = await CreateProductAsync(stock: 10);

        var response = await _client.PostAsync($"/api/products/{product.Id}/decrement-stock/4", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<ProductResponse>();
        updated!.Stock.Should().Be(6);
    }

    [Fact]
    public async Task DecrementStock_InsufficientStock_ReturnsConflict()
    {
        var product = await CreateProductAsync(stock: 2);

        var response = await _client.PostAsync($"/api/products/{product.Id}/decrement-stock/10", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DecrementStock_ZeroQuantity_ReturnsBadRequest()
    {
        var product = await CreateProductAsync(stock: 10);

        var response = await _client.PostAsync($"/api/products/{product.Id}/decrement-stock/0", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DecrementStock_NonExistentProduct_ReturnsNotFound()
    {
        var response = await _client.PostAsync("/api/products/999999/decrement-stock/1", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddToStock_ReturnsOkWithIncreasedStock()
    {
        var product = await CreateProductAsync(stock: 5);

        var response = await _client.PostAsync($"/api/products/{product.Id}/add-to-stock/15", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<ProductResponse>();
        updated!.Stock.Should().Be(20);
    }

    [Fact]
    public async Task DecrementStock_ConcurrentRequests_NeverOversells()
    {
        var product = await CreateProductAsync(stock: 100);

        // 20 concurrent requests of 10 units each = 200 requested against 100 available: at most 10 can succeed.
        var tasks = Enumerable.Range(0, 20)
            .Select(_ => _client.PostAsync($"/api/products/{product.Id}/decrement-stock/10", null));
        var responses = await Task.WhenAll(tasks);

        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        successCount.Should().Be(10);

        var final = await _client.GetFromJsonAsync<ProductResponse>($"/api/products/{product.Id}");
        final!.Stock.Should().Be(0);
    }
}
