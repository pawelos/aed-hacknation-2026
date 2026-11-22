using Application.Common.Localization;
using SharedKernel;

namespace Application.Modules.AedDevice;

/// <summary>
/// Errors of the AED records in the panel.
/// </summary>
public static class AedDeviceErrors
{
    /// <summary>
    /// Error <c>AedDevice.NotFound</c>: no such record, or it belongs to another owner.
    /// </summary>
    public static Error NotFound(Guid id) =>
        Error.NotFound("AedDevice.NotFound", $"AED record '{id}' was not found.");

    /// <summary>
    /// Error <c>AedDevice.SourceRecordIdTaken</c> on <c>sourceRecordId</c>.
    /// </summary>
    public static Error SourceRecordIdTaken(string id) =>
        Error.Conflict("AedDevice.SourceRecordIdTaken", Lang.T($"Another record of this source already has the id '{id}'.", $"Inny rekord tego źródła ma już id '{id}'.")).OnField("sourceRecordId");

    /// <summary>
    /// Error <c>AedDevice.BatchNotFound</c>: some of the chosen records do not exist (any more) or belong to another
    /// owner; nothing was changed.
    /// </summary>
    public static Error BatchNotFound(int missing, int total) =>
        Error.NotFound("AedDevice.BatchNotFound", Lang.T(
            $"{missing} of the {total} chosen records were not found (removed or of another holder). Nothing was changed.",
            $"Nie znaleziono {missing} z {total} zaznaczonych rekordów (usunięte albo innego dysponenta). Nic nie zmieniono."));
}
