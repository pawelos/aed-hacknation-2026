using Domain.Modules.AedDevice.Model;
using Domain.Modules.DataSource.Model;

namespace Application.Modules.OpenData;

/// <summary>
/// Code with its Polish and English label, as listed in the dictionaries of the reference data set.
/// </summary>
/// <param name="Code">Value written in the data set.</param>
/// <param name="LabelPl">Polish label.</param>
/// <param name="LabelEn">English label.</param>
public sealed record AedCode(string Code, string LabelPl, string LabelEn);

/// <summary>
/// Codes of the dictionary fields of the reference data set (Polish, lower case, ASCII), the same in CSV,
/// GeoJSON, JSON, the XLSX template and the import. Stable: a published code is never renamed.
/// </summary>
public static class AedCodes
{
    /// <summary>
    /// Code of <c>dostepnosc</c>.
    /// </summary>
    public static AedCode Availability(AedAvailability value)
    {
        switch (value)
        {
            case AedAvailability.Public24h:
                return new AedCode("calodobowa", "Dostępny dla każdego całą dobę", "Open to anyone 24/7");
            case AedAvailability.BusinessHours:
                return new AedCode("w_godzinach_otwarcia", "Dostępny w godzinach otwarcia miejsca", "During the opening hours of the place");
            case AedAvailability.Restricted:
                return new AedCode("ograniczona", "Dostęp ograniczony (personel, klucz)", "Restricted (staff, key)");
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }

    /// <summary>
    /// Code of <c>status_urzadzenia</c>.
    /// </summary>
    public static AedCode Status(AedStatus value)
    {
        switch (value)
        {
            case AedStatus.Operational:
                return new AedCode("sprawny", "Sprawny i na miejscu", "Working and in place");
            case AedStatus.ToVerify:
                return new AedCode("do_weryfikacji", "Do weryfikacji przez dysponenta", "To be verified by the owner");
            case AedStatus.Outdated:
                return new AedCode("nieaktualny", "Dane nieaktualne", "Outdated data");
            case AedStatus.Withdrawn:
                return new AedCode("wycofany", "Urządzenie wycofane", "Device withdrawn");
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }

    /// <summary>
    /// Code of <c>rodzaj_obiektu</c>.
    /// </summary>
    public static AedCode Facility(FacilityType value)
    {
        switch (value)
        {
            case FacilityType.PublicOffice:
                return new AedCode("urzad", "Urząd", "Public office");
            case FacilityType.School:
                return new AedCode("szkola", "Szkoła, przedszkole, uczelnia", "School or university");
            case FacilityType.Healthcare:
                return new AedCode("ochrona_zdrowia", "Szpital, przychodnia, apteka", "Healthcare");
            case FacilityType.FireStation:
                return new AedCode("straz_pozarna", "Remiza OSP lub PSP", "Fire station");
            case FacilityType.Police:
                return new AedCode("policja", "Policja lub straż miejska", "Police or city guard");
            case FacilityType.Shop:
                return new AedCode("handel", "Sklep lub centrum handlowe", "Shop or shopping centre");
            case FacilityType.Office:
                return new AedCode("biuro", "Biurowiec lub zakład pracy", "Office or workplace");
            case FacilityType.Transport:
                return new AedCode("transport", "Dworzec, lotnisko, metro", "Station, airport, metro");
            case FacilityType.Sport:
                return new AedCode("sport", "Obiekt sportowy", "Sports facility");
            case FacilityType.Religious:
                return new AedCode("obiekt_sakralny", "Kościół lub inny obiekt sakralny", "Place of worship");
            case FacilityType.CultureAndTourism:
                return new AedCode("kultura_turystyka", "Kultura, hotel, turystyka", "Culture, hotel, tourism");
            case FacilityType.OutdoorCabinet:
                return new AedCode("szafka_zewnetrzna", "Szafka w przestrzeni publicznej", "Outdoor cabinet in a public space");
            case FacilityType.Other:
                return new AedCode("inne", "Inne", "Other");
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }

    /// <summary>
    /// SPDX identifier of <c>licencja_danych</c>.
    /// </summary>
    public static AedCode License(DataLicense value)
    {
        switch (value)
        {
            case DataLicense.Cc0:
                return new AedCode("CC0-1.0", "Creative Commons Zero 1.0 (domena publiczna)", "Creative Commons Zero 1.0 (public domain)");
            case DataLicense.CcBy4:
                return new AedCode("CC-BY-4.0", "Creative Commons Uznanie autorstwa 4.0", "Creative Commons Attribution 4.0");
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }

    /// <summary>
    /// Address of the licence text, as required by DCAT-AP-PL (<c>dcat:license</c>).
    /// </summary>
    public static string LicenseUrl(DataLicense value)
    {
        switch (value)
        {
            case DataLicense.Cc0:
                return "https://creativecommons.org/publicdomain/zero/1.0/";
            case DataLicense.CcBy4:
                return "https://creativecommons.org/licenses/by/4.0/";
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }

    /// <summary>
    /// All codes of a dictionary, in the order of the enum.
    /// </summary>
    public static IReadOnlyList<AedCode> All<TEnum>(Func<TEnum, AedCode> code)
        where TEnum : struct, Enum =>
        Enum.GetValues<TEnum>().Select(code).ToList();

    /// <summary>
    /// Finds the enum value of a code (any case); used by the import.
    /// </summary>
    public static bool TryParse<TEnum>(string? text, Func<TEnum, AedCode> code, out TEnum value)
        where TEnum : struct, Enum
    {
        value = default;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        string trimmed = text.Trim();
        foreach (TEnum candidate in Enum.GetValues<TEnum>())
        {
            if (string.Equals(code(candidate).Code, trimmed, StringComparison.OrdinalIgnoreCase))
            {
                value = candidate;
                return true;
            }
        }

        return false;
    }
}
