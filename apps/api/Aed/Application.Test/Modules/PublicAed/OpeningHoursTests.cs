using Application.Modules.PublicAed;
using Domain.Modules.AedDevice.Model;

namespace Application.Test.Modules.PublicAed;

/// <summary>
/// "Available now" from OSM opening_hours. 2026-11-23 is a Monday.
/// </summary>
public sealed class OpeningHoursTests
{
    private static DateTime At(int day, int hour, int minute = 0, int month = 11) =>
        new(2026, month, day, hour, minute, 0, DateTimeKind.Unspecified);

    [Test]
    public void Always_open_text()
    {
        Assert.That(OpeningHours.IsOpen("24/7", At(23, 3)), Is.True);
    }

    [TestCase(23, 8, 0, true)]   // Monday, opening minute
    [TestCase(23, 15, 59, true)]
    [TestCase(23, 16, 0, false)] // closing minute
    [TestCase(23, 7, 59, false)]
    [TestCase(28, 12, 0, false)] // Saturday
    public void Weekday_range(int day, int hour, int minute, bool expected)
    {
        Assert.That(OpeningHours.IsOpen("Mo-Fr 08:00-16:00", At(day, hour, minute)), Is.EqualTo(expected));
    }

    [Test]
    public void Later_rule_and_several_ranges()
    {
        const string hours = "Mo-Fr 10:00-19:00; Sa 10:00-15:00";

        Assert.Multiple(() =>
        {
            Assert.That(OpeningHours.IsOpen(hours, At(28, 14)), Is.True);  // Saturday
            Assert.That(OpeningHours.IsOpen(hours, At(28, 16)), Is.False);
            Assert.That(OpeningHours.IsOpen(hours, At(29, 12)), Is.False); // Sunday
            Assert.That(OpeningHours.IsOpen("Mo-Fr 08:00-12:00,13:00-16:00", At(23, 12, 30)), Is.False);
        });
    }

    [Test]
    public void Range_past_midnight_stays_open_next_morning()
    {
        const string hours = "Fr 22:00-06:00";

        Assert.Multiple(() =>
        {
            Assert.That(OpeningHours.IsOpen(hours, At(27, 23)), Is.True);  // Friday night
            Assert.That(OpeningHours.IsOpen(hours, At(28, 5)), Is.True);   // Saturday morning
            Assert.That(OpeningHours.IsOpen(hours, At(28, 7)), Is.False);
        });
    }

    [Test]
    public void Month_range_is_seasonal()
    {
        Assert.Multiple(() =>
        {
            Assert.That(OpeningHours.IsOpen("Jun-Aug 09:00-19:00", At(15, 12, month: 7)), Is.True);
            Assert.That(OpeningHours.IsOpen("Jun-Aug 09:00-19:00", At(23, 12)), Is.False);
        });
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase("po umówieniu")]
    [TestCase("Mo-Fr 8-16")]
    [TestCase("PH off")]
    public void Unknown_text_gives_no_data(string hours)
    {
        Assert.That(OpeningHours.IsOpen(hours, At(23, 12)), Is.Null);
    }

    [Test]
    public void Public_device_is_available_without_hours()
    {
        Assert.Multiple(() =>
        {
            Assert.That(OpeningHours.IsAvailable(AedAvailability.Public24h, "", At(23, 3)), Is.True);
            Assert.That(OpeningHours.IsAvailable(AedAvailability.Restricted, "", At(23, 3)), Is.Null);
        });
    }

    [Test]
    public void Poland_time_follows_daylight_saving()
    {
        Assert.Multiple(() =>
        {
            Assert.That(OpeningHours.ToPolandTime(new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero)).Hour,
                Is.EqualTo(13));
            Assert.That(OpeningHours.ToPolandTime(new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.Zero)).Hour,
                Is.EqualTo(14));
        });
    }
}
