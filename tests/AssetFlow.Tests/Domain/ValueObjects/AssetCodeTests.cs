using AssetFlow.Domain.ValueObjects;

namespace AssetFlow.Tests.Domain.ValueObjects;

public sealed class AssetCodeTests
{
    [Fact]
    public void Constructor_ShouldNormalizeValue_WhenValueHasSpacesAndLowercaseLetters()
    {
        var assetCode = new AssetCode("  pat-0001  ");

        Assert.Equal("PAT-0001", assetCode.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_ShouldThrow_WhenValueIsEmpty(string value)
    {
        Assert.Throws<ArgumentException>(() => new AssetCode(value));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenValueExceedsMaximumLength()
    {
        var value = new string('A', 31);

        Assert.Throws<ArgumentException>(() => new AssetCode(value));
    }

    [Fact]
    public void Equality_ShouldReturnTrue_WhenNormalizedValuesAreEqual()
    {
        var first = new AssetCode("PAT-0001");
        var second = new AssetCode(" pat-0001 ");

        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.False(first != second);
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenValuesAreDifferent()
    {
        var first = new AssetCode("PAT-0001");
        var second = new AssetCode("PAT-0002");

        Assert.NotEqual(first, second);
        Assert.False(first == second);
        Assert.True(first != second);
    }

    [Fact]
    public void ToString_ShouldReturnNormalizedValue()
    {
        var assetCode = new AssetCode("pat-0001");

        Assert.Equal("PAT-0001", assetCode.ToString());
    }

    [Fact]
    public void ImplicitConversion_ShouldReturnStringValue()
    {
        var assetCode = new AssetCode("PAT-0001");

        string value = assetCode;

        Assert.Equal("PAT-0001", value);
    }

    [Fact]
    public void ExplicitConversion_ShouldCreateAssetCode()
    {
        var assetCode = (AssetCode)"pat-0001";

        Assert.Equal("PAT-0001", assetCode.Value);
    }
}