namespace AssetFlow.Domain.ValueObjects;

public sealed class AssetDescription : IEquatable<AssetDescription>
{
    public string Value { get; }

    public AssetDescription(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Asset description cannot be empty.", nameof(value));
        }

        value = value.Trim();

        if (value.Length > 500)
        {
            throw new ArgumentException("Asset description cannot exceed 500 characters.", nameof(value));
        }

        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }

    public bool Equals(AssetDescription? other)
    {
        return other is not null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as AssetDescription);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(AssetDescription? left, AssetDescription? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(AssetDescription? left, AssetDescription? right)
    {
        return !(left == right);
    }

    public static implicit operator string(AssetDescription description)
    {
        return description.Value;
    }

    public static explicit operator AssetDescription(string value)
    {
        return new AssetDescription(value);
    }
}