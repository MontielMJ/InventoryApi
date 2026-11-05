using FluentValidation;
using Inventory.Domain.Enums;
namespace Inventory.Application.Inventory.Commands.RegisterMovement;
public sealed class RegisterMovementCommandValidator : AbstractValidator<RegisterMovementCommand>
{
    public RegisterMovementCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.Type).IsInEnum();
        RuleFor(command => command.Quantity).GreaterThan(0);
        RuleFor(command => command.UnitCost).GreaterThanOrEqualTo(0);
        RuleFor(command => command.Reason).MaximumLength(500);
        RuleFor(command => command.Reference).MaximumLength(100);
        RuleFor(command => command.CreatedBy).NotEmpty().MaximumLength(100);
    }
}
