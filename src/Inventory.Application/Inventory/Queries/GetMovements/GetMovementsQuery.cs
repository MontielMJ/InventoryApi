using MediatR;
namespace Inventory.Application.Inventory.Queries.GetMovements;
public sealed record GetMovementsQuery(Guid? ProductId, int Page, int PageSize) : IRequest<Common.PagedResult<Dtos.InventoryMovementDto>>;
