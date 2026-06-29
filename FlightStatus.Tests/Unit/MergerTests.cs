using FlightStatus.Api.Models;
using FlightStatus.Api.Services;
using FluentAssertions;

namespace FlightStatus.Tests.Unit;

public class MergerTests
{
    private readonly FlightStatusMerger _sut = new();
    private const string FlightNumber = "BA123";
    private static readonly DateOnly Date = new(2024, 3, 15);

    [Fact]
    public void Merge_BothResultsPresent_AeroTrackLater_SelectsAeroTrack()
    {
        // Arrange
        var aeroData = new ProviderFlightData(
            RawStatus: "ON_SCHEDULE",
            ScheduledDeparture: new DateTime(2024, 3, 15, 8, 0, 0, DateTimeKind.Utc),
            ScheduledArrival: new DateTime(2024, 3, 15, 11, 30, 0, DateTimeKind.Utc),
            ActualDeparture: new DateTime(2024, 3, 15, 8, 5, 0, DateTimeKind.Utc),
            ActualArrival: new DateTime(2024, 3, 15, 11, 28, 0, DateTimeKind.Utc),
            Terminal: "5",
            Gate: "B42",
            DelayReason: null,
            LastUpdatedUtc: new DateTime(2024, 3, 15, 12, 0, 0, DateTimeKind.Utc)
        );

        var quickData = new ProviderFlightData(
            RawStatus: "scheduled",
            ScheduledDeparture: new DateTime(2024, 3, 15, 8, 0, 0, DateTimeKind.Utc),
            ScheduledArrival: new DateTime(2024, 3, 15, 11, 30, 0, DateTimeKind.Utc),
            ActualDeparture: null,
            ActualArrival: null,
            Terminal: null,
            Gate: null,
            DelayReason: null,
            LastUpdatedUtc: new DateTime(2024, 3, 15, 11, 0, 0, DateTimeKind.Utc)
        );

        (ProviderFlightData?, UnifiedFlightStatus, string)? aeroResult = (aeroData, UnifiedFlightStatus.OnTime, "AeroTrack");
        (ProviderFlightData?, UnifiedFlightStatus, string)? quickResult = (quickData, UnifiedFlightStatus.OnTime, "QuickFlight");

        // Act
        var result = _sut.Merge(FlightNumber, Date, aeroResult, quickResult);

        // Assert
        result.FlightNumber.Should().Be(FlightNumber);
        result.Date.Should().Be(Date);
        result.Status.Should().Be(UnifiedFlightStatus.OnTime);
        result.Provider.Should().Be("AeroTrack");
        result.ScheduledDeparture.Should().Be(aeroData.ScheduledDeparture);
        result.ScheduledArrival.Should().Be(aeroData.ScheduledArrival);
        result.ActualDeparture.Should().Be(aeroData.ActualDeparture);
        result.ActualArrival.Should().Be(aeroData.ActualArrival);
        result.Terminal.Should().Be("5");
        result.Gate.Should().Be("B42");
        result.DelayReason.Should().BeNull();
        result.LastUpdatedUtc.Should().Be(aeroData.LastUpdatedUtc);
    }

    [Fact]
    public void Merge_BothResultsPresent_QuickFlightLater_SelectsQuickFlight()
    {
        // Arrange
        var aeroData = new ProviderFlightData(
            RawStatus: "LATE",
            ScheduledDeparture: new DateTime(2024, 3, 15, 14, 0, 0, DateTimeKind.Utc),
            ScheduledArrival: new DateTime(2024, 3, 15, 17, 0, 0, DateTimeKind.Utc),
            ActualDeparture: new DateTime(2024, 3, 15, 14, 45, 0, DateTimeKind.Utc),
            ActualArrival: null,
            Terminal: "3",
            Gate: "A12",
            DelayReason: "Weather",
            LastUpdatedUtc: new DateTime(2024, 3, 15, 13, 0, 0, DateTimeKind.Utc)
        );

        var quickData = new ProviderFlightData(
            RawStatus: "delayed",
            ScheduledDeparture: new DateTime(2024, 3, 15, 14, 0, 0, DateTimeKind.Utc),
            ScheduledArrival: new DateTime(2024, 3, 15, 17, 0, 0, DateTimeKind.Utc),
            ActualDeparture: null,
            ActualArrival: null,
            Terminal: null,
            Gate: null,
            DelayReason: null,
            LastUpdatedUtc: new DateTime(2024, 3, 15, 14, 0, 0, DateTimeKind.Utc)
        );

        (ProviderFlightData?, UnifiedFlightStatus, string)? aeroResult = (aeroData, UnifiedFlightStatus.Delayed, "AeroTrack");
        (ProviderFlightData?, UnifiedFlightStatus, string)? quickResult = (quickData, UnifiedFlightStatus.Delayed, "QuickFlight");

        // Act
        var result = _sut.Merge(FlightNumber, Date, aeroResult, quickResult);

        // Assert
        result.FlightNumber.Should().Be(FlightNumber);
        result.Date.Should().Be(Date);
        result.Status.Should().Be(UnifiedFlightStatus.Delayed);
        result.Provider.Should().Be("QuickFlight");
        result.ScheduledDeparture.Should().Be(quickData.ScheduledDeparture);
        result.ScheduledArrival.Should().Be(quickData.ScheduledArrival);
        result.ActualDeparture.Should().BeNull();
        result.ActualArrival.Should().BeNull();
        result.Terminal.Should().BeNull();
        result.Gate.Should().BeNull();
        result.DelayReason.Should().BeNull();
        result.LastUpdatedUtc.Should().Be(quickData.LastUpdatedUtc);
    }

    [Fact]
    public void Merge_OnlyAeroTrackResult_UsedDirectly_AllFieldsPresent()
    {
        // Arrange
        var aeroData = new ProviderFlightData(
            RawStatus: "CANCELLED",
            ScheduledDeparture: new DateTime(2024, 3, 15, 9, 0, 0, DateTimeKind.Utc),
            ScheduledArrival: new DateTime(2024, 3, 15, 12, 0, 0, DateTimeKind.Utc),
            ActualDeparture: null,
            ActualArrival: null,
            Terminal: "2",
            Gate: "C7",
            DelayReason: "Technical issue",
            LastUpdatedUtc: new DateTime(2024, 3, 15, 7, 30, 0, DateTimeKind.Utc)
        );

        (ProviderFlightData?, UnifiedFlightStatus, string)? aeroResult = (aeroData, UnifiedFlightStatus.Cancelled, "AeroTrack");

        // Act
        var result = _sut.Merge(FlightNumber, Date, aeroResult, null);

        // Assert
        result.FlightNumber.Should().Be(FlightNumber);
        result.Date.Should().Be(Date);
        result.Status.Should().Be(UnifiedFlightStatus.Cancelled);
        result.Provider.Should().Be("AeroTrack");
        result.ScheduledDeparture.Should().Be(aeroData.ScheduledDeparture);
        result.ScheduledArrival.Should().Be(aeroData.ScheduledArrival);
        result.Terminal.Should().Be("2");
        result.Gate.Should().Be("C7");
        result.DelayReason.Should().Be("Technical issue");
        result.LastUpdatedUtc.Should().Be(aeroData.LastUpdatedUtc);
    }

    [Fact]
    public void Merge_OnlyQuickFlightResult_UsedDirectly_AdditionalFieldsNull()
    {
        // Arrange
        var quickData = new ProviderFlightData(
            RawStatus: "diverted",
            ScheduledDeparture: new DateTime(2024, 3, 15, 16, 0, 0, DateTimeKind.Utc),
            ScheduledArrival: new DateTime(2024, 3, 15, 19, 0, 0, DateTimeKind.Utc),
            ActualDeparture: null,
            ActualArrival: null,
            Terminal: null,
            Gate: null,
            DelayReason: null,
            LastUpdatedUtc: new DateTime(2024, 3, 15, 15, 30, 0, DateTimeKind.Utc)
        );

        (ProviderFlightData?, UnifiedFlightStatus, string)? quickResult = (quickData, UnifiedFlightStatus.Diverted, "QuickFlight");

        // Act
        var result = _sut.Merge(FlightNumber, Date, null, quickResult);

        // Assert
        result.FlightNumber.Should().Be(FlightNumber);
        result.Date.Should().Be(Date);
        result.Status.Should().Be(UnifiedFlightStatus.Diverted);
        result.Provider.Should().Be("QuickFlight");
        result.ScheduledDeparture.Should().Be(quickData.ScheduledDeparture);
        result.ScheduledArrival.Should().Be(quickData.ScheduledArrival);
        result.ActualDeparture.Should().BeNull();
        result.ActualArrival.Should().BeNull();
        result.Terminal.Should().BeNull();
        result.Gate.Should().BeNull();
        result.DelayReason.Should().BeNull();
        result.LastUpdatedUtc.Should().Be(quickData.LastUpdatedUtc);
    }

    [Fact]
    public void Merge_NoResults_ReturnsUnknownWithMessage()
    {
        // Act
        var result = _sut.Merge(FlightNumber, Date, null, null);

        // Assert
        result.FlightNumber.Should().Be(FlightNumber);
        result.Date.Should().Be(Date);
        result.Status.Should().Be(UnifiedFlightStatus.Unknown);
        result.Provider.Should().Be("None");
        result.Message.Should().Be("No flight data available from any provider.");
    }
}
