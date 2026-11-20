using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Application.GymMembers.Dtos;
using MediatR;
namespace Inventory.Application.GymMembers.Queries.GetGymMembers;
public sealed class GetGymMembersQueryHandler : IRequestHandler<GetGymMembersQuery, PagedResult<GymMemberDto>>
{
    private readonly IGymMemberReadRepository _repository;
    public GetGymMembersQueryHandler(IGymMemberReadRepository repository)
    {
        _repository = repository;
    }
    public Task<PagedResult<GymMemberDto>> Handle(GetGymMembersQuery request, CancellationToken cancellationToken)
    {
        return _repository.GetAsync(request with { Page = Math.Max(1, request.Page), PageSize = NormalizePageSize(request.PageSize) }, cancellationToken);
    }
    private static int NormalizePageSize(int pageSize)
    {
        return pageSize <= 0 ? 10 : Math.Min(pageSize, 100);
    }
}
