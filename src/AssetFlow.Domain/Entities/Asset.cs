using AssetFlow.Domain.Common;
using AssetFlow.Domain.Enums;
using AssetFlow.Domain.Exceptions;
using AssetFlow.Domain.ValueObjects;

namespace AssetFlow.Domain.Entities;

public sealed class Asset : Entity
{
    private readonly List<AssetMovement> _movements = [];

    public AssetCode Code { get; private set; }

    public AssetName Name { get; private set; }

    public AssetDescription? Description { get; private set; }

    public SerialNumber? SerialNumber { get; private set; }

    public AssetStatus Status { get; private set; }

    public AssetCondition Condition { get; private set; }

    public Guid? DepartmentId { get; private set; }

    public IReadOnlyCollection<AssetMovement> Movements => _movements.AsReadOnly();

    private Asset()
    {
        Code = null!;
        Name = null!;
    }

    public Asset(
        AssetCode code,
        AssetName name,
        SerialNumber? serialNumber = null,
        AssetDescription? description = null,
        AssetCondition condition = AssetCondition.New)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(name);

        Code = code;
        Name = name;
        SerialNumber = serialNumber;
        Description = description;
        Condition = ValidateCondition(condition);
        Status = AssetStatus.Available;
    }

    public void UpdateBasicInformation(
        AssetName name,
        SerialNumber? serialNumber,
        AssetDescription? description)
    {
        EnsureNotDisposed();
        ArgumentNullException.ThrowIfNull(name);

        Name = name;
        SerialNumber = serialNumber;
        Description = description;
    }

    public void ChangeCondition(AssetCondition condition)
    {
        EnsureNotDisposed();

        Condition = ValidateCondition(condition);
    }

    public void Reserve()
    {
        ChangeStatus(AssetStatus.Reserved, AssetStatus.Available);
    }

    public void CancelReservation()
    {
        ChangeStatus(
            AssetStatus.Available,
            AssetStatus.Reserved);
    }

    public void AssignToDepartment(
        Department department,
        DateTimeOffset occurredAt,
        string? notes = null)
    {
        ArgumentNullException.ThrowIfNull(department);
        EnsureCurrentStatus(AssetStatus.Available, AssetStatus.Assigned);

        var movement = new AssetMovement(
            AssetMovementType.Assignment,
            null,
            department.Id,
            occurredAt,
            notes);

        DepartmentId = department.Id;
        Status = AssetStatus.Assigned;
        _movements.Add(movement);
    }

    public void TransferToDepartment(
        Department department,
        DateTimeOffset occurredAt,
        string? notes = null)
    {
        ArgumentNullException.ThrowIfNull(department);
        EnsureCurrentStatus(AssetStatus.Assigned, AssetStatus.Assigned);

        if (DepartmentId == department.Id)
        {
            throw new InvalidOperationException(
                "The asset is already assigned to this department.");
        }

        var previousDepartmentId = DepartmentId;
        var movement = new AssetMovement(
            AssetMovementType.Transfer,
            previousDepartmentId,
            department.Id,
            occurredAt,
            notes);

        DepartmentId = department.Id;
        _movements.Add(movement);
    }

    public void ReturnToInventory(
        DateTimeOffset occurredAt,
        string? notes = null)
    {
        EnsureCurrentStatus(AssetStatus.Assigned, AssetStatus.Available);

        var previousDepartmentId = DepartmentId;
        var movement = new AssetMovement(
            AssetMovementType.Return,
            previousDepartmentId,
            null,
            occurredAt,
            notes);

        DepartmentId = null;
        Status = AssetStatus.Available;
        _movements.Add(movement);
    }

    public void SendToMaintenance()
    {
        ChangeStatus(
            AssetStatus.UnderMaintenance,
            AssetStatus.Available,
            AssetStatus.Reserved);
    }

    public void CompleteMaintenance(AssetCondition condition)
    {
        EnsureCurrentStatus(
            AssetStatus.UnderMaintenance,
            AssetStatus.Available);

        Condition = ValidateCondition(condition);
        Status = AssetStatus.Available;
    }

    public void MarkAsLost()
    {
        ChangeStatus(
            AssetStatus.Lost,
            AssetStatus.Available,
            AssetStatus.Reserved);
    }

    public void Recover(AssetCondition condition)
    {
        EnsureCurrentStatus(
            AssetStatus.Lost,
            AssetStatus.Available);

        Condition = ValidateCondition(condition);
        Status = AssetStatus.Available;
    }

    public void Retire()
    {
        ChangeStatus(AssetStatus.Retired, AssetStatus.Available, AssetStatus.UnderMaintenance, AssetStatus.Lost);
    }

    public void Dispose()
    {
        ChangeStatus(AssetStatus.Disposed, AssetStatus.Retired);
    }

    private void ChangeStatus(AssetStatus targetStatus, params AssetStatus[] allowedCurrentStatuses)
    {
        if (!allowedCurrentStatuses.Contains(Status))
        {
            throw new InvalidAssetStatusTransitionException(Status, targetStatus);
        }

        Status = targetStatus;
    }

    private void EnsureCurrentStatus(
        AssetStatus expectedStatus,
        AssetStatus targetStatus)
    {
        if (Status != expectedStatus)
        {
            throw new InvalidAssetStatusTransitionException(
                Status,
                targetStatus,
                expectedStatus);
        }
    }

    private void EnsureNotDisposed()
    {
        if (Status == AssetStatus.Disposed)
        {
            throw new InvalidOperationException("A disposed asset cannot be modified.");
        }
    }

    private static AssetCondition ValidateCondition(
        AssetCondition condition)
    {
        if (!Enum.IsDefined(condition))
        {
            throw new ArgumentOutOfRangeException(nameof(condition), condition, "Invalid asset condition.");
        }

        return condition;
    }
}
