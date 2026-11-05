namespace Inventory.Application.Categories.Dtos;
public sealed record CategoryDto(Guid Id, string Name, string? Description, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);
