using System.Text.Json;
using System.Text.Json.Serialization;
using FlightStatus.Api.Endpoints;
using FlightStatus.Api.Providers;
using FlightStatus.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Register DI services
builder.Services.AddSingleton<IFlightStatusProvider, AeroTrackProvider>();
builder.Services.AddSingleton<IFlightStatusProvider, QuickFlightProvider>();
builder.Services.AddSingleton<IStatusNormaliser, StatusNormaliser>();
builder.Services.AddSingleton<IFlightStatusMerger, FlightStatusMerger>();
builder.Services.AddSingleton<IFlightStatusService, FlightStatusService>();

// Configure JSON serialization (camelCase, enum as string)
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Configure CORS to allow frontend origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Use CORS middleware
app.UseCors("AllowFrontend");

// Map flight status endpoints
app.MapFlightStatusEndpoints();

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }
