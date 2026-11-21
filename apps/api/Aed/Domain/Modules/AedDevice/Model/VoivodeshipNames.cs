namespace Domain.Modules.AedDevice.Model;

/// <summary>
/// TERYT codes and official Polish names of <see cref="Voivodeship"/>, as written in the published data set.
/// </summary>
public static class VoivodeshipNames
{
    /// <summary>
    /// Two-digit TERYT code, e.g. <c>14</c> for mazowieckie.
    /// </summary>
    /// <param name="voivodeship">The voivodeship.</param>
    /// <returns>The code.</returns>
    public static string Teryt(Voivodeship voivodeship) =>
        ((int)voivodeship).ToString("00", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>
    /// Official Polish name in lower case, e.g. <c>mazowieckie</c>.
    /// </summary>
    /// <param name="voivodeship">The voivodeship.</param>
    /// <returns>The name.</returns>
    public static string Polish(Voivodeship voivodeship)
    {
        switch (voivodeship)
        {
            case Voivodeship.LowerSilesian:
                return "dolnośląskie";
            case Voivodeship.KuyavianPomeranian:
                return "kujawsko-pomorskie";
            case Voivodeship.Lublin:
                return "lubelskie";
            case Voivodeship.Lubusz:
                return "lubuskie";
            case Voivodeship.Lodz:
                return "łódzkie";
            case Voivodeship.LesserPoland:
                return "małopolskie";
            case Voivodeship.Masovian:
                return "mazowieckie";
            case Voivodeship.Opole:
                return "opolskie";
            case Voivodeship.Subcarpathian:
                return "podkarpackie";
            case Voivodeship.Podlaskie:
                return "podlaskie";
            case Voivodeship.Pomeranian:
                return "pomorskie";
            case Voivodeship.Silesian:
                return "śląskie";
            case Voivodeship.HolyCross:
                return "świętokrzyskie";
            case Voivodeship.WarmianMasurian:
                return "warmińsko-mazurskie";
            case Voivodeship.GreaterPoland:
                return "wielkopolskie";
            case Voivodeship.WestPomeranian:
                return "zachodniopomorskie";
            default:
                throw new ArgumentOutOfRangeException(nameof(voivodeship), voivodeship, null);
        }
    }

    /// <summary>
    /// Finds a voivodeship by its Polish name (any case, with or without "województwo") or its TERYT code.
    /// </summary>
    /// <param name="value">Name or code.</param>
    /// <param name="voivodeship">The voivodeship found.</param>
    /// <returns>True when found.</returns>
    public static bool TryParse(string? value, out Voivodeship voivodeship)
    {
        voivodeship = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string normalized = value.Trim().ToLowerInvariant();
        if (normalized.StartsWith("województwo ", StringComparison.Ordinal))
        {
            normalized = normalized["województwo ".Length..].Trim();
        }

        foreach (Voivodeship candidate in Enum.GetValues<Voivodeship>())
        {
            if (normalized == Polish(candidate)
                || normalized == Teryt(candidate)
                || string.Equals(normalized, candidate.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                voivodeship = candidate;
                return true;
            }
        }

        return false;
    }
}
