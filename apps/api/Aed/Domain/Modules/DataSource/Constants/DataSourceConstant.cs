namespace Domain.Modules.DataSource.Constants;

/// <summary>
/// Route name and field limits of data sources. The limits are shared by EF, the validators and the web form.
/// </summary>
public static class DataSourceConstant
{
    /// <summary>
    /// Controller (route) name.
    /// </summary>
    public const string ControllerName = "DataSource";

    /// <summary>
    /// Maximum length of the name.
    /// </summary>
    public const int NameMaxLength = 300;

    /// <summary>
    /// Maximum length of the source address.
    /// </summary>
    public const int UrlMaxLength = 1000;

    /// <summary>
    /// Maximum length of the reference scope.
    /// </summary>
    public const int ReferenceScopeMaxLength = 500;
}
