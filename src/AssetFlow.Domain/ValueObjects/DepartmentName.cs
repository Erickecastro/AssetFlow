namespace AssetFlow.Domain.ValueObjects;

public sealed class DepartmentName : IEquatable<DepartmentName>
{
    public string Value { get; }

    public DepartmentName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Department name cannot be empty.", nameof(value));
        }

        value = value.Trim();

        if (value.Length > 150)
        {
            throw new ArgumentException("Department name cannot exceed 150 characters.", nameof(value));
        }

        Value = value;
    }

    public bool Equals(DepartmentName? other)
    {
        return other is not null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as DepartmentName);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(DepartmentName? left, DepartmentName? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(DepartmentName? left, DepartmentName? right)
    {
        return !(left == right);
    }

    public static implicit operator string(DepartmentName name)
    {
        return name.Value;
    }

    public static explicit operator DepartmentName(string value)
    {
        return new DepartmentName(value);
    }
}
