using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LabelVerificationSystem.Web.Components.Auth;

namespace LabelVerificationSystem.Web.Components.Roles;

public sealed class RoleCatalogAdministrationApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthSessionService _authSessionService;

    public RoleCatalogAdministrationApiClient(HttpClient httpClient, AuthSessionService authSessionService)
    {
        _httpClient = httpClient;
        _authSessionService = authSessionService;
    }

    public async Task<RoleCatalogListResponseDto> ListAsync(RoleCatalogListQueryDto query, CancellationToken cancellationToken)
    {
        var queryParameters = new List<string>
        {
            $"page={query.Page}",
            $"pageSize={query.PageSize}"
        };

        AddIfPresent(queryParameters, "query", query.Query);
        AddIfPresent(queryParameters, "code", query.Code);
        AddIfPresent(queryParameters, "name", query.Name);

        if (query.IsActive.HasValue)
        {
            queryParameters.Add($"isActive={query.IsActive.Value.ToString().ToLowerInvariant()}");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/roles?{string.Join("&", queryParameters)}");
        var response = await SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await ReadRequiredJsonAsync<RoleCatalogListResponseDto>(
                       response,
                       "No se pudo interpretar la respuesta del listado de roles.",
                       cancellationToken)
                   ?? new RoleCatalogListResponseDto([], query.Page, query.PageSize, 0, 0);
        }

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<RoleCatalogDetailDto> GetByCodeAsync(string roleCode, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/roles/{Uri.EscapeDataString(roleCode)}");
        var response = await SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await ReadRequiredJsonAsync<RoleCatalogDetailDto>(
                   response,
                   "La API devolvió una respuesta vacía.",
                   cancellationToken)
               ?? throw new InvalidOperationException("La API devolvió una respuesta vacía.");
        }

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<RoleCatalogDetailDto> SetActivationAsync(string roleCode, bool isActive, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/roles/{Uri.EscapeDataString(roleCode)}/activation")
        {
            Content = JsonContent.Create(new SetRoleActivationRequestDto(isActive))
        };
        var response = await SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await ReadRequiredJsonAsync<RoleCatalogDetailDto>(
                   response,
                   "La API devolvió una respuesta vacía.",
                   cancellationToken)
               ?? throw new InvalidOperationException("La API devolvió una respuesta vacía.");
        }

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    private static void AddIfPresent(ICollection<string> queryParameters, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            queryParameters.Add($"{key}={Uri.EscapeDataString(value.Trim())}");
        }
    }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await AttachAuthorizationAsync(request, cancellationToken);
        return await _httpClient.SendAsync(request, cancellationToken);
    }

    private async Task AttachAuthorizationAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var hasValidAccessToken = await _authSessionService.EnsureValidAccessTokenAsync(cancellationToken);
        if (!hasValidAccessToken)
        {
            return;
        }

        var token = _authSessionService.Current.AccessToken;
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return "No autorizado para consultar roles (401). Inicia sesión nuevamente.";
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            return "Acceso denegado al módulo de roles (403).";
        }

        var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var trimmedBody = rawBody?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(trimmedBody))
        {
            try
            {
                var apiError = JsonSerializer.Deserialize<ApiErrorResponseDto>(trimmedBody, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                if (!string.IsNullOrWhiteSpace(apiError?.Error))
                {
                    return apiError.Error;
                }
            }
            catch (JsonException)
            {
            }

            return $"La operación falló con código {(int)response.StatusCode} y devolvió: {TrimForMessage(trimmedBody)}";
        }

        return $"La operación falló con código {(int)response.StatusCode} y cuerpo vacío.";
    }

    private static async Task<T?> ReadRequiredJsonAsync<T>(
        HttpResponseMessage response,
        string emptyPayloadErrorMessage,
        CancellationToken cancellationToken)
    {
        var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var trimmedBody = rawBody?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(trimmedBody))
        {
            throw new InvalidOperationException($"{emptyPayloadErrorMessage} Código HTTP {(int)response.StatusCode}.");
        }

        try
        {
            return JsonSerializer.Deserialize<T>(trimmedBody, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Respuesta HTTP {(int)response.StatusCode} inválida para JSON. Body: {TrimForMessage(trimmedBody)}",
                ex);
        }
    }

    private static string TrimForMessage(string body)
    {
        const int maxLength = 300;
        return body.Length <= maxLength ? body : $"{body[..maxLength]}...";
    }
}
