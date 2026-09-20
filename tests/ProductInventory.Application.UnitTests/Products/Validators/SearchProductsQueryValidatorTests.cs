using FluentAssertions;
using ProductInventory.Application.Products.Dtos;
using ProductInventory.Application.Products.Validators;
using Xunit;

namespace ProductInventory.Application.UnitTests.Products.Validators;

public class SearchProductsQueryValidatorTests
{
    private readonly SearchProductsQueryValidator _validator = new();

    [Fact]
    public void Validate_NameAtMinimumLength_HasNoErrors()
    {
        var query = new SearchProductsQuery { Name = "ab" };

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    public void Validate_NameShorterThanMinimum_HasError(string name)
    {
        var query = new SearchProductsQuery { Name = name };

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SearchProductsQuery.Name));
    }
}
