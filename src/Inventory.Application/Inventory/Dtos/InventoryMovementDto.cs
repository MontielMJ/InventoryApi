using Inventory.Domain.Enums;
namespace Inventory.Application.Inventory.Dtos;
public sealed record InventoryMovementDto(Guid Id, Guid ProductId, string ProductName, MovementType Type, int Quantity, decimal UnitCost, string? Reason, string? Reference, string CreatedBy, DateTime CreatedAt);
