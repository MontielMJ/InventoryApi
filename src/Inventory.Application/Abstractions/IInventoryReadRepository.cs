using Inventory.Application.Common;
using Inventory.Application.Inventory.Dtos;
using Inventory.Application.Inventory.Queries.GetMovements;
namespace Inventory.Application.Abstractions;
public interface IInventoryReadRepository
{
    Task<PagedResult<InventoryMovementDto>> GetPagedAsync(GetMovementsQuery query, CancellationToken cancellationToken);
}
