using Inventory.Application.Abstractions;
using Inventory.Domain.Exceptions;
using MediatR;
namespace Inventory.Application.Categories.Commands.UpdateCategory;
public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly ICategoryReadRepository _categoryReadRepository;
    private readonly ICategoryWriteRepository _categoryWriteRepository;
    private readonly TimeProvider _timeProvider;
    public UpdateCategoryCommandHandler(ICategoryReadRepository categoryReadRepository, ICategoryWriteRepository categoryWriteRepository, TimeProvider timeProvider)
    {
        _categoryReadRepository = categoryReadRepository;
        _categoryWriteRepository = categoryWriteRepository;
        _timeProvider = timeProvider;
    }
    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryReadRepository.GetEntityAsync(request.Id, cancellationToken);
        if (category is null)
        {
            throw new NotFoundException($"Category '{request.Id}' was not found.");
        }
        if (await _categoryReadRepository.NameExistsAsync(request.Name, request.Id, cancellationToken))
        {
            throw new ConflictException($"A category named '{request.Name}' already exists.");
        }
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        category.Update(request.Name, request.Description, utcNow);
        var updated = await _categoryWriteRepository.UpdateAsync(category, cancellationToken);
        if (!updated)
        {
            throw new NotFoundException($"Category '{request.Id}' was not found.");
        }
    }
}
