---
goal: Remove unused outbound webhook delivery (OrderPlacedDeliveryJob, OutboundWebhookOptions, config)
version: 1.0
date_created: 2026-07-11
status: Planned
tags: refactor, cleanup, ordering
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Remove the `OrderPlacedDeliveryJob` Hangfire job, `OutboundWebhookOptions` config, and all wiring — there are zero external consumers configured, making this speculative code. Simplifies the Ordering module by ~80 LOC and removes an unused HTTP client factory dependency.

## 1. Requirements & Constraints

- **REQ-001**: Remove `OrderPlacedDeliveryJob.cs` and `OrderPlacedDeliveryJobDefaults`
- **REQ-002**: Remove `OutboundWebhookOptions.cs` and `OutboundWebhookOptions.Extensions.cs`
- **REQ-003**: Remove `Webhooks:Outbound` config section from `appsettings.json` and `appsettings.Development.json`
- **REQ-004**: Remove `OrderPlacedDeliveryJob` DI registration and `AddOutboundWebhooks()` call from `Ordering.Extension.cs`
- **REQ-005**: Remove `backgroundJobClient.Enqueue<OrderPlacedDeliveryJob>(...)` and Hangfire enqueue from `CreateOrderFromCart.cs`
- **REQ-006**: Remove unused `using` directives (Hangfire, `Jobs` namespace, `OutboundWebhookOptions` references)
- **CON-001**: Do not remove `IBackgroundJobClient` from `CreateOrderFromCart.cs` — it may be used for other jobs in the future
- **CON-002**: Keep `NullOrderEventPublisher` and `IOrderEventPublisher` interface — they may be needed for future internal event handling

## 2. Implementation Steps

### Implementation Phase 1: Delete source files

- GOAL-001: Remove the two job/options files and their DI registration

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Delete `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/Jobs/OrderPlacedDeliveryJob.cs` | | |
| TASK-002 | Delete `service/Api/src/Module/Ordering/Infrastructure/Options/OutboundWebhookOptions.cs` | | |
| TASK-003 | Delete `service/Api/src/Module/Ordering/Infrastructure/Options/OutboundWebhookOptions.Extensions.cs` | | |

### Implementation Phase 2: Clean up config

- GOAL-002: Remove `Webhooks:Outbound` from appsettings files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | Remove lines 239-244 (`"Webhooks": { "Outbound": ... }`) from `service/Api/src/Api/appsettings.json` | | |
| TASK-005 | Remove lines 77-82 (`"Webhooks": { "Outbound": ... }`) from `service/Api/src/Api/appsettings.Development.json` | | |

### Implementation Phase 3: Clean up DI and handler

- GOAL-003: Remove all references to deleted types from `Ordering.Extension.cs` and `CreateOrderFromCart.cs`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | In `Ordering.Extension.cs`: remove line 6 (`using Module.Ordering.Features.Storefront.Cart.Checkout.Jobs`), line 7 (`using Module.Ordering.Infrastructure.Options`), line 25 (`builder.AddOutboundWebhooks()`), line 29 (`builder.Services.AddScoped<OrderPlacedDeliveryJob>()`) | | |
| TASK-007 | In `CreateOrderFromCart.cs`: remove line 1 (`using Hangfire`), line 9 (`using Module.Ordering.Features.Storefront.Cart.Checkout.Jobs`), lines 149-154 (the `Enqueue<OrderPlacedDeliveryJob>` block and its comment) | | |
| TASK-008 | In `CreateOrderFromCart.cs`: remove `IBackgroundJobClient backgroundJobClient` from the `CommandHandler` constructor (line 29) and its corresponding field/parameter | | |

### Implementation Phase 4: Verify

- GOAL-004: Ensure build passes and no stale references remain

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Run `dotnet build service/Api/src/Api` to verify no compilation errors | | |
| TASK-010 | Run `dotnet test service/Api/tests/Module.UnitTests` to verify tests still pass | | |
| TASK-011 | Search codebase for remaining references to `OrderPlacedDeliveryJob`, `OutboundWebhook`, or `OutboundWebhookOptions` | | |

## 3. Alternatives

- **ALT-001**: Keep the code disabled-by-default for future use. Rejected — YAGNI principle; adds maintenance overhead and unused HTTP infrastructure. Re-creating a simple Hangfire job is trivial when an actual consumer appears.
- **ALT-002**: Extract to a separate `Module.Webhooks` assembly. Rejected — the whole point of the previous refactor was to remove that module.

## 4. Dependencies

- **DEP-001**: None — this is a pure deletion with no upstream consumers

## 5. Files

- **FILE-001**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/Jobs/OrderPlacedDeliveryJob.cs` — **delete**
- **FILE-002**: `service/Api/src/Module/Ordering/Infrastructure/Options/OutboundWebhookOptions.cs` — **delete**
- **FILE-003**: `service/Api/src/Module/Ordering/Infrastructure/Options/OutboundWebhookOptions.Extensions.cs` — **delete**
- **FILE-004**: `service/Api/src/Api/appsettings.json` — remove Webhooks config block
- **FILE-005**: `service/Api/src/Api/appsettings.Development.json` — remove Webhooks config block
- **FILE-006**: `service/Api/src/Module/Ordering/Ordering.Extension.cs` — remove imports and DI registrations
- **FILE-007**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` — remove Hangfire import, job enqueue, and `IBackgroundJobClient` from constructor

## 6. Testing

- **TEST-001**: `dotnet build` succeeds with no warnings or errors
- **TEST-002**: Existing unit tests pass (`dotnet test service/Api/tests/Module.UnitTests`)
- **TEST-003**: grep for `OrderPlacedDeliveryJob`, `OutboundWebhookOptions`, `OutboundWebhook` returns zero matches in `service/` source files

## 7. Risks & Assumptions

- **RISK-001**: None — code is disabled by default (`Enabled: false`) and has no consumers
- **ASSUMPTION-001**: No external system is actively receiving `order.placed` webhooks from this deployment
- **ASSUMPTION-002**: Future webhook consumers will be added by re-creating a simple HTTP POST job, not by restoring the removed Webhooks module

## 8. Related Specifications / Further Reading

- `spec/design-remove-webhooks-module.md` — original design that introduced this code
- `docs/superpowers/plans/2026-07-11-remove-webhooks-module.md` — implementation plan that created it
