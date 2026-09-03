using AssetFlow.Domain.Enums;

namespace AssetFlow.Domain.Exceptions;

public sealed class InvalidAssetStatusTransitionException : Exception
{
    public AssetStatus CurrentStatus { get; }

    public AssetStatus TargetStatus { get; }

    public AssetStatus? ExpectedCurrentStatus { get; }

    public InvalidAssetStatusTransitionException(
        AssetStatus currentStatus,
        AssetStatus targetStatus,
        AssetStatus? expectedCurrentStatus = null)
        : base(CreateMessage(currentStatus, targetStatus, expectedCurrentStatus))
    {
        CurrentStatus = currentStatus;
        TargetStatus = targetStatus;
        ExpectedCurrentStatus = expectedCurrentStatus;
    }

    private static string CreateMessage(
        AssetStatus currentStatus,
        AssetStatus targetStatus,
        AssetStatus? expectedCurrentStatus)
    {
        var message = $"Cannot change asset status from '{currentStatus}' to '{targetStatus}'.";

        return expectedCurrentStatus is null
            ? message
            : $"{message} Expected current status: '{expectedCurrentStatus}'.";
    }
}
