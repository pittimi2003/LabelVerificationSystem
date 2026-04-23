using LabelVerificationSystem.Application.Contracts.Parts;
using LabelVerificationSystem.Application.Interfaces.Auth;
using LabelVerificationSystem.Application.Interfaces.Parts;
using LabelVerificationSystem.Domain.Entities;
using LabelVerificationSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LabelVerificationSystem.Infrastructure.Parts;

public sealed class PartAdministrationService : IPartAdministrationService
{
    private readonly AppDbContext _dbContext;

    public PartAdministrationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PartListResponse> ListAsync(PartListQuery query, CancellationToken cancellationToken)
    {
        if (query.Page <= 0)
        {
            throw new AuthValidationException("page debe ser mayor o igual a 1.");
        }

        if (query.PageSize is < 1 or > 100)
        {
            throw new AuthValidationException("pageSize debe estar entre 1 y 100.");
        }

        var partsQuery = _dbContext.Parts.AsNoTracking();

        var partNumber = NormalizeFilter(query.PartNumber);
        if (!string.IsNullOrWhiteSpace(partNumber))
        {
            partsQuery = partsQuery.Where(x => x.PartNumber.ToLower().Contains(partNumber));
        }

        var model = NormalizeFilter(query.Model);
        if (!string.IsNullOrWhiteSpace(model))
        {
            partsQuery = partsQuery.Where(x => x.Model.ToLower().Contains(model));
        }

        var minghuaDescription = NormalizeFilter(query.MinghuaDescription);
        if (!string.IsNullOrWhiteSpace(minghuaDescription))
        {
            partsQuery = partsQuery.Where(x => x.MinghuaDescription.ToLower().Contains(minghuaDescription));
        }

        var cco = NormalizeFilter(query.Cco);
        if (!string.IsNullOrWhiteSpace(cco))
        {
            partsQuery = partsQuery.Where(x => x.Cco.ToLower().Contains(cco));
        }

        var totalItems = await partsQuery.CountAsync(cancellationToken);
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)query.PageSize);

        var items = await partsQuery
            .OrderBy(x => x.PartNumber)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(MapToListItem())
            .ToListAsync(cancellationToken);

        return new PartListResponse(items, query.Page, query.PageSize, totalItems, totalPages);
    }

    public async Task<PartDetailDto> GetByIdAsync(Guid partId, CancellationToken cancellationToken)
    {
        if (partId == Guid.Empty)
        {
            throw new AuthValidationException("partId es requerido.");
        }

        var item = await _dbContext.Parts
            .AsNoTracking()
            .Where(x => x.Id == partId)
            .Select(MapToDetail())
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            throw new AuthUnauthorizedException("Part no encontrada.");
        }

        return item;
    }

    public async Task<PartDetailDto> CreateAsync(CreatePartRequest request, CancellationToken cancellationToken)
    {
        var normalizedPartNumber = ValidateRequiredTrimmed(request.PartNumber, "partNumber", 1, 120);
        var normalizedModel = ValidateRequiredTrimmed(request.Model, "model", 1, 120);
        var normalizedDescription = ValidateRequiredTrimmed(request.MinghuaDescription, "minghuaDescription", 1, 400);
        var normalizedCco = ValidateRequiredTrimmed(request.Cco, "cco", 1, 120);
        ValidateCaducidad(request.Caducidad);

        if (await _dbContext.Parts.AnyAsync(x => x.PartNumber == normalizedPartNumber, cancellationToken))
        {
            throw new AuthConflictException("partNumber ya está en uso.");
        }

        var entity = new Part
        {
            Id = Guid.NewGuid(),
            PartNumber = normalizedPartNumber,
            Model = normalizedModel,
            MinghuaDescription = normalizedDescription,
            Caducidad = request.Caducidad,
            Cco = normalizedCco,
            CertificationEac = request.CertificationEac,
            FirstFourNumbers = request.FirstFourNumbers,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Parts.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDetail(entity);
    }

    public async Task<PartDetailDto> UpdateAsync(Guid partId, UpdatePartRequest request, CancellationToken cancellationToken)
    {
        if (partId == Guid.Empty)
        {
            throw new AuthValidationException("partId es requerido.");
        }

        var normalizedPartNumber = ValidateRequiredTrimmed(request.PartNumber, "partNumber", 1, 120);
        var normalizedModel = ValidateRequiredTrimmed(request.Model, "model", 1, 120);
        var normalizedDescription = ValidateRequiredTrimmed(request.MinghuaDescription, "minghuaDescription", 1, 400);
        var normalizedCco = ValidateRequiredTrimmed(request.Cco, "cco", 1, 120);
        ValidateCaducidad(request.Caducidad);

        var entity = await _dbContext.Parts.FirstOrDefaultAsync(x => x.Id == partId, cancellationToken);
        if (entity is null)
        {
            throw new AuthUnauthorizedException("Part no encontrada.");
        }

        var partNumberInUse = await _dbContext.Parts.AnyAsync(x => x.Id != partId && x.PartNumber == normalizedPartNumber, cancellationToken);
        if (partNumberInUse)
        {
            throw new AuthConflictException("partNumber ya está en uso.");
        }

        entity.PartNumber = normalizedPartNumber;
        entity.Model = normalizedModel;
        entity.MinghuaDescription = normalizedDescription;
        entity.Caducidad = request.Caducidad;
        entity.Cco = normalizedCco;
        entity.CertificationEac = request.CertificationEac;
        entity.FirstFourNumbers = request.FirstFourNumbers;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDetail(entity);
    }

    private static string? NormalizeFilter(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static string ValidateRequiredTrimmed(string? value, string field, int minLength, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new AuthValidationException($"{field} es requerido.");
        }

        var normalized = value.Trim();
        if (normalized.Length < minLength || normalized.Length > maxLength)
        {
            throw new AuthValidationException($"{field} debe tener entre {minLength} y {maxLength} caracteres.");
        }

        return normalized;
    }

    private static void ValidateCaducidad(int? caducidad)
    {
        if (caducidad is < 0)
        {
            throw new AuthValidationException("caducidad no puede ser negativa.");
        }
    }

    private static System.Linq.Expressions.Expression<Func<Part, PartListItemDto>> MapToListItem() =>
        x => new PartListItemDto(
            x.Id,
            x.PartNumber,
            x.Model,
            x.MinghuaDescription,
            x.Caducidad,
            x.Cco,
            x.CertificationEac,
            x.FirstFourNumbers,
            x.CreatedByExcelUploadId,
            x.CreatedAtUtc);

    private static System.Linq.Expressions.Expression<Func<Part, PartDetailDto>> MapToDetail() =>
        x => new PartDetailDto(
            x.Id,
            x.PartNumber,
            x.Model,
            x.MinghuaDescription,
            x.Caducidad,
            x.Cco,
            x.CertificationEac,
            x.FirstFourNumbers,
            x.CreatedByExcelUploadId,
            x.CreatedAtUtc);

    private static PartDetailDto MapToDetail(Part x) =>
        new(
            x.Id,
            x.PartNumber,
            x.Model,
            x.MinghuaDescription,
            x.Caducidad,
            x.Cco,
            x.CertificationEac,
            x.FirstFourNumbers,
            x.CreatedByExcelUploadId,
            x.CreatedAtUtc);
}
