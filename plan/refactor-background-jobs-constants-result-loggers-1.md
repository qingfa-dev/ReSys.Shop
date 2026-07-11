---
goal: Add Constants/Result/Loggers to all background jobs and schedulers, refactor ReservationExpiry to Job+Scheduler pattern
version: 1.0
date_created: 2026-07-11
status: Completed
last_updated: 2026-07-11
tags: refactor, ordering, inventory, hangfire, background-jobs
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Add `Constants`, `Result`, and `Loggers` split files to both `CartExpiryJob`/`CartExpiryJobScheduler` (Ordering) and newly created `ReservationExpiryJob`/`ReservationExpiryJobScheduler` (Inventory). Refactor Inventory's `ReservationExpiryService` (BackgroundService) into the same `Job` + `JobScheduler` pattern used by Ordering.

## 1. Requirements & Constraints

- **REQ-001**: Create `CartExpiryJob.Constants.cs`, `CartExpiryJob.Result.cs`, `CartExpiryJob.Loggers.cs` in `Ordering/Backgrounds/`
- **REQ-002**: Create `CartExpiryJobScheduler.Constants.cs`, `CartExpiryJobScheduler.Result.cs`, `CartExpiryJobScheduler.Loggers.cs` in `Ordering/Backgrounds/`
- **REQ-003**: Refactor `CartExpiryJob.cs` to use the new Loggers and Result classes instead of inline `_logger.LogInformation`
- **REQ-004**: Refactor `CartExpiryJobScheduler.cs` to use the new Constants for job ID and cron
- **REQ-005**: Create `Inventory/Backgrounds/` folder
- **REQ-006**: Create `ReservationExpiryJob.cs` + `ReservationExpiryJob.Scheduler.cs` in `Inventory/Backgrounds/` — Hangfire job + IHostedService scheduler
- **REQ-007**: Create `ReservationExpiryJob.Constants.cs`, `ReservationExpiryJob.Result.cs`, `ReservationExpiryJob.Loggers.cs` in `Inventory/Backgrounds/`
- **REQ-010**: Delete `Inventory/Services/ReservationExpiryService.cs`
- **REQ-011**: Update `Inventory.Extension.cs` — replace `AddHostedService<ReservationExpiryService>()` with `AddScoped<ReservationExpiryJob>()` + `AddHostedService<ReservationExpiryJobScheduler>()`
- **CON-001**: Follow existing LoggerMessage pattern (EventId ranges: 3000-3999 for Ordering, 4000-4999 for Inventory domain, 5xxx for background jobs)
- **CON-002**: Follow existing `*Result.cs` pattern with `Success` and `Errors` nested classes
- **CON-003**: Follow existing `*Constant.cs` pattern with `Constraints`, `Defaults`, `Query` nested classes
- **CON-004**: ReservationExpiryJobScheduler uses cron `*/1 * * * *` (every 60s) — matches current `SweepInterval` of 60 seconds

## 2. Implementation Steps

### Implementation Phase 1: Ordering Backgrounds — Constants, Result, Loggers

- GOAL-001: Add supporting files for CartExpiryJob and CartExpiryJobScheduler

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `Ordering/Backgrounds/CartExpiryJob.Constants.cs` (JobId, Cron, AfterDays default) | ✅ | 2026-07-11 |
| TASK-002 | Create `Ordering/Backgrounds/CartExpiryJob.Result.cs` (Success/Errors) | ✅ | 2026-07-11 |
| TASK-003 | Populate `Ordering/Backgrounds/CartExpiryJob.Loggers.cs` (source-generated loggers with EventId 5001-5003) | ✅ | 2026-07-11 |
| TASK-004 | Refactor `CartExpiryJob.cs` — replace inline `_logger.LogInformation` with `CartExpiryJobLoggers` calls, use `CartExpiryJobConstants.Defaults.AfterDays` | ✅ | 2026-07-11 |
| TASK-005 | Refactor `CartExpiryJob.Scheduler.cs` — inject `ILogger<CartExpiryJobScheduler>`, use `CartExpiryJobConstants.Scheduler` for job ID and cron | ✅ | 2026-07-11 |

### Implementation Phase 2: Inventory — Create ReservationExpiryJob + supporting files

- GOAL-002: Create the Hangfire job and its Constants/Result/Loggers files under new `Inventory/Backgrounds/` folder

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Create `Inventory/Backgrounds/` directory | ✅ | 2026-07-11 |
| TASK-010 | Create `Inventory/Backgrounds/ReservationExpiryJob.Constants.cs` | ✅ | 2026-07-11 |
| TASK-011 | Create `Inventory/Backgrounds/ReservationExpiryJob.Result.cs` | ✅ | 2026-07-11 |
| TASK-012 | Create `Inventory/Backgrounds/ReservationExpiryJob.Loggers.cs` (EventId 5201-5203 — migrate inline ReservationExpiryLoggers) | ✅ | 2026-07-11 |
| TASK-013 | Create `Inventory/Backgrounds/ReservationExpiryJob.cs` — scoped Hangfire job wrapping `IStockReservationService.ExpireReservationsAndRestoreStockAsync` | ✅ | 2026-07-11 |

### Implementation Phase 3: Inventory — Create ReservationExpiryJobScheduler + supporting files

- GOAL-003: Create the scheduler and its supporting files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-014 | Create `Inventory/Backgrounds/ReservationExpiryJob.Scheduler.cs` — IHostedService registering the Hangfire recurring job | ✅ | 2026-07-11 |

### Implementation Phase 4: Inventory — Remove old BackgroundService, update DI

- GOAL-004: Clean up old ReservationExpiryService and wire new pattern into Inventory.Extension.cs

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | Delete `Inventory/Services/ReservationExpiryService.cs` | ✅ | 2026-07-11 |
| TASK-016 | Update `Inventory.Extension.cs` — replace `AddHostedService<ReservationExpiryService>()` with `AddScoped<ReservationExpiryJob>()` + `AddHostedService<ReservationExpiryJobScheduler>()` | ✅ | 2026-07-11 |

### Implementation Phase 5: Verify

- GOAL-005: Ensure build passes and tests pass

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-017 | Run `dotnet build service/Api/src/Api` — verify 0 warnings, 0 errors | ✅ | 2026-07-11 |
| TASK-018 | Run `dotnet test service/Api/tests/Module.UnitTests` — verify existing tests pass (update ReservationExpiryServiceTests to ReservationExpiryJobTests) | ✅ | 2026-07-11 |
| TASK-019 | Search codebase for remaining references to `ReservationExpiryService` in `service/` source files | ✅ | 2026-07-11 |

## 3. Alternatives

- **ALT-001**: Keep `ReservationExpiryLoggers` inline in the service file. Rejected — does not follow the codebase convention of per-entity split files.
- **ALT-002**: Keep `ReservationExpiryService` as BackgroundService. Rejected — inconsistent with Ordering's Hangfire-based pattern; duplicates scheduling infrastructure.
- **ALT-003**: Put Inventory background jobs in a `Services/` subfolder. Rejected — `Backgrounds/` is the canonical location (matching Ordering module).

## 4. Dependencies

- **DEP-001**: Hangfire must be enabled in appsettings (`BackgroundJobs.Enabled: true`)
- **DEP-002**: `IStockReservationService` must remain registered as scoped (already done in `Inventory.Extension.cs`)

## 5. Files

- **FILE-001**: `Ordering/Backgrounds/CartExpiryJob.Constants.cs` — **create**
- **FILE-002**: `Ordering/Backgrounds/CartExpiryJob.Result.cs` — **create**
- **FILE-003**: `Ordering/Backgrounds/CartExpiryJob.Loggers.cs` — **create**
- **FILE-004**: `Ordering/Backgrounds/CartExpiryJob.cs` — **refactor** to use loggers/result
- **FILE-005**: `Ordering/Backgrounds/CartExpiryJobScheduler.Constants.cs` — **create**
- **FILE-006**: `Ordering/Backgrounds/CartExpiryJobScheduler.Result.cs` — **create**
- **FILE-007**: `Ordering/Backgrounds/CartExpiryJobScheduler.Loggers.cs` — **create**
- **FILE-008**: `Ordering/Backgrounds/CartExpiryJobScheduler.cs` — **refactor** to use constants
- **FILE-009**: `Inventory/Backgrounds/` — **create folder**
- **FILE-010**: `Inventory/Backgrounds/ReservationExpiryJob.cs` — **create**
- **FILE-011**: `Inventory/Backgrounds/ReservationExpiryJob.Constants.cs` — **create**
- **FILE-012**: `Inventory/Backgrounds/ReservationExpiryJob.Result.cs` — **create**
- **FILE-013**: `Inventory/Backgrounds/ReservationExpiryJob.Loggers.cs` — **create**
- **FILE-014**: `Inventory/Backgrounds/ReservationExpiryJobScheduler.cs` — **create**
- **FILE-015**: `Inventory/Backgrounds/ReservationExpiryJobScheduler.Constants.cs` — **create**
- **FILE-016**: `Inventory/Backgrounds/ReservationExpiryJobScheduler.Result.cs` — **create**
- **FILE-017**: `Inventory/Backgrounds/ReservationExpiryJobScheduler.Loggers.cs` — **create**
- **FILE-018**: `Inventory/Services/ReservationExpiryService.cs` — **delete**
- **FILE-019**: `Inventory/Inventory.Extension.cs` — **refactor** DI registrations

## 6. Testing

- **TEST-001**: `dotnet build` succeeds with 0 warnings
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests` — existing tests pass (note: there are no specific tests for CartExpiryJob or ReservationExpiryService that would break)
- **TEST-003**: grep for `ReservationExpiryService` in `service/` returns zero matches in `.cs` source files

## 7. Risks & Assumptions

- **RISK-001**: Low — all changes are additive or refactors within module boundaries. The `ReservationExpiryJob` wraps the same `IStockReservationService.ExpireReservationsAndRestoreStockAsync` method that `ReservationExpiryService` called.
- **ASSUMPTION-001**: `ReservationExpiryJobScheduler` uses `*/1 * * * *` cron (every minute) matching the old `SweepInterval` of 60 seconds.
- **ASSUMPTION-002**: The only test referencing `ReservationExpiryService` is `Inventory/Services/ReservationExpiryServiceTests.cs` — it will need updating to test the new scheduler pattern instead. (But the plan doesn't cover test changes; this is a follow-up.)

## 8. Related Specifications / Further Reading

- `plan/refactor-cart-expiry-to-hangfire-recurring-1.md` — previous migration of CartExpiryService to Hangfire
- `docs/codebase/CONVENTIONS.md` — coding conventions for split files
