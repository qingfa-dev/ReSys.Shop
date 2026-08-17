# Order Cross-Module State Synchronization Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the Order's derived state (`PaymentState`, `PaymentTotal`, `OutstandingBalance`, `ShipmentState`) and related entities (payments, shipments, stock) update correctly across modules at runtime.

**Architecture:** Three phases. A) Derive the order's payment state from its captures (`RecomputePaymentState`), timestamp-only `MarkPayment*`, recompute on payment events, fix partial capture + refund mirroring. B) Sync `ShipmentState` on shipment creation and order cancellation. C) Return consumed stock through the reservation service (multi-location).

**Tech Stack:** .NET 10 (warnings-as-errors), EF Core InMemory (tests), xunit v3 (MTP runner) + FluentAssertions, Moq.

## Global Constraints

- `TreatWarningsAsErrors=true` — any C# warning fails the build; test code must not trigger nullable-reference warnings.
- Test runner (backend): `dotnet test --filter` does NOT work (xunit v3 MTP rejects it — "Zero tests ran", exit 5). Run a single class via the built binary's `-class` flag.
- Single assembly `Module`; tests set `ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly]`.
- Result objects, not exceptions (factories return `Result<T>`, handlers return `Result`).
- Cross-module `using` and direct service calls permitted (AGENTS.md rule 2); prefer `ISender` where it already exists.
- Reservations are linked to the order via `CartToken == orderId.ToString()` (the consume path queries by `CartToken`, not `OrderId`).
- Stock movement factory: `StockMovementMethod.Create(stockItemId, quantity, previousCountOnHand, originatorType, originatorId, reason)`. Domain increment: `StockItem.Restock(int)`.

---

## File Structure

- Modify: `Ordering/Domain/Orders/Order.Method.Computation.cs` (+ `RecomputePaymentState`)
- Modify: `Ordering/Domain/Orders/Order.Method.Timestamps.cs` (timestamp-only `MarkPayment*`)
- Modify: `Ordering/Features/Storefront/RecordOrderPaymentState/RecordOrderPaymentState.cs`
- Modify: `Ordering/Features/Storefront/CompleteCheckoutForPayment/CompleteCheckoutForPayment.cs`
- Create: `Ordering/Features/Storefront/RecomputeOrderPaymentState/` (command + handler)
- Modify: `Billing/Features/Admin/Payments/Capture/CapturePayment.cs`
- Modify: `Billing/Features/Admin/Payments/Refund/RefundPayment.cs`
- Modify: `Billing/Backgrounds/ProcessStripeWebhookEventJob.cs` (charge.refunded recompute)
- Modify: `Shipping/Features/Shared/Commands/CreateShipment.cs`
- Modify: `Inventory/Domain/StockReservations/StockReservation.Method.cs` (+ `Return()`)
- Modify: `Inventory/Services/StockReservations/StockReservation.Service.Interface.cs` + `.Implementation.cs` (+ `ReturnConsumedForOrderAsync`)
- Modify: `Ordering/Features/Storefront/Orders/Cancel/CancelOrder.cs`
- Modify: `Ordering/Features/Admin/Orders/Cancel/CancelOrderAdmin.cs`
- Tests under `service/Api/tests/Module.UnitTests/` (new + updated).

---

### Task 1: Derived payment state (Ordering domain)

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Computation.cs`
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Timestamps.cs`
- Create: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/OrderPaymentStateTests.cs`

**Interfaces:**
- Consumes: `PaymentCapture.CapturedAmount`/`RefundedAmount` (Billing); `Order.PaymentCaptures` nav.
- Produces: `Order.RecomputePaymentState()` (idempotent, derived state); timestamp-only `MarkPaymentCompleted`/`MarkPaymentFailed`.

- [ ] **Step 1: Write the failing test**

Create `OrderPaymentStateTests.cs`:

```csharp
using Module.Billing.Domain.PaymentCaptures;
using Module.Ordering.Domain.Orders;

namespace Module.UnitTests.Ordering.Domain.Orders;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "OrderPaymentState")]
public class OrderPaymentStateTests
{
    private static Order NewOrder(decimal total = 100m)
    {
        var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        order.ItemTotal = total;
        order.Total = total;
        return order;
    }

    private static PaymentCapture Capture(decimal amount, decimal refunded = 0m)
    {
        var capture = PaymentCaptureMethod.Create(amount, Guid.NewGuid(), Guid.NewGuid()).Value;
        capture.State = PaymentRecordState.Completed;
        capture.CapturedAmount = amount;
        capture.RefundedAmount = refunded;
        return capture;
    }

    [Fact(DisplayName = "RecomputePaymentState: fully paid order yields Paid")]
    public void RecomputePaymentState_FullyPaid_YieldsPaid()
    {
        var order = NewOrder(100m);
        order.PaymentCaptures.Add(Capture(100m));

        order.RecomputePaymentState();

        order.PaymentTotal.Should().Be(100m);
        order.OutstandingBalance.Should().Be(0m);
        order.PaymentState.Should().Be(OrderPaymentState.Paid);
    }

    [Fact(DisplayName = "RecomputePaymentState: underpaid order yields BalanceDue")]
    public void RecomputePaymentState_Underpaid_YieldsBalanceDue()
    {
        var order = NewOrder(100m);
        order.PaymentCaptures.Add(Capture(40m));

        order.RecomputePaymentState();

        order.PaymentTotal.Should().Be(40m);
        order.OutstandingBalance.Should().Be(60m);
        order.PaymentState.Should().Be(OrderPaymentState.BalanceDue);
    }

    [Fact(DisplayName = "RecomputePaymentState: refunded amount reduces PaymentTotal")]
    public void RecomputePaymentState_Refunded_ReducesPaymentTotal()
    {
        var order = NewOrder(100m);
        order.PaymentCaptures.Add(Capture(100m, refunded: 20m));

        order.RecomputePaymentState();

        order.PaymentTotal.Should().Be(80m);
        order.OutstandingBalance.Should().Be(20m);
        order.PaymentState.Should().Be(OrderPaymentState.BalanceDue);
    }

    [Fact(DisplayName = "MarkPaymentCompleted: stamps timestamp but no longer sets PaymentState")]
    public void MarkPaymentCompleted_StampsTimestampOnly()
    {
        var order = NewOrder(100m);
        order.PaymentState = OrderPaymentState.Checkout;
        var at = DateTimeOffset.UtcNow;

        order.MarkPaymentCompleted(at);

        order.PaymentCompletedAtUtc.Should().Be(at);
        order.PaymentState.Should().Be(OrderPaymentState.Checkout);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo`
Expected: FAIL — `CS1061 'Order' does not contain a definition for 'RecomputePaymentState'` (and the `MarkPaymentCompleted_StampsTimestampOnly` assertion fails on `PaymentState == Checkout` because current code sets `Completed`).

- [ ] **Step 3: Implement**

In `Order.Method.Computation.cs`, add after `UpdatePaymentState`:

```csharp
    // Compute: PaymentTotal = net captured amount across all captures; OutstandingBalance = Total - PaymentTotal;
    //           then derive PaymentState (Paid / BalanceDue / CreditOwed / Void). Idempotent.
    public static Result RecomputePaymentState(this Order order)
    {
        order.PaymentTotal = order.PaymentCaptures.Sum(p => p.CapturedAmount)
                           - order.PaymentCaptures.Sum(p => p.RefundedAmount);
        order.OutstandingBalance = order.Total - order.PaymentTotal;
        order.UpdatePaymentState();
        return Result.Ok();
    }
```

In `Order.Method.Timestamps.cs`, remove the two `PaymentState` assignments:
- In `MarkPaymentCompleted`, delete the line `order.PaymentState = OrderPaymentState.Completed;`
- In `MarkPaymentFailed`, delete the line `order.PaymentState = OrderPaymentState.Failed;`

- [ ] **Step 4: Run test to verify it passes**

Run:
```bash
dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Ordering.Domain.Orders.OrderPaymentStateTests"
```
Expected: PASS (`Failed: 0`). Note: other tests that assert `MarkPaymentCompleted`/`MarkPaymentFailed` set `PaymentState` may now fail — run the full Ordering domain tests and update any assertion that expected the old `Completed`/`Failed` assignment (e.g. `RecordOrderPaymentStateTests`). This is expected and fixed in later tasks.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Computation.cs \
        service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Timestamps.cs \
        service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/OrderPaymentStateTests.cs
git commit -m "feat(ordering): derive payment state from captures"
```

---

### Task 2: Recompute on payment events (RecordOrderPaymentState + CompleteCheckoutForPayment)

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/RecordOrderPaymentState/RecordOrderPaymentState.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/CompleteCheckoutForPayment/CompleteCheckoutForPayment.cs`
- Modify (tests): `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/RecordOrderPaymentState/RecordOrderPaymentStateTests.cs` (create if absent) and `.../CompleteCheckoutForPayment/CompleteCheckoutForPaymentTests.cs`

**Interfaces:**
- Consumes: `Order.RecomputePaymentState()` (Task 1).
- Produces: handlers now include `PaymentCaptures` and recompute on Completed/Failed.

- [ ] **Step 1: Write/adjust the failing test**

In `RecordOrderPaymentStateTests` (or create it), assert that dispatching `RecordOrderPaymentStateCommand { PaymentState = Completed }` on a seeded order with a Completed capture yields `PaymentState == Paid` and `PaymentTotal == amount`. If the file already exists, add this test; otherwise create it following the `CompleteCheckoutForPaymentTests` InMemory pattern (seed Order + PaymentCapture, dispatch command via handler).

- [ ] **Step 2: Run to confirm red**

Run the test class via `-class`. Expected: FAIL (PaymentState not Paid; PaymentTotal still 0).

- [ ] **Step 3: Implement**

In `RecordOrderPaymentState.cs`, change the order load to include captures and recompute:

```csharp
        var order = await dbContext.Set<Order>()
            .Include(o => o.PaymentCaptures)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);
        if (order is null)
            return OrderResult.Errors.NotFound(command.OrderId);

        var result = command.PaymentState switch
        {
            PaymentTimelineState.Completed => order.MarkPaymentCompleted(command.AtUtc),
            PaymentTimelineState.Failed => order.MarkPaymentFailed(command.AtUtc),
            PaymentTimelineState.Processing => order.MarkPaymentProcessing(command.AtUtc),
            _ => Result.Ok()
        };
        if (result.IsFailure)
            return result.Errors;

        if (command.PaymentState is PaymentTimelineState.Completed or PaymentTimelineState.Failed)
            order.RecomputePaymentState();

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok();
```

Add `using Microsoft.EntityFrameworkCore;` if not present.

In `CompleteCheckoutForPayment.cs`, change the cart query (line 17-20) to include captures:

```csharp
        var cart = await dbContext.Set<Order>()
            .Include(x => x.LineItems)
            .Include(x => x.PaymentCaptures)
            .Where(x => x.Id == command.CartId && x.Status == OrderStatus.Draft)
            .FirstOrDefaultAsync(cancellationToken);
```

and after `cart.MarkPaymentCompleted(...)` (line 41), add:

```csharp
        cart.RecomputePaymentState();
```

- [ ] **Step 4: Run tests to verify they pass**

Run both test classes via `-class`. Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/RecordOrderPaymentState/RecordOrderPaymentState.cs \
        service/Api/src/Module/Ordering/Features/Storefront/CompleteCheckoutForPayment/CompleteCheckoutForPayment.cs \
        service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/RecordOrderPaymentState/ \
        service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/CompleteCheckoutForPayment/
git commit -m "feat(ordering): recompute payment state on payment events"
```

---

### Task 3: Fix partial-capture mirror (Billing)

**Files:**
- Modify: `service/Api/src/Module/Billing/Features/Admin/Payments/Capture/CapturePayment.cs`
- Modify (test): `service/Api/tests/Module.UnitTests/Billing/Features/Admin/Payments/Capture/CapturePaymentTests.cs` (create if absent)

**Interfaces:**
- Consumes: `PaymentRecordState.Completed`; `PaymentTimelineState`.
- Produces: capture dispatches `Completed` only when fully completed, else `Processing`.

- [ ] **Step 1: Write the failing test**

In `CapturePaymentTests` (or create), mock `IPaymentProcessingService.CaptureAsync` to leave the capture `Processing` (partial), and verify the handler dispatches `RecordOrderPaymentStateCommand` with `PaymentState == Processing` (not `Completed`). Use Moq on `ISender` to capture the command.

- [ ] **Step 2: Run to confirm red**

Expected: FAIL (handler always sends `Completed`).

- [ ] **Step 3: Implement**

In `CapturePayment.cs`, replace the `RecordOrderPaymentStateCommand` dispatch block (lines 71-76) with:

```csharp
            var notifyResult = await sender.Send(new RecordOrderPaymentStateCommand
            {
                OrderId = payment.OrderId,
                PaymentState = payment.State == PaymentRecordState.Completed
                    ? PaymentTimelineState.Completed
                    : PaymentTimelineState.Processing,
                AtUtc = payment.CompletedAtUtc ?? DateTimeOffset.UtcNow
            }, cancellationToken);
```

- [ ] **Step 4: Run test to verify pass**

Run the class via `-class`. Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Billing/Features/Admin/Payments/Capture/CapturePayment.cs \
        service/Api/tests/Module.UnitTests/Billing/Features/Admin/Payments/Capture/
git commit -m "fix(billing): mirror processing state on partial capture"
```

---

### Task 4: Refund mirror (RecomputeOrderPaymentStateCommand)

**Files:**
- Create: `service/Api/src/Module/Ordering/Features/Storefront/RecomputeOrderPaymentState/RecomputeOrderPaymentState.cs` (command + handler in one file, matching the RecordOrderPaymentState single-file style)
- Modify: `service/Api/src/Module/Billing/Features/Admin/Payments/Refund/RefundPayment.cs`
- Modify: `service/Api/src/Module/Billing/Backgrounds/ProcessStripeWebhookEventJob.cs`
- Create/modify tests for the new handler.

**Interfaces:**
- Consumes: `Order.RecomputePaymentState()` (Task 1).
- Produces: `RecomputeOrderPaymentStateCommand { Guid OrderId }` dispatched from refund paths.

- [ ] **Step 1: Create the command + handler**

Create `RecomputeOrderPaymentState.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.RecomputeOrderPaymentState;

/// <summary>Recomputes the order's derived payment state from its captures.</summary>
public sealed record RecomputeOrderPaymentStateCommand : ICommand
{
    public Guid OrderId { get; init; }
}

public sealed class RecomputeOrderPaymentStateCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<RecomputeOrderPaymentStateCommand>
{
    public async Task<Result> Handle(
        RecomputeOrderPaymentStateCommand command, CancellationToken cancellationToken)
    {
        var order = await dbContext.Set<Order>()
            .Include(o => o.PaymentCaptures)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);
        if (order is null)
            return OrderResult.Errors.NotFound(command.OrderId);

        order.RecomputePaymentState();
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
```

- [ ] **Step 2: Dispatch from RefundPayment**

In `RefundPayment.cs`, add `ISender sender` to the handler constructor and, after the `SaveChangesAsync` retry loop (before the map block at line 81), add:

```csharp
            await sender.Send(new Module.Ordering.Features.Storefront.RecomputeOrderPaymentState.RecomputeOrderPaymentStateCommand
            {
                OrderId = payment.OrderId
            }, cancellationToken);
```

Add `using Module.Ordering.Features.Storefront.RecomputeOrderPaymentState;` (or use the fully-qualified name as above).

- [ ] **Step 3: Dispatch from the refund webhook**

In `ProcessStripeWebhookEventJob.cs`, in `HandleChargeRefunded` after `await RecordStripeEventAsync(payment, stripeEvent, ct);` (line 263), add:

```csharp
        await _sender.Send(new RecomputeOrderPaymentStateCommand { OrderId = payment.OrderId }, ct);
```

Add `using Module.Ordering.Features.Storefront.RecomputeOrderPaymentState;` to the file.

- [ ] **Step 4: Test**

Create `RecomputeOrderPaymentStateTests.cs` (InMemory): seed Order + Completed capture with `RefundedAmount`, dispatch the command, assert `PaymentTotal`/`OutstandingBalance`/`PaymentState` reflect the refund. Verify green.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/RecomputeOrderPaymentState/ \
        service/Api/src/Module/Billing/Features/Admin/Payments/Refund/RefundPayment.cs \
        service/Api/src/Module/Billing/Backgrounds/ProcessStripeWebhookEventJob.cs \
        service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/RecomputeOrderPaymentState/
git commit -m "feat(billing): recompute order payment state on refund"
```

---

### Task 5: Sync fulfillment on shipment creation (Shipping)

**Files:**
- Modify: `service/Api/src/Module/Shipping/Features/Shared/Commands/CreateShipment.cs`
- Modify (test): `service/Api/tests/Module.UnitTests/Shipping/Features/Shared/Commands/CreateShipmentTests.cs`

**Interfaces:**
- Consumes: `ShipmentFulfillmentSyncService.SyncOrderFulfillmentAsync(Guid, CancellationToken)`.
- Produces: placed orders get `ShipmentState = Pending`.

- [ ] **Step 1: Write the failing test**

In `CreateShipmentTests`, add a test asserting that after creating a shipment, `ShipmentFulfillmentSyncService` is invoked (mock the sync service or assert the resulting order state). Since `CreateShipmentCommandHandler` currently takes only `IApplicationDbContext`, the test must construct it with a sync-service mock.

- [ ] **Step 2: Implement**

In `CreateShipment.cs`, inject the sync service:

```csharp
public sealed class CreateShipmentCommandHandler(
    IApplicationDbContext dbContext,
    ShipmentFulfillmentSyncService syncService)
    : ICommandHandler<CreateShipmentCommand>
```

and after `await dbContext.SaveChangesAsync(cancellationToken);` (line 30), before `return Result.Ok();` add:

```csharp
        await syncService.SyncOrderFulfillmentAsync(command.OrderId, cancellationToken);
```

Add `using Module.Shipping.Services;` if not present. `ShipmentFulfillmentSyncService` is registered in DI (used by `UpdateShipmentStatus`).

- [ ] **Step 3: Run tests**

Build + run `CreateShipmentTests` and `ShipmentFulfillmentSyncServiceTests` via `-class`. Expected: PASS (update any constructor call in existing `CreateShipmentTests` to pass the sync service mock).

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Shipping/Features/Shared/Commands/CreateShipment.cs \
        service/Api/tests/Module.UnitTests/Shipping/Features/Shared/Commands/CreateShipmentTests.cs
git commit -m "feat(shipping): sync order fulfillment state on shipment creation"
```

---

### Task 6: Return consumed stock (Inventory)

**Files:**
- Modify: `service/Api/src/Module/Inventory/Domain/StockReservations/StockReservation.Method.cs`
- Modify: `service/Api/src/Module/Inventory/Services/StockReservations/StockReservation.Service.Interface.cs`
- Modify: `service/Api/src/Module/Inventory/Services/StockReservations/StockReservation.Service.Implementation.cs`
- Create: `service/Api/tests/Module.UnitTests/Inventory/Services/StockReservationServiceTests.cs` (extend existing) — add return tests.

**Interfaces:**
- Consumes: `StockItem.Restock(int)`; `StockMovementMethod.Create(...)`; `ReservationState.Fulfilled`.
- Produces: `IStockReservationService.ReturnConsumedForOrderAsync(Guid orderId, CancellationToken ct)` (multi-location return); `StockReservation.Return()`.

- [ ] **Step 1: Add the `Return()` transition**

In `StockReservation.Method.cs`, add after `Release`:

```csharp
    // Update: Return a consumed (Fulfilled) reservation back to Released — used on order cancellation.
    public static Result Return(this StockReservation reservation)
    {
        if (reservation.State != ReservationState.Fulfilled)
            return StockReservationResult.Errors.InvalidStateTransition;
        reservation.State = ReservationState.Released;
        reservation.ExpiresAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }
```

- [ ] **Step 2: Add the interface method**

In `StockReservation.Service.Interface.cs`, add:

```csharp
    Task<Result> ReturnConsumedForOrderAsync(Guid orderId, CancellationToken ct = default);
```

- [ ] **Step 3: Implement the service method**

In `StockReservation.Service.Implementation.cs`, add (mirrors `ConsumeForOrderAsync`'s `CartToken == orderId.ToString()` convention):

```csharp
    public async Task<Result> ReturnConsumedForOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        var reservations = await dbContext.Set<StockReservation>()
            .Where(r => r.CartToken == orderId.ToString()
                        && r.State == ReservationState.Fulfilled)
            .ToListAsync(ct);

        foreach (var reservation in reservations)
        {
            var stockItem = await dbContext.Set<StockItem>()
                .FirstOrDefaultAsync(
                    si => si.VariantId == reservation.VariantId
                          && si.StockLocationId == reservation.StockLocationId,
                    ct);
            if (stockItem is null)
                return StockReservationResult.Errors.StockItemNotFound(reservation.VariantId);

            var previous = stockItem.CountOnHand;
            var restockResult = stockItem.Restock(reservation.Quantity);
            if (restockResult.IsFailure)
                return restockResult.Errors;

            var movement = StockMovementMethod.Create(
                stockItemId: stockItem.Id,
                quantity: reservation.Quantity,
                previousCountOnHand: previous,
                originatorType: "Order",
                originatorId: orderId,
                reason: "canceled");
            if (movement.IsSuccess)
                dbContext.Set<StockMovement>().Add(movement.Value);

            reservation.Return();
            reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(ct);
        return Result.Ok();
    }
```

Note: add `using Module.Inventory.Domain.StockItems;` and `using Module.Inventory.Domain.StockMovements;` if not already present.

- [ ] **Step 4: Write failing test first (before Step 3)**

In the existing `StockReservationServiceTests`, add a test: seed a `Fulfilled` reservation (via `SeedForTest(..., ReservationState.Fulfilled, ...)`) + a StockItem, call `ReturnConsumedForOrderAsync(orderId)`, assert `CountOnHand` incremented, a `canceled` StockMovement created, and the reservation state `Released`. Confirm it fails before the method exists, then passes after.

- [ ] **Step 5: Run tests + commit**

```bash
dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Inventory.Services.StockReservationServiceTests"
git add service/Api/src/Module/Inventory/Domain/StockReservations/StockReservation.Method.cs \
        service/Api/src/Module/Inventory/Services/StockReservations/ \
        service/Api/tests/Module.UnitTests/Inventory/Services/StockReservationServiceTests.cs
git commit -m "feat(inventory): return consumed stock per location on order cancel"
```

---

### Task 7: Full cancel cascade (Ordering)

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Orders/Cancel/CancelOrder.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/Cancel/CancelOrderAdmin.cs`
- Modify (tests): `CancelOrder`/`CancelOrderAdmin` tests (create/update as needed).

**Interfaces:**
- Consumes: `Shipment.Cancel()`; `IStockReservationService.ReturnConsumedForOrderAsync` (Task 6); `Order.ShipmentState`.
- Produces: canceling an order cancels its shipments, sets `ShipmentState = Canceled`, and returns stock via the reservation service.

- [ ] **Step 1: Write the failing tests**

Add assertions to the cancel-handler tests: after a successful cancel of a Placed order, the order's shipments are `Canceled`, `ShipmentState == Canceled`, and stock is returned (via a mocked `IStockReservationService.ReturnConsumedForOrderAsync` verify). Use Moq for the reservation service; replace `IStockItemService` with `IStockReservationService` in the handler constructor.

- [ ] **Step 2: Implement (Storefront CancelOrder)**

In `CancelOrder.cs`:
1. Replace `IStockItemService stockItem` with `IStockReservationService stockReservation` in the constructor.
2. Load shipments: change the order query to `.Include(x => x.LineItems).Include(x => x.Shipments)`.
3. After `entity.Cancel(userId)` (and after the void-payments call), add shipment cancellation + state:

```csharp
            foreach (var shipment in entity.Shipments)
                shipment.Cancel();

            entity.ShipmentState = ShipmentState.Canceled;
```

4. Replace the stock-return loop (lines 66-81) with:

```csharp
            if (wasPlaced)
            {
                var returnResult = await stockReservation.ReturnConsumedForOrderAsync(entity.Id, cancellationToken);
                if (returnResult.IsFailure)
                    return returnResult.Errors;
            }
```

Add `using Module.Shipping.Domain.Shipments;` and `using Module.Inventory.Services.StockReservations;` (for `IStockReservationService`) as needed. Remove the now-unused `IStockItemService`/`Module.Inventory.Services` usings.

- [ ] **Step 3: Implement (Admin CancelOrderAdmin)**

Apply the identical changes to `CancelOrderAdmin.cs` (replace `IStockItemService` with `IStockReservationService`, `.Include(Shipments)`, cancel shipments + `ShipmentState = Canceled`, `ReturnConsumedForOrderAsync`).

- [ ] **Step 4: Run tests**

Build + run `CancelOrder`/`CancelOrderAdmin` tests via `-class` (update mocks/assertions). Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Orders/Cancel/CancelOrder.cs \
        service/Api/src/Module/Ordering/Features/Admin/Orders/Cancel/CancelOrderAdmin.cs \
        service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Orders/Cancel/ \
        service/Api/tests/Module.UnitTests/Ordering/Features/Admin/Orders/Cancel/
git commit -m "feat(ordering): cancel shipments and return stock on order cancellation"
```

---

### Verification (after all tasks)

```bash
dotnet build service/Api/src/Api/Api.csproj -v q --nologo
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests
```

Expected: build 0 warnings/0 errors; only the 3 pre-existing `OrderStatusValueConverterTests` failures remain (unrelated WIP). All new tests pass.

## Self-Review

- **Spec coverage:** Phase A → Tasks 1-4; Phase B → Task 5 (+ Task 7 shipment part); Phase C → Task 6 (+ Task 7 stock part). All spec bullets covered.
- **Placeholder scan:** no TBD/TODO; concrete code in every step.
- **Type consistency:** `RecomputePaymentState`/`ReturnConsumedForOrderAsync`/`RecomputeOrderPaymentStateCommand` names match across tasks; `StockMovementMethod.Create` param order matches `ConsumeForOrderAsync` usage; `StockItem.Restock` exists (`StockItem.Method.Adjustment.cs:46`); `Shipment.Cancel()` exists (`Shipment.Method.State.cs:51`); `PaymentRecordState.Completed` for the partial-capture check.
- **Reservation convention:** return queries `CartToken == orderId.ToString()` (matches consume). Confirm in Task 6.
- **Regression safety:** existing tests asserting old `MarkPayment*` → `Completed/Failed` are updated in Tasks 1-2; the plan flags this.

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-08-16-order-cross-module-state-sync.md`. Two execution options:

1. **Subagent-Driven (recommended)** — dispatch a fresh subagent per task, review between tasks.
2. **Inline Execution** — execute tasks in this session with checkpoints.

Which approach?
