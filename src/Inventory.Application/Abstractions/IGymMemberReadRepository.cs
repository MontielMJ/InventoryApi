using Inventory.Application.Common;
using Inventory.Application.GymMembers.Dtos;
using Inventory.Application.GymMembers.Queries.GetGymMembers;
using Inventory.Domain.Entities;
namespace Inventory.Application.Abstractions;
public interface IGymMemberReadRepository
{
    Task<PagedResult<GymMemberDto>> GetAsync(GetGymMembersQuery query, CancellationToken cancellationToken);
    Task<GymMember?> GetEntityAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> MemberNumberExistsAsync(string memberNumber, Guid? excludedId, CancellationToken cancellationToken);
}
