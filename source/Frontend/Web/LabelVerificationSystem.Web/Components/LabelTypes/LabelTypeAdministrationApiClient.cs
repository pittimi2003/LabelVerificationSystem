using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LabelVerificationSystem.Web.Components.Auth;

namespace LabelVerificationSystem.Web.Components.LabelTypes;

public sealed class LabelTypeAdministrationApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthSessionService _authSessionService;

    public LabelTypeAdministrationApiClient(HttpClient httpClient, AuthSessionService authSessionService)
    {
        _httpClient = httpClient;
        _authSessionService = authSessionService;
    }

    public async Task<LabelTypeListResponseDto> ListAsync(LabelTypeListQueryDto query, CancellationToken cancellationToken)
    {
        var qp = new List<string> {$"page={query.Page}", $"pageSize={query.PageSize}"};
        if (!string.IsNullOrWhiteSpace(query.Query)) qp.Add($"query={Uri.EscapeDataString(query.Query.Trim())}");
        if (query.IsActive.HasValue) qp.Add($"isActive={query.IsActive.Value.ToString().ToLowerInvariant()}");
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/label-types?{string.Join("&", qp)}");
        var response = await SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
            return await ReadRequiredJsonAsync<LabelTypeListResponseDto>(response, "Respuesta vacía.", cancellationToken) ?? new([], query.Page, query.PageSize, 0, 0);
        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<IReadOnlyList<string>> GetAvailableColumnsAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/label-types/available-columns");
        var response = await SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
            return await ReadRequiredJsonAsync<IReadOnlyList<string>>(response, "Respuesta vacía.", cancellationToken) ?? [];
        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<LabelTypeDetailDto> CreateAsync(CreateLabelTypeRequestDto payload, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/label-types") { Content = JsonContent.Create(payload) };
        var response = await SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
            return await ReadRequiredJsonAsync<LabelTypeDetailDto>(response, "Respuesta vacía.", cancellationToken) ?? throw new InvalidOperationException("Respuesta vacía");
        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<LabelTypeDetailDto> UpdateAsync(Guid id, UpdateLabelTypeRequestDto payload, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"api/label-types/{id:D}") { Content = JsonContent.Create(payload) };
        var response = await SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
            return await ReadRequiredJsonAsync<LabelTypeDetailDto>(response, "Respuesta vacía.", cancellationToken) ?? throw new InvalidOperationException("Respuesta vacía");
        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<LabelTypeDetailDto> SetActivationAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/label-types/{id:D}/activation") { Content = JsonContent.Create(new SetLabelTypeActivationRequestDto(isActive)) };
        var response = await SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
            return await ReadRequiredJsonAsync<LabelTypeDetailDto>(response, "Respuesta vacía.", cancellationToken) ?? throw new InvalidOperationException("Respuesta vacía");
        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (await _authSessionService.EnsureValidAccessTokenAsync(cancellationToken) && !string.IsNullOrWhiteSpace(_authSessionService.Current.AccessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authSessionService.Current.AccessToken);
        return await _httpClient.SendAsync(request, cancellationToken);
    }

    private static async Task<T?> ReadRequiredJsonAsync<T>(HttpResponseMessage response, string emptyMessage, CancellationToken cancellationToken)
    {
        var body = (await response.Content.ReadAsStringAsync(cancellationToken)).Trim();
        if (string.IsNullOrWhiteSpace(body)) throw new InvalidOperationException(emptyMessage);
        return JsonSerializer.Deserialize<T>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode is HttpStatusCode.Forbidden) return "Acceso denegado al módulo Label Types (403).";
        var rawBody = (await response.Content.ReadAsStringAsync(cancellationToken)).Trim();
        if (!string.IsNullOrWhiteSpace(rawBody))
        {
            try
            {
                var apiError = JsonSerializer.Deserialize<ApiErrorResponseDto>(rawBody, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                if (!string.IsNullOrWhiteSpace(apiError?.Error)) return apiError.Error;
            }
            catch (JsonException) { }
        }

        return $"La operación falló con código {(int)response.StatusCode}.";
    }
}
