using FluentValidation;
using ProductInventory.Application.Products.Dtos;

namespace ProductInventory.Application.Products.Validators;

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.RowVersion)
            .NotEmpty()
            .WithMessage("RowVersion is required to detect conflicting concurrent updates.")
            .Must(BeValidBase64)
            .WithMessage("RowVersion must be a valid base64-encoded value.");
    }

    private static bool BeValidBase64(string value)
    {
        Span<byte> buffer = stackalloc byte[value.Length];
        return Convert.TryFromBase64String(value, buffer, out _);
    }
}
