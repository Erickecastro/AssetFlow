namespace AssetFlow.Api.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Authentication";
    public string Issuer { get; init; } = "AssetFlow.Api";
    public string Audience { get; init; } = "AssetFlow.Client";
    public string SigningKey { get; init; } = string.Empty;
    public string DemoUsername { get; init; } = string.Empty;
    public string DemoPassword { get; init; } = string.Empty;
}
