using FluentValidation.TestHelper;
using Inventory.Application.Inventory.Commands.RegisterMovement;
using Inventory.Domain.Enums;
namespace Inventory.UnitTests.Application.Inventory;
public sealed class RegisterMovementCommandValidatorTests
{
    private readonly RegisterMovementCommandValidator _validator = new();
    [Fact]
    public void Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new RegisterMovementCommand(Guid.NewGuid(), MovementType.Out, 2, 10m, "sale", "REF-1", "tester");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
    [Fact]
    public void Validate_ShouldFail_WhenQuantityIsZero()
    {
        var command = new RegisterMovementCommand(Guid.NewGuid(), MovementType.In, 0, 10m, null, null, "tester");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(movement => movement.Quantity);
    }
    [Fact]
    public void Validate_ShouldFail_WhenMovementTypeIsInvalid()
    {
        var command = new RegisterMovementCommand(Guid.NewGuid(), (MovementType)99, 1, 10m, null, null, "tester");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(movement => movement.Type);
    }
    [Fact]
    public void Validate_ShouldFail_WhenCreatedByIsEmpty()
    {
        var command = new RegisterMovementCommand(Guid.NewGuid(), MovementType.In, 1, 10m, null, null, string.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(movement => movement.CreatedBy);
    }
}
