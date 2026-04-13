namespace LabelVerificationSystem.Infrastructure.ExcelUploads;

public sealed class ExcelUploadStorageOptions
{
    public const string SectionName = "ExcelUploadStorage";
    public string RootFolder { get; set; } = "App_Data/excel-uploads";
}
