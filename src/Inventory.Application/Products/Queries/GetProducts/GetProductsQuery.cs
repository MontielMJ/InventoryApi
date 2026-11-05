using Inventory.Application.Common;
using Inventory.Application.Products.Dtos;
using MediatR;
namespace Inventory.Application.Products.Queries.GetProducts;
public sealed record GetProductsQuery(Guid? CategoryId, string? Search, bool? IsActive, int Page, int PageSize) : IRequest<PagedResult<ProductDto>>;
