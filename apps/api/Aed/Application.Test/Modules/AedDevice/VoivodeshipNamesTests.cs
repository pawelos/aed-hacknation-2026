using Domain.Modules.AedDevice.Model;

namespace Application.Test.Modules.AedDevice;

/// <summary>
/// TERYT codes and Polish names of voivodeships, as read from imported files.
/// </summary>
public sealed class VoivodeshipNamesTests
{
    [Test]
    public void Every_voivodeship_has_a_name_and_a_two_digit_code()
    {
        Voivodeship[] all = Enum.GetValues<Voivodeship>();

        Assert.Multiple(() =>
        {
            Assert.That(all, Has.Length.EqualTo(16));
            Assert.That(all.Select(VoivodeshipNames.Polish), Is.Unique);
            Assert.That(all.Select(VoivodeshipNames.Teryt), Has.All.Length.EqualTo(2));
        });
    }

    [TestCase("mazowieckie", Voivodeship.Masovian)]
    [TestCase("Województwo Dolnośląskie", Voivodeship.LowerSilesian)]
    [TestCase("  ŚLĄSKIE ", Voivodeship.Silesian)]
    [TestCase("02", Voivodeship.LowerSilesian)]
    [TestCase("WestPomeranian", Voivodeship.WestPomeranian)]
    public void Parses_name_code_and_enum_name(string value, Voivodeship expected)
    {
        bool parsed = VoivodeshipNames.TryParse(value, out Voivodeship voivodeship);

        Assert.Multiple(() =>
        {
            Assert.That(parsed, Is.True);
            Assert.That(voivodeship, Is.EqualTo(expected));
        });
    }

    [TestCase("")]
    [TestCase("Bawaria")]
    [TestCase("99")]
    public void Rejects_unknown_values(string value)
    {
        Assert.That(VoivodeshipNames.TryParse(value, out _), Is.False);
    }
}
