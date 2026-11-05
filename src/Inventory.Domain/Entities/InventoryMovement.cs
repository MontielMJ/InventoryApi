using Inventory.Domain.Enums;
namespace Inventory.Domain.Entities;
public sealed class InventoryMovement
{
    private InventoryMovement() { }
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public MovementType Type { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public string? Reason { get; private set; }
    public string? Reference { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public Product? Product { get; private set; }
    public static InventoryMovement Create(Guid productId, MovementType type, int quantity, decimal unitCost, string? reason, string? reference, string createdBy, DateTime utcNow)
    {
        return new InventoryMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Type = type,
            Quantity = quantity,
            UnitCost = unitCost,
            Reason = reason?.Trim(),
            Reference = reference?.Trim(),
            CreatedBy = createdBy,
            CreatedAt = utcNow
        };
    }
}
