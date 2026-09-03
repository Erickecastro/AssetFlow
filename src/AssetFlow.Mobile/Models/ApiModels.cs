using System.Text.Json.Serialization;

namespace AssetFlow.Mobile.Models;

public sealed record TokenResponse(string AccessToken, string TokenType, int ExpiresInSeconds);

public sealed record DepartmentModel(Guid Id, string Name, string? Description)
{
    public override string ToString() => Name;
}

public sealed record AssetMovementModel(
    Guid Id,
    int Type,
    Guid? FromDepartmentId,
    Guid? ToDepartmentId,
    DateTimeOffset OccurredAtUtc,
    string? Notes)
{
    [JsonIgnore]
    public string Summary => $"{TypeName} · {OccurredAtUtc.ToLocalTime():g} · {Notes ?? "Sem observações"}";

    private string TypeName => Type switch
    {
        1 => "Atribuição",
        2 => "Transferência",
        3 => "Devolução",
        _ => "Movimentação"
    };
}

public sealed record AssetModel(
    Guid Id,
    string Code,
    string Name,
    string? SerialNumber,
    string? Description,
    int Status,
    int Condition,
    Guid? DepartmentId,
    string QrCodeValue,
    IReadOnlyList<AssetMovementModel> Movements)
{
    [JsonIgnore]
    public string StatusName => Status switch
    {
        1 => "Disponível",
        2 => "Atribuído",
        3 => "Reservado",
        4 => "Em manutenção",
        5 => "Baixado",
        6 => "Perdido",
        7 => "Descartado",
        _ => "Desconhecido"
    };

    [JsonIgnore]
    public string Summary => $"{Code} · {Name}";

    [JsonIgnore]
    public bool HasNoMovements => Movements.Count == 0;
}
