using Inventory.Domain.Enums;
namespace Inventory.Api.Contracts;
public sealed record RegisterMovementRequest(Guid ProductId, MovementType Type, int Quantity, decimal UnitCost, string? Reason, string? Reference);
