using AssetFlow.Domain.Entities;
using AssetFlow.Domain.Enums;
using AssetFlow.Domain.Exceptions;
using AssetFlow.Domain.ValueObjects;

namespace AssetFlow.Tests.Domain.Entities;

public sealed class AssetTests
{
    [Fact]
    public void Constructor_ShouldCreateAvailableAsset_WhenDataIsValid()
    {
        var asset = new Asset(
            new AssetCode("PAT-0001"),
            new AssetName("Notebook Dell"),
            new SerialNumber("SN-123"),
            new AssetDescription("Notebook para uso administrativo."),
            AssetCondition.Good);

        Assert.Equal("PAT-0001", asset.Code.Value);
        Assert.Equal("Notebook Dell", asset.Name.Value);
        Assert.Equal("SN-123", asset.SerialNumber?.Value);
        Assert.Equal("Notebook para uso administrativo.", asset.Description?.Value);
        Assert.Equal(AssetCondition.Good, asset.Condition);
        Assert.Equal(AssetStatus.Available, asset.Status);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNameIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Asset(new AssetCode("PAT-0001"), null!));
    }

    [Fact]
    public void UpdateBasicInformation_ShouldReplaceValueObjects_WhenAssetCanBeModified()
    {
        var asset = CreateAsset();
        var name = new AssetName("Monitor Dell");
        var serialNumber = new SerialNumber("SN-456");
        var description = new AssetDescription("Monitor para design.");

        asset.UpdateBasicInformation(name, serialNumber, description);

        Assert.Same(name, asset.Name);
        Assert.Same(serialNumber, asset.SerialNumber);
        Assert.Same(description, asset.Description);
    }

    [Fact]
    public void UpdateBasicInformation_ShouldThrow_WhenNameIsNull()
    {
        var asset = CreateAsset();

        Assert.Throws<ArgumentNullException>(() =>
            asset.UpdateBasicInformation(null!, null, null));
    }

    [Fact]
    public void Reserve_ShouldChangeStatusToReserved_WhenAssetIsAvailable()
    {
        var asset = CreateAsset();

        asset.Reserve();

        Assert.Equal(AssetStatus.Reserved, asset.Status);
    }

    [Fact]
    public void CancelReservation_ShouldChangeStatusToAvailable_WhenAssetIsReserved()
    {
        var asset = CreateAsset();
        asset.Reserve();

        asset.CancelReservation();

        Assert.Equal(AssetStatus.Available, asset.Status);
    }

    [Fact]
    public void AssignToDepartment_ShouldAssignAssetAndRecordMovement()
    {
        var asset = CreateAsset();
        var department = CreateDepartment("Tecnologia");
        var occurredAt = new DateTimeOffset(2026, 9, 3, 14, 30, 0, TimeSpan.FromHours(-4));

        asset.AssignToDepartment(department, occurredAt, "  Entregue ao setor.  ");

        Assert.Equal(AssetStatus.Assigned, asset.Status);
        Assert.Equal(department.Id, asset.DepartmentId);
        var movement = Assert.Single(asset.Movements);
        Assert.Equal(AssetMovementType.Assignment, movement.Type);
        Assert.Null(movement.FromDepartmentId);
        Assert.Equal(department.Id, movement.ToDepartmentId);
        Assert.Equal(occurredAt.ToUniversalTime(), movement.OccurredAtUtc);
        Assert.Equal("Entregue ao setor.", movement.Notes);
    }

    [Fact]
    public void AssignToDepartment_ShouldThrow_WhenAssetIsNotAvailable()
    {
        var asset = CreateAsset();
        asset.Reserve();

        var exception = Assert.Throws<InvalidAssetStatusTransitionException>(() =>
            asset.AssignToDepartment(CreateDepartment("Tecnologia"), DateTimeOffset.UtcNow));

        Assert.Equal(AssetStatus.Reserved, exception.CurrentStatus);
        Assert.Equal(AssetStatus.Assigned, exception.TargetStatus);
        Assert.Equal(AssetStatus.Available, exception.ExpectedCurrentStatus);
        Assert.Empty(asset.Movements);
    }

    [Fact]
    public void TransferToDepartment_ShouldUpdateDepartmentAndPreserveHistory()
    {
        var asset = CreateAsset();
        var technology = CreateDepartment("Tecnologia");
        var finance = CreateDepartment("Financeiro");
        asset.AssignToDepartment(technology, DateTimeOffset.UtcNow);

        asset.TransferToDepartment(finance, DateTimeOffset.UtcNow, "Mudança de setor");

        Assert.Equal(AssetStatus.Assigned, asset.Status);
        Assert.Equal(finance.Id, asset.DepartmentId);
        Assert.Equal(2, asset.Movements.Count);
        var movement = asset.Movements.Last();
        Assert.Equal(AssetMovementType.Transfer, movement.Type);
        Assert.Equal(technology.Id, movement.FromDepartmentId);
        Assert.Equal(finance.Id, movement.ToDepartmentId);
    }

    [Fact]
    public void TransferToDepartment_ShouldThrow_WhenDestinationIsCurrentDepartment()
    {
        var asset = CreateAsset();
        var department = CreateDepartment("Tecnologia");
        asset.AssignToDepartment(department, DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            asset.TransferToDepartment(department, DateTimeOffset.UtcNow));

        Assert.Single(asset.Movements);
    }

    [Fact]
    public void ReturnToInventory_ShouldClearDepartmentAndRecordMovement()
    {
        var asset = CreateAsset();
        var department = CreateDepartment("Tecnologia");
        asset.AssignToDepartment(department, DateTimeOffset.UtcNow);

        asset.ReturnToInventory(DateTimeOffset.UtcNow, "Devolvido");

        Assert.Equal(AssetStatus.Available, asset.Status);
        Assert.Null(asset.DepartmentId);
        Assert.Equal(2, asset.Movements.Count);
        var movement = asset.Movements.Last();
        Assert.Equal(AssetMovementType.Return, movement.Type);
        Assert.Equal(department.Id, movement.FromDepartmentId);
        Assert.Null(movement.ToDepartmentId);
    }

    [Fact]
    public void Movement_ShouldThrow_WhenNotesExceedMaximumLength()
    {
        var asset = CreateAsset();

        Assert.Throws<ArgumentException>(() =>
            asset.AssignToDepartment(
                CreateDepartment("Tecnologia"),
                DateTimeOffset.UtcNow,
                new string('A', 501)));

        Assert.Equal(AssetStatus.Available, asset.Status);
        Assert.Null(asset.DepartmentId);
        Assert.Empty(asset.Movements);
    }

    [Fact]
    public void SendToMaintenance_ShouldChangeStatus_WhenAssetIsAvailable()
    {
        var asset = CreateAsset();

        asset.SendToMaintenance();

        Assert.Equal(AssetStatus.UnderMaintenance, asset.Status);
    }

    [Fact]
    public void CompleteMaintenance_ShouldReturnAssetToAvailable()
    {
        var asset = CreateAsset();
        asset.SendToMaintenance();

        asset.CompleteMaintenance(AssetCondition.Good);

        Assert.Equal(AssetStatus.Available, asset.Status);
        Assert.Equal(AssetCondition.Good, asset.Condition);
    }

    [Fact]
    public void CompleteMaintenance_ShouldThrow_WhenAssetIsNotUnderMaintenance()
    {
        var asset = CreateAsset();

        var exception = Assert.Throws<InvalidAssetStatusTransitionException>(() => asset.CompleteMaintenance(AssetCondition.Good));

        Assert.Equal(AssetStatus.Available, exception.CurrentStatus);

        Assert.Equal(AssetStatus.Available, exception.TargetStatus);
        Assert.Equal(AssetStatus.UnderMaintenance, exception.ExpectedCurrentStatus);
    }

    [Fact]
    public void MarkAsLost_ShouldChangeStatusToLost_WhenAssetIsAvailable()
    {
        var asset = CreateAsset();

        asset.MarkAsLost();

        Assert.Equal(AssetStatus.Lost, asset.Status);
    }

    [Fact]
    public void MarkAsLost_ShouldChangeStatusToLost_WhenAssetIsReserved()
    {
        var asset = CreateAsset();
        asset.Reserve();

        asset.MarkAsLost();

        Assert.Equal(AssetStatus.Lost, asset.Status);
    }

    [Fact]
    public void Recover_ShouldReturnAssetToAvailableAndUpdateCondition()
    {
        var asset = CreateAsset();
        asset.MarkAsLost();

        asset.Recover(AssetCondition.Fair);

        Assert.Equal(AssetStatus.Available, asset.Status);
        Assert.Equal(AssetCondition.Fair, asset.Condition);
    }

    [Fact]
    public void Recover_ShouldThrow_WhenAssetIsNotLost()
    {
        var asset = CreateAsset();

        var exception = Assert.Throws<InvalidAssetStatusTransitionException>(() => asset.Recover(AssetCondition.Good));

        Assert.Equal(AssetStatus.Available, exception.CurrentStatus);

        Assert.Equal(AssetStatus.Available, exception.TargetStatus);
        Assert.Equal(AssetStatus.Lost, exception.ExpectedCurrentStatus);
    }

    [Fact]
    public void Retire_ShouldChangeStatusToRetired_WhenAssetIsAvailable()
    {
        var asset = CreateAsset();

        asset.Retire();

        Assert.Equal(AssetStatus.Retired, asset.Status);
    }

    [Fact]
    public void Retire_ShouldChangeStatusToRetired_WhenAssetIsUnderMaintenance()
    {
        var asset = CreateAsset();
        asset.SendToMaintenance();

        asset.Retire();

        Assert.Equal(AssetStatus.Retired, asset.Status);
    }

    [Fact]
    public void Reserve_ShouldThrow_WhenAssetIsUnderMaintenance()
    {
        var asset = CreateAsset();
        asset.SendToMaintenance();

        var exception = Assert.Throws<InvalidAssetStatusTransitionException>(asset.Reserve);

        Assert.Equal(AssetStatus.UnderMaintenance, exception.CurrentStatus);

        Assert.Equal(AssetStatus.Reserved, exception.TargetStatus);
    }

    [Fact]
    public void Dispose_ShouldThrow_WhenAssetIsNotRetired()
    {
        var asset = CreateAsset();

        var exception = Assert.Throws<InvalidAssetStatusTransitionException>(asset.Dispose);

        Assert.Equal(AssetStatus.Available, exception.CurrentStatus);

        Assert.Equal(AssetStatus.Disposed, exception.TargetStatus);
    }

    [Fact]
    public void Dispose_ShouldChangeStatus_WhenAssetIsRetired()
    {
        var asset = CreateAsset();
        asset.Retire();

        asset.Dispose();

        Assert.Equal(AssetStatus.Disposed, asset.Status);
    }

    [Fact]
    public void UpdateBasicInformation_ShouldThrow_WhenAssetIsDisposed()
    {
        var asset = CreateAsset();
        asset.Retire();
        asset.Dispose();

        Assert.Throws<InvalidOperationException>(() => asset.UpdateBasicInformation(new AssetName("Novo nome"), null, null));
    }

    private static Asset CreateAsset()
    {
        return new Asset(
            new AssetCode("PAT-0001"),
            new AssetName("Notebook Dell"),
            condition: AssetCondition.Good);
    }

    private static Department CreateDepartment(string name)
    {
        return new Department(new DepartmentName(name));
    }
}
