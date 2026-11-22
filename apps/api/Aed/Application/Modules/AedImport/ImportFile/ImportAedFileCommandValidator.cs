using Application.Common.Localization;
using FluentValidation;

namespace Application.Modules.AedImport.ImportFile;

/// <summary>
/// Validates <see cref="ImportAedFileCommand"/>: a source, a file that is not empty and not too large.
/// </summary>
internal sealed class ImportAedFileCommandValidator : AbstractValidator<ImportAedFileCommand>
{
    public ImportAedFileCommandValidator()
    {
        RuleFor(x => x.DataSourceId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(AedImportConstant.MaxFileNameLength).OverridePropertyName("file");
        RuleFor(x => x.Content)
            .Must(c => c is { Length: > 0 })
            .WithErrorCode("Import.EmptyFile")
            .WithMessage(_ => Lang.T("The file is empty.", "Plik jest pusty."))
            .Must(c => c is null || c.Length <= AedImportConstant.MaxFileBytes)
            .WithErrorCode("Import.FileTooLarge")
            .WithMessage(_ => Lang.T($"The file is larger than {AedImportConstant.MaxFileBytes / 1024 / 1024} MB.", $"Plik jest większy niż {AedImportConstant.MaxFileBytes / 1024 / 1024} MB."))
            .OverridePropertyName("file");
    }
}
