using AssetFlow.Domain.Entities;

namespace AssetFlow.Application.Abstractions.Persistence;

public interface IAssetRepository
{
    Task AddAsync(Asset asset, CancellationToken cancellationToken = default);

    Task<Asset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Asset>> ListAsync(CancellationToken cancellationToken = default);

    void Remove(Asset asset) => throw new NotSupportedException();

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
