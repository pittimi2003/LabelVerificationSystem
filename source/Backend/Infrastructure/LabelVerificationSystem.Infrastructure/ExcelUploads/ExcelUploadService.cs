using ClosedXML.Excel;
using LabelVerificationSystem.Application.Contracts.ExcelUploads;
using LabelVerificationSystem.Application.Interfaces.ExcelUploads;
using LabelVerificationSystem.Domain.Entities;
using LabelVerificationSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LabelVerificationSystem.Infrastructure.ExcelUploads;

public sealed class ExcelUploadService : IExcelUploadService
{
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
    private static readonly string[] RequiredHeaderNames =
    [
        "Part Number",
        "Model",
        "Minghua description",
        "CADUCIDAD",
        "CCO",
        "Certification EAC",
        "4 FIRST NUMERS"
    ];
    private static readonly IReadOnlyDictionary<string, string> RequiredHeadersByNormalizedName =
        RequiredHeaderNames.ToDictionary(NormalizeHeader, header => header, StringComparer.OrdinalIgnoreCase);
    private static readonly string PartNumberHeader = NormalizeHeader("Part Number");
    private static readonly string ModelHeader = NormalizeHeader("Model");
    private static readonly string MinghuaDescriptionHeader = NormalizeHeader("Minghua description");
    private static readonly string CaducidadHeader = NormalizeHeader("CADUCIDAD");
    private static readonly string CcoHeader = NormalizeHeader("CCO");
    private static readonly string CertificationEacHeader = NormalizeHeader("Certification EAC");
    private static readonly string FirstFourNumbersHeader = NormalizeHeader("4 FIRST NUMERS");

    private readonly AppDbContext _dbContext;
    private readonly ExcelUploadStorageOptions _storageOptions;

    public ExcelUploadService(AppDbContext dbContext, Microsoft.Extensions.Options.IOptions<ExcelUploadStorageOptions> storageOptions)
    {
        _dbContext = dbContext;
        _storageOptions = storageOptions.Value;
    }

    public async Task<ExcelUploadResult> ProcessUploadAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken)
    {
        var uploadId = Guid.NewGuid();
        string? persistedFilePath = null;

        try
        {
            persistedFilePath = await SaveOriginalFileAsync(uploadId, fileStream, originalFileName, cancellationToken);
            fileStream.Position = 0;

            using var workbook = OpenWorkbook(fileStream);
            var worksheet = workbook.Worksheets.FirstOrDefault()
                            ?? throw new GlobalValidationException("El archivo Excel no contiene hojas procesables.");

            var headerDetection = DetectHeaderRowAndBuildMap(worksheet);
            ValidateRequiredHeaders(headerDetection);
            var requiredHeaderColumns = BuildRequiredHeaderColumns(headerDetection.HeaderMap);

            var rowErrors = new List<ExcelUploadResultRowError>();
            var pendingRows = new List<PendingPartRow>();

            var existingPartNumbers = await _dbContext.Parts
                .AsNoTracking()
                .Select(x => x.PartNumber)
                .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, cancellationToken);

            var processedPartNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var firstDataRow = headerDetection.HeaderRowNumber + 1;
            var lastDataRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
            var totalRows = Math.Max(0, lastDataRow - headerDetection.HeaderRowNumber);

            for (var rowNumber = firstDataRow; rowNumber <= lastDataRow; rowNumber++)
            {
                var rowPartNumber = GetCellValue(worksheet, rowNumber, requiredHeaderColumns[PartNumberHeader]);
                var rowModel = GetCellValue(worksheet, rowNumber, requiredHeaderColumns[ModelHeader]);
                var rowDescription = GetCellValue(worksheet, rowNumber, requiredHeaderColumns[MinghuaDescriptionHeader]);
                var rowCaducidad = GetCellValue(worksheet, rowNumber, requiredHeaderColumns[CaducidadHeader]);
                var rowCco = GetCellValue(worksheet, rowNumber, requiredHeaderColumns[CcoHeader]);
                var rowCertificationEac = GetCellValue(worksheet, rowNumber, requiredHeaderColumns[CertificationEacHeader]);
                var rowFirstFourNumbers = GetCellValue(worksheet, rowNumber, requiredHeaderColumns[FirstFourNumbersHeader]);

                if (string.IsNullOrWhiteSpace(rowPartNumber) &&
                    string.IsNullOrWhiteSpace(rowModel) &&
                    string.IsNullOrWhiteSpace(rowDescription) &&
                    string.IsNullOrWhiteSpace(rowCaducidad) &&
                    string.IsNullOrWhiteSpace(rowCco) &&
                    string.IsNullOrWhiteSpace(rowCertificationEac) &&
                    string.IsNullOrWhiteSpace(rowFirstFourNumbers))
                {
                    rowErrors.Add(new ExcelUploadResultRowError(rowNumber, string.Empty, "Fila vacía."));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(rowPartNumber))
                {
                    rowErrors.Add(new ExcelUploadResultRowError(rowNumber, string.Empty, "Part Number es obligatorio."));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(rowDescription) ||
                    string.IsNullOrWhiteSpace(rowModel) ||
                    string.IsNullOrWhiteSpace(rowCaducidad) ||
                    string.IsNullOrWhiteSpace(rowCco) ||
                    string.IsNullOrWhiteSpace(rowCertificationEac) ||
                    string.IsNullOrWhiteSpace(rowFirstFourNumbers))
                {
                    rowErrors.Add(new ExcelUploadResultRowError(rowNumber, rowPartNumber, "Faltan columnas mínimas obligatorias en la fila."));
                    continue;
                }

                if (existingPartNumbers.Contains(rowPartNumber) || !processedPartNumbers.Add(rowPartNumber))
                {
                    rowErrors.Add(new ExcelUploadResultRowError(rowNumber, rowPartNumber, "Part Number duplicado."));
                    continue;
                }

                pendingRows.Add(new PendingPartRow(
                    rowNumber,
                    new Part
                    {
                        Id = Guid.NewGuid(),
                        PartNumber = rowPartNumber,
                        Model = rowModel,
                        MinghuaDescription = rowDescription,
                        Caducidad = rowCaducidad,
                        Cco = rowCco,
                        CertificationEac = rowCertificationEac,
                        FirstFourNumbers = rowFirstFourNumbers,
                        CreatedAtUtc = DateTime.UtcNow
                    }));
            }

            var insertedRows = await InsertWithDuplicateRetryAsync(pendingRows, rowErrors, cancellationToken);
            var rejectedRows = rowErrors.Count;
            var status = rejectedRows > 0 ? "ProcessedWithErrors" : "Processed";

            await SaveUploadHistoryAsync(
                uploadId,
                originalFileName,
                persistedFilePath,
                status,
                totalRows,
                insertedRows,
                rejectedRows,
                cancellationToken);

            return new ExcelUploadResult(uploadId, originalFileName, totalRows, insertedRows, rejectedRows, rowErrors);
        }
        catch (GlobalValidationException ex)
        {
            await HandleGlobalFailureAsync(uploadId, originalFileName, persistedFilePath, "FailedValidation", ex.Message, cancellationToken);
            throw new InvalidOperationException(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await HandleGlobalFailureAsync(uploadId, originalFileName, persistedFilePath, "FailedProcessing", ex.Message, cancellationToken);
            throw;
        }
        catch (Exception ex)
        {
            await HandleGlobalFailureAsync(uploadId, originalFileName, persistedFilePath, "FailedUnexpected", ex.Message, cancellationToken);
            throw new InvalidOperationException("Ocurrió un error inesperado durante la carga de Excel.");
        }
    }

    public async Task<IReadOnlyList<ExcelUploadHistoryItem>> GetHistoryAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.ExcelUploads
            .AsNoTracking()
            .OrderByDescending(x => x.UploadedAtUtc)
            .Select(MapToHistoryItem())
            .ToListAsync(cancellationToken);
    }

    public async Task<ExcelUploadHistoryItem?> GetHistoryItemByIdAsync(Guid uploadId, CancellationToken cancellationToken)
    {
        return await _dbContext.ExcelUploads
            .AsNoTracking()
            .Where(x => x.Id == uploadId)
            .Select(MapToHistoryItem())
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static XLWorkbook OpenWorkbook(Stream fileStream)
    {
        try
        {
            return new XLWorkbook(fileStream);
        }
        catch
        {
            throw new GlobalValidationException("El archivo recibido no es un Excel válido.");
        }
    }

    private static void ValidateRequiredHeaders(HeaderDetectionResult headerDetection)
    {
        var headerMap = headerDetection.HeaderMap;
        var missingHeaders = RequiredHeadersByNormalizedName
            .Where(required => !headerMap.ContainsKey(required.Key))
            .Select(required => required.Value)
            .ToArray();

        if (missingHeaders.Length > 0)
        {
            var detectedHeaders = headerMap.Values
                .OrderBy(header => header.ColumnNumber)
                .Select(header => $"{header.DetectedHeader} (normalizado: {header.NormalizedHeader})")
                .ToArray();

            throw new GlobalValidationException(
                $"Archivo inválido. Faltan columnas obligatorias: {string.Join(", ", missingHeaders)}. " +
                $"Fila tomada como encabezado: {headerDetection.HeaderRowNumber}. " +
                $"Encabezados detectados en esa fila: {string.Join(", ", detectedHeaders)}.");
        }
    }

    private static IReadOnlyDictionary<string, int> BuildRequiredHeaderColumns(IReadOnlyDictionary<string, HeaderColumnInfo> headerMap)
    {
        return RequiredHeadersByNormalizedName.Keys.ToDictionary(
            normalizedRequiredHeader => normalizedRequiredHeader,
            normalizedRequiredHeader => headerMap[normalizedRequiredHeader].ColumnNumber,
            StringComparer.OrdinalIgnoreCase);
    }

    private async Task<int> InsertWithDuplicateRetryAsync(
        IReadOnlyCollection<PendingPartRow> pendingRows,
        ICollection<ExcelUploadResultRowError> rowErrors,
        CancellationToken cancellationToken)
    {
        var remaining = pendingRows.ToList();
        var insertedRows = 0;

        while (remaining.Count > 0)
        {
            try
            {
                await _dbContext.Parts.AddRangeAsync(remaining.Select(x => x.Part), cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                insertedRows += remaining.Count;
                break;
            }
            catch (DbUpdateException)
            {
                foreach (var entry in _dbContext.ChangeTracker.Entries<Part>().Where(x => x.State == EntityState.Added))
                {
                    entry.State = EntityState.Detached;
                }

                var partNumbers = remaining.Select(x => x.Part.PartNumber).ToArray();
                var nowExisting = await _dbContext.Parts
                    .AsNoTracking()
                    .Where(x => partNumbers.Contains(x.PartNumber))
                    .Select(x => x.PartNumber)
                    .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, cancellationToken);

                if (nowExisting.Count == 0)
                {
                    throw new InvalidOperationException("No se pudo guardar la carga por un conflicto de concurrencia no resoluble.");
                }

                var duplicatedNow = remaining.Where(x => nowExisting.Contains(x.Part.PartNumber)).ToList();
                foreach (var duplicated in duplicatedNow)
                {
                    rowErrors.Add(new ExcelUploadResultRowError(
                        duplicated.RowNumber,
                        duplicated.Part.PartNumber,
                        "Part Number duplicado (detectado al guardar)."));
                }

                remaining = remaining.Where(x => !nowExisting.Contains(x.Part.PartNumber)).ToList();
            }
        }

        return insertedRows;
    }

    private async Task HandleGlobalFailureAsync(
        Guid uploadId,
        string originalFileName,
        string? persistedFilePath,
        string failureStatus,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(persistedFilePath))
        {
            return;
        }

        try
        {
            await SaveUploadHistoryAsync(
                uploadId,
                originalFileName,
                persistedFilePath,
                failureStatus,
                0,
                0,
                0,
                cancellationToken);
        }
        catch
        {
            TryDeleteFile(persistedFilePath);
            throw new InvalidOperationException($"{errorMessage} Además, no se pudo registrar el historial de la carga fallida.");
        }
    }

    private async Task SaveUploadHistoryAsync(
        Guid uploadId,
        string originalFileName,
        string storedFilePath,
        string status,
        int totalRows,
        int insertedRows,
        int rejectedRows,
        CancellationToken cancellationToken)
    {
        _dbContext.ExcelUploads.Add(new ExcelUpload
        {
            Id = uploadId,
            OriginalFileName = originalFileName,
            StoredFilePath = storedFilePath,
            UploadedAtUtc = DateTime.UtcNow,
            Status = status,
            TotalRows = totalRows,
            InsertedRows = insertedRows,
            RejectedRows = rejectedRows
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> SaveOriginalFileAsync(Guid uploadId, Stream source, string originalFileName, CancellationToken cancellationToken)
    {
        var rootPath = Path.GetFullPath(_storageOptions.RootFolder);
        Directory.CreateDirectory(rootPath);

        var extension = Path.GetExtension(originalFileName);
        var safeExtension = string.IsNullOrWhiteSpace(extension) ? ".xlsx" : extension;
        var fileName = $"{uploadId:N}{safeExtension}";
        var absolutePath = Path.Combine(rootPath, fileName);

        source.Position = 0;
        await using var output = File.Create(absolutePath);
        await source.CopyToAsync(output, cancellationToken);

        return absolutePath;
    }

    private static void TryDeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch
        {
            // Intencionalmente vacío: no se relanza para no ocultar error principal.
        }
    }

    private static HeaderDetectionResult DetectHeaderRowAndBuildMap(IXLWorksheet worksheet)
    {
        var firstUsedRow = worksheet.FirstRowUsed()?.RowNumber() ?? 1;
        var lastUsedRow = worksheet.LastRowUsed()?.RowNumber() ?? firstUsedRow;
        var maxRowsToInspect = 50;
        var lastCandidateRow = Math.Min(lastUsedRow, firstUsedRow + maxRowsToInspect - 1);

        var bestHeaderRow = firstUsedRow;
        var bestHeaderMap = BuildHeaderMap(worksheet, bestHeaderRow);
        var bestMatchCount = CountRequiredHeaderMatches(bestHeaderMap);

        for (var rowNumber = firstUsedRow + 1; rowNumber <= lastCandidateRow; rowNumber++)
        {
            var candidateHeaderMap = BuildHeaderMap(worksheet, rowNumber);
            var candidateMatchCount = CountRequiredHeaderMatches(candidateHeaderMap);

            if (candidateMatchCount > bestMatchCount)
            {
                bestHeaderRow = rowNumber;
                bestHeaderMap = candidateHeaderMap;
                bestMatchCount = candidateMatchCount;
            }
        }

        return new HeaderDetectionResult(bestHeaderRow, bestHeaderMap, bestMatchCount);
    }

    private static int CountRequiredHeaderMatches(IReadOnlyDictionary<string, HeaderColumnInfo> headerMap)
    {
        return RequiredHeadersByNormalizedName.Keys.Count(headerMap.ContainsKey);
    }

    private static Dictionary<string, HeaderColumnInfo> BuildHeaderMap(IXLWorksheet worksheet, int headerRowNumber)
    {
        var headerMap = new Dictionary<string, HeaderColumnInfo>(StringComparer.OrdinalIgnoreCase);
        var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;

        for (var columnIndex = 1; columnIndex <= lastColumn; columnIndex++)
        {
            var rawHeader = worksheet.Cell(headerRowNumber, columnIndex).GetString();
            var normalizedHeader = NormalizeHeader(rawHeader);
            if (!string.IsNullOrWhiteSpace(normalizedHeader))
            {
                var canonicalHeader = NormalizeSpacing(rawHeader);
                headerMap.TryAdd(
                    normalizedHeader,
                    new HeaderColumnInfo(columnIndex, canonicalHeader, normalizedHeader));
            }
        }

        return headerMap;
    }

    private static string NormalizeHeader(string header)
    {
        if (string.IsNullOrWhiteSpace(header))
        {
            return string.Empty;
        }

        var canonicalSpacing = NormalizeSpacing(header);
        var withoutDiacritics = RemoveDiacritics(canonicalSpacing);

        return withoutDiacritics
            .Trim()
            .ToUpperInvariant();
    }

    private static string NormalizeSpacing(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var headerWithReplacedNonBreakingSpaces = value
            .Replace('\u00A0', ' ')
            .Replace('\u202F', ' ')
            .Replace('\u2007', ' ');

        return WhitespaceRegex
            .Replace(headerWithReplacedNonBreakingSpaces, " ")
            .Trim();
    }

    private static string RemoveDiacritics(string text)
    {
        var decomposed = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string GetCellValue(IXLWorksheet worksheet, int rowNumber, int columnNumber) =>
        worksheet.Cell(rowNumber, columnNumber).GetString().Trim();

    private static System.Linq.Expressions.Expression<Func<LabelVerificationSystem.Domain.Entities.ExcelUpload, ExcelUploadHistoryItem>> MapToHistoryItem() =>
        x => new ExcelUploadHistoryItem(
            x.Id,
            x.OriginalFileName,
            x.UploadedAtUtc,
            x.Status,
            x.TotalRows,
            x.InsertedRows,
            x.RejectedRows);

    private sealed record PendingPartRow(int RowNumber, Part Part);
    private sealed record HeaderColumnInfo(int ColumnNumber, string DetectedHeader, string NormalizedHeader);
    private sealed record HeaderDetectionResult(int HeaderRowNumber, IReadOnlyDictionary<string, HeaderColumnInfo> HeaderMap, int MatchedRequiredHeaderCount);

    private sealed class GlobalValidationException : InvalidOperationException
    {
        public GlobalValidationException(string message)
            : base(message)
        {
        }
    }
}
