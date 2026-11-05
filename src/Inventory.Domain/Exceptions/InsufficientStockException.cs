namespace Inventory.Domain.Exceptions;
public sealed class InsufficientStockException : ConflictException
{
    public InsufficientStockException(int availableStock, int requestedQuantity)
        : base($"Requested quantity {requestedQuantity} exceeds available stock {availableStock}.") { }
}
