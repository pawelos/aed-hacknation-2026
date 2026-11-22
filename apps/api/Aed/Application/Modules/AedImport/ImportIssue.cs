namespace Application.Modules.AedImport;

/// <summary>
/// Problem found in an imported row: an error rejects the row, a warning keeps it and marks the record as
/// needing verification.
/// </summary>
/// <param name="Field">Field of the reference data set the problem is about; null for the whole row.</param>
/// <param name="Code">Stable code, e.g. <c>Import.OutsidePoland</c> (translated by the web panel).</param>
/// <param name="Message">English description with the value that caused it.</param>
public sealed record ImportIssue(string? Field, string Code, string Message);
