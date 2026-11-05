using Inventory.Application.Abstractions;
using Inventory.Domain.Exceptions;
using MediatR;
namespace Inventory.Application.Products.Commands.DeleteProduct;
public sealed class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IProductWriteRepository _productWriteRepository;
    private readonly TimeProvider _timeProvider;
    public DeleteProductCommandHandler(IProductWriteRepository productWriteRepository, TimeProvider timeProvider)
    {
        _productWriteRepository = productWriteRepository;
        _timeProvider = timeProvider;
    }
    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var deactivated = await _productWriteRepository.DeactivateAsync(request.Id, utcNow, cancellationToken);
        if (!deactivated)
        {
            throw new NotFoundException($"Product '{request.Id}' was not found.");
        }
    }
}
