using Application.Common.Localization;
using System.Text.RegularExpressions;
using Application.Modules.PublicAed;
using Domain.Modules.AedDevice.Constants;
using Domain.Modules.AedDevice.Model;
using FluentValidation;

namespace Application.Modules.AedDevice;

/// <summary>
/// Editable fields of an AED record (the panel form), shared by create and update.
/// </summary>
public abstract class AedDeviceInput : IAedLocation
{
    public string SourceRecordId { get; set; } = string.Empty;

    public string LocationName { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Municipality { get; set; } = string.Empty;

    public string MunicipalityTeryt { get; set; } = string.Empty;

    public string County { get; set; } = string.Empty;

    public Voivodeship? Voivodeship { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string PlacementDetails { get; set; } = string.Empty;

    public bool IsIndoor { get; set; } = true;

    public FacilityType FacilityType { get; set; } = FacilityType.Other;

    public AedAvailability Availability { get; set; } = AedAvailability.BusinessHours;

    public string AvailabilityHours { get; set; } = string.Empty;

    public string AccessNotes { get; set; } = string.Empty;

    public AedStatus Status { get; set; } = AedStatus.Operational;

    public string ContactPhone { get; set; } = string.Empty;
}

/// <summary>
/// Validation rules of <see cref="AedDeviceInput"/>; the limits are the field limits of the reference data set
/// (the web form uses the same values as maxLength).
/// </summary>
internal static partial class AedDeviceInputRules
{
    public static void Add<T>(AbstractValidator<T> v, TimeProvider timeProvider)
        where T : AedDeviceInput
    {
        v.RuleFor(x => x.SourceRecordId).MaximumLength(AedDeviceConstant.SourceRecordIdMaxLength);
        v.RuleFor(x => x.LocationName).NotEmpty().MaximumLength(AedDeviceConstant.LocationNameMaxLength);
        v.RuleFor(x => x.Address).NotEmpty().MaximumLength(AedDeviceConstant.AddressMaxLength);
        v.RuleFor(x => x.PostalCode)
            .Matches(PostalCode()).WithErrorCode("AedDevice.InvalidPostalCode").WithMessage(_ => Lang.T("Use the format 00-000.", "Użyj formatu 00-000."))
            .When(x => !string.IsNullOrWhiteSpace(x.PostalCode));
        v.RuleFor(x => x.City).NotEmpty().MaximumLength(AedDeviceConstant.CityMaxLength);
        v.RuleFor(x => x.Municipality).MaximumLength(AedDeviceConstant.MunicipalityMaxLength);
        v.RuleFor(x => x.MunicipalityTeryt)
            .Matches(Teryt()).WithErrorCode("AedDevice.InvalidTeryt").WithMessage(_ => Lang.T("The TERYT code has 7 digits.", "Kod TERYT ma 7 cyfr."))
            .When(x => !string.IsNullOrWhiteSpace(x.MunicipalityTeryt));
        v.RuleFor(x => x.County).MaximumLength(AedDeviceConstant.CountyMaxLength);
        v.RuleFor(x => x.Voivodeship).NotNull().IsInEnum();
        v.RuleFor(x => x.PlacementDetails).MaximumLength(AedDeviceConstant.PlacementDetailsMaxLength);
        v.RuleFor(x => x.FacilityType).IsInEnum();
        v.RuleFor(x => x.Availability).IsInEnum();
        v.RuleFor(x => x.Status).IsInEnum();
        v.RuleFor(x => x.AvailabilityHours)
            .MaximumLength(AedDeviceConstant.AvailabilityHoursMaxLength)
            .Must(h => OpeningHours.IsOpen(h, OpeningHours.ToPolandTime(timeProvider.GetUtcNow())) is not null)
            .WithErrorCode("AedDevice.UnknownHoursFormat")
            .WithMessage(_ => Lang.T("Use the opening_hours format, e.g. Mo-Fr 08:00-16:00; Sa 10:00-14:00 or 24/7.", "Użyj formatu opening_hours, np. Mo-Fr 08:00-16:00; Sa 10:00-14:00 albo 24/7."))
            .When(x => !string.IsNullOrWhiteSpace(x.AvailabilityHours));
        v.RuleFor(x => x.AccessNotes).MaximumLength(AedDeviceConstant.AccessNotesMaxLength);
        v.RuleFor(x => x.ContactPhone)
            .MaximumLength(AedDeviceConstant.ContactPhoneMaxLength)
            .Must(p => AedImport.AedRowNormalizer.NormalizePhone(p).Length > 0)
            .WithErrorCode("AedDevice.InvalidPhone").WithMessage(_ => Lang.T("Give a phone number, e.g. +48 71 777 00 00.", "Podaj numer telefonu, np. +48 71 777 00 00."))
            .When(x => !string.IsNullOrWhiteSpace(x.ContactPhone));
        AedLocation.AddRules(v);
    }

    /// <summary>
    /// Writes the input to a record: trimmed text, normalized phone, rounded point.
    /// </summary>
    public static void Apply(AedDeviceInput input, Domain.Modules.AedDevice.Model.AedDevice target)
    {
        target.SourceRecordId = input.SourceRecordId.Trim();
        target.LocationName = input.LocationName.Trim();
        target.Address = input.Address.Trim();
        target.PostalCode = input.PostalCode.Trim();
        target.City = input.City.Trim();
        target.Municipality = string.IsNullOrWhiteSpace(input.Municipality) ? input.City.Trim() : input.Municipality.Trim();
        target.MunicipalityTeryt = input.MunicipalityTeryt.Trim();
        target.County = input.County.Trim();
        target.Voivodeship = input.Voivodeship!.Value;
        target.Latitude = AedLocation.Coordinate(input.Latitude!.Value);
        target.Longitude = AedLocation.Coordinate(input.Longitude!.Value);
        target.Location = AedLocation.ToPoint(input.Latitude.Value, input.Longitude.Value);
        target.PlacementDetails = input.PlacementDetails.Trim();
        target.IsIndoor = input.IsIndoor;
        target.FacilityType = input.FacilityType;
        target.Availability = input.Availability;
        target.AvailabilityHours = input.Availability == AedAvailability.Public24h && string.IsNullOrWhiteSpace(input.AvailabilityHours)
            ? "24/7"
            : input.AvailabilityHours.Trim();
        target.AccessNotes = input.AccessNotes.Trim();
        target.Status = input.Status;
        target.ContactPhone = string.IsNullOrWhiteSpace(input.ContactPhone) ? string.Empty : AedImport.AedRowNormalizer.NormalizePhone(input.ContactPhone);
    }

    [GeneratedRegex(@"^\d{2}-\d{3}$")]
    private static partial Regex PostalCode();

    [GeneratedRegex(@"^\d{7}$")]
    private static partial Regex Teryt();
}
