using FluentValidation;
namespace Inventory.Application.GymMembers.Commands.CreateGymMember;
public sealed class CreateGymMemberCommandValidator : AbstractValidator<CreateGymMemberCommand>
{
    public CreateGymMemberCommandValidator()
    {
        RuleFor(command => command.MemberNumber).NotEmpty().MaximumLength(30);
        RuleFor(command => command.FullName).NotEmpty().MaximumLength(160);
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(command => command.Phone).NotEmpty().MaximumLength(30);
        RuleFor(command => command.MembershipStartDate).NotEmpty();
    }
}
