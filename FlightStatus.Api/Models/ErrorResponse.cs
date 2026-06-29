namespace FlightStatus.Api.Models;

public record ErrorResponse(string Message, string? Field = null);
