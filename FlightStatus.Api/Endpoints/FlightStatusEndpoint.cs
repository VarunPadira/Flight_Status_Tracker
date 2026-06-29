namespace FlightStatus.Api.Endpoints;

using System.Globalization;
using FlightStatus.Api.Models;
using FlightStatus.Api.Services;

public static class FlightStatusEndpoint
{
    public static WebApplication MapFlightStatusEndpoints(this WebApplication app)
    {
        app.MapGet("/flights/status", async (string? flightNumber, string? date, IFlightStatusService flightStatusService) =>
        {
            if (string.IsNullOrWhiteSpace(flightNumber))
            {
                return Results.BadRequest(new ErrorResponse("The 'flightNumber' parameter is required.", "flightNumber"));
            }

            if (string.IsNullOrWhiteSpace(date))
            {
                return Results.BadRequest(new ErrorResponse("The 'date' parameter is required.", "date"));
            }

            if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                return Results.BadRequest(new ErrorResponse("The 'date' parameter must be in yyyy-MM-dd format.", "date"));
            }

            var result = await flightStatusService.GetFlightStatusAsync(flightNumber, parsedDate);

            return Results.Ok(result);
        });

        return app;
    }
}
