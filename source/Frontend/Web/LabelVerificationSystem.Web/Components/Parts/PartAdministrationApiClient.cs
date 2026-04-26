using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LabelVerificationSystem.Web.Components.Auth;

namespace LabelVerificationSystem.Web.Components.Parts;

public sealed class PartAdministrationApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthSessionService _authSessionService;

    public PartAdministrationApiClient(HttpClient httpClient, AuthSessionService authSessionService)
    {
        _httpClient = httpClient;
        _authSessionService = authSessionService;
    }

    public async Task<PartListResponseDto> ListAsync(PartListQueryDto query, CancellationToken cancellationToken)
    {
        var queryParameters = new List<string>
        {
            $"page={query.Page}",
            $"pageSize={query.PageSize}"
        };

        AddIfPresent(queryParameters, "partNumber", query.PartNumber);
        AddIfPresent(queryParameters, "model", query.Model);
        AddIfPresent(queryParameters, "minghuaDescription", query.MinghuaDescription);
        AddIfPresent(queryParameters, "cco", query.Cco);
        AddIfPresent(queryParameters, "labelTypeName", query.LabelTypeName);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/parts?{string.Join("&", queryParameters)}");
        var response = await SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return await ReadRequiredJsonAsync<PartListResponseDto>(response, "No se pudo interpretar el listado de parts.", cancellationToken)
                   ?? new PartListResponseDto([], query.Page, query.PageSize, 0, 0);
        }

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<PartDetailDto> GetByIdAsync(Guid partId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/parts/{partId:D}");
        var response = await SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return await ReadRequiredJsonAsync<PartDetailDto>(response, "La API devolvió una respuesta vacía.", cancellationToken)
                   ?? throw new InvalidOperationException("La API devolvió una respuesta vacía.");
        }

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<PartDetailDto> CreateAsync(CreatePartRequestDto payload, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/parts")
        {
            Content = JsonContent.Create(payload)
        };
        var response = await SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return await ReadRequiredJsonAsync<PartDetailDto>(response, "La API devolvió una respuesta vacía.", cancellationToken)
                   ?? throw new InvalidOperationException("La API devolvió una respuesta vacía.");
        }

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<PartDetailDto> UpdateAsync(Guid partId, UpdatePartRequestDto payload, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"api/parts/{partId:D}")
        {
            Content = JsonContent.Create(payload)
        };
        var response = await SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return await ReadRequiredJsonAsync<PartDetailDto>(response, "La API devolvió una respuesta vacía.", cancellationToken)
                   ?? throw new InvalidOperationException("La API devolvió una respuesta vacía.");
        }

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return "No autorizado para consultar parts (401). Inicia sesión nuevamente.";
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            return "Acceso denegado al módulo de Parts Catalog (403).";
        }

        var rawBody = (await response.Content.ReadAsStringAsync(cancellationToken)).Trim();
        if (!string.IsNullOrWhiteSpace(rawBody))
        {
            try
            {
                var apiError = JsonSerializer.Deserialize<ApiErrorResponseDto>(rawBody, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                if (!string.IsNullOrWhiteSpace(apiError?.Error))
                {
                    return apiError.Error;
                }
            }
            catch (JsonException)
            {
            }

            return $"La operación falló con código {(int)response.StatusCode}: {TrimForMessage(rawBody)}";
        }

        return $"La operación falló con código {(int)response.StatusCode} y cuerpo vacío.";
    }

    private static async Task<T?> ReadRequiredJsonAsync<T>(HttpResponseMessage response, string emptyPayloadErrorMessage, CancellationToken cancellationToken)
    {
        var body = (await response.Content.ReadAsStringAsync(cancellationToken)).Trim();
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new InvalidOperationException($"{emptyPayloadErrorMessage} Código HTTP {(int)response.StatusCode}.");
        }

        try
        {
            return JsonSerializer.Deserialize<T>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Respuesta HTTP {(int)response.StatusCode} inválida: {TrimForMessage(body)}", ex);
        }
    }

    private static string TrimForMessage(string body) => body.Length <= 300 ? body : $"{body[..300]}...";

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
}
