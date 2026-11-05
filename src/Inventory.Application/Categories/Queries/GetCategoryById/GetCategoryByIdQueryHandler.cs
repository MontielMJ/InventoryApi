using Inventory.Application.Abstractions;
using Inventory.Application.Categories.Dtos;
using Inventory.Domain.Exceptions;
using MediatR;
namespace Inventory.Application.Categories.Queries.GetCategoryById;
public sealed class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly ICategoryReadRepository _categoryReadRepository;
    public GetCategoryByIdQueryHandler(ICategoryReadRepository categoryReadRepository)
    {
        _categoryReadRepository = categoryReadRepository;
    }
    public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _categoryReadRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
        {
            throw new NotFoundException($"Category '{request.Id}' was not found.");
        }
        return category;
    }
}
