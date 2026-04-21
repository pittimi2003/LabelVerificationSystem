using LabelVerificationSystem.Api.Auth;
using LabelVerificationSystem.Api.Contracts;
using LabelVerificationSystem.Application.Contracts.Authorization;
using LabelVerificationSystem.Application.Interfaces.Auth;
using LabelVerificationSystem.Application.Interfaces.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabelVerificationSystem.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/authorization-matrix")]
public sealed class AuthorizationMatrixController : ControllerBase
{
    private readonly IAuthorizationAdministrationService _authorizationAdministrationService;

    public AuthorizationMatrixController(IAuthorizationAdministrationService authorizationAdministrationService)
    {
        _authorizationAdministrationService = authorizationAdministrationService;
    }

    [HttpGet("roles")]
    [Authorize(Policy = AuthAuthorizationPolicies.UsersManage)]
    [ProducesResponseType(typeof(IReadOnlyList<AuthorizationRoleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AuthorizationRoleDto>>> ListRoles(CancellationToken cancellationToken)
    {
        var roles = await _authorizationAdministrationService.ListRolesAsync(cancellationToken);
        return Ok(roles);
    }

    [HttpGet("roles/{roleCode}")]
    [Authorize(Policy = AuthAuthorizationPolicies.UsersManage)]
    [ProducesResponseType(typeof(RoleAuthorizationMatrixDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleAuthorizationMatrixDto>> GetRoleMatrix(string roleCode, CancellationToken cancellationToken)
    {
        try
        {
            var matrix = await _authorizationAdministrationService.GetRoleMatrixAsync(roleCode, cancellationToken);
            return Ok(matrix);
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

    [HttpPut("roles/{roleCode}")]
    [Authorize(Policy = AuthAuthorizationPolicies.UsersManage)]
    [ProducesResponseType(typeof(RoleAuthorizationMatrixDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleAuthorizationMatrixDto>> UpdateRoleMatrix(
        string roleCode,
        [FromBody] UpdateRoleAuthorizationMatrixRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var matrix = await _authorizationAdministrationService.UpdateRoleMatrixAsync(roleCode, request, cancellationToken);
            return Ok(matrix);
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
