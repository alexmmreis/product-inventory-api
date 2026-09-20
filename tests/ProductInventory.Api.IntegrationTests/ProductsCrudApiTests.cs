using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProductInventory.Application.Common;
using ProductInventory.Application.Products.Dtos;
using Xunit;

namespace ProductInventory.Api.IntegrationTests;

[Collection(ProductApiCollection.Name)]
public class ProductsCrudApiTests
{
    private readonly HttpClient _client;

    public ProductsCrudApiTests(ProductApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static CreateProductRequest ValidCreateRequest(string name = "Integration Test Product") => new()
    {
        Name = name,
        Description = "Created by an integration test",
        Price = 42.50m,
        Stock = 25,
    };

    [Fact]
    public async Task CreateProduct_ValidRequest_ReturnsCreatedWithLocationAndBody()
    {
        var response = await _client.PostAsJsonAsync("/api/products", ValidCreateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var body = await response.Content.ReadFromJsonAsync<ProductResponse>();
        body!.Id.Should().BeInRange(100000, 999999);
        body.Stock.Should().Be(25);
    }

    [Fact]
    public async Task CreateProduct_InvalidRequest_ReturnsBadRequest()
    {
        var invalidRequest = new CreateProductRequest { Name = "", Price = -1, Stock = -5 };

        var response = await _client.PostAsJsonAsync("/api/products", invalidRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_ExistingProduct_ReturnsOk()
    {
        var created = await _client.PostAsJsonAsync("/api/products", ValidCreateRequest());
        var product = await created.Content.ReadFromJsonAsync<ProductResponse>();

        var response = await _client.GetAsync($"/api/products/{product!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_NonExistentProduct_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/products/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProducts_ReturnsPagedSeededProducts()
    {
        var response = await _client.GetAsync("/api/products?page=1&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResult<ProductResponse>>();
        body!.Items.Should().HaveCount(5);
        body.TotalCount.Should().BeGreaterThanOrEqualTo(10);
    }

    [Fact]
    public async Task UpdateProduct_ValidRequest_ReturnsOkWithUpdatedFields()
    {
        var created = await _client.PostAsJsonAsync("/api/products", ValidCreateRequest());
        var product = await created.Content.ReadFromJsonAsync<ProductResponse>();

        var updateRequest = new UpdateProductRequest
        {
            Name = "Updated Name",
            Description = product!.Description,
            Price = 99.99m,
            Stock = 40,
            RowVersion = product.RowVersion,
        };

        var response = await _client.PutAsJsonAsync($"/api/products/{product.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<ProductResponse>();
        updated!.Name.Should().Be("Updated Name");
        updated.Price.Should().Be(99.99m);
    }

    [Fact]
    public async Task UpdateProduct_StaleRowVersion_ReturnsConflict()
    {
        var created = await _client.PostAsJsonAsync("/api/products", ValidCreateRequest());
        var product = await created.Content.ReadFromJsonAsync<ProductResponse>();

        var firstUpdate = new UpdateProductRequest
        {
            Name = "First Update",
            Price = 10m,
            Stock = 1,
            RowVersion = product!.RowVersion,
        };
        await _client.PutAsJsonAsync($"/api/products/{product.Id}", firstUpdate);

        // reuse the now-stale RowVersion from the original GET/create response
        var staleUpdate = new UpdateProductRequest
        {
            Name = "Second Update",
            Price = 20m,
            Stock = 2,
            RowVersion = product.RowVersion,
        };
        var response = await _client.PutAsJsonAsync($"/api/products/{product.Id}", staleUpdate);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DeleteProduct_ExistingProduct_ReturnsNoContentThenNotFound()
    {
        var created = await _client.PostAsJsonAsync("/api/products", ValidCreateRequest());
        var product = await created.Content.ReadFromJsonAsync<ProductResponse>();

        var deleteResponse = await _client.DeleteAsync($"/api/products/{product!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/products/{product.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProduct_NonExistentProduct_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/products/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
