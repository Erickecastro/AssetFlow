using AssetFlow.Domain.Common;
using AssetFlow.Domain.ValueObjects;

namespace AssetFlow.Domain.Entities;

public sealed class Department : Entity
{
    public DepartmentName Name { get; private set; }

    public string? Description { get; private set; }

    private Department()
    {
        Name = null!;
    }

    public Department(DepartmentName name, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(name);

        Name = name;
        Description = NormalizeDescription(description);
    }

    public void Update(DepartmentName name, string? description)
    {
        ArgumentNullException.ThrowIfNull(name);

        Name = name;
        Description = NormalizeDescription(description);
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
                "Department description cannot exceed 500 characters.",
                nameof(description));
        }

        return description;
    }
}
