using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ProductInventory.Application.Products;

namespace ProductInventory.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));

        return services;
    }
}
