namespace FlightStatus.Api.Services;

using FlightStatus.Api.Models;
using FlightStatus.Api.Providers;

public class FlightStatusService : IFlightStatusService
{
    private readonly IEnumerable<IFlightStatusProvider> _providers;
    private readonly IStatusNormaliser _normaliser;
    private readonly IFlightStatusMerger _merger;

    public FlightStatusService(
        IEnumerable<IFlightStatusProvider> providers,
        IStatusNormaliser normaliser,
        IFlightStatusMerger merger)
    {
        _providers = providers;
        _normaliser = normaliser;
        _merger = merger;
    }

    public async Task<FlightStatusResult> GetFlightStatusAsync(string flightNumber, DateOnly date)
    {
        var aeroProvider = _providers.FirstOrDefault(p =>
            string.Equals(p.ProviderName, "AeroTrack", StringComparison.OrdinalIgnoreCase));
        var quickProvider = _providers.FirstOrDefault(p =>
            string.Equals(p.ProviderName, "QuickFlight", StringComparison.OrdinalIgnoreCase));

        var aeroTask = QueryProviderSafeAsync(aeroProvider, flightNumber, date);
        var quickTask = QueryProviderSafeAsync(quickProvider, flightNumber, date);

        await Task.WhenAll(aeroTask, quickTask);

        var aeroData = await aeroTask;
        var quickData = await quickTask;

        var aeroResult = BuildProviderTuple(aeroData, "AeroTrack");
        var quickResult = BuildProviderTuple(quickData, "QuickFlight");

        return _merger.Merge(flightNumber, date, aeroResult, quickResult);
    }

    private static async Task<ProviderFlightData?> QueryProviderSafeAsync(
        IFlightStatusProvider? provider,
        string flightNumber,
        DateOnly date)
    {
        if (provider is null)
            return null;

        try
        {
            return await provider.GetFlightStatusAsync(flightNumber, date);
        }
        catch
        {
            return null;
        }
    }

    private (ProviderFlightData? Data, UnifiedFlightStatus Status, string ProviderName)? BuildProviderTuple(
        ProviderFlightData? data,
        string providerName)
    {
        if (data is null)
            return null;

        var status = _normaliser.Normalise(data.RawStatus, providerName, data);
        return (data, status, providerName);
    }
}
