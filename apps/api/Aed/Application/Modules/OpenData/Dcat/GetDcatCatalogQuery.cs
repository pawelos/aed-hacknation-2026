using Application.Abstractions.Messaging;

namespace Application.Modules.OpenData.Dcat;

/// <summary>
/// The DCAT-AP-PL catalog (RDF/XML) of the data set, for the dane.gov.pl harvester.
/// </summary>
public sealed record GetDcatCatalogQuery : IQuery<OpenDataFile>;
