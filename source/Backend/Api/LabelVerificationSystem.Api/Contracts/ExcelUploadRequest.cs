using Microsoft.AspNetCore.Http;

namespace LabelVerificationSystem.Api.Contracts;

public sealed class ExcelUploadRequest
{
    public IFormFile? File { get; init; }
}
