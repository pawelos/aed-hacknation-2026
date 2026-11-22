namespace Application.Modules.AedImport.Parsing;

/// <summary>
/// One row of an imported file with its values under the field names of the reference data set
/// (<c>nazwa_lokalizacji</c>, <c>szerokosc_geograficzna</c>…), whatever the columns were called in the file.
/// </summary>
/// <param name="RowNumber">Line of the CSV (1 = header, so data starts at 2) or position of the GeoJSON feature (1-based).</param>
/// <param name="Values">Trimmed, non-empty values by field name.</param>
public sealed record ImportedRow(int RowNumber, IReadOnlyDictionary<string, string> Values)
{
    /// <summary>
    /// Value of a field, or null when the file has none.
    /// </summary>
    public string? Get(string field) => Values.TryGetValue(field, out string? value) ? value : null;
}

/// <summary>
/// Rows read from a file, with the columns that matched no field (reported as a warning, not an error).
/// </summary>
/// <param name="Rows">The rows.</param>
/// <param name="UnknownColumns">Columns or properties the import ignored.</param>
/// <param name="Error">Why the file could not be read at all; null when it was read.</param>
public sealed record ParsedFile(IReadOnlyList<ImportedRow> Rows, IReadOnlyList<string> UnknownColumns, string? Error)
{
    /// <summary>
    /// A file that could not be read.
    /// </summary>
    public static ParsedFile Failed(string error) => new([], [], error);
}
