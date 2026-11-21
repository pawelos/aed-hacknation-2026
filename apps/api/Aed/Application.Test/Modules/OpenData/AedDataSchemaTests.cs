using Application.Modules.OpenData;
using Domain.Modules.AedDevice.Model;
using Domain.Modules.DataSource.Model;

namespace Application.Test.Modules.OpenData;

/// <summary>
/// The reference data set: field names, the fields required by the task and the dictionaries.
/// </summary>
public sealed class AedDataSchemaTests
{
    [Test]
    public void Field_names_are_unique_snake_case()
    {
        Assert.Multiple(() =>
        {
            Assert.That(AedDataSchema.Fields.Select(f => f.Name), Is.Unique);
            Assert.That(AedDataSchema.Fields.Select(f => f.Name), Has.All.Match("^[a-z][a-z_]*$"));
        });
    }

    [Test]
    public void Contains_every_field_proposed_in_the_task()
    {
        string[] task =
        [
            "id_rekordu", "nazwa_lokalizacji", "adres", "gmina", "powiat", "wojewodztwo",
            "szerokosc_geograficzna", "dlugosc_geograficzna", "wlasciciel_dysponent", "rodzaj_obiektu",
            "dostepnosc", "godziny_dostepnosci", "status_urzadzenia", "telefon_kontaktowy", "zrodlo_danych",
            "url_zrodla", "data_ostatniej_weryfikacji", "licencja_danych", "zakres_referencyjny"
        ];

        Assert.That(AedDataSchema.Fields.Select(f => f.Name), Is.SupersetOf(task));
    }

    [Test]
    public void Every_code_field_has_its_dictionary()
    {
        string[] dictionaries = AedDataSchema.Dictionaries.Select(d => d.Name).ToArray();

        Assert.That(
            AedDataSchema.Fields.Where(f => f.Type == AedFieldType.Code).Select(f => f.Dictionary),
            Is.SubsetOf(dictionaries));
    }

    [Test]
    public void Codes_are_unique_and_parse_back()
    {
        Assert.Multiple(() =>
        {
            foreach (AedDictionary dictionary in AedDataSchema.Dictionaries)
            {
                Assert.That(dictionary.Codes.Select(c => c.Code), Is.Unique, dictionary.Name);
            }

            foreach (FacilityType value in Enum.GetValues<FacilityType>())
            {
                Assert.That(AedCodes.TryParse(AedCodes.Facility(value).Code.ToUpperInvariant(), AedCodes.Facility, out FacilityType parsed), Is.True);
                Assert.That(parsed, Is.EqualTo(value));
            }

            Assert.That(AedCodes.TryParse("CC-BY-4.0", AedCodes.License, out DataLicense license), Is.True);
            Assert.That(license, Is.EqualTo(DataLicense.CcBy4));
            Assert.That(AedCodes.TryParse("nieznany", AedCodes.Status, out AedStatus _), Is.False);
        });
    }
}
