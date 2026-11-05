using Inventory.Application.Abstractions;
using Inventory.Domain.Exceptions;
using MediatR;
namespace Inventory.Application.Products.Commands.UpdateProduct;
public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IProductReadRepository _productReadRepository;
    private readonly IProductWriteRepository _productWriteRepository;
    private readonly ICategoryReadRepository _categoryReadRepository;
    private readonly TimeProvider _timeProvider;
    public UpdateProductCommandHandler(IProductReadRepository productReadRepository, IProductWriteRepository productWriteRepository, ICategoryReadRepository categoryReadRepository, TimeProvider timeProvider)
    {
        _productReadRepository = productReadRepository;
        _productWriteRepository = productWriteRepository;
        _categoryReadRepository = categoryReadRepository;
        _timeProvider = timeProvider;
    }
    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productReadRepository.GetEntityAsync(request.Id, cancellationToken);
        if (product is null)
        {
            throw new NotFoundException($"Product '{request.Id}' was not found.");
        }
        await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);
        if (await _productReadRepository.SkuExistsAsync(request.Sku, request.Id, cancellationToken))
        {
            throw new ConflictException($"A product with SKU '{request.Sku}' already exists.");
        }
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        product.Update(request.Sku, request.Name, request.Description, request.CategoryId, request.Price, request.MinimumStock, utcNow);
        var updated = await _productWriteRepository.UpdateAsync(product, cancellationToken);
        if (!updated)
        {
            throw new NotFoundException($"Product '{request.Id}' was not found.");
        }
    }
    private async Task EnsureCategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var category = await _categoryReadRepository.GetByIdAsync(categoryId, cancellationToken);
        if (category is null || !category.IsActive)
        {
            throw new NotFoundException($"Category '{categoryId}' was not found.");
        }
    }
}
