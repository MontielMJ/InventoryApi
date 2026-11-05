using Inventory.Domain.Enums;
using Inventory.Domain.Exceptions;
namespace Inventory.Domain.Rules;
public static class StockPolicy
{
    public static int Apply(int currentStock, MovementType type, int quantity)
    {
        var nextStock = type == MovementType.In ? currentStock + quantity : currentStock - quantity;
        if (nextStock < 0)
        {
            throw new InsufficientStockException(currentStock, quantity);
        }
        return nextStock;
    }
}
