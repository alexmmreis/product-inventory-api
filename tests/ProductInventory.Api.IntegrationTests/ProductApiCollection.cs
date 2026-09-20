using Xunit;

namespace ProductInventory.Api.IntegrationTests;

[CollectionDefinition(Name)]
public class ProductApiCollection : ICollectionFixture<ProductApiFactory>
{
    public const string Name = "ProductApi";
}
