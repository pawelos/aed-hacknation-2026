using AedDeviceEntity = Domain.Modules.AedDevice.Model.AedDevice;
using Domain.Modules.AedDevice.Model;

namespace Application.Modules.PublicAed;

/// <summary>
/// Shared parts of the public queries: which records are published, filters and the map point projection.
/// </summary>
internal static class PublicAedQueries
{
    /// <summary>
    /// Records shown to the public: not deleted and not withdrawn.
    /// </summary>
    /// <param name="devices">All records.</param>
    /// <returns>Published records.</returns>
    public static IQueryable<AedDeviceEntity> Published(IQueryable<AedDeviceEntity> devices) =>
        devices.Where(d => !d.IsDeleted && d.Status != AedStatus.Withdrawn);

    /// <summary>
    /// Applies the optional availability, status and facility filters.
    /// </summary>
    public static IQueryable<AedDeviceEntity> Filter(
        IQueryable<AedDeviceEntity> devices,
        AedAvailability[]? availability,
        AedStatus[]? status,
        FacilityType[]? facilityType)
    {
        if (availability is { Length: > 0 })
        {
            devices = devices.Where(d => availability.Contains(d.Availability));
        }

        if (status is { Length: > 0 })
        {
            devices = devices.Where(d => status.Contains(d.Status));
        }

        if (facilityType is { Length: > 0 })
        {
            devices = devices.Where(d => facilityType.Contains(d.FacilityType));
        }

        return devices;
    }

    /// <summary>
    /// Columns read for a map point (the opening state is computed after the query).
    /// </summary>
    public sealed record Row(
        Guid Id,
        decimal Latitude,
        decimal Longitude,
        string LocationName,
        string City,
        string Address,
        AedAvailability Availability,
        AedStatus Status,
        string AvailabilityHours);

    /// <summary>
    /// Map point of a row at the given local time.
    /// </summary>
    public static AedMapPoint ToPoint(Row row, DateTime localTime) =>
        new(
            row.Id,
            row.Latitude,
            row.Longitude,
            row.LocationName,
            row.City,
            row.Address,
            row.Availability,
            row.Status,
            OpeningHours.IsAvailable(row.Availability, row.AvailabilityHours, localTime));
}
