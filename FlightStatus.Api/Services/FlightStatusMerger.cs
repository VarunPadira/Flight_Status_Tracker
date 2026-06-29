namespace FlightStatus.Api.Services;

using FlightStatus.Api.Models;

public class FlightStatusMerger : IFlightStatusMerger
{
    public FlightStatusResult Merge(
        string flightNumber,
        DateOnly date,
        (ProviderFlightData? Data, UnifiedFlightStatus Status, string ProviderName)? aeroResult,
        (ProviderFlightData? Data, UnifiedFlightStatus Status, string ProviderName)? quickResult)
    {
        var aeroData = aeroResult?.Data;
        var quickData = quickResult?.Data;

        // When both have data: select by later lastUpdatedUtc
        if (aeroData is not null && quickData is not null)
        {
            var selected = aeroData.LastUpdatedUtc >= quickData.LastUpdatedUtc
                ? aeroResult!.Value
                : quickResult!.Value;

            return BuildResult(flightNumber, date, selected);
        }

        // When only one has data: use it directly
        if (aeroData is not null)
        {
            return BuildResult(flightNumber, date, aeroResult!.Value);
        }

        if (quickData is not null)
        {
            return BuildResult(flightNumber, date, quickResult!.Value);
        }

        // When neither has data: return Unknown status with message
        return new FlightStatusResult
        {
            FlightNumber = flightNumber,
            Date = date,
            Status = UnifiedFlightStatus.Unknown,
            Provider = "None",
            Message = "No flight data available from any provider."
        };
    }

    private static FlightStatusResult BuildResult(
        string flightNumber,
        DateOnly date,
        (ProviderFlightData? Data, UnifiedFlightStatus Status, string ProviderName) selected)
    {
        var data = selected.Data!;

        return new FlightStatusResult
        {
            FlightNumber = flightNumber,
            Date = date,
            Status = selected.Status,
            ScheduledDeparture = data.ScheduledDeparture,
            ScheduledArrival = data.ScheduledArrival,
            ActualDeparture = data.ActualDeparture,
            ActualArrival = data.ActualArrival,
            Terminal = data.Terminal,
            Gate = data.Gate,
            DelayReason = data.DelayReason,
            LastUpdatedUtc = data.LastUpdatedUtc,
            Provider = selected.ProviderName
        };
    }
}
