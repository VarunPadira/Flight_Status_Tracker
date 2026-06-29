# reflection.md — What I Would Improve With More Time

## Design Trade-offs I Made

### Normaliser: Vocabulary Mapping vs Time-Based Calculation
The biggest design decision was how to determine OnTime vs Delayed. The requirement defines these by a 15-minute rule, but QuickFlight doesn't provide actual times. I chose a layered approach: use actual times when available (AeroTrack), fall back to the raw status vocabulary when they're not (QuickFlight). This means the system gracefully degrades — it gives the most accurate answer it can with the data available, rather than returning Unknown when times are missing.

**With more time:** I'd introduce a confidence score alongside the status. A time-calculated result gets high confidence; a vocabulary-inferred result gets medium. The UI could then show "Delayed (estimated)" vs "Delayed (confirmed)" to give agents better context for passenger conversations.

### Merge Strategy: Simplicity vs Intelligence
The current merger selects purely by `lastUpdatedUtc`. This is correct per spec, but it's naive — a provider could have a later timestamp but less data (QuickFlight updated more recently but has no gate info).

**With more time:** I'd implement a weighted merge that considers both freshness AND completeness. If timestamps are within a configurable window (e.g., 5 minutes), prefer the provider with richer data. This gives agents more actionable information without sacrificing accuracy.

---

## Architecture Improvements

### 1. Provider Health Monitoring
Currently, provider exceptions are silently swallowed. In production, this masks outages. I'd add:
- A `/health` endpoint that reports per-provider status
- Circuit breaker pattern (Polly) to avoid repeated calls to a failing provider
- Metrics emission (provider latency, error rate) for observability dashboards

### 2. Request Pipeline Middleware
The validation logic lives in the endpoint handler. I'd extract it into a reusable validation middleware or filter, following the single-responsibility principle. This becomes important when adding more endpoints (e.g., `/flights/history`, `/flights/alerts`).

### 3. Configuration Externalisation
Provider status vocabularies are hardcoded in the normaliser. If a provider changes their vocabulary (e.g., AeroTrack adds "BOARDING"), it requires a code change and redeployment. I'd move vocabulary mappings to configuration (`appsettings.json` or a database), enabling ops teams to add mappings without developer involvement.

### 4. Event-Driven Architecture Readiness
For a real SkyRoute platform, I'd design the provider interface to support both pull (current) and push (WebSocket/SSE) models. The `IFlightStatusProvider` could expose an `IObservable<FlightStatusUpdate>` for real-time streaming, with the current polling approach as a fallback.

---

## Testing Improvements

### 5. Boundary Value Analysis
My property tests generate delays from 0-15 (OnTime) and 16-180 (Delayed), but I'd add explicit boundary tests at exactly 14, 15, and 16 minutes to verify the threshold is implemented correctly at the edges. Boundary errors are the most common bugs in threshold-based logic.

### 6. Contract Testing
The frontend TypeScript interfaces and backend C# models could drift apart silently. I'd generate an OpenAPI spec from the .NET API (via Swashbuckle), then use it to auto-generate TypeScript types. Any contract break would fail the build.

### 7. Chaos Testing
I'd add tests where providers respond with:
- Extremely slow responses (timeout handling)
- Malformed data (defensive parsing)
- Intermittent failures (retry logic verification)

This validates the system's resilience, not just its happy-path correctness.

### 8. Visual Regression Testing
With the colour-coded status cards, I'd add Playwright screenshot comparison tests to catch unintended CSS regressions. A broken colour variable or specificity override could silently break the green/amber/red distinction.

---

## Frontend Improvements

### 9. Accessibility (WCAG 2.1 AA)
Current state covers basics (semantic HTML, `role="alert"`, form labels), but a full audit would add:
- `aria-live="polite"` on the result region so screen readers announce status changes
- Focus management — move focus to the result card after a successful search
- High-contrast mode support using `prefers-contrast` media query
- Keyboard shortcuts (Enter to search, Escape to clear)

### 10. Internationalisation (i18n)
Status labels, error messages, and date formatting are hardcoded in English. Angular's i18n support would allow locale-aware date formatting and translated status labels — important for an airline support tool used globally.

### 11. Progressive Enhancement
The search could pre-populate from URL query params (`/search?flight=BA123&date=2024-03-15`), enabling agents to share links to specific flight lookups. This is a small change with high usability impact.

---

## Operational Readiness

### 12. Structured Logging & Correlation
Add a correlation ID to each request that flows through provider calls, normalisation, and merging. When an agent reports incorrect data, support can trace the exact decision chain: which provider was selected, what the raw status was, how the 15-minute rule resolved.

### 13. Docker Compose for One-Command Startup
```yaml
services:
  api:
    build: ./FlightStatus.Api
    ports: ["5000:5000"]
  ui:
    build: ./flight-status-ui
    ports: ["4200:80"]
    depends_on: [api]
```
This eliminates "works on my machine" issues and simplifies onboarding.

### 14. Feature Flags
Wrap the 15-minute rule threshold in a feature flag (e.g., LaunchDarkly or a simple config toggle). This allows ops to adjust the threshold during irregular operations (e.g., airport-wide delays where 15 minutes is too aggressive).

---

## What I'm Most Proud Of

1. **The spec-first approach** — designing before coding caught the 15-minute rule complexity early, before it became a refactoring debt.
2. **Property-based testing** — generating hundreds of random delay values to verify the time threshold gives much higher confidence than a handful of example-based tests.
3. **The layered normaliser** — gracefully handling QuickFlight's lack of actual times without forcing a "least common denominator" design that loses AeroTrack's richer data.
4. **Clean separation of concerns** — each component (provider, normaliser, merger, service) is independently testable and replaceable, exactly what DI-based architecture should enable.

---

## Summary

The implementation satisfies all functional requirements. The improvements above focus on **production-readiness**, **operational excellence**, and **user experience depth** — the qualities that differentiate a working prototype from a system teams can confidently operate and evolve.
