using Application.Abstractions.Data;
using Domain.Modules.AedDevice.Model;
using Microsoft.EntityFrameworkCore;

namespace Application.Modules.OpenData;

/// <summary>
/// Reads the records of the open data set: every record that is not deleted (withdrawn devices stay, with
/// their status, so users of the data see that a device is gone), with owner and source, ordered by id.
/// </summary>
internal static class ExportRows
{
    public static Task<List<AedExportRow>> LoadAsync(
        IApplicationDbContext dbContext,
        Voivodeship? voivodeship,
        Guid? dataSourceId,
        CancellationToken cancellationToken)
    {
        var devices = dbContext.AedDevices.AsNoTracking().Where(d => !d.IsDeleted);
        if (voivodeship is not null)
        {
            devices = devices.Where(d => d.Voivodeship == voivodeship);
        }

        if (dataSourceId is not null)
        {
            devices = devices.Where(d => d.DataSourceId == dataSourceId);
        }

        return (
                from d in devices
                join s in dbContext.DataSources on d.DataSourceId equals s.Id
                join o in dbContext.Owners on d.OwnerId equals o.Id
                orderby d.Id
                select new AedExportRow
                {
                    Id = d.Id,
                    SourceRecordId = d.SourceRecordId,
                    LocationName = d.LocationName,
                    Address = d.Address,
                    PostalCode = d.PostalCode,
                    City = d.City,
                    Municipality = d.Municipality,
                    MunicipalityTeryt = d.MunicipalityTeryt,
                    County = d.County,
                    Voivodeship = d.Voivodeship,
                    Latitude = d.Latitude,
                    Longitude = d.Longitude,
                    PlacementDetails = d.PlacementDetails,
                    IsIndoor = d.IsIndoor,
                    FacilityType = d.FacilityType,
                    Availability = d.Availability,
                    AvailabilityHours = d.AvailabilityHours,
                    AccessNotes = d.AccessNotes,
                    Status = d.Status,
                    ContactPhone = d.ContactPhone,
                    OwnerName = o.Name,
                    DataSourceId = s.Id,
                    SourceName = s.Name,
                    SourceUrl = s.Url,
                    License = s.License,
                    ReferenceScope = s.ReferenceScope,
                    LastVerifiedAt = d.LastVerifiedAt,
                    NeedsVerification = d.NeedsVerification,
                    UpdatedAt = d.UpdatedAt ?? d.CreatedAt
                })
            .ToListAsync(cancellationToken);
    }
}
