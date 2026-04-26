using System.Linq;
using System.Security.Claims;
using LabelVerificationSystem.Api.Auth;
using LabelVerificationSystem.Api.Contracts;
using LabelVerificationSystem.Application.Contracts.LabelTypes;
using LabelVerificationSystem.Application.Interfaces.Auth;
using LabelVerificationSystem.Application.Interfaces.LabelTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabelVerificationSystem.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/label-types")]
public sealed class LabelTypesController : ControllerBase
{
    private readonly ILabelTypeAdministrationService _service;

    public LabelTypesController(ILabelTypeAdministrationService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = AuthAuthorizationPolicies.LabelTypesRead)]
    public async Task<ActionResult<LabelTypeListResponse>> List([FromQuery] string? query, [FromQuery] bool? isActive, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try { return Ok(await _service.ListAsync(new LabelTypeListQuery(query, isActive, page, pageSize), cancellationToken)); }
        catch (AuthValidationException ex) { return BadRequest(new ApiErrorResponse(ex.Message)); }
    }

    [HttpGet("available-columns")]
    [Authorize(Policy = AuthAuthorizationPolicies.LabelTypesRead)]
    public async Task<ActionResult<IReadOnlyList<string>>> AvailableColumns(CancellationToken cancellationToken)
        => Ok(await _service.GetAvailableColumnsAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AuthAuthorizationPolicies.LabelTypesRead)]
    public async Task<ActionResult<LabelTypeDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(await _service.GetByIdAsync(id, cancellationToken)); }
        catch (AuthUnauthorizedException ex) { return NotFound(new ApiErrorResponse(ex.Message)); }
    }

    [HttpPost]
    [Authorize(Policy = AuthAuthorizationPolicies.LabelTypesCreate)]
    public async Task<ActionResult<LabelTypeDetailDto>> Create([FromBody] CreateLabelTypeApiRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _service.CreateAsync(new CreateLabelTypeRequest(request.Name, request.Rules.Select(x => new LabelTypeRuleDto(x.ColumnName, x.ExpectedValue)).ToList(), GetActorId(), GetActorName()), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (AuthValidationException ex) { return BadRequest(new ApiErrorResponse(ex.Message)); }
        catch (AuthConflictException ex) { return Conflict(new ApiErrorResponse(ex.Message)); }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthAuthorizationPolicies.LabelTypesEdit)]
    public async Task<ActionResult<LabelTypeDetailDto>> Update(Guid id, [FromBody] UpdateLabelTypeApiRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.UpdateAsync(id, new UpdateLabelTypeRequest(request.Name, request.Rules.Select(x => new LabelTypeRuleDto(x.ColumnName, x.ExpectedValue)).ToList(), request.IsActive, GetActorId(), GetActorName()), cancellationToken));
        }
        catch (AuthValidationException ex) { return BadRequest(new ApiErrorResponse(ex.Message)); }
        catch (AuthUnauthorizedException ex) { return NotFound(new ApiErrorResponse(ex.Message)); }
        catch (AuthConflictException ex) { return Conflict(new ApiErrorResponse(ex.Message)); }
    }

    [HttpPatch("{id:guid}/activation")]
    [Authorize(Policy = AuthAuthorizationPolicies.LabelTypesActivateDeactivate)]
    public async Task<ActionResult<LabelTypeDetailDto>> SetActivation(Guid id, [FromBody] SetLabelTypeActivationApiRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.SetActivationAsync(id, new LabelTypeActivationRequest(request.IsActive, GetActorId(), GetActorName()), cancellationToken));
        }
        catch (AuthValidationException ex) { return BadRequest(new ApiErrorResponse(ex.Message)); }
        catch (AuthUnauthorizedException ex) { return NotFound(new ApiErrorResponse(ex.Message)); }
    }

    private string GetActorId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? "unknown";
    private string GetActorName() => User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue("unique_name") ?? "unknown";
}

public sealed record LabelTypeRuleApiRequest(string ColumnName, string ExpectedValue);
public sealed record CreateLabelTypeApiRequest(string Name, IReadOnlyList<LabelTypeRuleApiRequest> Rules);
public sealed record UpdateLabelTypeApiRequest(string Name, IReadOnlyList<LabelTypeRuleApiRequest> Rules, bool IsActive);
public sealed record SetLabelTypeActivationApiRequest(bool IsActive);
