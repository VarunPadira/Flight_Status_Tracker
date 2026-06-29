// Feature: flight-status-tracker, Property 6: Result model structural completeness

namespace FlightStatus.Tests.Properties;

using FlightAssertions = FluentAssertions.AssertionExtensions;
using FlightStatus.Api.Models;
using FlightStatus.Api.Services;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

public class ResultModelPropertyTests
{
    private readonly FlightStatusMerger _merger = new();

    private static Gen<DateTime> GenDateTime()
    {
        return from year in Gen.Choose(2020, 2025)
               from month in Gen.Choose(1, 12)
               from day in Gen.Choose(1, 28)
               from hour in Gen.Choose(0, 23)
               from minute in Gen.Choose(0, 59)
               select new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc);
    }

    private static Gen<bool> GenBool()
    {
        return Gen.Elements(true, false);
    }

    private static Gen<ProviderFlightData> GenProviderFlightData()
    {
        return from rawStatus in Gen.Elements("ON_SCHEDULE", "LATE", "CANCELLED", "REROUTED", "scheduled", "delayed", "cancelled", "diverted")
               from scheduledDep in GenDateTime()
               from scheduledArr in GenDateTime()
               from hasActualDep in GenBool()
               from actualDep in GenDateTime()
               from hasActualArr in GenBool()
               from actualArr in GenDateTime()
               from hasTerminal in GenBool()
               from terminal in Gen.Elements("1", "2", "3", "5", "N")
               from hasGate in GenBool()
               from gate in Gen.Elements("A1", "B12", "C7", "D22")
               from hasDelay in GenBool()
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

    private static Gen<UnifiedFlightStatus> GenStatus()
    {
        return Gen.Elements(
            UnifiedFlightStatus.OnTime,
            UnifiedFlightStatus.Delayed,
            UnifiedFlightStatus.Cancelled,
            UnifiedFlightStatus.Diverted,
            UnifiedFlightStatus.Unknown);
    }

    private static Gen<string> GenFlightNumber()
    {
        return from prefix in Gen.Elements("BA", "LH", "AF", "AA", "UA", "DL", "EK")
               from number in Gen.Choose(100, 9999)
               select $"{prefix}{number}";
    }

    private static Gen<DateOnly> GenDate()
    {
        return from year in Gen.Choose(2020, 2025)
               from month in Gen.Choose(1, 12)
               from day in Gen.Choose(1, 28)
               select new DateOnly(year, month, day);
    }

    // **Validates: Requirements 5.1, 5.3, 5.4**
    [Property(MaxTest = 100)]
    public Property ResultFromBothProviders_HasRequiredFields()
    {
        var arb = (from data1 in GenProviderFlightData()
                   from data2 in GenProviderFlightData()
                   from status1 in GenStatus()
                   from status2 in GenStatus()
                   from flightNumber in GenFlightNumber()
                   from date in GenDate()
                   select (data1, data2, status1, status2, flightNumber, date))
                  .ToArbitrary();

        return Prop.ForAll(arb, t =>
        {
            var (data1, data2, status1, status2, flightNumber, date) = t;

            var aeroResult = ((ProviderFlightData?)data1, status1, "AeroTrack");
            var quickResult = ((ProviderFlightData?)data2, status2, "QuickFlight");

            var result = _merger.Merge(flightNumber, date, aeroResult, quickResult);

            FlightAssertions.Should(result.FlightNumber).NotBeNullOrEmpty();
            FlightAssertions.Should(result.Date).NotBe(default);
            FlightAssertions.Should(result.ScheduledDeparture).NotBe(default);
            FlightAssertions.Should(result.ScheduledArrival).NotBe(default);
            FlightAssertions.Should(result.LastUpdatedUtc).NotBe(default);
            FlightAssertions.Should(result.Provider).NotBeNullOrEmpty();
        });
    }

    // **Validates: Requirements 5.1, 5.3, 5.4**
    [Property(MaxTest = 100)]
    public Property ResultFromSingleProvider_HasRequiredFields()
    {
        var arb = (from data in GenProviderFlightData()
                   from status in GenStatus()
                   from flightNumber in GenFlightNumber()
                   from date in GenDate()
                   from useAero in GenBool()
                   select (data, status, flightNumber, date, useAero))
                  .ToArbitrary();

        return Prop.ForAll(arb, t =>
        {
            var (data, status, flightNumber, date, useAero) = t;

            var providerName = useAero ? "AeroTrack" : "QuickFlight";
            var providerResult = ((ProviderFlightData?)data, status, providerName);

            var result = useAero
                ? _merger.Merge(flightNumber, date, providerResult, null)
                : _merger.Merge(flightNumber, date, null, providerResult);

            FlightAssertions.Should(result.FlightNumber).NotBeNullOrEmpty();
            FlightAssertions.Should(result.Date).NotBe(default);
            FlightAssertions.Should(result.ScheduledDeparture).NotBe(default);
            FlightAssertions.Should(result.ScheduledArrival).NotBe(default);
            FlightAssertions.Should(result.LastUpdatedUtc).NotBe(default);
            FlightAssertions.Should(result.Provider).NotBeNullOrEmpty();
        });
    }

    // **Validates: Requirements 5.1, 5.3, 5.4**
    [Property(MaxTest = 100)]
    public Property ResultFromNoProviders_HasRequiredFields()
    {
        var arb = (from flightNumber in GenFlightNumber()
                   from date in GenDate()
                   select (flightNumber, date))
                  .ToArbitrary();

        return Prop.ForAll(arb, t =>
        {
            var (flightNumber, date) = t;

            var result = _merger.Merge(flightNumber, date, null, null);

            FlightAssertions.Should(result.FlightNumber).NotBeNullOrEmpty();
            FlightAssertions.Should(result.Date).NotBe(default);
            FlightAssertions.Should(result.Provider).NotBeNullOrEmpty();
            // When no providers return data, status is Unknown and provider is "None"
            FlightAssertions.Should(result.Status).Be(UnifiedFlightStatus.Unknown);
            FlightAssertions.Should(result.Provider).Be("None");
        });
    }
}
