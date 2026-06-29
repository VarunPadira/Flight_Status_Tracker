using FlightStatus.Api.Models;
using FlightStatus.Api.Services;
using FluentAssertions;

namespace FlightStatus.Tests.Unit;

public class NormaliserTests
{
    private readonly StatusNormaliser _sut = new();

    private static readonly DateTime BaseScheduledDep = new(2024, 3, 15, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime BaseScheduledArr = new(2024, 3, 15, 11, 30, 0, DateTimeKind.Utc);

    private static ProviderFlightData MakeFlightData(
        string rawStatus,
        DateTime? actualDep = null,
        DateTime? actualArr = null) => new(
        RawStatus: rawStatus,
        ScheduledDeparture: BaseScheduledDep,
        ScheduledArrival: BaseScheduledArr,
        ActualDeparture: actualDep,
        ActualArrival: actualArr,
        Terminal: null,
        Gate: null,
        DelayReason: null,
        LastUpdatedUtc: DateTime.UtcNow
    );

    // --- Cancelled / Diverted are direct mappings regardless of times ---

    [Fact]
    public void Normalise_AeroTrack_Cancelled_ReturnsCancelled()
    {
        var data = MakeFlightData("CANCELLED");
        _sut.Normalise("CANCELLED", "AeroTrack", data).Should().Be(UnifiedFlightStatus.Cancelled);
    }

    [Fact]
    public void Normalise_AeroTrack_Rerouted_ReturnsDiverted()
    {
        var data = MakeFlightData("REROUTED");
        _sut.Normalise("REROUTED", "AeroTrack", data).Should().Be(UnifiedFlightStatus.Diverted);
    }

    [Fact]
    public void Normalise_QuickFlight_Cancelled_ReturnsCancelled()
    {
        var data = MakeFlightData("cancelled");
        _sut.Normalise("cancelled", "QuickFlight", data).Should().Be(UnifiedFlightStatus.Cancelled);
    }

    [Fact]
    public void Normalise_QuickFlight_Diverted_ReturnsDiverted()
    {
        var data = MakeFlightData("diverted");
        _sut.Normalise("diverted", "QuickFlight", data).Should().Be(UnifiedFlightStatus.Diverted);
    }

    // --- 15-minute rule: OnTime when within 15 min ---

    [Fact]
    public void Normalise_AeroTrack_OnSchedule_ActualWithin15Min_ReturnsOnTime()
    {
        var data = MakeFlightData("ON_SCHEDULE",
            actualDep: BaseScheduledDep.AddMinutes(5),
            actualArr: BaseScheduledArr.AddMinutes(-2));
        _sut.Normalise("ON_SCHEDULE", "AeroTrack", data).Should().Be(UnifiedFlightStatus.OnTime);
    }

    [Fact]
    public void Normalise_AeroTrack_OnSchedule_ExactlyOnTime_ReturnsOnTime()
    {
        var data = MakeFlightData("ON_SCHEDULE",
            actualDep: BaseScheduledDep,
            actualArr: BaseScheduledArr);
        _sut.Normalise("ON_SCHEDULE", "AeroTrack", data).Should().Be(UnifiedFlightStatus.OnTime);
    }

    [Fact]
    public void Normalise_AeroTrack_OnSchedule_Exactly15Min_ReturnsOnTime()
    {
        var data = MakeFlightData("ON_SCHEDULE",
            actualDep: BaseScheduledDep.AddMinutes(15),
            actualArr: BaseScheduledArr);
        _sut.Normalise("ON_SCHEDULE", "AeroTrack", data).Should().Be(UnifiedFlightStatus.OnTime);
    }

    // --- 15-minute rule: Delayed when beyond 15 min ---

    [Fact]
    public void Normalise_AeroTrack_Late_DepartureDelayed20Min_ReturnsDelayed()
    {
        var data = MakeFlightData("LATE",
            actualDep: BaseScheduledDep.AddMinutes(20),
            actualArr: BaseScheduledArr.AddMinutes(15));
        _sut.Normalise("LATE", "AeroTrack", data).Should().Be(UnifiedFlightStatus.Delayed);
    }

    [Fact]
    public void Normalise_AeroTrack_Late_ArrivalDelayed45Min_ReturnsDelayed()
    {
        var data = MakeFlightData("LATE",
            actualDep: BaseScheduledDep.AddMinutes(10),
            actualArr: BaseScheduledArr.AddMinutes(45));
        _sut.Normalise("LATE", "AeroTrack", data).Should().Be(UnifiedFlightStatus.Delayed);
    }

    [Fact]
    public void Normalise_QuickFlight_Delayed_NoActualTimes_FallsBackToDelayed()
    {
        // QuickFlight has no actual times — falls back to raw status hint
        var data = MakeFlightData("delayed");
        _sut.Normalise("delayed", "QuickFlight", data).Should().Be(UnifiedFlightStatus.Delayed);
    }

    [Fact]
    public void Normalise_QuickFlight_Scheduled_NoActualTimes_ReturnsOnTime()
    {
        // QuickFlight "scheduled" with no actual times → OnTime
        var data = MakeFlightData("scheduled");
        _sut.Normalise("scheduled", "QuickFlight", data).Should().Be(UnifiedFlightStatus.OnTime);
    }

    // --- Unknown status ---

    [Fact]
    public void Normalise_UnrecognisedAeroTrackStatus_ReturnsUnknown()
    {
        var data = MakeFlightData("SOME_RANDOM_STATUS");
        _sut.Normalise("SOME_RANDOM_STATUS", "AeroTrack", data).Should().Be(UnifiedFlightStatus.Unknown);
    }

    [Fact]
    public void Normalise_UnrecognisedQuickFlightStatus_ReturnsUnknown()
    {
        var data = MakeFlightData("unknown_status");
        _sut.Normalise("unknown_status", "QuickFlight", data).Should().Be(UnifiedFlightStatus.Unknown);
    }

    [Fact]
    public void Normalise_UnknownProvider_ReturnsUnknown()
    {
        var data = MakeFlightData("ON_SCHEDULE");
        _sut.Normalise("ON_SCHEDULE", "SomeOtherProvider", data).Should().Be(UnifiedFlightStatus.Unknown);
    }

    // --- Case-insensitive provider name ---

    [Theory]
    [InlineData("aerotrack")]
    [InlineData("AEROTRACK")]
    [InlineData("AeroTrack")]
    public void Normalise_CaseInsensitiveProviderName_AeroTrack(string providerName)
    {
        var data = MakeFlightData("ON_SCHEDULE",
            actualDep: BaseScheduledDep.AddMinutes(3));
        _sut.Normalise("ON_SCHEDULE", providerName, data).Should().Be(UnifiedFlightStatus.OnTime);
    }

    [Theory]
    [InlineData("quickflight")]
    [InlineData("QUICKFLIGHT")]
    [InlineData("QuickFlight")]
    public void Normalise_CaseInsensitiveProviderName_QuickFlight(string providerName)
    {
        var data = MakeFlightData("scheduled");
        _sut.Normalise("scheduled", providerName, data).Should().Be(UnifiedFlightStatus.OnTime);
    }

    // --- No flight data (backward compat) ---

    [Fact]
    public void Normalise_NoFlightData_CancelledStillWorks()
    {
        _sut.Normalise("CANCELLED", "AeroTrack").Should().Be(UnifiedFlightStatus.Cancelled);
    }

    [Fact]
    public void Normalise_NoFlightData_OnScheduleDefaultsToOnTime()
    {
        _sut.Normalise("ON_SCHEDULE", "AeroTrack").Should().Be(UnifiedFlightStatus.OnTime);
    }
}
