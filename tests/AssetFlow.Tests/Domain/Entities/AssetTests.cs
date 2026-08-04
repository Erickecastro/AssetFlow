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
        var asset = new Asset(new AssetCode("PAT-0001"), "Notebook Dell", new SerialNumber("SN-123"), "Notebook para uso administrativo.", AssetCondition.Good);

        Assert.Equal("PAT-0001", asset.Code.Value);
        Assert.Equal("Notebook Dell", asset.Name);
        Assert.Equal("SN-123", asset.SerialNumber?.Value);
        Assert.Equal("Notebook para uso administrativo.", asset.Description);
        Assert.Equal(AssetCondition.Good, asset.Condition);
        Assert.Equal(AssetStatus.Available, asset.Status);
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

        Assert.Equal(AssetStatus.UnderMaintenance, exception.TargetStatus);
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

        Assert.Equal(AssetStatus.Lost, exception.TargetStatus);
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

        Assert.Throws<InvalidOperationException>(() => asset.UpdateBasicInformation("Novo nome", null, null));
    }

    private static Asset CreateAsset()
    {
        return new Asset(new AssetCode("PAT-0001"), "Notebook Dell", condition: AssetCondition.Good);
    }
}