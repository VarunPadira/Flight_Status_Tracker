using FlightStatus.Api.Models;
using FlightStatus.Api.Providers;
using FlightStatus.Api.Services;
using FluentAssertions;

namespace FlightStatus.Tests.Unit;

public class ServiceTests
{
    private const string FlightNumber = "BA123";
    private static readonly DateOnly Date = new(2024, 3, 15);

    #region Test Doubles

    private class FakeProvider : IFlightStatusProvider
    {
        private readonly Func<string, DateOnly, Task<ProviderFlightData?>> _handler;

        public FakeProvider(string providerName, Func<string, DateOnly, Task<ProviderFlightData?>> handler)
        {
            ProviderName = providerName;
            _handler = handler;
        }

        public string ProviderName { get; }

        public Task<ProviderFlightData?> GetFlightStatusAsync(string flightNumber, DateOnly date)
            => _handler(flightNumber, date);
    }

    private class FakeNormaliser : IStatusNormaliser
    {
        private readonly Dictionary<string, UnifiedFlightStatus> _mappings = new();

        public List<(string RawStatus, string ProviderName)> NormaliseCalls { get; } = new();

        public void Setup(string rawStatus, UnifiedFlightStatus result)
            => _mappings[rawStatus] = result;

        public UnifiedFlightStatus Normalise(string rawStatus, string providerName, ProviderFlightData? flightData = null)
        {
            NormaliseCalls.Add((rawStatus, providerName));
            return _mappings.TryGetValue(rawStatus, out var status)
                ? status
                : UnifiedFlightStatus.Unknown;
        }
    }

    private class FakeMerger : IFlightStatusMerger
    {
        public (string FlightNumber, DateOnly Date,
            (ProviderFlightData? Data, UnifiedFlightStatus Status, string ProviderName)? AeroResult,
            (ProviderFlightData? Data, UnifiedFlightStatus Status, string ProviderName)? QuickResult)? LastCall { get; private set; }

        public FlightStatusResult ResultToReturn { get; set; } = new()
        {
            FlightNumber = FlightNumber,
            Date = Date,
            Status = UnifiedFlightStatus.OnTime,
            Provider = "AeroTrack"
        };

        public FlightStatusResult Merge(
            string flightNumber,
            DateOnly date,
            (ProviderFlightData? Data, UnifiedFlightStatus Status, string ProviderName)? aeroResult,
            (ProviderFlightData? Data, UnifiedFlightStatus Status, string ProviderName)? quickResult)
        {
            LastCall = (flightNumber, date, aeroResult, quickResult);
            return ResultToReturn;
        }
    }

    #endregion

    private static ProviderFlightData CreateAeroData() => new(
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

    private static ProviderFlightData CreateQuickData() => new(
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

    [Fact]
    public async Task GetFlightStatusAsync_BothProvidersReturnData_NormalisesAndPassesToMerger()
    {
        // Arrange
        var aeroData = CreateAeroData();
        var quickData = CreateQuickData();

        var aeroProvider = new FakeProvider("AeroTrack", (_, _) => Task.FromResult<ProviderFlightData?>(aeroData));
        var quickProvider = new FakeProvider("QuickFlight", (_, _) => Task.FromResult<ProviderFlightData?>(quickData));

        var normaliser = new FakeNormaliser();
        normaliser.Setup("ON_SCHEDULE", UnifiedFlightStatus.OnTime);
        normaliser.Setup("scheduled", UnifiedFlightStatus.OnTime);

        var merger = new FakeMerger();

        var service = new FlightStatusService(
            new IFlightStatusProvider[] { aeroProvider, quickProvider },
            normaliser,
            merger);

        // Act
        await service.GetFlightStatusAsync(FlightNumber, Date);

        // Assert
        normaliser.NormaliseCalls.Should().HaveCount(2);
        normaliser.NormaliseCalls.Should().Contain(("ON_SCHEDULE", "AeroTrack"));
        normaliser.NormaliseCalls.Should().Contain(("scheduled", "QuickFlight"));

        merger.LastCall.Should().NotBeNull();
        merger.LastCall!.Value.FlightNumber.Should().Be(FlightNumber);
        merger.LastCall!.Value.Date.Should().Be(Date);

        merger.LastCall!.Value.AeroResult.Should().NotBeNull();
        merger.LastCall!.Value.AeroResult!.Value.Data.Should().Be(aeroData);
        merger.LastCall!.Value.AeroResult!.Value.Status.Should().Be(UnifiedFlightStatus.OnTime);
        merger.LastCall!.Value.AeroResult!.Value.ProviderName.Should().Be("AeroTrack");

        merger.LastCall!.Value.QuickResult.Should().NotBeNull();
        merger.LastCall!.Value.QuickResult!.Value.Data.Should().Be(quickData);
        merger.LastCall!.Value.QuickResult!.Value.Status.Should().Be(UnifiedFlightStatus.OnTime);
        merger.LastCall!.Value.QuickResult!.Value.ProviderName.Should().Be("QuickFlight");
    }

    [Fact]
    public async Task GetFlightStatusAsync_OneProviderReturnsNull_PassesNullToMerger()
    {
        // Arrange
        var aeroData = CreateAeroData();

        var aeroProvider = new FakeProvider("AeroTrack", (_, _) => Task.FromResult<ProviderFlightData?>(aeroData));
        var quickProvider = new FakeProvider("QuickFlight", (_, _) => Task.FromResult<ProviderFlightData?>(null));

        var normaliser = new FakeNormaliser();
        normaliser.Setup("ON_SCHEDULE", UnifiedFlightStatus.OnTime);

        var merger = new FakeMerger();

        var service = new FlightStatusService(
            new IFlightStatusProvider[] { aeroProvider, quickProvider },
            normaliser,
            merger);

        // Act
        await service.GetFlightStatusAsync(FlightNumber, Date);

        // Assert
        normaliser.NormaliseCalls.Should().HaveCount(1);
        normaliser.NormaliseCalls.Should().Contain(("ON_SCHEDULE", "AeroTrack"));

        merger.LastCall.Should().NotBeNull();
        merger.LastCall!.Value.AeroResult.Should().NotBeNull();
        merger.LastCall!.Value.AeroResult!.Value.Data.Should().Be(aeroData);
        merger.LastCall!.Value.QuickResult.Should().BeNull();
    }

    [Fact]
    public async Task GetFlightStatusAsync_BothProvidersReturnNull_PassesNullForBothToMerger()
    {
        // Arrange
        var aeroProvider = new FakeProvider("AeroTrack", (_, _) => Task.FromResult<ProviderFlightData?>(null));
        var quickProvider = new FakeProvider("QuickFlight", (_, _) => Task.FromResult<ProviderFlightData?>(null));

        var normaliser = new FakeNormaliser();
        var merger = new FakeMerger();
        merger.ResultToReturn = new FlightStatusResult
        {
            FlightNumber = FlightNumber,
            Date = Date,
            Status = UnifiedFlightStatus.Unknown,
            Provider = "None",
            Message = "No flight data available from any provider."
        };

        var service = new FlightStatusService(
            new IFlightStatusProvider[] { aeroProvider, quickProvider },
            normaliser,
            merger);

        // Act
        var result = await service.GetFlightStatusAsync(FlightNumber, Date);

        // Assert
        normaliser.NormaliseCalls.Should().BeEmpty();

        merger.LastCall.Should().NotBeNull();
        merger.LastCall!.Value.AeroResult.Should().BeNull();
        merger.LastCall!.Value.QuickResult.Should().BeNull();

        result.Status.Should().Be(UnifiedFlightStatus.Unknown);
        result.Message.Should().Be("No flight data available from any provider.");
    }

    [Fact]
    public async Task GetFlightStatusAsync_ProviderThrowsException_TreatedAsNull()
    {
        // Arrange
        var quickData = CreateQuickData();

        var aeroProvider = new FakeProvider("AeroTrack", (_, _) =>
            throw new InvalidOperationException("Provider unavailable"));
        var quickProvider = new FakeProvider("QuickFlight", (_, _) => Task.FromResult<ProviderFlightData?>(quickData));

        var normaliser = new FakeNormaliser();
        normaliser.Setup("scheduled", UnifiedFlightStatus.OnTime);

        var merger = new FakeMerger();

        var service = new FlightStatusService(
            new IFlightStatusProvider[] { aeroProvider, quickProvider },
            normaliser,
            merger);

        // Act
        await service.GetFlightStatusAsync(FlightNumber, Date);

        // Assert — AeroTrack exception is swallowed; only QuickFlight result passed to merger
        normaliser.NormaliseCalls.Should().HaveCount(1);
        normaliser.NormaliseCalls.Should().Contain(("scheduled", "QuickFlight"));

        merger.LastCall.Should().NotBeNull();
        merger.LastCall!.Value.AeroResult.Should().BeNull();
        merger.LastCall!.Value.QuickResult.Should().NotBeNull();
        merger.LastCall!.Value.QuickResult!.Value.Data.Should().Be(quickData);
        merger.LastCall!.Value.QuickResult!.Value.Status.Should().Be(UnifiedFlightStatus.OnTime);
        merger.LastCall!.Value.QuickResult!.Value.ProviderName.Should().Be("QuickFlight");
    }
}
