using System.Net;
using System.Net.Http.Headers;

namespace LabelVerificationSystem.Web.Components.Auth;

public sealed class BackendApiAuthHandler : DelegatingHandler
{
    private readonly AuthSessionService _authSessionService;

    public BackendApiAuthHandler(AuthSessionService authSessionService)
    {
        _authSessionService = authSessionService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var isAuthEndpoint = request.RequestUri?.AbsolutePath.StartsWith("/api/auth", StringComparison.OrdinalIgnoreCase) == true;
        if (!isAuthEndpoint)
        {
            await AttachAuthorizationAsync(request, cancellationToken);
        }

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            await _authSessionService.HandleAuthorizationFailureAsync(response.StatusCode, cancellationToken);
        }

        return response;
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
