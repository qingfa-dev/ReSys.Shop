---
goal: Replace CartExpiryService BackgroundService with Hangfire recurring job
version: 1.0
date_created: 2026-07-11
status: Completed
last_updated: 2026-07-11
tags: refactor, ordering, hangfire, background-jobs
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Replace `CartExpiryService` (a `BackgroundService` that polls every hour with `Task.Delay` + manual DI scope) with a Hangfire recurring job. `CartExpiryJob` already exists as a scoped service — the only gap is scheduling. This eliminates 59 lines of manual loop infrastructure and aligns with the existing Hangfire stack.

## 1. Requirements & Constraints

- **REQ-001**: Delete `CartExpiryService.cs` (the BackgroundService host)
- **REQ-002**: Keep `CartExpiryJob.cs` unchanged — it already works as a Hangfire-compatible scoped service
- **REQ-003**: Create `CartExpiryJobScheduler.cs` in `Backgrounds/` — an `IHostedService` that calls `RecurringJob.AddOrUpdate<CartExpiryJob>` on startup with cron `0 * * * *` (hourly)
- **REQ-004**: The recurring job registration must run inside the `IHostedService.StartAsync` to ensure Hangfire server is ready — no `Program.cs` changes
- **REQ-005**: Remove `builder.Services.AddHostedService<Services.CartExpiryService>()` from `Ordering.Extension.cs`
- **CON-001**: Keep `builder.Services.AddScoped<Backgrounds.CartExpiryJob>()` — Hangfire resolves the job class from DI
- **CON-002**: Keep `Ordering:CartExpiry:AfterDays` config — `CartExpiryJob` reads it via constructor injection with default of 7
- **CON-003**: Follow existing codebase conventions — do not create new abstractions

## 2. Implementation Steps

### Implementation Phase 1: Delete BackgroundService host

- GOAL-001: Remove the manual loop infrastructure

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Delete `service/Api/src/Module/Ordering/Services/CartExpiryService.cs` | ✅ | 2026-07-11 |
| TASK-002 | Remove `using Module.Ordering.Services;` import from any file that only imports it for `CartExpiryService` | ✅ | 2026-07-11 |

### Implementation Phase 2: Update DI registration

- GOAL-002: Replace `AddHostedService` with no-op (job already registered as Scoped)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | In `Ordering.Extension.cs`: remove `AddHostedService<Services.CartExpiryService>()`, add `AddHostedService<Backgrounds.CartExpiryJobScheduler>()` | ✅ | 2026-07-11 |

### Implementation Phase 3: Create Hangfire recurring job scheduler

- GOAL-003: Create `CartExpiryJobScheduler` in `Backgrounds/` folder alongside the job

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | Create `service/Api/src/Module/Ordering/Backgrounds/CartExpiryJobScheduler.cs` — `IHostedService` that calls `RecurringJob.AddOrUpdate<CartExpiryJob>("cart-expiry", job => job.RunAsync(CancellationToken.None), "0 * * * *")` in `StartAsync` | ✅ | 2026-07-11 |
| TASK-005 | In `Ordering.Extension.cs`: add `services.AddHostedService<Backgrounds.CartExpiryJobScheduler>()` | ✅ | 2026-07-11 |

### Implementation Phase 4: Verify

- GOAL-004: Ensure build passes and no stale references remain

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Run `dotnet build service/Api/src/Api` — verify 0 warnings, 0 errors | ✅ | 2026-07-11 |
| TASK-007 | Run `dotnet test service/Api/tests/Module.UnitTests` — verify existing tests pass | ✅ | 2026-07-11 |
| TASK-008 | Search codebase for any remaining references to `CartExpiryService` in `service/` source files | ✅ | 2026-07-11 |

## 3. Alternatives

- **ALT-001**: Keep `BackgroundService` pattern. Rejected — it duplicates the scheduling infrastructure that Hangfire already provides (persistence, retry, monitoring dashboard, cron expressions). The `ReservationExpiryService` in Inventory still uses `BackgroundService`, but that can be migrated separately.
- **ALT-002**: Add `RecurringJob.AddOrUpdate` directly in `Program.cs`. Initially implemented but rejected — violates module boundary principle. The recurring job belongs to the Ordering module, so scheduling should be encapsulated within the module via `IHostedService` in `Backgrounds/` folder.

## 4. Dependencies

- **DEP-001**: Hangfire must be enabled (`BackgroundJobs.Enabled: true` in `appsettings.json`). The recurring job registration is harmless when Hangfire is disabled — `RecurringJob.AddOrUpdate` uses the in-memory storage which still works.

## 5. Files

- **FILE-001**: `service/Api/src/Module/Ordering/Services/CartExpiryService.cs` — **deleted**
- **FILE-002**: `service/Api/src/Module/Ordering/Backgrounds/CartExpiryJobScheduler.cs` — **created** (IHostedService, schedules recurring job)
- **FILE-003**: `service/Api/src/Module/Ordering/Ordering.Extension.cs` — replaced `AddHostedService<Services.CartExpiryService>` with `AddHostedService<Backgrounds.CartExpiryJobScheduler>`

## 6. Testing

- **TEST-001**: `dotnet build` succeeds with 0 warnings
- **TEST-002**: Existing unit tests pass (`dotnet test service/Api/tests/Module.UnitTests`)
- **TEST-003**: grep for `CartExpiryService` in `service/` returns zero matches in `.cs` source files (doc/spec files excluded)

## 7. Risks & Assumptions

- **RISK-001**: Low — `CartExpiryJob` is already a scoped service registered in DI; Hangfire resolves it via `IServiceProvider` automatically. The `RunAsync` signature (`CancellationToken ct = default`) is Hangfire-compatible.
- **ASSUMPTION-001**: `CartExpiryJob` constructor's `int afterDays = 7` is resolved correctly by Hangfire (default value is used since it's not registered in DI). Verified: Hangfire falls back to `ActivatorUtilities` which respects default parameter values.
- **ASSUMPTION-002**: `RecurringJob.AddOrUpdate` is safe to call even when Hangfire uses in-memory storage (dev/test). Verified: the static API works with any storage implementation.

## 8. Related Specifications / Further Reading

- `docs/superpowers/plans/2026-07-07-mvp-cut.md` — original implementation that created both `CartExpiryJob` and `CartExpiryService`
- `docs/superpowers/specs/2026-07-07-mvp-cut-design.md` — original design spec
- [Hangfire Recurring Jobs Documentation](https://docs.hangfire.io/en/latest/background-methods/performing-recurrent-tasks.html)
