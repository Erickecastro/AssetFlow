using AssetFlow.Application.Abstractions.Persistence;
using AssetFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Infrastructure.Persistence.Repositories;

internal sealed class AssetRepository : IAssetRepository
{
    private readonly AssetFlowDbContext _dbContext;

    public AssetRepository(AssetFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Asset asset, CancellationToken cancellationToken = default)
    {
        _dbContext.Assets.Add(asset);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Asset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Assets
            .Include(asset => asset.Movements)
            .SingleOrDefaultAsync(asset => asset.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Asset>> ListAsync(CancellationToken cancellationToken = default)
    {
        var assets = await _dbContext.Assets
            .AsNoTracking()
            .Include(asset => asset.Movements)
            .ToListAsync(cancellationToken);

        return assets.OrderBy(asset => asset.Code.Value).ToArray();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Remove(Asset asset) => _dbContext.Assets.Remove(asset);
}
