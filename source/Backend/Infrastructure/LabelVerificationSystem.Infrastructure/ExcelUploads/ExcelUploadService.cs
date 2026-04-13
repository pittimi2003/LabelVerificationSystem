using ClosedXML.Excel;
using LabelVerificationSystem.Application.Contracts.ExcelUploads;
using LabelVerificationSystem.Application.Interfaces.ExcelUploads;
using LabelVerificationSystem.Domain.Entities;
using LabelVerificationSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LabelVerificationSystem.Infrastructure.ExcelUploads;

public sealed class ExcelUploadService : IExcelUploadService
{
    private static readonly string[] RequiredHeaders =
    [
        "Part Number",
        "Minghua description",
        "CADUCIDAD",
        "CCO",
        "Certification EAC",
        "4 FIRST NUMERS"
    ];

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

            var headerMap = BuildHeaderMap(worksheet);
            ValidateRequiredHeaders(headerMap);

            var rowErrors = new List<ExcelUploadResultRowError>();
            var pendingRows = new List<PendingPartRow>();

            var existingPartNumbers = await _dbContext.Parts
                .AsNoTracking()
                .Select(x => x.PartNumber)
                .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, cancellationToken);

            var processedPartNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var firstDataRow = 2;
            var lastDataRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
            var totalRows = Math.Max(0, lastDataRow - 1);

            for (var rowNumber = firstDataRow; rowNumber <= lastDataRow; rowNumber++)
            {
                var rowPartNumber = GetCellValue(worksheet, rowNumber, headerMap["Part Number"]);
                var rowDescription = GetCellValue(worksheet, rowNumber, headerMap["Minghua description"]);
                var rowCaducidad = GetCellValue(worksheet, rowNumber, headerMap["CADUCIDAD"]);
                var rowCco = GetCellValue(worksheet, rowNumber, headerMap["CCO"]);
                var rowCertificationEac = GetCellValue(worksheet, rowNumber, headerMap["Certification EAC"]);
                var rowFirstFourNumbers = GetCellValue(worksheet, rowNumber, headerMap["4 FIRST NUMERS"]);

                if (string.IsNullOrWhiteSpace(rowPartNumber) &&
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

    private static void ValidateRequiredHeaders(Dictionary<string, int> headerMap)
    {
        var missingHeaders = RequiredHeaders.Where(header => !headerMap.ContainsKey(header)).ToArray();
        if (missingHeaders.Length > 0)
        {
            throw new GlobalValidationException($"Archivo inválido. Faltan columnas obligatorias: {string.Join(", ", missingHeaders)}.");
        }
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

    private static Dictionary<string, int> BuildHeaderMap(IXLWorksheet worksheet)
    {
        var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;

        for (var columnIndex = 1; columnIndex <= lastColumn; columnIndex++)
        {
            var header = worksheet.Cell(1, columnIndex).GetString().Trim();
            if (!string.IsNullOrWhiteSpace(header))
            {
                headerMap[header] = columnIndex;
            }
        }

        return headerMap;
    }

    private static string GetCellValue(IXLWorksheet worksheet, int rowNumber, int columnNumber) =>
        worksheet.Cell(rowNumber, columnNumber).GetString().Trim();

    private sealed record PendingPartRow(int RowNumber, Part Part);

    private sealed class GlobalValidationException : InvalidOperationException
    {
        public GlobalValidationException(string message)
            : base(message)
        {
        }
    }
}
