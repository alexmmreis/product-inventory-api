using System.Net.Http.Json;
using FluentAssertions;
using ProductInventory.Application.Products.Dtos;
using ProductInventory.Bdd.Tests.Support;
using Reqnroll;

namespace ProductInventory.Bdd.Tests.Steps;

[Binding]
public class ProductSearchSteps
{
    private readonly ScenarioContext _scenarioContext;

    public ProductSearchSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Given(@"a product named ""(.*)"" exists")]
    public async Task GivenAProductNamedExists(string name)
    {
        var request = new CreateProductRequest { Name = name, Price = 9.99m, Stock = 10 };
        await Hooks.Client.PostAsJsonAsync("/api/products", request);
    }

    [Given(@"a product named ""(.*)"" with (\d+) units in stock exists")]
    public async Task GivenAProductNamedWithUnitsInStockExists(string name, int stock)
    {
        var request = new CreateProductRequest { Name = name, Price = 9.99m, Stock = stock };
        await Hooks.Client.PostAsJsonAsync("/api/products", request);
    }

    [When(@"I search for products matching ""(.*)""")]
    public async Task WhenISearchForProductsMatching(string term)
    {
        var response = await Hooks.Client.GetAsync($"/api/products/search?name={Uri.EscapeDataString(term)}");
        _scenarioContext["Response"] = response;
    }

    [When(@"I filter products with stock between (\d+) and (\d+)")]
    public async Task WhenIFilterProductsWithStockBetweenAnd(int min, int max)
    {
        var response = await Hooks.Client.GetAsync($"/api/products/stock-level?min={min}&max={max}");
        _scenarioContext["Response"] = response;
    }

    [Then(@"the results should include ""(.*)""")]
    public async Task ThenTheResultsShouldInclude(string expectedName)
    {
        var response = _scenarioContext.Get<HttpResponseMessage>("Response");
        var results = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();
        results.Should().Contain(p => p.Name == expectedName);
    }
}
