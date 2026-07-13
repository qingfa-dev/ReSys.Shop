# Order Domain — Dead Code Removal & Method Surface Optimization

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove 20 dead methods, 3 dead files, 4 Ruby aliases, and 6 dead error constants from the Order domain; consolidate 10 surviving methods into 4 concern files; add missing constants and result factories.

**Architecture:** Create new `Order.Method.{Concern}.cs` partial class files with extracted surviving methods first (so the partial class always compiles), then delete old files and dead methods. `Order.Method.cs` is deleted entirely — its content is moved to Factory/StateMachine/Computation or removed.

**Tech Stack:** .NET 10 C#, xUnit + FluentAssertions (tests), no new dependencies.

## Global Constraints

- `dotnet build` must pass with `TreatWarningsAsErrors=true` (zero warnings).
- No new cross-module references.
- No behavioral change to surviving methods. Method bodies are extracted verbatim.
- Deleted methods must leave no orphaned `using` statements, `#region`/`#endregion` directives, or empty partial class declarations.
- The `OrderMethod` is a `static partial class` — methods can be moved between partial files without code changes.

---

### Task 1: Add constants, result factories, and remove dead errors

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Constant.cs`
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Result.cs`

**Produces:** New constants and result factories consumed by surviving methods in Tasks 3-5. Dead errors removed.

- [ ] **Step 1: Add constants to `OrderConstant.Constraints`**

Read `Order.Constant.cs`. Find the `Constraints` class. After the line `public const int Scale = 2;`, add:

```csharp
        public const int MaxLineItems = 100;
```

After that line, add:

```csharp
        public const int MaxAdjustments = 50;
```

- [ ] **Step 2: Add constants to `OrderConstant.Defaults`**

In the `Defaults` class, after the line `public const string CreatedBy = "System";`, add:

```csharp
        public const string PaymentState = PaymentStateConstants.Pending;
        public const string ShipmentState = ShipmentStateConstants.Pending;
```

- [ ] **Step 3: Remove 6 dead error constants from `OrderResult.Errors`**

Read `Order.Result.cs`. Delete these exact blocks of lines:

1. Lines 74-77 (`CannotComplete` — 5 lines including the XML doc comment)
2. Lines 79-82 (`CannotResume` — 5 lines)
3. Lines 126-129 (`PaymentMethodRequired` — 5 lines)
4. Lines 131-134 (`MinimumOrderAmount` — 5 lines)
5. Lines 141-144 (`ShippingRateInvalid` — 5 lines)
6. Lines 146-149 (`CartSessionMismatch` — 5 lines)

The exact text to delete for block 1:
```
        /// <summary>Only placed orders can be completed.</summary>
        public static Error CannotComplete => Error.Validation(
            code: "Order.CannotComplete",
            message: "Only placed orders can be completed.");

```

Repeat the same pattern for blocks 2-6, matching their exact content.

- [ ] **Step 4: Add success factories to `OrderResult.Success`**

Read `Order.Result.cs`. Find the `Success` class. After the line `public static string Completed(Guid id, string by) => ...`, add:

```csharp
        /// <summary>Guest cart was merged into user cart.</summary>
        public static string Merged(Guid id) => $"Order with ID '{id}' was successfully merged.";
        /// <summary>Checkout step was advanced.</summary>
        public static string CheckoutAdvanced(Guid id) => $"Order with ID '{id}' checkout step was advanced.";
```

- [ ] **Step 5: Add error factory to `OrderResult.Errors`**

Find the `Errors` class. After the `#region OrderNumber` block (which contains `OrderNumberGenerationFailed`), before `#region Auth`, add:

```csharp
        #region Constraints
        /// <summary>Order has reached the maximum number of line items.</summary>
        public static Error MaxLineItemsExceeded => Error.Validation(
            code: "Order.LineItems.MaxExceeded",
            message: $"Order cannot have more than {OrderConstant.Constraints.MaxLineItems} line items.");
        #endregion
```

- [ ] **Step 6: Build to verify**

```bash
dotnet build service/Api/src/Module/ --no-restore
```

Expected: PASS (0 warnings).

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "refactor: add missing constants, result factories, remove 6 dead errors"
```

---

### Task 2: Remove dead methods from `Order.Method.Checkout.cs`

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Checkout.cs`

**Removes:** `AssignDefaultAddresses()` and `EnsureLineItemsPresent()` — both have zero production callers.

- [ ] **Step 1: Read the file to find exact content**

```bash
rg -n "AssignDefaultAddresses\|EnsureLineItemsPresent" service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Checkout.cs
```

Expected: 2 matches — the definitions of these methods.

- [ ] **Step 2: Delete `AssignDefaultAddresses`**

Read the file. Find the method `internal void AssignDefaultAddresses(Guid? billAddressId, Guid? shipAddressId)`. Delete the entire method including its XML doc comment and the preceding blank line.

- [ ] **Step 3: Delete `EnsureLineItemsPresent`**

Find the method `internal bool EnsureLineItemsPresent()`. Delete the entire method including the preceding blank line.

- [ ] **Step 4: Verify zero callers before deleting**

```bash
rg "AssignDefaultAddresses\|EnsureLineItemsPresent" service/Api/src/ --type cs | grep -v "Method.Checkout.cs"
```

Expected: Zero results.

- [ ] **Step 5: Build to verify**

```bash
dotnet build service/Api/src/Module/ --no-restore
```

Expected: PASS (0 warnings).

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "refactor: remove dead methods from Order.Method.Checkout.cs"
```

---

### Task 3: Create `Order.Method.StateMachine.cs` + delete `Order.Method.Merge.cs`

**Files:**
- Create: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.StateMachine.cs`
- Delete: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Merge.cs`

**Interfaces:**
- Produces: `OrderMethod.Finalize(Order)`, `OrderMethod.Cancel(Order, Guid)`, `OrderMethod.Resume(Order)`, `OrderMethod.Approve(Order, Guid)`, `OrderMethod.Empty(Order)`, `OrderMethod.Delete(Order, string)`, `OrderMethod.Merge(Order, Order, Guid?, bool)`

**Consumes:** `OrderResult.Errors`, `OrderResult.Success`, `LineItem.RecalculateTotal()`, `LineItemConstant.MaxQuantity` — all already exist.

- [ ] **Step 1: Create `Order.Method.StateMachine.cs`**

Create file `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.StateMachine.cs` with this content:

```csharp
using Module.Ordering.Domain.LineItems;

namespace Module.Ordering.Domain.Orders;

public static partial class OrderMethod
{
    #region State Machine

    /// <summary>
    /// Finalizes the order by transitioning it to Placed status.
    /// </summary>
    public static Result Finalize(this Order order)
    {
        if (order.Status == OrderStatus.Canceled)
            return OrderResult.Errors.AlreadyCanceled;

        if (order.Status == OrderStatus.Placed)
            return OrderResult.Errors.AlreadyFinalized;

        if (order.LineItems.Count == 0)
            return OrderResult.Errors.EmptyOrderCannotFinalize;

        order.Status = OrderStatus.Placed;
        order.CompletedAtUtc = DateTimeOffset.UtcNow;
        order.RecalculateTotals();

        return Result.Ok(OrderResult.Success.Finalized(order.Id));
    }

    /// <summary>
    /// Cancels a placed order and records the canceler.
    /// </summary>
    public static Result Cancel(this Order order, Guid canceledById)
    {
        if (order.Status == OrderStatus.Canceled)
            return OrderResult.Errors.AlreadyCanceled;

        if (order.Status == OrderStatus.Draft)
            return OrderResult.Errors.InvalidStatusTransition;

        order.Status = OrderStatus.Canceled;
        order.CanceledAtUtc = DateTimeOffset.UtcNow;
        order.CanceledById = canceledById;

        return Result.Ok(OrderResult.Success.Canceled(order.Id));
    }

    /// <summary>
    /// Resumes a previously canceled order, restoring it to placed status.
    /// </summary>
    public static Result Resume(this Order order)
    {
        if (order.Status != OrderStatus.Canceled)
            return OrderResult.Errors.InvalidStatusTransition;

        order.Status = OrderStatus.Placed;
        order.CanceledAtUtc = null;
        order.CanceledById = null;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(OrderResult.Success.Resumed(order.Id));
    }

    /// <summary>
    /// Approves a placed order and records the approver.
    /// </summary>
    public static Result Approve(this Order order, Guid approvedById)
    {
        if (order.Status == OrderStatus.Canceled)
            return OrderResult.Errors.AlreadyCanceled;

        if (order.ApprovedById.HasValue)
            return OrderResult.Errors.AlreadyApproved;

        order.ApprovedById = approvedById;

        return Result.Ok(OrderResult.Success.Approved(order.Id));
    }

    /// <summary>
    /// Empties the order by clearing all line items, adjustments, and resetting totals to zero.
    /// </summary>
    public static Result Empty(this Order order)
    {
        if (order.Status == OrderStatus.Placed)
            return OrderResult.Errors.InvalidStatusTransition;

        order.LineItems.Clear();
        order.ItemCount = 0;
        order.Adjustments.Clear();
        order.ItemTotal = 0m;
        order.AdjustmentTotal = 0m;
        order.ShipmentTotal = 0m;
        order.Total = 0m;
        order.PaymentTotal = 0m;
        order.OutstandingBalance = 0m;

        return Result.Ok(OrderResult.Success.Emptied(order.Id));
    }

    /// <summary>
    /// Soft-deletes the order by marking it as deleted.
    /// </summary>
    public static Result Delete(this Order order, string deletedBy)
    {
        if (order.Status != OrderStatus.Draft && order.Status != OrderStatus.Expired)
            return OrderResult.Errors.InvalidStatusForDelete;

        if (order.IsDeleted)
        {
            return Result.Ok();
        }

        order.IsDeleted = true;
        order.DeletedAtUtc = DateTimeOffset.UtcNow;
        order.DeletedBy = deletedBy;

        return Result.Ok();
    }

    /// <summary>
    /// Merges the other order into this order, combining matching line items by variant ID.
    /// </summary>
    public static void Merge(this Order order, Order otherOrder, Guid? userId = null, bool discardMerged = true)
    {
        foreach (var otherLineItem in otherOrder.LineItems)
        {
            var matchingLineItem = order.LineItems
                .FirstOrDefault(myLi => myLi.VariantId == otherLineItem.VariantId);
            HandleMerge(order, matchingLineItem, otherLineItem);
        }

        if (userId.HasValue)
        {
            order.UserId = userId;
        }

        if (discardMerged)
        {
            otherOrder.LineItems.Clear();
        }
    }

    private static void HandleMerge(Order order, LineItem? currentLineItem, LineItem otherLineItem)
    {
        if (currentLineItem is not null)
        {
            if (currentLineItem.Quantity + otherLineItem.Quantity > LineItemConstant.MaxQuantity)
                return;
            currentLineItem.Quantity += otherLineItem.Quantity;
            currentLineItem.RecalculateTotal();
        }
        else
        {
            otherLineItem.OrderId = order.Id;
            order.LineItems.Add(otherLineItem);
        }
    }

    #endregion
}
```

- [ ] **Step 2: Delete `Order.Method.Merge.cs`**

```bash
rm service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Merge.cs
```

- [ ] **Step 3: Build to verify**

```bash
dotnet build service/Api/src/Module/ --no-restore
```

Expected: PASS. (The `OrderMethod` methods in `StateMachine.cs` duplicate those still in `Order.Method.cs` — but C# partial classes allow this. Build should succeed with duplicate method definitions? **No!** Duplicate method signatures in the same partial class will cause compiler error.)

**IMPORTANT:** Before building, the now-duplicated methods in `Order.Method.cs` must be removed first. These methods are `Finalize`, `Cancel`, `Resume`, `Approve`, `Empty`, `Delete` — all currently in Order.Method.cs lines 102-298. Let me add this step.

- [ ] **Step 3a: Remove the now-duplicated methods from `Order.Method.cs`**

Read `Order.Method.cs`. Remove exactly these method blocks (they are now in `StateMachine.cs`):

- `Finalize` (lines 96-122 — whole method + doc comment + blank line before)
- `Cancel` (lines 124-147 — whole method + doc comment + blank line before)
- `Resume` (lines 149-167 — whole method + doc comment + blank line before)
- `Approve` (lines 169-189 — whole method + doc comment + blank line before)
- `Empty` (lines 191-215 — whole method + doc comment + blank line before)
- `Delete` (lines 276-297 — whole method + doc comment + blank line before)
- Keep `RecalculateTotals`, `UpdatePaymentState`, `Create`, dead methods for now

Additionally, remove:
- The `#region State Machine` directive at line 50 if it exists (now empty after removing the state machine methods)
- The `#endregion` at line 298

- [ ] **Step 3b: Build**

```bash
dotnet build service/Api/src/Module/ --no-restore
```

Expected: PASS (0 warnings). The removed methods exist in `StateMachine.cs`, so the partial class is complete.

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "refactor: create Order.Method.StateMachine.cs, fold Merge, delete Order.Method.Merge.cs"
```

---

### Task 4: Create `Order.Method.Computation.cs`

**Files:**
- Create: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Computation.cs`
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.cs` (remove `RecalculateTotals` and `UpdatePaymentState`)

- [ ] **Step 1: Create `Order.Method.Computation.cs`**

Create file `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Computation.cs`:

```csharp
using Module.Ordering.Domain.Adjustments;

namespace Module.Ordering.Domain.Orders;

public static partial class OrderMethod
{
    #region Computations

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

    public static void UpdatePaymentState(this Order order)
    {
        if (order.Status == OrderStatus.Canceled && order.PaymentTotal == 0m)
            order.PaymentState = OrderConstant.PaymentState.Void;
        else if (order.OutstandingBalance > 0m)
            order.PaymentState = OrderConstant.PaymentState.BalanceDue;
        else if (order.OutstandingBalance < 0m)
            order.PaymentState = OrderConstant.PaymentState.CreditOwed;
        else
            order.PaymentState = OrderConstant.PaymentState.Paid;
    }

    #endregion
}
```

- [ ] **Step 2: Remove `RecalculateTotals` and `UpdatePaymentState` from `Order.Method.cs`**

Read `Order.Method.cs`. Remove:
- `RecalculateTotals` method (lines 217-230 — whole method + comment line + blank line before)
- `UpdatePaymentState` method (lines 341-351 — whole method + doc comment + blank line before)
- The `#region State Derivations` directive (line 335) and its `#endregion` (line 352) — now empty

- [ ] **Step 3: Build**

```bash
dotnet build service/Api/src/Module/ --no-restore
```

Expected: PASS. The methods now live in `Computation.cs`.

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "refactor: create Order.Method.Computation.cs"
```

---

### Task 5: Create `Order.Method.Factory.cs`

**Files:**
- Create: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Factory.cs`
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.cs` (remove `Create`)

- [ ] **Step 1: Create `Order.Method.Factory.cs`**

Create file `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Factory.cs`:

```csharp
namespace Module.Ordering.Domain.Orders;

public static partial class OrderMethod
{
    #region Factory

    public static Result<Order> Create(
        string currency,
        Guid? userId,
        Guid storeId,
        Guid? id = null,
        string? sessionId = null,
        Guid? shipAddressId = null)
    {
        var order = new Order
        {
            Id = id ?? Guid.NewGuid(),
            Number = $"DRAFT-{Guid.NewGuid():N}",
            SessionId = sessionId,
            Status = OrderStatus.Draft,
            CheckoutState = CheckoutState.Address,
            Currency = currency,
            UserId = userId,
            StoreId = storeId,
            ShipAddressId = shipAddressId,
            ItemTotal = 0m,
            AdjustmentTotal = 0m,
            ShipmentTotal = 0m,
            Total = 0m,
            PaymentTotal = 0m,
            OutstandingBalance = 0m,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = OrderConstant.Defaults.CreatedBy
        };

        return order;
    }

    #endregion
}
```

- [ ] **Step 2: Remove `Create` from `Order.Method.cs`**

Read `Order.Method.cs`. Remove:
- The entire `#region Factory Methods` block (lines 7-48): `Create` method + doc comment + the `#region`/`#endregion` directives

- [ ] **Step 3: Build**

```bash
dotnet build service/Api/src/Module/ --no-restore
```

Expected: PASS.

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "refactor: create Order.Method.Factory.cs"
```

---

### Task 6: Delete remaining dead files + clean up `Order.Method.cs`

**Files:**
- Delete: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.AddressBook.cs`
- Delete: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Currency.cs`
- Delete: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Contents.cs`
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.cs` (remove remaining dead methods)
- Delete: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.cs` (after all content is removed)

**Note:** After Tasks 3-5, `Order.Method.cs` should contain only: `AdvanceCheckout`, `IsPaid`, `IsPaidCheck`, `GetOutstandingBalance`, `CanceledBy`, `ApprovedBy`, `CanAdvanceTo`, and any remaining `#region`/`#endregion` directives. All of these are dead.

- [ ] **Step 1: Delete the 3 dead whole files**

```bash
rm service/Api/src/Module/Ordering/Domain/Orders/Order.Method.AddressBook.cs
rm service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Currency.cs
rm service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Contents.cs
```

- [ ] **Step 2: Verify `Order.Method.cs` only contains dead methods**

Read `Order.Method.cs` at this point. If it's entirely dead methods (AdvanceCheckout, IsPaid, IsPaidCheck, GetOutstandingBalance, CanceledBy, ApprovedBy, CanAdvanceTo + any remaining `#region`/`#endregion` directives and the `using Module.Ordering.Domain.Adjustments;` import), then delete the entire file.

- [ ] **Step 3: Delete `Order.Method.cs`**

```bash
rm service/Api/src/Module/Ordering/Domain/Orders/Order.Method.cs
```

- [ ] **Step 4: Build to verify**

```bash
dotnet build service/Api/src/Module/ --no-restore
```

Expected: PASS (0 warnings). The partial class now consists of `Factory.cs`, `StateMachine.cs`, `Checkout.cs`, and `Computation.cs`.

- [ ] **Step 5: Verify no remaining `using Module.Ordering.Domain.Adjustments;` is orphaned**

```bash
rg "using Module.Ordering.Domain.Adjustments;" service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Computation.cs
```

Expected: Exactly 1 result (it moved to `Computation.cs`).

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "refactor: delete dead Method files and Order.Method.cs"
```

---

### Task 7: Remove dead test methods

**Files:**
- Modify: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs`

**Removes:** 7 test methods that exercise dead methods.

- [ ] **Step 1: Read the test file**

```bash
wc -l service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs
```

Expected: 245 lines, 21 test methods.

- [ ] **Step 2: Delete 5 `AdvanceCheckout_*` test methods**

Delete lines 24-79 from the test file. This removes all methods between `public void Create_WithValidParams_ShouldReturn()` and `public void Finalize_WithItems_ShouldSucceed()` — specifically:
- `AdvanceCheckout_FromAddress_ShouldTransition` (lines 24-34)
- `AdvanceCheckout_WithoutAddress_ShouldFail` (lines 36-44)
- `AdvanceCheckout_DeliveryWithoutMethod_ShouldFail` (lines 46-56)
- `AdvanceCheckout_DeliveryWithMethod_ShouldTransition` (lines 58-69)
- `AdvanceCheckout_FromComplete_ShouldFail` (lines 71-79)

- [ ] **Step 3: Delete 2 `IsPaid_*` test methods**

Delete lines 164-178 (after `public void Empty_ShouldClearItemsAndTotals()`, before `public void Delete_WhenDraft_ShouldSucceed()`):
- `IsPaid_WhenBalanceZero_ShouldReturnTrue` (lines 164-170)
- `IsPaid_WhenBalancePositive_ShouldReturnFalse` (lines 172-178)

- [ ] **Step 4: Verify correct test count**

```bash
rg "public void.*Should" service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs | wc -l
```

Expected: 14 (21 - 7 = 14 remaining).

- [ ] **Step 5: Run the remaining tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~OrderMethod" --no-restore
```

Expected: 14/14 pass.

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "test: remove 7 tests for dead Order methods"
```

---

### Task 8: Full build and verification

- [ ] **Step 1: Full build**

```bash
dotnet build
```

Expected: PASS with zero warnings.

- [ ] **Step 2: Run all Order-related tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Order" --no-restore
```

Expected: All surviving tests pass.

- [ ] **Step 3: Run all unit tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --no-restore
```

Expected: All tests pass.

- [ ] **Step 4: Validation — verify dead files are deleted**

```bash
ls service/Api/src/Module/Ordering/Domain/Orders/Order.Method.AddressBook.cs 2>&1
ls service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Currency.cs 2>&1
ls service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Contents.cs 2>&1
ls service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Merge.cs 2>&1
ls service/Api/src/Module/Ordering/Domain/Orders/Order.Method.cs 2>&1
```

Expected: All "No such file or directory".

- [ ] **Step 5: Validation — verify new files exist**

```bash
ls service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Factory.cs
ls service/Api/src/Module/Ordering/Domain/Orders/Order.Method.StateMachine.cs
ls service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Checkout.cs
ls service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Computation.cs
```

Expected: All exist.

- [ ] **Step 6: Validation — verify dead methods are gone**

```bash
rg "AdvanceCheckout|IsPaidCheck|GetOutstandingBalance|CanceledBy|ApprovedBy|CanAdvanceTo|AssignDefaultAddresses|EnsureLineItemsPresent|CloneShippingAddress|CloneBillingAddress|SetBillAddressId|SetShipAddressId|ShippingEqualsBillingAddress|AddItem\b|RemoveItem\b|RemoveLineItem\b|HomogenizeLineItemCurrencies|UpdateLineItemCurrencies|UpdateLineItemPrice" service/Api/src/Module/Ordering/Domain/Orders/ | wc -l
```

Expected: 0.

- [ ] **Step 7: Validation — verify dead errors are removed**

```bash
rg "CannotComplete|CannotResume|MinimumOrderAmount|CartSessionMismatch|ShippingRateInvalid|PaymentMethodRequired" service/Api/src/Module/Ordering/Domain/Orders/Order.Result.cs | wc -l
```

Expected: 0.

- [ ] **Step 8: Validation — verify new constants exist**

```bash
rg "MaxLineItems|MaxAdjustments" service/Api/src/Module/Ordering/Domain/Orders/Order.Constant.cs | wc -l
rg "PaymentState = PaymentStateConstants|ShipmentState = ShipmentStateConstants" service/Api/src/Module/Ordering/Domain/Orders/Order.Constant.cs | wc -l
```

Expected: 2 lines each.

- [ ] **Step 9: Validation — verify new results exist**

```bash
rg "Merged\(Guid|CheckoutAdvanced\(Guid" service/Api/src/Module/Ordering/Domain/Orders/Order.Result.cs | wc -l
rg "MaxLineItemsExceeded" service/Api/src/Module/Ordering/Domain/Orders/Order.Result.cs | wc -l
```

Expected: 2 and 1.

- [ ] **Step 10: Final commit if anything remains**

```bash
git status
git add -A
git commit -m "chore: final verification after Order domain dead-code removal"
```
