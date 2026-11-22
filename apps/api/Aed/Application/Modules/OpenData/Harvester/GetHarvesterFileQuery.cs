using Application.Abstractions.Messaging;

namespace Application.Modules.OpenData.Harvester;

/// <summary>
/// File for the dane.gov.pl XML importer: the whole integrated data set, or the wykaz of one provider (one data
/// source), which that provider registers in the portal under its own institution profile.
/// </summary>
/// <param name="DataSourceId">One data source; null for the whole data set.</param>
/// <param name="Checksum">True for the MD5 file next to the XML file.</param>
public sealed record GetHarvesterFileQuery(Guid? DataSourceId, bool Checksum) : IQuery<OpenDataFile>;
