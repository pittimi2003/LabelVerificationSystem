using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LabelVerificationSystem.Application.Contracts.Auth;
using LabelVerificationSystem.Application.Interfaces.Auth;
using LabelVerificationSystem.Domain.Entities.Auth;
using LabelVerificationSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LabelVerificationSystem.Infrastructure.Auth;

public sealed class AuthService : IAuthService
{
    private const string AuthModeUser = "User";
    private const string AuthModeBypass = "Bypass";
    private const string PasswordResetNeutralMessage = "If the account exists, reset instructions were sent.";

    private readonly AppDbContext _dbContext;
    private readonly AuthenticationOptions _options;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<AuthService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public AuthService(
        AppDbContext dbContext,
        IOptions<AuthenticationOptions> options,
        IHostEnvironment hostEnvironment,
        ILogger<AuthService> logger)
    {
        _dbContext = dbContext;
        _options = options.Value;
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    public async Task<AuthTokenResponse> LoginAsync(AuthLoginRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
    {
        ValidateLoginRequest(request);

        if (IsBypassEnabled())
        {
            throw new AuthConflictException("Bypass habilitado: login de usuario deshabilitado.");
        }

        var matchedUser = await ResolveUserByLoginAsync(request.UsernameOrEmail, cancellationToken);

        if (matchedUser is null || !matchedUser.IsActive || !await IsValidPasswordAsync(matchedUser.UserId, matchedUser.FallbackPassword, request.Password, cancellationToken))
        {
            throw new AuthUnauthorizedException("Credenciales inválidas.");
        }

        var nowUtc = DateTime.UtcNow;
        var session = new AuthSession
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid().ToString("N"),
            UserId = matchedUser.UserId,
            Username = matchedUser.Username,
            DisplayName = matchedUser.DisplayName,
            Email = matchedUser.Email,
            AuthMode = AuthModeUser,
            CreatedAtUtc = nowUtc,
            LastActivityAtUtc = nowUtc,
            CreatedByIp = Truncate(ipAddress, 64),
            CreatedByUserAgent = Truncate(userAgent, 512)
        };

        var refreshToken = BuildRefreshToken(session, nowUtc, ipAddress, userAgent);

        _dbContext.AuthSessions.Add(session);
        _dbContext.RefreshTokens.Add(refreshToken.Entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return BuildTokenResponse(session, matchedUser.Roles, matchedUser.Permissions, refreshToken.PlainToken, refreshToken.Entity.ExpiresAtUtc, nowUtc);
    }

    public async Task<AuthTokenResponse> RefreshAsync(AuthRefreshRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
    {
        if (IsBypassEnabled())
        {
            throw new AuthConflictException("Bypass habilitado: refresh de usuario deshabilitado.");
        }

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new AuthValidationException("refreshToken es obligatorio.");
        }

        var nowUtc = DateTime.UtcNow;
        var tokenHash = ComputeTokenHash(request.RefreshToken);

        var existingToken = await _dbContext.RefreshTokens
            .Include(x => x.Session)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        if (existingToken is null || existingToken.Session.RevokedAtUtc is not null)
        {
            throw new AuthUnauthorizedException("Refresh token inválido.");
        }

        if (existingToken.ExpiresAtUtc <= nowUtc || existingToken.RevokedAtUtc is not null)
        {
            throw new AuthUnauthorizedException("Refresh token expirado o revocado.");
        }

        if (existingToken.UsedAtUtc is not null)
        {
            await RevokeSessionChainAsync(existingToken.Session, "refresh_reuse_detected", nowUtc, cancellationToken);
            throw new AuthConflictException("Refresh token ya utilizado.");
        }

        existingToken.UsedAtUtc = nowUtc;
        existingToken.Session.LastActivityAtUtc = nowUtc;

        var replacement = BuildRefreshToken(existingToken.Session, nowUtc, ipAddress, userAgent);
        existingToken.ReplacedByTokenId = replacement.Entity.Id;

        _dbContext.RefreshTokens.Add(replacement.Entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var resolvedUser = await ResolveUserByIdAsync(existingToken.Session.UserId, cancellationToken);
        if (resolvedUser is null || !resolvedUser.IsActive)
        {
            throw new AuthUnauthorizedException("Usuario de sesión no disponible.");
        }

        return BuildTokenResponse(existingToken.Session, resolvedUser.Roles, resolvedUser.Permissions, replacement.PlainToken, replacement.Entity.ExpiresAtUtc, nowUtc);
    }

    public async Task LogoutAsync(string? sessionId, string? refreshToken, CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            var session = await _dbContext.AuthSessions.FirstOrDefaultAsync(x => x.SessionId == sessionId, cancellationToken);
            if (session is not null)
            {
                await RevokeSessionChainAsync(session, "logout", nowUtc, cancellationToken);
                return;
            }
        }

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var refreshTokenHash = ComputeTokenHash(refreshToken);
            var token = await _dbContext.RefreshTokens.Include(x => x.Session)
                .FirstOrDefaultAsync(x => x.TokenHash == refreshTokenHash, cancellationToken);
            if (token is not null)
            {
                await RevokeSessionChainAsync(token.Session, "logout_by_refresh", nowUtc, cancellationToken);
            }
        }
    }

    public async Task<AuthMeResponse> GetMeAsync(string? sessionId, DateTime? accessTokenExpiresAtUtc, CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;

        if (IsBypassEnabled())
        {
            _logger.LogInformation("authMode=Bypass endpoint=/api/auth/me");
            var bypassUser = new AuthUserDto(
                _options.Bypass.UserId,
                _options.Bypass.Username,
                _options.Bypass.DisplayName,
                _options.Bypass.Email,
                _options.Bypass.Roles,
                _options.Bypass.Permissions);

            return new AuthMeResponse(true, AuthModeBypass, bypassUser, null);
        }

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new AuthUnauthorizedException("Token inválido o ausente.");
        }

        var session = await _dbContext.AuthSessions.FirstOrDefaultAsync(x => x.SessionId == sessionId, cancellationToken);
        if (session is null || session.RevokedAtUtc is not null)
        {
            throw new AuthUnauthorizedException("Sesión inválida o revocada.");
        }

        var resolvedUser = await ResolveUserByIdAsync(session.UserId, cancellationToken);
        if (resolvedUser is null || !resolvedUser.IsActive)
        {
            throw new AuthUnauthorizedException("Usuario no disponible.");
        }

        var accessExpiresAtUtc = accessTokenExpiresAtUtc ?? nowUtc.AddMinutes(_options.Jwt.AccessTokenTtlMinutes);
        var refreshRecommendedAtUtc = accessExpiresAtUtc.AddMinutes(-_options.Jwt.RefreshProactiveWindowMinutes);

        var user = new AuthUserDto(
            resolvedUser.UserId,
            resolvedUser.Username,
            resolvedUser.DisplayName,
            resolvedUser.Email,
            resolvedUser.Roles,
            resolvedUser.Permissions);

        return new AuthMeResponse(true, AuthModeUser, user, new AuthSessionDto(accessExpiresAtUtc, refreshRecommendedAtUtc, nowUtc));
    }

    public async Task<AuthResetRequestResponse> PasswordResetRequestAsync(AuthResetRequestRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
    {
        ValidatePasswordResetRequest(request);

        var matchedUser = await ResolveUserByLoginAsync(request.UsernameOrEmail, cancellationToken);

        if (matchedUser is null || !matchedUser.IsActive)
        {
            return new AuthResetRequestResponse(PasswordResetNeutralMessage);
        }

        var nowUtc = DateTime.UtcNow;
        var resetToken = BuildPasswordResetToken(matchedUser.UserId, nowUtc, ipAddress, userAgent);

        var priorTokens = await _dbContext.PasswordResetTokens
            .Where(x => x.UserId == matchedUser.UserId && x.UsedAtUtc == null && x.RevokedAtUtc == null && x.ExpiresAtUtc > nowUtc)
            .ToListAsync(cancellationToken);

        foreach (var prior in priorTokens)
        {
            prior.RevokedAtUtc = nowUtc;
            prior.RevocationReason = "superseded_by_new_reset_request";
        }

        _dbContext.PasswordResetTokens.Add(resetToken.Entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "auth.password_reset.requested userId={UserId} delivery=out_of_band_phase token={ResetToken}",
            matchedUser.UserId,
            resetToken.PlainToken);

        return new AuthResetRequestResponse(PasswordResetNeutralMessage);
    }

    public async Task PasswordResetConfirmAsync(AuthResetConfirmRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
    {
        ValidatePasswordResetConfirm(request);

        var nowUtc = DateTime.UtcNow;
        var resetTokenHash = ComputeTokenHash(request.ResetToken);

        var tokenEntity = await _dbContext.PasswordResetTokens
            .FirstOrDefaultAsync(x => x.TokenHash == resetTokenHash, cancellationToken);

        if (tokenEntity is null)
        {
            throw new AuthUnauthorizedException("Reset token inválido.");
        }

        if (tokenEntity.RevokedAtUtc is not null || tokenEntity.UsedAtUtc is not null)
        {
            throw new AuthConflictException("Reset token ya utilizado o revocado.");
        }

        if (tokenEntity.ExpiresAtUtc <= nowUtc)
        {
            throw new AuthUnauthorizedException("Reset token expirado.");
        }

        var resolvedUser = await ResolveUserByIdAsync(tokenEntity.UserId, cancellationToken);
        if (resolvedUser is null || !resolvedUser.IsActive)
        {
            tokenEntity.RevokedAtUtc = nowUtc;
            tokenEntity.RevocationReason = "user_not_available";
            await _dbContext.SaveChangesAsync(cancellationToken);
            throw new AuthUnauthorizedException("Reset token inválido.");
        }

        var existingCredential = await _dbContext.UserPasswordCredentials
            .FirstOrDefaultAsync(x => x.UserId == resolvedUser.UserId, cancellationToken);

        var passwordHash = HashPassword(request.NewPassword);
        if (existingCredential is null)
        {
            _dbContext.UserPasswordCredentials.Add(new UserPasswordCredential
            {
                Id = Guid.NewGuid(),
                UserId = resolvedUser.UserId,
                PasswordHash = passwordHash,
                CreatedAtUtc = nowUtc,
                UpdatedAtUtc = nowUtc,
                UpdatedByIp = Truncate(ipAddress, 64),
                UpdatedByUserAgent = Truncate(userAgent, 512)
            });
        }
        else
        {
            existingCredential.PasswordHash = passwordHash;
            existingCredential.UpdatedAtUtc = nowUtc;
            existingCredential.UpdatedByIp = Truncate(ipAddress, 64);
            existingCredential.UpdatedByUserAgent = Truncate(userAgent, 512);
        }

        tokenEntity.UsedAtUtc = nowUtc;

        var activeUserSessions = await _dbContext.AuthSessions
            .Where(x => x.UserId == resolvedUser.UserId && x.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        if (_options.PasswordReset.RevokeAllSessionsOnPasswordReset)
        {
            foreach (var activeSession in activeUserSessions)
            {
                await RevokeSessionChainAsync(activeSession, "password_reset", nowUtc, cancellationToken);
            }
        }

        var activeResetTokens = await _dbContext.PasswordResetTokens
            .Where(x => x.UserId == resolvedUser.UserId && x.Id != tokenEntity.Id && x.UsedAtUtc == null && x.RevokedAtUtc == null && x.ExpiresAtUtc > nowUtc)
            .ToListAsync(cancellationToken);

        foreach (var activeResetToken in activeResetTokens)
        {
            activeResetToken.RevokedAtUtc = nowUtc;
            activeResetToken.RevocationReason = "password_reset_completed";
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private AuthTokenResponse BuildTokenResponse(AuthSession session, IReadOnlyList<string> roles, IReadOnlyList<string> permissions, string refreshToken, DateTime refreshExpiresAtUtc, DateTime nowUtc)
    {
        var accessExpiresAtUtc = nowUtc.AddMinutes(_options.Jwt.AccessTokenTtlMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, session.UserId ?? session.Username),
            new(JwtRegisteredClaimNames.UniqueName, session.Username),
            new(JwtRegisteredClaimNames.Email, session.Email ?? string.Empty),
            new("sid", session.SessionId),
            new("auth_mode", session.AuthMode),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = accessExpiresAtUtc,
            Issuer = _options.Jwt.Issuer,
            Audience = _options.Jwt.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Jwt.SigningKey)),
                SecurityAlgorithms.HmacSha256)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(securityToken);

        var user = new AuthUserDto(
            session.UserId ?? session.Username,
            session.Username,
            session.DisplayName,
            session.Email,
            roles,
            permissions);

        return new AuthTokenResponse(
            accessToken,
            "Bearer",
            accessExpiresAtUtc,
            (int)TimeSpan.FromMinutes(_options.Jwt.AccessTokenTtlMinutes).TotalSeconds,
            refreshToken,
            refreshExpiresAtUtc,
            user);
    }

    private (RefreshToken Entity, string PlainToken) BuildRefreshToken(AuthSession session, DateTime nowUtc, string? ipAddress, string? userAgent)
    {
        var plainToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hashedToken = ComputeTokenHash(plainToken);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            TokenHash = hashedToken,
            CreatedAtUtc = nowUtc,
            ExpiresAtUtc = nowUtc.AddMinutes(_options.RefreshToken.TtlMinutes),
            CreatedByIp = Truncate(ipAddress, 64),
            CreatedByUserAgent = Truncate(userAgent, 512)
        };

        return (refreshToken, plainToken);
    }

    private (PasswordResetToken Entity, string PlainToken) BuildPasswordResetToken(string userId, DateTime nowUtc, string? ipAddress, string? userAgent)
    {
        var plainToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hashedToken = ComputeTokenHash(plainToken);

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = hashedToken,
            CreatedAtUtc = nowUtc,
            ExpiresAtUtc = nowUtc.AddMinutes(_options.PasswordReset.TokenTtlMinutes),
            CreatedByIp = Truncate(ipAddress, 64),
            CreatedByUserAgent = Truncate(userAgent, 512)
        };

        return (resetToken, plainToken);
    }

    private async Task RevokeSessionChainAsync(AuthSession session, string reason, DateTime nowUtc, CancellationToken cancellationToken)
    {
        if (session.RevokedAtUtc is null)
        {
            session.RevokedAtUtc = nowUtc;
            session.RevocationReason = reason;
        }

        var activeTokens = await _dbContext.RefreshTokens
            .Where(x => x.SessionId == session.Id && x.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAtUtc = nowUtc;
            token.RevocationReason = reason;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private bool IsBypassEnabled()
    {
        if (!_options.Bypass.Enabled)
        {
            return false;
        }

        return _options.Bypass.AllowedEnvironments.Count == 0
               || _options.Bypass.AllowedEnvironments.Contains(_hostEnvironment.EnvironmentName, StringComparer.OrdinalIgnoreCase);
    }

    private static string ComputeTokenHash(string plainToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(plainToken));
        return Convert.ToHexString(hash);
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static void ValidateLoginRequest(AuthLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UsernameOrEmail) || request.UsernameOrEmail.Trim().Length is < 3 or > 256)
        {
            throw new AuthValidationException("usernameOrEmail debe tener entre 3 y 256 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            throw new AuthValidationException("password debe tener mínimo 8 caracteres.");
        }

        if (request.ClientInfo is not null
            && !string.IsNullOrWhiteSpace(request.ClientInfo.App)
            && !string.Equals(request.ClientInfo.App, "BlazorWasm", StringComparison.OrdinalIgnoreCase))
        {
            throw new AuthValidationException("clientInfo.app debe ser BlazorWasm.");
        }
    }

    private static void ValidatePasswordResetRequest(AuthResetRequestRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UsernameOrEmail) || request.UsernameOrEmail.Trim().Length is < 3 or > 256)
        {
            throw new AuthValidationException("usernameOrEmail debe tener entre 3 y 256 caracteres.");
        }
    }

    private static void ValidatePasswordResetConfirm(AuthResetConfirmRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ResetToken))
        {
            throw new AuthValidationException("resetToken es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
        {
            throw new AuthValidationException("newPassword debe tener mínimo 8 caracteres.");
        }

        var hasLetter = request.NewPassword.Any(char.IsLetter);
        var hasDigit = request.NewPassword.Any(char.IsDigit);
        if (!hasLetter || !hasDigit)
        {
            throw new AuthValidationException("newPassword debe contener al menos una letra y un número.");
        }

        if (!string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
        {
            throw new AuthValidationException("newPassword y confirmPassword deben coincidir.");
        }
    }

    private async Task<bool> IsValidPasswordAsync(string userId, string? fallbackPassword, string providedPassword, CancellationToken cancellationToken)
    {
        var storedCredential = await _dbContext.UserPasswordCredentials
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (storedCredential is not null)
        {
            return VerifyPassword(providedPassword, storedCredential.PasswordHash);
        }

        return !string.IsNullOrWhiteSpace(fallbackPassword)
               && string.Equals(fallbackPassword, providedPassword, StringComparison.Ordinal);
    }

    private async Task<ResolvedAuthUser?> ResolveUserByLoginAsync(string usernameOrEmail, CancellationToken cancellationToken)
    {
        var login = usernameOrEmail.Trim();
        var dbUser = await _dbContext.SystemUsers.AsNoTracking().FirstOrDefaultAsync(x =>
            x.Username == login
            || (x.Email != null && x.Email == login), cancellationToken);

        if (dbUser is not null)
        {
            var effectiveRoles = await ResolveEffectiveRolesAsync(dbUser, cancellationToken);
            return new ResolvedAuthUser(
                dbUser.UserId,
                dbUser.Username,
                dbUser.DisplayName,
                dbUser.Email,
                dbUser.IsActive,
                effectiveRoles,
                DeserializeList(dbUser.PermissionsJson),
                null);
        }

        var configuredUser = _options.Users.FirstOrDefault(x =>
            string.Equals(x.Username, login, StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrWhiteSpace(x.Email) && string.Equals(x.Email, login, StringComparison.OrdinalIgnoreCase)));

        return configuredUser is null
            ? null
            : new ResolvedAuthUser(
                configuredUser.UserId,
                configuredUser.Username,
                configuredUser.DisplayName,
                configuredUser.Email,
                configuredUser.IsActive,
                configuredUser.Roles,
                configuredUser.Permissions,
                configuredUser.Password);
    }

    private async Task<ResolvedAuthUser?> ResolveUserByIdAsync(string? userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var dbUser = await _dbContext.SystemUsers.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (dbUser is not null)
        {
            var effectiveRoles = await ResolveEffectiveRolesAsync(dbUser, cancellationToken);
            return new ResolvedAuthUser(
                dbUser.UserId,
                dbUser.Username,
                dbUser.DisplayName,
                dbUser.Email,
                dbUser.IsActive,
                effectiveRoles,
                DeserializeList(dbUser.PermissionsJson),
                null);
        }

        var configuredUser = _options.Users.FirstOrDefault(x => x.UserId == userId);
        return configuredUser is null
            ? null
            : new ResolvedAuthUser(
                configuredUser.UserId,
                configuredUser.Username,
                configuredUser.DisplayName,
                configuredUser.Email,
                configuredUser.IsActive,
                configuredUser.Roles,
                configuredUser.Permissions,
                configuredUser.Password);
    }

    private static IReadOnlyList<string> DeserializeList(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 120000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        try
        {
            var pieces = storedHash.Split(':', 2);
            if (pieces.Length != 2)
            {
                return false;
            }

            var salt = Convert.FromBase64String(pieces[0]);
            var expectedHash = Convert.FromBase64String(pieces[1]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 120000, HashAlgorithmName.SHA256, expectedHash.Length);
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private sealed record ResolvedAuthUser(
        string UserId,
        string Username,
        string DisplayName,
        string? Email,
        bool IsActive,
        IReadOnlyList<string> Roles,
        IReadOnlyList<string> Permissions,
        string? FallbackPassword);

    private async Task<IReadOnlyList<string>> ResolveEffectiveRolesAsync(SystemUser dbUser, CancellationToken cancellationToken)
    {
        var catalogRoles = await _dbContext.SystemUserRoles
            .AsNoTracking()
            .Where(x => x.SystemUserId == dbUser.Id && x.Role.IsActive)
            .Select(x => x.Role.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (catalogRoles.Count > 0)
        {
            return catalogRoles;
        }

        return DeserializeList(dbUser.RolesJson);
    }
}
