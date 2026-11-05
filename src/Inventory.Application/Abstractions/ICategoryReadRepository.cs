using Inventory.Application.Categories.Dtos;
using Inventory.Application.Categories.Queries.GetCategories;
using Inventory.Domain.Entities;
namespace Inventory.Application.Abstractions;
public interface ICategoryReadRepository
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(GetCategoriesQuery query, CancellationToken cancellationToken);
    Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Category?> GetEntityAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> NameExistsAsync(string name, Guid? excludedId, CancellationToken cancellationToken);
}
