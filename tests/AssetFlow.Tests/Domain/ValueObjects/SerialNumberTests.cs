using AssetFlow.Domain.ValueObjects;

namespace AssetFlow.Tests.Domain.ValueObjects;

public sealed class SerialNumberTests
{
    [Fact]
    public void Constructor_ShouldNormalizeValue_WhenValueHasSpacesAndLowercaseLetters()
    {
        var serialNumber = new SerialNumber("  sn-123  ");

        Assert.Equal("SN-123", serialNumber.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_ShouldThrow_WhenValueIsEmpty(string value)
    {
        Assert.Throws<ArgumentException>(() => new SerialNumber(value));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenValueExceedsMaximumLength()
    {
        var value = new string('A', 101);

        Assert.Throws<ArgumentException>(() => new SerialNumber(value));
    }

    [Fact]
    public void Equality_ShouldReturnTrue_WhenNormalizedValuesAreEqual()
    {
        var first = new SerialNumber("SN-123");
        var second = new SerialNumber(" sn-123 ");

        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.False(first != second);
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenValuesAreDifferent()
    {
        var first = new SerialNumber("SN-123");
        var second = new SerialNumber("SN-456");

        Assert.NotEqual(first, second);
        Assert.False(first == second);
        Assert.True(first != second);
    }

    [Fact]
    public void ToString_ShouldReturnNormalizedValue()
    {
        var serialNumber = new SerialNumber("sn-123");

        Assert.Equal("SN-123", serialNumber.ToString());
    }

    [Fact]
    public void ImplicitConversion_ShouldReturnStringValue()
    {
        var serialNumber = new SerialNumber("SN-123");

        string value = serialNumber;

        Assert.Equal("SN-123", value);
    }

    [Fact]
    public void ExplicitConversion_ShouldCreateSerialNumber()
    {
        var serialNumber = (SerialNumber)"sn-123";

        Assert.Equal("SN-123", serialNumber.Value);
    }
}