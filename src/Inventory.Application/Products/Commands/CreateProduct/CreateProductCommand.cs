using MediatR;
namespace Inventory.Application.Products.Commands.CreateProduct;
public sealed record CreateProductCommand(string Sku, string Name, string? Description, Guid CategoryId, decimal Price, int MinimumStock) : IRequest<Guid>;
