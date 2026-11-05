using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Inventory.Infrastructure.Persistence.Configurations;
public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(product => product.Id);
        builder.Property(product => product.Sku).HasMaxLength(40).IsRequired();
        builder.Property(product => product.Name).HasMaxLength(200).IsRequired();
        builder.Property(product => product.Description).HasMaxLength(1000);
        builder.Property(product => product.Price).HasColumnType("decimal(18,2)");
        builder.Property(product => product.CreatedAt).HasColumnType("datetime2");
        builder.Property(product => product.UpdatedAt).HasColumnType("datetime2");
        builder.HasIndex(product => product.Sku).IsUnique();
        builder.HasIndex(product => product.CategoryId);
        builder.HasOne(product => product.Category)
            .WithMany()
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasData(
            new { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Sku = "WPI-001", Name = "Whey Protein Isolate", Description = "Isolated whey protein, 30g per serving", CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"), Price = 49.99m, Stock = 25, MinimumStock = 5, IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp },
            new { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Sku = "CRE-001", Name = "Creatine Monohydrate", Description = "Micronized creatine, 5g per serving", CategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222"), Price = 24.99m, Stock = 12, MinimumStock = 3, IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp },
            new { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), Sku = "OMG-001", Name = "Omega 3", Description = "EPA and DHA fatty acids, 120 capsules", CategoryId = Guid.Parse("33333333-3333-3333-3333-333333333333"), Price = 19.99m, Stock = 40, MinimumStock = 10, IsActive = true, CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp }
        );
    }
}
