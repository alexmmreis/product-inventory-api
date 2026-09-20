using FluentAssertions;
using ProductInventory.Application.Products.Dtos;
using ProductInventory.Application.Products.Validators;
using Xunit;

namespace ProductInventory.Application.UnitTests.Products.Validators;

public class CreateProductRequestValidatorTests
{
    private readonly CreateProductRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        var request = new CreateProductRequest { Name = "Product", Description = "Desc", Price = 9.99m, Stock = 10 };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_MissingName_HasError(string? name)
    {
        var request = new CreateProductRequest { Name = name!, Price = 9.99m, Stock = 10 };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductRequest.Name));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_PriceNotPositive_HasError(decimal price)
    {
        var request = new CreateProductRequest { Name = "Product", Price = price, Stock = 10 };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductRequest.Price));
    }

    [Fact]
    public void Validate_NegativeStock_HasError()
    {
        var request = new CreateProductRequest { Name = "Product", Price = 9.99m, Stock = -1 };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductRequest.Stock));
    }

    [Fact]
    public void Validate_DescriptionTooLong_HasError()
    {
        var request = new CreateProductRequest { Name = "Product", Description = new string('a', 1001), Price = 9.99m, Stock = 1 };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductRequest.Description));
    }
}
