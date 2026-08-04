using AssetFlow.Domain.Enums;

namespace AssetFlow.Domain.Exceptions;

public sealed class InvalidAssetStatusTransitionException : Exception
{
    public AssetStatus CurrentStatus { get; }

    public AssetStatus TargetStatus { get; }

    public InvalidAssetStatusTransitionException(AssetStatus currentStatus, AssetStatus targetStatus) : base($"Cannot change asset status from '{currentStatus}' to '{targetStatus}'.")
    {
        CurrentStatus = currentStatus;
        TargetStatus = targetStatus;
    }
}