using FluentValidation.TestHelper;
using Inventory.Application.GymMembers.Commands.CreateGymMember;
namespace Inventory.UnitTests.Application.GymMembers;
public sealed class CreateGymMemberCommandValidatorTests
{
    private readonly CreateGymMemberCommandValidator _validator = new();
    [Fact]
    public void Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new CreateGymMemberCommand("GYM-001", "Alex Member", "alex@example.com", "555-0100", DateTime.UtcNow.Date);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
    [Fact]
    public void Validate_ShouldFail_WhenEmailIsInvalid()
    {
        var command = new CreateGymMemberCommand("GYM-001", "Alex Member", "invalid", "555-0100", DateTime.UtcNow.Date);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(member => member.Email);
    }
}
