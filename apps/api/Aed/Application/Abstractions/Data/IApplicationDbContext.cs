using Domain.Modules.AedDevice.Model;
using Domain.Modules.CorrectionReport.Model;
using Domain.Modules.DataSource.Model;
using Domain.Modules.ImportBatch.Model;
using Domain.Modules.Owner.Model;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

/// <summary>
/// Database context of the AED application.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Owners (data holders) of AED devices: municipalities, schools, companies, fire brigades.
    /// </summary>
    DbSet<Owner> Owners { get; }

    /// <summary>
    /// Data sources an owner publishes AED records from.
    /// </summary>
    DbSet<DataSource> DataSources { get; }

    /// <summary>
    /// AED devices (one record of the reference data set each).
    /// </summary>
    DbSet<AedDevice> AedDevices { get; }

    /// <summary>
    /// Corrections reported by citizens for AED records.
    /// </summary>
    DbSet<CorrectionReport> CorrectionReports { get; }

    /// <summary>
    /// History of imports with their reports.
    /// </summary>
    DbSet<ImportBatch> ImportBatches { get; }

    /// <summary>
    /// Saves all changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of written rows.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
