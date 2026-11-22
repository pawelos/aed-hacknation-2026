using Application.Common.Localization;
using System.Globalization;
using System.Text.Json;

namespace Application.Modules.AedImport.Parsing;

/// <summary>
/// Reads a GeoJSON FeatureCollection (RFC 7946) of points: the coordinates come from the geometry
/// ([longitude, latitude]), the other values from the properties, mapped like CSV columns. OpenStreetMap exports
/// (e.g. overpass-turbo, emergency=defibrillator) work too: their tags opening_hours, indoor, access, phone and
/// defibrillator:location map to the reference fields.
/// </summary>
public static class GeoJsonReader
{
    public static ParsedFile Read(string text)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(text, new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });
        }
        catch (JsonException ex)
        {
            return ParsedFile.Failed(Lang.T($"The file is not valid JSON: {ex.Message}", $"Plik nie jest poprawnym JSON: {ex.Message}"));
        }

        using (document)
        {
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("features", out JsonElement features)
                || features.ValueKind != JsonValueKind.Array)
            {
                return ParsedFile.Failed(Lang.T("The file is not a GeoJSON FeatureCollection (no \"features\" array).", "Plik nie jest kolekcją GeoJSON FeatureCollection (brak tablicy \"features\")."));
            }

            var rows = new List<ImportedRow>();
            var unknown = new SortedSet<string>(StringComparer.Ordinal);
            int position = 0;
            foreach (JsonElement feature in features.EnumerateArray())
            {
                position++;
                var values = new Dictionary<string, string>(StringComparer.Ordinal);

                if (feature.TryGetProperty("properties", out JsonElement properties) && properties.ValueKind == JsonValueKind.Object)
                {
                    foreach (JsonProperty property in properties.EnumerateObject())
                    {
                        string? value = Text(property.Value);
                        string? field = FieldAliases.FieldFor(property.Name);
                        if (field is null)
                        {
                            unknown.Add(property.Name);
                        }
                        else if (!string.IsNullOrWhiteSpace(value) && !values.ContainsKey(field))
                        {
                            values[field] = value.Trim();
                        }
                    }
                }

                // The feature id (e.g. "node/123" in OSM exports) when the properties have no source id.
                if (!values.ContainsKey("id_zrodlowe") && feature.TryGetProperty("id", out JsonElement id) && Text(id) is { Length: > 0 } idText)
                {
                    values["id_zrodlowe"] = idText;
                }

                // The geometry is the position; it overrides coordinates in the properties.
                if (feature.TryGetProperty("geometry", out JsonElement geometry)
                    && geometry.ValueKind == JsonValueKind.Object
                    && geometry.TryGetProperty("type", out JsonElement type)
                    && type.GetString() == "Point"
                    && geometry.TryGetProperty("coordinates", out JsonElement coordinates)
                    && coordinates.ValueKind == JsonValueKind.Array
                    && coordinates.GetArrayLength() >= 2)
                {
                    values["dlugosc_geograficzna"] = coordinates[0].GetRawText();
                    values["szerokosc_geograficzna"] = coordinates[1].GetRawText();
                }

                rows.Add(new ImportedRow(position, values));
            }

            return new ParsedFile(rows, unknown.ToList(), null);
        }
    }

    private static string? Text(JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.String:
                return value.GetString();
            case JsonValueKind.Number:
                return value.GetRawText();
            case JsonValueKind.True:
                return "true";
            case JsonValueKind.False:
                return "false";
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;
            default:
                return value.GetRawText().ToString(CultureInfo.InvariantCulture);
        }
    }
}
