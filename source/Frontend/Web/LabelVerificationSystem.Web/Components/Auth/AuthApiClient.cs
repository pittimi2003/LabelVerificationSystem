using LabelVerificationSystem.Web.Components.Services;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace LabelVerificationSystem.Web.Components.Auth;

public sealed class AuthApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<AuthTokenResponseDto> LoginAsync(AuthLoginRequestDto request, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(BackendApiHttpClientOptions.RawClientName);
        var response = await client.PostAsJsonAsync("api/auth/login", request, cancellationToken);
        return await ReadTokenResponseAsync(response, cancellationToken);
    }

    public async Task<AuthTokenResponseDto> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(BackendApiHttpClientOptions.RawClientName);
        var response = await client.PostAsJsonAsync("api/auth/refresh", new AuthRefreshRequestDto(refreshToken), cancellationToken);
        return await ReadTokenResponseAsync(response, cancellationToken);
    }

    public async Task LogoutAsync(string? accessToken, string? refreshToken, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(BackendApiHttpClientOptions.RawClientName);
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        var response = await client.PostAsJsonAsync("api/auth/logout", new AuthLogoutRequestDto(refreshToken), cancellationToken);
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task<AuthMeResponseDto?> GetMeAsync(string? accessToken, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(BackendApiHttpClientOptions.RawClientName);
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        var response = await client.GetAsync("api/auth/me", cancellationToken);
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthMeResponseDto>(cancellationToken: cancellationToken);
    }

    public async Task<AuthResetRequestResponseDto> RequestPasswordResetAsync(string usernameOrEmail, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(BackendApiHttpClientOptions.RawClientName);
        var response = await client.PostAsJsonAsync(
            "api/auth/password/reset-request",
            new AuthResetRequestRequestDto(usernameOrEmail),
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AuthResetRequestResponseDto>(cancellationToken: cancellationToken);
            return result ?? new AuthResetRequestResponseDto("If the account exists, reset instructions were sent.");
        }

        var apiError = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>(cancellationToken: cancellationToken);
        var message = string.IsNullOrWhiteSpace(apiError?.Error)
            ? $"Error de autenticación {(int)response.StatusCode}."
            : apiError.Error;
        throw new HttpRequestException(message, null, response.StatusCode);
    }

    public async Task ConfirmPasswordResetAsync(string resetToken, string newPassword, string confirmPassword, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(BackendApiHttpClientOptions.RawClientName);
        var response = await client.PostAsJsonAsync(
            "api/auth/password/reset-confirm",
            new AuthResetConfirmRequestDto(resetToken, newPassword, confirmPassword),
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var apiError = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>(cancellationToken: cancellationToken);
        var message = string.IsNullOrWhiteSpace(apiError?.Error)
            ? $"Error de autenticación {(int)response.StatusCode}."
            : apiError.Error;
        throw new HttpRequestException(message, null, response.StatusCode);
    }

    private static async Task<AuthTokenResponseDto> ReadTokenResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            var tokenResponse = await response.Content.ReadFromJsonAsync<AuthTokenResponseDto>(cancellationToken: cancellationToken);
            return tokenResponse ?? throw new InvalidOperationException("Respuesta de autenticación vacía.");
        }

        var apiError = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>(cancellationToken: cancellationToken);
        var message = string.IsNullOrWhiteSpace(apiError?.Error)
            ? $"Error de autenticación {(int)response.StatusCode}."
            : apiError.Error;

        throw new HttpRequestException(message, null, response.StatusCode);
    }
}
