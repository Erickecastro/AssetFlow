using AssetFlow.Domain.Common;
using AssetFlow.Domain.Enums;

namespace AssetFlow.Domain.Entities;

public sealed class AssetMovement : Entity
{
    public AssetMovementType Type { get; private set; }

    public Guid? FromDepartmentId { get; private set; }

    public Guid? ToDepartmentId { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public string? Notes { get; private set; }

    private AssetMovement()
    {
    }

    internal AssetMovement(
        AssetMovementType type,
        Guid? fromDepartmentId,
        Guid? toDepartmentId,
        DateTimeOffset occurredAt,
        string? notes)
    {
        Type = type;
        FromDepartmentId = fromDepartmentId;
        ToDepartmentId = toDepartmentId;
        OccurredAtUtc = occurredAt.ToUniversalTime();
        Notes = NormalizeNotes(notes);
    }

    private static string? NormalizeNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return null;
        }

        notes = notes.Trim();

        if (notes.Length > 500)
        {
            throw new ArgumentException(
                "Asset movement notes cannot exceed 500 characters.",
                nameof(notes));
        }

        return notes;
    }
}
