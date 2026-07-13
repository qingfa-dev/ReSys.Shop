# Ordering Domain — Feature Handler Domain Logic Extraction

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Extract all domain logic from 22 Ordering feature handlers into 19 new domain methods and 2 enhanced methods, wire 7 handlers to use existing-but-bypassed domain methods, and eliminate all direct entity property mutation from handlers.

**Architecture:** Build domain methods first (no callers yet — methods compile standalone), then wire callers in batches. Error factories, domain methods, and handler updates are each independent build phases. New methods go into `Order.Method.StateMachine.cs`, `Order.Method.Checkout.cs`, and a new `Order.Method.Operations.cs`.

**Tech Stack:** .NET 10 C#, xUnit + FluentAssertions (tests), no new dependencies.

## Global Constraints

- `dotnet build` must pass with `TreatWarningsAsErrors=true` (zero warnings).
- No new cross-module references. New domain methods stay in `Module.Ordering.Domain.Orders`.
- All new mutator methods must return `Result` or `Result<T>`. Query methods return `bool` or `decimal`.
- Persistence operations (`dbContext.Set<>.Add/Remove`, `SaveChangesAsync`) remain in handlers.
- No method may directly set `Order.Status`, `Order.IsDeleted`, `Order.DeletedAtUtc`, `Order.CanceledAtUtc`, `Order.CanceledById` outside domain methods.

---

### Task 1: Add new error factories + enhance `Finalize()` and `Approve()`

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Result.cs`
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.StateMachine.cs`

**Produces:** Error factories for new methods. Enhanced `Finalize()` and `Approve()` used by Tasks 3-9.

- [ ] **Step 1: Add new error factories to `OrderResult.Errors`**

Read `Order.Result.cs`. After the last existing error factory (before `#region Auth`), add:

```csharp
        #region Operations
        /// <summary>Line item with the specified ID was not found on this order.</summary>
        public static Error LineItemNotFound(Guid id) => Error.NotFound(
            code: "Order.LineItem.NotFound",
            message: $"Line item with ID '{id}' was not found on this order.");

        /// <summary>Payment has not been confirmed by the gateway.</summary>
        public static Error PaymentNotConfirmed => Error.Validation(
            code: "Order.Payment.NotConfirmed",
            message: "Payment has not been confirmed by the gateway.");

        /// <summary>Shipping adjustment was not found on this order.</summary>
        public static Error ShippingAdjustmentNotFound => Error.NotFound(
            code: "Order.ShippingAdjustment.NotFound",
            message: "Shipping adjustment was not found on this order.");
        #endregion
```

- [ ] **Step 2: Enhance `Finalize()` in `StateMachine.cs`**

Read `Order.Method.StateMachine.cs`. Find `Finalize()`. After the line `order.RecalculateTotals();` (currently line ~25), add:

```csharp
        order.CheckoutState = CheckoutState.Complete;
```

- [ ] **Step 3: Enhance `Approve()` in `StateMachine.cs`**

Find `Approve()`. After the line `order.ApprovedById = approvedById;`, add:

```csharp
        order.ApprovedAtUtc = DateTimeOffset.UtcNow;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;
```

- [ ] **Step 4: Build and commit**

```bash
dotnet build service/Api/src/Module/ --no-restore
git add -A
git commit -m "feat: add error factories, enhance Finalize and Approve"
```

---

### Task 2: Create `Place()` and `Complete()` in `StateMachine.cs` + update 3 callers

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.StateMachine.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/Complete/CompleteOrder.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateStatus/UpdateOrderStatus.cs`

**Produces:** `Place()` and `Complete()` domain methods. `CreateOrderFromCart` and `CompleteOrder` wired to use them. `UpdateOrderStatus` uses `Finalize()` instead of manual `Status = Placed`.

- [ ] **Step 1: Add `Place()` method to `StateMachine.cs`**

Read `StateMachine.cs`. Add before the closing `#endregion` (before `Delete`):

```csharp
    /// <summary>
    /// Places the order: validates checkout prerequisites, transitions to Placed, assigns order number.
    /// </summary>
    public static Result Place(this Order order, string orderNumber)
    {
        var prerequisites = order.ValidateCheckoutPrerequisites();
        if (prerequisites.IsFailure)
            return prerequisites.Errors;

        order.Status = OrderStatus.Placed;
        order.CheckoutState = CheckoutState.Complete;
        order.CompletedAtUtc = DateTimeOffset.UtcNow;
        order.Number = orderNumber;
        order.RecalculateTotals();

        return Result.Ok(OrderResult.Success.Finalized(order.Id));
    }
```

- [ ] **Step 2: Add `Complete()` method to `StateMachine.cs`**

Add after `Place()`:

```csharp
    /// <summary>
    /// Marks a placed order as complete.
    /// </summary>
    public static Result Complete(this Order order, string modifiedBy)
    {
        if (order.Status != OrderStatus.Placed)
            return OrderResult.Errors.InvalidStatusTransition;

        order.CheckoutState = CheckoutState.Complete;
        order.CompletedAtUtc = DateTimeOffset.UtcNow;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;
        order.ModifiedBy = modifiedBy;

        return Result.Ok(OrderResult.Success.Updated(order.Id));
    }
```

- [ ] **Step 3: Add `ValidateCheckoutPrerequisites()` stub to `Checkout.cs`**

`Place()` calls `ValidateCheckoutPrerequisites()` but it doesn't exist yet (Task 4 creates it). For now, add a stub to avoid build errors. Read `Order.Method.Checkout.cs`. Add at the end of the `sealed partial class Order` (before the closing `}`):

```csharp
    internal Result ValidateCheckoutPrerequisites()
    {
        if (Status == OrderStatus.Canceled)
            return OrderResult.Errors.InvalidStatusTransition;

        if (CheckoutState < CheckoutState.Confirm)
            return OrderResult.Errors.CheckoutNotComplete;

        if (BillAddressId is null || ShipAddressId is null)
            return OrderResult.Errors.AddressRequired;

        if (ShippingMethodId is null)
            return OrderResult.Errors.DeliveryMethodRequired;

        if (string.IsNullOrWhiteSpace(Email))
            return OrderResult.Errors.EmailRequired;

        if (LineItems.Count == 0)
            return OrderResult.Errors.EmptyOrderCannotFinalize;

        return Result.Ok();
    }
```

- [ ] **Step 4: Update `CreateOrderFromCart.cs` to use `Place()`**

Read `CreateOrderFromCart.cs`. Replace lines ~109-115 (the manual `Status = Placed`, `CheckoutState = Complete`, etc.) with:

```csharp
            var placeResult = cart.Place(numberResult.Value);
            if (placeResult.IsFailure)
                return placeResult.Errors;
```

Also remove the manual `cart.PaymentState = OrderConstant.PaymentState.Paid;` at line ~91 and the prerequisite validations at lines ~55-68 (Address, shipping, email checks) since `Place()` handles all of them. The payment verification logic (lines ~71-88) stays for now.

- [ ] **Step 5: Update `CompleteOrder.cs` to use `Complete()`**

Read `CompleteOrder.cs`. Replace lines ~26-33 (the manual status check, `CheckoutState = Complete`, `CompletedAtUtc`, `ModifiedAtUtc`, `ModifiedBy`) with:

```csharp
            var completeResult = order.Complete(currentUser.UserName ?? "System");
            if (completeResult.IsFailure)
                return (Result<Response>)completeResult.Errors;
```

- [ ] **Step 6: Update `UpdateOrderStatus.cs` to use `Finalize()` and `Cancel()`**

Read `UpdateOrderStatus.cs`. Find the status switchboard. Replace the `Placed` branch (lines ~40-43, which sets `entity.Status = Placed` manually) with:

```csharp
                case OrderStatus.Placed when entity.Status == OrderStatus.Draft:
                    var finalizeResult = entity.Finalize();
                    if (finalizeResult.IsFailure)
                        return finalizeResult.Errors;
                    break;
```

Replace the `Canceled` branch (lines ~44-49) with:

```csharp
                case OrderStatus.Canceled when entity.Status == OrderStatus.Placed:
                    var cancelResult = entity.Cancel(Guid.TryParse(currentUser.UserId, out var uid) ? uid : Guid.Empty);
                    if (cancelResult.IsFailure)
                        return cancelResult.Errors;
                    break;
```

- [ ] **Step 7: Build and commit**

```bash
dotnet build service/Api/src/Module/ --no-restore
git add -A
git commit -m "feat: add Place and Complete domain methods, wire 3 handlers"
```

---

### Task 3: Create 10 Checkout domain methods in `Order.Method.Checkout.cs`

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Checkout.cs`

**Produces:** 10 new methods on `OrderMethod` or `Order` partial class. No callers yet — methods compile standalone.

- [ ] **Step 1: Read `Order.Method.Checkout.cs` to understand current structure**

The file currently contains `sealed partial class Order` with instance members (checkout step queries, guard methods, etc.). The new methods will be added as `static partial class OrderMethod` extension methods at the end of the file, before the closing `}` of the `Order` partial class.

**IMPORTANT:** The new methods need to be in `static partial class OrderMethod`, not on the `Order` instance. Since the file currently only has `sealed partial class Order`, add a NEW `static partial class OrderMethod` block after the `Order` class closing `}`.

- [ ] **Step 2: Add all 10 methods at the end of the file**

After the closing `}` of `sealed partial class Order`, add:

```csharp

public static partial class OrderMethod
{
    /// <summary>
    /// Validates all checkout prerequisites are met before placing the order.
    /// </summary>
    public static Result ValidateCheckoutPrerequisites(this Order order)
    {
        if (order.Status == OrderStatus.Canceled)
            return OrderResult.Errors.InvalidStatusTransition;

        if (order.CheckoutState < CheckoutState.Confirm)
            return OrderResult.Errors.CheckoutNotComplete;

        if (order.BillAddressId is null || order.ShipAddressId is null)
            return OrderResult.Errors.AddressRequired;

        if (order.ShippingMethodId is null)
            return OrderResult.Errors.DeliveryMethodRequired;

        if (string.IsNullOrWhiteSpace(order.Email))
            return OrderResult.Errors.EmailRequired;

        if (order.LineItems.Count == 0)
            return OrderResult.Errors.EmptyOrderCannotFinalize;

        return Result.Ok();
    }

    /// <summary>
    /// Marks the order's payment as paid.
    /// </summary>
    public static Result MarkPaymentAsPaid(this Order order)
    {
        order.PaymentState = OrderConstant.PaymentState.Paid;
        return Result.Ok(OrderResult.Success.Updated(order.Id));
    }

    /// <summary>
    /// Updates checkout details on a Draft order. Null values are left unchanged.
    /// </summary>
    public static Result UpdateDetails(this Order order,
        string? email, string? specialInstructions,
        Guid? billAddressId, Guid? shipAddressId, Guid? shippingMethodId)
    {
        if (order.Status != OrderStatus.Draft)
            return OrderResult.Errors.NotDraft;

        if (email is not null) order.Email = email;
        if (specialInstructions is not null) order.SpecialInstructions = specialInstructions;
        if (billAddressId.HasValue) order.BillAddressId = billAddressId;
        if (shipAddressId.HasValue) order.ShipAddressId = shipAddressId;
        if (shippingMethodId.HasValue) order.ShippingMethodId = shippingMethodId;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(OrderResult.Success.Updated(order.Id));
    }

    /// <summary>
    /// Sets the billing address on a Draft order.
    /// </summary>
    public static Result SetBillAddress(this Order order, Guid addressId)
    {
        if (order.Status != OrderStatus.Draft)
            return OrderResult.Errors.NotDraftForBillAddress;

        order.BillAddressId = addressId;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(OrderResult.Success.Updated(order.Id));
    }

    /// <summary>
    /// Sets the shipping address on a Draft order.
    /// </summary>
    public static Result SetShipAddress(this Order order, Guid addressId)
    {
        if (order.Status != OrderStatus.Draft)
            return OrderResult.Errors.NotDraftForShipAddress;

        order.ShipAddressId = addressId;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(OrderResult.Success.Updated(order.Id));
    }

    /// <summary>
    /// Sets the shipping method, resets shipment total, and recalculates.
    /// </summary>
    public static Result SetShippingMethod(this Order order, Guid methodId)
    {
        order.ShippingMethodId = methodId;
        order.ShipmentTotal = 0m;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;
        order.RecalculateTotals();

        return Result.Ok(OrderResult.Success.Updated(order.Id));
    }

    /// <summary>
    /// Returns true if both billing and shipping addresses are set.
    /// </summary>
    public static bool HasAddresses(this Order order) =>
        order.BillAddressId.HasValue && order.ShipAddressId.HasValue;

    /// <summary>
    /// Returns true if a shipping method is selected.
    /// </summary>
    public static bool HasShippingMethod(this Order order) =>
        order.ShippingMethodId.HasValue;

    /// <summary>
    /// Returns true if the order has a non-empty email.
    /// </summary>
    public static bool HasEmail(this Order order) =>
        !string.IsNullOrWhiteSpace(order.Email);

    /// <summary>
    /// Returns true if the order is in Draft status and line items can be modified.
    /// </summary>
    public static bool CanModifyLineItems(this Order order) =>
        order.Status == OrderStatus.Draft;
}
```

- [ ] **Step 3: Remove the `ValidateCheckoutPrerequisites` instance method added in Task 2**

In Task 2 Step 3, we added `internal Result ValidateCheckoutPrerequisites()` as an instance method on the `Order` partial class. Since Task 3 creates it as a static extension method, the instance version is no longer needed. Find and delete it from the `sealed partial class Order` body.

- [ ] **Step 4: Build and commit**

```bash
dotnet build service/Api/src/Module/ --no-restore
git add -A
git commit -m "feat: add 10 Checkout domain methods"
```

---

### Task 4: Create `Order.Method.Operations.cs` with 6 methods

**Files:**
- Create: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Operations.cs`

**Produces:** 6 new domain methods. No callers yet.

- [ ] **Step 1: Create `Order.Method.Operations.cs`**

```csharp
using Module.Ordering.Domain.Adjustments;
using Module.Ordering.Domain.LineItems;

namespace Module.Ordering.Domain.Orders;

public static partial class OrderMethod
{
    #region Operations

    /// <summary>
    /// Validates that the payment amount matches the order total and has been confirmed.
    /// </summary>
    public static Result ValidatePayment(this Order order, decimal paidAmount, bool isConfirmed)
    {
        if (order.Total > 0m && !isConfirmed)
            return OrderResult.Errors.PaymentNotConfirmed;

        if (paidAmount != order.Total)
            return OrderResult.Errors.PaymentAmountMismatch;

        return Result.Ok();
    }

    /// <summary>
    /// Adds a line item to a Draft order, enforces the max-line-items limit, and recalculates totals.
    /// </summary>
    public static Result<LineItem> AddLineItem(this Order order, LineItem lineItem)
    {
        if (order.Status != OrderStatus.Draft)
            return OrderResult.Errors.NotDraftForLineItem;

        if (order.LineItems.Count >= OrderConstant.Constraints.MaxLineItems)
            return OrderResult.Errors.MaxLineItemsExceeded;

        order.LineItems.Add(lineItem);
        order.RecalculateTotals();

        return lineItem;
    }

    /// <summary>
    /// Removes a line item by ID from a Draft order and recalculates totals.
    /// </summary>
    public static Result<LineItem> RemoveLineItem(this Order order, Guid lineItemId)
    {
        if (order.Status != OrderStatus.Draft)
            return OrderResult.Errors.InvalidStatusForLineItemRemove;

        var lineItem = order.LineItems.FirstOrDefault(li => li.Id == lineItemId);
        if (lineItem is null)
            return OrderResult.Errors.LineItemNotFound(lineItemId);

        order.LineItems.Remove(lineItem);
        order.RecalculateTotals();

        return lineItem;
    }

    /// <summary>
    /// Atomically removes all existing shipping adjustments and adds a new one for the given cost.
    /// </summary>
    public static Result ReplaceShippingAdjustment(this Order order, decimal cost, Guid shippingMethodId)
    {
        var toRemove = order.Adjustments
            .Where(a => a.SourceType == AdjustmentConstant.SourceTypes.Shipping)
            .ToList();
        foreach (var adj in toRemove)
            order.Adjustments.Remove(adj);

        var adjResult = AdjustmentMethod.Create(
            label: $"Shipping",
            amount: cost,
            adjustableId: order.Id,
            adjustableType: AdjustmentConstant.AdjustableTypes.Order,
            sourceId: shippingMethodId,
            sourceType: AdjustmentConstant.SourceTypes.Shipping,
            orderId: order.Id);
        if (adjResult.IsFailure)
            return adjResult.Errors;

        order.Adjustments.Add(adjResult.Value);
        order.RecalculateTotals();

        return Result.Ok(OrderResult.Success.Updated(order.Id));
    }

    /// <summary>
    /// Computes total order weight from a variant-weight lookup dictionary.
    /// </summary>
    public static decimal CalculateTotalWeight(this Order order, Dictionary<Guid, decimal> variantWeights)
    {
        return order.LineItems.Sum(li =>
            variantWeights.TryGetValue(li.VariantId, out var weight) ? weight * li.Quantity : 0m);
    }

    /// <summary>
    /// Transfers ownership of a Draft cart to a user, clearing the session.
    /// </summary>
    public static Result TransferOwnership(this Order order, Guid userId)
    {
        if (order.Status != OrderStatus.Draft)
            return OrderResult.Errors.NotDraft;

        order.UserId = userId;
        order.SessionId = null;
        order.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(OrderResult.Success.Updated(order.Id));
    }

    #endregion
}
```

- [ ] **Step 2: Verify `AdjustmentMethod.Create` parameter names**

`ReplaceShippingAdjustment` calls `AdjustmentMethod.Create(label, amount, adjustableId, adjustableType, sourceId, sourceType, orderId)`. The signature is at `Adjustment.Method.Factory.cs`. Verify parameter names match before building. If they differ, read the file and adjust the call.

- [ ] **Step 3: Build and commit**

```bash
dotnet build service/Api/src/Module/ --no-restore
git add -A
git commit -m "feat: create Order.Method.Operations.cs with 6 methods"
```

---

### Task 5: Wire existing domain methods into bypassing handlers (7 handlers)

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/DeleteCart/DeleteCart.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/Delete/DeleteOrder.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Orders/Cancel/CancelOrder.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/ValidateCheckout/ValidateCheckout.cs`

- [ ] **Step 1: `DeleteCart.cs` — use `cart.Delete()`**

Read `DeleteCart.cs`. Find lines `cart.IsDeleted = true; cart.DeletedAtUtc = DateTimeOffset.UtcNow;`. Replace with:

```csharp
            var deleteResult = cart.Delete(currentUser.UserName ?? "System");
            if (deleteResult.IsFailure)
                return deleteResult.Errors;
```

- [ ] **Step 2: `DeleteOrder.cs` — use `order.Delete()`**

Read `DeleteOrder.cs`. Find the inline `IsDeleted = true`, `DeletedAtUtc` mutations. Replace with:

```csharp
            var deleteResult = order.Delete(currentUser.UserName ?? "System");
            if (deleteResult.IsFailure)
                return (Result<Response>)deleteResult.Errors;
```

Remove the redundant status check (`if (order.Status is OrderStatus.Placed)` — already inside `Delete()`).

- [ ] **Step 3: `CancelOrder.cs` — use `entity.Cancel()`**

Read `CancelOrder.cs`. Find lines ~51-55 (manual `Status = Canceled`, `CanceledAtUtc`, `CanceledById`). Replace with:

```csharp
            var cancelResult = entity.Cancel(canceledById);
            if (cancelResult.IsFailure)
                return cancelResult.Errors;
```

Remove the redundant `AlreadyCanceled` check (lines ~46-47 — already inside `Cancel()`).

- [ ] **Step 4: `AddToCart.cs` — use `existingLine.UpdateQuantity()`**

Read `AddToCart.cs`. Find lines ~114-115:
```csharp
                existingLine.Quantity += request.Quantity;
                existingLine.Total = existingLine.Price * existingLine.Quantity;
```
Replace with:
```csharp
                var updateResult = existingLine.UpdateQuantity(existingLine.Quantity + request.Quantity);
                if (updateResult.IsFailure)
                    return updateResult.Errors;
```

- [ ] **Step 5: `UpdateCartItemQuantity.cs` — use `lineItem.UpdateQuantity()`**

Read `UpdateCartItemQuantity.cs`. Find lines ~54-55 (manual `Quantity = `, `Total = Price * Quantity`). Replace with:
```csharp
            var updateResult = lineItem.UpdateQuantity(command.Request.Quantity);
            if (updateResult.IsFailure)
                return updateResult.Errors;
```

- [ ] **Step 6: `ValidateCheckout.cs` — use `cart.CheckoutAllowed()`**

Read `ValidateCheckout.cs`. Find `if (cart.LineItems.Count == 0) return ...`. Replace with:
```csharp
            if (!cart.CheckoutAllowed())
                return OrderResult.Errors.EmptyOrderCannotFinalize;
```

- [ ] **Step 7: Build and commit**

```bash
dotnet build service/Api/src/Module/ --no-restore
git add -A
git commit -m "refactor: wire existing domain methods into 6 bypassing handlers"
```

---

### Task 6: Wire new Checkout domain methods into callers (8 handlers)

**Files:**
- Modify: 8 handler files (listed below)

- [ ] **Step 1: `CreateOrderFromCart.cs` — use `ValidateCheckoutPrerequisites()` and `MarkPaymentAsPaid()`**

Read `CreateOrderFromCart.cs`. 

A) Replace lines ~55-68 (checkout state, address, shipping, email checks) with:
```csharp
            var prereqResult = cart.ValidateCheckoutPrerequisites();
            if (prereqResult.IsFailure)
                return prereqResult.Errors;
```

B) Replace line ~91 (`cart.PaymentState = OrderConstant.PaymentState.Paid`) with:
```csharp
            var paymentMarkResult = cart.MarkPaymentAsPaid();
            if (paymentMarkResult.IsFailure)
                return paymentMarkResult.Errors;
```

C) Replace lines ~71-88 (payment verification: PaymentRequired, PaymentFailed, PaymentAmountMismatch checks) with:
```csharp
            var paymentResult = cart.ValidatePayment(
                payment?.Amount ?? 0m,
                payment?.State == PaymentRecordState.Completed);
            if (paymentResult.IsFailure)
                return paymentResult.Errors;
```

**Important note:** This changes the payment verification flow. The old code queried the payment and checked it inline. The new `ValidatePayment` method takes `decimal + bool`. Read the file first to understand the current payment variable name (`payment`) and adapt accordingly.

D) Remove the redundant check `if (cart.LineItems.Count == 0)` at line ~95 — already inside `Place()`.

- [ ] **Step 2: `UpdateCheckout.cs` — use `UpdateDetails()` and `ReplaceShippingAdjustment()` and `CalculateTotalWeight()`**

Read `UpdateCheckout.cs`.

A) Replace lines ~42-46 (manual `Email`, `BillAddressId`, `ShipAddressId`, `SpecialInstructions` assignments) with:
```csharp
            var updateResult = cart.UpdateDetails(
                req.Email, req.SpecialInstructions,
                req.BillAddressId, req.ShipAddressId, null);
            if (updateResult.IsFailure)
                return updateResult.Errors;
```

B) Replace lines ~49-93 (inline weight computation + shipping adjustment replace block) with:
```csharp
            var totalWeight = cart.CalculateTotalWeight(variantWeights);
            // ... rate calculation from totalWeight stays in handler (orchestration) ...
            var shippingResult = cart.ReplaceShippingAdjustment(selectedRate.Cost, shippingMethodId);
            if (shippingResult.IsFailure)
                return shippingResult.Errors;
```

**Note:** The rate lookup and selection (querying Shipping module for rates) is orchestration that stays in the handler. Only the weight computation and adjustment creation move to domain.

- [ ] **Step 3: `UpdateOrderAdmin.cs` — use `UpdateDetails()`**

Read `UpdateOrderAdmin.cs`. Replace lines ~26-37 (Draft check + manual property assignments) with:

```csharp
            var updateResult = order.UpdateDetails(
                req.Email, req.SpecialInstructions,
                req.BillAddressId, req.ShipAddressId, req.ShippingMethodId);
            if (updateResult.IsFailure)
                return (Result<Response>)updateResult.Errors;
```

- [ ] **Step 4: `UpdateOrderBillAddress.cs` — use `SetBillAddress()`**

Read `UpdateOrderBillAddress.cs`. Replace lines ~26-31 (Draft check + `BillAddressId = ...`) with:

```csharp
            var addressResult = order.SetBillAddress(command.Request.AddressId);
            if (addressResult.IsFailure)
                return (Result<Response>)addressResult.Errors;
```

- [ ] **Step 5: `UpdateOrderShipAddress.cs` — use `SetShipAddress()`**

Read `UpdateOrderShipAddress.cs`. Replace lines ~26-31 (Draft check + `ShipAddressId = ...`) with:

```csharp
            var addressResult = order.SetShipAddress(command.Request.AddressId);
            if (addressResult.IsFailure)
                return (Result<Response>)addressResult.Errors;
```

- [ ] **Step 6: `UpdateOrderShippingMethod.cs` — use `SetShippingMethod()`**

Read `UpdateOrderShippingMethod.cs`. Replace lines ~26-31 (manual `ShippingMethodId`, `ShipmentTotal = 0`, `ModifiedAtUtc`, `RecalculateTotals`) with:

```csharp
            var methodResult = order.SetShippingMethod(command.Request.ShippingMethodId);
            if (methodResult.IsFailure)
                return (Result<Response>)methodResult.Errors;
```

Remove the redundant `RecalculateTotals()` call after — `SetShippingMethod` already does it.

- [ ] **Step 7: `ValidateCheckout.cs` — use `HasAddresses()`, `HasShippingMethod()`, `HasEmail()`**

Read `ValidateCheckout.cs`. Replace lines ~40-49 (3 inline null checks) with:

```csharp
            if (!cart.HasAddresses())
                return OrderResult.Errors.AddressRequired;
            if (!cart.HasShippingMethod())
                return OrderResult.Errors.DeliveryMethodRequired;
            if (!cart.HasEmail())
                return OrderResult.Errors.EmailRequired;
```

- [ ] **Step 8: `UpdateOrderLineItem.cs` — use `CanModifyLineItems()`**

Read `UpdateOrderLineItem.cs`. Replace the inline `if (order.Status != OrderStatus.Draft)` with:

```csharp
            if (!order.CanModifyLineItems())
                return OrderResult.Errors.NotDraftForLineItem;
```

- [ ] **Step 9: Build and commit**

```bash
dotnet build service/Api/src/Module/ --no-restore
git add -A
git commit -m "refactor: wire new Checkout domain methods into 8 handlers"
```

---

### Task 7: Wire new Operations domain methods into callers (5 handlers)

**Files:**
- Modify: 5 handler files (listed below)

- [ ] **Step 1: `AddOrderLineItem.cs` — use `AddLineItem()`**

Read `AddOrderLineItem.cs`. After creating the `LineItem` (via `LineItemMethod.Create(...)`), instead of directly calling `dbContext.Set<LineItem>().Add(lineItem)`, add it through the domain:

```csharp
            var createResult = LineItemMethod.Create(order.Id, command.Request.VariantId, command.Request.Quantity, price);
            if (createResult.IsFailure)
                return (Result<Response>)createResult.Errors;
            var lineItem = createResult.Value;

            var addResult = order.AddLineItem(lineItem);
            if (addResult.IsFailure)
                return (Result<Response>)addResult.Errors;

            dbContext.Set<LineItem>().Add(addResult.Value);
```

Remove the separate `order.RecalculateTotals()` call — `AddLineItem` does it.

- [ ] **Step 2: `RemoveCartItem.cs` — use `RemoveLineItem()`**

Read `RemoveCartItem.cs`. After finding the line item, replace the direct `cart.LineItems.Remove(lineItem)` and `dbContext.Set<LineItem>().Remove()` with:

```csharp
            var removeResult = cart.RemoveLineItem(command.LineItemId);
            if (removeResult.IsFailure)
                return removeResult.Errors;

            dbContext.Set<LineItem>().Remove(removeResult.Value);
```

Remove the separate `cart.RecalculateTotals()` call — `RemoveLineItem` does it.

- [ ] **Step 3: `RemoveOrderLineItem.cs` — use `RemoveLineItem()`**

Same pattern as Step 2:

```csharp
            var removeResult = order.RemoveLineItem(command.LineItemId);
            if (removeResult.IsFailure)
                return (Result<Response>)removeResult.Errors;

            dbContext.Set<LineItem>().Remove(removeResult.Value);
```

Remove the separate `order.RecalculateTotals()` call.

- [ ] **Step 4: `SelectShippingRate.cs` — use `ReplaceShippingAdjustment()` and `CalculateTotalWeight()`**

Read `SelectShippingRate.cs`. Replace lines ~43-51 (inline weight computation loop) and lines ~66-93 (adjustment removal + creation block) with:

```csharp
            var totalWeight = cart.CalculateTotalWeight(variantWeights);
            // ... rate selection from totalWeight stays in handler ...
            var shippingResult = cart.ReplaceShippingAdjustment(selectedRate.Cost, shippingMethodId);
            if (shippingResult.IsFailure)
                return shippingResult.Errors;
```

- [ ] **Step 5: `AssociateCartWithUser.cs` — use `TransferOwnership()`**

Read `AssociateCartWithUser.cs`. Replace lines ~53-54 (`guestOrder.UserId = userId; guestOrder.SessionId = null;`) with:

```csharp
                var transferResult = guestOrder.TransferOwnership(userId);
                if (transferResult.IsFailure)
                    return (Result<Response>)transferResult.Errors;
```

- [ ] **Step 6: Build and commit**

```bash
dotnet build service/Api/src/Module/ --no-restore
git add -A
git commit -m "refactor: wire new Operations domain methods into 5 handlers"
```

---

### Task 8: Remove `ApproveOrder.cs` external timestamp mutations

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/Approve/ApproveOrder.cs`

- [ ] **Step 1: Remove handler-side timestamp mutations**

Read `ApproveOrder.cs`. Find lines ~36-37 (sets `ApprovedAtUtc` and `ModifiedAtUtc` after calling `order.Approve()`). These are now set inside `Approve()` (Task 1 Step 3). Delete those two lines.

- [ ] **Step 2: Build and commit**

```bash
dotnet build service/Api/src/Module/ --no-restore
git add -A
git commit -m "refactor: remove handler-side timestamps from ApproveOrder"
```

---

### Task 9: Write unit tests for all new domain methods

**Files:**
- Modify: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs`
- Modify: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/OrderCheckoutTests.cs`

- [ ] **Step 1: Add tests for `Place()` and `Complete()`**

Append to `Order.Method.Tests.cs`:

```csharp
    [Fact]
    public void Place_WithValidPrerequisites_ShouldSucceed()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.CheckoutState = CheckoutState.Confirm;
        order.BillAddressId = Guid.NewGuid();
        order.ShipAddressId = Guid.NewGuid();
        order.ShippingMethodId = Guid.NewGuid();
        order.Email = "test@test.com";
        order.LineItems.Add(new() { Quantity = 1, Price = 10 });
        var r = order.Place("R20260713-1A2B3C4D");
        r.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Placed);
        order.CheckoutState.Should().Be(CheckoutState.Complete);
        order.Number.Should().Be("R20260713-1A2B3C4D");
    }

    [Fact]
    public void Place_MissingAddresses_ShouldFail()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.CheckoutState = CheckoutState.Confirm;
        order.ShippingMethodId = Guid.NewGuid();
        order.Email = "test@test.com";
        order.LineItems.Add(new() { Quantity = 1, Price = 10 });
        var r = order.Place("R-test");
        r.IsFailure.Should().BeTrue();
        r.Errors[0].Should().Be(OrderResult.Errors.AddressRequired);
    }

    [Fact]
    public void Complete_WhenPlaced_ShouldSucceed()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.LineItems.Add(new() { Quantity = 1, Price = 10 });
        order.Finalize();
        var r = order.Complete("tester");
        r.IsSuccess.Should().BeTrue();
        order.CheckoutState.Should().Be(CheckoutState.Complete);
    }

    [Fact]
    public void Complete_WhenDraft_ShouldFail()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        var r = order.Complete("tester");
        r.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void UpdateDetails_WhenDraft_ShouldSucceed()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        var r = order.UpdateDetails("a@b.com", "Handle with care", null, null, null);
        r.IsSuccess.Should().BeTrue();
        order.Email.Should().Be("a@b.com");
        order.SpecialInstructions.Should().Be("Handle with care");
    }

    [Fact]
    public void UpdateDetails_WhenPlaced_ShouldFail()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.LineItems.Add(new() { Quantity = 1, Price = 10 });
        order.Finalize();
        var r = order.UpdateDetails("a@b.com", null, null, null, null);
        r.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void SetBillAddress_WhenDraft_ShouldSucceed()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        var id = Guid.NewGuid();
        var r = order.SetBillAddress(id);
        r.IsSuccess.Should().BeTrue();
        order.BillAddressId.Should().Be(id);
    }

    [Fact]
    public void SetShipAddress_WhenPlaced_ShouldFail()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.LineItems.Add(new() { Quantity = 1, Price = 10 });
        order.Finalize();
        var r = order.SetShipAddress(Guid.NewGuid());
        r.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void SetShippingMethod_ShouldResetShipmentTotal()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.ShipmentTotal = 99m;
        var r = order.SetShippingMethod(Guid.NewGuid());
        r.IsSuccess.Should().BeTrue();
        order.ShipmentTotal.Should().Be(0m);
    }

    [Fact]
    public void AddLineItem_WhenDraft_ShouldSucceed()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        var lineItem = LineItemMethod.Create(order.Id, Guid.NewGuid(), 2, 15m).Value;
        var r = order.AddLineItem(lineItem);
        r.IsSuccess.Should().BeTrue();
        order.LineItems.Should().Contain(lineItem);
    }

    [Fact]
    public void RemoveLineItem_ShouldRemoveAndRecalculate()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        var lineItem = LineItemMethod.Create(order.Id, Guid.NewGuid(), 1, 10m).Value;
        order.LineItems.Add(lineItem);
        order.ItemTotal = 10m;
        var r = order.RemoveLineItem(lineItem.Id);
        r.IsSuccess.Should().BeTrue();
        order.LineItems.Should().BeEmpty();
    }

    [Fact]
    public void RemoveLineItem_WhenNotFound_ShouldFail()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        var r = order.RemoveLineItem(Guid.NewGuid());
        r.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void TransferOwnership_WhenDraft_ShouldSucceed()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        var userId = Guid.NewGuid();
        var r = order.TransferOwnership(userId);
        r.IsSuccess.Should().BeTrue();
        order.UserId.Should().Be(userId);
        order.SessionId.Should().BeNull();
    }

    [Fact]
    public void HasAddresses_WhenBothSet_ShouldReturnTrue()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.BillAddressId = Guid.NewGuid();
        order.ShipAddressId = Guid.NewGuid();
        order.HasAddresses().Should().BeTrue();
    }

    [Fact]
    public void HasAddresses_WhenMissing_ShouldReturnFalse()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.BillAddressId = Guid.NewGuid();
        order.HasAddresses().Should().BeFalse();
    }

    [Fact]
    public void CanModifyLineItems_WhenDraft_ShouldReturnTrue()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.CanModifyLineItems().Should().BeTrue();
    }

    [Fact]
    public void CanModifyLineItems_WhenPlaced_ShouldReturnFalse()
    {
        var order = OrderMethod.Create("USD", null, Guid.NewGuid()).Value;
        order.LineItems.Add(new() { Quantity = 1, Price = 10 });
        order.Finalize();
        order.CanModifyLineItems().Should().BeFalse();
    }
```

- [ ] **Step 2: Add `using Module.Ordering.Domain.LineItems;` and `using Module.Ordering.Domain.Adjustments;`**

At the top of `Order.Method.Tests.cs`, add:
```csharp
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Adjustments;
```

- [ ] **Step 3: Run tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --no-restore
```

Expected: All tests pass (including the 16 new ones + existing 14).

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "test: add 16 tests for new domain methods"
```

---

### Task 10: Build, test, and verify all handler mutations are extracted

- [ ] **Step 1: Full build**

```bash
dotnet build
```

Expected: PASS with zero warnings.

- [ ] **Step 2: Run all tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --no-restore
```

Expected: All tests pass.

- [ ] **Step 3: Verify no handler directly sets `IsDeleted`**

```bash
rg "\.IsDeleted = true" service/Api/src/Module/Ordering/Features/
```

Expected: Zero results.

- [ ] **Step 4: Verify no handler directly sets `Status` on Order**

```bash
rg "\.Status = OrderStatus\." service/Api/src/Module/Ordering/Features/
```

Expected: Zero results.

- [ ] **Step 5: Verify no handler directly sets `CanceledAtUtc` or `CanceledById`**

```bash
rg "CanceledAtUtc =|CanceledById =" service/Api/src/Module/Ordering/Features/
```

Expected: Zero results.

- [ ] **Step 6: Verify no handler directly sets `LineItem.Total = ...` (manual computation)**

```bash
rg "\.Total = .* Price \*|\.Total = .* Quantity" service/Api/src/Module/Ordering/Features/
```

Expected: Zero results.

- [ ] **Step 7: Verify new files and methods exist**

```bash
ls service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Operations.cs
rg "public static Result Place\b" service/Api/src/Module/Ordering/Domain/Orders/
rg "public static Result Complete\b" service/Api/src/Module/Ordering/Domain/Orders/
rg "public static Result UpdateDetails\b" service/Api/src/Module/Ordering/Domain/Orders/
rg "public static Result ReplaceShippingAdjustment\b" service/Api/src/Module/Ordering/Domain/Orders/
```

Expected: All files and methods exist.

- [ ] **Step 8: Final commit if anything remains**

```bash
git status
git add -A
git commit -m "chore: final verification after domain logic extraction"
```
