using FluentValidation;
using ProductInventory.Application.Products.Dtos;

namespace ProductInventory.Application.Products.Validators;

public class SearchProductsQueryValidator : AbstractValidator<SearchProductsQuery>
{
    public const int MinNameLength = 2;

    public SearchProductsQueryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(MinNameLength)
            .WithMessage($"Search term must be at least {MinNameLength} characters long.");
    }
}
