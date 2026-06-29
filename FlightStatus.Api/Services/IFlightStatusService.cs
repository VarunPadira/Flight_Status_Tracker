namespace FlightStatus.Api.Services;

using FlightStatus.Api.Models;

public interface IFlightStatusService
{
    Task<FlightStatusResult> GetFlightStatusAsync(string flightNumber, DateOnly date);
}
