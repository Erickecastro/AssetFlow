namespace AssetFlow.Application.Common;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string resourceName, Guid id)
        : base($"{resourceName} '{id}' was not found.")
    {
    }
}
