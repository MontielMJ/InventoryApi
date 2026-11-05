using FluentValidation;
namespace Inventory.Application.Products.Commands.CreateProduct;
public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(command => command.Sku).NotEmpty().MaximumLength(40).Matches("^[A-Za-z0-9-]+$");
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Description).MaximumLength(1000);
        RuleFor(command => command.CategoryId).NotEmpty();
        RuleFor(command => command.Price).GreaterThan(0);
        RuleFor(command => command.MinimumStock).GreaterThanOrEqualTo(0);
    }
}
