namespace FlightStatus.Api.Models;

public record FlightStatusResult
{
    public string FlightNumber { get; init; } = string.Empty;
    public DateOnly Date { get; init; }
    public UnifiedFlightStatus Status { get; init; }
    public DateTime ScheduledDeparture { get; init; }
    public DateTime ScheduledArrival { get; init; }
    public DateTime? ActualDeparture { get; init; }
    public DateTime? ActualArrival { get; init; }
    public string? Terminal { get; init; }
    public string? Gate { get; init; }
    public string? DelayReason { get; init; }
    public DateTime LastUpdatedUtc { get; init; }
    public string Provider { get; init; } = string.Empty;
    public string? Message { get; init; }
}
