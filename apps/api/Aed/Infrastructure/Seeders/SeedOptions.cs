namespace Infrastructure.Seeders;

/// <summary>
/// Seed configuration (section <c>Seed</c>). Accounts without a password are skipped, so production
/// creates no accounts unless the passwords come from the environment (<c>Seed__AdminPassword</c>).
/// </summary>
public sealed class SeedOptions
{
    /// <summary>
    /// Name of the configuration section.
    /// </summary>
    public const string SectionName = "Seed";

    /// <summary>
    /// E-mail of the administrator account.
    /// </summary>
    public string AdminEmail { get; set; } = "admin@aed.local";

    /// <summary>
    /// Password of the administrator account; empty skips the account.
    /// </summary>
    public string AdminPassword { get; set; } = string.Empty;

    /// <summary>
    /// Password of the demo owner accounts (<c>wroclaw@aed.local</c>, <c>osp@aed.local</c>); empty skips them.
    /// </summary>
    public string DemoPassword { get; set; } = string.Empty;

    /// <summary>
    /// True adds the demo owners, sources and AED records (fictional, for the prototype) when the database has none.
    /// </summary>
    public bool DemoData { get; set; }

    /// <summary>
    /// Development only: drops and recreates the database on start.
    /// </summary>
    public bool ResetDatabase { get; set; }
}
