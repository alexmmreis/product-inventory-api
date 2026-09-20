using System.Net.Http.Json;
using FluentAssertions;
using ProductInventory.Application.Products.Dtos;
using Xunit;

namespace ProductInventory.Api.IntegrationTests;

[Collection(ProductApiCollection.Name)]
public class ProductIdUniquenessApiTests
{
    private readonly HttpClient _client;

    public ProductIdUniquenessApiTests(ProductApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateProducts_Concurrently_AllGeneratedIdsAreUniqueAndSixDigits()
    {
        var tasks = Enumerable.Range(0, 30).Select(i => _client.PostAsJsonAsync(
            "/api/products",
            new CreateProductRequest { Name = $"Concurrent Product {i}", Price = 1m, Stock = 0 }));

        var responses = await Task.WhenAll(tasks);
        var products = await Task.WhenAll(responses.Select(r => r.Content.ReadFromJsonAsync<ProductResponse>()));
        var ids = products.Select(p => p!.Id).ToList();

        ids.Should().OnlyHaveUniqueItems();
        ids.Should().OnlyContain(id => id >= 100000 && id <= 999999);
    }
}
