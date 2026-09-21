using FluentAssertions;
using ProductInventory.Application.Products.Dtos;
using ProductInventory.Application.Products.Validators;
using Xunit;

namespace ProductInventory.Application.UnitTests.Products.Validators;

public class UpdateProductRequestValidatorTests
{
    private readonly UpdateProductRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        var request = new UpdateProductRequest
        {
            Name = "Product",
            Price = 9.99m,
            Stock = 10,
            RowVersion = Convert.ToBase64String([1, 2, 3]),
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_MissingRowVersion_HasError()
    {
        var request = new UpdateProductRequest { Name = "Product", Price = 9.99m, Stock = 10, RowVersion = "" };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProductRequest.RowVersion));
    }

    [Fact]
    public void Validate_NonBase64RowVersion_HasError()
    {
        var request = new UpdateProductRequest { Name = "Product", Price = 9.99m, Stock = 10, RowVersion = "not-base64!!" };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProductRequest.RowVersion));
    }

    [Fact]
    public void Validate_MissingName_HasError()
    {
        var request = new UpdateProductRequest { Name = "", Price = 9.99m, Stock = 10, RowVersion = Convert.ToBase64String([1]) };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProductRequest.Name));
    }
}
