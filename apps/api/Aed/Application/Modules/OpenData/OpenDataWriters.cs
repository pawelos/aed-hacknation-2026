using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Application.Modules.OpenData;

/// <summary>
/// Writes the records as CSV, GeoJSON and JSON with the fields of <see cref="AedDataSchema"/>.
/// </summary>
internal static class OpenDataWriters
{
    private static readonly JsonWriterOptions JsonOptions = new()
    {
        Indented = true,
        // Polish letters stay readable in the file (still valid JSON).
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// CSV per RFC 4180: UTF-8 without BOM, comma, CRLF, a header row with the field names, values with a
    /// comma, quote or line break in quotes. Numbers with a dot, booleans as <c>true</c>/<c>false</c>.
    /// </summary>
    public static byte[] Csv(IReadOnlyList<AedExportRow> rows)
    {
        var builder = new StringBuilder();
        builder.AppendJoin(',', AedDataSchema.Fields.Select(f => Quote(f.Name))).Append("\r\n");
        foreach (AedExportRow row in rows)
        {
            builder.AppendJoin(',', AedDataSchema.Fields.Select(f => Quote(Text(f.Value(row))))).Append("\r\n");
        }

        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(builder.ToString());
    }

    /// <summary>
    /// GeoJSON FeatureCollection (RFC 7946): a Point [longitude, latitude] per record, all fields as properties.
    /// </summary>
    public static byte[] GeoJson(IReadOnlyList<AedExportRow> rows)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, JsonOptions))
        {
            writer.WriteStartObject();
            writer.WriteString("type", "FeatureCollection");
            writer.WriteStartArray("features");
            foreach (AedExportRow row in rows)
            {
                writer.WriteStartObject();
                writer.WriteString("type", "Feature");
                writer.WriteString("id", row.Id.ToString());
                writer.WriteStartObject("geometry");
                writer.WriteString("type", "Point");
                writer.WriteStartArray("coordinates");
                writer.WriteNumberValue(row.Longitude);
                writer.WriteNumberValue(row.Latitude);
                writer.WriteEndArray();
                writer.WriteEndObject();
                writer.WritePropertyName("properties");
                WriteRecord(writer, row);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        return stream.ToArray();
    }

    /// <summary>
    /// JSON: the schema version, generation time, number of records and the records.
    /// </summary>
    public static byte[] Json(IReadOnlyList<AedExportRow> rows, DateTime generatedUtc)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, JsonOptions))
        {
            writer.WriteStartObject();
            writer.WriteString("wersja_schematu", AedDataSchema.Version);
            writer.WriteString("wygenerowano", generatedUtc.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture));
            writer.WriteNumber("liczba_rekordow", rows.Count);
            writer.WriteStartArray("rekordy");
            foreach (AedExportRow row in rows)
            {
                WriteRecord(writer, row);
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        return stream.ToArray();
    }

    private static void WriteRecord(Utf8JsonWriter writer, AedExportRow row)
    {
        writer.WriteStartObject();
        foreach (AedField field in AedDataSchema.Fields)
        {
            object? value = field.Value(row);
            switch (value)
            {
                case null:
                    writer.WriteNull(field.Name);
                    break;
                case decimal number:
                    writer.WriteNumber(field.Name, number);
                    break;
                case bool flag:
                    writer.WriteBoolean(field.Name, flag);
                    break;
                case string text when text.Length == 0:
                    // Empty optional text: null, so "not given" is not mistaken for a value.
                    writer.WriteNull(field.Name);
                    break;
                default:
                    writer.WriteString(field.Name, Convert.ToString(value, CultureInfo.InvariantCulture));
                    break;
            }
        }

        writer.WriteEndObject();
    }

    private static string Text(object? value)
    {
        switch (value)
        {
            case null:
                return string.Empty;
            case bool flag:
                return flag ? "true" : "false";
            case decimal number:
                return number.ToString("0.000000", CultureInfo.InvariantCulture);
            default:
                return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        }
    }

    private static string Quote(string value) =>
        value.IndexOfAny([',', '"', '\r', '\n']) >= 0
            ? "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\""
            : value;
}
