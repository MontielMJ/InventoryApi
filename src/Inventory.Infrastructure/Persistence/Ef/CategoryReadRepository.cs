using System.Linq.Expressions;
using Inventory.Application.Abstractions;
using Inventory.Application.Categories.Dtos;
using Inventory.Application.Categories.Queries.GetCategories;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace Inventory.Infrastructure.Persistence.Ef;
public sealed class CategoryReadRepository : ICategoryReadRepository
{
    private static readonly Expression<Func<Category, CategoryDto>> Projection = category => new CategoryDto(
        category.Id,
        category.Name,
        category.Description,
        category.IsActive,
        category.CreatedAt,
        category.UpdatedAt);
    private readonly InventoryDbContext _dbContext;
    public CategoryReadRepository(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(GetCategoriesQuery query, CancellationToken cancellationToken)
    {
        var categories = _dbContext.Categories.AsNoTracking();
        if (!query.IncludeInactive)
        {
            categories = categories.Where(category => category.IsActive);
        }
        return await categories.OrderBy(category => category.Name).Select(Projection).ToListAsync(cancellationToken);
    }
    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Categories.AsNoTracking().Where(category => category.Id == id).Select(Projection).FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<Category?> GetEntityAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Categories.AsNoTracking().FirstOrDefaultAsync(category => category.Id == id, cancellationToken);
    }
    public Task<bool> NameExistsAsync(string name, Guid? excludedId, CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim();
        return _dbContext.Categories.AnyAsync(category => category.Name == normalizedName && (excludedId == null || category.Id != excludedId), cancellationToken);
    }
}
