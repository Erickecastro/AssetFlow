using AssetFlow.Domain.Common;
using AssetFlow.Domain.Enums;
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
        AssetCode code,
        string name,
        SerialNumber? serialNumber = null,
        string? description = null,
        AssetCondition condition = AssetCondition.New)
    {
        ArgumentNullException.ThrowIfNull(code);

        Code = code;
        Name = NormalizeName(name);
        SerialNumber = serialNumber;
        Description = NormalizeDescription(description);
        Condition = ValidateCondition(condition);
        Status = AssetStatus.Available;
    }

    public void UpdateBasicInformation(
        string name,
        SerialNumber? serialNumber,
        string? description)
    {
        Name = NormalizeName(name);
        SerialNumber = serialNumber;
        Description = NormalizeDescription(description);
    }

    public void ChangeCondition(AssetCondition condition)
    {
        Condition = ValidateCondition(condition);
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Asset name cannot be empty.",
                nameof(name));
        }

        name = name.Trim();

        if (name.Length > 150)
        {
            throw new ArgumentException(
                "Asset name cannot exceed 150 characters.",
                nameof(name));
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
            throw new ArgumentException(
                "Asset description cannot exceed 500 characters.",
                nameof(description));
        }

        return description;
    }

    private static AssetCondition ValidateCondition(
        AssetCondition condition)
    {
        if (!Enum.IsDefined(condition))
        {
            throw new ArgumentOutOfRangeException(
                nameof(condition),
                condition,
                "Invalid asset condition.");
        }

        return condition;
    }
}