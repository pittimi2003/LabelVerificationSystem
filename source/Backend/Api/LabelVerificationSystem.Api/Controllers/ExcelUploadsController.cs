using LabelVerificationSystem.Api.Contracts;
using LabelVerificationSystem.Application.Contracts.ExcelUploads;
using LabelVerificationSystem.Application.Interfaces.ExcelUploads;
using Microsoft.AspNetCore.Mvc;

namespace LabelVerificationSystem.Api.Controllers;

[ApiController]
[Route("api/excel-uploads")]
public sealed class ExcelUploadsController : ControllerBase
{
    private readonly IExcelUploadService _excelUploadService;

    public ExcelUploadsController(IExcelUploadService excelUploadService)
    {
        _excelUploadService = excelUploadService;
    }

    [HttpPost]
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
