using FluentValidation;
using Inventory.Domain.Enums;
namespace Inventory.Application.GymMembers.Commands.UpdateGymMember;
public sealed class UpdateGymMemberCommandValidator : AbstractValidator<UpdateGymMemberCommand>
{
    public UpdateGymMemberCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.MemberNumber).NotEmpty().MaximumLength(30);
        RuleFor(command => command.FullName).NotEmpty().MaximumLength(160);
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(command => command.Phone).NotEmpty().MaximumLength(30);
        RuleFor(command => command.Status).IsInEnum();
        RuleFor(command => command.MembershipStartDate).NotEmpty();
        RuleFor(command => command.MembershipEndDate).GreaterThanOrEqualTo(command => command.MembershipStartDate).When(command => command.MembershipEndDate.HasValue);
        RuleFor(command => command.CancellationReason).NotEmpty().MaximumLength(500).When(command => command.Status == GymMemberStatus.Cancelled);
    }
}
