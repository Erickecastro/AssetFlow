using AssetFlow.Domain.Enums;

namespace AssetFlow.Application.Assets;

public sealed record CreateAssetCommand(
    string Code,
    string Name,
    string? SerialNumber,
    string? Description,
    Guid? DepartmentId = null,
    AssetCondition Condition = AssetCondition.New);

public sealed record MoveAssetCommand(
    Guid DepartmentId,
    string? Notes = null);

public sealed record ReturnAssetCommand(string? Notes = null);

public sealed record UpdateAssetCommand(
    string Name,
    string? SerialNumber,
    string? Description,
    AssetCondition Condition);

public sealed record AssetMovementDto(
    Guid Id,
    AssetMovementType Type,
    Guid? FromDepartmentId,
    Guid? ToDepartmentId,
    DateTimeOffset OccurredAtUtc,
    string? Notes);

public sealed record AssetDto(
    Guid Id,
    string Code,
    string Name,
    string? SerialNumber,
    string? Description,
    AssetStatus Status,
    AssetCondition Condition,
    Guid? DepartmentId,
    string QrCodeValue,
    IReadOnlyList<AssetMovementDto> Movements);
