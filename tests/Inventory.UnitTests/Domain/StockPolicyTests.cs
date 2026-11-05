using Inventory.Domain.Enums;
using Inventory.Domain.Exceptions;
using Inventory.Domain.Rules;
using Shouldly;
namespace Inventory.UnitTests.Domain;
public sealed class StockPolicyTests
{
    [Fact]
    public void Apply_ShouldIncreaseStock_WhenMovementIsIn()
    {
        var result = StockPolicy.Apply(currentStock: 10, MovementType.In, quantity: 5);
        result.ShouldBe(15);
    }
    [Fact]
    public void Apply_ShouldDecreaseStock_WhenMovementIsOut()
    {
        var result = StockPolicy.Apply(currentStock: 10, MovementType.Out, quantity: 4);
        result.ShouldBe(6);
    }
    [Fact]
    public void Apply_ShouldThrow_WhenMovementExceedsAvailableStock()
    {
        Should.Throw<InsufficientStockException>(() => StockPolicy.Apply(currentStock: 3, MovementType.Out, quantity: 4));
    }
}
