namespace Inventory.Domain.Entities;
public sealed class Product
{
    private Product() { }
    public Guid Id { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid CategoryId { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public int MinimumStock { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Category? Category { get; private set; }
    public static Product Create(string sku, string name, string? description, Guid categoryId, decimal price, int minimumStock, DateTime utcNow)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Sku = sku.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            CategoryId = categoryId,
            Price = price,
            Stock = 0,
            MinimumStock = minimumStock,
            IsActive = true,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }
    public void Update(string sku, string name, string? description, Guid categoryId, decimal price, int minimumStock, DateTime utcNow)
    {
        Sku = sku.Trim().ToUpperInvariant();
        Name = name.Trim();
        Description = description?.Trim();
        CategoryId = categoryId;
        Price = price;
        MinimumStock = minimumStock;
        UpdatedAt = utcNow;
    }
    public void Deactivate(DateTime utcNow)
    {
        IsActive = false;
        UpdatedAt = utcNow;
    }
}
