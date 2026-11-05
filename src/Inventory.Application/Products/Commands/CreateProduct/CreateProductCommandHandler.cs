using Inventory.Application.Abstractions;
using Inventory.Domain.Entities;
using Inventory.Domain.Exceptions;
using MediatR;
namespace Inventory.Application.Products.Commands.CreateProduct;
public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductReadRepository _productReadRepository;
    private readonly IProductWriteRepository _productWriteRepository;
    private readonly ICategoryReadRepository _categoryReadRepository;
    private readonly TimeProvider _timeProvider;
    public CreateProductCommandHandler(IProductReadRepository productReadRepository, IProductWriteRepository productWriteRepository, ICategoryReadRepository categoryReadRepository, TimeProvider timeProvider)
    {
        _productReadRepository = productReadRepository;
        _productWriteRepository = productWriteRepository;
        _categoryReadRepository = categoryReadRepository;
        _timeProvider = timeProvider;
    }
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryReadRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null || !category.IsActive)
        {
            throw new NotFoundException($"Category '{request.CategoryId}' was not found.");
        }
        if (await _productReadRepository.SkuExistsAsync(request.Sku, null, cancellationToken))
        {
            throw new ConflictException($"A product with SKU '{request.Sku}' already exists.");
        }
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var product = Product.Create(request.Sku, request.Name, request.Description, request.CategoryId, request.Price, request.MinimumStock, utcNow);
        await _productWriteRepository.CreateAsync(product, cancellationToken);
        return product.Id;
    }
}
