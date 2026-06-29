namespace FlightStatus.Api.Providers;

using FlightStatus.Api.Models;

public interface IFlightStatusProvider
{
    string ProviderName { get; }
    Task<ProviderFlightData?> GetFlightStatusAsync(string flightNumber, DateOnly date);
}
