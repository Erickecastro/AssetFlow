using AssetFlow.Domain.Entities;
using AssetFlow.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetFlow.Infrastructure.Persistence.Configurations;

internal sealed class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("assets");
        builder.HasKey(asset => asset.Id);
        builder.Property(asset => asset.Id).ValueGeneratedNever();

        builder.Property(asset => asset.Code)
            .HasConversion(code => code.Value, value => new AssetCode(value))
            .HasMaxLength(30)
            .IsRequired();
        builder.HasIndex(asset => asset.Code).IsUnique();

        builder.Property(asset => asset.Name)
            .HasConversion(name => name.Value, value => new AssetName(value))
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(asset => asset.Description)
            .HasConversion(
                description => description == null ? null : description.Value,
                value => value == null ? null : new AssetDescription(value))
            .HasMaxLength(500);

        builder.Property(asset => asset.SerialNumber)
            .HasConversion(
                serialNumber => serialNumber == null ? null : serialNumber.Value,
                value => value == null ? null : new SerialNumber(value))
            .HasMaxLength(100);
        builder.HasIndex(asset => asset.SerialNumber).IsUnique();

        builder.Property(asset => asset.Status).HasConversion<int>();
        builder.Property(asset => asset.Condition).HasConversion<int>();

        builder.HasOne<Department>()
            .WithMany()
            .HasForeignKey(asset => asset.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(asset => asset.Movements)
            .WithOne()
            .HasForeignKey("AssetId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(asset => asset.Movements)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
