namespace Inventory.Domain.Entities;
public sealed class Category
{
    private Category() { }
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public static Category Create(string name, string? description, DateTime utcNow)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            IsActive = true,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }
    public void Update(string name, string? description, DateTime utcNow)
    {
        Name = name.Trim();
        Description = description?.Trim();
        UpdatedAt = utcNow;
    }
    public void Deactivate(DateTime utcNow)
    {
        IsActive = false;
        UpdatedAt = utcNow;
    }
}
