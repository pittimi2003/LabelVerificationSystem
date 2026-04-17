using System.Net.Http.Json;

namespace LabelVerificationSystem.Web.Components.Users;

public sealed class UserAdministrationApiClient
{
    private readonly HttpClient _httpClient;

    public UserAdministrationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UserListResponseDto> ListAsync(string? query, bool? isActive, int page, int pageSize, CancellationToken cancellationToken)
    {
        var queryParameters = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrWhiteSpace(query))
        {
            queryParameters.Add($"query={Uri.EscapeDataString(query.Trim())}");
        }

        if (isActive.HasValue)
        {
            queryParameters.Add($"isActive={isActive.Value.ToString().ToLowerInvariant()}");
        }

        var requestUri = $"api/users?{string.Join("&", queryParameters)}";
        var response = await _httpClient.GetAsync(requestUri, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UserListResponseDto>(cancellationToken: cancellationToken)
                ?? new UserListResponseDto([], page, pageSize, 0, 0);
        }

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<UserDetailDto> GetByUserIdAsync(string userId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"api/users/{Uri.EscapeDataString(userId)}", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UserDetailDto>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("La API devolvió una respuesta vacía.");
        }

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<UserDetailDto> CreateAsync(CreateUserRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/users", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UserDetailDto>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("La API devolvió una respuesta vacía.");
        }

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    public async Task<UserDetailDto> UpdateAsync(string userId, UpdateUserRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/users/{Uri.EscapeDataString(userId)}", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UserDetailDto>(cancellationToken: cancellationToken)
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
            return await response.Content.ReadFromJsonAsync<UserDetailDto>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("La API devolvió una respuesta vacía.");
        }

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var apiError = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>(cancellationToken: cancellationToken);

        if (!string.IsNullOrWhiteSpace(apiError?.Error))
        {
            return apiError.Error;
        }

        return $"La operación falló con código {(int)response.StatusCode}.";
    }
}
