using Inventory.Domain.Enums;
using MediatR;
namespace Inventory.Application.GymMembers.Commands.UpdateGymMember;
public sealed record UpdateGymMemberCommand(Guid Id, string MemberNumber, string FullName, string Email, string Phone, GymMemberStatus Status, DateTime MembershipStartDate, DateTime? MembershipEndDate, string? CancellationReason) : IRequest;
