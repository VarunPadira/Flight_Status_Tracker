// Feature: flight-status-tracker, Property 4: Merger selects the result with the later timestamp
// Feature: flight-status-tracker, Property 5: Merger preserves single-source data unchanged

namespace FlightStatus.Tests.Properties;

using FlightAssertions = FluentAssertions.AssertionExtensions;
using FlightStatus.Api.Models;
using FlightStatus.Api.Services;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

public class MergerPropertyTests
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
        return from rawStatus in Gen.Elements("ON_SCHEDULE", "LATE", "CANCELLED", "REROUTED", "scheduled", "delayed")
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

    // **Validates: Requirements 4.1**
    [Property(MaxTest = 100)]
    public Property MergerSelectsResultWithLaterTimestamp()
    {
        var arb = (from data1 in GenProviderFlightData()
                   from data2 in GenProviderFlightData()
                   from status1 in GenStatus()
                   from status2 in GenStatus()
                   from flightNumber in Gen.Elements("BA123", "LH456", "AF789")
                   let offset = TimeSpan.FromMinutes(1)
                   // Ensure distinct timestamps by adding offset to data2
                   let adjustedData2 = data2 with { LastUpdatedUtc = data1.LastUpdatedUtc + offset }
                   select (data1, adjustedData2, status1, status2, flightNumber))
                  .ToArbitrary();

        return Prop.ForAll(arb, t =>
        {
            var (data1, data2, status1, status2, flightNumber) = t;
            var date = DateOnly.FromDateTime(DateTime.Today);

            var aeroResult = ((ProviderFlightData?)data1, status1, "AeroTrack");
            var quickResult = ((ProviderFlightData?)data2, status2, "QuickFlight");

            var result = _merger.Merge(flightNumber, date, aeroResult, quickResult);

            // data2 always has the later timestamp (data1 + offset)
            var expectedLater = data2;
            var expectedProvider = "QuickFlight";

            FlightAssertions.Should(result.LastUpdatedUtc).Be(expectedLater.LastUpdatedUtc);
            FlightAssertions.Should(result.Provider).Be(expectedProvider);
        });
    }

    // **Validates: Requirements 4.1**
    [Property(MaxTest = 100)]
    public Property MergerSelectsResultWithLaterTimestamp_Reversed()
    {
        var arb = (from data1 in GenProviderFlightData()
                   from data2 in GenProviderFlightData()
                   from status1 in GenStatus()
                   from status2 in GenStatus()
                   from flightNumber in Gen.Elements("BA123", "LH456", "AF789")
                   let offset = TimeSpan.FromMinutes(1)
                   // Make aero the later one
                   let adjustedData1 = data1 with { LastUpdatedUtc = data2.LastUpdatedUtc + offset }
                   select (adjustedData1, data2, status1, status2, flightNumber))
                  .ToArbitrary();

        return Prop.ForAll(arb, t =>
        {
            var (data1, data2, status1, status2, flightNumber) = t;
            var date = DateOnly.FromDateTime(DateTime.Today);

            var aeroResult = ((ProviderFlightData?)data1, status1, "AeroTrack");
            var quickResult = ((ProviderFlightData?)data2, status2, "QuickFlight");

            var result = _merger.Merge(flightNumber, date, aeroResult, quickResult);

            // data1 (aero) has the later timestamp
            FlightAssertions.Should(result.LastUpdatedUtc).Be(data1.LastUpdatedUtc);
            FlightAssertions.Should(result.Provider).Be("AeroTrack");
        });
    }

    // **Validates: Requirements 4.2, 4.4, 4.5**
    [Property(MaxTest = 100)]
    public Property MergerPreservesSingleSourceDataUnchanged_AeroOnly()
    {
        var arb = (from data in GenProviderFlightData()
                   from status in GenStatus()
                   from flightNumber in Gen.Elements("BA123", "LH456", "AF789")
                   select (data, status, flightNumber))
                  .ToArbitrary();

        return Prop.ForAll(arb, t =>
        {
            var (data, status, flightNumber) = t;
            var date = DateOnly.FromDateTime(DateTime.Today);

            var aeroResult = ((ProviderFlightData?)data, status, "AeroTrack");

            var result = _merger.Merge(flightNumber, date, aeroResult, null);

            FlightAssertions.Should(result.ScheduledDeparture).Be(data.ScheduledDeparture);
            FlightAssertions.Should(result.ScheduledArrival).Be(data.ScheduledArrival);
            FlightAssertions.Should(result.ActualDeparture).Be(data.ActualDeparture);
            FlightAssertions.Should(result.ActualArrival).Be(data.ActualArrival);
            FlightAssertions.Should(result.Terminal).Be(data.Terminal);
            FlightAssertions.Should(result.Gate).Be(data.Gate);
            FlightAssertions.Should(result.DelayReason).Be(data.DelayReason);
            FlightAssertions.Should(result.LastUpdatedUtc).Be(data.LastUpdatedUtc);
            FlightAssertions.Should(result.Provider).Be("AeroTrack");
        });
    }

    // **Validates: Requirements 4.2, 4.4, 4.5**
    [Property(MaxTest = 100)]
    public Property MergerPreservesSingleSourceDataUnchanged_QuickOnly()
    {
        var arb = (from data in GenProviderFlightData()
                   from status in GenStatus()
                   from flightNumber in Gen.Elements("BA123", "LH456", "AF789")
                   select (data, status, flightNumber))
                  .ToArbitrary();

        return Prop.ForAll(arb, t =>
        {
            var (data, status, flightNumber) = t;
            var date = DateOnly.FromDateTime(DateTime.Today);

            var quickResult = ((ProviderFlightData?)data, status, "QuickFlight");

            var result = _merger.Merge(flightNumber, date, null, quickResult);

            FlightAssertions.Should(result.ScheduledDeparture).Be(data.ScheduledDeparture);
            FlightAssertions.Should(result.ScheduledArrival).Be(data.ScheduledArrival);
            FlightAssertions.Should(result.ActualDeparture).Be(data.ActualDeparture);
            FlightAssertions.Should(result.ActualArrival).Be(data.ActualArrival);
            FlightAssertions.Should(result.Terminal).Be(data.Terminal);
            FlightAssertions.Should(result.Gate).Be(data.Gate);
            FlightAssertions.Should(result.DelayReason).Be(data.DelayReason);
            FlightAssertions.Should(result.LastUpdatedUtc).Be(data.LastUpdatedUtc);
            FlightAssertions.Should(result.Provider).Be("QuickFlight");
        });
    }
}
