---
goal: Migrate 16 direct _logger.Log* calls to [LoggerMessage] source-generated patterns across 10 files
version: 1.0
date_created: 2026-07-19
owner: Engineering Standards
status: 'Completed'
tags: refactor, logging, source-generators, csharp, standards
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Migrate all 16 direct `_logger.Log*` calls (found across 10 files in Payment, Ordering, Identity, Inventory, Shared modules) to use the well-established `[LoggerMessage]` source-generated pattern already present in 66 files across the codebase. The pattern uses `static partial class XxxLoggers` with `[LoggerMessage]` attributes, called as `XxxLoggers.Method(_logger, ...)` or via extension methods on `ILogger`.

Current state audit: 16 raw `_logger.Log*` calls spread across 10 files. Target: 0 raw calls — all routed through source-generated logger partial classes.

## 1. Requirements & Constraints

- **REQ-001**: All 16 direct `_logger.Log*` calls must be replaced with `[LoggerMessage]` source-generated pattern calls
- **REQ-002**: Each `[LoggerMessage]` must have a unique `EventId` integer — use the existing codebase convention (EventId ranges: 1000-1999 Shared, 2000-2999 Catalog, 3000-3999 Ordering, 4000-4999 Inventory, 5000-5999 Payment, 6000-6999 Profile, 7000-7999 Identity, 8000-8999 Location, 9000-9999 Shipping)
- **REQ-003**: Each `[LoggerMessage]` must specify `Level` and `Message` — structured logging template syntax with `{Placeholder}` parameters
- **REQ-004**: Where a `XxxLoggers` partial class already exists for the domain (e.g., `OrderLoggers`, `PaymentCaptureLoggers`, `UserLoggers`, `DatabaseInitializer.Loggers`), prefer adding the new method there rather than creating a new file
- **REQ-005**: Where no existing Loggers class is suitable, create a new adjecent `{ClassName}.Loggers.cs` file with a `static partial class {ClassName}Loggers`
- **REQ-006**: `dotnet build` must pass with TreatWarningsAsErrors=true after all changes
- **REQ-007**: `dotnet test` must pass after all changes
- **CON-001**: Do NOT modify test files
- **CON-002**: Do NOT change log message templates — preserve the exact Wording and structure parameters
- **PAT-001**: Follow existing `[LoggerMessage]` pattern from `Order.Loggers.cs`, `User.Loggers.cs`, `PaymentCapture.Loggers.cs` (EventId, Level, Message template, public static partial void Method(ILogger logger, ...))
- **PAT-002**: Use `public static partial class` for Loggers classes, matching the existing codebase convention (not `internal` unless the parent class is sealed)

## 2. Implementation Steps

### Implementation Phase 1: Payment Module — ProcessStripeWebhookEventJob (3 calls)

- GOAL-001: Create `ProcessStripeWebhookEventJobLoggers` with 3 `[LoggerMessage]` methods for the LogWarning calls, or add to existing `PaymentCaptureLoggers`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `service/Api/src/Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.Loggers.cs` — define `public static partial class ProcessStripeWebhookEventJobLoggers` with 3 `[LoggerMessage(EventId=5006..5008, Level=LogLevel.Warning)]` methods: `CannotCompletePayment(ILogger logger, Guid PaymentId, ...)`, `CannotFailPayment(...)`, `CannotRefundPayment(...)` | | |
| TASK-002 | Edit `ProcessStripeWebhookEventJob.cs` — replace 3 `_logger.LogWarning(...)` calls with `ProcessStripeWebhookEventJobLoggers.CannotCompletePayment(_logger, ...)` etc. | | |

### Implementation Phase 2: Payment Module — StripeWebhookService (1 call)

- GOAL-002: Create `StripeWebhookHandlerLoggers` with 1 `[LoggerMessage]` method, or add to existing `PaymentCaptureLoggers`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | Create `service/Api/src/Module/Payment/Services/Webhook/StripeWebhookService.Loggers.cs` — define `public static partial class StripeWebhookHandlerLoggers` with `[LoggerMessage(EventId=5009, Level=LogLevel.Error, Message="Stripe event parse failed: {Payload}")] EventParseFailed(ILogger logger, Exception ex, string Payload)` | | |
| TASK-004 | Edit `StripeWebhookService.cs` — replace `_logger.LogError(ex, "Stripe event parse failed: {Payload}", payload)` with `StripeWebhookHandlerLoggers.EventParseFailed(_logger, ex, payload)` | | |

### Implementation Phase 3: Payment Module — StripeWebhookDispatcher (2 calls)

- GOAL-003: Create `StripeWebhookDispatcherLoggers` with 2 `[LoggerMessage]` methods

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Create `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.Loggers.cs` — define `public static partial class StripeWebhookDispatcherLoggers` with `[LoggerMessage(EventId=5010, Level=LogLevel.Warning)] SignatureValidationFailed(ILogger logger, Exception ex)` and `[LoggerMessage(EventId=5011, Level=LogLevel.Error)] EventParseFailed(ILogger logger, Exception ex, string Payload)` | | |
| TASK-006 | Edit `StripeWebhookDispatcher.cs` — replace 2 `_logger.Log*` calls with `StripeWebhookDispatcherLoggers.*(_logger, ...)` | | |

### Implementation Phase 4: Ordering Module — CancelOrder (2 calls)

- GOAL-004: Add 2 `[LoggerMessage]` methods to existing `OrderLoggers` in `Ordering/Domain/Orders/Order.Loggers.cs`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | Edit `service/Api/src/Module/Ordering/Domain/Orders/Order.Loggers.cs` — add `[LoggerMessage(EventId=3010, Level=LogLevel.Warning)] VoidPaymentsFailed(ILogger logger, Guid OrderId, string Errors)` and `[LoggerMessage(EventId=3011, Level=LogLevel.Warning)] CancelNotificationFailed(ILogger logger, Guid OrderId, string Errors)` | | |
| TASK-008 | Edit `service/Api/src/Module/Ordering/Features/Storefront/Orders/Cancel/CancelOrder.cs` — replace 2 `_logger.LogWarning(...)` calls with `OrderLoggers.VoidPaymentsFailed(_logger, ...)` and `OrderLoggers.CancelNotificationFailed(_logger, ...)` | | |

### Implementation Phase 5: Ordering Module — CancelOrderAdmin (2 calls)

- GOAL-005: Add 2 methods to existing `OrderLoggers` (same methods as Phase 4 — already added in TASK-007)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Edit `service/Api/src/Module/Ordering/Features/Admin/Orders/Cancel/CancelOrderAdmin.cs` — replace 2 `_logger.LogWarning(...)` calls with `OrderLoggers.VoidPaymentsFailed(_logger, ...)` and `OrderLoggers.CancelNotificationFailed(_logger, ...)` | | |

### Implementation Phase 6: Ordering Module — CreateOrderFromCart (1 call)

- GOAL-006: Add 1 `[LoggerMessage]` method to existing `OrderLoggers`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | Edit `Order.Loggers.cs` — add `[LoggerMessage(EventId=3012, Level=LogLevel.Warning)] ConfirmationNotificationFailed(ILogger logger, Guid OrderId, string Errors)` | | |
| TASK-011 | Edit `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` — replace `_logger.LogWarning(...)` with `OrderLoggers.ConfirmationNotificationFailed(_logger, ...)` | | |

### Implementation Phase 7: Ordering Module — ResumeOrder (1 call)

- GOAL-007: Add 1 `[LoggerMessage]` method to existing `OrderLoggers`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-012 | Edit `Order.Loggers.cs` — add `[LoggerMessage(EventId=3013, Level=LogLevel.Warning)] ResumeNotificationFailed(ILogger logger, Guid OrderId, string Errors)` | | |
| TASK-013 | Edit `service/Api/src/Module/Ordering/Features/Admin/Orders/Resume/ResumeOrder.cs` — replace `_logger.LogWarning(...)` with `OrderLoggers.ResumeNotificationFailed(_logger, ...)` | | |

### Implementation Phase 8: Identity Module — RequestPasswordReset (1 call)

- GOAL-008: Add 1 `[LoggerMessage]` method to existing `UserLoggers`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-014 | Edit `service/Api/src/Shared/Security/Identity/Domain/Users/User.Loggers.cs` — add `[LoggerMessage(EventId=1033, Level=LogLevel.Warning)] PasswordResetNotificationFailed(ILogger logger, Guid UserId, Exception ex)` inside the `Passwords` nested static class | | |
| TASK-015 | Edit `service/Api/src/Module/Identity/Features/Store/Passwords/Forgot/RequestPasswordReset.cs` — replace `_logger.LogWarning(ex, "Failed to send password reset notification to {UserId}", ...)` with `UserLoggers.Passwords.PasswordResetNotificationFailed(_logger, user.Id, ex)` | | |

### Implementation Phase 9: Inventory Module — ImportStockItems (1 call)

- GOAL-009: Create `ImportStockItemsLoggers` with 1 `[LoggerMessage]` method

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-016 | Create `service/Api/src/Module/Inventory/Features/Admin/StockItems/Import/ImportStockItems.Loggers.cs` — define `public static partial class ImportStockItemsLoggers` with `[LoggerMessage(EventId=4001, Level=LogLevel.Debug, Message="[StockItem.Import]: Created {Created}, Updated {Updated}, Failed {Failed}")] ImportCompleted(ILogger logger, int Created, int Updated, int Failed)` | | |
| TASK-017 | Edit `ImportStockItems.cs` — replace `_logger.LogDebug(...)` with `ImportStockItemsLoggers.ImportCompleted(_logger, created, updated, errors.Count)` | | |

### Implementation Phase 10: Shared Module — DatabaseInitializerHostedService (2 calls)

- GOAL-010: Create `DatabaseInitializerHostedServiceLoggers` with 2 `[LoggerMessage]` methods — or add to existing `DatabaseInitializer.Loggers`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-018 | Create `service/Api/src/Shared/Operational/Persistence/Initializers/DatabaseInitializerHostedService.Loggers.cs` — define `public static partial class DatabaseInitializerHostedServiceLoggers` with `[LoggerMessage(EventId=265, Level=LogLevel.Information)] InitializationComplete(ILogger logger)` and `[LoggerMessage(EventId=266, Level=LogLevel.Critical)] InitializationFailed(ILogger logger, Exception ex)` | | |
| TASK-019 | Edit `DatabaseInitializerHostedService.cs` — replace `logger.LogInformation("Database initialization complete.")` with `DatabaseInitializerHostedServiceLoggers.InitializationComplete(logger)` and `logger.LogCritical(ex, "Database initialization failed.")` with `DatabaseInitializerHostedServiceLoggers.InitializationFailed(logger, ex)` | | |

### Implementation Phase 11: Verification

- GOAL-011: Full build and test verification

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-020 | `dotnet build` — verify 0 warnings, 0 errors | | |
| TASK-021 | `dotnet test service/Api/tests/Module.UnitTests` — verify all tests pass | | |
| TASK-022 | `grep -r "_logger\.Log\|logger\.Log" service/Api/src/ --include="*.cs"` — verify 0 remaining direct `_logger.Log*` calls in non-Loggers files | | |

## 3. Alternatives

- **ALT-001**: Create one monolithic `PaymentLoggers` / `OrderingLoggers` class shared across modules — rejected because the existing pattern is per-domain/per-entity loggers (e.g., `OrderLoggers`, `PaymentCaptureLoggers`)
- **ALT-002**: Use extension methods on `ILogger` (Style A) instead of static methods taking `ILogger` (Style B) — both exist in the codebase; this plan chooses Style B for new loggers to match the majority pattern (48 of 66 existing files use Style B)
- **ALT-003**: Add all Ordering methods to `OrderLoggers` instead of creating per-handler Loggers files — chosen because `OrderLoggers` already exists and the messages are order-domain-specific

## 4. Dependencies

- **DEP-001**: `Microsoft.Extensions.Logging.Abstractions` — provides `[LoggerMessage]` attribute (net10.0 built-in)
- **DEP-002**: `dotnet build` with TreatWarningsAsErrors=true — must pass after migration

## 5. Files

- **FILE-001**: `Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.cs` — replace 3 `LogWarning` calls
- **FILE-002**: (new) `Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.Loggers.cs` — 3 `[LoggerMessage]` methods
- **FILE-003**: `Module/Payment/Services/Webhook/StripeWebhookService.cs` — replace 1 `LogError` call
- **FILE-004**: (new) `Module/Payment/Services/Webhook/StripeWebhookService.Loggers.cs` — 1 `[LoggerMessage]` method
- **FILE-005**: `Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.cs` — replace 2 `Log*` calls
- **FILE-006**: (new) `Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.Loggers.cs` — 2 `[LoggerMessage]` methods
- **FILE-007**: `Module/Ordering/Domain/Orders/Order.Loggers.cs` — add 4 `[LoggerMessage]` methods
- **FILE-008**: `Module/Ordering/Features/Storefront/Orders/Cancel/CancelOrder.cs` — replace 2 `LogWarning` calls
- **FILE-009**: `Module/Ordering/Features/Admin/Orders/Cancel/CancelOrderAdmin.cs` — replace 2 `LogWarning` calls
- **FILE-010**: `Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` — replace 1 `LogWarning` call
- **FILE-011**: `Module/Ordering/Features/Admin/Orders/Resume/ResumeOrder.cs` — replace 1 `LogWarning` call
- **FILE-012**: `Shared/Security/Identity/Domain/Users/User.Loggers.cs` — add 1 `[LoggerMessage]` method
- **FILE-013**: `Module/Identity/Features/Store/Passwords/Forgot/RequestPasswordReset.cs` — replace 1 `LogWarning` call
- **FILE-014**: `Module/Inventory/Features/Admin/StockItems/Import/ImportStockItems.cs` — replace 1 `LogDebug` call
- **FILE-015**: (new) `Module/Inventory/Features/Admin/StockItems/Import/ImportStockItems.Loggers.cs` — 1 `[LoggerMessage]` method
- **FILE-016**: `Shared/Operational/Persistence/Initializers/DatabaseInitializerHostedService.cs` — replace 2 direct calls
- **FILE-017**: (new) `Shared/Operational/Persistence/Initializers/DatabaseInitializerHostedService.Loggers.cs` — 2 `[LoggerMessage]` methods

## 6. Testing

- **TEST-001**: `dotnet build` — 0 warnings (TreatWarningsAsErrors=true)
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests` — all tests pass
- **TEST-003**: `grep -rn "_logger\.Log\|logger\.Log" service/Api/src/ --include="*.cs" | grep -v "/obj/" | grep -v "Loggers.cs"` — verify zero remaining direct logger calls outside Loggers files

## 7. Risks & Assumptions

- **RISK-001**: Duplicate `EventId` values — mitigation: use EventId range 5006-5011 for Payment (avoiding conflicts with existing 2000-2003 in PaymentCaptureLoggers), 3010-3013 for Ordering (avoiding 3000-3003 in OrderLoggers), 1033 for UserLoggers.Passwords, 4001 for Inventory, 265-266 for Shared DatabaseInitializer
- **RISK-002**: The `_logger.LogWarning(ex, ...)` overload with Exception parameter must map to `[LoggerMessage]` with `Exception` as a special named parameter — the source generator handles this via `Exception ex` in the method signature
- **ASSUMPTION-001**: All 10 files compile independently with no cross-file dependencies that would break after refactoring
- **ASSUMPTION-002**: The `static partial class` pattern works for both standalone classes and nested classes — the codebase already uses both (`ProductLoggers` is standalone; `Loggers` inside `CartExpiryService` is nested)

## 8. Related Specifications / Further Reading

- `guide/code-commenting/CommentingRules.xml` — CAT-9 Observability label conventions
- `module/Ordering/Domain/Orders/Order.Loggers.cs` — existing `[LoggerMessage]` pattern reference
- `module/Catalog/Domain/Products/Product.Loggers.cs` — existing `[LoggerMessage]` pattern reference (standalone)
- `shared/Operational/Persistence/Initializers/DatabaseInitializer.Logger.cs` — existing `[LoggerMessage]` pattern reference (nested)
