---
goal: Clean up Ordering module Result classes — naming, regions, XML comments, missing success/error definitions, and cross-module violations
version: 1.0
date_created: 2026-07-13
status: Planned
tags: refactor, ordering, results, constants, cross-module
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Standardize the 4 Result classes and 4 Constant classes in the Ordering module. Fix naming inconsistency (`Failure` vs `Errors`), add `#region` groupings, fill missing XML comments, add missing success/error factory methods for uncovered admin/storefront operations, and eliminate the cross-module `using Module.Catalog` reference in `Order.Constant.cs`. Cross-module violations in feature handler files (25 detected) are out of scope for this plan — addressed separately.

## 1. Requirements & Constraints

- **REQ-001**: All 4 Result classes must use the same nested class name for errors (`Errors`, not a mix of `Failure` and `Errors`)
- **REQ-002**: Every `public static Error` property/method must have a `<summary>` XML comment explaining the failure case
- **REQ-003**: Every `public static string` success method must have a `<summary>` XML comment
- **REQ-004**: Every Result file must use `#region` blocks to group: existence errors, validation errors, state errors, auth errors
- **REQ-005**: Every admin/storefront handler that mutates state must have a corresponding `OrderResult.Success.Xxx` or `LineItemResult.Success.Xxx` method
- **REQ-006**: `Order.Constant.cs` must not import from `Module.Catalog` — duplicate or extract the shared constants
- **CON-001**: Result files are `static partial class` named `XxxResult` and live in the entity's `Domain/` folder
- **CON-002**: Error codes follow `{Entity}.{Category}.{Specific}` pattern (e.g. `Order.NotFound`, `LineItem.Quantity.OutOfRange`)
- **CON-003**: Do not change error code strings — only add new ones
- **CON-004**: Do not change handler logic — only the Result definitions and return-site success wrappers
- **PAT-001**: Order uses `OrderResult.Errors.Xxx` (after rename), LineItem uses `LineItemResult.Errors`, Adjustment uses `AdjustmentResult.Errors`, CartExpiry uses `CartExpiryJobResult.Errors`
- **PAT-002**: Use `#region Existence` / `#region Validation` / `#region State` / `#region Auth` per entity
- **PAT-003**: Success methods are parameterized with entity ID where applicable

## 2. Implementation Steps

### Implementation Phase 1 — Naming: OrderResult.Failure → OrderResult.Errors

- GOAL-001: Rename `OrderResult.Failure` to `OrderResult.Errors` to match LineItem, Adjustment, CartExpiry. Update all 64 call sites.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Rename class `OrderResult.Failure` → `OrderResult.Errors` in `Order.Result.cs:26` | | |
| TASK-002 | Rename class `CartExpiryJobResult.Errors` → `CartExpiryJobResult.Failure` in `CartExpiryJob.Result.cs:10` (or rename to `Errors` everywhere — decide one) | | |
| TASK-003 | Update all `OrderResult.Failure.Xxx` references in `Features/` to `OrderResult.Errors.Xxx` (64 call sites) | | |
| TASK-004 | Update all `CartExpiryJobResult.Errors.Xxx` references if renamed (0 expected — `NotFound` is a property access, same after rename) | | |
| TASK-005 | `dotnet build service/Api/src/Api/Api.csproj` — verify 0 warnings, 0 errors | | |

### Implementation Phase 2 — Add #region Groupings

- GOAL-002: Add `#region` blocks to all 4 Result files to group by concern

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | `Order.Result.cs`: add `#region Existence` (NotFound), `#region Validation` (QuantityNotPositive, MinimumOrderAmount, EmailRequired, AddressRequired, DeliveryMethodRequired, PaymentMethodRequired, EmptyOrderCannotFinalize, CheckoutNotComplete, PaymentRequired, PaymentFailed, PaymentAmountMismatch, VariantDiscontinued, IdRequired), `#region State` (AlreadyFinalized, AlreadyCanceled, CannotAdvanceState, InvalidStatusTransition, InvalidStatusForLineItemRemove, InvalidStatusForDelete), `#region Auth` (UserNotAuthenticated) | | |
| TASK-011 | `LineItem.Result.cs`: replace single `#region Business` with `#region Existence` (NotFound, OrderNotFound, VariantNotFound), `#region Validation` (QuantityExceedsMax, InvalidPrice) | | |
| TASK-012 | `Adjustment.Result.cs`: add `#region Existence` (NotFound), `#region Validation` (InvalidAmount, AdjustableRequired, SourceRequired, InvalidAdjustableType, InvalidSourceType), `#region State` (AlreadyClosed, AlreadyOpen), `#region Misc` (ActionInvalid) | | |
| TASK-013 | `CartExpiryJob.Result.cs`: add `#region Existence` (NotFound) | | |

### Implementation Phase 3 — Add Missing XML Comments

- GOAL-003: Every public member in Result files has a `<summary>` tag

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-020 | `Order.Result.cs`: add XML `<summary>` to 14 missing error defs: `AlreadyFinalized`, `AlreadyCanceled`, `CannotAdvanceState`, `InvalidStatusTransition`, `AddressRequired`, `DeliveryMethodRequired`, `PaymentMethodRequired`, `MinimumOrderAmount`, `EmptyOrderCannotFinalize`, `QuantityNotPositive`, `InvalidStatusForLineItemRemove`, `InvalidStatusForDelete`, `IdRequired`, `VariantDiscontinued` | | |
| TASK-021 | `Order.Result.cs`: add XML `<summary>` to all 7 success methods: `Created`, `Placed`, `Canceled`, `Approved`, `Finalized`, `Emptied`, `Resumed` | | |
| TASK-022 | `LineItem.Result.cs`: add XML `<summary>` to 3 missing: `QuantityExceedsMax`, `InvalidPrice`, `OrderNotFound`, `VariantNotFound` | | |
| TASK-023 | `Adjustment.Result.cs`: add XML `<summary>` to 7 missing: `InvalidAmount`, `AdjustableRequired`, `SourceRequired`, `InvalidAdjustableType`, `InvalidSourceType`, `AlreadyClosed`, `AlreadyOpen` | | |
| TASK-024 | `LineItem.Result.cs`: add XML `<summary>` to 3 success methods: `Created`, `QuantityUpdated`, `Recalculated` | | |
| TASK-025 | `Adjustment.Result.cs`: add XML `<summary>` to 4 success methods: `Created`, `Updated`, `Closed`, `Opened` | | |

### Implementation Phase 4 — Add Missing Success Messages for Admin Operations

- GOAL-004: Every admin mutation handler returns a success message via `OrderResult.Success.Xxx` or `LineItemResult.Success.Xxx`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-030 | `Order.Result.cs`: add `OrderResult.Success.Deleted(Guid id)` → `"Order with ID '{id}' was successfully deleted."` | | |
| TASK-031 | `Order.Result.cs`: add `OrderResult.Success.Updated(Guid id)` → `"Order with ID '{id}' was successfully updated."` (for address/shipping/lineitem updates) | | |
| TASK-032 | `Order.Result.cs`: add `OrderResult.Success.Completed(Guid id, string by)` → `"Order with ID '{id}' was completed by '{by}'."` | | |
| TASK-033 | `LineItem.Result.cs`: add `LineItemResult.Success.Removed(Guid id)` → `"LineItem with ID '{id}' was removed."` | | |
| TASK-034 | `LineItem.Result.cs`: add `LineItemResult.Success.Updated(Guid id)` → `"LineItem with ID '{id}' was updated."` (for admin line item quantity/price update) | | |
| TASK-035 | `DeleteOrder.cs:34`: change `return Result.Ok()` → `return Result.Ok(OrderResult.Success.Deleted(command.Id))` | | |
| TASK-036 | `CompleteOrder.cs:38`: wrap return as `return Result.Ok(OrderResult.Success.Completed(command.Id, currentUser.UserName), order.MapToDetail<Response>())` — note: `Result<T>` factory pattern needs confirmation | | |
| TASK-037 | `RemoveOrderLineItem.cs:33`: change `return Result.Ok()` → `return Result.Ok(LineItemResult.Success.Removed(command.LineItemId))` | | |
| TASK-038 | `ApproveOrder.cs`, `ResumeOrder.cs`, `UpdateOrderAdmin.cs`, `UpdateBillAddress.cs`, `UpdateShipAddress.cs`, `UpdateShippingMethod.cs`, `UpdateOrderStatus.cs`, `CancelOrderAdmin.cs`: wrap existing return with success message (e.g. `OrderResult.Success.Approved(command.Id)` or `OrderResult.Success.Updated(command.Id)`) | | |
| TASK-039 | `UpdateLineItem.cs`: wrap return with `LineItemResult.Success.Updated(command.LineItemId)` | | |
| TASK-040 | `dotnet build` — verify 0 warnings, 0 errors | | |

### Implementation Phase 5 — Add Missing Error Definitions

- GOAL-005: Add specific error definitions for state transitions that currently fall through to generic `InvalidStatusTransition`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-050 | `Order.Result.cs`: add `OrderResult.Errors.AlreadyApproved` → `Error.Conflict("Order.AlreadyApproved", "Order is already approved.")` | | |
| TASK-051 | `Order.Result.cs`: add `OrderResult.Errors.CannotComplete` → `Error.Validation("Order.CannotComplete", "Only placed orders can be completed.")` | | |
| TASK-052 | `Order.Result.cs`: add `OrderResult.Errors.CannotResume` → `Error.Validation("Order.CannotResume", "Only canceled orders can be resumed.")` | | |
| TASK-053 | `Order.Result.cs`: add `OrderResult.Errors.ShippingRateInvalid` → `Error.Validation("Order.ShippingRate.Invalid", "The selected shipping rate is not valid for this order.")` | | |
| TASK-054 | `Order.Result.cs`: add `OrderResult.Errors.CartSessionMismatch` → `Error.Conflict("Order.CartSession.Mismatch", "Cart session does not match the current user session.")` | | |
| TASK-055 | `ApproveOrder.cs:29`: replace generic `OrderResult.Failure.NotFound` + no state-guard with `OrderResult.Errors.AlreadyApproved` if `order.Status == Placed` | | |
| TASK-056 | `CompleteOrder.cs:27`: change `OrderResult.Failure.InvalidStatusTransition` → `OrderResult.Errors.CannotComplete` | | |
| TASK-057 | `CancelOrderAdmin.cs`: replace generic fallthrough with specific `OrderResult.Errors.AlreadyCanceled` if applicable | | |
| TASK-058 | `dotnet build` — verify 0 warnings, 0 errors | | |

### Implementation Phase 6 — Fix Cross-Module Reference in Order.Constant.cs

- GOAL-006: Remove `using Module.Catalog.Domain.Products.Variants.Prices` from `Order.Constant.cs`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-060 | `Order.Constant.cs`: replace line 1 `using Module.Catalog.Domain.Products.Variants.Prices;` with local `PriceConstant` re-declaration inside `Ordering.Domain.Orders` namespace (duplicate `Precision = 18` / `Scale = 2` values) | | |
| TASK-061 | `Order.Constant.cs:15-16`: change `PriceConstant.Constraints.Precision` → `OrderPriceConstant.Precision` (or inline values directly) | | |
| TASK-062 | Verify no other files in Ordering import `PriceConstant` from Catalog — `grep -r "PriceConstant" service/Api/src/Module/Ordering/` should return 0 Catalog-origin refs | | |
| TASK-063 | `dotnet build` — verify 0 warnings, 0 errors | | |

## 3. Alternatives

- **ALT-001**: Keep `Failure` name in Order.Result.cs and rename LineItem/Adjustment/Expiry to `Failure` instead. Rejected because `Errors` is the more descriptive name (the property returns `Error` structs, not `Failure` structs) and 3 out of 4 files already use `Errors`.
- **ALT-002**: Extract shared `PriceConstant` to `Shared` module. This is the correct long-term fix but requires a Shared project change and cross-project coordination. The short-term duplicate is safe (these are compile-time constants, not runtime state).
- **ALT-003**: Fix all 25 cross-module `using` violations in feature handler files as part of this plan. Rejected — too large, each handler needs MediatR `ISender` refactoring. Separate plan.

## 4. Dependencies

- **DEP-001**: `dotnet build` must pass after each phase — any failure backpropagates to fix the cause before proceeding
- **DEP-002**: Phase 1 (rename `Failure` → `Errors`) blocks all subsequent phases that reference `OrderResult`
- **DEP-003**: Phase 4 (success messages) depends on Phase 1 naming being done

## 5. Files

- **FILE-001**: `service/Api/src/Module/Ordering/Domain/Orders/Order.Result.cs` — rename class, add regions, add XML, add success/error defs
- **FILE-002**: `service/Api/src/Module/Ordering/Domain/Orders/Order.Constant.cs` — remove cross-module import
- **FILE-003**: `service/Api/src/Module/Ordering/Domain/LineItems/LineItem.Result.cs` — add regions, XML, success defs
- **FILE-004**: `service/Api/src/Module/Ordering/Domain/Adjustments/Adjustment.Result.cs` — add regions, XML
- **FILE-005**: `service/Api/src/Module/Ordering/Backgrounds/CartExpiryJob.Result.cs` — add regions, XML
- **FILE-006**: 64 call sites in `Features/` subdirectories — rename `OrderResult.Failure` → `OrderResult.Errors`
- **FILE-007**: ~10 admin handler files — wrap returns with success messages (DeleteOrder, CompleteOrder, RemoveOrderLineItem, ApproveOrder, ResumeOrder, UpdateOrderAdmin, UpdateBillAddress, UpdateShipAddress, UpdateShippingMethod, UpdateStatus, CancelOrderAdmin, UpdateLineItem)

## 6. Testing

- **TEST-001**: Verify each phase with `dotnet build` — warnings-as-errors catches naming/type errors
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests/Ordering/` — existing unit tests pass after rename (no behavioral change)
- **TEST-003**: Review all handler return statements for consistent success message wrapping

## 7. Risks & Assumptions

- **RISK-001**: Rename `Failure` → `Errors` touches 64 call sites. If any site uses `.Failure` via `using static` or extension methods, the rename silently misses it. Mitigation: `grep -r "OrderResult\.Failure"` after rename to confirm zero remaining.
- **RISK-002**: Adding success message strings to handlers that return `Result<T>` (like `CompleteOrder` which returns `Result<Response>`) requires verifying `Result<T>` has an overload accepting both value and message string. If not, use `Result.Ok(value)` with logging instead.
- **ASSUMPTION-001**: `Result<T>.Ok(value)` accepts implicit conversion from value — this is confirmed by existing usage patterns in the codebase.
- **ASSUMPTION-002**: The `PriceConstant.Precision` / `PriceConstant.Scale` values are `18` and `2` — these are ISO 4217 defaults and can be safely duplicated.

## 8. Related Specifications / Further Reading

- `plan/data-README-consolidation-modules-1.md` — prior README cleanup that removed cross-module deps from Ordering README
- `docs/codebase/CONCERNS.md:73` — Ordering event publisher removal concern (now resolved)
- `docs/codebase/CONVENTIONS.md` — coding conventions for partial class structure
- AGENTS.md §2 — modules must not reference each other; communication via ISender only
