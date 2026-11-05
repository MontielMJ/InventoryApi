using Inventory.Application.Abstractions;
using Inventory.Application.Inventory.Commands.RegisterMovement;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Exceptions;
using NSubstitute;
using Shouldly;
namespace Inventory.UnitTests.Application.Inventory;
public sealed class RegisterMovementCommandHandlerTests
{
    private readonly IInventoryWriteRepository _writeRepository = Substitute.For<IInventoryWriteRepository>();
    private readonly RegisterMovementCommandHandler _handler;
    public RegisterMovementCommandHandlerTests()
    {
        _handler = new RegisterMovementCommandHandler(_writeRepository, TimeProvider.System);
    }
    [Fact]
    public async Task Handle_ShouldReturnMovementId_WhenRegistrationSucceeds()
    {
        var command = BuildCommand(MovementType.In, quantity: 5);
        _writeRepository.RegisterAsync(Arg.Any<InventoryMovement>(), Arg.Any<CancellationToken>())
            .Returns(new MovementRegistrationResult(MovementRegistrationStatus.Created, 15));
        var id = await _handler.Handle(command, CancellationToken.None);
        id.ShouldNotBe(Guid.Empty);
        await _writeRepository.Received(1).RegisterAsync(Arg.Is<InventoryMovement>(movement => movement.Quantity == 5), Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenProductDoesNotExist()
    {
        var command = BuildCommand(MovementType.Out, quantity: 1);
        _writeRepository.RegisterAsync(Arg.Any<InventoryMovement>(), Arg.Any<CancellationToken>())
            .Returns(new MovementRegistrationResult(MovementRegistrationStatus.ProductNotFound, 0));
        await Should.ThrowAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
    [Fact]
    public async Task Handle_ShouldThrowInsufficientStock_WhenCurrentStockIsLowerThanQuantity()
    {
        var command = BuildCommand(MovementType.Out, quantity: 10);
        _writeRepository.RegisterAsync(Arg.Any<InventoryMovement>(), Arg.Any<CancellationToken>())
            .Returns(new MovementRegistrationResult(MovementRegistrationStatus.InsufficientStock, 3));
        await Should.ThrowAsync<InsufficientStockException>(() => _handler.Handle(command, CancellationToken.None));
    }
    private static RegisterMovementCommand BuildCommand(MovementType type, int quantity)
    {
        return new RegisterMovementCommand(Guid.NewGuid(), type, quantity, 12.5m, "test movement", "REF-1", "tester");
    }
}
