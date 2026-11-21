using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Common.Xlsx;
using SharedKernel;

namespace Application.Modules.OpenData.Template;

/// <summary>
/// Handler for <see cref="GetReferenceTemplateQuery"/>. Sheets: "Zakres danych" (one row per field), "Słowniki"
/// (codes with Polish and English labels), "Przykład" (a few records of the data set with the column names a
/// holder fills in), "Informacje" (format rules).
/// </summary>
internal sealed class GetReferenceTemplateQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetReferenceTemplateQuery, OpenDataFile>
{
    private const int ExampleRows = 5;

    /// <summary>
    /// Builds the workbook.
    /// </summary>
    public async ValueTask<Result<OpenDataFile>> Handle(GetReferenceTemplateQuery query, CancellationToken cancellationToken)
    {
        List<AedExportRow> rows = await ExportRows.LoadAsync(dbContext, null, null, cancellationToken);

        var fields = new XlsxSheet(
            "Zakres danych",
            ["Lp.", "Pole", "Typ", "Wymagane", "Maks. długość", "Format / wartości", "Opis", "Description (EN)", "Przykład", "Słownik"],
            AedDataSchema.Fields
                .Select((f, i) => (IReadOnlyList<object?>)
                [
                    i + 1,
                    f.Name,
                    AedDataSchema.TypeName(f.Type),
                    f.Required ? "tak" : "nie",
                    f.MaxLength,
                    f.Format,
                    f.DescriptionPl,
                    f.DescriptionEn,
                    f.Example,
                    f.Dictionary ?? string.Empty
                ])
                .ToList());

        var dictionaries = new XlsxSheet(
            "Słowniki",
            ["Słownik", "Kod", "Opis", "Description (EN)"],
            AedDataSchema.Dictionaries
                .SelectMany(d => d.Codes.Select(c => (IReadOnlyList<object?>)[d.Name, c.Code, c.LabelPl, c.LabelEn]))
                .ToList());

        var example = new XlsxSheet(
            "Przykład",
            AedDataSchema.Fields.Select(f => f.Name).ToList(),
            rows
                .OrderBy(r => r.City)
                .Take(ExampleRows)
                .Select(r => (IReadOnlyList<object?>)AedDataSchema.Fields.Select(f => Cell(f.Value(r))).ToList())
                .ToList());

        var info = new XlsxSheet(
            "Informacje",
            ["Zasada", "Opis"],
            [
                ["Wersja schematu", AedDataSchema.Version],
                ["Kodowanie", "UTF-8"],
                ["Plik CSV", "RFC 4180: separator przecinek, nagłówek z nazwami pól z arkusza „Zakres danych”, wartości z przecinkiem w cudzysłowie."],
                ["Współrzędne", "WGS 84 (EPSG:4326), stopnie dziesiętne z kropką, 6 miejsc po kropce."],
                ["Daty", "YYYY-MM-DD; data i czas: ISO 8601 w UTC (YYYY-MM-DDThh:mm:ssZ)."],
                ["Godziny dostępności", "Format OpenStreetMap opening_hours, np. „Mo-Fr 08:00-16:00; Sa 10:00-14:00” albo „24/7”."],
                ["Kody", "Pola słownikowe przyjmują tylko kody z arkusza „Słowniki”."],
                ["Licencja", "Dysponent wybiera CC0 1.0 albo CC BY 4.0; licencja jest zapisana w każdym rekordzie."],
                ["Formaty publikacji", "Dane publikowane są w CSV, GeoJSON i JSON. Ten plik XLSX jest wyłącznie wzorcem zakresu danych."]
            ]);

        byte[] content = SimpleXlsxWriter.Write([fields, dictionaries, example, info]);
        return new OpenDataFile(content, SimpleXlsxWriter.ContentType, OpenDataConstant.TemplateFileName, null);
    }

    /// <summary>
    /// Cell value of a field: booleans as the text written in CSV.
    /// </summary>
    private static object? Cell(object? value)
    {
        switch (value)
        {
            case bool flag:
                return flag ? "true" : "false";
            default:
                return value;
        }
    }
}
