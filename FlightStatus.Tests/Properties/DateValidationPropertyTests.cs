// Feature: flight-status-tracker, Property 1: Date validation rejects invalid formats

namespace FlightStatus.Tests.Properties;

using System.Net;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

public class DateValidationPropertyTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DateValidationPropertyTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private static Gen<string> GenInvalidDateString()
    {
        return Gen.OneOf(
            // Random alphanumeric strings
            Gen.Choose(1, 20).SelectMany(len =>
                Gen.ArrayOf(Gen.Elements(
                    'a', 'b', 'c', 'x', 'y', 'z', '0', '1', '9', '-', '/', ' ', '.', 'T'))
                .Select(chars => new string(chars))),
            // Partial dates like "2024-03"
            Gen.Choose(2020, 2025).SelectMany(y =>
                Gen.Choose(1, 12).Select(m => $"{y}-{m:D2}")),
            // Wrong separators (slashes)
            Gen.Choose(2020, 2025).SelectMany(y =>
                Gen.Choose(1, 12).SelectMany(m =>
                    Gen.Choose(1, 28).Select(d => $"{y}/{m:D2}/{d:D2}"))),
            // dd-MM-yyyy (reversed format)
            Gen.Choose(1, 28).SelectMany(d =>
                Gen.Choose(1, 12).SelectMany(m =>
                    Gen.Choose(2020, 2025).Select(y => $"{d:D2}-{m:D2}-{y}"))),
            // MM/dd/yyyy (US format)
            Gen.Choose(1, 12).SelectMany(m =>
                Gen.Choose(1, 28).SelectMany(d =>
                    Gen.Choose(2020, 2025).Select(y => $"{m:D2}/{d:D2}/{y}"))),
            // Invalid month values in correct format structure
            Gen.Choose(2020, 2025).Select(y => $"{y}-13-01"),
            Gen.Choose(2020, 2025).Select(y => $"{y}-00-15"),
            // Invalid day values
            Gen.Choose(2020, 2025).Select(y => $"{y}-02-30"),
            Gen.Choose(2020, 2025).Select(y => $"{y}-04-31"),
            // Common invalid patterns
            Gen.Elements(
                "", " ", "not-a-date", "2024/03/15", "15-03-2024",
                "2024-3-15", "2024-03-5", "24-03-15", "2024-13-15",
                "2024-00-15", "2024-03-32", "abcd-ef-gh")
        ).Where(s => !IsValidYyyyMmDd(s));
    }

    // **Validates: Requirements 1.4**
    [Property(MaxTest = 100)]
    public Property InvalidDateFormats_AreRejectedWith400()
    {
        var arb = GenInvalidDateString().ToArbitrary();

        return Prop.ForAll(arb, date =>
        {
            var response = _client.GetAsync($"/flights/status?flightNumber=BA123&date={Uri.EscapeDataString(date)}").Result;
            return (response.StatusCode == HttpStatusCode.BadRequest)
                .Label($"Expected 400 for date='{date}' but got {(int)response.StatusCode}");
        });
    }

    // **Validates: Requirements 1.4**
    [Property(MaxTest = 100)]
    public Property ValidDateFormats_AreAcceptedAndNotRejectedAsInvalid()
    {
        var validDateGen = (from year in Gen.Choose(2020, 2025)
                            from month in Gen.Choose(1, 12)
                            from day in Gen.Choose(1, 28)
                            select $"{year:D4}-{month:D2}-{day:D2}")
                           .ToArbitrary();

        return Prop.ForAll(validDateGen, date =>
        {
            var response = _client.GetAsync($"/flights/status?flightNumber=BA123&date={date}").Result;
            // Valid dates should get 200 (date validation passes; endpoint returns flight data or unknown)
            return (response.StatusCode == HttpStatusCode.OK)
                .Label($"Expected 200 for date='{date}' but got {(int)response.StatusCode}");
        });
    }

    private static bool IsValidYyyyMmDd(string value)
    {
        return DateOnly.TryParseExact(
            value,
            "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out _);
    }
}
