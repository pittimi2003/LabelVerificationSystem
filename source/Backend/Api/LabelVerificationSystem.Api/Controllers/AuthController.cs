using LabelVerificationSystem.Api.Contracts;
using LabelVerificationSystem.Application.Contracts.Auth;
using LabelVerificationSystem.Application.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabelVerificationSystem.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthTokenResponse>> Login([FromBody] AuthLoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.LoginAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString(), cancellationToken);
            return Ok(response);
        }
        catch (AuthValidationException ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
        catch (AuthUnauthorizedException ex)
        {
            return Unauthorized(new ApiErrorResponse(ex.Message));
        }
        catch (AuthConflictException ex)
        {
            return Conflict(new ApiErrorResponse(ex.Message));
        }
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthTokenResponse>> Refresh([FromBody] AuthRefreshRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.RefreshAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString(), cancellationToken);
            return Ok(response);
        }
        catch (AuthValidationException ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
        catch (AuthUnauthorizedException ex)
        {
            return Unauthorized(new ApiErrorResponse(ex.Message));
        }
        catch (AuthConflictException ex)
        {
            return Conflict(new ApiErrorResponse(ex.Message));
        }
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] AuthLogoutRequest? request, CancellationToken cancellationToken)
    {
        var sessionId = User.FindFirst("sid")?.Value;
        await _authService.LogoutAsync(sessionId, request?.RefreshToken, cancellationToken);
        return NoContent();
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(AuthMeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthMeResponse>> Me(CancellationToken cancellationToken)
    {
        try
        {
            var sessionId = User.FindFirst("sid")?.Value;
            DateTime? expiresAtUtc = null;
            var expClaim = User.FindFirst("exp")?.Value;
            if (long.TryParse(expClaim, out var expUnix))
            {
                expiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
            }

            var me = await _authService.GetMeAsync(sessionId, expiresAtUtc, cancellationToken);
            return Ok(me);
        }
        catch (AuthUnauthorizedException ex)
        {
            return Unauthorized(new ApiErrorResponse(ex.Message));
        }
    }

    [HttpPost("password/reset-request")]
    [ProducesResponseType(typeof(AuthResetRequestResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResetRequestResponse>> PasswordResetRequest([FromBody] AuthResetRequestRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.PasswordResetRequestAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString(), cancellationToken);
            return Accepted(response);
        }
        catch (AuthValidationException ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }

    [HttpPost("password/reset-confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PasswordResetConfirm([FromBody] AuthResetConfirmRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _authService.PasswordResetConfirmAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString(), cancellationToken);
            return NoContent();
        }
        catch (AuthValidationException ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
        catch (AuthUnauthorizedException ex)
        {
            return Unauthorized(new ApiErrorResponse(ex.Message));
        }
        catch (AuthConflictException ex)
        {
            return Conflict(new ApiErrorResponse(ex.Message));
        }
    }
}
