namespace Domain.Modules.CorrectionReport.Model;

/// <summary>
/// State of a correction report. Stored as text.
/// </summary>
public enum CorrectionReportStatus
{
    /// <summary>
    /// Waiting for the owner or an administrator.
    /// </summary>
    New,

    /// <summary>
    /// The record was corrected.
    /// </summary>
    Accepted,

    /// <summary>
    /// The report was not confirmed.
    /// </summary>
    Rejected
}
