namespace AssetFlow.Domain.ValueObjects;

public sealed class AssetCode : IEquatable<AssetCode>
{
    public string Value { get; }

    public AssetCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Asset code cannot be empty.", nameof(value));

        value = value.Trim().ToUpperInvariant();

        if (value.Length > 30)
            throw new ArgumentException("Asset code cannot exceed 30 characters.", nameof(value));

        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }

    public bool Equals(AssetCode? other)
    {
        if (other is null)
            return false;

        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as AssetCode);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(AssetCode? left, AssetCode? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(AssetCode? left, AssetCode? right)
    {
        return !(left == right);
    }

    public static implicit operator string(AssetCode assetCode)
    {
        return assetCode.Value;
    }

    public static explicit operator AssetCode(string value)
    {
        return new AssetCode(value);
    }
}