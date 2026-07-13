# Order Domain — Result Pattern Compliance

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Convert `RecalculateTotals()`, `UpdatePaymentState()`, and `Merge()` from `void` to `Result`, update all 15 call sites.

**Architecture:** Add success factories first, then convert each method signature and update all callers in the same task (signature change + caller updates are atomic — one can't build without the other). Each method gets its own task since they touch different caller sets.

**Tech Stack:** .NET 10 C#, xUnit + FluentAssertions (tests), no new dependencies.

## Global Constraints

- `dotnet build` must pass with `TreatWarningsAsErrors=true` (zero warnings).
- No behavioral change. All 3 methods always succeed — the `Result` wrapper documents the contract.
- No new cross-module references.
- Only Ordering-module callers are updated.

---

### Task 1: Add success factories for `Recalculated` and `PaymentStateUpdated`

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Result.cs`

**Produces:** `OrderResult.Success.Recalculated(Guid)` and `OrderResult.Success.PaymentStateUpdated(Guid)` — consumed by Tasks 2 and 3.

- [ ] **Step 1: Read `Order.Result.cs` to find the `Success` class**

Find the `Success` class. The last existing factories are `Merged(Guid)` and `CheckoutAdvanced(Guid)` (added in the dead-code removal spec). They end at approximately line 36.

- [ ] **Step 2: Add `Recalculated` and `PaymentStateUpdated`**

After the last existing success factory (after `CheckoutAdvanced` or `Merged`), add:

```csharp
        /// <summary>Order totals were recalculated.</summary>
        public static string Recalculated(Guid id) => $"Order with ID '{id}' totals were recalculated.";
        /// <summary>Payment state was derived and updated.</summary>
        public static string PaymentStateUpdated(Guid id) => $"Order with ID '{id}' payment state was updated.";
```

- [ ] **Step 3: Build to verify**

```bash
dotnet build service/Api/src/Module/ --no-restore
```

Expected: PASS (0 warnings).

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Ordering/Domain/Orders/Order.Result.cs
git commit -m "feat: add Recalculated and PaymentStateUpdated success factories"
```

---

### Task 2: Convert `RecalculateTotals` → `Result` + update 13 callers

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Computation.cs`
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.StateMachine.cs`
- Modify: all 11 feature handler files listed below

- [ ] **Step 1: Change `RecalculateTotals` signature to return `Result`**

Read `Order.Method.Computation.cs`. Replace line 9:
```csharp
    public static void RecalculateTotals(this Order order)
```
with:
```csharp
    public static Result RecalculateTotals(this Order order)
```

Add the return statement. Replace line 21 (`}` — the closing brace of the method) with:
```csharp
        return Result.Ok(OrderResult.Success.Recalculated(order.Id));
    }
```

The full method becomes:
```csharp
    public static Result RecalculateTotals(this Order order)
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
        return Result.Ok(OrderResult.Success.Recalculated(order.Id));
    }
```

- [ ] **Step 2: Update the domain-internal caller in `Finalize()`**

Read `Order.Method.StateMachine.cs`. Find line 25: `order.RecalculateTotals();`. Replace that line with:

```csharp
        var recalcResult = order.RecalculateTotals();
        if (recalcResult.IsFailure)
            return recalcResult.Errors;
```

The surrounding context becomes:
```csharp
        order.Status = OrderStatus.Placed;
        order.CompletedAtUtc = DateTimeOffset.UtcNow;
        var recalcResult = order.RecalculateTotals();
        if (recalcResult.IsFailure)
            return recalcResult.Errors;

        return Result.Ok(OrderResult.Success.Finalized(order.Id));
```

- [ ] **Step 3: Update 11 feature-handler callers using the uniform replacement pattern**

For EACH file in the list below, read the file, find `X.RecalculateTotals();` (where `X` is `cart`, `order`, or `targetOrder`), and replace it with:

```csharp
            var recalcResult = X.RecalculateTotals();
            if (recalcResult.IsFailure)
                return recalcResult.Errors;
```

The full list of files and the exact line to find:

| # | File | Find This Exact Line |
|---|------|---------------------|
| 1 | `Features/Storefront/Cart/AddItem/AddToCart.cs` | `                cart.RecalculateTotals();` (line ~116) |
| 2 | `Features/Storefront/Cart/AddItem/AddToCart.cs` | `            cart.RecalculateTotals();` (line ~129) |
| 3 | `Features/Storefront/Cart/EmptyCart/EmptyCart.cs` | `            cart.RecalculateTotals();` |
| 4 | `Features/Storefront/Cart/RemoveItem/RemoveCartItem.cs` | `            cart.RecalculateTotals();` |
| 5 | `Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.cs` | `            cart.RecalculateTotals();` |
| 6 | `Features/Storefront/Cart/UpdateCheckout/UpdateCheckout.cs` | `            cart.RecalculateTotals();` |
| 7 | `Features/Storefront/Cart/SelectShippingRate/SelectShippingRate.cs` | `            cart.RecalculateTotals();` |
| 8 | `Features/Storefront/Cart/AssociateCart/AssociateCartWithUser.cs` | `            targetOrder.RecalculateTotals();` |
| 9 | `Features/Admin/Orders/AddLineItem/AddOrderLineItem.cs` | `                order.RecalculateTotals();` |
| 10 | `Features/Admin/Orders/RemoveLineItem/RemoveOrderLineItem.cs` | `            order.RecalculateTotals();` |
| 11 | `Features/Admin/Orders/UpdateLineItem/UpdateOrderLineItem.cs` | `            order.RecalculateTotals();` |
| 12 | `Features/Admin/Orders/UpdateShippingMethod/UpdateOrderShippingMethod.cs` | `            order.RecalculateTotals();` |

THE REPLACEMENT PATTERN (match the indentation of the original line for the `var` declaration):

```csharp
            var recalcResult = cart.RecalculateTotals();
            if (recalcResult.IsFailure)
                return recalcResult.Errors;
```

For files using `order` instead of `cart`, use that variable name. For `targetOrder`, use `targetOrder.RecalculateTotals()`.

**Note for AddToCart.cs:** This file calls `RecalculateTotals()` TWICE (lines ~116 and ~129). Both must be updated.

- [ ] **Step 4: Build to verify**

```bash
dotnet build service/Api/src/Module/ --no-restore
```

Expected: PASS (0 warnings). If a build error occurs, it's likely a missing `using` import for `Result` — but all feature handlers already import `Result` via the MediatR pipeline.

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "refactor: RecalculateTotals returns Result, update 13 callers"
```

---

### Task 3: Convert `UpdatePaymentState` → `Result` + update 1 caller

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Computation.cs`
- Modify: `service/Api/src/Module/Ordering/Persistence/Seeders/Order.Seeder.cs`

- [ ] **Step 1: Change `UpdatePaymentState` signature to return `Result`**

Read `Order.Method.Computation.cs`. Replace line 23:
```csharp
    public static void UpdatePaymentState(this Order order)
```
with:
```csharp
    public static Result UpdatePaymentState(this Order order)
```

Add the return before the closing brace. Replace line 33 (`    }` — the closing brace of the method) with:
```csharp
        return Result.Ok(OrderResult.Success.PaymentStateUpdated(order.Id));
    }
```

The full method becomes:
```csharp
    public static Result UpdatePaymentState(this Order order)
    {
        if (order.Status == OrderStatus.Canceled && order.PaymentTotal == 0m)
            order.PaymentState = OrderConstant.PaymentState.Void;
        else if (order.OutstandingBalance > 0m)
            order.PaymentState = OrderConstant.PaymentState.BalanceDue;
        else if (order.OutstandingBalance < 0m)
            order.PaymentState = OrderConstant.PaymentState.CreditOwed;
        else
            order.PaymentState = OrderConstant.PaymentState.Paid;
        return Result.Ok(OrderResult.Success.PaymentStateUpdated(order.Id));
    }
```

- [ ] **Step 2: Update the seeder caller**

Read `Order.Seeder.cs`. Find line 114: `order.UpdatePaymentState();`. The seeder is async initialization code that writes entities directly to the database. The Result is intentionally discarded. Replace with:

```csharp
        order.UpdatePaymentState(); // Result unused — seeder writes domain state directly
```

- [ ] **Step 3: Build to verify**

```bash
dotnet build service/Api/src/Module/ --no-restore
```

Expected: PASS.

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "refactor: UpdatePaymentState returns Result, update seeder"
```

---

### Task 4: Convert `Merge` → `Result` + update 1 caller

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.StateMachine.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/AssociateCart/AssociateCartWithUser.cs`

- [ ] **Step 1: Change `Merge` signature to return `Result`**

Read `Order.Method.StateMachine.cs`. Find the `Merge` method (near the end of the file, around line 110). Replace:
```csharp
    public static void Merge(this Order order, Order otherOrder, Guid? userId = null, bool discardMerged = true)
```
with:
```csharp
    public static Result Merge(this Order order, Order otherOrder, Guid? userId = null, bool discardMerged = true)
```

Add the return statement just before the closing `}` of the `Merge` method (before `HandleMerge`). After the `if (discardMerged)` block closes with `}`, on the next line before the method's closing `}`, add:

```csharp
        return Result.Ok(OrderResult.Success.Merged(order.Id));
```

The tail of the `Merge` method becomes:
```csharp
        if (discardMerged)
        {
            otherOrder.LineItems.Clear();
        }
        return Result.Ok(OrderResult.Success.Merged(order.Id));
    }
```

- [ ] **Step 2: Update the `AssociateCartWithUser.cs` caller**

Read `AssociateCartWithUser.cs`. Find line ~59: `userOrder.Merge(guestOrder, userId, discardMerged: true);`. Replace with:

```csharp
                var mergeResult = userOrder.Merge(guestOrder, userId, discardMerged: true);
                if (mergeResult.IsFailure)
                    return (Result<Response>)mergeResult.Errors;
```

Note the cast: `(Result<Response>)mergeResult.Errors` — this handler returns `Result<Response>` via `ICommandHandler<Command, Response>`.

- [ ] **Step 3: Build to verify**

```bash
dotnet build service/Api/src/Module/ --no-restore
```

Expected: PASS.

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "refactor: Merge returns Result, update caller"
```

---

### Task 5: Update unit test to assert Result

**Files:**
- Modify: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs`

- [ ] **Step 1: Update the `RecalculateTotals` test**

Read `Order.Method.Tests.cs`. Find the method `RecalculateTotals_ShouldIncludeLineItemAdjustments`. Find the line `order.RecalculateTotals();` inside that method. Replace it with:

```csharp
        var result = order.RecalculateTotals();
        result.IsSuccess.Should().BeTrue();
```

The test body becomes:
```csharp
        var result = order.RecalculateTotals();
        result.IsSuccess.Should().BeTrue();
        order.AdjustmentTotal.Should().Be(7m);
        order.ItemTotal.Should().Be(10m);
        order.Total.Should().Be(17m);
        order.OutstandingBalance.Should().Be(17m);
```

- [ ] **Step 2: Run the test**

```bash
dotnet test service/Api/tests/Module.UnitTests --no-restore
```

Expected: All tests pass. The `RecalculateTotals_ShouldIncludeLineItemAdjustments` test now asserts the Result.

- [ ] **Step 3: Commit**

```bash
git add -A
git commit -m "test: update RecalculateTotals test to assert Result"
```

---

### Task 6: Full build and verification

- [ ] **Step 1: Full build**

```bash
dotnet build
```

Expected: PASS with zero warnings.

- [ ] **Step 2: Run all unit tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --no-restore
```

Expected: All tests pass.

- [ ] **Step 3: Verify no void mutators remain**

```bash
rg "public static void.*this Order" service/Api/src/Module/Ordering/Domain/Orders/
```

Expected: Zero results.

- [ ] **Step 4: Verify all 3 methods return Result**

```bash
rg "public static Result (RecalculateTotals|UpdatePaymentState|Merge)\b" service/Api/src/Module/Ordering/Domain/Orders/
```

Expected: 3 matches.

- [ ] **Step 5: Verify no bare `RecalculateTotals();` calls remain in features**

```bash
rg "\.RecalculateTotals\(\);" service/Api/src/Module/Ordering/Features/
```

Expected: Zero results (all should now be `var recalcResult = ...` pattern).

- [ ] **Step 6: Final commit if anything remains**

```bash
git status
git add -A
git commit -m "chore: final verification after Result pattern compliance"
```
