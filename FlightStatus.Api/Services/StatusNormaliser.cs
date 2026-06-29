namespace FlightStatus.Api.Services;

using FlightStatus.Api.Models;

public class StatusNormaliser : IStatusNormaliser
{
    private static readonly TimeSpan DelayThreshold = TimeSpan.FromMinutes(15);

    // AeroTrack vocabulary → semantic category
    private static readonly Dictionary<string, StatusCategory> AeroTrackMappings = new()
    {
        ["ON_SCHEDULE"] = StatusCategory.OnTimeOrDelayed,
        ["LATE"] = StatusCategory.OnTimeOrDelayed,
        ["CANCELLED"] = StatusCategory.Cancelled,
        ["REROUTED"] = StatusCategory.Diverted
    };

    // QuickFlight vocabulary → semantic category
    private static readonly Dictionary<string, StatusCategory> QuickFlightMappings = new()
    {
        ["scheduled"] = StatusCategory.OnTimeOrDelayed,
        ["delayed"] = StatusCategory.OnTimeOrDelayed,
        ["cancelled"] = StatusCategory.Cancelled,
        ["diverted"] = StatusCategory.Diverted
    };

    public UnifiedFlightStatus Normalise(string rawStatus, string providerName, ProviderFlightData? flightData = null)
    {
        var category = GetCategory(rawStatus, providerName);

        return category switch
        {
            StatusCategory.Cancelled => UnifiedFlightStatus.Cancelled,
            StatusCategory.Diverted => UnifiedFlightStatus.Diverted,
            StatusCategory.OnTimeOrDelayed => DetermineByTime(flightData),
            _ => UnifiedFlightStatus.Unknown
        };
    }

    private static StatusCategory GetCategory(string rawStatus, string providerName)
    {
        if (string.Equals(providerName, "AeroTrack", StringComparison.OrdinalIgnoreCase))
        {
            return AeroTrackMappings.TryGetValue(rawStatus, out var cat) ? cat : StatusCategory.Unknown;
        }

        if (string.Equals(providerName, "QuickFlight", StringComparison.OrdinalIgnoreCase))
        {
            return QuickFlightMappings.TryGetValue(rawStatus, out var cat) ? cat : StatusCategory.Unknown;
        }

        return StatusCategory.Unknown;
    }

    /// <summary>
    /// Determines OnTime vs Delayed using the 15-minute rule:
    /// - If actual times are available, compare actual vs scheduled
    /// - If no actual times, fall back to provider's raw status hint
    /// </summary>
    private static UnifiedFlightStatus DetermineByTime(ProviderFlightData? data)
    {
        if (data is null)
            return UnifiedFlightStatus.OnTime;

        // Check departure delay
        if (data.ActualDeparture.HasValue)
        {
            var departureDelay = data.ActualDeparture.Value - data.ScheduledDeparture;
            if (departureDelay > DelayThreshold)
                return UnifiedFlightStatus.Delayed;
        }

        // Check arrival delay
        if (data.ActualArrival.HasValue)
        {
            var arrivalDelay = data.ActualArrival.Value - data.ScheduledArrival;
            if (arrivalDelay > DelayThreshold)
                return UnifiedFlightStatus.Delayed;
        }

        // If we have actual times and neither exceeds 15 min → OnTime
        if (data.ActualDeparture.HasValue || data.ActualArrival.HasValue)
            return UnifiedFlightStatus.OnTime;

        // No actual times available — use raw status vocabulary as fallback hint
        // Providers that say "delayed"/"LATE" without actual times → treat as Delayed
        if (!string.IsNullOrEmpty(data.RawStatus))
        {
            var raw = data.RawStatus;
            if (raw == "LATE" || raw == "delayed")
                return UnifiedFlightStatus.Delayed;
        }

        return UnifiedFlightStatus.OnTime;
    }

    private enum StatusCategory
    {
        OnTimeOrDelayed,
        Cancelled,
        Diverted,
        Unknown
    }
}
