using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AssetFlow.Mobile.Models;

namespace AssetFlow.Mobile.Services;

public sealed class AssetFlowApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public AssetFlowApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/auth/token",
            new { username, password },
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("A API retornou um token inválido.");

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(token.TokenType, token.AccessToken);
    }

    public Task<IReadOnlyList<DepartmentModel>> GetDepartmentsAsync(CancellationToken cancellationToken = default) =>
        GetListAsync<DepartmentModel>("api/departments", cancellationToken);

    public async Task<DepartmentModel> CreateDepartmentAsync(
        string name,
        string? description,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/departments",
            new { name, description },
            cancellationToken);
        return await ReadAsync<DepartmentModel>(response, cancellationToken);
    }

    public async Task<DepartmentModel> UpdateDepartmentAsync(
        Guid id, string name, string? description, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"api/departments/{id}", new { name, description }, cancellationToken);
        return await ReadAsync<DepartmentModel>(response, cancellationToken);
    }

    public async Task DeleteDepartmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"api/departments/{id}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public Task<IReadOnlyList<AssetModel>> GetAssetsAsync(CancellationToken cancellationToken = default) =>
        GetListAsync<AssetModel>("api/assets", cancellationToken);

    public async Task<AssetModel> CreateAssetAsync(
        string code,
        string name,
        string? serialNumber,
        string? description,
        Guid? departmentId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/assets",
            new { code, name, serialNumber, description, departmentId, condition = 1 },
            cancellationToken);
        return await ReadAsync<AssetModel>(response, cancellationToken);
    }

    public async Task<AssetModel> UpdateAssetAsync(
        Guid id, string name, string? serialNumber, string? description, int condition,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"api/assets/{id}", new { name, serialNumber, description, condition }, cancellationToken);
        return await ReadAsync<AssetModel>(response, cancellationToken);
    }

    public async Task DeleteAssetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"api/assets/{id}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<AssetModel> MoveAssetAsync(
        AssetModel asset,
        Guid departmentId,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var operation = asset.Status == 2 ? "transfer" : "assign";
        var response = await _httpClient.PostAsJsonAsync(
            $"api/assets/{asset.Id}/{operation}",
            new { departmentId, notes },
            cancellationToken);
        return await ReadAsync<AssetModel>(response, cancellationToken);
    }

    public async Task<AssetModel> ReturnAssetAsync(
        Guid assetId,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"api/assets/{assetId}/return",
            new { notes },
            cancellationToken);
        return await ReadAsync<AssetModel>(response, cancellationToken);
    }

    private async Task<IReadOnlyList<T>> GetListAsync<T>(string uri, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(uri, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T[]>(JsonOptions, cancellationToken) ?? [];
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("A API retornou uma resposta vazia.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = string.IsNullOrWhiteSpace(body)
            ? $"A API respondeu com {(int)response.StatusCode}."
            : body;
        throw new HttpRequestException(message, null, response.StatusCode);
    }
}
