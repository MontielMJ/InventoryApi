using Inventory.Application.Common;
using Inventory.Application.GymMembers.Dtos;
using Inventory.Domain.Enums;
using MediatR;
namespace Inventory.Application.GymMembers.Queries.GetGymMembers;
public sealed record GetGymMembersQuery(GymMemberStatus? Status, string? Search, int Page = 1, int PageSize = 10) : IRequest<PagedResult<GymMemberDto>>;
