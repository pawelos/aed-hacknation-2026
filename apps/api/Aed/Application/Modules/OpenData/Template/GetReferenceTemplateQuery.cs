using Application.Abstractions.Messaging;

namespace Application.Modules.OpenData.Template;

/// <summary>
/// The XLSX template of the reference data set: fields, dictionaries and an example (documentation only;
/// the data itself is published in CSV, GeoJSON and JSON).
/// </summary>
public sealed record GetReferenceTemplateQuery : IQuery<OpenDataFile>;
