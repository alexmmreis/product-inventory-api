using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProductInventory.Application.Products.Dtos;
using Xunit;

namespace ProductInventory.Api.IntegrationTests;

[Collection(ProductApiCollection.Name)]
public class SearchAndStockLevelApiTests
{
    private readonly HttpClient _client;

    public SearchAndStockLevelApiTests(ProductApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Search_PartialCaseInsensitiveMatch_ReturnsMatchingProducts()
    {
        await _client.PostAsJsonAsync("/api/products", new CreateProductRequest { Name = "UniqueSearchableWidget", Price = 5m, Stock = 1 });

        var response = await _client.GetAsync("/api/products/search?name=searchablewidget");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var results = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();
        results.Should().Contain(p => p.Name == "UniqueSearchableWidget");
    }

    [Fact]
    public async Task Search_NameTooShort_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/products/search?name=a");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetByStockRange_ValidRange_ReturnsProductsWithinRange()
    {
        await _client.PostAsJsonAsync("/api/products", new CreateProductRequest { Name = "RangeProduct", Price = 5m, Stock = 42 });

        var response = await _client.GetAsync("/api/products/stock-level?min=40&max=45");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var results = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();
        results.Should().OnlyContain(p => p.Stock >= 40 && p.Stock <= 45);
        results.Should().Contain(p => p.Name == "RangeProduct");
    }

    [Fact]
    public async Task GetByStockRange_MinGreaterThanMax_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/products/stock-level?min=50&max=10");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
