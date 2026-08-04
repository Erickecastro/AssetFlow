namespace AssetFlow.Domain.ValueObjects;

public sealed class SerialNumber : IEquatable<SerialNumber>
{
    public string Value { get; }

    public SerialNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Serial number cannot be null or whitespace.", nameof(value));
        }

        value = value.Trim().ToUpperInvariant();

        if (value.Length > 100)
        {
            throw new ArgumentException("Serial number cannot exceed 100 characters.", nameof(value));
        }

        Value = value;
    }

    public bool Equals(SerialNumber? other)
    {
        if (other is null)
        {
            return false;
        }

        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as SerialNumber);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(
        SerialNumber? left,
        SerialNumber? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(SerialNumber? left, SerialNumber? right)
    {
        return !(left == right);
    }

    public static implicit operator string(SerialNumber serialNumber)
    {
        return serialNumber.Value;
    }

    public static explicit operator SerialNumber(string value)
    {
        return new SerialNumber(value);
    }
}