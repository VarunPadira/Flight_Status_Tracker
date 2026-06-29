using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FlightStatus.Api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FlightStatus.Tests.Unit;

public class EndpointValidationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public EndpointValidationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    [Fact]
    public async Task GetFlightStatus_MissingFlightNumber_Returns400WithError()
    {
        // Act
        var response = await _client.GetAsync("/flights/status?date=2024-03-15");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        error.Should().NotBeNull();
        error!.Message.Should().Contain("flightNumber");
        error.Field.Should().Be("flightNumber");
    }

    [Fact]
    public async Task GetFlightStatus_MissingDate_Returns400WithError()
    {
        // Act
        var response = await _client.GetAsync("/flights/status?flightNumber=BA123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        error.Should().NotBeNull();
        error!.Message.Should().Contain("date");
        error.Field.Should().Be("date");
    }

    [Fact]
    public async Task GetFlightStatus_InvalidDateFormat_Returns400WithError()
    {
        // Act
        var response = await _client.GetAsync("/flights/status?flightNumber=BA123&date=15-03-2024");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        error.Should().NotBeNull();
        error!.Message.Should().Contain("yyyy-MM-dd");
        error.Field.Should().Be("date");
    }

    [Fact]
    public async Task GetFlightStatus_ValidKnownFlight_Returns200WithExpectedShape()
    {
        // Act
        var response = await _client.GetAsync("/flights/status?flightNumber=BA123&date=2024-03-15");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<FlightStatusResult>(_jsonOptions);
        result.Should().NotBeNull();
        result!.FlightNumber.Should().Be("BA123");
        result.Date.Should().Be(new DateOnly(2024, 3, 15));
        result.Status.Should().NotBe(UnifiedFlightStatus.Unknown);
        result.Provider.Should().NotBeNullOrEmpty();
        result.ScheduledDeparture.Should().NotBe(default);
        result.ScheduledArrival.Should().NotBe(default);
        result.LastUpdatedUtc.Should().NotBe(default);
    }

    [Fact]
    public async Task GetFlightStatus_UnknownFlight_Returns200WithUnknownStatus()
    {
        // Act
        var response = await _client.GetAsync("/flights/status?flightNumber=XX999&date=2024-03-15");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<FlightStatusResult>(_jsonOptions);
        result.Should().NotBeNull();
        result!.FlightNumber.Should().Be("XX999");
        result.Status.Should().Be(UnifiedFlightStatus.Unknown);
        result.Message.Should().NotBeNullOrEmpty();
    }
}
