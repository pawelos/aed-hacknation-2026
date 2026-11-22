using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Import;
using Application.Abstractions.Messaging;
using Domain.Modules.ImportBatch.Model;
using SharedKernel;
using DataSourceEntity = Domain.Modules.DataSource.Model.DataSource;

namespace Application.Modules.AedImport.ImportUrl;

/// <summary>
/// Handler for <see cref="ImportAedFromUrlCommand"/>: the address given, or the one saved in the source, so a
/// holder that publishes its file at a stable address refreshes the national data set with one call.
/// </summary>
internal sealed class ImportAedFromUrlCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    IImportDownloader downloader,
    AedImporter importer) : ICommandHandler<ImportAedFromUrlCommand, ImportReport>
{
    public async ValueTask<Result<ImportReport>> Handle(ImportAedFromUrlCommand command, CancellationToken cancellationToken)
    {
        Result<DataSourceEntity> source = await DataSourceAccess.GetAsync(dbContext, userContext, command.DataSourceId, cancellationToken);
        if (source.IsFailure)
        {
            return Result.Failure<ImportReport>(source.Error);
        }

        string url = string.IsNullOrWhiteSpace(command.Url) ? source.Value.Url : command.Url.Trim();
        if (string.IsNullOrWhiteSpace(url))
        {
            return Result.Failure<ImportReport>(ImportErrors.DownloadFailed("Give the address of the file; the source has none saved."));
        }

        Result<DownloadedFile> file = await downloader.DownloadAsync(url, cancellationToken);
        if (file.IsFailure)
        {
            return Result.Failure<ImportReport>(file.Error);
        }

        Result<ImportReport> report = await importer.ImportAsync(
            command.DataSourceId,
            file.Value.FileName,
            ImportFormat.Url,
            file.Value.Content,
            command.DryRun,
            command.MarkMissingAsOutdated,
            cancellationToken);

        return report.IsSuccess ? report.Value with { FileName = url } : report;
    }
}
