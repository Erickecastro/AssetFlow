using AssetFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Infrastructure.Persistence;

public sealed class AssetFlowDbContext : DbContext
{
    public AssetFlowDbContext(DbContextOptions<AssetFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<Asset> Assets => Set<Asset>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<AssetMovement> AssetMovements => Set<AssetMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssetFlowDbContext).Assembly);
    }
}
