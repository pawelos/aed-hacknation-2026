using SharedKernel;

namespace Domain.Modules.Owner.Model;

/// <summary>
/// Owner or data holder of AED devices (field <c>wlasciciel_dysponent</c> of the reference data set):
/// a municipality, school, company, fire brigade and so on.
/// </summary>
public sealed class Owner : BaseEntity
{
    /// <summary>
    /// Name of the organisation.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Kind of organisation.
    /// </summary>
    public OwnerType Type { get; set; } = OwnerType.Other;

    /// <summary>
    /// Contact e-mail of the organisation for data questions.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contact phone of the organisation (E.164, e.g. <c>+48221234567</c>).
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Website of the organisation.
    /// </summary>
    public string Website { get; set; } = string.Empty;

    /// <summary>
    /// Polish statistical number REGON (9 or 14 digits); empty when unknown.
    /// </summary>
    public string Regon { get; set; } = string.Empty;
}
