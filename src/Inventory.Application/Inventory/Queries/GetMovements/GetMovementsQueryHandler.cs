using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Application.Inventory.Dtos;
using MediatR;
namespace Inventory.Application.Inventory.Queries.GetMovements;
public sealed class GetMovementsQueryHandler : IRequestHandler<GetMovementsQuery, PagedResult<InventoryMovementDto>>
{
    private readonly IInventoryReadRepository _inventoryReadRepository;
    public GetMovementsQueryHandler(IInventoryReadRepository inventoryReadRepository)
    {
        _inventoryReadRepository = inventoryReadRepository;
    }
    public Task<PagedResult<InventoryMovementDto>> Handle(GetMovementsQuery request, CancellationToken cancellationToken)
    {
        var normalizedQuery = request with
        {
            Page = request.Page <= 0 ? 1 : request.Page,
            PageSize = request.PageSize <= 0 ? 10 : Math.Clamp(request.PageSize, 1, 100)
        };
        return _inventoryReadRepository.GetPagedAsync(normalizedQuery, cancellationToken);
    }
}
