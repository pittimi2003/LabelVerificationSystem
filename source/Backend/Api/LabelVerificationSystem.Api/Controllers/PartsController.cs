using LabelVerificationSystem.Api.Auth;
using LabelVerificationSystem.Api.Contracts;
using LabelVerificationSystem.Application.Contracts.Parts;
using LabelVerificationSystem.Application.Interfaces.Auth;
using LabelVerificationSystem.Application.Interfaces.Parts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabelVerificationSystem.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/parts")]
public sealed class PartsController : ControllerBase
{
    private readonly IPartAdministrationService _partAdministrationService;

    public PartsController(IPartAdministrationService partAdministrationService)
    {
        _partAdministrationService = partAdministrationService;
    }

    [HttpGet]
    [Authorize(Policy = AuthAuthorizationPolicies.PartsRead)]
    [ProducesResponseType(typeof(PartListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PartListResponse>> List(
        [FromQuery] string? partNumber,
        [FromQuery] string? model,
        [FromQuery] string? minghuaDescription,
        [FromQuery] string? cco,
        [FromQuery] string? labelTypeName,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _partAdministrationService.ListAsync(new PartListQuery(partNumber, model, minghuaDescription, cco, labelTypeName, page, pageSize), cancellationToken);
            return Ok(response);
        }
        catch (AuthValidationException ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }

    [HttpGet("{partId:guid}")]
    [Authorize(Policy = AuthAuthorizationPolicies.PartsRead)]
    [ProducesResponseType(typeof(PartDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PartDetailDto>> GetById(Guid partId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _partAdministrationService.GetByIdAsync(partId, cancellationToken));
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

    [HttpPost]
    [Authorize(Policy = AuthAuthorizationPolicies.PartsCreate)]
    [ProducesResponseType(typeof(PartDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PartDetailDto>> Create([FromBody] CreatePartRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _partAdministrationService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { partId = created.Id }, created);
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

    [HttpPut("{partId:guid}")]
    [Authorize(Policy = AuthAuthorizationPolicies.PartsEdit)]
    [ProducesResponseType(typeof(PartDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PartDetailDto>> Update(Guid partId, [FromBody] UpdatePartRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _partAdministrationService.UpdateAsync(partId, request, cancellationToken));
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
}
