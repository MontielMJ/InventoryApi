using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Application.Inventory.Dtos;
using Inventory.Application.Inventory.Queries.GetMovements;
using Microsoft.EntityFrameworkCore;
namespace Inventory.Infrastructure.Persistence.Ef;
public sealed class InventoryReadRepository : IInventoryReadRepository
{
    private readonly InventoryDbContext _dbContext;
    public InventoryReadRepository(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<PagedResult<InventoryMovementDto>> GetPagedAsync(GetMovementsQuery query, CancellationToken cancellationToken)
    {
        var movements = _dbContext.InventoryMovements.AsNoTracking();
        if (query.ProductId.HasValue)
        {
            movements = movements.Where(movement => movement.ProductId == query.ProductId.Value);
        }
        var totalCount = await movements.CountAsync(cancellationToken);
        var items = await movements
            .OrderByDescending(movement => movement.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(movement => new InventoryMovementDto(
                movement.Id,
                movement.ProductId,
                movement.Product!.Name,
                movement.Type,
                movement.Quantity,
                movement.UnitCost,
                movement.Reason,
                movement.Reference,
                movement.CreatedBy,
                movement.CreatedAt))
            .ToListAsync(cancellationToken);
        return new PagedResult<InventoryMovementDto>(items, query.Page, query.PageSize, totalCount);
    }
}
