using FluentValidation.TestHelper;
using Inventory.Application.GymMembers.Commands.UpdateGymMember;
using Inventory.Domain.Enums;
namespace Inventory.UnitTests.Application.GymMembers;
public sealed class UpdateGymMemberCommandValidatorTests
{
    private readonly UpdateGymMemberCommandValidator _validator = new();
    [Fact]
    public void Validate_ShouldFail_WhenCancellationReasonIsMissing()
    {
        var command = new UpdateGymMemberCommand(Guid.NewGuid(), "GYM-001", "Alex Member", "alex@example.com", "555-0100", GymMemberStatus.Cancelled, DateTime.UtcNow.Date, DateTime.UtcNow.Date, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(member => member.CancellationReason);
    }
    [Fact]
    public void Validate_ShouldFail_WhenEndDateIsBeforeStartDate()
    {
        var command = new UpdateGymMemberCommand(Guid.NewGuid(), "GYM-001", "Alex Member", "alex@example.com", "555-0100", GymMemberStatus.Inactive, DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(-1), null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(member => member.MembershipEndDate);
    }
}
