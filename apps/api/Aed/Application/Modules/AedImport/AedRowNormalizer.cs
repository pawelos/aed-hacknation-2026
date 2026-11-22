using Application.Common.Localization;
using System.Globalization;
using System.Text.RegularExpressions;
using Application.Modules.AedDevice;
using Application.Modules.AedImport.Parsing;
using Application.Modules.OpenData;
using Application.Modules.PublicAed;
using Domain.Modules.AedDevice.Constants;
using Domain.Modules.AedDevice.Model;

namespace Application.Modules.AedImport;

/// <summary>
/// Validates and cleans one imported row. Errors (the row is rejected): missing name, town, coordinates or
/// voivodeship, coordinates outside Poland, a value longer than the field allows. Warnings (the row is imported
/// and marked for verification): swapped coordinates (fixed), a phone, postal code, TERYT code or date that
/// could not be read (left empty), unknown codes (default used), opening hours in an unknown format, a
/// verification older than 12 months, a missing address.
/// </summary>
public static partial class AedRowNormalizer
{
    private static readonly string[] DateFormats = ["yyyy-MM-dd", "dd.MM.yyyy", "d.M.yyyy", "yyyy-MM-dd'T'HH:mm:ss'Z'", "yyyy-MM-dd'T'HH:mm:ss", "yyyy/MM/dd"];

    public static NormalizedRow Normalize(ImportedRow row, DateTime utcNow)
    {
        var errors = new List<ImportIssue>();
        var warnings = new List<ImportIssue>();

        string sourceId = Text(row, "id_zrodlowe", AedDeviceConstant.SourceRecordIdMaxLength, errors);
        string name = Text(row, "nazwa_lokalizacji", AedDeviceConstant.LocationNameMaxLength, errors);
        string address = Text(row, "adres", AedDeviceConstant.AddressMaxLength, errors);
        string city = Text(row, "miejscowosc", AedDeviceConstant.CityMaxLength, errors);
        string municipality = Text(row, "gmina", AedDeviceConstant.MunicipalityMaxLength, errors);
        string county = Text(row, "powiat", AedDeviceConstant.CountyMaxLength, errors);
        string placement = Text(row, "lokalizacja_szczegolowa", AedDeviceConstant.PlacementDetailsMaxLength, errors);
        string accessNotes = Text(row, "uwagi_dostepu", AedDeviceConstant.AccessNotesMaxLength, errors);
        string hours = Text(row, "godziny_dostepnosci", AedDeviceConstant.AvailabilityHoursMaxLength, errors);

        if (name.Length == 0)
        {
            errors.Add(new ImportIssue("nazwa_lokalizacji", "Import.Required", Lang.T("The location name is missing.", "Brak nazwy lokalizacji.")));
        }

        if (city.Length == 0)
        {
            city = municipality;
        }

        if (city.Length == 0)
        {
            errors.Add(new ImportIssue("miejscowosc", "Import.Required", Lang.T("The town is missing.", "Brak miejscowości.")));
        }

        if (address.Length == 0)
        {
            warnings.Add(new ImportIssue("adres", "Import.AddressMissing", Lang.T("The address is missing.", "Brak adresu.")));
        }

        string teryt = Digits(row.Get("teryt_gminy"));
        if (teryt.Length > 0 && teryt.Length != AedDeviceConstant.MunicipalityTerytLength)
        {
            warnings.Add(new ImportIssue("teryt_gminy", "Import.InvalidTeryt", Lang.T($"The TERYT code '{row.Get("teryt_gminy")}' does not have 7 digits; left empty.", $"Kod TERYT '{row.Get("teryt_gminy")}' nie ma 7 cyfr; pole pozostawiono puste.")));
            teryt = string.Empty;
        }

        Voivodeship? voivodeship = ReadVoivodeship(row, teryt);
        if (voivodeship is null)
        {
            errors.Add(new ImportIssue("wojewodztwo", "Import.VoivodeshipMissing",
                row.Get("wojewodztwo") is { } given
                    ? Lang.T($"Unknown voivodeship '{given}'.", $"Nieznane województwo '{given}'.")
                    : Lang.T("The voivodeship is missing (give wojewodztwo or teryt_gminy).", "Brak województwa (podaj wojewodztwo albo teryt_gminy).")));
        }

        (decimal latitude, decimal longitude) = ReadCoordinates(row, errors, warnings);

        string postalCode = ReadPostalCode(row, warnings);
        string phone = ReadPhone(row, warnings);
        FacilityType facility = ReadCode(row, "rodzaj_obiektu", AedCodes.Facility, FacilityType.Other, warnings);
        AedStatus status = ReadCode(row, "status_urzadzenia", AedCodes.Status, AedStatus.Operational, warnings);
        AedAvailability availability = ReadAvailability(row, hours, warnings);
        bool indoor = ReadIndoor(row, warnings);

        if (availability == AedAvailability.Public24h && hours.Length == 0)
        {
            hours = "24/7";
        }

        if (hours.Length > 0 && OpeningHours.IsOpen(hours, OpeningHours.ToPolandTime(utcNow)) is null)
        {
            warnings.Add(new ImportIssue("godziny_dostepnosci", "Import.UnknownHoursFormat",
                Lang.T($"The opening hours '{hours}' are not in the opening_hours format (e.g. Mo-Fr 08:00-16:00); \"available now\" cannot be computed.", $"Godziny '{hours}' nie są w formacie opening_hours (np. Mo-Fr 08:00-16:00); nie da się ustalić, czy AED jest dostępne teraz.")));
        }

        DateTime? verifiedAt = ReadVerificationDate(row, utcNow, warnings);

        if (errors.Count > 0)
        {
            return new NormalizedRow(null, errors, warnings);
        }

        return new NormalizedRow(
            new NormalizedAed
            {
                SourceRecordId = sourceId,
                LocationName = name,
                Address = address,
                PostalCode = postalCode,
                City = city,
                Municipality = municipality.Length > 0 ? municipality : city,
                MunicipalityTeryt = teryt,
                County = county,
                Voivodeship = voivodeship!.Value,
                Latitude = latitude,
                Longitude = longitude,
                PlacementDetails = placement,
                IsIndoor = indoor,
                FacilityType = facility,
                Availability = availability,
                AvailabilityHours = hours,
                AccessNotes = accessNotes,
                Status = status,
                ContactPhone = phone,
                LastVerifiedAt = verifiedAt
            },
            errors,
            warnings);
    }

    private static string Text(ImportedRow row, string field, int maxLength, List<ImportIssue> errors)
    {
        string value = row.Get(field)?.Trim() ?? string.Empty;
        if (value.Length > maxLength)
        {
            errors.Add(new ImportIssue(field, "Import.TooLong", Lang.T($"The value has {value.Length} characters; at most {maxLength} are allowed.", $"Wartość ma {value.Length} znaków; dozwolone najwyżej {maxLength}.")));
            return string.Empty;
        }

        return value;
    }

    private static Voivodeship? ReadVoivodeship(ImportedRow row, string teryt)
    {
        if (VoivodeshipNames.TryParse(row.Get("wojewodztwo"), out Voivodeship byName))
        {
            return byName;
        }

        // The first two digits of a municipality TERYT code are the voivodeship.
        if (teryt.Length >= 2 && VoivodeshipNames.TryParse(teryt[..2], out Voivodeship byTeryt))
        {
            return byTeryt;
        }

        return VoivodeshipNames.TryParse(row.Get("teryt_wojewodztwa"), out Voivodeship byCode) ? byCode : null;
    }

    private static (decimal Latitude, decimal Longitude) ReadCoordinates(ImportedRow row, List<ImportIssue> errors, List<ImportIssue> warnings)
    {
        decimal? latitude = Number(row.Get("szerokosc_geograficzna"));
        decimal? longitude = Number(row.Get("dlugosc_geograficzna"));
        if (latitude is null || longitude is null)
        {
            if (latitude is null)
            {
                errors.Add(new ImportIssue("szerokosc_geograficzna", "Import.CoordinateMissing",
                    Lang.T($"The latitude '{row.Get("szerokosc_geograficzna")}' is missing or not a number.", $"Brak szerokości geograficznej albo '{row.Get("szerokosc_geograficzna")}' nie jest liczbą.")));
            }

            if (longitude is null)
            {
                errors.Add(new ImportIssue("dlugosc_geograficzna", "Import.CoordinateMissing",
                    Lang.T($"The longitude '{row.Get("dlugosc_geograficzna")}' is missing or not a number.", $"Brak długości geograficznej albo '{row.Get("dlugosc_geograficzna")}' nie jest liczbą.")));
            }

            return (0, 0);
        }

        decimal lat = latitude.Value;
        decimal lng = longitude.Value;
        if (!AedLocation.IsInPoland(lat, lng) && AedLocation.IsInPoland(lng, lat))
        {
            warnings.Add(new ImportIssue("szerokosc_geograficzna", "Import.CoordinatesSwapped",
                Lang.T($"The latitude and longitude were swapped ({lat}, {lng}); they were exchanged.", $"Szerokość i długość geograficzna były zamienione ({lat}, {lng}); poprawiono.")));
            (lat, lng) = (lng, lat);
        }

        if (!AedLocation.IsInPoland(lat, lng))
        {
            errors.Add(new ImportIssue("szerokosc_geograficzna", "Import.OutsidePoland",
                Lang.T($"The point ({lat}, {lng}) is outside Poland.", $"Punkt ({lat}, {lng}) leży poza Polską.")));
            return (0, 0);
        }

        return (AedLocation.Coordinate(lat), AedLocation.Coordinate(lng));
    }

    private static string ReadPostalCode(ImportedRow row, List<ImportIssue> warnings)
    {
        string? value = row.Get("kod_pocztowy");
        if (value is null)
        {
            return string.Empty;
        }

        string digits = Digits(value);
        if (digits.Length == 5)
        {
            return $"{digits[..2]}-{digits[2..]}";
        }

        warnings.Add(new ImportIssue("kod_pocztowy", "Import.InvalidPostalCode", Lang.T($"The postal code '{value}' is not 00-000; left empty.", $"Kod pocztowy '{value}' nie ma formatu 00-000; pole pozostawiono puste.")));
        return string.Empty;
    }

    /// <summary>
    /// Phone in E.164: 9 Polish digits get +48; spaces, dashes and brackets are removed.
    /// </summary>
    public static string NormalizePhone(string value)
    {
        string trimmed = value.Trim();
        bool plus = trimmed.StartsWith('+') || trimmed.StartsWith("00", StringComparison.Ordinal);
        string digits = Digits(trimmed);
        if (trimmed.StartsWith("00", StringComparison.Ordinal))
        {
            digits = digits[2..];
        }

        if (!plus && digits.Length == 9)
        {
            digits = "48" + digits;
        }

        string phone = "+" + digits;
        return PhoneRegex().IsMatch(phone) && phone.Length <= AedDeviceConstant.ContactPhoneMaxLength ? phone : string.Empty;
    }

    private static string ReadPhone(ImportedRow row, List<ImportIssue> warnings)
    {
        string? value = row.Get("telefon_kontaktowy");
        if (value is null)
        {
            return string.Empty;
        }

        string phone = NormalizePhone(value);
        if (phone.Length == 0)
        {
            warnings.Add(new ImportIssue("telefon_kontaktowy", "Import.InvalidPhone", Lang.T($"The phone '{value}' is not a valid number; left empty.", $"Telefon '{value}' nie jest poprawnym numerem; pole pozostawiono puste.")));
        }

        return phone;
    }

    private static TEnum ReadCode<TEnum>(ImportedRow row, string field, Func<TEnum, AedCode> code, TEnum fallback, List<ImportIssue> warnings)
        where TEnum : struct, Enum
    {
        string? value = row.Get(field);
        if (value is null)
        {
            return fallback;
        }

        if (AedCodes.TryParse(value, code, out TEnum parsed) || Enum.TryParse(value, ignoreCase: true, out parsed))
        {
            return parsed;
        }

        warnings.Add(new ImportIssue(field, "Import.UnknownCode",
            Lang.T($"Unknown code '{value}'; '{code(fallback).Code}' was used. Allowed: {string.Join(", ", AedCodes.All(code).Select(c => c.Code))}.", $"Nieznany kod '{value}'; użyto '{code(fallback).Code}'. Dozwolone: {string.Join(", ", AedCodes.All(code).Select(c => c.Code))}.")));
        return fallback;
    }

    /// <summary>
    /// Availability: a code of the dictionary, else the OpenStreetMap "access" tag, else from the hours.
    /// </summary>
    private static AedAvailability ReadAvailability(ImportedRow row, string hours, List<ImportIssue> warnings)
    {
        string? value = row.Get("dostepnosc");
        AedAvailability fromHours = hours == "24/7" ? AedAvailability.Public24h : AedAvailability.BusinessHours;
        if (value is null)
        {
            return fromHours;
        }

        if (AedCodes.TryParse(value, AedCodes.Availability, out AedAvailability parsed) || Enum.TryParse(value, ignoreCase: true, out parsed))
        {
            return parsed;
        }

        switch (value.Trim().ToLowerInvariant())
        {
            case "yes":
            case "public":
            case "tak":
                return fromHours;
            case "customers":
            case "permissive":
                return AedAvailability.BusinessHours;
            case "private":
            case "no":
            case "nie":
                return AedAvailability.Restricted;
            default:
                warnings.Add(new ImportIssue("dostepnosc", "Import.UnknownCode",
                    Lang.T($"Unknown availability '{value}'; '{AedCodes.Availability(fromHours).Code}' was used.", $"Nieznana dostępność '{value}'; użyto '{AedCodes.Availability(fromHours).Code}'.")));
                return fromHours;
        }
    }

    private static bool ReadIndoor(ImportedRow row, List<ImportIssue> warnings)
    {
        string? value = row.Get("wewnatrz_budynku");
        if (value is null)
        {
            return true;
        }

        switch (value.Trim().ToLowerInvariant())
        {
            case "true":
            case "tak":
            case "yes":
            case "1":
                return true;
            case "false":
            case "nie":
            case "no":
            case "0":
                return false;
            default:
                warnings.Add(new ImportIssue("wewnatrz_budynku", "Import.InvalidBoolean", Lang.T($"'{value}' is not true/false; true was used.", $"'{value}' to nie true/false; użyto true.")));
                return true;
        }
    }

    private static DateTime? ReadVerificationDate(ImportedRow row, DateTime utcNow, List<ImportIssue> warnings)
    {
        string? value = row.Get("data_ostatniej_weryfikacji");
        if (value is null)
        {
            return null;
        }

        if (!DateTime.TryParseExact(value.Trim(), DateFormats, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime date))
        {
            warnings.Add(new ImportIssue("data_ostatniej_weryfikacji", "Import.InvalidDate", Lang.T($"The date '{value}' is not YYYY-MM-DD; left empty.", $"Data '{value}' nie ma formatu RRRR-MM-DD; pole pozostawiono puste.")));
            return null;
        }

        if (date > utcNow.AddDays(1))
        {
            warnings.Add(new ImportIssue("data_ostatniej_weryfikacji", "Import.FutureDate", Lang.T($"The date '{value}' is in the future; left empty.", $"Data '{value}' jest z przyszłości; pole pozostawiono puste.")));
            return null;
        }

        if (date < utcNow.AddMonths(-AedDeviceConstant.VerificationValidityMonths))
        {
            warnings.Add(new ImportIssue("data_ostatniej_weryfikacji", "Import.VerificationStale",
                Lang.T($"The record was last verified on {date:yyyy-MM-dd}, more than {AedDeviceConstant.VerificationValidityMonths} months ago.", $"Rekord zweryfikowano {date:yyyy-MM-dd}, ponad {AedDeviceConstant.VerificationValidityMonths} miesięcy temu.")));
        }

        return date;
    }

    private static decimal? Number(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        // Polish decimal comma ("52,2297") when there is no dot.
        string text = value.Trim();
        if (!text.Contains('.'))
        {
            text = text.Replace(',', '.');
        }

        return decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal number) ? number : null;
    }

    private static string Digits(string? value) =>
        value is null ? string.Empty : new string(value.Where(char.IsAsciiDigit).ToArray());

    [GeneratedRegex(@"^\+[1-9]\d{8,14}$")]
    private static partial Regex PhoneRegex();
}
