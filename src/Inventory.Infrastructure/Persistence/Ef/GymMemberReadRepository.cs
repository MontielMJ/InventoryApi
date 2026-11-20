using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Application.GymMembers.Dtos;
using Inventory.Application.GymMembers.Queries.GetGymMembers;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace Inventory.Infrastructure.Persistence.Ef;
public sealed class GymMemberReadRepository : IGymMemberReadRepository
{
    private readonly InventoryDbContext _dbContext;
    public GymMemberReadRepository(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<PagedResult<GymMemberDto>> GetAsync(GetGymMembersQuery query, CancellationToken cancellationToken)
    {
        var members = ApplyFilters(_dbContext.GymMembers.AsNoTracking(), query);
        var totalCount = await members.CountAsync(cancellationToken);
        var items = await members.OrderBy(member => member.FullName).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).Select(member => new GymMemberDto(member.Id, member.MemberNumber, member.FullName, member.Email, member.Phone, member.Status, member.MembershipStartDate, member.MembershipEndDate, member.CancellationReason, member.CreatedAt, member.UpdatedAt)).ToListAsync(cancellationToken);
        return new PagedResult<GymMemberDto>(items, query.Page, query.PageSize, totalCount);
    }
    public Task<GymMember?> GetEntityAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.GymMembers.AsNoTracking().FirstOrDefaultAsync(member => member.Id == id, cancellationToken);
    }
    public Task<bool> MemberNumberExistsAsync(string memberNumber, Guid? excludedId, CancellationToken cancellationToken)
    {
        var normalizedNumber = memberNumber.Trim().ToUpperInvariant();
        return _dbContext.GymMembers.AnyAsync(member => member.MemberNumber == normalizedNumber && (excludedId == null || member.Id != excludedId), cancellationToken);
    }
    private static IQueryable<GymMember> ApplyFilters(IQueryable<GymMember> members, GetGymMembersQuery query)
    {
        if (query.Status.HasValue)
        {
            members = members.Where(member => member.Status == query.Status.Value);
        }
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            members = members.Where(member => member.FullName.Contains(query.Search) || member.MemberNumber.Contains(query.Search) || member.Email.Contains(query.Search));
        }
        return members;
    }
}
