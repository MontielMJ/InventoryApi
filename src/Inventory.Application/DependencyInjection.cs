using FluentValidation;
using Inventory.Application.Products.Commands.CreateProduct;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
namespace Inventory.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssemblyContaining<CreateProductCommand>());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Behaviors.ValidationBehavior<,>));
        services.AddValidatorsFromAssemblyContaining<CreateProductCommandValidator>();
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        return services;
    }
}
