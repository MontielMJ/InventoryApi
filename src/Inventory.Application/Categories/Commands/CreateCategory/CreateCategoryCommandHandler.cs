using Inventory.Application.Abstractions;
using Inventory.Domain.Entities;
using Inventory.Domain.Exceptions;
using MediatR;
namespace Inventory.Application.Categories.Commands.CreateCategory;
public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryReadRepository _categoryReadRepository;
    private readonly ICategoryWriteRepository _categoryWriteRepository;
    private readonly TimeProvider _timeProvider;
    public CreateCategoryCommandHandler(ICategoryReadRepository categoryReadRepository, ICategoryWriteRepository categoryWriteRepository, TimeProvider timeProvider)
    {
        _categoryReadRepository = categoryReadRepository;
        _categoryWriteRepository = categoryWriteRepository;
        _timeProvider = timeProvider;
    }
    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (await _categoryReadRepository.NameExistsAsync(request.Name, null, cancellationToken))
        {
            throw new ConflictException($"A category named '{request.Name}' already exists.");
        }
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var category = Category.Create(request.Name, request.Description, utcNow);
        await _categoryWriteRepository.CreateAsync(category, cancellationToken);
        return category.Id;
    }
}
