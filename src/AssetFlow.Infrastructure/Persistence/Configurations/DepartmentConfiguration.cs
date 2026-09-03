using AssetFlow.Domain.Entities;
using AssetFlow.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetFlow.Infrastructure.Persistence.Configurations;

internal sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");
        builder.HasKey(department => department.Id);
        builder.Property(department => department.Id).ValueGeneratedNever();

        builder.Property(department => department.Name)
            .HasConversion(name => name.Value, value => new DepartmentName(value))
            .HasMaxLength(150)
            .IsRequired();
        builder.HasIndex(department => department.Name).IsUnique();

        builder.Property(department => department.Description)
            .HasMaxLength(500);
    }
}
