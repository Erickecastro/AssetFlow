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
        return new Asset(
            new AssetCode("PAT-0001"),
            "Notebook Dell",
            condition: AssetCondition.Good);
    }
}