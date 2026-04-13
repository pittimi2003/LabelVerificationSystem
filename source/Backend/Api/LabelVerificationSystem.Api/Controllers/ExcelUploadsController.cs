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
    [ProducesResponseType(typeof(ExcelUploadResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ExcelUploadResult>> Upload([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "El archivo es obligatorio." });
        }

        await using var stream = file.OpenReadStream();

        try
        {
            var result = await _excelUploadService.ProcessUploadAsync(stream, file.FileName, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
