using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;

namespace LabelVerificationSystem.Web.Components.ExcelUploads;

public sealed class ExcelUploadApiClient
{
    private const long MaxUploadSizeBytes = 50 * 1024 * 1024;
    private readonly HttpClient _httpClient;

    public ExcelUploadApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<ExcelUploadHistoryItemDto>> GetHistoryAsync(CancellationToken cancellationToken)
    {
        var history = await _httpClient.GetFromJsonAsync<List<ExcelUploadHistoryItemDto>>("api/excel-uploads", cancellationToken);
        return history ?? [];
    }

    public async Task<ExcelUploadResultDto> UploadAsync(IBrowserFile file, CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();
        await using var fileStream = file.OpenReadStream(MaxUploadSizeBytes, cancellationToken);
        using var fileContent = new StreamContent(fileStream);

        fileContent.Headers.ContentType = new(file.ContentType);
        content.Add(fileContent, "file", file.Name);

        var response = await _httpClient.PostAsync("api/excel-uploads", content, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ExcelUploadResultDto>(cancellationToken: cancellationToken);
            return result ?? throw new InvalidOperationException("La API devolvió una respuesta vacía.");
        }

        var apiError = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>(cancellationToken: cancellationToken);
        var errorMessage = apiError?.Error;

        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            errorMessage = $"La operación falló con código {(int)response.StatusCode}.";
        }

        throw new InvalidOperationException(errorMessage);
    }
}
