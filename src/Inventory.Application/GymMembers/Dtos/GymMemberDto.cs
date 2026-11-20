using Inventory.Domain.Enums;
namespace Inventory.Application.GymMembers.Dtos;
public sealed record GymMemberDto(Guid Id, string MemberNumber, string FullName, string Email, string Phone, GymMemberStatus Status, DateTime MembershipStartDate, DateTime? MembershipEndDate, string? CancellationReason, DateTime CreatedAt, DateTime UpdatedAt);
