namespace FlightStatus.Api.Models;

public record ProviderFlightData(
    string RawStatus,
    DateTime ScheduledDeparture,
    DateTime ScheduledArrival,
    DateTime? ActualDeparture,
    DateTime? ActualArrival,
    string? Terminal,
    string? Gate,
    string? DelayReason,
    DateTime LastUpdatedUtc
);
