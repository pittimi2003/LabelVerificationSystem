using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LabelVerificationSystem.Web.Components.Users;

public sealed class UserAdministrationApiClient
{
    private readonly HttpClient _httpClient;

    public UserAdministrationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UserListResponseDto> ListAsync(UserListQueryDto query, CancellationToken cancellationToken)
    {
        var queryParameters = new List<string>
        {
            $"page={query.Page}",
            $"pageSize={query.PageSize}"
        };

        if (!string.IsNullOrWhiteSpace(query.Query))
        {
            queryParameters.Add($"query={Uri.EscapeDataString(query.Query.Trim())}");
        }

        AddIfPresent(queryParameters, "userId", query.UserId);
        AddIfPresent(queryParameters, "username", query.Username);
        AddIfPresent(queryParameters, "displayName", query.DisplayName);
        AddIfPresent(queryParameters, "email", query.Email);
        AddIfPresent(queryParameters, "role", query.Role);
        AddIfPresent(queryParameters, "permission", query.Permission);

        if (query.IsActive.HasValue)
        {
            queryParameters.Add($"isActive={query.IsActive.Value.ToString().ToLowerInvariant()}");
        }

        var requestUri = $"api/users?{string.Join("&", queryParameters)}";
        var response = await _httpClient.GetAsync(requestUri, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await ReadRequiredJsonAsync<UserListResponseDto>(
                       response,
                       "No se pudo interpretar la respuesta del listado de usuarios.",
                       cancellationToken)
                   ?? new UserListResponseDto([], query.Page, query.PageSize, 0, 0);
        }

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<UserDetailDto> GetByUserIdAsync(string userId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"api/users/{Uri.EscapeDataString(userId)}", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await ReadRequiredJsonAsync<UserDetailDto>(
                   response,
                   "La API devolvió una respuesta vacía.",
                   cancellationToken)
               ?? throw new InvalidOperationException("La API devolvió una respuesta vacía.");
        }

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<UserDetailDto> CreateAsync(CreateUserRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/users", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await ReadRequiredJsonAsync<UserDetailDto>(
                   response,
                   "La API devolvió una respuesta vacía.",
                   cancellationToken)
               ?? throw new InvalidOperationException("La API devolvió una respuesta vacía.");
        }

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<UserDetailDto> UpdateAsync(string userId, UpdateUserRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/users/{Uri.EscapeDataString(userId)}", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await ReadRequiredJsonAsync<UserDetailDto>(
                   response,
                   "La API devolvió una respuesta vacía.",
                   cancellationToken)
               ?? throw new InvalidOperationException("La API devolvió una respuesta vacía.");
        }

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<UserDetailDto> SetActivationAsync(string userId, bool isActive, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PatchAsJsonAsync(
            $"api/users/{Uri.EscapeDataString(userId)}/activation",
            new SetUserActivationRequestDto(isActive),
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await ReadRequiredJsonAsync<UserDetailDto>(
                   response,
                   "La API devolvió una respuesta vacía.",
                   cancellationToken)
               ?? throw new InvalidOperationException("La API devolvió una respuesta vacía.");
        }

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return "No autorizado para consultar usuarios. Inicia sesión nuevamente.";
        }

        var contentType = response.Content.Headers.ContentType?.MediaType;
        var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var trimmedBody = rawBody?.Trim() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(trimmedBody))
        {
            if (string.Equals(contentType, "application/json", StringComparison.OrdinalIgnoreCase)
                || string.Equals(contentType, "application/problem+json", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var apiError = JsonSerializer.Deserialize<ApiErrorResponseDto>(
                        trimmedBody,
                        new JsonSerializerOptions(JsonSerializerDefaults.Web));
                    if (!string.IsNullOrWhiteSpace(apiError?.Error))
                    {
                        return apiError.Error;
                    }
                }
                catch (JsonException)
                {
                    // Se reporta abajo el body real para no ocultar la causa del backend.
                }
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
        var contentType = response.Content.Headers.ContentType?.MediaType;
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
                $"Respuesta HTTP {(int)response.StatusCode} inválida para JSON (content-type: {contentType ?? "desconocido"}). Body: {TrimForMessage(trimmedBody)}",
                ex);
        }
    }

    private static string TrimForMessage(string body)
    {
        const int maxLength = 300;
        return body.Length <= maxLength ? body : $"{body[..maxLength]}...";
    }

    private static void AddIfPresent(ICollection<string> queryParameters, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            queryParameters.Add($"{key}={Uri.EscapeDataString(value.Trim())}");
        }
    }
}
