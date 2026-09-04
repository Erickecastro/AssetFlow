using AssetFlow.Application.Abstractions.Persistence;
using AssetFlow.Application.Assets;
using AssetFlow.Domain.Entities;
using AssetFlow.Domain.Enums;
using AssetFlow.Domain.ValueObjects;

namespace AssetFlow.Tests.Application.Assets;

public sealed class AssetServiceTests
{
    [Fact]
    public async Task CreateAsync_WithDepartment_ShouldCreateAssignedAsset()
    {
        var occurredAt = new DateTimeOffset(2026, 9, 4, 12, 0, 0, TimeSpan.Zero);
        var department = new Department(new DepartmentName("Tecnologia"), "Segundo andar");
        var assetRepository = new AssetRepositorySpy();
        var service = new AssetService(
            assetRepository,
            new DepartmentRepositoryStub(department),
            new TimeProviderStub(occurredAt));

        var result = await service.CreateAsync(new CreateAssetCommand(
            "PAT-0001",
            "Notebook",
            "SN-123",
            "Equipamento de desenvolvimento",
            department.Id));

        Assert.NotNull(assetRepository.AddedAsset);
        Assert.Equal(AssetStatus.Assigned, result.Status);
        Assert.Equal(department.Id, result.DepartmentId);
        var movement = Assert.Single(result.Movements);
        Assert.Equal(AssetMovementType.Assignment, movement.Type);
        Assert.Equal(department.Id, movement.ToDepartmentId);
        Assert.Equal(occurredAt, movement.OccurredAtUtc);
    }

    private sealed class AssetRepositorySpy : IAssetRepository
    {
        public Asset? AddedAsset { get; private set; }

        public Task AddAsync(Asset asset, CancellationToken cancellationToken = default)
        {
            AddedAsset = asset;
            return Task.CompletedTask;
        }

        public Task<Asset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<Asset?>(null);

        public Task<IReadOnlyList<Asset>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Asset>>([]);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class DepartmentRepositoryStub(Department department) : IDepartmentRepository
    {
        public Task AddAsync(Department value, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<Department?>(id == department.Id ? department : null);

        public Task<IReadOnlyList<Department>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Department>>([department]);
    }

    private sealed class TimeProviderStub(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
