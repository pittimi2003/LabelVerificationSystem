using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LabelVerificationSystem.Web.Components.Auth;

namespace LabelVerificationSystem.Web.Components.Authorization;

public sealed class AuthorizationAdministrationApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthSessionService _authSessionService;

    public AuthorizationAdministrationApiClient(HttpClient httpClient, AuthSessionService authSessionService)
    {
        _httpClient = httpClient;
        _authSessionService = authSessionService;
    }

    public async Task<IReadOnlyList<AuthorizationRoleDto>> ListRolesAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/authorization-matrix/roles");
        var response = await SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await ReadRequiredJsonAsync<IReadOnlyList<AuthorizationRoleDto>>(
                       response,
                       "No se pudo interpretar el catálogo de roles.",
                       cancellationToken)
                   ?? [];
        }

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<RoleAuthorizationMatrixDto> GetRoleMatrixAsync(string roleCode, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/authorization-matrix/roles/{Uri.EscapeDataString(roleCode)}");
        var response = await SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await ReadRequiredJsonAsync<RoleAuthorizationMatrixDto>(
                   response,
                   "No se pudo interpretar la matriz de autorización.",
                   cancellationToken)
               ?? throw new InvalidOperationException("La API devolvió una respuesta vacía.");
        }

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<RoleAuthorizationMatrixDto> UpdateRoleMatrixAsync(
        string roleCode,
        UpdateRoleAuthorizationMatrixRequestDto request,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Put, $"api/authorization-matrix/roles/{Uri.EscapeDataString(roleCode)}")
        {
            Content = JsonContent.Create(request)
        };
        var response = await SendAsync(message, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await ReadRequiredJsonAsync<RoleAuthorizationMatrixDto>(
                   response,
                   "No se pudo interpretar la matriz de autorización guardada.",
                   cancellationToken)
               ?? throw new InvalidOperationException("La API devolvió una respuesta vacía.");
        }

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return "No autorizado (401). Inicia sesión nuevamente.";
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            return "Acceso denegado (403) para administrar la matriz de autorización por rol.";
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

            return $"La operación falló con código {(int)response.StatusCode}: {TrimForMessage(trimmedBody)}";
        }

        return $"La operación falló con código {(int)response.StatusCode} y cuerpo vacío.";
    }

    private static async Task<T?> ReadRequiredJsonAsync<T>(HttpResponseMessage response, string emptyPayloadErrorMessage, CancellationToken cancellationToken)
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
            throw new InvalidOperationException($"Respuesta inválida: {TrimForMessage(trimmedBody)}", ex);
        }
    }

    private static string TrimForMessage(string body)
    {
        const int maxLength = 300;
        return body.Length <= maxLength ? body : $"{body[..maxLength]}...";
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
}
