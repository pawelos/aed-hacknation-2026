using SharedKernel;

namespace Domain.Modules.ImportBatch.Model;

/// <summary>
/// One import of AED records into a data source, with its counters and the report of rejected rows.
/// </summary>
public sealed class ImportBatch : BaseEntity
{
    /// <summary>
    /// Source the records were imported into.
    /// </summary>
    public Guid DataSourceId { get; set; }

    /// <summary>
    /// Format of the imported data.
    /// </summary>
    public ImportFormat Format { get; set; }

    /// <summary>
    /// Name of the uploaded file, or the address the data was read from.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Number of rows read.
    /// </summary>
    public int TotalRows { get; set; }

    /// <summary>
    /// Number of new records.
    /// </summary>
    public int Created { get; set; }

    /// <summary>
    /// Number of records updated (same source record id).
    /// </summary>
    public int Updated { get; set; }

    /// <summary>
    /// Number of rows rejected by validation.
    /// </summary>
    public int Rejected { get; set; }

    /// <summary>
    /// Number of records marked as possible duplicates of another source.
    /// </summary>
    public int PossibleDuplicates { get; set; }

    /// <summary>
    /// Report with the errors and warnings of each row, as JSON (<c>jsonb</c>).
    /// </summary>
    public string ReportJson { get; set; } = "[]";
}
