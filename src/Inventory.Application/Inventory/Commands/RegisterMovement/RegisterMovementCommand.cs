using Inventory.Domain.Enums;
using MediatR;
namespace Inventory.Application.Inventory.Commands.RegisterMovement;
public sealed record RegisterMovementCommand(Guid ProductId, MovementType Type, int Quantity, decimal UnitCost, string? Reason, string? Reference, string CreatedBy) : IRequest<Guid>;
