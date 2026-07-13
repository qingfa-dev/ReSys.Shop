---
title: Order Domain — Convention Alignment & Bug Fixes
version: 1.0
date_created: 2026-07-13
last_updated: 2026-07-13
owner: Platform Team
tags: design, ordering, domain, convention, bug-fix, service/Api
---

# Introduction

The `Orders` domain entity in the Ordering module deviates from the `{Entity}.Method.{Concern}.cs` file-naming convention followed by every other entity across the codebase (Catalog, Inventory, Payment, Profile, and Ordering's own `LineItems` and `Adjustments`). Additionally, several bugs, dead code paths, and duplicated logic were identified during review. This specification defines the migration plan to align the Orders domain with conventions while fixing all identified defects.

## 1. Purpose & Scope

**Purpose**: Rename and restructure `Order` domain files to match the `{Entity}.Method.{Concern}.cs` convention, fix bugs, eliminate duplication, and remove dead code. Behavior must remain identical where correct; broken behaviors must be fixed.

**Scope**: All 14 files under `service/Api/src/Module/Ordering/Domain/Orders/`. No changes to `LineItems/`, `Adjustments/`, Features, or any other module.

**Audience**: Backend developers and coding agents implementing the migration.

**Assumptions**:
- `dotnet build` compiles cleanly before migration.
- No upstream callers reference `OrderExtensions`, `OrderContents`, `OrderMerger`, or `OrderUpdater` by type name (they may be referenced by method name via extension methods on `Order`).
- `OrderNumber.Generate()` callers are tolerant of a change from `out int` to `Result<string>` return.
- `AfterCancel()` / `AfterResume()` stubs have no callers that depend on them doing anything observable.

## 2. Definitions

| Term | Definition |
|------|-----------|
| **Convention file naming** | Domain entity files use `{Entity}.Method.{Concern}.cs` (e.g., `Product.Method.cs`, `PaymentCapture.Method.State.cs`, `LineItem.Method.Compute.cs`). |
| **Method class** | Static class named `{Entity}Method` containing factory (`Create`) and instance extension methods for domain operations. |
| **Result object** | `Result` or `Result<T>` returned from all domain operations. Exceptions are reserved for unrecoverable infrastructure failures. |
| **Dead code** | Code defined but never reachable (errors defined but never returned, loggers defined but never called). |

## 3. Requirements, Constraints & Guidelines

### File Renames

- **REQ-001**: Rename `Order.Extensions.cs` to `Order.Method.cs`. Rename class `OrderExtensions` to `OrderMethod`.
- **REQ-002**: Rename `Order.Checkout.cs` to `Order.Method.Checkout.cs`.
- **REQ-003**: Rename `Order.AddressBook.cs` to `Order.Method.AddressBook.cs`.
- **REQ-004**: Rename `Order.CurrencyUpdater.cs` to `Order.Method.Currency.cs`.
- **REQ-005**: Move `Services/OrderContents.cs` to `Order.Method.Contents.cs`. Rename class `OrderContents` to a concern-appropriate static method on `Order` (the `OrderContents` instance wrapper adds a layer not present in other modules).
- **REQ-006**: Move `Services/OrderMerger.cs` to `Order.Method.Merge.cs`. Rename class `OrderMerger` similarly — or keep as an instance extension on `Order`.
- **REQ-007**: Move `Services/OrderUpdater.cs` to `Order.Method.Update.cs`. Rename class `OrderUpdater` — or fold its methods into `OrderMethod` as extensions on `Order`.
- **REQ-008**: Delete the empty `Services/` directory after moves are complete.

### Bug Fixes

- **REQ-009**: `OrderNumber.Generate()` must return `Result<string>` instead of throwing `InvalidOperationException`. The `out int attempts` parameter must be replaced by a `Result<string>` return that includes the generated number on success or an `Error` on failure.
- **REQ-010**: `OrderMethod.Delete()` must enforce the Draft/Expired status constraint. Return `OrderResult.Errors.InvalidStatusForDelete` when status is not Draft or Expired.
- **REQ-011**: Eliminate the duplicate `UpdatePaymentState()` logic. Keep a single implementation in the `OrderMethod` class and delete the copy in the former `OrderUpdater`.
- **REQ-012**: Unify `RecalculateTotals()` and `OrderUpdater.UpdateTotals()` into a single recalculation path that is correct for all callers. The unified path must include line-item-level adjustment totals when computing `AdjustmentTotal`.
- **REQ-013**: Ensure `OutstandingBalance` is recalculated whenever `Total` or `PaymentTotal` changes. The unified recalculation path must update `OutstandingBalance = Total - PaymentTotal`.
- **REQ-014**: `OrderMerger.HandleMerge()` must call `lineItem.RecalculateTotal()` instead of directly setting `Total = Price * Quantity`.
- **REQ-015**: `Approve()` must check whether the order is already approved. Return `OrderResult.Errors.AlreadyApproved` if `ApprovedById.HasValue`.
- **REQ-016**: `SetBillAddressId()` and `SetShipAddressId()` must guard against editing non-Draft orders. Return the appropriate `NotDraftForBillAddress` / `NotDraftForShipAddress` error.
- **REQ-017**: `Empty()` must clear `ItemCount` to 0. It must also return errors using the same style as other methods (`return OrderResult.Errors.X`), not `return Result.Failure(OrderResult.Errors.X)`.
- **REQ-018**: Remove empty stubs `AfterCancel()` and `AfterResume()` along with their `#pragma warning disable CA1822` suppressions if no callers exist. If callers exist, replace with a clear no-op comment and keep the warning suppression minimal.
- **REQ-019**: Simplify `EnsureLineItemsPresent()` to `return LineItems.Count > 0`.
- **REQ-020**: Change `!LineItems.Any(li => discontinuedVariantIds.Contains(li.VariantId))` to `LineItems.All(li => !discontinuedVariantIds.Contains(li.VariantId))` to eliminate the double negation.

### Dead Code Removal

- **REQ-021**: Delete `Order.Loggers.cs` if Loggers are not referenced within the Orders domain. If they are referenceable from feature handlers, keep them but ensure at least one call site exists; otherwise remove.
- **REQ-022**: Verify all errors defined in `OrderResult.Errors` have at least one call site. Add call sites for any that are missing (e.g., `AlreadyApproved`, `NotDraftForBillAddress`, `NotDraftForShipAddress`, `InvalidStatusForDelete`).

### Constraints

- **CON-001**: `dotnet build` must pass with `TreatWarningsAsErrors=true`.
- **CON-002**: All domain operations must return `Result` / `Result<T>`. No exceptions for control flow.
- **CON-003**: No new cross-module references. Existing references within `Domain.Orders` must remain unchanged.
- **CON-004**: File renames must update all `using` statements and namespace declarations across the codebase to compile cleanly.
- **CON-005**: No behavioral change to correct code paths. Only broken paths are modified.

### Guidelines

- **GUD-001**: Follow the existing `{Entity}Method` static class pattern (see `ProductMethod`, `PaymentMethodMethod`, `StockLocationMethod`, `LineItemMethod`).
- **GUD-002**: Factory method `Create()` returns `Result<Order>` directly (no explicit `Result.Ok()` wrapper needed — implicit conversion handles it).
- **GUD-003**: Error factories follow `{Entity}.{Action}.{Problem}` code pattern (e.g., `Order.Delete.InvalidStatus`).

## 4. Interfaces & Data Contracts

### File Mapping (Before → After)

| Before | After |
|--------|-------|
| `Order.Extensions.cs` (`OrderExtensions`) | `Order.Method.cs` (`OrderMethod`) |
| `Order.Checkout.cs` | `Order.Method.Checkout.cs` |
| `Order.AddressBook.cs` | `Order.Method.AddressBook.cs` |
| `Order.CurrencyUpdater.cs` | `Order.Method.Currency.cs` |
| `Services/OrderContents.cs` | `Order.Method.Contents.cs` |
| `Services/OrderMerger.cs` | `Order.Method.Merge.cs` |
| `Services/OrderUpdater.cs` | `Order.Method.Update.cs` |
| `Order.cs` | *unmoved* |
| `Order.Constant.cs` | *unmoved* |
| `Order.Enumerate.cs` | *unmoved* |
| `Order.Result.cs` | *unmoved* |
| `Order.Validation.cs` | *unmoved* |
| `Order.Loggers.cs` | *deleted* (REQ-021) |
| `OrderNumber.cs` | *unmoved* (API changed per REQ-009) |

### `OrderMethod` Static Class — Target Structure

```csharp
// Order.Method.cs
public static partial class OrderMethod
{
    // Factory
    public static Result<Order> Create(string currency, Guid? userId, Guid storeId, ...) { }

    // State machine extensions on Order
    public static Result AdvanceCheckout(this Order order) { }
    public static Result Finalize(this Order order) { }
    public static Result Cancel(this Order order, Guid canceledById) { }
    public static Result Resume(this Order order) { }
    public static Result Approve(this Order order, Guid approvedById) { }
    public static Result Empty(this Order order) { }

    // Computation
    public static void RecalculateTotals(this Order order) { }  // unified path
    public static bool IsPaid(this Order order) { }
    public static void UpdatePaymentState(this Order order) { }  // single copy

    // Lifecycle
    public static Result Delete(this Order order, string deletedBy) { }
    public static Result CanAdvanceTo(this Order order, CheckoutState targetState) { }
}
```

### Error Codes — New or Newly-Wired

| Error Property | Code | Call Site |
|---------------|------|-----------|
| `AlreadyApproved` | `Order.AlreadyApproved` | `Approve()` |
| `NotDraftForBillAddress` | `Order.BillAddress.Update.NotDraft` | `SetBillAddressId()` |
| `NotDraftForShipAddress` | `Order.ShipAddress.Update.NotDraft` | `SetShipAddressId()` |
| `InvalidStatusForDelete` | `Order.Delete.InvalidStatus` | `Delete()` |
| `OrderNumberGenerationFailed` | `Order.Number.GenerationFailed` | `OrderNumber.Generate()` |

## 5. Acceptance Criteria

- **AC-001**: Given the migrated codebase, when `dotnet build` is run, then it passes with zero warnings.
- **AC-002**: Given an order in Draft status with line items, when `Delete()` is called, then the order is soft-deleted.
- **AC-003**: Given an order in Placed status, when `Delete()` is called, then `OrderResult.Errors.InvalidStatusForDelete` is returned.
- **AC-004**: Given an order with `ApprovedById` set, when `Approve()` is called, then `OrderResult.Errors.AlreadyApproved` is returned.
- **AC-005**: Given an order in Placed status, when `SetBillAddressId()` is called, then `OrderResult.Errors.NotDraftForBillAddress` is returned.
- **AC-006**: Given `OrderNumber.Generate()` fails after 8 attempts, then a `Result<string>` failure is returned (no thrown exception).
- **AC-007**: Given any order, when `RecalculateTotals()` is called, then `AdjustmentTotal` includes both order-level eligible adjustments AND line-item adjustment totals, and `OutstandingBalance = Total - PaymentTotal`.
- **AC-008**: Given order merge, when a matching line item is merged, then `RecalculateTotal()` is called on the line item (not manual `Total = Price * Qty`).
- **AC-009**: Given `Empty()` is called, then `ItemCount` is reset to 0.
- **AC-010**: No duplicate `UpdatePaymentState()` exists in the codebase.
- **AC-011**: File `Order.Loggers.cs` does not exist.
- **AC-012**: `AfterCancel()` and `AfterResume()` stubs are removed.
- **AC-013**: `EnsureLineItemsPresent()` is a single-expression `return LineItems.Count > 0`.
- **AC-014**: Variant discontinuation check uses `All()` instead of `!Any()`.
- **AC-015**: Files `Order.Method.Checkout.cs`, `Order.Method.AddressBook.cs`, `Order.Method.Currency.cs`, `Order.Method.Contents.cs`, `Order.Method.Merge.cs`, `Order.Method.Update.cs` exist and the old filenames no longer exist.
- **AC-016**: Directory `Services/` no longer exists under `Domain/Orders/`.
- **AC-017**: All existing unit tests referencing `OrderExtensions`, `OrderContents`, `OrderMerger`, or `OrderUpdater` continue to pass after updating references to the new names.

## 6. Test Automation Strategy

- **Test Levels**: Unit tests only (domain logic, no Docker required).
- **Frameworks**: xUnit, FluentAssertions, Moq (existing project standards).
- **Test Data Management**: Inline test data via factory methods. No database seeding needed for domain logic tests.
- **CI/CD Integration**: `dotnet test service/Api/tests/Module.UnitTests` must pass.
- **Coverage Requirements**: All new guard clauses and error paths must have at least one test.
- **Performance Testing**: Not applicable — no hot-path changes.

### Specific Test Cases to Add/Update

| Test | Covers |
|------|--------|
| `Delete_RejectsPlacedOrder` | REQ-010 |
| `Delete_RejectsCanceledOrder` | REQ-010 |
| `Approve_RejectsAlreadyApproved` | REQ-015 |
| `SetBillAddress_RejectsNonDraftOrder` | REQ-016 |
| `SetShipAddress_RejectsNonDraftOrder` | REQ-016 |
| `Empty_ResetsItemCount` | REQ-017 |
| `GenerateNumber_ReturnsErrorOnExhaustion` | REQ-009 |
| `RecalculateTotals_IncludesLineItemAdjustments` | REQ-012 |
| `RecalculateTotals_UpdatesOutstandingBalance` | REQ-013 |
| `Merge_RecalculatesLineItemTotal` | REQ-014 |

## 7. Rationale & Context

All 7 other entities across Catalog, Inventory, Payment, Profile, and Ordering's own `LineItems` and `Adjustments` use the `{Entity}.Method.{Concern}.cs` convention with a static `{Entity}Method` class. The `Orders` entity is the sole outlier, using `{Entity}.Extensions.cs` (`OrderExtensions`) and custom file names like `Checkout.cs`, `AddressBook.cs`, `CurrencyUpdater.cs` instead of `Method.Checkout.cs`, etc. This discrepancy:
- Makes code navigation unpredictable (developers expect `Order.Method.Foo.cs`, must remember the nonstandard names).
- Violates the domain file convention documented in `docs/codebase/CONVENTIONS.md` line 10.
- Introduces an unnecessary `Services/` subdirectory pattern not found elsewhere.

The `Services/` subdirectory classes (`OrderContents`, `OrderMerger`, `OrderUpdater`) are thin wrappers around `Order` that duplicate or diverge from logic already in `OrderExtensions`. Folding them into `OrderMethod` eliminates the wrapper layer and aligns with the static extension-method pattern used by all other modules.

The bugs (duplicate recalculation paths, missing guard clauses, exception-throwing domain code) represent real correctness risks under concurrent use or edge-case inputs.

## 8. Dependencies & External Integrations

### Internal Dependencies
- **EXT-001**: `OrderMethod` must remain in namespace `Module.Ordering.Domain.Orders` (same namespace as `Order` entity).
- **EXT-002**: Callers in `Module/Ordering/Features/` may reference `OrderMethod.Create()`, `OrderMethod.Delete()`, `OrderNumber.Generate()`, etc. These references must be updated to match new names.
- **EXT-003**: `Order.Result.cs` must gain error factories for any new error paths (e.g., `OrderNumberGenerationFailed`).

### Infrastructure Dependencies
- **INF-001**: No new infrastructure dependencies. Migrated code runs in the same `Module` project.

### Technology Platform Dependencies
- **PLT-001**: .NET 10, C# preview (unchanged from existing stack).

## 9. Examples & Edge Cases

### Unified `RecalculateTotals()` — Before (broken, two paths)

```csharp
// Path A: OrderExtensions.RecalculateTotals (missing line item adjustment totals)
order.AdjustmentTotal = order.Adjustments.Where(a => a.Eligible).Sum(a => a.Amount);
order.Total = order.ItemTotal + order.AdjustmentTotal;
order.OutstandingBalance = order.Total - order.PaymentTotal; // includes balance

// Path B: OrderUpdater.UpdateAdjustmentTotal (includes line item adj, no balance)
var lineItemAdjustmentTotal = Order.LineItems.Sum(li => li.AdjustmentTotal);
var orderAdjustmentTotal = Order.Adjustments.Where(a => a.Eligible).Sum(a => a.Amount);
Order.AdjustmentTotal = lineItemAdjustmentTotal + orderAdjustmentTotal;
Order.Total = Order.ItemTotal + Order.ShipmentTotal + Order.AdjustmentTotal;
// BUG: OutstandingBalance not updated
```

### Unified `RecalculateTotals()` — After

```csharp
public static void RecalculateTotals(this Order order)
{
    order.ItemCount = order.LineItems.Sum(li => li.Quantity);
    order.ItemTotal = order.LineItems.Sum(li => li.Total);
    order.AdjustmentTotal =
        order.LineItems.Sum(li => li.AdjustmentTotal) +
        order.Adjustments.Where(a => a.Eligible).Sum(a => a.Amount);
    order.ShipmentTotal = order.Adjustments
        .Where(a => a.Eligible && a.SourceType == AdjustmentConstant.SourceTypes.Shipping)
        .Sum(a => a.Amount);
    order.Total = order.ItemTotal + order.ShipmentTotal + order.AdjustmentTotal;
    order.OutstandingBalance = order.Total - order.PaymentTotal;
}
```

### `OrderNumber.Generate()` — After

```csharp
public static Result<string> Generate(IApplicationDbContext dbContext)
{
    for (var attempts = 1; attempts <= MaxAttempts; attempts++)
    {
        var candidate = $"R{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..17].ToUpperInvariant();
        var exists = dbContext.Set<Order>().Any(o => o.Number == candidate);
        if (!exists) return Result.Ok(candidate);
    }
    return OrderResult.Errors.OrderNumberGenerationFailed;
}
```

## 10. Validation Criteria

- **VAL-001**: `dotnet build` exits with code 0 and zero warnings.
- **VAL-002**: `dotnet test service/Api/tests/Module.UnitTests` passes all tests including new ones.
- **VAL-003**: `rg "OrderExtensions" service/Api/src/` returns zero results (class name fully migrated).
- **VAL-004**: `rg "Services/OrderContents|Services/OrderMerger|Services/OrderUpdater" service/Api/src/` returns zero results (old files gone, callers updated).
- **VAL-005**: `rg "throw new" service/Api/src/Module/Ordering/Domain/Orders/` returns zero results.
- **VAL-006**: `rg "UpdatePaymentState" service/Api/src/Module/Ordering/Domain/Orders/` returns exactly 1 result (no duplicate).
- **VAL-007**: `rg "AfterCancel|AfterResume" service/Api/src/Module/Ordering/Domain/Orders/` returns zero results.
- **VAL-008**: `ls service/Api/src/Module/Ordering/Domain/Orders/Services/` fails with "No such file or directory".
- **VAL-009**: Every error property in `OrderResult.Errors` has at least one call site (verified via `rg` on each error name).

## 11. Related Specifications / Further Reading

- [spec-architecture-api-mvp-gaps.md](./spec-architecture-api-mvp-gaps.md) — MVP readiness gaps for the API service.
- [docs/codebase/CONVENTIONS.md](../docs/codebase/CONVENTIONS.md) — Coding conventions (domain file naming: line 10).
- [.harness/enforcement.yml](../.harness/enforcement.yml) — Naming and file limit enforcement rules.
- [.harness/principles.yml](../.harness/principles.yml) — Golden principles (Result objects, vertical slice isolation).
