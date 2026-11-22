using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using AedDeviceEntity = Domain.Modules.AedDevice.Model.AedDevice;

namespace Application.Modules.AedDevice;

/// <summary>
/// AED records a panel user may see and change: an administrator all, an owner's employee those of the owner.
/// </summary>
internal static class AedDeviceAccess
{
    public static IQueryable<AedDeviceEntity> Visible(IApplicationDbContext dbContext, IUserContext userContext)
    {
        IQueryable<AedDeviceEntity> devices = dbContext.AedDevices.Where(d => !d.IsDeleted);
        if (!userContext.IsAdmin)
        {
            Guid? ownerId = userContext.OwnerId;
            devices = devices.Where(d => d.OwnerId == ownerId);
        }

        return devices;
    }
}
