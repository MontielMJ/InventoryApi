using Inventory.Application.Categories.Queries.GetCategories;
using Inventory.Application.Common;
using Inventory.Application.Products.Dtos;
using Inventory.Application.Products.Queries.GetProducts;
using Inventory.Domain.Entities;
namespace Inventory.Application.Abstractions;
public interface IProductReadRepository
{
    Task<PagedResult<ProductDto>> GetPagedAsync(GetProductsQuery query, CancellationToken cancellationToken);
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Product?> GetEntityAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> SkuExistsAsync(string sku, Guid? excludedId, CancellationToken cancellationToken);
    Task<int> CountActiveByCategoryAsync(Guid categoryId, CancellationToken cancellationToken);
}
