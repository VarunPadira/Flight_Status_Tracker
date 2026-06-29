namespace FlightStatus.Api.Services;

using FlightStatus.Api.Models;

public interface IFlightStatusMerger
{
    FlightStatusResult Merge(
        string flightNumber,
        DateOnly date,
        (ProviderFlightData? Data, UnifiedFlightStatus Status, string ProviderName)? aeroResult,
        (ProviderFlightData? Data, UnifiedFlightStatus Status, string ProviderName)? quickResult
    );
}
