using Inventory.Domain.Entities;
namespace Inventory.Application.Abstractions;
public interface IInventoryWriteRepository
{
    Task<MovementRegistrationResult> RegisterAsync(InventoryMovement movement, CancellationToken cancellationToken);
}
