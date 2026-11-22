namespace Application.Modules.AedImport;

/// <summary>
/// What the import did with a row.
/// </summary>
public enum ImportRowAction
{
    /// <summary>
    /// New record.
    /// </summary>
    Created,

    /// <summary>
    /// Existing record of the source changed (same source id, or the same place without one).
    /// </summary>
    Updated,

    /// <summary>
    /// Existing record of the source, nothing changed.
    /// </summary>
    Unchanged,

    /// <summary>
    /// Not imported because of errors.
    /// </summary>
    Rejected
}

/// <summary>
/// Report of one row.
/// </summary>
public sealed record ImportRowReport
{
    public required int Row { get; init; }

    public required ImportRowAction Action { get; init; }

    public string? SourceRecordId { get; init; }

    public string? LocationName { get; init; }

    /// <summary>
    /// The record in the national data set (new or updated); null for a rejected row.
    /// </summary>
    public Guid? RecordId { get; init; }

    public IReadOnlyList<ImportIssue> Errors { get; init; } = [];

    public IReadOnlyList<ImportIssue> Warnings { get; init; } = [];

    /// <summary>
    /// Record of another source this one probably duplicates; null when none.
    /// </summary>
    public Guid? PossibleDuplicateOf { get; init; }
}

/// <summary>
/// Report of an import: counters and the rows with errors or warnings (all rows of a dry run).
/// </summary>
public sealed record ImportReport
{
    /// <summary>
    /// Stored import (history); null for a dry run.
    /// </summary>
    public Guid? ImportId { get; init; }

    public required bool DryRun { get; init; }

    public required Guid DataSourceId { get; init; }

    public required string DataSourceName { get; init; }

    public required string FileName { get; init; }

    public required string Format { get; init; }

    public required int TotalRows { get; init; }

    public required int Created { get; init; }

    public required int Updated { get; init; }

    public required int Unchanged { get; init; }

    public required int Rejected { get; init; }

    /// <summary>
    /// Rows imported with warnings (marked as needing verification).
    /// </summary>
    public required int WithWarnings { get; init; }

    public required int PossibleDuplicates { get; init; }

    /// <summary>
    /// Records of the source missing from the file and marked as outdated (only with markMissingAsOutdated).
    /// </summary>
    public required int MarkedOutdated { get; init; }

    /// <summary>
    /// Columns or properties of the file that match no field of the reference data set (ignored).
    /// </summary>
    public required IReadOnlyList<string> UnknownColumns { get; init; }

    public required IReadOnlyList<ImportRowReport> Rows { get; init; }
}
