using Inventory.Application.Common;
using Inventory.Application.GymMembers.Commands.CreateGymMember;
using Inventory.Application.GymMembers.Commands.UpdateGymMember;
using Inventory.Application.GymMembers.Dtos;
using Inventory.Application.GymMembers.Queries.GetGymMembers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Inventory.Api.Controllers;
[ApiController]
[Authorize]
[Route("api/gym-members")]
public sealed class GymMembersController : ControllerBase
{
    private readonly ISender _sender;
    public GymMembersController(ISender sender)
    {
        _sender = sender;
    }
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<GymMemberDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<GymMemberDto>>> Get([FromQuery] GetGymMembersQuery query, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(query, cancellationToken));
    }
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateGymMemberCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return Created("/api/gym-members", id);
    }
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGymMemberCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest("Route id and body id must match.");
        }
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }
}
