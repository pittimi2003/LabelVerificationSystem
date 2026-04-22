using LabelVerificationSystem.Api.Contracts;
using LabelVerificationSystem.Api.Auth;
using LabelVerificationSystem.Application.Contracts.ExcelUploads;
using LabelVerificationSystem.Application.Interfaces.ExcelUploads;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabelVerificationSystem.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/excel-uploads")]
public sealed class ExcelUploadsController : ControllerBase
{
    private readonly IExcelUploadService _excelUploadService;

    public ExcelUploadsController(IExcelUploadService excelUploadService)
    {
        _excelUploadService = excelUploadService;
    }

    [HttpGet]
    [Authorize(Policy = AuthAuthorizationPolicies.ExcelUploadsRead)]
    [ProducesResponseType(typeof(IReadOnlyList<ExcelUploadHistoryItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ExcelUploadHistoryItem>>> GetHistory(CancellationToken cancellationToken)
    {
        var history = await _excelUploadService.GetHistoryAsync(cancellationToken);
        return Ok(history);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AuthAuthorizationPolicies.ExcelUploadsRead)]
    [ProducesResponseType(typeof(ExcelUploadHistoryItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExcelUploadHistoryItem>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var historyItem = await _excelUploadService.GetHistoryItemByIdAsync(id, cancellationToken);
        return historyItem is null ? NotFound() : Ok(historyItem);
    }


    [HttpGet("{id:guid}/details")]
    [Authorize(Policy = AuthAuthorizationPolicies.ExcelUploadsRead)]
    [ProducesResponseType(typeof(ExcelUploadDetailItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExcelUploadDetailItem>> GetDetailById(Guid id, CancellationToken cancellationToken)
    {
        var detail = await _excelUploadService.GetUploadDetailByIdAsync(id, cancellationToken);
        return detail is null ? NotFound() : Ok(detail);
    }

    [HttpPost]
    [Authorize(Policy = AuthAuthorizationPolicies.ExcelUploadsUpload)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ExcelUploadResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ExcelUploadResult>> Upload([FromForm] ExcelUploadRequest request, CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest(new ApiErrorResponse("El archivo es obligatorio."));
        }

        await using var stream = request.File.OpenReadStream();

        try
        {
            var result = await _excelUploadService.ProcessUploadAsync(stream, request.File.FileName, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }
}
