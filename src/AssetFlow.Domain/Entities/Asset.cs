using AssetFlow.Domain.Common;
using AssetFlow.Domain.Enums;
using AssetFlow.Domain.Exceptions;
using AssetFlow.Domain.ValueObjects;

namespace AssetFlow.Domain.Entities;

public sealed class Asset : Entity
{
    public AssetCode Code { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public SerialNumber? SerialNumber { get; private set; }

    public AssetStatus Status { get; private set; }

    public AssetCondition Condition { get; private set; }

    private Asset()
    {
        Code = null!;
        Name = string.Empty;
    }

    public Asset(
        AssetCode code, string name, SerialNumber? serialNumber = null, string? description = null, AssetCondition condition = AssetCondition.New)
    {
        ArgumentNullException.ThrowIfNull(code);

        Code = code;
        Name = NormalizeName(name);
        SerialNumber = serialNumber;
        Description = NormalizeDescription(description);
        Condition = ValidateCondition(condition);
        Status = AssetStatus.Available;
    }

    public void UpdateBasicInformation(string name, SerialNumber? serialNumber, string? description)
    {
        EnsureNotDisposed();

        Name = NormalizeName(name);
        SerialNumber = serialNumber;
        Description = NormalizeDescription(description);
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

    public void SendToMaintenance()
    {
        ChangeStatus(
            AssetStatus.UnderMaintenance,
            AssetStatus.Available,
            AssetStatus.Reserved);
    }

    public void CompleteMaintenance(AssetCondition condition)
    {
        EnsureCurrentStatus(AssetStatus.UnderMaintenance);

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
        EnsureCurrentStatus(AssetStatus.Lost);

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

    private void EnsureCurrentStatus(AssetStatus expectedStatus)
    {
        if (Status != expectedStatus)
        {
            throw new InvalidAssetStatusTransitionException(
                Status,
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

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Asset name cannot be empty.", nameof(name));
        }

        name = name.Trim();

        if (name.Length > 150)
        {
            throw new ArgumentException("Asset name cannot exceed 150 characters.", nameof(name));
        }

        return name;
    }

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        description = description.Trim();

        if (description.Length > 500)
        {
            throw new ArgumentException("Asset description cannot exceed 500 characters.", nameof(description));
        }

        return description;
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