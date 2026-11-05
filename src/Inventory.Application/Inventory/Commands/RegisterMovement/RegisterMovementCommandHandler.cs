using Inventory.Application.Abstractions;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Exceptions;
using MediatR;
namespace Inventory.Application.Inventory.Commands.RegisterMovement;
public sealed class RegisterMovementCommandHandler : IRequestHandler<RegisterMovementCommand, Guid>
{
    private readonly IInventoryWriteRepository _inventoryWriteRepository;
    private readonly TimeProvider _timeProvider;
    public RegisterMovementCommandHandler(IInventoryWriteRepository inventoryWriteRepository, TimeProvider timeProvider)
    {
        _inventoryWriteRepository = inventoryWriteRepository;
        _timeProvider = timeProvider;
    }
    public async Task<Guid> Handle(RegisterMovementCommand request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var movement = InventoryMovement.Create(request.ProductId, request.Type, request.Quantity, request.UnitCost, request.Reason, request.Reference, request.CreatedBy, utcNow);
        var result = await _inventoryWriteRepository.RegisterAsync(movement, cancellationToken);
        return result.Status switch
        {
            MovementRegistrationStatus.Created => movement.Id,
            MovementRegistrationStatus.ProductNotFound => throw new NotFoundException($"Product '{request.ProductId}' was not found."),
            _ => throw new InsufficientStockException(result.CurrentStock, request.Quantity)
        };
    }
}
