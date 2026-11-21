namespace Domain.Modules.CorrectionReport.Constants;

/// <summary>
/// Route name and field limits of correction reports.
/// </summary>
public static class CorrectionReportConstant
{
    /// <summary>
    /// Controller (route) name.
    /// </summary>
    public const string ControllerName = "CorrectionReport";

    /// <summary>
    /// Maximum length of the message.
    /// </summary>
    public const int MessageMaxLength = 2000;

    /// <summary>
    /// Maximum length of the reporter contact.
    /// </summary>
    public const int ReporterContactMaxLength = 200;

    /// <summary>
    /// Maximum length of the resolution note.
    /// </summary>
    public const int ResolutionNoteMaxLength = 1000;
}
