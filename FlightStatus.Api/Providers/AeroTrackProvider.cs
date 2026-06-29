namespace FlightStatus.Api.Providers;

using FlightStatus.Api.Models;

public class AeroTrackProvider : IFlightStatusProvider
{
    public string ProviderName => "AeroTrack";

    private static readonly Dictionary<(string FlightNumber, DateOnly Date), ProviderFlightData> _flights = new()
    {
        {
            ("BA123", new DateOnly(2024, 3, 15)),
            new ProviderFlightData(
                RawStatus: "ON_SCHEDULE",
                ScheduledDeparture: new DateTime(2024, 3, 15, 8, 0, 0, DateTimeKind.Utc),
                ScheduledArrival: new DateTime(2024, 3, 15, 11, 30, 0, DateTimeKind.Utc),
                ActualDeparture: new DateTime(2024, 3, 15, 8, 5, 0, DateTimeKind.Utc),
                ActualArrival: new DateTime(2024, 3, 15, 11, 28, 0, DateTimeKind.Utc),
                Terminal: "5",
                Gate: "B42",
                DelayReason: null,
                LastUpdatedUtc: new DateTime(2024, 3, 15, 11, 30, 0, DateTimeKind.Utc)
            )
        },
        {
            ("LH456", new DateOnly(2024, 3, 15)),
            new ProviderFlightData(
                RawStatus: "LATE",
                ScheduledDeparture: new DateTime(2024, 3, 15, 14, 0, 0, DateTimeKind.Utc),
                ScheduledArrival: new DateTime(2024, 3, 15, 17, 0, 0, DateTimeKind.Utc),
                ActualDeparture: new DateTime(2024, 3, 15, 15, 20, 0, DateTimeKind.Utc),
                ActualArrival: new DateTime(2024, 3, 15, 18, 15, 0, DateTimeKind.Utc),
                Terminal: "2",
                Gate: "A14",
                DelayReason: "Air traffic control restrictions",
                LastUpdatedUtc: new DateTime(2024, 3, 15, 13, 30, 0, DateTimeKind.Utc)
            )
        },
        {
            ("AF789", new DateOnly(2024, 3, 15)),
            new ProviderFlightData(
                RawStatus: "CANCELLED",
                ScheduledDeparture: new DateTime(2024, 3, 15, 9, 30, 0, DateTimeKind.Utc),
                ScheduledArrival: new DateTime(2024, 3, 15, 12, 45, 0, DateTimeKind.Utc),
                ActualDeparture: null,
                ActualArrival: null,
                Terminal: "1",
                Gate: "C7",
                DelayReason: "Severe weather conditions",
                LastUpdatedUtc: new DateTime(2024, 3, 15, 7, 0, 0, DateTimeKind.Utc)
            )
        },
        {
            ("EK101", new DateOnly(2024, 3, 15)),
            new ProviderFlightData(
                RawStatus: "REROUTED",
                ScheduledDeparture: new DateTime(2024, 3, 15, 22, 0, 0, DateTimeKind.Utc),
                ScheduledArrival: new DateTime(2024, 3, 16, 6, 30, 0, DateTimeKind.Utc),
                ActualDeparture: new DateTime(2024, 3, 15, 22, 10, 0, DateTimeKind.Utc),
                ActualArrival: new DateTime(2024, 3, 16, 7, 0, 0, DateTimeKind.Utc),
                Terminal: "3",
                Gate: "D22",
                DelayReason: "Destination airport closed; diverted to alternate",
                LastUpdatedUtc: new DateTime(2024, 3, 16, 7, 15, 0, DateTimeKind.Utc)
            )
        }
    };

    public Task<ProviderFlightData?> GetFlightStatusAsync(string flightNumber, DateOnly date)
    {
        _flights.TryGetValue((flightNumber, date), out var result);
        return Task.FromResult(result);
    }
}
