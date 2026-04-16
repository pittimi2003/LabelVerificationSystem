namespace LabelVerificationSystem.Web.Components.Auth;

public sealed record AuthClientInfoDto(string? App, string? DeviceId);

public sealed record AuthLoginRequestDto(string UsernameOrEmail, string Password, bool RememberMe, AuthClientInfoDto? ClientInfo);

public sealed record AuthRefreshRequestDto(string RefreshToken);

public sealed record AuthLogoutRequestDto(string? RefreshToken);

public sealed record AuthUserDto(
    string UserId,
    string Username,
    string DisplayName,
    string? Email,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);

public sealed record AuthSessionDto(DateTime ExpiresAtUtc, DateTime RefreshRecommendedAtUtc, DateTime ServerUtcNow);

public sealed record AuthTokenResponseDto(
    string AccessToken,
    string TokenType,
    DateTime ExpiresAtUtc,
    int ExpiresInSeconds,
    string RefreshToken,
    DateTime RefreshExpiresAtUtc,
    AuthUserDto User);

public sealed record AuthMeResponseDto(
    bool IsAuthenticated,
    string AuthenticationMode,
    AuthUserDto User,
    AuthSessionDto? Session);

public sealed record ApiErrorResponseDto(string Error);
