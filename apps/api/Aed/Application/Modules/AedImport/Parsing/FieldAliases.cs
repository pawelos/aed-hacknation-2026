using System.Globalization;
using System.Text;
using Application.Modules.OpenData;

namespace Application.Modules.AedImport.Parsing;

/// <summary>
/// Maps the column names of holders' files to the fields of the reference data set. Every field matches its
/// own name; common other names (Polish, English, OpenStreetMap tags) are listed here. Matching ignores case,
/// Polish letters, spaces, dashes and dots, so "Szerokość geograficzna", "szerokosc-geograficzna" and
/// "SZEROKOSC_GEOGRAFICZNA" are the same column.
/// </summary>
public static class FieldAliases
{
    private static readonly Dictionary<string, string> Aliases = Build();

    /// <summary>
    /// Field of the reference data set for a column name, or null when the column is unknown.
    /// </summary>
    public static string? FieldFor(string column)
    {
        string key = Normalize(column);
        return key.Length == 0 ? null : Aliases.GetValueOrDefault(key);
    }

    /// <summary>
    /// Lower case ASCII with underscores: "Szerokość geograficzna" → "szerokosc_geograficzna".
    /// </summary>
    public static string Normalize(string text)
    {
        string decomposed = text.Trim().Replace('ł', 'l').Replace('Ł', 'L').Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        bool underscore = false;
        foreach (char c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(c))
            {
                builder.Append(char.ToLowerInvariant(c));
                underscore = false;
            }
            else if (!underscore && builder.Length > 0)
            {
                builder.Append('_');
                underscore = true;
            }
        }

        return builder.ToString().TrimEnd('_');
    }

    private static Dictionary<string, string> Build()
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (AedField field in AedDataSchema.Fields)
        {
            map[Normalize(field.Name)] = field.Name;
        }

        void Add(string field, params string[] names)
        {
            foreach (string name in names)
            {
                map[Normalize(name)] = field;
            }
        }

        Add("id_zrodlowe", "id", "identyfikator", "nr", "numer", "source_id", "ref", "osm_id", "@id");
        Add("nazwa_lokalizacji", "nazwa", "name", "lokalizacja", "obiekt", "miejsce", "location_name");
        Add("adres", "address", "ulica", "ulica_i_numer", "addr_street_full", "addr_street", "adres_ulica");
        Add("kod_pocztowy", "kod", "postal_code", "postcode", "addr_postcode", "zip");
        Add("miejscowosc", "miasto", "city", "town", "addr_city", "miejscowość");
        Add("gmina", "municipality");
        Add("teryt_gminy", "teryt", "teryt_terc", "kod_teryt");
        Add("powiat", "county", "district");
        Add("wojewodztwo", "region", "voivodeship", "province", "state");
        Add("szerokosc_geograficzna", "szerokosc", "lat", "latitude", "y", "wsp_y");
        Add("dlugosc_geograficzna", "dlugosc", "lon", "lng", "long", "longitude", "x", "wsp_x");
        Add("lokalizacja_szczegolowa", "defibrillator_location", "gdzie", "polozenie", "opis_lokalizacji", "placement", "description");
        Add("wewnatrz_budynku", "indoor", "w_budynku", "wewnatrz");
        Add("rodzaj_obiektu", "rodzaj", "typ_obiektu", "facility_type", "kategoria");
        Add("dostepnosc", "access", "dostep", "availability");
        Add("godziny_dostepnosci", "opening_hours", "godziny", "godziny_otwarcia", "hours");
        Add("uwagi_dostepu", "uwagi", "access_notes", "note", "notes");
        Add("status_urzadzenia", "status", "stan");
        Add("telefon_kontaktowy", "telefon", "phone", "tel", "contact_phone", "emergency_phone");
        Add("data_ostatniej_weryfikacji", "data_weryfikacji", "weryfikacja", "check_date", "verified", "last_verified");
        return map;
    }
}
