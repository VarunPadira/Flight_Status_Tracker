// Feature: flight-status-tracker, Property 2: Normaliser maps all known statuses to valid enum values
// Feature: flight-status-tracker, Property 3: Normaliser maps unrecognised statuses to Unknown

namespace FlightStatus.Tests.Properties;

using FlightStatus.Api.Models;
using FlightStatus.Api.Services;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

public class NormaliserPropertyTests
{
    private static readonly string[] AeroTrackCancelledDiverted = ["CANCELLED", "REROUTED"];
    private static readonly string[] QuickFlightCancelledDiverted = ["cancelled", "diverted"];
    private static readonly string[] AeroTrackOnTimeDelayed = ["ON_SCHEDULE", "LATE"];
    private static readonly string[] QuickFlightOnTimeDelayed = ["scheduled", "delayed"];
    private static readonly HashSet<string> AllKnownStatuses = new(
        AeroTrackCancelledDiverted.Concat(QuickFlightCancelledDiverted)
            .Concat(AeroTrackOnTimeDelayed).Concat(QuickFlightOnTimeDelayed));

    private readonly StatusNormaliser _normaliser = new();

    // **Validates: Requirements 3.1, 3.2 — Cancelled/Diverted always map correctly**
    [Property(MaxTest = 100)]
    public Property CancelledDivertedStatuses_NeverMapToUnknown()
    {
        var arb = Gen.OneOf(
            Gen.Elements(AeroTrackCancelledDiverted).Select(s => (status: s, provider: "AeroTrack")),
            Gen.Elements(QuickFlightCancelledDiverted).Select(s => (status: s, provider: "QuickFlight"))
        ).ToArbitrary();

        return Prop.ForAll(arb, t =>
        {
            var result = _normaliser.Normalise(t.status, t.provider);
            return result == UnifiedFlightStatus.Cancelled || result == UnifiedFlightStatus.Diverted;
        });
    }

    // **Validates: Requirements 3.1, 3.2 — OnTime/Delayed statuses resolve to OnTime or Delayed (never Unknown)**
    [Property(MaxTest = 100)]
    public Property OnTimeDelayedStatuses_ResolveToOnTimeOrDelayed()
    {
        var arb = Gen.OneOf(
            Gen.Elements(AeroTrackOnTimeDelayed).Select(s => (status: s, provider: "AeroTrack")),
            Gen.Elements(QuickFlightOnTimeDelayed).Select(s => (status: s, provider: "QuickFlight"))
        ).ToArbitrary();

        return Prop.ForAll(arb, t =>
        {
            var result = _normaliser.Normalise(t.status, t.provider);
            return result == UnifiedFlightStatus.OnTime || result == UnifiedFlightStatus.Delayed;
        });
    }

    // **Validates: Property 2 — 15-minute rule: flights with actual times within 15 min are OnTime**
    [Property(MaxTest = 100)]
    public Property ActualTimesWithin15Minutes_AlwaysOnTime()
    {
        var arb = (from delayMinutes in Gen.Choose(0, 15)
                   from status in Gen.Elements("ON_SCHEDULE", "LATE")
                   select (status, delayMinutes))
                  .ToArbitrary();

        return Prop.ForAll(arb, t =>
        {
            var scheduled = new DateTime(2024, 3, 15, 8, 0, 0, DateTimeKind.Utc);
            var data = new ProviderFlightData(
                RawStatus: t.status,
                ScheduledDeparture: scheduled,
                ScheduledArrival: scheduled.AddHours(3),
                ActualDeparture: scheduled.AddMinutes(t.delayMinutes),
                ActualArrival: scheduled.AddHours(3).AddMinutes(t.delayMinutes),
                Terminal: null, Gate: null, DelayReason: null,
                LastUpdatedUtc: DateTime.UtcNow);

            var result = _normaliser.Normalise(t.status, "AeroTrack", data);
            return result == UnifiedFlightStatus.OnTime;
        });
    }

    // **Validates: Property 2 — 15-minute rule: flights with actual times beyond 15 min are Delayed**
    [Property(MaxTest = 100)]
    public Property ActualTimesBeyond15Minutes_AlwaysDelayed()
    {
        var arb = (from delayMinutes in Gen.Choose(16, 180)
                   from status in Gen.Elements("ON_SCHEDULE", "LATE")
                   select (status, delayMinutes))
                  .ToArbitrary();

        return Prop.ForAll(arb, t =>
        {
            var scheduled = new DateTime(2024, 3, 15, 8, 0, 0, DateTimeKind.Utc);
            var data = new ProviderFlightData(
                RawStatus: t.status,
                ScheduledDeparture: scheduled,
                ScheduledArrival: scheduled.AddHours(3),
                ActualDeparture: scheduled.AddMinutes(t.delayMinutes),
                ActualArrival: scheduled.AddHours(3),
                Terminal: null, Gate: null, DelayReason: null,
                LastUpdatedUtc: DateTime.UtcNow);

            var result = _normaliser.Normalise(t.status, "AeroTrack", data);
            return result == UnifiedFlightStatus.Delayed;
        });
    }

    // **Validates: Requirements 3.3 — Unrecognised statuses always map to Unknown**
    [Property(MaxTest = 100)]
    public bool UnrecognisedStatuses_AlwaysMapToUnknown(string rawStatus)
    {
        if (rawStatus is null || AllKnownStatuses.Contains(rawStatus))
            return true; // skip known statuses

        var result = _normaliser.Normalise(rawStatus, "AeroTrack");
        return result == UnifiedFlightStatus.Unknown;
    }
}
