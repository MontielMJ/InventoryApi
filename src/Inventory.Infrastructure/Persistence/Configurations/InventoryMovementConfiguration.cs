using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Inventory.Infrastructure.Persistence.Configurations;
public sealed class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.ToTable("InventoryMovements");
        builder.HasKey(movement => movement.Id);
        builder.Property(movement => movement.Type).HasConversion<int>().IsRequired();
        builder.Property(movement => movement.UnitCost).HasColumnType("decimal(18,2)");
        builder.Property(movement => movement.Reason).HasMaxLength(500);
        builder.Property(movement => movement.Reference).HasMaxLength(100);
        builder.Property(movement => movement.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(movement => movement.CreatedAt).HasColumnType("datetime2");
        builder.HasIndex(movement => movement.ProductId);
        builder.HasIndex(movement => movement.CreatedAt);
        builder.HasOne(movement => movement.Product)
            .WithMany()
            .HasForeignKey(movement => movement.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasData(
            new { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), ProductId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Type = MovementType.In, Quantity = 25, UnitCost = 32.50m, Reason = "Initial stock", Reference = "SEED", CreatedBy = "system", CreatedAt = SeedTimestamp },
            new { Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), ProductId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Type = MovementType.In, Quantity = 12, UnitCost = 15.00m, Reason = "Initial stock", Reference = "SEED", CreatedBy = "system", CreatedAt = SeedTimestamp },
            new { Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), ProductId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), Type = MovementType.In, Quantity = 40, UnitCost = 11.25m, Reason = "Initial stock", Reference = "SEED", CreatedBy = "system", CreatedAt = SeedTimestamp }
        );
    }
}
