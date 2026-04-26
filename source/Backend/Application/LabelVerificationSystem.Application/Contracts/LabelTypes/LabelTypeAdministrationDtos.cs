namespace LabelVerificationSystem.Application.Contracts.LabelTypes;

public sealed record LabelTypeListQuery(string? Query, bool? IsActive, int Page, int PageSize);

public sealed record LabelTypeListItemDto(
    Guid Id,
    string Name,
    string Columns,
    bool IsActive,
    DateTime CreatedAtUtc,
    string CreatedByUserId,
    string CreatedByUserName,
    DateTime UpdatedAtUtc,
    string UpdatedByUserId,
    string UpdatedByUserName);

public sealed record LabelTypeListResponse(IReadOnlyList<LabelTypeListItemDto> Items, int Page, int PageSize, int TotalItems, int TotalPages);

public sealed record LabelTypeDetailDto(
    Guid Id,
    string Name,
    string Columns,
    bool IsActive,
    DateTime CreatedAtUtc,
    string CreatedByUserId,
    string CreatedByUserName,
    DateTime UpdatedAtUtc,
    string UpdatedByUserId,
    string UpdatedByUserName);

public sealed record CreateLabelTypeRequest(string Name, IReadOnlyList<string> Columns, string ActorUserId, string ActorUserName);
public sealed record UpdateLabelTypeRequest(string Name, IReadOnlyList<string> Columns, bool IsActive, string ActorUserId, string ActorUserName);
public sealed record LabelTypeActivationRequest(bool IsActive, string ActorUserId, string ActorUserName);
