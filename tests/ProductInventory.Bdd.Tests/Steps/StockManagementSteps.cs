using System.Net.Http.Json;
using FluentAssertions;
using ProductInventory.Application.Products.Dtos;
using ProductInventory.Bdd.Tests.Support;
using Reqnroll;

namespace ProductInventory.Bdd.Tests.Steps;

[Binding]
public class StockManagementSteps
{
    private readonly ScenarioContext _scenarioContext;

    public StockManagementSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Given(@"a product with (\d+) units in stock")]
    public async Task GivenAProductWithUnitsInStock(int stock)
    {
        var request = new CreateProductRequest { Name = "BDD Stock Test Product", Price = 9.99m, Stock = stock };
        var response = await Hooks.Client.PostAsJsonAsync("/api/products", request);
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        _scenarioContext["Product"] = product!;
    }

    [When(@"I decrement its stock by (\d+)")]
    public async Task WhenIDecrementItsStockBy(int quantity)
    {
        var product = _scenarioContext.Get<ProductResponse>("Product");
        var response = await Hooks.Client.PostAsync($"/api/products/{product.Id}/decrement-stock/{quantity}", null);
        _scenarioContext["Response"] = response;
    }

    [When(@"I add (\d+) units to its stock")]
    public async Task WhenIAddUnitsToItsStock(int quantity)
    {
        var product = _scenarioContext.Get<ProductResponse>("Product");
        var response = await Hooks.Client.PostAsync($"/api/products/{product.Id}/add-to-stock/{quantity}", null);
        _scenarioContext["Response"] = response;
    }

    [Then(@"the product stock should be (\d+)")]
    public async Task ThenTheProductStockShouldBe(int expectedStock)
    {
        var product = _scenarioContext.Get<ProductResponse>("Product");
        var current = await Hooks.Client.GetFromJsonAsync<ProductResponse>($"/api/products/{product.Id}");
        current!.Stock.Should().Be(expectedStock);
    }
}
