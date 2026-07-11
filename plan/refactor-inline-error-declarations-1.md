---
goal: Replace all inline Error.Xxx("code", "message") calls in production code with pre-declared .Failure/.Errors properties
version: 1.0
date_created: 2026-07-11
status: Planned
tags: refactor, error-handling, Result-pattern
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

9 production source locations use `Error.Xxx("code", "message")` inline instead of referencing pre-declared `.Failure.Xxx` or `.Errors.Xxx` properties from the domain's Result class. This creates inconsistent error codes, doubles the maintenance surface, and prevents centralized error-code tracking.

## 1. Requirements & Constraints

- **REQ-001**: Every inline `Error.Xxx("code", "message")` call in production `src/` (excluding `.Result.cs` files and test code) must be replaced with a property/method from the corresponding Result class.
- **REQ-002**: Error codes must remain unchanged (backward compatibility for any API consumers that depend on exact strings).
- **CON-001**: Acceptable inline: framework mappers (`Validation.Behavior.cs`, `ValidationResult.Mapper.cs`, `IdentityResult.Mapper.cs`) and dynamic Stripe errors (`StripeGateway.cs:110`, `CreateSetupIntent.cs:63`) — these produce runtime error codes from external sources.
- **PAT-001**: Naming convention for new properties follows the existing class style:
  - `UserResult.Failure` (PascalCase, in `Shared.Security.Identity.Domain.Users`)
  - `UserProfileResult.Failure` (PascalCase, in `Module.Profile.Domain`)
  - `OrderResult.Errors` (PascalCase, in `Module.Ordering.Domain.Orders`)
  - `StockItemResult.Errors` (PascalCase, in `Module.Inventory.Domain.StockLocations.StockItems`)
  - `WebhookSubscriptionErrors.Failure` (PascalCase, in `Shared.Operational.Webhooks.Domain`)
- **GUD-001**: Use `ErrorType.Validation` for business-rule violations, `ErrorType.Forbidden` for authorization failures.

## 2. Implementation Steps

### Implementation Phase 1: Declare missing Failure properties

- GOAL-001: Add missing pre-declared Error properties to the five affected Result classes.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Add `SelfStatusToggle` to `UserResult.Failure` in `Shared/Security/Identity/Domain/Users/User.Result.cs`. Code: `User.Status.SelfToggle`, message: `Cannot toggle your own account status.`, type: Forbidden. Insert at end of `Failure` class (after `RevokeDenied`) | | |
| TASK-002 | Add `SelfDelete` to `UserResult.Failure` in `Shared/Security/Identity/Domain/Users/User.Result.cs`. Code: `User.Delete.Self`, message: `Cannot delete your own account.`, type: Forbidden. Insert after `SelfStatusToggle` | | |
| TASK-003 | Add `AccessDenied` to `UserProfileResult.Failure` in `Module/Profile/Domain/UserProfile.Result.cs`. Code: `Profile.Get.Permission`, message: `Cannot access another user's profile.`, type: Forbidden. Insert after `AuthRequired` | | |
| TASK-004 | Add `InvalidStatusForLineItemRemove` to `OrderResult.Errors` in `Module/Ordering/Domain/Orders/Order.Result.cs`. Code: `Order.RemoveLineItem.InvalidStatus`, message: `Line items can only be removed from Draft orders.`, type: Validation. Insert after `QuantityNotPositive` | | |
| TASK-005 | Add `InvalidStatusForDelete` to `OrderResult.Errors` in `Module/Ordering/Domain/Orders/Order.Result.cs`. Code: `Order.Delete.InvalidStatus`, message: `Only Draft or Expired orders can be deleted.`, type: Validation. Insert after `InvalidStatusForLineItemRemove` | | |
| TASK-006 | Add `ImportFileRequired` to `StockItemResult.Errors` in `Module/Inventory/Domain/StockLocations/StockItems/StockItem.Result.cs`. Code: `StockItem.Import.FileRequired`, message: `CSV file is required.`, type: Validation. Insert after `AlreadyExists` | | |
| TASK-007 | Add `ImportEmptyFile` to `StockItemResult.Errors` in `Module/Inventory/Domain/StockLocations/StockItems/StockItem.Result.cs`. Code: `StockItem.Import.EmptyFile`, message: `CSV file is empty.`, type: Validation. Insert after `ImportFileRequired` | | |
| TASK-008 | Add `UrlEmpty` to `WebhookSubscriptionErrors.Failure` in `Shared/Operational/Webhooks/Domain/WebhookSubscription.Result.cs`. Code: `Webhooks.Subscription.Url.Empty`, message: `URL must not be empty.`, type: Validation. Insert after `SecretHashRequired` | | |
| TASK-009 | Add `UrlInvalid` to `WebhookSubscriptionErrors.Failure` in `Shared/Operational/Webhooks/Domain/WebhookSubscription.Result.cs`. Code: `Webhooks.Subscription.Url.Invalid`, message: `URL must be a valid absolute URI.`, type: Validation. Insert after `UrlEmpty` | | |
| TASK-010 | Add `UrlScheme` to `WebhookSubscriptionErrors.Failure` in `Shared/Operational/Webhooks/Domain/WebhookSubscription.Result.cs`. Code: `Webhooks.Subscription.Url.Scheme`, message: `Only HTTPS URLs are allowed.`, type: Validation. Insert after `UrlInvalid` | | |
| TASK-011 | Add `UrlBlocked` to `WebhookSubscriptionErrors.Failure` in `Shared/Operational/Webhooks/Domain/WebhookSubscription.Result.cs`. Code: `Webhooks.Subscription.Url.Blocked`, message: `This hostname is not allowed.`, type: Validation. Insert after `UrlScheme` | | |
| TASK-012 | Add `UrlPrivate` to `WebhookSubscriptionErrors.Failure` in `Shared/Operational/Webhooks/Domain/WebhookSubscription.Result.cs`. Code: `Webhooks.Subscription.Url.Private`, message: `Private network addresses are not allowed.`, type: Validation. Insert after `UrlBlocked` | | |

### Implementation Phase 2: Replace inline Error calls

- GOAL-002: Replace every inline `Error.Xxx(...)` return with the corresponding Failure/Errors property.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | `Module/Identity/Features/Admin/Users/Status/ToggleUserStatus.cs:35` — replace `Error.Forbidden("User.Status.Self", ...)` with `UserResult.Failure.SelfStatusToggle` | | |
| TASK-014 | `Module/Identity/Features/Admin/Users/Delete/DeleteUser.cs:42` — replace `Error.Forbidden("User.Delete.Self", ...)` with `UserResult.Failure.SelfDelete` | | |
| TASK-015 | `Module/Profile/Features/Store/Profiles/Get/Detail/GetProfile.cs:26` — replace `Error.Forbidden("Profile.Get.Permission", ...)` with `UserProfileResult.Failure.AccessDenied` | | |
| TASK-016 | `Module/Ordering/Features/Admin/Orders/RemoveLineItem/RemoveOrderLineItem.cs:22` — replace `Error.Validation("Order.RemoveLineItem.InvalidStatus", ...)` with `OrderResult.Errors.InvalidStatusForLineItemRemove` | | |
| TASK-017 | `Module/Ordering/Features/Admin/Orders/Delete/DeleteOrder.cs:24` — replace `Error.Validation("Order.Delete.InvalidStatus", ...)` with `OrderResult.Errors.InvalidStatusForDelete` | | |
| TASK-018 | `Module/Inventory/Features/Admin/StockItems/Import/ImportStockItems.cs:21` — replace `Error.Validation("StockItem.Import.FileRequired", ...)` with `StockItemResult.Errors.ImportFileRequired` | | |
| TASK-019 | `Module/Inventory/Features/Admin/StockItems/Import/ImportStockItems.cs:27` — replace `Error.Validation("StockItem.Import.EmptyFile", ...)` with `StockItemResult.Errors.ImportEmptyFile` | | |
| TASK-020 | `Shared/Operational/Webhooks/Domain/WebhookUrlValidator.cs:24` — replace `Error.Validation("Webhooks.Subscription.Url.Empty", ...)` with `WebhookSubscriptionErrors.Failure.UrlEmpty` | | |
| TASK-021 | `Shared/Operational/Webhooks/Domain/WebhookUrlValidator.cs:27` — replace `Error.Validation("Webhooks.Subscription.Url.TooLong", ...)` with `WebhookSubscriptionErrors.Failure.UrlTooLong` | | |
| TASK-022 | `Shared/Operational/Webhooks/Domain/WebhookUrlValidator.cs:30` — replace `Error.Validation("Webhooks.Subscription.Url.Invalid", ...)` with `WebhookSubscriptionErrors.Failure.UrlInvalid` | | |
| TASK-023 | `Shared/Operational/Webhooks/Domain/WebhookUrlValidator.cs:33` — replace `Error.Validation("Webhooks.Subscription.Url.Scheme", ...)` with `WebhookSubscriptionErrors.Failure.UrlScheme` | | |
| TASK-024 | `Shared/Operational/Webhooks/Domain/WebhookUrlValidator.cs:36` — replace `Error.Validation("Webhooks.Subscription.Url.Blocked", ...)` with `WebhookSubscriptionErrors.Failure.UrlBlocked` | | |
| TASK-025 | `Shared/Operational/Webhooks/Domain/WebhookUrlValidator.cs:43` — replace `Error.Validation("Webhooks.Subscription.Url.Private", ...)` with `WebhookSubscriptionErrors.Failure.UrlPrivate` | | |

## 3. Alternatives

- **ALT-001**: Keep inline errors — rejected because it violates the codebase convention of centralized error declarations, creates duplicate maintenance, and prevents tooling from tracking all error codes.
- **ALT-002**: Use a single generic `Forbidden("access.denied", "")` instead of unique codes — rejected because error codes serve as machine-readable identifiers for API consumers; unique codes are required per REQ-002.

## 4. Dependencies

- **DEP-001**: The `Shared` assembly must compile first (contains `UserResult`, `WebhookSubscriptionErrors`) before `Module` assembly (contains `OrderResult`, `StockItemResult`, `UserProfileResult`). No cross-assembly issue since `Module` already depends on `Shared`.

## 5. Files

- **FILE-001**: `service/Api/src/Shared/Security/Identity/Domain/Users/User.Result.cs` — add 2 properties (TASK-001, TASK-002)
- **FILE-002**: `service/Api/src/Module/Profile/Domain/UserProfile.Result.cs` — add 1 property (TASK-003)
- **FILE-003**: `service/Api/src/Module/Ordering/Domain/Orders/Order.Result.cs` — add 2 properties (TASK-004, TASK-005)
- **FILE-004**: `service/Api/src/Module/Inventory/Domain/StockLocations/StockItems/StockItem.Result.cs` — add 2 properties (TASK-006, TASK-007)
- **FILE-005**: `service/Api/src/Shared/Operational/Webhooks/Domain/WebhookSubscription.Result.cs` — add 5 properties (TASK-008 through TASK-012)
- **FILE-006**: `service/Api/src/Module/Identity/Features/Admin/Users/Status/ToggleUserStatus.cs` — 1 replacement (TASK-013)
- **FILE-007**: `service/Api/src/Module/Identity/Features/Admin/Users/Delete/DeleteUser.cs` — 1 replacement (TASK-014)
- **FILE-008**: `service/Api/src/Module/Profile/Features/Store/Profiles/Get/Detail/GetProfile.cs` — 1 replacement (TASK-015)
- **FILE-009**: `service/Api/src/Module/Ordering/Features/Admin/Orders/RemoveLineItem/RemoveOrderLineItem.cs` — 1 replacement (TASK-016)
- **FILE-010**: `service/Api/src/Module/Ordering/Features/Admin/Orders/Delete/DeleteOrder.cs` — 1 replacement (TASK-017)
- **FILE-011**: `service/Api/src/Module/Inventory/Features/Admin/StockItems/Import/ImportStockItems.cs` — 2 replacements (TASK-018, TASK-019)
- **FILE-012**: `service/Api/src/Shared/Operational/Webhooks/Domain/WebhookUrlValidator.cs` — 6 replacements (TASK-020 through TASK-025)

## 6. Testing

- **TEST-001**: Run `dotnet build` — must pass with zero warnings (TreatWarningsAsErrors).
- **TEST-002**: Run `dotnet test` — all existing tests must pass unchanged (error codes preserved exactly).

## 7. Risks & Assumptions

- **RISK-001**: Any error code change would break existing API consumers. Mitigation: REQ-002 mandates exact code preservation; verify by grep for each old code string.
- **ASSUMPTION-001**: No other callers reference the old inline error codes stringly (verified via pre-refactor grep `rg '"User\.Status\.Self|User\.Delete\.Self|Profile\.Get\.Permission|Order\.RemoveLineItem\.InvalidStatus|Order\.Delete\.InvalidStatus|StockItem\.Import\.[FileRequired|EmptyFile]|Webhooks\.Subscription\.Url\.'`).

## 8. Related Specifications / Further Reading

- `docs/codebase/CONVENTIONS.md` — Result pattern conventions
- `service/Api/src/Shared/Application/Models/Results/Error.cs` — Error type definition
- AGENTS.md §Code Organization — assembly dependency order (Shared → Module → Api)
