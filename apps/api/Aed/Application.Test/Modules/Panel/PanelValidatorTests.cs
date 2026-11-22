using Application.Modules.AedDevice.Panel;
using Application.Modules.CorrectionReport;
using Domain.Modules.AedDevice.Model;
using FluentValidation.Results;

namespace Application.Test.Modules.Panel;

/// <summary>
/// Validation of the panel form of an AED record and of the public correction report, with Polish messages.
/// </summary>
[SetUICulture("pl")]
public sealed class PanelValidatorTests
{
    private static readonly TimeProvider Time = TimeProvider.System;

    private static CreateAedDeviceCommand Valid() => new()
    {
        DataSourceId = Guid.NewGuid(),
        LocationName = "Teatr",
        Address = "ul. Zapolskiej 3",
        City = "Wrocław",
        Voivodeship = Voivodeship.LowerSilesian,
        Latitude = 51.1052m,
        Longitude = 17.0269m,
        AvailabilityHours = "Mo-Fr 08:00-16:00",
        ContactPhone = "71 316 07 77",
        PostalCode = "50-032"
    };

    [Test]
    public void Valid_record_passes()
    {
        Assert.That(new CreateAedDeviceCommandValidator(Time).Validate(Valid()).IsValid, Is.True);
    }

    [Test]
    public void Bad_values_are_reported_under_their_fields_in_polish()
    {
        CreateAedDeviceCommand command = Valid();
        command.LocationName = "";
        command.Voivodeship = null;
        command.AvailabilityHours = "pn-pt 8-16";
        command.PostalCode = "50032";
        command.MunicipalityTeryt = "12";
        command.ContactPhone = "brak";
        command.Latitude = 48.85m;
        command.Longitude = 2.35m;

        ValidationResult result = new CreateAedDeviceCommandValidator(Time).Validate(command);
        Dictionary<string, string> codes = result.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.First().ErrorCode);

        Assert.Multiple(() =>
        {
            Assert.That(codes.Keys, Is.SupersetOf(new[]
            {
                "LocationName", "Voivodeship", "AvailabilityHours", "PostalCode", "MunicipalityTeryt", "ContactPhone", "Latitude", "Longitude"
            }));
            Assert.That(codes["AvailabilityHours"], Is.EqualTo("AedDevice.UnknownHoursFormat"));
            Assert.That(result.Errors.First(e => e.PropertyName == "PostalCode").ErrorMessage, Is.EqualTo("Użyj formatu 00-000."));
        });
    }

    [Test]
    public void Correction_report_needs_ten_characters()
    {
        var validator = new SubmitCorrectionReportCommandValidator();

        Assert.Multiple(() =>
        {
            Assert.That(validator.Validate(new SubmitCorrectionReportCommand { AedDeviceId = Guid.NewGuid(), Message = "zamknięte" }).IsValid, Is.False);
            Assert.That(validator.Validate(new SubmitCorrectionReportCommand { AedDeviceId = Guid.NewGuid(), Message = "Szafka jest zamknięta na klucz." }).IsValid, Is.True);
        });
    }
}
