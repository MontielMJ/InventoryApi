using Inventory.Domain.Entities;
namespace Inventory.Application.Abstractions;
public interface IProductWriteRepository
{
    Task CreateAsync(Product product, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Product product, CancellationToken cancellationToken);
    Task<bool> DeactivateAsync(Guid id, DateTime utcNow, CancellationToken cancellationToken);
}
