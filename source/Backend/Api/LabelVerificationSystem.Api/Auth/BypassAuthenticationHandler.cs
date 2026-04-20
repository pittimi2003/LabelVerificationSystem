using System.Security.Claims;
using System.Text.Encodings.Web;
using LabelVerificationSystem.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace LabelVerificationSystem.Api.Auth;

public sealed class BypassAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly LabelVerificationSystem.Infrastructure.Auth.AuthenticationOptions _authenticationOptions;
    private readonly IHostEnvironment _hostEnvironment;

    public BypassAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        LabelVerificationSystem.Infrastructure.Auth.AuthenticationOptions authenticationOptions,
        IHostEnvironment hostEnvironment)
        : base(options, logger, encoder)
    {
        _authenticationOptions = authenticationOptions;
        _hostEnvironment = hostEnvironment;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!IsBypassEnabledForCurrentEnvironment())
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, _authenticationOptions.Bypass.UserId),
            new(ClaimTypes.Name, _authenticationOptions.Bypass.Username),
            new("username", _authenticationOptions.Bypass.Username)
        };

        if (!string.IsNullOrWhiteSpace(_authenticationOptions.Bypass.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, _authenticationOptions.Bypass.Email));
        }

        claims.AddRange(_authenticationOptions.Bypass.Roles
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(_authenticationOptions.Bypass.Permissions
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(permission => new Claim(AuthPermissionClaims.Type, permission)));

        var identity = new ClaimsIdentity(claims, BypassAuthenticationDefaults.Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, BypassAuthenticationDefaults.Scheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private bool IsBypassEnabledForCurrentEnvironment()
    {
        if (!_authenticationOptions.Bypass.Enabled)
        {
            return false;
        }

        return _authenticationOptions.Bypass.AllowedEnvironments.Count == 0
               || _authenticationOptions.Bypass.AllowedEnvironments.Contains(_hostEnvironment.EnvironmentName, StringComparer.OrdinalIgnoreCase);
    }
}
