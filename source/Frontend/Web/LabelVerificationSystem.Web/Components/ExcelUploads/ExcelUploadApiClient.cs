using LabelVerificationSystem.Web.Components.Auth;
using Microsoft.AspNetCore.Components.Forms;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LabelVerificationSystem.Web.Components.ExcelUploads;

public sealed class ExcelUploadApiClient
{
    private const long MaxUploadSizeBytes = 50 * 1024 * 1024;
    private readonly HttpClient _httpClient;
    private readonly AuthSessionService _authSessionService;

    public ExcelUploadApiClient(HttpClient httpClient, AuthSessionService authSessionService)
    {
        _httpClient = httpClient;
        _authSessionService = authSessionService;
    }

    public async Task<IReadOnlyList<ExcelUploadHistoryItemDto>> GetHistoryAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/excel-uploads");
        var response = await SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await ReadRequiredJsonAsync<IReadOnlyList<ExcelUploadHistoryItemDto>>(
                       response,
                       "No se pudo interpretar el historial de cargas Excel.",
                       cancellationToken)
                   ?? [];
        }

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<ExcelUploadDetailDto?> GetDetailAsync(Guid uploadId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/excel-uploads/{uploadId}/details");
        var response = await SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (response.IsSuccessStatusCode)
        {
            return await ReadRequiredJsonAsync<ExcelUploadDetailDto>(
                response,
                "No se pudo interpretar el detalle de carga Excel.",
                cancellationToken);
        }

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<ExcelUploadResultDto> UploadAsync(IBrowserFile file, CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();
        await using var fileStream = file.OpenReadStream(MaxUploadSizeBytes, cancellationToken);
        using var fileContent = new StreamContent(fileStream);

        fileContent.Headers.ContentType = new(file.ContentType);
        content.Add(fileContent, "file", file.Name);

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/excel-uploads")
        {
            Content = content
        };
        var response = await SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await ReadRequiredJsonAsync<ExcelUploadResultDto>(
                       response,
                       "La API devolvió una respuesta vacía.",
                       cancellationToken)
                   ?? throw new InvalidOperationException("La API devolvió una respuesta vacía.");
        }

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
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
            return "No autorizado (401). Inicia sesión nuevamente.";
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            return "Acceso denegado (403) al módulo de carga Excel.";
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
            throw new InvalidOperationException($"Respuesta inválida: {TrimForMessage(trimmedBody)}", ex);
        }
    }

    private static string TrimForMessage(string body)
    {
        const int maxLength = 300;
        return body.Length <= maxLength ? body : $"{body[..maxLength]}...";
    }
}
