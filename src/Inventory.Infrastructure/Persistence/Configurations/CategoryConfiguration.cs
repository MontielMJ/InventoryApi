using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Inventory.Infrastructure.Persistence.Configurations;
public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(category => category.Id);
        builder.Property(category => category.Name).HasMaxLength(120).IsRequired();
        builder.Property(category => category.Description).HasMaxLength(500);
        builder.Property(category => category.CreatedAt).HasColumnType("datetime2");
        builder.Property(category => category.UpdatedAt).HasColumnType("datetime2");
        builder.HasIndex(category => category.Name).IsUnique();
        builder.HasData(
            new { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Protein", Description = "Protein and amino acid supplements", IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp },
            new { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Creatine", Description = "Creatine based supplements", IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp },
            new { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Vitamins", Description = "Vitamins and minerals", IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp }
        );
    }
}
