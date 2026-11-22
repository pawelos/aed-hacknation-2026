using System.Text;
using Application.Abstractions.Messaging;
using Application.Modules.OpenData;
using SharedKernel;

namespace Application.Modules.AedImport.Template;

/// <summary>
/// Empty CSV for holders: the header with the fields of the reference data set and one example row
/// (UTF-8 with BOM, so Excel opens Polish letters correctly; the import accepts both).
/// </summary>
public sealed record GetImportTemplateQuery : IQuery<OpenDataFile>;

internal sealed class GetImportTemplateQueryHandler : IQueryHandler<GetImportTemplateQuery, OpenDataFile>
{
    /// <summary>
    /// Fields filled by the national data set itself, not by the holder.
    /// </summary>
    private static readonly HashSet<string> Generated =
    [
        "id_rekordu", "teryt_wojewodztwa", "wlasciciel_dysponent", "zrodlo_danych", "url_zrodla", "licencja_danych",
        "zakres_referencyjny", "wymaga_weryfikacji", "data_aktualizacji"
    ];

    public ValueTask<Result<OpenDataFile>> Handle(GetImportTemplateQuery query, CancellationToken cancellationToken)
    {
        List<AedField> fields = AedDataSchema.Fields.Where(f => !Generated.Contains(f.Name)).ToList();
        var csv = new StringBuilder();
        csv.AppendJoin(',', fields.Select(f => f.Name)).Append("\r\n");
        csv.AppendJoin(',', fields.Select(f => Quote(f.Example))).Append("\r\n");

        byte[] content = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetPreamble()
            .Concat(Encoding.UTF8.GetBytes(csv.ToString()))
            .ToArray();
        return ValueTask.FromResult<Result<OpenDataFile>>(
            new OpenDataFile(content, "text/csv; charset=utf-8", "szablon-importu-aed.csv", null));
    }

    private static string Quote(string value) =>
        value.IndexOfAny([',', '"', '\n']) >= 0 ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
}
