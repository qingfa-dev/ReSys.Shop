---
goal: Migrate 9 inline return Error.* calls to pre-defined *.Result.cs error factories
version: 1.0
date_created: 2026-07-19
owner: Engineering Standards
status: 'Completed'
tags: refactor, errors, result, factories, csharp
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Migrate 9 inline `return Error.*` calls (across 5 files in Payment, Catalog, Inventory modules) to use pre-defined error factory properties in `*.Result.cs` files. The codebase has 40+ existing `*.Result.cs` files with the pattern `public static class XxxResult { public static class Errors { public static Error Xxx => Error.Yyy(...); } }`. These 9 sites bypass that pattern and declare errors inline.

Target: zero inline `return Error.*` calls outside infrastructure mappers.

## 1. Requirements & Constraints

- **REQ-001**: All 9 inline `return Error.*` sites must be replaced with `XxxResult.Errors.Xxx` factory calls
- **REQ-002**: Add new error factories to existing `*.Result.cs` files where one exists (e.g., `StripeGatewayResult.Errors`, `StripeWebhookResult.Errors`, `StockItemResult.Errors`)
- **REQ-003**: Create new `*.Result.cs` files where none exists (e.g., `GatewayRegistry.Result.cs`, `SearchByImage.Result.cs`)
- **REQ-004**: Each error factory must be a `public static Error` property returning `Error.Yyy(code, message)` following the existing pattern
- **REQ-005**: Error code strings must be moved to constants in `*.Constant.cs` files or remain as string literals following existing pattern in the same Result file
- **REQ-006**: `dotnet build` must pass with TreatWarningsAsErrors=true after all changes
- **REQ-007**: Do NOT modify infrastructure mappers (`ValidationResult.Mapper.cs`, `IdentityResult.Mapper.cs`) — their inline `Error.*` usage is acceptable
- **PAT-001**: Follow existing pattern from `StripeGatewayResult.Errors`, `StripeWebhookResult.Errors`, `StockItemResult.Errors`

## 2. Implementation Steps

### Implementation Phase 1: Payment — GatewayRegistry.cs (1 inline Error.NotFound)

- GOAL-001: Create `GatewayRegistry.Result.cs` with `Errors.ProviderNotFound(string)`, replace inline call

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `service/Api/src/Module/Payment/Services/Provider/GatewayRegistry.Result.cs` — define `public static class GatewayRegistryResult` with `public static class Errors { public static Error ProviderNotFound(string providerKey) => Error.NotFound("Gateway.Provider.{providerKey}.NotFound", $"No gateway registered for provider '{providerKey}'."); }` | | |
| TASK-002 | Edit `GatewayRegistry.cs` line 27 — replace `return Error.NotFound(...)` with `return GatewayRegistryResult.Errors.ProviderNotFound(providerKey)` | | |

### Implementation Phase 2: Payment — StripeGateway.cs (3 inline Error.BadRequest)

- GOAL-002: Add `PurchaseNotSucceeded`, `AuthorizeNotRequiresCapture` factories to `StripeGatewayResult.Errors`; add `GatewayError(string,string)` for the dynamic path

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | Edit `StripeGateway.Result.cs` — add `public static Error PurchaseNotSucceeded(string status) => Error.BadRequest("Stripe.Purchase.NotSucceeded", $"Purchase status: {status}");` | | |
| TASK-004 | Edit `StripeGateway.Result.cs` — add `public static Error AuthorizeNotRequiresCapture(string status) => Error.BadRequest("Stripe.Authorize.NotRequiresCapture", $"Authorize status: {status}");` | | |
| TASK-005 | Edit `StripeGateway.Result.cs` — add `public static Error GatewayError(string code, string message) => Error.BadRequest($"Stripe.{code}", message);` (for `MapStripeException` dynamic path) | | |
| TASK-006 | Edit `StripeGateway.cs` line 48 — replace `Error.BadRequest(...)` with `StripeGatewayResult.Errors.PurchaseNotSucceeded(intent.Status)` | | |
| TASK-007 | Edit `StripeGateway.cs` line 67 — replace `Error.BadRequest(...)` with `StripeGatewayResult.Errors.AuthorizeNotRequiresCapture(intent.Status)` | | |
| TASK-008 | Edit `StripeGateway.cs` line 193 (`MapStripeException`) — replace `Error.BadRequest($"Stripe.{code}", msg)` with `StripeGatewayResult.Errors.GatewayError(code, msg)` | | |

### Implementation Phase 3: Payment — StripeWebhookDispatcher.cs + StripeWebhookService.cs (2 inline Error.Validation, same code)

- GOAL-003: Add `WebhookSecretNotConfigured` to `StripeWebhookResult.Errors`, replace both inline calls

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Edit `StripeWebhook.Result.cs` — add `public static Error WebhookSecretNotConfigured => Error.Validation("Stripe.WebhookSecret.NotConfigured", "Stripe webhook secret is not configured.");` | | |
| TASK-010 | Edit `StripeWebhookDispatcher.cs` line 49 — replace `return Error.Validation(...)` with `return StripeWebhookResult.Errors.WebhookSecretNotConfigured` | | |
| TASK-011 | Edit `StripeWebhookService.cs` line 44 — replace `return Error.Validation(...)` with `return StripeWebhookResult.Errors.WebhookSecretNotConfigured` | | |

### Implementation Phase 4: Catalog — SearchByImage.cs (2 inline Error.Validation)

- GOAL-004: Create `SearchByImage.Result.cs` with `Errors.FileTooLarge` and `Errors.InvalidContentType`, replace inline calls

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-012 | Create `service/Api/src/Module/Catalog/Features/Storefront/Products/SearchByImage/SearchByImage.Result.cs` — define `public static class SearchByImageResult` with `public static class Errors { public static Error FileTooLarge => Error.Validation("SearchByImage.FileTooLarge", "Image file must not exceed 10 MB."); public static Error InvalidContentType => Error.Validation("SearchByImage.InvalidContentType", "File must be an image."); }` | | |
| TASK-013 | Edit `SearchByImage.cs` line 41 — replace `return Error.Validation("SearchByImage.FileTooLarge", ...)` with `return SearchByImageResult.Errors.FileTooLarge` | | |
| TASK-014 | Edit `SearchByImage.cs` line 44 — replace `return Error.Validation("SearchByImage.InvalidContentType", ...)` with `return SearchByImageResult.Errors.InvalidContentType` | | |

### Implementation Phase 5: Inventory — ImportStockItems.cs (1 inline Error.Validation)

- GOAL-005: Add `ImportFileTooLarge` to `StockItemResult.Errors`, replace inline call

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | Edit `StockItem.Result.cs` — add `public static Error ImportFileTooLarge => Error.Validation(code: "StockItem.Import.FileTooLarge", message: "CSV file must not exceed 5 MB.");` in the `Errors` class alongside `ImportFileRequired` and `ImportEmptyFile` | | |
| TASK-016 | Edit `ImportStockItems.cs` line 29 — replace `return Error.Validation("StockItem.Import.FileTooLarge", "CSV file must not exceed 5 MB.")` with `return StockItemResult.Errors.ImportFileTooLarge` | | |

### Implementation Phase 6: Verification

- GOAL-006: Full build and test verification

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-017 | `dotnet build` — verify 0 warnings, 0 errors | | |
| TASK-018 | `dotnet test service/Api/tests/Module.UnitTests` — verify all tests pass | | |
| TASK-019 | `grep -rn "return Error\." service/Api/src/Module/ --include="*.cs" | grep -v "Result.cs"` — verify no remaining inline `return Error.*` outside Result.cs files in Module (infrastructure mappers in Shared excluded) | | |

## 3. Alternatives

- **ALT-001**: Leave dynamic-code errors (like StripeGateway's `MapStripeException`) as inline — rejected; the `GatewayError(string,string)` factory wraps even dynamic codes consistently
- **ALT-002**: Create one monolithic `PaymentErrors` class — rejected; existing per-domain pattern (`StripeGatewayResult`, `StripeWebhookResult`) is more maintainable

## 4. Dependencies

- **DEP-001**: Existing `Error` type from `Shared/Application/Models/Errors/Error.cs` with factory methods `Error.Validation()`, `Error.NotFound()`, `Error.BadRequest()`
- **DEP-002**: Existing `*.Result.cs` pattern files as reference

## 5. Files

- **FILE-001**: (new) `Module/Payment/Services/Provider/GatewayRegistry.Result.cs`
- **FILE-002**: `Module/Payment/Services/Provider/GatewayRegistry.cs` — replace inline
- **FILE-003**: `Module/Payment/Services/Provider/Stripe/StripeGateway.Result.cs` — add 3 factories
- **FILE-004**: `Module/Payment/Services/Provider/Stripe/StripeGateway.cs` — replace 3 inline calls
- **FILE-005**: `Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.Result.cs` — add 1 factory
- **FILE-006**: `Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.cs` — replace inline
- **FILE-007**: `Module/Payment/Services/Webhook/StripeWebhookService.cs` — replace inline
- **FILE-008**: (new) `Module/Catalog/Features/Storefront/Products/SearchByImage/SearchByImage.Result.cs`
- **FILE-009**: `Module/Catalog/Features/Storefront/Products/SearchByImage/SearchByImage.cs` — replace 2 inline calls
- **FILE-010**: `Module/Inventory/Domain/StockLocations/StockItems/StockItem.Result.cs` — add 1 factory
- **FILE-011**: `Module/Inventory/Features/Admin/StockItems/Import/ImportStockItems.cs` — replace inline

## 6. Testing

- **TEST-001**: `dotnet build` — 0 warnings
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests --no-build` — all tests pass
- **TEST-003**: Grep audit — zero `return Error.*` in Module/ outside Result.cs files

## 7. Risks & Assumptions

- **RISK-001**: StripeGateway's `MapStripeException` uses a dynamic error code from Stripe's API — the `GatewayError(string,string)` factory handles this dynamically but still routes through the Result class
- **ASSUMPTION-001**: All 5 source files with inline errors are in the Module assembly and can be modified independently
- **ASSUMPTION-002**: The `StripeWebhookResult` is in namespace `Module.Payment.Features.Storefront.Payment.Webhooks` which is accessible from `StripeWebhookService` (in `Module.Payment.Services.Webhook`) via the containing Module assembly

## 8. Related Specifications / Further Reading

- `plan/refactor-logger-messages-1.md` — similar refactoring pattern for logger calls
- `Module/Payment/Services/Provider/Stripe/StripeGateway.Result.cs` — existing Result pattern reference
- `Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.Result.cs` — existing Result pattern reference
- `Module/Inventory/Domain/StockLocations/StockItems/StockItem.Result.cs` — existing Result pattern reference
