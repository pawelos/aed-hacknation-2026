using Application.Abstractions.Messaging;
using Domain.Modules.ImportBatch.Model;
using SharedKernel;

namespace Application.Modules.AedImport.ImportFile;

/// <summary>
/// Handler for <see cref="ImportAedFileCommand"/>.
/// </summary>
internal sealed class ImportAedFileCommandHandler(AedImporter importer) : ICommandHandler<ImportAedFileCommand, ImportReport>
{
    public async ValueTask<Result<ImportReport>> Handle(ImportAedFileCommand command, CancellationToken cancellationToken) =>
        await importer.ImportAsync(
            command.DataSourceId,
            command.FileName,
            ImportFormat.Csv,
            command.Content,
            command.DryRun,
            command.MarkMissingAsOutdated,
            cancellationToken);
}
