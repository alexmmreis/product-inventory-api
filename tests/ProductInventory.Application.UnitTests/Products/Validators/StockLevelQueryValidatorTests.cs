using FluentAssertions;
using ProductInventory.Application.Products.Dtos;
using ProductInventory.Application.Products.Validators;
using Xunit;

namespace ProductInventory.Application.UnitTests.Products.Validators;

public class StockLevelQueryValidatorTests
{
    private readonly StockLevelQueryValidator _validator = new();

    [Fact]
    public void Validate_MinLessThanOrEqualToMax_HasNoErrors()
    {
        var query = new StockLevelQuery { Min = 0, Max = 10 };

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_MinGreaterThanMax_HasError()
    {
        var query = new StockLevelQuery { Min = 20, Max = 10 };

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_NegativeMin_HasError()
    {
        var query = new StockLevelQuery { Min = -1, Max = 10 };

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
    }
}
