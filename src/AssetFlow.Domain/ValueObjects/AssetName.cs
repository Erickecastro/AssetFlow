namespace AssetFlow.Domain.ValueObjects;

public sealed class AssetName : IEquatable<AssetName>
{
    public string Value { get; }

    public AssetName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Asset name cannot be empty.", nameof(value));
        }

        value = value.Trim();

        if (value.Length > 150)
        {
            throw new ArgumentException("Asset name cannot exceed 150 characters.", nameof(value));
        }

        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }

    public bool Equals(AssetName? other)
    {
        return other is not null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as AssetName);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(AssetName? left, AssetName? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(AssetName? left, AssetName? right)
    {
        return !(left == right);
    }

    public static implicit operator string(AssetName value)
    {
        return value.Value;
    }

    public static explicit operator AssetName(string value)
    {
        return new AssetName(value);
    }
}