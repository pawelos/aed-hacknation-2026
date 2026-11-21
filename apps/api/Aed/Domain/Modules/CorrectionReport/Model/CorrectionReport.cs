using SharedKernel;

namespace Domain.Modules.CorrectionReport.Model;

/// <summary>
/// Correction of an AED record reported by anyone on the public map (e.g. "the device is locked",
/// "it was moved to the first floor"). The owner or an administrator accepts or rejects it.
/// </summary>
public sealed class CorrectionReport : BaseEntity
{
    /// <summary>
    /// AED record the report is about.
    /// </summary>
    public Guid AedDeviceId { get; set; }

    /// <summary>
    /// What is wrong, in the reporter's words.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional e-mail or phone of the reporter, for questions.
    /// </summary>
    public string ReporterContact { get; set; } = string.Empty;

    /// <summary>
    /// State of the report.
    /// </summary>
    public CorrectionReportStatus Status { get; set; } = CorrectionReportStatus.New;

    /// <summary>
    /// Note of the person who resolved the report.
    /// </summary>
    public string ResolutionNote { get; set; } = string.Empty;

    /// <summary>
    /// When the report was resolved (UTC); null while new.
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// Who resolved the report; null while new.
    /// </summary>
    public Guid? ResolvedBy { get; set; }
}
