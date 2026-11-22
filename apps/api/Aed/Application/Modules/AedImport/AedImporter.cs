using Application.Common.Localization;
using System.Text.Json;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Common.Geo;
using Application.Modules.AedDevice;
using Application.Modules.AedImport.Parsing;
using Domain.Modules.AedDevice.Constants;
using Domain.Modules.AedDevice.Model;
using Domain.Modules.ImportBatch.Model;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using AedDeviceEntity = Domain.Modules.AedDevice.Model.AedDevice;
using DataSourceEntity = Domain.Modules.DataSource.Model.DataSource;
using ImportBatchEntity = Domain.Modules.ImportBatch.Model.ImportBatch;

namespace Application.Modules.AedImport;

/// <summary>
/// Imports the rows of a CSV or GeoJSON file into one data source:
/// <list type="number">
/// <item>every row is validated (<see cref="AedRowNormalizer"/>); rows with errors are rejected, rows with warnings
/// are imported and marked as needing verification;</item>
/// <item>a row with the source id of an existing record of the source updates it (without a source id: a record
/// of the source within 5 m); otherwise a new record is created;</item>
/// <item>a record of another source within 30 m with a similar name or the same address is reported as a possible
/// duplicate, and the imported record is marked for verification (an administrator decides; both stay);</item>
/// <item>optionally, records of the source missing from the file are marked as outdated (full refresh).</item>
/// </list>
/// Every record keeps its source, so the origin and reference scope stay known. A dry run reports without saving.
/// </summary>
internal sealed class AedImporter(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider)
{
    private static readonly JsonSerializerOptions ReportJsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<ImportReport>> ImportAsync(
        Guid dataSourceId,
        string fileName,
        ImportFormat format,
        byte[] content,
        bool dryRun,
        bool markMissingAsOutdated,
        CancellationToken cancellationToken)
    {
        Result<DataSourceEntity> sourceResult = await DataSourceAccess.GetAsync(dbContext, userContext, dataSourceId, cancellationToken);
        if (sourceResult.IsFailure)
        {
            return Result.Failure<ImportReport>(sourceResult.Error);
        }

        DataSourceEntity source = sourceResult.Value;
        string text = ImportText.Decode(content);
        bool geoJson = IsGeoJson(fileName, text);
        ParsedFile parsed = geoJson ? GeoJsonReader.Read(text) : CsvTableReader.Read(text);
        if (parsed.Error is not null)
        {
            return Result.Failure<ImportReport>(ImportErrors.UnreadableFile(parsed.Error));
        }

        if (parsed.Rows.Count > AedImportConstant.MaxRows)
        {
            return Result.Failure<ImportReport>(ImportErrors.TooManyRows(parsed.Rows.Count));
        }

        DateTime utcNow = timeProvider.GetUtcNow().UtcDateTime;
        List<NormalizedRow> normalized = parsed.Rows.Select(r => AedRowNormalizer.Normalize(r, utcNow)).ToList();

        List<AedDeviceEntity> existing = await dbContext.AedDevices
            .Where(d => d.DataSourceId == source.Id && !d.IsDeleted)
            .ToListAsync(cancellationToken);
        Dictionary<string, AedDeviceEntity> bySourceId = existing
            .Where(d => d.SourceRecordId.Length > 0)
            .GroupBy(d => d.SourceRecordId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        List<OtherRecord> others = await LoadOtherSourcesAsync(source.Id, normalized, cancellationToken);

        var reports = new List<ImportRowReport>(parsed.Rows.Count);
        var seenSourceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var matched = new HashSet<Guid>();
        int created = 0, updated = 0, unchanged = 0, rejected = 0, withWarnings = 0, duplicates = 0;

        for (int i = 0; i < parsed.Rows.Count; i++)
        {
            ImportedRow row = parsed.Rows[i];
            NormalizedRow result = normalized[i];
            var errors = result.Errors.ToList();
            var warnings = result.Warnings.ToList();
            NormalizedAed? aed = result.Aed;

            if (aed is not null && aed.SourceRecordId.Length > 0 && !seenSourceIds.Add(aed.SourceRecordId))
            {
                errors.Add(new ImportIssue("id_zrodlowe", "Import.DuplicateInFile",
                    Lang.T($"The source id '{aed.SourceRecordId}' appears more than once in the file; only the first row was imported.", $"Id '{aed.SourceRecordId}' występuje w pliku więcej niż raz; zaimportowano tylko pierwszy wiersz.")));
                aed = null;
            }

            if (aed is null)
            {
                rejected++;
                reports.Add(new ImportRowReport
                {
                    Row = row.RowNumber,
                    Action = ImportRowAction.Rejected,
                    SourceRecordId = row.Get("id_zrodlowe"),
                    LocationName = row.Get("nazwa_lokalizacji"),
                    Errors = errors,
                    Warnings = warnings
                });
                continue;
            }

            OtherRecord? duplicate = FindDuplicate(aed, others);
            if (duplicate is not null)
            {
                duplicates++;
                warnings.Add(new ImportIssue(null, "Import.PossibleDuplicate",
                    Lang.T($"Possible duplicate of '{duplicate.Name}' ({duplicate.SourceName}), {Math.Round(duplicate.DistanceMeters)} m away.", $"Możliwy duplikat rekordu '{duplicate.Name}' ({duplicate.SourceName}), {Math.Round(duplicate.DistanceMeters)} m dalej.")));
            }

            AedDeviceEntity? target = aed.SourceRecordId.Length > 0
                ? bySourceId.GetValueOrDefault(aed.SourceRecordId)
                : existing.FirstOrDefault(d => !matched.Contains(d.Id)
                                               && d.SourceRecordId.Length == 0
                                               && Meters(d.Latitude, d.Longitude, aed.Latitude, aed.Longitude) <= AedImportConstant.SameSourceMatchMeters);

            ImportRowAction action;
            if (target is null)
            {
                target = new AedDeviceEntity
                {
                    Id = Guid.NewGuid(),
                    DataSourceId = source.Id,
                    OwnerId = source.OwnerId,
                    CreatedAt = utcNow,
                    CreatedBy = userContext.UserId
                };
                Apply(target, aed, warnings);
                if (!dryRun)
                {
                    dbContext.AedDevices.Add(target);
                }

                action = ImportRowAction.Created;
                created++;
            }
            else
            {
                matched.Add(target.Id);
                string before = Fingerprint(target);
                Apply(target, aed, warnings);
                if (before == Fingerprint(target))
                {
                    action = ImportRowAction.Unchanged;
                    unchanged++;
                }
                else
                {
                    target.UpdatedAt = utcNow;
                    target.UpdatedBy = userContext.UserId;
                    action = ImportRowAction.Updated;
                    updated++;
                }
            }

            if (warnings.Count > 0)
            {
                withWarnings++;
            }

            reports.Add(new ImportRowReport
            {
                Row = row.RowNumber,
                Action = action,
                SourceRecordId = aed.SourceRecordId.Length > 0 ? aed.SourceRecordId : null,
                LocationName = aed.LocationName,
                RecordId = target.Id,
                Warnings = warnings,
                PossibleDuplicateOf = duplicate?.Id
            });
        }

        int markedOutdated = 0;
        if (markMissingAsOutdated)
        {
            HashSet<Guid> inFile = reports.Where(r => r.RecordId is not null).Select(r => r.RecordId!.Value).ToHashSet();
            foreach (AedDeviceEntity missing in existing.Where(d => !inFile.Contains(d.Id) && d.Status != AedStatus.Outdated && d.Status != AedStatus.Withdrawn))
            {
                missing.Status = AedStatus.Outdated;
                missing.NeedsVerification = true;
                missing.VerificationReason = Truncate(Lang.T("The record is missing from the last import of its source.", "Rekordu nie było w ostatnim imporcie jego źródła."));
                missing.UpdatedAt = utcNow;
                missing.UpdatedBy = userContext.UserId;
                markedOutdated++;
            }
        }

        var report = new ImportReport
        {
            DryRun = dryRun,
            DataSourceId = source.Id,
            DataSourceName = source.Name,
            FileName = fileName,
            Format = geoJson ? "GeoJSON" : "CSV",
            TotalRows = parsed.Rows.Count,
            Created = created,
            Updated = updated,
            Unchanged = unchanged,
            Rejected = rejected,
            WithWarnings = withWarnings,
            PossibleDuplicates = duplicates,
            MarkedOutdated = markedOutdated,
            UnknownColumns = parsed.UnknownColumns,
            // A dry run shows every row; a stored import keeps the rows with something to say.
            Rows = dryRun
                ? reports
                : reports.Where(r => r.Errors.Count > 0 || r.Warnings.Count > 0).Take(AedImportConstant.MaxReportRows).ToList()
        };

        if (dryRun)
        {
            return report;
        }

        var batch = new ImportBatchEntity
        {
            Id = Guid.NewGuid(),
            DataSourceId = source.Id,
            Format = format == ImportFormat.Url ? ImportFormat.Url : geoJson ? ImportFormat.GeoJson : ImportFormat.Csv,
            FileName = Truncate(fileName, AedImportConstant.MaxFileNameLength),
            TotalRows = report.TotalRows,
            Created = created,
            Updated = updated,
            Rejected = rejected,
            PossibleDuplicates = duplicates,
            CreatedAt = utcNow,
            CreatedBy = userContext.UserId
        };
        report = report with { ImportId = batch.Id };
        batch.ReportJson = JsonSerializer.Serialize(report, ReportJsonOptions);
        dbContext.ImportBatches.Add(batch);

        // Records that only shared the source were changed in place; the source counts as verified now.
        source.LastVerifiedAt = utcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return report;
    }

    /// <summary>
    /// Writes the imported values; warnings mark the record for verification, a clean row clears an older flag.
    /// </summary>
    private static void Apply(AedDeviceEntity target, NormalizedAed aed, List<ImportIssue> warnings)
    {
        target.SourceRecordId = aed.SourceRecordId;
        target.LocationName = aed.LocationName;
        target.Address = aed.Address;
        target.PostalCode = aed.PostalCode;
        target.City = aed.City;
        target.Municipality = aed.Municipality;
        target.MunicipalityTeryt = aed.MunicipalityTeryt;
        target.County = aed.County;
        target.Voivodeship = aed.Voivodeship;
        if (target.Latitude != aed.Latitude || target.Longitude != aed.Longitude || target.Location is null)
        {
            target.Latitude = aed.Latitude;
            target.Longitude = aed.Longitude;
            target.Location = AedLocation.ToPoint(aed.Latitude, aed.Longitude);
        }

        target.PlacementDetails = aed.PlacementDetails;
        target.IsIndoor = aed.IsIndoor;
        target.FacilityType = aed.FacilityType;
        target.Availability = aed.Availability;
        target.AvailabilityHours = aed.AvailabilityHours;
        target.AccessNotes = aed.AccessNotes;
        target.Status = aed.Status;
        target.ContactPhone = aed.ContactPhone;
        target.LastVerifiedAt = aed.LastVerifiedAt;
        target.NeedsVerification = warnings.Count > 0;
        target.VerificationReason = warnings.Count > 0
            ? Truncate(string.Join(" ", warnings.Select(w => w.Message)))
            : string.Empty;
    }

    private static string Fingerprint(AedDeviceEntity d) =>
        string.Join('|', d.SourceRecordId, d.LocationName, d.Address, d.PostalCode, d.City, d.Municipality,
            d.MunicipalityTeryt, d.County, d.Voivodeship, d.Latitude, d.Longitude, d.PlacementDetails, d.IsIndoor,
            d.FacilityType, d.Availability, d.AvailabilityHours, d.AccessNotes, d.Status, d.ContactPhone,
            d.LastVerifiedAt?.Ticks, d.NeedsVerification, d.VerificationReason);

    private sealed record OtherRecord(Guid Id, string Name, string Address, string SourceName, decimal Latitude, decimal Longitude)
    {
        public double DistanceMeters { get; init; }
    }

    /// <summary>
    /// Records of the other sources around the imported points (one query for the whole file).
    /// </summary>
    private async Task<List<OtherRecord>> LoadOtherSourcesAsync(Guid sourceId, List<NormalizedRow> rows, CancellationToken cancellationToken)
    {
        List<NormalizedAed> points = rows.Where(r => r.Aed is not null).Select(r => r.Aed!).ToList();
        if (points.Count == 0)
        {
            return [];
        }

        // About 55 m of margin around the file.
        const decimal margin = 0.0005m;
        decimal minLat = points.Min(p => p.Latitude) - margin, maxLat = points.Max(p => p.Latitude) + margin;
        decimal minLng = points.Min(p => p.Longitude) - margin, maxLng = points.Max(p => p.Longitude) + margin;

        return await (
                from d in dbContext.AedDevices.AsNoTracking()
                join s in dbContext.DataSources on d.DataSourceId equals s.Id
                where d.DataSourceId != sourceId && !d.IsDeleted && d.Status != AedStatus.Withdrawn
                      && d.Latitude >= minLat && d.Latitude <= maxLat && d.Longitude >= minLng && d.Longitude <= maxLng
                select new OtherRecord(d.Id, d.LocationName, d.Address, s.Name, d.Latitude, d.Longitude))
            .ToListAsync(cancellationToken);
    }

    private static OtherRecord? FindDuplicate(NormalizedAed aed, List<OtherRecord> others) =>
        others
            .Select(o => o with { DistanceMeters = Meters(o.Latitude, o.Longitude, aed.Latitude, aed.Longitude) })
            .Where(o => o.DistanceMeters <= AedImportConstant.DuplicateRadiusMeters
                        && DuplicateMatcher.IsSimilar(aed.LocationName, aed.Address, o.Name, o.Address))
            .OrderBy(o => o.DistanceMeters)
            .FirstOrDefault();

    private static double Meters(decimal lat1, decimal lng1, decimal lat2, decimal lng2) =>
        GeoDistance.Kilometers((double)lat1, (double)lng1, (double)lat2, (double)lng2) * 1000;

    private static bool IsGeoJson(string fileName, string text)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (extension is ".geojson" or ".json")
        {
            return true;
        }

        if (extension is ".csv" or ".txt")
        {
            return false;
        }

        return text.TrimStart().StartsWith('{');
    }

    private static string Truncate(string text, int max = AedDeviceConstant.VerificationReasonMaxLength) =>
        text.Length <= max ? text : text[..(max - 1)] + "…";
}
