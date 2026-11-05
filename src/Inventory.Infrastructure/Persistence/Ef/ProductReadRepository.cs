using System.Linq.Expressions;
using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Application.Products.Dtos;
using Inventory.Application.Products.Queries.GetProducts;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace Inventory.Infrastructure.Persistence.Ef;
public sealed class ProductReadRepository : IProductReadRepository
{
    private static readonly Expression<Func<Product, ProductDto>> Projection = product => new ProductDto(
        product.Id,
        product.Sku,
        product.Name,
        product.Description,
        product.CategoryId,
        product.Category!.Name,
        product.Price,
        product.Stock,
        product.MinimumStock,
        product.IsActive,
        product.CreatedAt,
        product.UpdatedAt);
    private readonly InventoryDbContext _dbContext;
    public ProductReadRepository(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<PagedResult<ProductDto>> GetPagedAsync(GetProductsQuery query, CancellationToken cancellationToken)
    {
        var products = ApplyFilters(_dbContext.Products.AsNoTracking(), query);
        var totalCount = await products.CountAsync(cancellationToken);
        var items = await products
            .OrderBy(product => product.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(Projection)
            .ToListAsync(cancellationToken);
        return new PagedResult<ProductDto>(items, query.Page, query.PageSize, totalCount);
    }
    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.Id == id)
            .Select(Projection)
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<Product?> GetEntityAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Products.AsNoTracking().FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }
    public Task<bool> SkuExistsAsync(string sku, Guid? excludedId, CancellationToken cancellationToken)
    {
        var normalizedSku = sku.Trim().ToUpperInvariant();
        return _dbContext.Products.AnyAsync(product => product.Sku == normalizedSku && (excludedId == null || product.Id != excludedId), cancellationToken);
    }
    public Task<int> CountActiveByCategoryAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        return _dbContext.Products.CountAsync(product => product.CategoryId == categoryId && product.IsActive, cancellationToken);
    }
    private static IQueryable<Product> ApplyFilters(IQueryable<Product> products, GetProductsQuery query)
    {
        if (query.IsActive.HasValue)
        {
            products = products.Where(product => product.IsActive == query.IsActive.Value);
        }
        if (query.CategoryId.HasValue)
        {
            products = products.Where(product => product.CategoryId == query.CategoryId.Value);
        }
        if (query.Search is not null)
        {
            products = products.Where(product => product.Name.Contains(query.Search) || product.Sku.Contains(query.Search));
        }
        return products;
    }
}
