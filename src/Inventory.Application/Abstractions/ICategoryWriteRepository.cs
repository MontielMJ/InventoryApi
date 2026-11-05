using Inventory.Domain.Entities;
namespace Inventory.Application.Abstractions;
public interface ICategoryWriteRepository
{
    Task CreateAsync(Category category, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Category category, CancellationToken cancellationToken);
    Task<bool> DeactivateAsync(Guid id, DateTime utcNow, CancellationToken cancellationToken);
}
