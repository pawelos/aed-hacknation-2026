namespace Domain.Modules.Owner.Constants;

/// <summary>
/// Route name and field limits of owners. The limits are shared by EF, the validators and the web form.
/// </summary>
public static class OwnerConstant
{
    /// <summary>
    /// Controller (route) name.
    /// </summary>
    public const string ControllerName = "Owner";

    /// <summary>
    /// Maximum length of the name.
    /// </summary>
    public const int NameMaxLength = 300;

    /// <summary>
    /// Maximum length of the e-mail.
    /// </summary>
    public const int EmailMaxLength = 200;

    /// <summary>
    /// Maximum length of the phone number.
    /// </summary>
    public const int PhoneMaxLength = 20;

    /// <summary>
    /// Maximum length of the website address.
    /// </summary>
    public const int WebsiteMaxLength = 500;

    /// <summary>
    /// Maximum length of the REGON number.
    /// </summary>
    public const int RegonMaxLength = 14;
}
