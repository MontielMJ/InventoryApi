using Inventory.Application.Abstractions;
using Inventory.Application.Products.Dtos;
using Inventory.Domain.Exceptions;
using MediatR;
namespace Inventory.Application.Products.Queries.GetProductById;
public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IProductReadRepository _productReadRepository;
    public GetProductByIdQueryHandler(IProductReadRepository productReadRepository)
    {
        _productReadRepository = productReadRepository;
    }
    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productReadRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            throw new NotFoundException($"Product '{request.Id}' was not found.");
        }
        return product;
    }
}
