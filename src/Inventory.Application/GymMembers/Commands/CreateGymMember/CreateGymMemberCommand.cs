using Inventory.Domain.Enums;
using MediatR;
namespace Inventory.Application.GymMembers.Commands.CreateGymMember;
public sealed record CreateGymMemberCommand(string MemberNumber, string FullName, string Email, string Phone, DateTime MembershipStartDate) : IRequest<Guid>;
