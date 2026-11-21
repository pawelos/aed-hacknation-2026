using Application.Modules.AedDevice;
using Domain.Modules.AedDevice.Model;
using Domain.Modules.DataSource.Model;
using Domain.Modules.Owner.Model;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Seeders;

/// <summary>
/// Fictional demo data of the prototype: three owners, three sources and about forty AED records in several
/// cities, with every availability and status. The community source repeats two Wrocław devices a few metres
/// away under other names, so duplicate detection has something to find. Runs only on an empty database.
/// The places and coordinates are examples, not a register of real devices.
/// </summary>
public static class DemoDataSeeder
{
    /// <summary>
    /// Owner of the demo account <see cref="UserSeeder.WroclawEmail"/>.
    /// </summary>
    public static readonly Guid WroclawOwnerId = new("a3d1c0de-0001-4000-8000-000000000001");

    /// <summary>
    /// Owner of the demo account <see cref="UserSeeder.FireBrigadeEmail"/>.
    /// </summary>
    public static readonly Guid FireBrigadeOwnerId = new("a3d1c0de-0001-4000-8000-000000000002");

    /// <summary>
    /// Owner of the community source (no panel account; managed by the administrator).
    /// </summary>
    public static readonly Guid FoundationOwnerId = new("a3d1c0de-0001-4000-8000-000000000003");

    private static readonly Guid WroclawSourceId = new("a3d1c0de-0002-4000-8000-000000000001");
    private static readonly Guid FireBrigadeSourceId = new("a3d1c0de-0002-4000-8000-000000000002");
    private static readonly Guid CommunitySourceId = new("a3d1c0de-0002-4000-8000-000000000003");

    private const string WroclawPhone = "+48717770000";
    private const string FireBrigadePhone = "+48713000000";
    private const string FoundationPhone = "+48220000000";

    /// <summary>
    /// Adds the demo data when there are no owners yet.
    /// </summary>
    /// <param name="dbContext">Database context.</param>
    /// <param name="utcNow">Current UTC time.</param>
    /// <param name="logger">Logger.</param>
    public static async Task SeedAsync(ApplicationDbContext dbContext, DateTime utcNow, ILogger logger)
    {
        if (await dbContext.Owners.AnyAsync())
        {
            return;
        }

        _seedTime = utcNow;

        dbContext.Owners.AddRange(
            new Owner
            {
                Id = WroclawOwnerId,
                Name = "Urząd Miejski Wrocławia (demo)",
                Type = OwnerType.LocalGovernment,
                Email = "aed@example.org",
                Phone = WroclawPhone,
                Website = "https://example.org/wroclaw",
                CreatedAt = utcNow
            },
            new Owner
            {
                Id = FireBrigadeOwnerId,
                Name = "OSP Gminy Przykładowej (demo)",
                Type = OwnerType.FireBrigade,
                Email = "osp@example.org",
                Phone = FireBrigadePhone,
                CreatedAt = utcNow
            },
            new Owner
            {
                Id = FoundationOwnerId,
                Name = "Fundacja Ratujmy Razem (demo)",
                Type = OwnerType.NonProfit,
                Email = "mapa@example.org",
                Phone = FoundationPhone,
                Website = "https://example.org/fundacja",
                CreatedAt = utcNow
            });

        dbContext.DataSources.AddRange(
            new DataSource
            {
                Id = WroclawSourceId,
                OwnerId = WroclawOwnerId,
                Name = "Rejestr AED w budynkach miejskich Wrocławia (dane przykładowe)",
                Url = "https://example.org/wroclaw/aed.csv",
                License = DataLicense.Cc0,
                ReferenceScope = "Gmina Wrocław – budynki i obiekty miejskie",
                LastVerifiedAt = utcNow.AddDays(-20),
                CreatedAt = utcNow
            },
            new DataSource
            {
                Id = FireBrigadeSourceId,
                OwnerId = FireBrigadeOwnerId,
                Name = "AED w remizach OSP (dane przykładowe)",
                Url = string.Empty,
                License = DataLicense.CcBy4,
                ReferenceScope = "Powiat wrocławski – remizy OSP",
                LastVerifiedAt = utcNow.AddDays(-200),
                CreatedAt = utcNow
            },
            new DataSource
            {
                Id = CommunitySourceId,
                OwnerId = FoundationOwnerId,
                Name = "Społeczna mapa AED (dane przykładowe)",
                Url = "https://example.org/fundacja/aed.geojson",
                License = DataLicense.CcBy4,
                ReferenceScope = "Polska – zgłoszenia mieszkańców",
                LastVerifiedAt = utcNow.AddDays(-400),
                CreatedAt = utcNow
            });

        List<AedDevice> devices = [.. WroclawDevices(), .. FireBrigadeDevices(), .. CommunityDevices()];
        foreach (AedDevice device in devices)
        {
            device.Id = Guid.NewGuid();
            device.CreatedAt = utcNow;
            device.Location = AedLocation.ToPoint(device.Latitude, device.Longitude);
            device.Latitude = AedLocation.Coordinate(device.Latitude);
            device.Longitude = AedLocation.Coordinate(device.Longitude);
        }

        dbContext.AedDevices.AddRange(devices);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Demo data seeded: 3 owners, 3 sources, {Count} AED records.", devices.Count);
    }

    /// <summary>
    /// Time of the current seed run; the seeder runs once, on start, before the API takes requests.
    /// </summary>
    private static DateTime _seedTime;

    /// <summary>
    /// Date of a verification <paramref name="days"/> days before the seed run.
    /// </summary>
    private static DateTime DaysAgo(int days) => _seedTime.AddDays(-days);

    private static AedDevice Wroclaw(
        string id,
        string name,
        string address,
        string postalCode,
        decimal latitude,
        decimal longitude,
        FacilityType facility,
        AedAvailability availability,
        string hours,
        string placement,
        string access = "",
        bool indoor = true,
        AedStatus status = AedStatus.Operational,
        int verifiedDaysAgo = 20) =>
        new()
        {
            DataSourceId = WroclawSourceId,
            OwnerId = WroclawOwnerId,
            SourceRecordId = id,
            LocationName = name,
            Address = address,
            PostalCode = postalCode,
            City = "Wrocław",
            Municipality = "Wrocław",
            MunicipalityTeryt = "0264011",
            County = "Wrocław",
            Voivodeship = Voivodeship.LowerSilesian,
            Latitude = latitude,
            Longitude = longitude,
            PlacementDetails = placement,
            IsIndoor = indoor,
            FacilityType = facility,
            Availability = availability,
            AvailabilityHours = hours,
            AccessNotes = access,
            Status = status,
            ContactPhone = WroclawPhone,
            LastVerifiedAt = DaysAgo(verifiedDaysAgo)
        };

    private static IEnumerable<AedDevice> WroclawDevices()
    {
        yield return Wroclaw("WRO-001", "Urząd Miejski – Nowy Ratusz", "Sukiennice 9", "50-107",
            51.109650m, 17.031900m, FacilityType.PublicOffice, AedAvailability.BusinessHours,
            "Mo-Fr 07:45-15:45", "Parter, przy punkcie informacyjnym");
        yield return Wroclaw("WRO-002", "Rynek – kapsuła AED przy pręgierzu", "Rynek", "50-106",
            51.110100m, 17.032000m, FacilityType.OutdoorCabinet, AedAvailability.Public24h,
            "24/7", "Szafka na słupie, ogrzewana", indoor: false);
        yield return Wroclaw("WRO-003", "Biuro Obsługi Mieszkańców", "ul. Zapolskiej 4", "50-032",
            51.104300m, 17.025700m, FacilityType.PublicOffice, AedAvailability.BusinessHours,
            "Mo-Fr 08:00-17:00", "Hol główny, obok biletomatu");
        yield return Wroclaw("WRO-004", "Szkoła Podstawowa nr 1 (przykład)", "ul. Szkolna 1", "50-001",
            51.115200m, 17.040100m, FacilityType.School, AedAvailability.Restricted,
            "Mo-Fr 07:00-18:00", "Pokój nauczycielski", "Klucz w portierni szkoły");
        yield return Wroclaw("WRO-005", "Hala sportowa (przykład)", "ul. Sportowa 10", "51-001",
            51.107400m, 17.077300m, FacilityType.Sport, AedAvailability.BusinessHours,
            "Mo-Su 07:00-22:00", "Przy wejściu na trybuny");
        yield return Wroclaw("WRO-006", "Basen miejski (przykład)", "ul. Wodna 2", "51-002",
            51.121900m, 17.012400m, FacilityType.Sport, AedAvailability.BusinessHours,
            "Mo-Su 06:00-22:00", "Recepcja, za ladą", "Poproś ratownika lub recepcję");
        yield return Wroclaw("WRO-007", "Dworzec – przejście podziemne", "ul. Dworcowa 1", "50-002",
            51.098000m, 17.036800m, FacilityType.Transport, AedAvailability.Public24h,
            "24/7", "Ściana przy schodach na peron 1");
        yield return Wroclaw("WRO-008", "Biblioteka miejska – filia (przykład)", "ul. Książkowa 3", "53-001",
            51.095100m, 17.010600m, FacilityType.CultureAndTourism, AedAvailability.BusinessHours,
            "Mo-Fr 10:00-19:00; Sa 10:00-15:00", "Wypożyczalnia, przy ladzie");
        yield return Wroclaw("WRO-009", "Straż Miejska – komisariat (przykład)", "ul. Strażnicza 5", "50-003",
            51.112800m, 17.019500m, FacilityType.Police, AedAvailability.Public24h,
            "24/7", "Dyżurka, wydawany na prośbę", "Zadzwoń domofonem „Dyżurny”");
        yield return Wroclaw("WRO-010", "Park – pawilon zarządcy (przykład)", "ul. Parkowa 1", "51-003",
            51.115900m, 17.083100m, FacilityType.OutdoorCabinet, AedAvailability.Public24h,
            "24/7", "Szafka na ścianie pawilonu od strony alejki", indoor: false);
        yield return Wroclaw("WRO-011", "Urząd Stanu Cywilnego (przykład)", "ul. Ślubna 7", "50-004",
            51.107000m, 17.035100m, FacilityType.PublicOffice, AedAvailability.BusinessHours,
            "Mo-Fr 08:00-15:00", "I piętro, korytarz przy sali ślubów",
            status: AedStatus.Outdated, verifiedDaysAgo: 500);
        yield return Wroclaw("WRO-012", "Przedszkole nr 5 (przykład)", "ul. Wesoła 12", "53-002",
            51.091200m, 16.998700m, FacilityType.School, AedAvailability.Restricted,
            "Mo-Fr 06:30-17:00", "Gabinet dyrektora", "Tylko dla personelu – zapukaj do sekretariatu");
    }

    private static AedDevice FireStation(
        string id,
        string village,
        string municipality,
        decimal latitude,
        decimal longitude,
        AedAvailability availability,
        string hours,
        string access,
        int verifiedDaysAgo,
        AedStatus status = AedStatus.Operational,
        bool indoor = false) =>
        new()
        {
            DataSourceId = FireBrigadeSourceId,
            OwnerId = FireBrigadeOwnerId,
            SourceRecordId = id,
            LocationName = $"Remiza OSP {village}",
            Address = $"{village}, ul. Strażacka 1",
            PostalCode = string.Empty,
            City = village,
            Municipality = municipality,
            County = "wrocławski",
            Voivodeship = Voivodeship.LowerSilesian,
            Latitude = latitude,
            Longitude = longitude,
            PlacementDetails = indoor ? "Garaż, przy wozie bojowym" : "Szafka na elewacji remizy",
            IsIndoor = indoor,
            FacilityType = FacilityType.FireStation,
            Availability = availability,
            AvailabilityHours = hours,
            AccessNotes = access,
            Status = status,
            ContactPhone = FireBrigadePhone,
            LastVerifiedAt = DaysAgo(verifiedDaysAgo)
        };

    private static IEnumerable<AedDevice> FireBrigadeDevices()
    {
        yield return FireStation("OSP-01", "Kobierzyce", "Kobierzyce", 51.002500m, 16.928600m,
            AedAvailability.Public24h, "24/7", string.Empty, 200);
        yield return FireStation("OSP-02", "Siechnice", "Siechnice", 51.034800m, 17.149800m,
            AedAvailability.Public24h, "24/7", string.Empty, 200);
        yield return FireStation("OSP-03", "Długołęka", "Długołęka", 51.181600m, 17.193800m,
            AedAvailability.Restricted, "Mo-Su 18:00-21:00", "Druhowie na zbiórkach; poza nimi telefon do naczelnika",
            200, indoor: true);
        yield return FireStation("OSP-04", "Sobótka", "Sobótka", 50.897900m, 16.744400m,
            AedAvailability.Public24h, "24/7", string.Empty, 200);
        yield return FireStation("OSP-05", "Czernica", "Czernica", 51.047600m, 17.244300m,
            AedAvailability.Public24h, "24/7", string.Empty, 420);

        // Imported with a warning: the owner has to confirm it.
        AedDevice toVerify = FireStation("OSP-06", "Kąty Wrocławskie", "Kąty Wrocławskie", 51.031100m, 16.766200m,
            AedAvailability.Public24h, "24/7", string.Empty, 200, AedStatus.ToVerify);
        toVerify.NeedsVerification = true;
        toVerify.VerificationReason = "Mieszkaniec zgłosił, że szafka jest pusta.";
        yield return toVerify;
    }

    private static AedDevice Community(
        string id,
        string name,
        string address,
        string postalCode,
        string city,
        string teryt,
        Voivodeship voivodeship,
        decimal latitude,
        decimal longitude,
        FacilityType facility,
        AedAvailability availability,
        string hours,
        string placement,
        string access = "",
        bool indoor = true) =>
        new()
        {
            DataSourceId = CommunitySourceId,
            OwnerId = FoundationOwnerId,
            SourceRecordId = id,
            LocationName = name,
            Address = address,
            PostalCode = postalCode,
            City = city,
            Municipality = city,
            MunicipalityTeryt = teryt,
            County = city,
            Voivodeship = voivodeship,
            Latitude = latitude,
            Longitude = longitude,
            PlacementDetails = placement,
            IsIndoor = indoor,
            FacilityType = facility,
            Availability = availability,
            AvailabilityHours = hours,
            AccessNotes = access,
            Status = AedStatus.Operational,
            ContactPhone = FoundationPhone,
            LastVerifiedAt = DaysAgo(400)
        };

    private static IEnumerable<AedDevice> CommunityDevices()
    {
        const string warsaw = "Warszawa";
        const string krakow = "Kraków";
        const string gdansk = "Gdańsk";
        const string poznan = "Poznań";

        // Same devices as WRO-001 and WRO-003, a few metres away, under other names: possible duplicates.
        yield return Community("SM-0001", "Ratusz Wrocław – AED w holu", "Sukiennice 9", "50-107", "Wrocław",
            "0264011", Voivodeship.LowerSilesian, 51.109690m, 17.031950m, FacilityType.PublicOffice,
            AedAvailability.BusinessHours, "Mo-Fr 08:00-15:30", "Hol na parterze");
        yield return Community("SM-0002", "BOM Zapolskiej", "Zapolskiej 4", "50-032", "Wrocław",
            "0264011", Voivodeship.LowerSilesian, 51.104330m, 17.025660m, FacilityType.PublicOffice,
            AedAvailability.BusinessHours, "Mo-Fr 08:00-17:00", "Przy wejściu");

        yield return Community("SM-0101", "Stacja metra – antresola (przykład)", "ul. Metra 1", "00-001", warsaw,
            "1465011", Voivodeship.Masovian, 52.229700m, 21.011800m, FacilityType.Transport,
            AedAvailability.Public24h, "Mo-Su 05:00-24:00", "Antresola, przy bramkach");
        yield return Community("SM-0102", "Centrum handlowe (przykład)", "ul. Handlowa 2", "00-002", warsaw,
            "1465011", Voivodeship.Masovian, 52.231900m, 21.002400m, FacilityType.Shop,
            AedAvailability.BusinessHours, "Mo-Sa 09:00-21:00; Su 10:00-20:00", "Poziom 0, punkt informacji");
        yield return Community("SM-0103", "Urząd dzielnicy (przykład)", "ul. Urzędowa 3", "00-003", warsaw,
            "1465011", Voivodeship.Masovian, 52.247800m, 21.015200m, FacilityType.PublicOffice,
            AedAvailability.BusinessHours, "Mo-Fr 08:00-16:00", "Sala obsługi klienta");
        yield return Community("SM-0104", "Biurowiec (przykład)", "al. Biurowa 4", "00-004", warsaw,
            "1465011", Voivodeship.Masovian, 52.225600m, 20.998700m, FacilityType.Office,
            AedAvailability.Restricted, "Mo-Fr 07:00-20:00", "Recepcja w lobby", "Ochrona budynku wydaje AED");
        yield return Community("SM-0105", "Park – szafka przy placu zabaw (przykład)", "ul. Parkowa 5", "00-005",
            warsaw, "1465011", Voivodeship.Masovian, 52.215300m, 21.028900m, FacilityType.OutdoorCabinet,
            AedAvailability.Public24h, "24/7", "Szafka przy wejściu do parku", indoor: false);
        yield return Community("SM-0106", "Kościół parafialny (przykład)", "ul. Kościelna 6", "00-006", warsaw,
            "1465011", Voivodeship.Masovian, 52.254100m, 20.983300m, FacilityType.Religious,
            AedAvailability.BusinessHours, "Mo-Su 06:30-19:00", "Kruchta, po prawej stronie");

        yield return Community("SM-0201", "Dworzec autobusowy (przykład)", "ul. Podróżnych 1", "31-001", krakow,
            "1261011", Voivodeship.LesserPoland, 50.067500m, 19.947500m, FacilityType.Transport,
            AedAvailability.Public24h, "24/7", "Hala odjazdów, przy kasach");
        yield return Community("SM-0202", "Uczelnia – budynek główny (przykład)", "ul. Akademicka 2", "31-002",
            krakow, "1261011", Voivodeship.LesserPoland, 50.061200m, 19.923800m, FacilityType.School,
            AedAvailability.BusinessHours, "Mo-Fr 07:00-21:00", "Portiernia");
        yield return Community("SM-0203", "Rynek – punkt informacji (przykład)", "Rynek 3", "31-003", krakow,
            "1261011", Voivodeship.LesserPoland, 50.061700m, 19.937300m, FacilityType.CultureAndTourism,
            AedAvailability.BusinessHours, "Mo-Su 09:00-19:00", "Za ladą informacji turystycznej");
        yield return Community("SM-0204", "Apteka całodobowa (przykład)", "ul. Zdrowa 4", "31-004", krakow,
            "1261011", Voivodeship.LesserPoland, 50.056300m, 19.944900m, FacilityType.Healthcare,
            AedAvailability.Public24h, "24/7", "Przy okienku nocnym");

        yield return Community("SM-0301", "Plaża – wieża ratowników (przykład)", "ul. Nadmorska 1", "80-001",
            gdansk, "2261011", Voivodeship.Pomeranian, 54.408700m, 18.650100m, FacilityType.OutdoorCabinet,
            AedAvailability.Restricted, "Jun-Aug 09:00-19:00", "Wieża ratowników WOPR", "Tylko w sezonie, u ratownika",
            indoor: false);
        yield return Community("SM-0302", "Stadion (przykład)", "ul. Kibiców 2", "80-002", gdansk,
            "2261011", Voivodeship.Pomeranian, 54.389900m, 18.640200m, FacilityType.Sport,
            AedAvailability.BusinessHours, "Mo-Fr 09:00-17:00", "Brama główna, punkt medyczny");
        yield return Community("SM-0303", "Galeria handlowa (przykład)", "ul. Zakupowa 3", "80-003", gdansk,
            "2261011", Voivodeship.Pomeranian, 54.382100m, 18.601300m, FacilityType.Shop,
            AedAvailability.BusinessHours, "Mo-Su 09:00-21:00", "Parter, przy toaletach");
        yield return Community("SM-0304", "Hotel (przykład)", "ul. Gościnna 4", "80-004", gdansk,
            "2261011", Voivodeship.Pomeranian, 54.351600m, 18.655900m, FacilityType.CultureAndTourism,
            AedAvailability.Public24h, "24/7", "Recepcja");

        yield return Community("SM-0401", "Dworzec kolejowy (przykład)", "ul. Kolejowa 1", "60-001", poznan,
            "3064011", Voivodeship.GreaterPoland, 52.401900m, 16.911800m, FacilityType.Transport,
            AedAvailability.Public24h, "24/7", "Hala główna, przy informacji");
        yield return Community("SM-0402", "Urząd miasta (przykład)", "pl. Miejski 2", "60-002", poznan,
            "3064011", Voivodeship.GreaterPoland, 52.408300m, 16.934200m, FacilityType.PublicOffice,
            AedAvailability.BusinessHours, "Mo-Fr 07:30-15:30", "Kancelaria, parter");
        yield return Community("SM-0403", "Pływalnia (przykład)", "ul. Pływacka 3", "60-003", poznan,
            "3064011", Voivodeship.GreaterPoland, 52.396400m, 16.952700m, FacilityType.Sport,
            AedAvailability.BusinessHours, "Mo-Su 06:00-22:00", "Kasy, za szybą");
        yield return Community("SM-0404", "Zakład pracy – portiernia (przykład)", "ul. Fabryczna 4", "60-004",
            poznan, "3064011", Voivodeship.GreaterPoland, 52.426100m, 16.887500m, FacilityType.Office,
            AedAvailability.Restricted, "24/7", "Portiernia zakładu", "Portier wydaje AED każdemu, kto prosi");
    }
}
