using AssetFlow.Domain.ValueObjects;

namespace AssetFlow.Tests.Domain.ValueObjects;

public sealed class AssetNameTests
{
    [Fact]
    public void Constructor_ShouldTrimValue_WhenValueHasExtraSpaces()
    {
        var assetName = new AssetName("  Notebook Dell  ");

        Assert.Equal("Notebook Dell", assetName.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_ShouldThrow_WhenValueIsEmpty(string value)
    {
        Assert.Throws<ArgumentException>(() => new AssetName(value));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenValueExceedsMaximumLength()
    {
        var value = new string('A', 151);

        Assert.Throws<ArgumentException>(() => new AssetName(value));
    }

    [Fact]
    public void Equality_ShouldReturnTrue_WhenValuesAreEqual()
    {
        var first = new AssetName("Notebook Dell");
        var second = new AssetName("Notebook Dell");

        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.False(first != second);
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenValuesAreDifferent()
    {
        var first = new AssetName("Notebook Dell");
        var second = new AssetName("Monitor Dell");

        Assert.NotEqual(first, second);
        Assert.False(first == second);
        Assert.True(first != second);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var assetName = new AssetName("Notebook Dell");

        Assert.Equal("Notebook Dell", assetName.ToString());
    }

    [Fact]
    public void ImplicitConversion_ShouldReturnStringValue()
    {
        var assetName = new AssetName("Notebook Dell");

        string value = assetName;

        Assert.Equal("Notebook Dell", value);
    }

    [Fact]
    public void ExplicitConversion_ShouldCreateAssetName()
    {
        var assetName = (AssetName)"Notebook Dell";

        Assert.Equal("Notebook Dell", assetName.Value);
    }
}