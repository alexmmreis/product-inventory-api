using System.Net.Http.Json;
using FluentAssertions;
using ProductInventory.Application.Products.Dtos;
using ProductInventory.Bdd.Tests.Support;
using Reqnroll;

namespace ProductInventory.Bdd.Tests.Steps;

[Binding]
public class ProductCreationSteps
{
    private readonly ScenarioContext _scenarioContext;

    public ProductCreationSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Given(@"I have a valid product named ""(.*)"" priced at (-?[\d.]+) with (\d+) units in stock")]
    [Given(@"I have a product named ""(.*)"" priced at (-?[\d.]+) with (\d+) units in stock")]
    public void GivenIHaveAProductNamedPricedAtWithUnitsInStock(string name, decimal price, int stock)
    {
        var request = new CreateProductRequest { Name = name, Price = price, Stock = stock };
        _scenarioContext["Request"] = request;
    }

    [When(@"I submit the product for creation")]
    public async Task WhenISubmitTheProductForCreation()
    {
        var request = _scenarioContext.Get<CreateProductRequest>("Request");
        var response = await Hooks.Client.PostAsJsonAsync("/api/products", request);
        _scenarioContext["Response"] = response;
    }

    [Then(@"the created product should have a 6-digit identifier")]
    public async Task ThenTheCreatedProductShouldHaveASixDigitIdentifier()
    {
        var response = _scenarioContext.Get<HttpResponseMessage>("Response");
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        product!.Id.Should().BeInRange(100000, 999999);
    }
}
