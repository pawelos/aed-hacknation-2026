using Domain.Modules.AedDevice.Constants;
using Domain.Modules.AedDevice.Model;
using Domain.Modules.DataSource.Model;
using Domain.Modules.Owner.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

/// <summary>
/// Mapping of <see cref="AedDevice"/>: text enums, decimal(9,6) coordinates and a geography point with a GiST index.
/// </summary>
internal sealed class AedDeviceConfiguration : IEntityTypeConfiguration<AedDevice>
{
    public void Configure(EntityTypeBuilder<AedDevice> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SourceRecordId).HasMaxLength(AedDeviceConstant.SourceRecordIdMaxLength);
        builder.Property(x => x.LocationName).HasMaxLength(AedDeviceConstant.LocationNameMaxLength).IsRequired();
        builder.Property(x => x.Address).HasMaxLength(AedDeviceConstant.AddressMaxLength).IsRequired();
        builder.Property(x => x.PostalCode).HasMaxLength(AedDeviceConstant.PostalCodeMaxLength);
        builder.Property(x => x.City).HasMaxLength(AedDeviceConstant.CityMaxLength).IsRequired();
        builder.Property(x => x.Municipality).HasMaxLength(AedDeviceConstant.MunicipalityMaxLength);
        builder.Property(x => x.MunicipalityTeryt).HasMaxLength(AedDeviceConstant.MunicipalityTerytLength);
        builder.Property(x => x.County).HasMaxLength(AedDeviceConstant.CountyMaxLength);
        builder.Property(x => x.Voivodeship).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Latitude).HasPrecision(9, AedDeviceConstant.CoordinateDecimals);
        builder.Property(x => x.Longitude).HasPrecision(9, AedDeviceConstant.CoordinateDecimals);
        // geography: ST_DWithin / ST_Distance work in metres without projecting.
        builder.Property(x => x.Location).HasColumnType($"geography (point, {AedDeviceConstant.Srid})").IsRequired();
        builder.Property(x => x.PlacementDetails).HasMaxLength(AedDeviceConstant.PlacementDetailsMaxLength);
        builder.Property(x => x.FacilityType).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Availability).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.AvailabilityHours).HasMaxLength(AedDeviceConstant.AvailabilityHoursMaxLength);
        builder.Property(x => x.AccessNotes).HasMaxLength(AedDeviceConstant.AccessNotesMaxLength);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.ContactPhone).HasMaxLength(AedDeviceConstant.ContactPhoneMaxLength);
        builder.Property(x => x.VerificationReason).HasMaxLength(AedDeviceConstant.VerificationReasonMaxLength);

        builder.HasOne<DataSource>().WithMany().HasForeignKey(x => x.DataSourceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Owner>().WithMany().HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Location).HasMethod("gist");
        // The next import of the same source updates the record instead of adding a copy.
        builder.HasIndex(x => new { x.DataSourceId, x.SourceRecordId })
            .IsUnique()
            .HasFilter("source_record_id <> '' AND is_deleted = false");
        builder.HasIndex(x => x.Voivodeship);
        builder.HasIndex(x => x.City);
        builder.HasIndex(x => x.Status);
    }
}
