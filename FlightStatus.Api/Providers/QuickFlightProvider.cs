namespace FlightStatus.Api.Providers;

using FlightStatus.Api.Models;

public class QuickFlightProvider : IFlightStatusProvider
{
    public string ProviderName => "QuickFlight";

    private static readonly Dictionary<(string FlightNumber, DateOnly Date), ProviderFlightData> _flights = new()
    {
        {
            ("BA123", new DateOnly(2024, 3, 15)),
            new ProviderFlightData(
                RawStatus: "scheduled",
                ScheduledDeparture: new DateTime(2024, 3, 15, 8, 0, 0, DateTimeKind.Utc),
                ScheduledArrival: new DateTime(2024, 3, 15, 11, 30, 0, DateTimeKind.Utc),
                ActualDeparture: null,
                ActualArrival: null,
                Terminal: null,
                Gate: null,
                DelayReason: null,
                LastUpdatedUtc: new DateTime(2024, 3, 15, 10, 0, 0, DateTimeKind.Utc)
            )
        },
        {
            ("LH456", new DateOnly(2024, 3, 15)),
            new ProviderFlightData(
                RawStatus: "delayed",
                ScheduledDeparture: new DateTime(2024, 3, 15, 14, 0, 0, DateTimeKind.Utc),
                ScheduledArrival: new DateTime(2024, 3, 15, 17, 0, 0, DateTimeKind.Utc),
                ActualDeparture: null,
                ActualArrival: null,
                Terminal: null,
                Gate: null,
                DelayReason: null,
                LastUpdatedUtc: new DateTime(2024, 3, 15, 14, 0, 0, DateTimeKind.Utc)
            )
        },
        {
            ("AF789", new DateOnly(2024, 3, 15)),
            new ProviderFlightData(
                RawStatus: "cancelled",
                ScheduledDeparture: new DateTime(2024, 3, 15, 9, 30, 0, DateTimeKind.Utc),
                ScheduledArrival: new DateTime(2024, 3, 15, 12, 45, 0, DateTimeKind.Utc),
                ActualDeparture: null,
                ActualArrival: null,
                Terminal: null,
                Gate: null,
                DelayReason: null,
                LastUpdatedUtc: new DateTime(2024, 3, 15, 8, 30, 0, DateTimeKind.Utc)
            )
        },
        {
            ("EK101", new DateOnly(2024, 3, 15)),
            new ProviderFlightData(
                RawStatus: "diverted",
                ScheduledDeparture: new DateTime(2024, 3, 15, 22, 0, 0, DateTimeKind.Utc),
                ScheduledArrival: new DateTime(2024, 3, 16, 6, 30, 0, DateTimeKind.Utc),
                ActualDeparture: null,
                ActualArrival: null,
                Terminal: null,
                Gate: null,
                DelayReason: null,
                LastUpdatedUtc: new DateTime(2024, 3, 16, 6, 0, 0, DateTimeKind.Utc)
            )
        }
    };

    public Task<ProviderFlightData?> GetFlightStatusAsync(string flightNumber, DateOnly date)
    {
        _flights.TryGetValue((flightNumber, date), out var result);
        return Task.FromResult(result);
    }
}
