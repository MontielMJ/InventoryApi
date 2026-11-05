using Inventory.Application.Abstractions;
using Inventory.Domain.Exceptions;
using MediatR;
namespace Inventory.Application.Categories.Commands.DeleteCategory;
public sealed class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly ICategoryWriteRepository _categoryWriteRepository;
    private readonly IProductReadRepository _productReadRepository;
    private readonly TimeProvider _timeProvider;
    public DeleteCategoryCommandHandler(ICategoryWriteRepository categoryWriteRepository, IProductReadRepository productReadRepository, TimeProvider timeProvider)
    {
        _categoryWriteRepository = categoryWriteRepository;
        _productReadRepository = productReadRepository;
        _timeProvider = timeProvider;
    }
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var activeProducts = await _productReadRepository.CountActiveByCategoryAsync(request.Id, cancellationToken);
        if (activeProducts > 0)
        {
            throw new ConflictException($"Category '{request.Id}' has {activeProducts} active products and cannot be deleted.");
        }
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var deactivated = await _categoryWriteRepository.DeactivateAsync(request.Id, utcNow, cancellationToken);
        if (!deactivated)
        {
            throw new NotFoundException($"Category '{request.Id}' was not found.");
        }
    }
}
