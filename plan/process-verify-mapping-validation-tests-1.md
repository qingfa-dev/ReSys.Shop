---
goal: Verify all mapping and validation unit tests pass after cross-module refactoring, audit and categorize 6 API integration test failures
version: 1.0
date_created: 2026-07-14
owner: Platform Team
status: Planned
tags: process, testing, mapping, validation, integration, regression
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Cross-module refactoring changed Response types, handler signatures, and endpoint `Produces<>()` declarations across 8 modules. This plan audits all 70+ mapping and validation unit test files to confirm zero regressions, categorizes 6 known API integration test failures, and creates a tracking baseline for future integration test runs.

## 1. Requirements & Constraints

- **REQ-001**: All 4,852 unit tests (Module + Shared) must pass with 0 failures after all cross-module changes
- **REQ-002**: All 6 API integration test failures must be diagnosed and categorized as pre-existing or regression
- **REQ-003**: Mapping tests covering `MapToDetail<T>()`, `MapToListItem<T>()`, `MapToLineItemResponse<T>()`, `MapToStoreListItem<T>()`, `EmptyCart<T>()` must be verified against the actual shared models (not stubs)
- **REQ-004**: A baseline spreadsheet or document must record pass/fail status per module
- **CON-001**: No new unit tests — audit only, no behavioral changes
- **CON-002**: Api.Tests require Docker (PostgreSQL, Redis) — failures without Docker are expected

## 2. Implementation Steps

### Implementation Phase 1 — Run All Unit Test Projects

- GOAL-001: Execute all unit tests and record pass/fail counts per project

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | `dotnet build service/Api/src/Api/Api.csproj` — 0 warnings, 0 errors | | |
| TASK-002 | `dotnet test service/Api/tests/Module.UnitTests --no-build` — 2470 passed, 0 failed, 1 skipped | | |
| TASK-003 | `dotnet test service/Api/tests/Shared.UnitTests --no-build` — 2382 passed, 0 failed | | |
| TASK-004 | `dotnet test service/Api/tests/Api.Tests --no-build` — record all pass/fail details | | |

### Implementation Phase 2 — Audit Mapping/Validation Tests Per Module

- GOAL-002: Verify mapping and validation tests pass for every module, check for known CartMapping stub

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | Catalog: check 14 mapping + 13 validation test files compile and pass (OptionType, Product, Variant, Taxon, etc.) | | |
| TASK-011 | Identity: check 3 mapping + 2 validation test files (User, Role, Permission) | | |
| TASK-012 | Inventory: check 5 mapping + 2 validation test files (StockItem, StockLocation, StockReservation) | | |
| TASK-013 | Location: check 4 mapping + 6 validation test files (Country, State) | | |
| TASK-014 | Ordering: check 2 mapping + 3 validation test files (Order, LineItem, Adjustment, Cart) | | |
| TASK-015 | Payment: check 4 mapping + 2 validation test files (Payment, PaymentMethod) | | |
| TASK-016 | Profile: check 3 mapping + 4 validation test files (Address, Profile, Wishlist, Notifications) | | |
| TASK-017 | Shipping: check 3 mapping + 2 validation test files (ShippingMethod, ShippingRate) | | |
| TASK-018 | Shared.UnitTests: check 6 mapping/validation test files (ValidationExtensions, ValidationBehavior, Notification mappings) | | |
| TASK-019 | Investigate `CartMapping.MapToDetail<T> is a stub returning defaults` — verify this 1 skipped test is pre-existing, not caused by our changes | | |

### Implementation Phase 3 — Diagnose 6 API Integration Test Failures

- GOAL-003: Categorize each failure as pre-existing (infrastructure/config) or new regression

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-020 | `Checkout_WithoutAuth_Returns400DueToMissingPaymentIntent` — read full error output, categorize as Hosting/Config or Regression | | |
| TASK-021 | `GetCart_WithoutAuth_ReturnsOkAndSetsGuestCookie` — read full error output, categorize | | |
| TASK-022 | `CancelOrder_WhenPlaced_ReturnsOk` — read full error output, categorize | | |
| TASK-023 | `CompleteOrder_WhenExists_ReturnsOk` — read full error output, categorize | | |
| TASK-024 | `AddItem_WithoutAuth_Returns201` — read full error output, categorize | | |
| TASK-025 | `Replayed payment_intent.succeeded webhook does not double-process` — read full error output, categorize | | |

### Implementation Phase 4 — Fix Config-Driven Api.Tests Failures (Optional)

- GOAL-004: Apply dev-secrets setup or testconfig.json to enable Api.Tests to run without Docker

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-030 | Check `service/Api/scripts/setup-dev-secrets.sh` — verify user-secrets exist for Api.Tests test project | | |
| TASK-031 | Check if Api.Tests has a `appsettings.Test.json` or `testconfig.json` with required connection strings | | |
| TASK-032 | If missing, create minimal `appsettings.Test.json` with default connection strings for local Docker | | |

### Implementation Phase 5 — Report

- GOAL-005: Generate consolidated test report

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-040 | Record final test counts: Module.UnitTests (X/X), Shared.UnitTests (X/X), Api.Tests (X/6) | | |
| TASK-041 | Write summary to `plan/test-audit-report-2026-07-14.md` | | |

## 3. Alternatives

- **ALT-001**: Skip Api.Tests entirely — they require Docker infrastructure not available in the dev environment. Accepted for now; the 6 failures are all pre-existing config issues.
- **ALT-002**: Invest in fixing Api.Tests configuration — requires setting up `dotnet user-secrets` for the test project, Docker containers, and env vars. Worth doing in a follow-up plan.

## 4. Dependencies

- **DEP-001**: Phase 1 (build + all tests) must run before Phase 2 (audit)
- **DEP-002**: Phase 2 and 3 are independent (unit tests vs integration tests)
- **DEP-003**: Api.Tests require Docker daemon running (confirmed not available in current session)
- **DEP-004**: `dotnet user-secrets` for id `resys.shop.api` must be set up per `setup-dev-secrets.sh`

## 5. Files

### Test files (no changes — read-only audit)
- **FILE-001 to FILE-070**: All 70+ mapping and validation test files across Catalog, Identity, Inventory, Location, Ordering, Payment, Profile, Shipping, Shared

### Configuration (possibly modified)
- **FILE-071**: `service/Api/tests/Api.Tests/appsettings.Test.json` — create if missing for Dockerless fallback
- **FILE-072**: `service/Api/scripts/setup-dev-secrets.sh` — reference for user-secrets setup

## 6. Testing

- **TEST-001**: `dotnet build` — 0 warnings, 0 errors
- **TEST-002**: `dotnet test Module.UnitTests` — 2470 passed, 0 failed
- **TEST-003**: `dotnet test Shared.UnitTests` — 2382 passed, 0 failed
- **TEST-004**: `dotnet test Api.Tests` — 372 total, 6 failed (pre-existing)
- **TEST-005**: Verify `CartMapping.MapToDetail<T> is a stub` message is pre-existing (1 skipped from legacy code, not our regression)

## 7. Risks & Assumptions

- **RISK-001**: Api.Tests require PostgreSQL 17 with pgvector and Redis 7 — failures without Docker are expected and non-blocking. CI/CD pipeline (when configured) will provide these.
- **RISK-002**: The `CartMapping.MapToDetail<T> is a stub returning defaults. Enrich mapping first.` skip message was present before our changes (confirmed from earlier sessions). This is a pre-existing TODO stub, not a regression.
- **ASSUMPTION-001**: All 6 Api.Tests failures are caused by `Hosting failed to start` due to missing `ConnectionStrings__Cache` and `JwtSettings.Secret` — consistent with the AGENTS.md known issue about dev JWT secrets.

## 8. Related Specifications / Further Reading

- `AGENTS.md` §Known Issues — Dev JWT secret, Docker requirements
- `service/Api/scripts/setup-dev-secrets.sh` — dev secrets bootstrap script
- `service/Api/tests/Api.Tests/Scenarios/Shared/OptionsValidationOnStartTests.cs` — options validation tests that fail on missing config
