using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Inventory.Infrastructure.Persistence.Configurations;
public sealed class GymMemberConfiguration : IEntityTypeConfiguration<GymMember>
{
    public void Configure(EntityTypeBuilder<GymMember> builder)
    {
        builder.ToTable("GymMembers");
        builder.HasKey(member => member.Id);
        builder.Property(member => member.MemberNumber).HasMaxLength(30).IsRequired();
        builder.Property(member => member.FullName).HasMaxLength(160).IsRequired();
        builder.Property(member => member.Email).HasMaxLength(200).IsRequired();
        builder.Property(member => member.Phone).HasMaxLength(30).IsRequired();
        builder.Property(member => member.Status).HasConversion<int>().IsRequired();
        builder.Property(member => member.MembershipStartDate).HasColumnType("date");
        builder.Property(member => member.MembershipEndDate).HasColumnType("date");
        builder.Property(member => member.CancellationReason).HasMaxLength(500);
        builder.Property(member => member.CreatedAt).HasColumnType("datetime2");
        builder.Property(member => member.UpdatedAt).HasColumnType("datetime2");
        builder.HasIndex(member => member.MemberNumber).IsUnique();
        builder.HasIndex(member => member.Status);
    }
}
