# prompts.md — AI Prompts and Decision Log

> Documents significant AI interactions during development, including key prompts, decisions made, and judgement calls.

## Tool Used

**Kiro** (IDE-integrated AI assistant) — used throughout the entire SDLC: requirements analysis, design, implementation, testing, and documentation.

---

## Phase 1: Analysis & Spec Creation

### Prompt 1: Initial Feature Specification
**Intent:** Transform the idea brief into a structured spec with requirements, design, and implementation plan.

**Approach:** Used Kiro's spec-driven workflow to:
1. Generate requirements.md with user stories and EARS-format acceptance criteria
2. Create design.md with architecture diagrams, interface contracts, and data models
3. Produce tasks.md with a dependency-ordered implementation plan

**Key Decision:** Chose Requirements-First workflow over Design-First because the idea provided clear business requirements that needed technical design.

---

## Phase 2: Architecture Decisions

### Prompt 2: Provider Abstraction
**Decision:** Used `IFlightStatusProvider` interface with DI-injected implementations.

**Reasoning:** The requirements explicitly requires this pattern. It enables swapping stubs for real providers without changing business logic. Each provider registers as a singleton via `IEnumerable<IFlightStatusProvider>`.

### Prompt 3: Normaliser Design — 15-Minute Rule
**Decision:** The normaliser uses a two-phase approach:
1. First maps raw status to a semantic category (Cancelled, Diverted, or OnTimeOrDelayed)
2. For OnTimeOrDelayed, applies the 15-minute rule using actual vs scheduled times
3. Falls back to vocabulary hints when actual times unavailable

**Reasoning:** The requirements defines OnTime/Delayed by time thresholds, not just vocabulary. A QuickFlight "delayed" status without actual times needs a fallback since QuickFlight doesn't provide actuals.

### Prompt 4: Merge Strategy
**Decision:** Simple timestamp comparison — later `lastUpdatedUtc` wins.

**Reasoning:** Direct from requirements. Implemented with null-safe tuple pattern where each provider result is wrapped as `(ProviderFlightData?, UnifiedFlightStatus, ProviderName)?`.

---

## Phase 3: Implementation

### Prompt 5: Project Structure
**Prompt summary:** "Create .NET 8 Minimal API with xUnit tests and Angular frontend"

**Decision:** Used minimal API pattern (no controllers) with extension method-based endpoint registration for clean separation.

### Prompt 6: Test Strategy
**Decision:** Three-layer testing:
- **Property-based tests** (FsCheck) — verify universal correctness properties across random inputs
- **Unit tests** — verify specific examples and edge cases
- **Integration tests** — verify full HTTP pipeline via WebApplicationFactory

**Reasoning:** Property-based tests catch edge cases that example-based tests miss. The 15-minute rule is particularly well-suited to PBT since we can generate arbitrary delay values.

### Prompt 7: Frontend Framework Choice
**Decision:** Angular 21 with standalone components and signals.

**Reasoning:** Angular was chosen from the allowed options. Angular 21 defaults to zoneless change detection, requiring signal-based state management for proper reactivity.

---

## Phase 4: Testing & Verification

### Prompt 8: Property Test Design
**Key properties tested:**
1. Date validation rejects all invalid formats, accepts all valid ones
2. Normaliser maps known statuses correctly (never Unknown)
3. Unrecognised statuses always return Unknown
4. Merger always selects the later timestamp
5. Merger preserves single-source data unchanged
6. Result model always has required non-null fields

### Prompt 9: Integration Test Approach
**Decision:** Used `WebApplicationFactory<Program>` for integration tests that exercise the full DI pipeline, validation, serialization, and routing.

---

## Phase 5: UI Redesign

### Prompt 10: Modern UI Design
**Prompt:** "The UI is looking old, need an attractive UI"

**Decision:** Redesigned with gradient hero section, glass-morphism search card, animated timeline in result card, colour-coded status badges with semantic colours, and smooth slide-up animations.

**Key judgement:** Used CSS custom properties for status colours to maintain consistency and easy theming.

---

## Reflections on AI Usage

**What worked well:**
- Spec-driven development ensured comprehensive requirements before coding
- Property-based test generation caught edge cases around the 15-minute boundary
- Rapid iteration on UI design with instant visual feedback

**What required human judgement:**
- The 15-minute rule interpretation (what to do when no actual times available)
- SSR vs client-only rendering decision (Angular 21 defaults to SSR which caused HTTP issues)
- Choosing between signal-based and zone-based change detection for Angular 21

**Where AI saved time:**
- Generating boilerplate (models, interfaces, DI registration)
- Writing comprehensive test suites with good coverage
- Producing consistent documentation across spec files
