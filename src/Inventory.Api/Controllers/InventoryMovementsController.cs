using Inventory.Api.Contracts;
using Inventory.Application.Common;
using Inventory.Application.Inventory.Commands.RegisterMovement;
using Inventory.Application.Inventory.Dtos;
using Inventory.Application.Inventory.Queries.GetMovements;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Inventory.Api.Controllers;
[ApiController]
[Authorize]
[Route("api/inventory-movements")]
public sealed class InventoryMovementsController : ControllerBase
{
    private readonly ISender _sender;
    public InventoryMovementsController(ISender sender)
    {
        _sender = sender;
    }
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<InventoryMovementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<InventoryMovementDto>>> Get([FromQuery] GetMovementsQuery query, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(query, cancellationToken));
    }
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Guid>> Register([FromBody] RegisterMovementRequest request, CancellationToken cancellationToken)
    {
        var createdBy = User.FindFirst("preferred_username")?.Value ?? User.Identity?.Name ?? "unknown";
        var command = new RegisterMovementCommand(request.ProductId, request.Type, request.Quantity, request.UnitCost, request.Reason, request.Reference, createdBy);
        var id = await _sender.Send(command, cancellationToken);
        return Created($"/api/inventory-movements?productId={request.ProductId}", id);
    }
}
