using LabelVerificationSystem.Application.Contracts.Auth;

namespace LabelVerificationSystem.Application.Interfaces.Auth;

public interface IAuthService
{
    Task<AuthTokenResponse> LoginAsync(AuthLoginRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken);
    Task<AuthTokenResponse> RefreshAsync(AuthRefreshRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken);
    Task LogoutAsync(string? sessionId, string? refreshToken, CancellationToken cancellationToken);
    Task<AuthMeResponse> GetMeAsync(string? sessionId, DateTime? accessTokenExpiresAtUtc, CancellationToken cancellationToken);
    Task<AuthResetRequestResponse> PasswordResetRequestAsync(AuthResetRequestRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken);
    Task PasswordResetConfirmAsync(AuthResetConfirmRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken);
}
