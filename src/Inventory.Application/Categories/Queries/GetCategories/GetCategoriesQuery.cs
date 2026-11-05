using Inventory.Application.Categories.Dtos;
using MediatR;
namespace Inventory.Application.Categories.Queries.GetCategories;
public sealed record GetCategoriesQuery(bool IncludeInactive) : IRequest<IReadOnlyList<CategoryDto>>;
