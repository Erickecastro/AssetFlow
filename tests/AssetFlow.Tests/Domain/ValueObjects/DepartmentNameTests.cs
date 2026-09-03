using AssetFlow.Domain.ValueObjects;

namespace AssetFlow.Tests.Domain.ValueObjects;

public sealed class DepartmentNameTests
{
    [Fact]
    public void Constructor_ShouldTrimValue()
    {
        var name = new DepartmentName("  Tecnologia  ");

        Assert.Equal("Tecnologia", name.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrow_WhenValueIsEmpty(string value)
    {
        Assert.Throws<ArgumentException>(() => new DepartmentName(value));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenValueExceedsMaximumLength()
    {
        Assert.Throws<ArgumentException>(() =>
            new DepartmentName(new string('A', 151)));
    }

    [Fact]
    public void Equality_ShouldUseNormalizedValue()
    {
        var first = new DepartmentName("Tecnologia");
        var second = new DepartmentName(" Tecnologia ");

        Assert.Equal(first, second);
        Assert.True(first == second);
    }
}
