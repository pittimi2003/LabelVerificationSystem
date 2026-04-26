namespace LabelVerificationSystem.Web.Components.Parts;

public sealed record PartListQueryDto(
    string? PartNumber,
    string? Model,
    string? MinghuaDescription,
    string? Cco,
    string? LabelTypeName,
    int Page,
    int PageSize);

public sealed record PartListItemDto(
    Guid Id,
    string PartNumber,
    string Model,
    string MinghuaDescription,
    int? Caducidad,
    string Cco,
    bool? CertificationEac,
    int FirstFourNumbers,
    Guid? CreatedByExcelUploadId,
    DateTime CreatedAtUtc,
    string LabelTypeName);

public sealed record PartListResponseDto(
    IReadOnlyList<PartListItemDto> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public sealed record PartDetailDto(
    Guid Id,
    string PartNumber,
    string Model,
    string MinghuaDescription,
    int? Caducidad,
    string Cco,
    bool? CertificationEac,
    int FirstFourNumbers,
    Guid? CreatedByExcelUploadId,
    DateTime CreatedAtUtc,
    string LabelTypeName);

public sealed record CreatePartRequestDto(
    string PartNumber,
    string Model,
    string MinghuaDescription,
    int? Caducidad,
    string Cco,
    bool? CertificationEac,
    int FirstFourNumbers);

public sealed record UpdatePartRequestDto(
    string PartNumber,
    string Model,
    string MinghuaDescription,
    int? Caducidad,
    string Cco,
    bool? CertificationEac,
    int FirstFourNumbers);

public sealed record ApiErrorResponseDto(string Error);
