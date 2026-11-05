using Inventory.Application.Abstractions;
using Inventory.Application.Categories.Dtos;
using MediatR;
namespace Inventory.Application.Categories.Queries.GetCategories;
public sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly ICategoryReadRepository _categoryReadRepository;
    public GetCategoriesQueryHandler(ICategoryReadRepository categoryReadRepository)
    {
        _categoryReadRepository = categoryReadRepository;
    }
    public Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        return _categoryReadRepository.GetAllAsync(request, cancellationToken);
    }
}
