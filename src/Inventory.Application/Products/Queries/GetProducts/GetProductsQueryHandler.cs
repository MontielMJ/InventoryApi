using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Application.Products.Dtos;
using MediatR;
namespace Inventory.Application.Products.Queries.GetProducts;
public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly IProductReadRepository _productReadRepository;
    public GetProductsQueryHandler(IProductReadRepository productReadRepository)
    {
        _productReadRepository = productReadRepository;
    }
    public Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var normalizedQuery = request with
        {
            Page = request.Page <= 0 ? 1 : request.Page,
            PageSize = request.PageSize <= 0 ? 10 : Math.Clamp(request.PageSize, 1, 100),
            Search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim()
        };
        return _productReadRepository.GetPagedAsync(normalizedQuery, cancellationToken);
    }
}
