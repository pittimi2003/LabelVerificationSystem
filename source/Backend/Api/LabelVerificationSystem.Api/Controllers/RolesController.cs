using LabelVerificationSystem.Api.Auth;
using LabelVerificationSystem.Api.Contracts;
using LabelVerificationSystem.Application.Contracts.Roles;
using LabelVerificationSystem.Application.Interfaces.Auth;
using LabelVerificationSystem.Application.Interfaces.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabelVerificationSystem.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/roles")]
public sealed class RolesController : ControllerBase
{
    private readonly IRoleCatalogAdministrationService _roleCatalogAdministrationService;

    public RolesController(IRoleCatalogAdministrationService roleCatalogAdministrationService)
    {
        _roleCatalogAdministrationService = roleCatalogAdministrationService;
    }

    [HttpGet]
    [Authorize(Policy = AuthAuthorizationPolicies.AuthorizationMatrixManage)]
    [ProducesResponseType(typeof(RoleCatalogListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RoleCatalogListResponse>> List(
        [FromQuery] string? query,
        [FromQuery] string? code,
        [FromQuery] string? name,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var listQuery = new RoleCatalogListQuery(query, code, name, isActive, page, pageSize);
            var response = await _roleCatalogAdministrationService.ListAsync(listQuery, cancellationToken);
            return Ok(response);
        }
        catch (AuthValidationException ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }

    [HttpGet("{roleCode}")]
    [Authorize(Policy = AuthAuthorizationPolicies.AuthorizationMatrixManage)]
    [ProducesResponseType(typeof(RoleCatalogDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleCatalogDetailDto>> GetByCode(string roleCode, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _roleCatalogAdministrationService.GetByCodeAsync(roleCode, cancellationToken);
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

    [HttpPatch("{roleCode}/activation")]
    [Authorize(Policy = AuthAuthorizationPolicies.AuthorizationMatrixManage)]
    [ProducesResponseType(typeof(RoleCatalogDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleCatalogDetailDto>> SetActivation(
        string roleCode,
        [FromBody] SetRoleActivationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _roleCatalogAdministrationService.SetActivationAsync(roleCode, request.IsActive, cancellationToken);
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

public sealed record SetRoleActivationRequest(bool IsActive);
