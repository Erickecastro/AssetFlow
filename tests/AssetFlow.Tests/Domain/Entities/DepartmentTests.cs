using AssetFlow.Domain.Entities;
using AssetFlow.Domain.ValueObjects;

namespace AssetFlow.Tests.Domain.Entities;

public sealed class DepartmentTests
{
    [Fact]
    public void Constructor_ShouldCreateDepartment_WhenDataIsValid()
    {
        var department = new Department(
            new DepartmentName("Tecnologia"),
            "  Segundo andar.  ");

        Assert.Equal("Tecnologia", department.Name.Value);
        Assert.Equal("Segundo andar.", department.Description);
        Assert.NotEqual(Guid.Empty, department.Id);
    }

    [Fact]
    public void Constructor_ShouldNormalizeEmptyDescriptionToNull()
    {
        var department = new Department(new DepartmentName("Estoque"), "  ");

        Assert.Null(department.Description);
    }

    [Fact]
    public void Update_ShouldReplaceDepartmentInformation()
    {
        var department = new Department(new DepartmentName("TI"));
        var name = new DepartmentName("Tecnologia");

        department.Update(name, "Sala principal");

        Assert.Same(name, department.Name);
        Assert.Equal("Sala principal", department.Description);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDescriptionExceedsMaximumLength()
    {
        Assert.Throws<ArgumentException>(() =>
            new Department(
                new DepartmentName("Tecnologia"),
                new string('A', 501)));
    }
}
