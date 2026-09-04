using AssetFlow.Application.Abstractions.Persistence;
using AssetFlow.Application.Common;
using AssetFlow.Domain.Entities;
using AssetFlow.Domain.ValueObjects;

namespace AssetFlow.Application.Assets;

public sealed class AssetService
{
    private readonly IAssetRepository _assets;
    private readonly IDepartmentRepository _departments;
    private readonly TimeProvider _timeProvider;

    public AssetService(
        IAssetRepository assets,
        IDepartmentRepository departments,
        TimeProvider timeProvider)
    {
        _assets = assets;
        _departments = departments;
        _timeProvider = timeProvider;
    }

    public async Task<AssetDto> CreateAsync(
        CreateAssetCommand command,
        CancellationToken cancellationToken = default)
    {
        var asset = new Asset(
            new AssetCode(command.Code),
            new AssetName(command.Name),
            string.IsNullOrWhiteSpace(command.SerialNumber)
                ? null
                : new SerialNumber(command.SerialNumber),
            string.IsNullOrWhiteSpace(command.Description)
                ? null
                : new AssetDescription(command.Description),
            command.Condition);

        if (command.DepartmentId is Guid departmentId)
        {
            var department = await GetDepartmentAsync(departmentId, cancellationToken);
            asset.AssignToDepartment(
                department,
                _timeProvider.GetUtcNow(),
                "Atribuição inicial realizada no cadastro do ativo.");
        }

        await _assets.AddAsync(asset, cancellationToken);
        return Map(asset);
    }

    public async Task<IReadOnlyList<AssetDto>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var assets = await _assets.ListAsync(cancellationToken);
        return assets.Select(Map).ToArray();
    }

    public async Task<AssetDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return Map(await GetAssetAsync(id, cancellationToken));
    }

    public async Task<AssetDto> UpdateAsync(
        Guid id,
        UpdateAssetCommand command,
        CancellationToken cancellationToken = default)
    {
        var asset = await GetAssetAsync(id, cancellationToken);
        asset.UpdateBasicInformation(
            new AssetName(command.Name),
            string.IsNullOrWhiteSpace(command.SerialNumber) ? null : new SerialNumber(command.SerialNumber),
            string.IsNullOrWhiteSpace(command.Description) ? null : new AssetDescription(command.Description));
        asset.ChangeCondition(command.Condition);
        await _assets.SaveChangesAsync(cancellationToken);
        return Map(asset);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var asset = await GetAssetAsync(id, cancellationToken);
        _assets.Remove(asset);
        await _assets.SaveChangesAsync(cancellationToken);
    }

    public async Task<AssetDto> AssignAsync(
        Guid id,
        MoveAssetCommand command,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default)
    {
        var asset = await GetAssetAsync(id, cancellationToken);
        var department = await GetDepartmentAsync(command.DepartmentId, cancellationToken);
        asset.AssignToDepartment(department, occurredAt, command.Notes);
        await _assets.SaveChangesAsync(cancellationToken);
        return Map(asset);
    }

    public async Task<AssetDto> TransferAsync(
        Guid id,
        MoveAssetCommand command,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default)
    {
        var asset = await GetAssetAsync(id, cancellationToken);
        var department = await GetDepartmentAsync(command.DepartmentId, cancellationToken);
        asset.TransferToDepartment(department, occurredAt, command.Notes);
        await _assets.SaveChangesAsync(cancellationToken);
        return Map(asset);
    }

    public async Task<AssetDto> ReturnAsync(
        Guid id,
        ReturnAssetCommand command,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default)
    {
        var asset = await GetAssetAsync(id, cancellationToken);
        asset.ReturnToInventory(occurredAt, command.Notes);
        await _assets.SaveChangesAsync(cancellationToken);
        return Map(asset);
    }

    private async Task<Asset> GetAssetAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _assets.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Asset", id);
    }

    private async Task<Department> GetDepartmentAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _departments.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Department", id);
    }

    private static AssetDto Map(Asset asset)
    {
        return new AssetDto(
            asset.Id,
            asset.Code.Value,
            asset.Name.Value,
            asset.SerialNumber?.Value,
            asset.Description?.Value,
            asset.Status,
            asset.Condition,
            asset.DepartmentId,
            $"assetflow:asset:{asset.Id}",
            asset.Movements
                .OrderBy(movement => movement.OccurredAtUtc)
                .Select(movement => new AssetMovementDto(
                    movement.Id,
                    movement.Type,
                    movement.FromDepartmentId,
                    movement.ToDepartmentId,
                    movement.OccurredAtUtc,
                    movement.Notes))
                .ToArray());
    }
}
