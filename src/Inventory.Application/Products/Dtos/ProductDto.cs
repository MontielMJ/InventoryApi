namespace Inventory.Application.Products.Dtos;
public sealed record ProductDto(Guid Id, string Sku, string Name, string? Description, Guid CategoryId, string CategoryName, decimal Price, int Stock, int MinimumStock, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);
