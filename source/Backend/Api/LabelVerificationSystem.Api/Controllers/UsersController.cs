using LabelVerificationSystem.Api.Contracts;
using LabelVerificationSystem.Application.Contracts.Users;
using LabelVerificationSystem.Application.Interfaces.Auth;
using LabelVerificationSystem.Application.Interfaces.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabelVerificationSystem.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserAdministrationService _userAdministrationService;

    public UsersController(IUserAdministrationService userAdministrationService)
    {
        _userAdministrationService = userAdministrationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(UserListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserListResponse>> List(
        [FromQuery] string? query,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _userAdministrationService.ListAsync(query, isActive, page, pageSize, cancellationToken);
            return Ok(response);
        }
        catch (AuthValidationException ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDetailDto>> GetByUserId(string userId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _userAdministrationService.GetByUserIdAsync(userId, cancellationToken);
            return Ok(response);
        }
        catch (AuthUnauthorizedException ex)
        {
            return NotFound(new ApiErrorResponse(ex.Message));
        }
        catch (AuthValidationException ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDetailDto>> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _userAdministrationService.CreateAsync(
                request,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString(),
                cancellationToken);
            return CreatedAtAction(nameof(GetByUserId), new { userId = response.UserId }, response);
        }
        catch (AuthValidationException ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
        catch (AuthConflictException ex)
        {
            return Conflict(new ApiErrorResponse(ex.Message));
        }
    }

    [HttpPut("{userId}")]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDetailDto>> Update(string userId, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _userAdministrationService.UpdateAsync(
                userId,
                request,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString(),
                cancellationToken);
            return Ok(response);
        }
        catch (AuthValidationException ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
        catch (AuthUnauthorizedException ex)
        {
            return NotFound(new ApiErrorResponse(ex.Message));
        }
        catch (AuthConflictException ex)
        {
            return Conflict(new ApiErrorResponse(ex.Message));
        }
    }

    [HttpPatch("{userId}/activation")]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDetailDto>> SetActivation(string userId, [FromBody] SetUserActivationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _userAdministrationService.SetActivationAsync(userId, request.IsActive, cancellationToken);
            return Ok(response);
        }
        catch (AuthValidationException ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
        catch (AuthUnauthorizedException ex)
        {
            return NotFound(new ApiErrorResponse(ex.Message));
        }
    }
}
