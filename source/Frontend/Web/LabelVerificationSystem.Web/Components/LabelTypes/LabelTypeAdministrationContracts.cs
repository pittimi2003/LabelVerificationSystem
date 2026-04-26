namespace LabelVerificationSystem.Web.Components.LabelTypes;

public sealed record LabelTypeListQueryDto(string? Query, bool? IsActive, int Page, int PageSize);
public sealed record LabelTypeListItemDto(Guid Id, string Name, string Columns, bool IsActive, DateTime CreatedAtUtc, string CreatedByUserId, string CreatedByUserName, DateTime UpdatedAtUtc, string UpdatedByUserId, string UpdatedByUserName);
public sealed record LabelTypeListResponseDto(IReadOnlyList<LabelTypeListItemDto> Items, int Page, int PageSize, int TotalItems, int TotalPages);
public sealed record LabelTypeDetailDto(Guid Id, string Name, string Columns, bool IsActive, DateTime CreatedAtUtc, string CreatedByUserId, string CreatedByUserName, DateTime UpdatedAtUtc, string UpdatedByUserId, string UpdatedByUserName);
public sealed record CreateLabelTypeRequestDto(string Name, IReadOnlyList<string> Columns);
public sealed record UpdateLabelTypeRequestDto(string Name, IReadOnlyList<string> Columns, bool IsActive);
public sealed record SetLabelTypeActivationRequestDto(bool IsActive);
public sealed record ApiErrorResponseDto(string Error);
