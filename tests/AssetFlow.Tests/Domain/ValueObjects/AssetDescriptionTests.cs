using AssetFlow.Domain.ValueObjects;

namespace AssetFlow.Tests.Domain.ValueObjects;

public sealed class AssetDescriptionTests
{
    [Fact]
    public void Constructor_ShouldTrimValue_WhenValueHasExtraSpaces()
    {
        var description =
            new AssetDescription("  Uso administrativo.  ");

        Assert.Equal("Uso administrativo.", description.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_ShouldThrow_WhenValueIsEmpty(string value)
    {
        Assert.Throws<ArgumentException>(
            () => new AssetDescription(value));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenValueExceedsMaximumLength()
    {
        var value = new string('A', 501);

        Assert.Throws<ArgumentException>(
            () => new AssetDescription(value));
    }

    [Fact]
    public void Equality_ShouldReturnTrue_WhenValuesAreEqual()
    {
        var first = new AssetDescription("Uso administrativo.");
        var second = new AssetDescription("Uso administrativo.");

        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.False(first != second);
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenValuesAreDifferent()
    {
        var first = new AssetDescription("Uso administrativo.");
        var second = new AssetDescription("Uso técnico.");

        Assert.NotEqual(first, second);
        Assert.False(first == second);
        Assert.True(first != second);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var description =
            new AssetDescription("Uso administrativo.");

        Assert.Equal(
            "Uso administrativo.",
            description.ToString());
    }

    [Fact]
    public void ImplicitConversion_ShouldReturnStringValue()
    {
        var description =
            new AssetDescription("Uso administrativo.");

        string value = description;

        Assert.Equal("Uso administrativo.", value);
    }

    [Fact]
    public void ExplicitConversion_ShouldCreateAssetDescription()
    {
        var description =
            (AssetDescription)"Uso administrativo.";

        Assert.Equal(
            "Uso administrativo.",
            description.Value);
    }
}