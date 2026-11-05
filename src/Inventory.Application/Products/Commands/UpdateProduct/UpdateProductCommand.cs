using MediatR;
namespace Inventory.Application.Products.Commands.UpdateProduct;
public sealed record UpdateProductCommand(Guid Id, string Sku, string Name, string? Description, Guid CategoryId, decimal Price, int MinimumStock) : IRequest;
