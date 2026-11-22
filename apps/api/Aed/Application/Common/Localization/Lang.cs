using System.Globalization;

namespace Application.Common.Localization;

/// <summary>
/// Message in the language of the request: Polish when the UI culture is Polish (Accept-Language pl, set by
/// UseRequestLocalization in the API), else English. For the texts the web panel shows as they are (field errors,
/// import reports); stable codes stay next to them.
/// </summary>
public static class Lang
{
    public static bool IsPolish => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "pl";

    /// <summary>
    /// The English or the Polish text.
    /// </summary>
    public static string T(string en, string pl) => IsPolish ? pl : en;
}
