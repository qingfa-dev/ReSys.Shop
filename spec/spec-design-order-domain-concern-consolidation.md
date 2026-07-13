---
title: Order Domain — Dead Code Removal & Method Surface Optimization
version: 2.0
date_created: 2026-07-13
last_updated: 2026-07-13
owner: Platform Team
tags: design, ordering, domain, refactor, dead-code, service/Api
---

# Introduction

A comprehensive caller audit of the `Order` domain revealed that 16 of 31 methods (52%) have zero production callers. Three entire `Order.Method.*.cs` files contain exclusively dead methods. Four Ruby-ported aliases (`CanceledBy`, `ApprovedBy`, `IsPaidCheck`, `GetOutstandingBalance`) serve no purpose. Six error constants in `OrderResult.Errors` have zero call sites. Additionally, 3 feature handlers bypass domain methods, duplicating their logic inline and circumventing domain guards.

This specification defines the removal of all dead methods, consolidation of the surviving methods into 4 concern files, removal of dead error constants and Ruby aliases, and addition of missing constants. It also identifies feature-bypass gaps for a follow-up specification.

## 1. Purpose & Scope

**Purpose**: Remove 16 dead methods, 3 dead Method files, 4 dead Ruby aliases, and 6 dead error constants from the `Order` domain. Consolidate the 10 surviving methods into 4 purpose-grouped `Order.Method.*.cs` files. Add missing constants. Document feature-bypass gaps for future work.

**Scope**: Files under `service/Api/src/Module/Ordering/Domain/Orders/`. This specification does NOT modify feature handlers — it only removes dead domain code. Feature-bypass gaps are documented as follow-up recommendations (GAP-001 through GAP-003).

**Audience**: Backend developers and coding agents implementing dead-code removal.

**Assumptions**:
- `dotnet build` compiles cleanly before removal.
- `OrderMethod` is a `static partial class` — methods can be deleted from individual files without affecting other partial files.
- Deleted methods that appear in unit tests (e.g., `AdvanceCheckout`, `IsPaid`) will also require test removal — this is covered in REQ-T01.
- `Order.Method.Checkout.cs` contains instance members on the `sealed partial class Order` — these are NOT `OrderMethod` extension methods and are subject to separate dead-code audit.

## 2. Definitions

| Term | Definition |
|------|-----------|
| **Dead method** | A method with zero call sites in `service/Api/src/` (feature handlers). Self-references within the same file and test-only callers do not count as production callers. |
| **Ruby alias** | A thin wrapper method whose sole purpose is to provide a Ruby-style naming convention (e.g., `CanceledBy` wrapping `Cancel`, `IsPaidCheck` wrapping `IsPaid`). |
| **Transitively dead** | A method that is only called by another dead method (e.g., `UpdateLineItemPrice` called only by `UpdateLineItemCurrencies`, which is called only by `HomogenizeLineItemCurrencies`, which has zero callers). |
| **Feature bypass** | A feature handler that duplicates domain method logic inline instead of calling the domain method (e.g., `CreateOrderFromCart.cs` setting `Status = Placed` directly instead of calling `order.Finalize()`). |
| **Concern** | A cohesive set of business operations grouped by purpose: factory, state transitions, checkout flow, and computations. |

## 3. Requirements, Constraints & Guidelines

---

### Removal: Dead Methods (REQ-D)

- **REQ-D01**: Remove the following 16 dead methods from the codebase:

| # | Method | Current File | Reason |
|---|--------|-------------|--------|
| 1 | `AdvanceCheckout` | `Order.Method.cs` | Zero production callers; checkout handler bypasses state machine |
| 2 | `IsPaid` | `Order.Method.cs` | Zero external callers; only internal caller is `IsPaidCheck` (also dead) |
| 3 | `IsPaidCheck` | `Order.Method.cs` | Ruby alias with zero callers; wraps `IsPaid` |
| 4 | `GetOutstandingBalance` | `Order.Method.cs` | Trivial pass-through to public property; zero callers |
| 5 | `CanceledBy` | `Order.Method.cs` | Ruby alias with zero callers; wraps `Cancel` |
| 6 | `ApprovedBy` | `Order.Method.cs` | Ruby alias with zero callers; wraps `Approve` |
| 7 | `CanAdvanceTo` | `Order.Method.cs` | Zero callers; duplicate validation of `AdvanceCheckout` |
| 8 | `AssignDefaultAddresses` | `Order.Method.Checkout.cs` | Zero callers; `internal` scope, never invoked |
| 9 | `EnsureLineItemsPresent` | `Order.Method.Checkout.cs` | Zero callers; all callers check `LineItems.Count` inline |
| 10 | `CloneShippingAddress` | `Order.Method.AddressBook.cs` | Zero callers |
| 11 | `CloneBillingAddress` | `Order.Method.AddressBook.cs` | Zero callers |
| 12 | `SetBillAddressId` | `Order.Method.AddressBook.cs` | Zero callers; features set `BillAddressId` property directly |
| 13 | `SetShipAddressId` | `Order.Method.AddressBook.cs` | Zero callers; features set `ShipAddressId` property directly |
| 14 | `ShippingEqualsBillingAddress` | `Order.Method.AddressBook.cs` | Zero callers |
| 15 | `AddItem` | `Order.Method.Contents.cs` | Zero callers; `AddToCart.cs` does merge logic inline |
| 16 | `RemoveItem` | `Order.Method.Contents.cs` | Zero callers; features use `LineItems.Remove()` directly |
| 17 | `RemoveLineItem` | `Order.Method.Contents.cs` | Zero callers; features use `LineItems.Remove()` directly |

- **REQ-D02**: Remove the following 3 transitively-dead currency methods:

| # | Method | Current File | Reason |
|---|--------|-------------|--------|
| 18 | `HomogenizeLineItemCurrencies` | `Order.Method.Currency.cs` | Zero callers; pass-through to `UpdateLineItemCurrencies` |
| 19 | `UpdateLineItemCurrencies` | `Order.Method.Currency.cs` | Only called by `HomogenizeLineItemCurrencies` |
| 20 | `UpdateLineItemPrice` | `Order.Method.Currency.cs` | Only called by `UpdateLineItemCurrencies` |

---

### Removal: Dead Files (REQ-F)

- **REQ-F01**: Delete entire file `Order.Method.AddressBook.cs` (49 lines, 5 methods, all dead per REQ-D01 #10-14).
- **REQ-F02**: Delete entire file `Order.Method.Currency.cs` (30 lines, 3 methods, all transitively dead per REQ-D02).
- **REQ-F03**: Delete entire file `Order.Method.Contents.cs` (48 lines, 3 methods, all dead per REQ-D01 #15-17).

---

### Removal: Dead Error Constants (REQ-E)

- **REQ-E01**: Remove the following 6 error properties from `OrderResult.Errors`:

| Error | Reason |
|-------|--------|
| `CannotComplete` | 0 callers; `CompleteOrder` handler uses `InvalidStatusTransition` |
| `CannotResume` | 0 callers; `ResumeOrder` handler uses `InvalidStatusTransition` |
| `MinimumOrderAmount` | 0 callers; no minimum-order-amount enforcement exists |
| `CartSessionMismatch` | 0 callers; no session-mismatch validation exists |
| `ShippingRateInvalid` | 0 callers; shipping-rate validation exists only in feature handlers |
| `PaymentMethodRequired` | 0 callers; checkout validation uses `DeliveryMethodRequired` |

---

### Consolidation: Surviving Methods (REQ-C)

After all removals, the 10 surviving methods are consolidated into 4 concern files:

- **REQ-C01**: `Order.Method.Factory.cs` — contains only `Create()` (~35 lines).

- **REQ-C02**: `Order.Method.StateMachine.cs` — contains `Finalize()`, `Cancel()`, `Resume()`, `Approve()`, `Empty()`, `Delete()`, `Merge()` (~210 lines). `Merge()` is classified as a state-machine operation because cart merging is an order lifecycle transition, not cart-contents manipulation.

- **REQ-C03**: `Order.Method.Checkout.cs` — contains only methods with active production callers from the current `Order.Method.Checkout.cs`. The `sealed partial class Order` instance methods that have zero callers (`AssignDefaultAddresses`, `EnsureLineItemsPresent`) are removed per REQ-D01. Surviving methods: `ResolvedCheckoutSteps`, `CurrentCheckoutStep`, `CompletedCheckoutSteps`, `HasCheckoutStep`, `PassedCheckoutStep`, `CheckoutStepIndex`, `CanGoToState`, `CheckoutAllowed`, `DeliveryRequired`, `PaymentRequired`, `ConfirmationRequired`, `RequireEmail`, `AllowCancel`, `CanShip`, `Uneditable`, `EnsureLineItemVariantsAreNotDiscontinued`.

- **REQ-C04**: `Order.Method.Computation.cs` — contains `RecalculateTotals()` and `UpdatePaymentState()` (~75 lines).

- **REQ-C05**: Delete `Order.Method.cs` entirely (all its content has been either removed as dead or moved into StateMachine, Computation, and Factory).

---

### File Map (Before → After)

| Before | After | Lines |
|--------|-------|-------|
| `Order.Method.cs` | — | DELETED (content distributed or removed) |
| `Order.Method.AddressBook.cs` | — | DELETED (all dead per REQ-F01) |
| `Order.Method.Currency.cs` | — | DELETED (all dead per REQ-F02) |
| `Order.Method.Contents.cs` | — | DELETED (all dead per REQ-F03) |
| `Order.Method.Merge.cs` | — | Merged into `Order.Method.StateMachine.cs` |
| — | `Order.Method.Factory.cs` | ~35 (NEW: `Create()` only) |
| — | `Order.Method.StateMachine.cs` | ~210 (7 methods) |
| `Order.Method.Checkout.cs` | `Order.Method.Checkout.cs` | ~100 (reduced: 2 dead methods removed) |
| — | `Order.Method.Computation.cs` | ~75 (2 methods) |

Result: **4 Method files** (was 6), 10 methods (was 31), ~420 lines (was ~620).

---

### Missing Constants (REQ-K)

- **REQ-K01**: Add to `OrderConstant.Constraints`:
  ```csharp
  public const int MaxLineItems = 100;
  public const int MaxAdjustments = 50;
  ```

- **REQ-K02**: Add to `OrderConstant.Defaults`:
  ```csharp
  public const string PaymentState = PaymentStateConstants.Pending;
  public const string ShipmentState = ShipmentStateConstants.Pending;
  ```

### Missing Result Factories (REQ-R)

- **REQ-R01**: Add to `OrderResult.Success`:
  ```csharp
  public static string Merged(Guid id) => $"Order with ID '{id}' was successfully merged.";
  public static string CheckoutAdvanced(Guid id) => $"Order with ID '{id}' checkout step was advanced.";
  ```

- **REQ-R02**: Add to `OrderResult.Errors`:
  ```csharp
  public static Error MaxLineItemsExceeded => Error.Validation(
      code: "Order.LineItems.MaxExceeded",
      message: $"Order cannot have more than {OrderConstant.Constraints.MaxLineItems} line items.");
  ```

---

### Test Updates (REQ-T)

- **REQ-T01**: Remove the following test methods from `Order.Method.Tests.cs` that test dead methods:

| Test Method | Dead Method Tested |
|------------|-------------------|
| `AdvanceCheckout_FromAddress_ShouldTransition` | `AdvanceCheckout` |
| `AdvanceCheckout_WithoutAddress_ShouldFail` | `AdvanceCheckout` |
| `AdvanceCheckout_DeliveryWithoutMethod_ShouldFail` | `AdvanceCheckout` |
| `AdvanceCheckout_DeliveryWithMethod_ShouldTransition` | `AdvanceCheckout` |
| `AdvanceCheckout_FromComplete_ShouldFail` | `AdvanceCheckout` |
| `IsPaid_WhenBalanceZero_ShouldReturnTrue` | `IsPaid` |
| `IsPaid_WhenBalancePositive_ShouldReturnFalse` | `IsPaid` |

These 7 tests are removed because they exercise dead methods. The remaining 14 tests keep passing.

---

### Feature Bypass Gaps (GAP — Follow-Up)

These are not requirements of this specification. They are documented gaps for a follow-up implementation plan that wires feature handlers to call domain methods instead of duplicating logic inline.

- **GAP-001**: `CreateOrderFromCart.cs` (lines 109-111) bypasses `order.Finalize()`. It manually sets `cart.Status = Placed`, `cart.CheckoutState = Complete`, `cart.CompletedAtUtc`. Should call `order.Finalize()` to enforce domain guards (canceled check, already-placed check, empty-order check).
- **GAP-002**: `DeleteCart.cs` (lines 39-40) bypasses `order.Delete()`. It manually sets `cart.IsDeleted = true` and `cart.DeletedAtUtc`. Should call `order.Delete(deletedBy)` to enforce the Draft/Expired status check.
- **GAP-003**: `CreateOrderFromCart.cs` (line 91) bypasses `order.UpdatePaymentState()`. It manually sets `cart.PaymentState = OrderConstant.PaymentState.Paid`. Should call `order.UpdatePaymentState()` for consistent derivation logic.

---

### Constraints

- **CON-001**: `dotnet build` must pass with `TreatWarningsAsErrors=true` (zero warnings).
- **CON-002**: No new cross-module references.
- **CON-003**: All 4 new `Order.Method.*.cs` files must declare the appropriate partial class (`static partial class OrderMethod` for Factory, StateMachine, Computation; `sealed partial class Order` for Checkout).
- **CON-004**: No behavioral change to surviving methods. Method bodies move or are removed — they are never altered.
- **CON-005**: Deleted methods must leave no orphaned `using` statements, `#region`/`#endregion` directives, or empty partial class declarations.

### Guidelines

- **GUD-001**: Each Method file should be between 35-220 lines. No file under 30 lines that does not represent an independent concern.
- **GUD-002**: New error codes follow `Order.{Aggregate}.{Problem}` pattern.
- **GUD-003**: Compact `Order.Result.cs` by removing empty `#region` blocks left by dead error removal.

## 4. Interfaces & Data Contracts

### Surviving Method Surface

```
Order.Method.Factory.cs:
  static partial class OrderMethod
    Create(currency, userId, storeId, id?, sessionId?, shipAddressId?) → Result<Order>

Order.Method.StateMachine.cs:
  static partial class OrderMethod
    Finalize(this Order)       → Result
    Cancel(this Order, Guid)   → Result
    Resume(this Order)         → Result
    Approve(this Order, Guid)  → Result
    Empty(this Order)          → Result
    Delete(this Order, string)  → Result
    Merge(this Order, Order, Guid?, bool) → void

Order.Method.Checkout.cs:
  sealed partial class Order
    // Properties
    ResolvedCheckoutSteps       → string[]
    CurrentCheckoutStep         → string
    CompletedCheckoutSteps      → string[]
    // Query methods
    HasCheckoutStep(string)     → bool
    PassedCheckoutStep(string)   → bool
    CheckoutStepIndex(string)   → int
    CanGoToState(string)        → bool
    // Guard methods
    CheckoutAllowed()           → bool
    AllowCancel()               → bool
    CanShip()                   → bool
    Uneditable()                → bool
    // Validation
    EnsureLineItemVariantsAreNotDiscontinued(HashSet<Guid>) → bool

Order.Method.Computation.cs:
  static partial class OrderMethod
    RecalculateTotals(this Order) → void
    UpdatePaymentState(this Order) → void
```

### Removed Method Surface

```
DELETED:
  AdvanceCheckout, CanAdvanceTo, IsPaid, IsPaidCheck, GetOutstandingBalance,
  CanceledBy, ApprovedBy,
  AssignDefaultAddresses, EnsureLineItemsPresent,
  CloneShippingAddress, CloneBillingAddress, SetBillAddressId, SetShipAddressId,
  ShippingEqualsBillingAddress,
  AddItem, RemoveItem, RemoveLineItem,
  HomogenizeLineItemCurrencies, UpdateLineItemCurrencies, UpdateLineItemPrice
```

### `OrderConstant` Additions

```csharp
public static class Constraints
{
    // ... existing ...
    public const int MaxLineItems = 100;      // NEW
    public const int MaxAdjustments = 50;     // NEW
}

public static class Defaults
{
    // ... existing ...
    public const string PaymentState = PaymentStateConstants.Pending;   // NEW
    public const string ShipmentState = ShipmentStateConstants.Pending; // NEW
}
```

### `OrderResult.Success` Additions

```csharp
/// <summary>Guest cart was merged into user cart.</summary>
public static string Merged(Guid id) => $"Order with ID '{id}' was successfully merged.";
/// <summary>Checkout step was advanced.</summary>
public static string CheckoutAdvanced(Guid id) => $"Order with ID '{id}' checkout step was advanced.";
```

### `OrderResult.Errors` Additions

```csharp
/// <summary>Order has reached the maximum number of line items.</summary>
public static Error MaxLineItemsExceeded => Error.Validation(
    code: "Order.LineItems.MaxExceeded",
    message: $"Order cannot have more than {OrderConstant.Constraints.MaxLineItems} line items.");
```

### `OrderResult.Errors` Removals

```csharp
// Removed (6 dead + 0 callers each):
// CannotComplete, CannotResume, MinimumOrderAmount, CartSessionMismatch,
// ShippingRateInvalid, PaymentMethodRequired
```

### `OrderConstant` Removals

None. The `#region Address` removed from `OrderResult.Errors` when the file is compacted after dead error removal.

## 5. Acceptance Criteria

### Dead Method Removal

- **AC-01**: `Order.Method.cs` does not contain `AdvanceCheckout`, `IsPaid`, `IsPaidCheck`, `GetOutstandingBalance`, `CanceledBy`, `ApprovedBy`, `CanAdvanceTo`.
- **AC-02**: `Order.Method.Checkout.cs` does not contain `AssignDefaultAddresses`, `EnsureLineItemsPresent`.
- **AC-03**: `Order.Method.AddressBook.cs` does not exist.
- **AC-04**: `Order.Method.Contents.cs` does not exist.
- **AC-05**: `Order.Method.Currency.cs` does not exist.

### File Consolidation

- **AC-06**: `Order.Method.Factory.cs` exists and contains only `Create()`.
- **AC-07**: `Order.Method.StateMachine.cs` exists and contains `Finalize`, `Cancel`, `Resume`, `Approve`, `Empty`, `Delete`, `Merge`.
- **AC-08**: `Order.Method.Checkout.cs` exists and contains all active checkout methods listed in REQ-C03.
- **AC-09**: `Order.Method.Computation.cs` exists and contains `RecalculateTotals`, `UpdatePaymentState`.
- **AC-10**: `Order.Method.cs` does not exist (all content moved or removed).
- **AC-11**: `Order.Method.Merge.cs` does not exist.

### Dead Error Removal

- **AC-12**: `OrderResult.Errors` does not contain `CannotComplete`, `CannotResume`, `MinimumOrderAmount`, `CartSessionMismatch`, `ShippingRateInvalid`, `PaymentMethodRequired`.

### Missing Constants

- **AC-13**: `OrderConstant.Constraints.MaxLineItems` is `100`.
- **AC-14**: `OrderConstant.Constraints.MaxAdjustments` is `50`.
- **AC-15**: `OrderConstant.Defaults.PaymentState` is `"pending"`.
- **AC-16**: `OrderConstant.Defaults.ShipmentState` is `"pending"`.

### Missing Results

- **AC-17**: `OrderResult.Success.Merged(Guid)` returns a string.
- **AC-18**: `OrderResult.Success.CheckoutAdvanced(Guid)` returns a string.
- **AC-19**: `OrderResult.Errors.MaxLineItemsExceeded` is `Error.Validation`.

### Test Updates

- **AC-20**: `Order.Method.Tests.cs` contains 14 test methods (21 - 7 removed).
- **AC-21**: All 14 remaining tests pass.

### Build

- **AC-22**: `dotnet build` passes with 0 warnings.

## 6. Test Automation Strategy

- **Test Levels**: Unit tests for domain logic.
- **Frameworks**: xUnit, FluentAssertions (existing project standards).
- **Test Data Management**: Inline test data via `OrderMethod.Create()` factory.
- **CI/CD Integration**: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Order"` must pass.
- **Coverage Requirements**: Not applicable — dead code removal cannot reduce coverage of surviving code.
- **Performance Testing**: Not applicable.

### Test Removal Rationale

The 7 removed tests exercise only dead methods. Removing them does not reduce coverage of surviving methods. Each test has this lifecycle:

```
AdvanceCheckout_FromAddress_ShouldTransition   → tests AdvanceCheckout (removed)
AdvanceCheckout_WithoutAddress_ShouldFail      → tests AdvanceCheckout (removed)
AdvanceCheckout_DeliveryWithoutMethod_ShouldFail → tests AdvanceCheckout (removed)
AdvanceCheckout_DeliveryWithMethod_ShouldTransition → tests AdvanceCheckout (removed)
AdvanceCheckout_FromComplete_ShouldFail        → tests AdvanceCheckout (removed)
IsPaid_WhenBalanceZero_ShouldReturnTrue        → tests IsPaid (removed)
IsPaid_WhenBalancePositive_ShouldReturnFalse   → tests IsPaid (removed)
```

## 7. Rationale & Context

### Why remove 20 methods?

The 20 dead methods inflate the domain surface by 65% without providing any value. They:

1. **Create maintenance burden**: Every code change to shared types (e.g., `Order`, `Result<T>`, `Error`) must consider impact on dead methods.
2. **Confuse navigation**: Developers searching for "how is the checkout state advanced" find `AdvanceCheckout` — a method that appears to do exactly that but is never called.
3. **Decay into lies**: `Delete()` has a Draft/Expired guard that `DeleteCart` bypasses entirely. The guard exists in code but is never enforced in production, creating a false sense of security.
4. **Waste review time**: Code changes to dead methods must still be reviewed.

### Why remove Ruby aliases specifically?

`CanceledBy`, `ApprovedBy`, `IsPaidCheck`, `GetOutstandingBalance` were ported from a Ruby codebase. They are thin wrappers with zero C# callers. The C# conventions (`Cancel`, `Approve`, `outstandingBalance` property, `IsPaid`) are the canonical API. The aliases add indirection with no benefit.

### Why 4 files instead of 5?

The previous specification proposed 5 files (Factory, StateMachine, Checkout, Computation, Contents). With `Order.Method.Contents.cs` entirely dead and `Order.Method.AddressBook.cs` entirely dead, only 4 concerns remain. `Merge()` moves to StateMachine because cart merging is an order lifecycle transition (guest→user cart association), not general-purpose cart-contents manipulation.

### Why keep `Delete()` when its only feature caller bypasses it?

`Delete()` has one test caller and zero production callers. However, it is kept because:
1. It enforces a valid business rule (only Draft/Expired orders can be deleted).
2. GAP-002 recommends wiring `DeleteCart.cs` to use it.
3. Other entities (Product, Variant) define `Delete()` extension methods — removing it breaks the pattern.

## 8. Dependencies & External Integrations

### Internal Dependencies

- **EXT-001**: `Order.Method.StateMachine.cs` references `OrderResult.Errors` and `OrderResult.Success` (from `Order.Result.cs`).
- **EXT-002**: `Order.Method.Computation.cs` references `AdjustmentConstant.SourceTypes` (from `Adjustment.Constant.cs`).
- **EXT-003**: `Order.Method.StateMachine.cs` `Merge()` references `LineItemConstant.MaxQuantity` (from `LineItem.Constant.cs`) and `LineItem.RecalculateTotal()` (from `LineItem.Method.Compute.cs`).
- **EXT-004**: Callers of `Order.Method.Finalize()` exist in `Order.Seeder.cs` (line 100).
- **EXT-005**: Callers of `Order.Method.Merge()` exist in `AssociateCartWithUser.cs` (line 59).
- **EXT-006**: Callers of `Order.Method.Delete()` exist in `DeleteOrder.cs` (line 37).
- **EXT-007**: Callers of `Order.Method.RecalculateTotals()` exist in 12 feature files (most heavily called domain method).

### Infrastructure Dependencies

- **INF-001**: No new infrastructure dependencies.

### Technology Platform Dependencies

- **PLT-001**: .NET 10, C# preview. Partial classes must compile with `TreatWarningsAsErrors=true`.

## 9. Examples & Edge Cases

### Dead Method Removal — Verification Before Deletion

Before deleting any method, verify zero callers:
```bash
rg "\.AdvanceCheckout\(" service/Api/src/ --type cs
```
Expected: Only the definition line in `Order.Method.cs`. If ANY feature handler calls it, do NOT delete.

### Transitively Dead Verification

Before deleting `UpdateLineItemCurrencies`, verify the ONLY caller is `HomogenizeLineItemCurrencies`:
```bash
rg "UpdateLineItemCurrencies" service/Api/src/ --type cs
```
Expected: Definition in `Order.Method.Currency.cs` + one call site inside `Order.Method.Currency.cs` from `HomogenizeLineItemCurrencies`. If any external caller exists, do NOT delete.

### Order.Method.cs Deletion — No Orphaned Content

After moving all surviving methods out of `Order.Method.cs`:
```bash
wc -l service/Api/src/Module/Ordering/Domain/Orders/Order.Method.cs
```
Expected: 0 (file deleted). If any code remains, a method was missed.

### Test Count Verification

```bash
rg "public void.*Should" service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs | wc -l
```
Expected: 14 (down from 21 after removing 7 dead-method tests).

### Detecting Feature Bypasses (for GAP follow-up)

GAP-001 detection:
```bash
rg "Status = OrderStatus\.Placed" service/Api/src/Module/Ordering/Features/ --type cs
```
Expected: `CreateOrderFromCart.cs` line 109. Any future matches indicate new bypasses.

## 10. Validation Criteria

- **VAL-01**: `dotnet build` exits with code 0 and zero warnings.
- **VAL-02**: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Order"` passes.
- **VAL-03**: Files `Order.Method.AddressBook.cs`, `Order.Method.Currency.cs`, `Order.Method.Contents.cs`, `Order.Method.Merge.cs`, `Order.Method.cs` do not exist.
- **VAL-04**: Files `Order.Method.Factory.cs`, `Order.Method.StateMachine.cs`, `Order.Method.Checkout.cs`, `Order.Method.Computation.cs` exist.
- **VAL-05**: `rg "AdvanceCheckout|IsPaidCheck|GetOutstandingBalance|CanceledBy|ApprovedBy|CanAdvanceTo|AssignDefaultAddresses|EnsureLineItemsPresent|CloneShippingAddress|CloneBillingAddress|SetBillAddressId|SetShipAddressId|ShippingEqualsBillingAddress|AddItem|RemoveItem|RemoveLineItem|HomogenizeLineItemCurrencies|UpdateLineItemCurrencies|UpdateLineItemPrice" service/Api/src/Module/Ordering/Domain/Orders/` returns zero results.
- **VAL-06**: `OrderResult.Errors` does not contain `CannotComplete`, `CannotResume`, `MinimumOrderAmount`, `CartSessionMismatch`, `ShippingRateInvalid`, `PaymentMethodRequired`.
- **VAL-07**: `OrderResult.Errors` contains `MaxLineItemsExceeded`.
- **VAL-08**: `OrderResult.Success` contains `Merged` and `CheckoutAdvanced`.
- **VAL-09**: `OrderConstant.Constraints` contains `MaxLineItems` and `MaxAdjustments`.
- **VAL-10**: `OrderConstant.Defaults` contains `PaymentState` and `ShipmentState`.
- **VAL-11**: `Order.Method.Tests.cs` contains exactly 14 test methods (7 removed, 14 remain).
- **VAL-12**: `rg "IsPaid\b" service/Api/src/Module/Ordering/Domain/Orders/` returns zero results (method removed from domain; property access like `order.IsPaid` would match the deleted extension method, not a property — verify the extension method specifically is gone).

## 11. Related Specifications / Further Reading

- [spec-design-order-domain-convention-fixes.md](./spec-design-order-domain-convention-fixes.md) — Prior specification that migrated files to `Order.Method.{Concern}.cs` naming.
- [docs/codebase/CONVENTIONS.md](../docs/codebase/CONVENTIONS.md) — Domain file naming conventions.
- [.harness/enforcement.yml](../.harness/enforcement.yml) — File size limit enforcement (500 warning, 800 max).
- [.harness/principles.yml](../.harness/principles.yml) — Vertical slice isolation and Result-object rules.
