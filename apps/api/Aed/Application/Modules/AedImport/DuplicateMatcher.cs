using Application.Modules.AedImport.Parsing;

namespace Application.Modules.AedImport;

/// <summary>
/// Whether two records of different sources likely describe the same device: close to each other and with a
/// similar name or the same address. "Ratusz Wrocław – AED w holu" and "Urząd Miejski – Nowy Ratusz" 5 m apart
/// share "ratusz"; "Sukiennice 9" and "ul. Sukiennice 9" are the same address.
/// </summary>
public static class DuplicateMatcher
{
    private static readonly HashSet<string> StopWords =
    [
        "aed", "defibrylator", "przyklad", "demo", "ul", "al", "pl", "os", "w", "i", "na", "przy", "the", "of", "nr"
    ];

    /// <summary>
    /// True when the names share enough words (Jaccard ≥ 1/3) or the addresses are the same.
    /// </summary>
    public static bool IsSimilar(string nameA, string addressA, string nameB, string addressB)
    {
        string normalizedAddressA = Words(addressA);
        if (normalizedAddressA.Length > 0 && normalizedAddressA == Words(addressB))
        {
            return true;
        }

        HashSet<string> a = Tokens(nameA);
        HashSet<string> b = Tokens(nameB);
        if (a.Count == 0 || b.Count == 0)
        {
            return false;
        }

        int common = a.Count(b.Contains);
        int union = a.Count + b.Count - common;
        return common > 0 && (double)common / union >= 1d / 3;
    }

    private static HashSet<string> Tokens(string text) =>
        FieldAliases.Normalize(text)
            .Split('_', StringSplitOptions.RemoveEmptyEntries)
            .Where(t => t.Length > 1 && !StopWords.Contains(t))
            .ToHashSet(StringComparer.Ordinal);

    /// <summary>
    /// Address words in a fixed order, building numbers kept ("9" tells "Sukiennice 9" from "Sukiennice 1").
    /// </summary>
    private static string Words(string text) =>
        string.Join('_', FieldAliases.Normalize(text)
            .Split('_', StringSplitOptions.RemoveEmptyEntries)
            .Where(t => (t.Length > 1 || char.IsAsciiDigit(t[0])) && !StopWords.Contains(t))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal));
}
