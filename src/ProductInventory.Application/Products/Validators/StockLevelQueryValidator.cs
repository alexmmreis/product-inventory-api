using FluentValidation;
using ProductInventory.Application.Products.Dtos;

namespace ProductInventory.Application.Products.Validators;

public class StockLevelQueryValidator : AbstractValidator<StockLevelQuery>
{
    public StockLevelQueryValidator()
    {
        RuleFor(x => x.Min)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Max)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x)
            .Must(x => x.Min <= x.Max)
            .WithMessage("'min' must be less than or equal to 'max'.")
            .OverridePropertyName("Min");
    }
}
