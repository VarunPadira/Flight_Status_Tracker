// Feature: flight-status-tracker — Reusable FsCheck generators for property-based tests

namespace FlightStatus.Tests.Generators;

using FlightStatus.Api.Models;
using FsCheck;
using FsCheck.Fluent;

/// <summary>
/// Static utility class providing reusable FsCheck 3.x generators
/// for flight data property-based tests.
/// </summary>
public static class FlightDataGenerators
{
    private static readonly string[] AeroTrackStatuses = ["ON_SCHEDULE", "LATE", "CANCELLED", "REROUTED"];
    private static readonly string[] QuickFlightStatuses = ["scheduled", "delayed", "cancelled", "diverted"];

    /// <summary>
    /// Generates a UTC DateTime within a reasonable range (2020–2025).
    /// </summary>
    public static Gen<DateTime> GenDateTime()
    {
        return from year in Gen.Choose(2020, 2025)
               from month in Gen.Choose(1, 12)
               from day in Gen.Choose(1, 28)
               from hour in Gen.Choose(0, 23)
               from minute in Gen.Choose(0, 59)
               select new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc);
    }

    /// <summary>
    /// Generates a random ProviderFlightData with valid structure.
    /// Optional fields (ActualDeparture, ActualArrival, Terminal, Gate, DelayReason)
    /// are randomly present or null.
    /// </summary>
    public static Gen<ProviderFlightData> GenProviderFlightData()
    {
        return from rawStatus in Gen.Elements(
                   "ON_SCHEDULE", "LATE", "CANCELLED", "REROUTED",
                   "scheduled", "delayed", "cancelled", "diverted")
               from scheduledDep in GenDateTime()
               from scheduledArr in GenDateTime()
               from hasActualDep in Gen.Elements(true, false)
               from actualDep in GenDateTime()
               from hasActualArr in Gen.Elements(true, false)
               from actualArr in GenDateTime()
               from hasTerminal in Gen.Elements(true, false)
               from terminal in Gen.Elements("1", "2", "3", "5", "N")
               from hasGate in Gen.Elements(true, false)
               from gate in Gen.Elements("A1", "B12", "C7", "D22")
               from hasDelay in Gen.Elements(true, false)
               from delayReason in Gen.Elements("Weather", "Technical", "Crew", "ATC")
               from lastUpdated in GenDateTime()
               select new ProviderFlightData(
                   RawStatus: rawStatus,
                   ScheduledDeparture: scheduledDep,
                   ScheduledArrival: scheduledArr,
                   ActualDeparture: hasActualDep ? actualDep : null,
                   ActualArrival: hasActualArr ? actualArr : null,
                   Terminal: hasTerminal ? terminal : null,
                   Gate: hasGate ? gate : null,
                   DelayReason: hasDelay ? delayReason : null,
                   LastUpdatedUtc: lastUpdated
               );
    }

    /// <summary>
    /// Generates a known valid status string from either AeroTrack or QuickFlight vocabularies.
    /// </summary>
    public static Gen<string> GenKnownStatus()
    {
        return Gen.Elements(
            AeroTrackStatuses.Concat(QuickFlightStatuses).ToArray());
    }

    /// <summary>
    /// Generates strings that are NOT in any known provider vocabulary.
    /// Useful for testing the Unknown mapping path.
    /// </summary>
    public static Gen<string> GenUnknownStatus()
    {
        var allKnown = new HashSet<string>(AeroTrackStatuses.Concat(QuickFlightStatuses));

        return Gen.OneOf(
            // Random words that aren't statuses
            Gen.Elements(
                "BOARDING", "LANDED", "IN_FLIGHT", "TAXIING", "GATE_CLOSED",
                "departed", "arrived", "en_route", "holding", "grounded",
                "unknown", "UNKNOWN", "active", "ACTIVE", "PENDING"),
            // Random short strings
            Gen.Choose(3, 15).SelectMany(len =>
                Gen.ArrayOf(Gen.Elements(
                    'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j',
                    'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't'), len)
                .Select(chars => new string(chars)))
        ).Where(s => !allKnown.Contains(s));
    }

    /// <summary>
    /// Generates valid date strings in yyyy-MM-dd format.
    /// </summary>
    public static Gen<string> GenValidDateString()
    {
        return from year in Gen.Choose(2020, 2025)
               from month in Gen.Choose(1, 12)
               from day in Gen.Choose(1, 28)
               select $"{year:D4}-{month:D2}-{day:D2}";
    }

    /// <summary>
    /// Generates strings that are NOT valid yyyy-MM-dd dates.
    /// Covers various invalid patterns: wrong separators, reversed formats,
    /// out-of-range values, and non-date strings.
    /// </summary>
    public static Gen<string> GenInvalidDateString()
    {
        return Gen.OneOf(
            // Wrong separators (slashes)
            from year in Gen.Choose(2020, 2025)
            from month in Gen.Choose(1, 12)
            from day in Gen.Choose(1, 28)
            select $"{year}/{month:D2}/{day:D2}",
            // dd-MM-yyyy (reversed format)
            from day in Gen.Choose(1, 28)
            from month in Gen.Choose(1, 12)
            from year in Gen.Choose(2020, 2025)
            select $"{day:D2}-{month:D2}-{year}",
            // MM/dd/yyyy (US format)
            from month in Gen.Choose(1, 12)
            from day in Gen.Choose(1, 28)
            from year in Gen.Choose(2020, 2025)
            select $"{month:D2}/{day:D2}/{year}",
            // Partial dates
            from year in Gen.Choose(2020, 2025)
            from month in Gen.Choose(1, 12)
            select $"{year}-{month:D2}",
            // Invalid month values
            Gen.Choose(2020, 2025).Select(y => $"{y}-13-01"),
            Gen.Choose(2020, 2025).Select(y => $"{y}-00-15"),
            // Invalid day values
            Gen.Choose(2020, 2025).Select(y => $"{y}-02-30"),
            Gen.Choose(2020, 2025).Select(y => $"{y}-04-31"),
            // Non-date strings
            Gen.Elements(
                "", " ", "not-a-date", "abc", "2024-3-15",
                "2024-03-5", "24-03-15", "abcd-ef-gh",
                "yesterday", "today", "2024")
        ).Where(s => !IsValidYyyyMmDd(s));
    }

    /// <summary>
    /// Generates realistic flight numbers (airline prefix + numeric).
    /// </summary>
    public static Gen<string> GenFlightNumber()
    {
        return from prefix in Gen.Elements("BA", "LH", "AF", "AA", "UA", "DL", "EK", "QF", "SQ", "EY")
               from number in Gen.Choose(100, 9999)
               select $"{prefix}{number}";
    }

    /// <summary>
    /// Creates an Arbitrary&lt;ProviderFlightData&gt; for auto-use with [Property] attribute.
    /// </summary>
    public static Arbitrary<ProviderFlightData> ArbitraryProviderFlightData()
    {
        return GenProviderFlightData().ToArbitrary();
    }

    private static bool IsValidYyyyMmDd(string value)
    {
        return DateOnly.TryParseExact(
            value,
            "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out _);
    }
}
