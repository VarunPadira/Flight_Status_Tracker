namespace FlightStatus.Api.Services;

using FlightStatus.Api.Models;

public interface IStatusNormaliser
{
    UnifiedFlightStatus Normalise(string rawStatus, string providerName, ProviderFlightData? flightData = null);
}
