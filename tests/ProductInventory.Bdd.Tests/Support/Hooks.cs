using ProductInventory.Api.IntegrationTests;
using Reqnroll;

namespace ProductInventory.Bdd.Tests.Support;

/// <summary>Boots a single shared API instance (real SQL Server via Testcontainers) for the whole BDD test run.</summary>
[Binding]
public static class Hooks
{
    private static ProductApiFactory? _factory;

    public static HttpClient Client { get; private set; } = null!;

    [BeforeTestRun]
    public static async Task BeforeTestRun()
    {
        _factory = new ProductApiFactory();
        await _factory.InitializeAsync();
        Client = _factory.CreateClient();
    }

    [AfterTestRun]
    public static async Task AfterTestRun()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }
    }
}
