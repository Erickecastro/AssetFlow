using AssetFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetFlow.Infrastructure.Persistence.Configurations;

internal sealed class AssetMovementConfiguration : IEntityTypeConfiguration<AssetMovement>
{
    public void Configure(EntityTypeBuilder<AssetMovement> builder)
    {
        builder.ToTable("asset_movements");
        builder.HasKey(movement => movement.Id);
        builder.Property(movement => movement.Id).ValueGeneratedNever();
        builder.Property(movement => movement.Type).HasConversion<int>();
        builder.Property(movement => movement.OccurredAtUtc)
            .HasColumnType("timestamp with time zone");
        builder.Property(movement => movement.Notes).HasMaxLength(500);
        builder.Property<Guid>("AssetId").IsRequired();

        builder.HasOne<Department>()
            .WithMany()
            .HasForeignKey(movement => movement.FromDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Department>()
            .WithMany()
            .HasForeignKey(movement => movement.ToDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
