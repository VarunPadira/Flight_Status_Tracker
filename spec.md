# spec.md — Data Models and Interface Contracts

> This document was authored **before** implementation began, capturing the design decisions and interface contracts that guided development.

## Overview

The Flight Status Tracker queries two stub flight data providers (AeroTrack and QuickFlight), normalises their provider-specific status vocabularies into a unified model, merges results based on data freshness (lastUpdatedUtc), and returns a single consolidated response via a REST API consumed by an Angular frontend.

## Unified Status Enum

```csharp
public enum UnifiedFlightStatus
{
    OnTime,     // Departure or arrival within 15 minutes of schedule
    Delayed,    // Departure or arrival pushed beyond 15 minutes
    Cancelled,  // Flight will not operate
    Diverted,   // Flight landed at a different airport
    Unknown     // No usable status returned by either provider
}
```

## Data Models

### ProviderFlightData (internal DTO from each provider)

```csharp
public record ProviderFlightData(
    string RawStatus,              // Provider-specific vocabulary
    DateTime ScheduledDeparture,
    DateTime ScheduledArrival,
    DateTime? ActualDeparture,     // AeroTrack only (null for QuickFlight)
    DateTime? ActualArrival,       // AeroTrack only (null for QuickFlight)
    string? Terminal,              // AeroTrack only
    string? Gate,                  // AeroTrack only
    string? DelayReason,           // AeroTrack only
    DateTime LastUpdatedUtc
);
```

### FlightStatusResult (API response)

```csharp
public record FlightStatusResult
{
    public string FlightNumber { get; init; }
    public DateOnly Date { get; init; }
    public UnifiedFlightStatus Status { get; init; }
    public DateTime ScheduledDeparture { get; init; }
    public DateTime ScheduledArrival { get; init; }
    public DateTime? ActualDeparture { get; init; }
    public DateTime? ActualArrival { get; init; }
    public string? Terminal { get; init; }
    public string? Gate { get; init; }
    public string? DelayReason { get; init; }
    public DateTime LastUpdatedUtc { get; init; }
    public string Provider { get; init; }
    public string? Message { get; init; }
}
```

### ErrorResponse (400 responses)

```csharp
public record ErrorResponse(string Message, string? Field = null);
```

## Interface Contracts

### IFlightStatusProvider

```csharp
public interface IFlightStatusProvider
{
    string ProviderName { get; }
    Task<ProviderFlightData?> GetFlightStatusAsync(string flightNumber, DateOnly date);
}
```

Two DI-injected implementations:
- **AeroTrackProvider** — Full detail, vocabulary: `ON_SCHEDULE`, `LATE`, `CANCELLED`, `REROUTED`
- **QuickFlightProvider** — Minimal detail, vocabulary: `scheduled`, `delayed`, `cancelled`, `diverted`

### IStatusNormaliser

```csharp
public interface IStatusNormaliser
{
    UnifiedFlightStatus Normalise(string rawStatus, string providerName, ProviderFlightData? flightData = null);
}
```

Normalisation rules:
1. `CANCELLED` / `cancelled` → Cancelled (direct mapping)
2. `REROUTED` / `diverted` → Diverted (direct mapping)
3. `ON_SCHEDULE`, `LATE`, `scheduled`, `delayed` → apply **15-minute rule**:
   - If actual times available and delay > 15 min → Delayed
   - If actual times available and delay ≤ 15 min → OnTime
   - If no actual times, fall back to raw status hint (LATE/delayed → Delayed, others → OnTime)
4. Unrecognised status → Unknown

### IFlightStatusMerger

```csharp
public interface IFlightStatusMerger
{
    FlightStatusResult Merge(
        string flightNumber,
        DateOnly date,
        (ProviderFlightData? Data, UnifiedFlightStatus Status, string ProviderName)? aeroResult,
        (ProviderFlightData? Data, UnifiedFlightStatus Status, string ProviderName)? quickResult
    );
}
```

Merge rules:
- Both present → select by later `lastUpdatedUtc`
- One present → use it
- Neither present → return Unknown with message "No flight data available from any provider."

### IFlightStatusService

```csharp
public interface IFlightStatusService
{
    Task<FlightStatusResult> GetFlightStatusAsync(string flightNumber, DateOnly date);
}
```

Orchestration: query both providers concurrently → normalise → merge → return result.

## API Endpoint

```
GET /flights/status?flightNumber={code}&date={yyyy-MM-dd}
```

| Scenario | HTTP | Response |
|----------|------|----------|
| Valid request | 200 | FlightStatusResult JSON (camelCase, enum as string) |
| Missing flightNumber | 400 | `{"message": "The 'flightNumber' parameter is required.", "field": "flightNumber"}` |
| Missing date | 400 | `{"message": "The 'date' parameter is required.", "field": "date"}` |
| Invalid date format | 400 | `{"message": "The 'date' parameter must be in yyyy-MM-dd format.", "field": "date"}` |

## Status Mapping Tables

| AeroTrack Vocab | Semantic Category |
|----------------|------------------|
| ON_SCHEDULE | OnTime/Delayed (15-min rule) |
| LATE | OnTime/Delayed (15-min rule) |
| CANCELLED | Cancelled |
| REROUTED | Diverted |

| QuickFlight Vocab | Semantic Category |
|------------------|------------------|
| scheduled | OnTime/Delayed (15-min rule) |
| delayed | OnTime/Delayed (15-min rule) |
| cancelled | Cancelled |
| diverted | Diverted |

## Frontend Components

- **FlightSearchComponent** — Reactive form with flight number + date, emits search event
- **FlightStatusCardComponent** — Displays result with colour-coded status badge (green/amber/red/grey), conditionally shows AeroTrack-only fields
- **ErrorDisplayComponent** — Displays error state
- **AppComponent** — Shell orchestrating state (loading/result/error)

## Assumptions

1. Stub providers return deterministic hardcoded data — no randomness
2. The 15-minute threshold is exclusive (>15 min is Delayed, ≤15 min is OnTime)
3. Provider exceptions are swallowed — treated as null (unavailable) for that provider
4. The system runs fully offline with no external dependencies
5. JSON serialization uses camelCase with enums as strings
