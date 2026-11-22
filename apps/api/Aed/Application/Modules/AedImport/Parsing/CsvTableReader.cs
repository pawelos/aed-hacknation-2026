using Application.Common.Localization;
using System.Text;

namespace Application.Modules.AedImport.Parsing;

/// <summary>
/// Reads a CSV file (RFC 4180): values in quotes may hold the separator, quotes ("") and line breaks. The
/// separator is detected from the header: comma (the reference format) or semicolon (Polish Excel), or tab.
/// The header names are mapped to the reference fields with <see cref="FieldAliases"/>.
/// </summary>
public static class CsvTableReader
{
    public static ParsedFile Read(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return ParsedFile.Failed(Lang.T(Lang.T("The file is empty.", "Plik jest pusty."), "Plik jest pusty."));
        }

        char separator = DetectSeparator(text);
        List<(int Line, List<string> Cells)> records = Split(text, separator);
        if (records.Count == 0)
        {
            return ParsedFile.Failed("The file is empty.");
        }

        List<string> header = records[0].Cells;
        var fields = new string?[header.Count];
        var unknown = new List<string>();
        var used = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < header.Count; i++)
        {
            string? field = FieldAliases.FieldFor(header[i]);
            // The first column of a field wins (e.g. "id_zrodlowe" before a later "id").
            if (field is not null && used.Add(field))
            {
                fields[i] = field;
            }
            else if (!string.IsNullOrWhiteSpace(header[i]))
            {
                unknown.Add(header[i].Trim());
            }
        }

        if (!used.Contains("szerokosc_geograficzna") || !used.Contains("dlugosc_geograficzna"))
        {
            return ParsedFile.Failed(Lang.T(
                "The header has no latitude and longitude columns (szerokosc_geograficzna, dlugosc_geograficzna). " +
                "Use the column names of the reference data set template.",
                "Nagłówek nie ma kolumn szerokości i długości geograficznej (szerokosc_geograficzna, dlugosc_geograficzna). " +
                "Użyj nazw kolumn z szablonu wzorcowego zakresu danych."));
        }

        var rows = new List<ImportedRow>(records.Count - 1);
        foreach ((int line, List<string> cells) in records.Skip(1))
        {
            if (cells.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int i = 0; i < cells.Count && i < fields.Length; i++)
            {
                string value = cells[i].Trim();
                if (fields[i] is { } field && value.Length > 0)
                {
                    values[field] = value;
                }
            }

            rows.Add(new ImportedRow(line, values));
        }

        return new ParsedFile(rows, unknown, null);
    }

    private static char DetectSeparator(string text)
    {
        int end = text.IndexOf('\n');
        string header = end < 0 ? text : text[..end];
        int commas = header.Count(c => c == ',');
        int semicolons = header.Count(c => c == ';');
        int tabs = header.Count(c => c == '\t');
        if (tabs > commas && tabs > semicolons)
        {
            return '\t';
        }

        return semicolons > commas ? ';' : ',';
    }

    /// <summary>
    /// Records with the line number they start on.
    /// </summary>
    private static List<(int Line, List<string> Cells)> Split(string text, char separator)
    {
        var records = new List<(int, List<string>)>();
        var cells = new List<string>();
        var cell = new StringBuilder();
        bool inQuotes = false;
        int line = 1;
        int recordLine = 1;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        cell.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    if (c == '\n')
                    {
                        line++;
                    }

                    cell.Append(c);
                }

                continue;
            }

            if (c == '"' && cell.Length == 0)
            {
                inQuotes = true;
            }
            else if (c == separator)
            {
                cells.Add(cell.ToString());
                cell.Clear();
            }
            else if (c == '\r')
            {
                // CR of CRLF: the LF ends the record.
            }
            else if (c == '\n')
            {
                cells.Add(cell.ToString());
                cell.Clear();
                records.Add((recordLine, cells));
                cells = [];
                line++;
                recordLine = line;
            }
            else
            {
                cell.Append(c);
            }
        }

        if (cell.Length > 0 || cells.Count > 0)
        {
            cells.Add(cell.ToString());
            records.Add((recordLine, cells));
        }

        return records;
    }
}
