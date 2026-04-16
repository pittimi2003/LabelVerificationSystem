namespace LabelVerificationSystem.Application.Contracts.Auth;

public sealed record AuthClientInfo(string? App, string? DeviceId);

public sealed record AuthLoginRequest(string UsernameOrEmail, string Password, bool RememberMe, AuthClientInfo? ClientInfo);

public sealed record AuthRefreshRequest(string RefreshToken);

public sealed record AuthLogoutRequest(string? RefreshToken);

public sealed record AuthUserDto(
    string UserId,
    string Username,
    string DisplayName,
    string? Email,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);

public sealed record AuthSessionDto(DateTime ExpiresAtUtc, DateTime RefreshRecommendedAtUtc, DateTime ServerUtcNow);

public sealed record AuthTokenResponse(
    string AccessToken,
    string TokenType,
    DateTime ExpiresAtUtc,
    int ExpiresInSeconds,
    string RefreshToken,
    DateTime RefreshExpiresAtUtc,
    AuthUserDto User);

public sealed record AuthMeResponse(
    bool IsAuthenticated,
    string AuthenticationMode,
    AuthUserDto User,
    AuthSessionDto? Session);
