using System.Globalization;
using System.IO.Compression;
using System.Security;
using System.Text;

namespace Application.Common.Xlsx;

/// <summary>
/// Writes a plain Excel workbook (.xlsx, Office Open XML) without a library: one sheet per table, a bold header
/// row, numbers as numbers and texts as inline strings. Enough for exports opened in Excel, LibreOffice or Google
/// Sheets; no formulas or formats beyond the header.
/// </summary>
internal static class SimpleXlsxWriter
{
    /// <summary>
    /// Media type of an .xlsx file.
    /// </summary>
    public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    /// <summary>
    /// Longest sheet name Excel accepts.
    /// </summary>
    private const int SheetNameMax = 31;

    /// <summary>
    /// The workbook of the sheets.
    /// </summary>
    /// <param name="sheets">Sheets in order.</param>
    /// <returns>Content of the file.</returns>
    public static byte[] Write(IReadOnlyList<XlsxSheet> sheets)
    {
        using var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            Add(zip, "[Content_Types].xml", ContentTypes(sheets.Count));
            Add(zip, "_rels/.rels",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                + "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
                + "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>"
                + "</Relationships>");
            Add(zip, "xl/workbook.xml", Workbook(sheets));
            Add(zip, "xl/_rels/workbook.xml.rels", WorkbookRelations(sheets.Count));
            Add(zip, "xl/styles.xml",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                + "<styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">"
                + "<fonts count=\"2\"><font><sz val=\"11\"/><name val=\"Calibri\"/></font><font><b/><sz val=\"11\"/><name val=\"Calibri\"/></font></fonts>"
                + "<fills count=\"2\"><fill><patternFill patternType=\"none\"/></fill><fill><patternFill patternType=\"gray125\"/></fill></fills>"
                + "<borders count=\"1\"><border><left/><right/><top/><bottom/><diagonal/></border></borders>"
                + "<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs>"
                + "<cellXfs count=\"2\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\"/>"
                + "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyFont=\"1\"/></cellXfs>"
                + "</styleSheet>");
            for (int i = 0; i < sheets.Count; i++)
            {
                Add(zip, $"xl/worksheets/sheet{i + 1}.xml", Sheet(sheets[i]));
            }
        }

        return stream.ToArray();
    }

    private static void Add(ZipArchive zip, string name, string content)
    {
        ZipArchiveEntry entry = zip.CreateEntry(name, CompressionLevel.Optimal);
        using Stream target = entry.Open();
        byte[] bytes = new UTF8Encoding(false).GetBytes(content);
        target.Write(bytes, 0, bytes.Length);
    }

    private static string ContentTypes(int sheets)
    {
        var builder = new StringBuilder(
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
            + "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">"
            + "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>"
            + "<Default Extension=\"xml\" ContentType=\"application/xml\"/>"
            + "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>"
            + "<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>");
        for (int i = 1; i <= sheets; i++)
        {
            builder.Append(CultureInfo.InvariantCulture,
                $"<Override PartName=\"/xl/worksheets/sheet{i}.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>");
        }

        return builder.Append("</Types>").ToString();
    }

    private static string Workbook(IReadOnlyList<XlsxSheet> sheets)
    {
        var builder = new StringBuilder(
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
            + "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" "
            + "xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets>");
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < sheets.Count; i++)
        {
            string name = SheetName(sheets[i].Name, i + 1, used);
            builder.Append(CultureInfo.InvariantCulture,
                $"<sheet name=\"{Escape(name)}\" sheetId=\"{i + 1}\" r:id=\"rId{i + 1}\"/>");
        }

        return builder.Append("</sheets></workbook>").ToString();
    }

    private static string WorkbookRelations(int sheets)
    {
        var builder = new StringBuilder(
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
            + "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");
        for (int i = 1; i <= sheets; i++)
        {
            builder.Append(CultureInfo.InvariantCulture,
                $"<Relationship Id=\"rId{i}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet{i}.xml\"/>");
        }

        builder.Append(CultureInfo.InvariantCulture,
            $"<Relationship Id=\"rId{sheets + 1}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>");
        return builder.Append("</Relationships>").ToString();
    }

    private static string Sheet(XlsxSheet sheet)
    {
        var builder = new StringBuilder(
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
            + "<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>");
        Row(builder, 1, sheet.Headers.Cast<object?>(), header: true);
        for (int i = 0; i < sheet.Rows.Count; i++)
        {
            Row(builder, i + 2, sheet.Rows[i], header: false);
        }

        return builder.Append("</sheetData></worksheet>").ToString();
    }

    private static void Row(StringBuilder builder, int number, IEnumerable<object?> cells, bool header)
    {
        builder.Append(CultureInfo.InvariantCulture, $"<row r=\"{number}\">");
        int column = 0;
        foreach (object? cell in cells)
        {
            string reference = ColumnName(column++) + number.ToString(CultureInfo.InvariantCulture);
            string style = header ? " s=\"1\"" : string.Empty;
            switch (cell)
            {
                case null:
                    break;
                case decimal or int or long or double:
                    builder.Append(CultureInfo.InvariantCulture,
                        $"<c r=\"{reference}\"{style}><v>{Convert.ToString(cell, CultureInfo.InvariantCulture)}</v></c>");
                    break;
                default:
                    builder.Append(CultureInfo.InvariantCulture,
                        $"<c r=\"{reference}\" t=\"inlineStr\"{style}><is><t xml:space=\"preserve\">{Escape(Convert.ToString(cell, CultureInfo.InvariantCulture) ?? string.Empty)}</t></is></c>");
                    break;
            }
        }

        builder.Append("</row>");
    }

    /// <summary>
    /// "A", "B", …, "Z", "AA" for a zero-based column.
    /// </summary>
    private static string ColumnName(int column)
    {
        string name = string.Empty;
        for (int value = column + 1; value > 0; value = (value - 1) / 26)
        {
            name = (char)('A' + (value - 1) % 26) + name;
        }

        return name;
    }

    /// <summary>
    /// A sheet name Excel accepts: no <c>[]:*?/\</c>, at most 31 characters, unique.
    /// </summary>
    private static string SheetName(string name, int index, HashSet<string> used)
    {
        string clean = new(name.Where(c => "[]:*?/\\".IndexOf(c) < 0).ToArray());
        clean = clean.Trim();
        if (clean.Length == 0)
        {
            clean = $"Sheet{index}";
        }

        if (clean.Length > SheetNameMax)
        {
            clean = clean[..SheetNameMax];
        }

        string unique = clean;
        for (int i = 2; !used.Add(unique); i++)
        {
            string suffix = $" ({i})";
            unique = clean[..Math.Min(clean.Length, SheetNameMax - suffix.Length)] + suffix;
        }

        return unique;
    }

    /// <summary>
    /// XML text without characters XML 1.0 does not allow.
    /// </summary>
    private static string Escape(string value)
    {
        string allowed = new(value.Where(c => c is '\t' or '\n' or '\r' || c >= ' ').ToArray());
        return SecurityElement.Escape(allowed) ?? string.Empty;
    }
}

/// <summary>
/// One sheet of a workbook.
/// </summary>
/// <param name="Name">Name of the tab.</param>
/// <param name="Headers">Header row.</param>
/// <param name="Rows">Rows; a cell is a number (decimal, int), a text or null for an empty cell.</param>
internal sealed record XlsxSheet(string Name, IReadOnlyList<string> Headers, IReadOnlyList<IReadOnlyList<object?>> Rows);
