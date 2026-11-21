using System.Globalization;
using Domain.Modules.AedDevice.Model;

namespace Application.Modules.PublicAed;

/// <summary>
/// "Is the AED available now?" from the OpenStreetMap <c>opening_hours</c> text of a record.
/// Supports the subset used by owners: <c>24/7</c>, rules separated by <c>;</c> with optional months
/// (<c>Jun-Aug</c>), optional days (<c>Mo-Fr</c>, <c>Sa,Su</c>), time ranges (<c>08:00-16:00</c>,
/// several separated by <c>,</c>, also past midnight like <c>22:00-06:00</c>) and <c>off</c>.
/// A later matching rule overrides an earlier one, as in OSM. Anything else gives <c>null</c>
/// ("no data"), never a false "open".
/// </summary>
public static class OpeningHours
{
    private static readonly string[] Days = ["Mo", "Tu", "We", "Th", "Fr", "Sa", "Su"];

    private static readonly string[] Months =
        ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    private static readonly TimeZoneInfo PolandTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

    /// <summary>
    /// Local time in Poland of a UTC instant.
    /// </summary>
    /// <param name="utcNow">UTC time.</param>
    /// <returns>Local time (Europe/Warsaw).</returns>
    public static DateTime ToPolandTime(DateTimeOffset utcNow) =>
        TimeZoneInfo.ConvertTime(utcNow, PolandTimeZone).DateTime;

    /// <summary>
    /// Whether a device can be reached at the given local time. Devices open to anyone day and night are
    /// always available; for the others the hours decide.
    /// </summary>
    /// <param name="availability">Availability of the device.</param>
    /// <param name="hours">Opening hours text.</param>
    /// <param name="localTime">Local time in Poland.</param>
    /// <returns>True or false, or null when the hours are missing or not understood.</returns>
    public static bool? IsAvailable(AedAvailability availability, string? hours, DateTime localTime)
    {
        switch (availability)
        {
            case AedAvailability.Public24h:
                return true;
            case AedAvailability.BusinessHours:
            case AedAvailability.Restricted:
                return IsOpen(hours, localTime);
            default:
                return null;
        }
    }

    /// <summary>
    /// Whether the hours are open at the given local time.
    /// </summary>
    /// <param name="hours">Opening hours text.</param>
    /// <param name="localTime">Local time in Poland.</param>
    /// <returns>True or false, or null when the text is empty or not understood.</returns>
    public static bool? IsOpen(string? hours, DateTime localTime)
    {
        if (string.IsNullOrWhiteSpace(hours))
        {
            return null;
        }

        string[] ruleTexts = hours.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var rules = new List<Rule>(ruleTexts.Length);
        foreach (string ruleText in ruleTexts)
        {
            Rule? rule = ParseRule(ruleText);
            if (rule is null)
            {
                return null;
            }

            rules.Add(rule);
        }

        if (rules.Count == 0)
        {
            return null;
        }

        int minute = localTime.Hour * 60 + localTime.Minute;
        int today = DayIndex(localTime.DayOfWeek);
        int yesterday = (today + 6) % 7;
        int month = localTime.Month - 1;
        int yesterdayMonth = localTime.AddDays(-1).Month - 1;

        bool? result = null;
        foreach (Rule rule in rules)
        {
            // The rule that applies to today decides; an earlier result is overridden (OSM semantics).
            if (rule.Applies(today, month))
            {
                result = rule.IsOpenOnDay(minute);
            }

            // A range past midnight started yesterday keeps the device open this morning.
            if (rule.Applies(yesterday, yesterdayMonth) && rule.IsOpenAfterMidnight(minute))
            {
                result = true;
            }
        }

        return result ?? false;
    }

    private static int DayIndex(DayOfWeek day) => ((int)day + 6) % 7;

    private static Rule? ParseRule(string text)
    {
        if (text.Equals("24/7", StringComparison.OrdinalIgnoreCase))
        {
            return new Rule(AllMonths(), AllDays(), [(0, 24 * 60)], IsOff: false);
        }

        string[] tokens = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int index = 0;

        bool[] months = AllMonths();
        if (index < tokens.Length && TryParseSet(tokens[index], Months, out bool[] parsedMonths))
        {
            months = parsedMonths;
            index++;
        }

        bool[] days = AllDays();
        if (index < tokens.Length && TryParseSet(tokens[index], Days, out bool[] parsedDays))
        {
            days = parsedDays;
            index++;
        }

        if (index != tokens.Length - 1)
        {
            return null;
        }

        string times = tokens[index];
        if (times.Equals("off", StringComparison.OrdinalIgnoreCase)
            || times.Equals("closed", StringComparison.OrdinalIgnoreCase))
        {
            return new Rule(months, days, [], IsOff: true);
        }

        var ranges = new List<(int Start, int End)>();
        foreach (string range in times.Split(','))
        {
            string[] parts = range.Split('-');
            if (parts.Length != 2 || !TryParseTime(parts[0], out int start) || !TryParseTime(parts[1], out int end))
            {
                return null;
            }

            ranges.Add((start, end));
        }

        return new Rule(months, days, ranges, IsOff: false);
    }

    private static bool TryParseSet(string token, string[] names, out bool[] set)
    {
        set = new bool[names.Length];
        foreach (string item in token.Split(','))
        {
            string[] bounds = item.Split('-');
            int from = Array.IndexOf(names, bounds[0]);
            int to = bounds.Length == 2 ? Array.IndexOf(names, bounds[1]) : from;
            if (bounds.Length > 2 || from < 0 || to < 0)
            {
                return false;
            }

            // Wraps around, e.g. Su-Mo or Nov-Feb.
            for (int i = from; ; i = (i + 1) % names.Length)
            {
                set[i] = true;
                if (i == to)
                {
                    break;
                }
            }
        }

        return true;
    }

    private static bool TryParseTime(string text, out int minutes)
    {
        minutes = 0;
        if (!TimeSpan.TryParseExact(text, @"hh\:mm", CultureInfo.InvariantCulture, out TimeSpan time))
        {
            if (text != "24:00")
            {
                return false;
            }

            minutes = 24 * 60;
            return true;
        }

        minutes = (int)time.TotalMinutes;
        return true;
    }

    private static bool[] AllDays() => [true, true, true, true, true, true, true];

    private static bool[] AllMonths() => Enumerable.Repeat(true, 12).ToArray();

    private sealed record Rule(bool[] Months, bool[] Days, List<(int Start, int End)> Ranges, bool IsOff)
    {
        public bool Applies(int day, int month) => Days[day] && Months[month];

        public bool IsOpenOnDay(int minute) =>
            !IsOff && Ranges.Any(r => r.End > r.Start
                ? minute >= r.Start && minute < r.End
                : minute >= r.Start);

        public bool IsOpenAfterMidnight(int minute) =>
            !IsOff && Ranges.Any(r => r.End <= r.Start && minute < r.End);
    }
}
