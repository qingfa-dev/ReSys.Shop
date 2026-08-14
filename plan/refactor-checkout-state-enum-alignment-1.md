---
goal: Rename CheckoutState to pick-method semantics and replace every status string with a typed enum across backend, DTOs, commands, and both SPAs, consolidated into a single backfill migration.
version: 1.0
date_created: 2026-08-14
last_updated: 2026-08-14
owner: Ordering / Billing / Store & Admin SPAs
status: 'Planned'
tags: [refactor, ordering, billing, checkout, payment, shipment, enum, migration]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

`CheckoutState.Payment` is mislabeled: that step means "pick a payment method"
(Credit Card vs Cash on Delivery), not "process the payment". The mislabel has
already caused a bug (`AdvanceCheckoutState` stamps `PaymentProcessingAt` when
the customer merely selects a method). Separately, checkout/payment/shipment
statuses cross module and API boundaries as raw strings. This plan renames the
mislabeled state, removes the wrong timestamp, converts every status string to a
typed enum end-to-end, and collapses the in-progress migrations into one clean
backfill migration.

**Spec:** `spec/spec-checkout-state-enum-alignment.md`

**Resolved open questions** (per recommendation): keep all 9 `OrderPaymentState`
values; stamp `PaymentProcessingAt` when the payment actually enters `Processing`
(gateway path) via `RecordOrderPaymentStateCommand{Processing}`; delete the dead
`CheckoutStep` helpers; replace `PaymentForCheckoutResponse.State` string with a
typed `bool IsPending`.

> **Commit note:** renaming an enum is cross-cutting — the C# backend does not
> compile green until Tasks 1–13 are all complete. Commit at the end of each
> Phase where the tree is green; do not commit a red tree. The SPA phases (14–16)
> and the migration phase (17) are independently green.

## Global Constraints

- **CON-GC1**: `TreatWarningsAsErrors=true` — any C# warning fails `dotnet build`.
- **CON-GC2**: Domain operations return `Result`/`Result<T>`; exceptions only for unrecoverable infra failures.
- **CON-GC3**: Modules communicate only via MediatR `ISender`; no new cross-module assembly references.
- **CON-GC4**: Vertical-slice feature files — `static partial class` split across `Features/{Admin|Storefront}/{Feature}/{Action}/`; subdirectory is `Storefront`.
- **CON-GC5**: SPA comments follow `app/Store/AGENTS.md` / `app/Admin/AGENTS.md` (`// Label: Sentence.`; `<!-- Section: Title — purpose -->`). No em dashes in comments.
- **CON-GC6**: No destructive git (`stash`/`restore`/`revert`/`checkout --`/`reset --hard`) without explicit human "yes" (AGENTS.md rule 6). The snapshot reset in Task 17 requires that explicit approval.
- **CON-GC7**: Do not introduce new `PaymentRecordState` values or new endpoints.
- **PAT-GC1**: Enum→string persistence uses EF Core `.HasConversion<string>()` (stores member name), matching `OrderStatus`/`CheckoutState`/`PaymentRecordState`.

---

## 1. Requirements & Constraints

### Requirements

- **REQ-001**: `CheckoutState` contains exactly `Address, PickDeliveryMethod, PickPaymentMethod, Confirm, Complete`; the spurious 6th `Payment` member is removed.
- **REQ-002**: Every C# reference uses `PickDeliveryMethod`/`PickPaymentMethod`; `PaymentSelected` and stale `Payment` are gone everywhere.
- **REQ-003**: `AdvanceCheckoutState` must NOT call `MarkPaymentProcessing`; selection is not processing.
- **REQ-004**: `AdvanceCheckoutStateCommand.TargetState`, `RegressCheckoutStateCommand.TargetState`, `CartResponseBase.CheckoutState`, `GetCartForCheckoutResponse.State` are typed `CheckoutState` (no `Enum.TryParse`, no `.ToString()`).
- **REQ-005**: `Order.PaymentState`/`Order.ShipmentState` become `OrderPaymentState?`/`OrderShipmentState?`; `OrderConstant.PaymentState`/`ShipmentState` string classes are removed.
- **REQ-006**: `UpdateOrderShipmentState.Request.ShipmentState` becomes `OrderShipmentState`.
- **REQ-007**: `RecordOrderPaymentStateCommand.PaymentState` becomes `PaymentTimelineState`; the `OrderPaymentState` static string class is removed.
- **REQ-008**: Billing sends enums: `ProcessStripeWebhookEventJob` (`TargetState = CheckoutState.PickDeliveryMethod`, `PaymentTimelineState.Completed/Failed`), `CapturePayment` (`PaymentTimelineState.Completed`), `CreatePaymentIntent` (`TargetState = CheckoutState.PickPaymentMethod`).
- **REQ-009**: `PaymentForCheckoutResponse.State` string is replaced with `bool IsPending`; `CreateOrderFromCart` uses `p.IsPending` (no `p.State == "Pending"`).
- **REQ-010**: `CreatePaymentIntent` notifies Ordering of `PaymentTimelineState.Processing` when a gateway payment enters `Processing`.
- **REQ-011**: Store & Admin SPAs define `CheckoutState`/`OrderPaymentState`/`OrderShipmentState` unions and type all relevant fields; zod uses `z.enum`.
- **REQ-012**: Drop in-progress migrations and create a single consolidated backfill migration (§1 of Testing).

### Constraints

- **CON-001**: Zero-warning `dotnet build`.
- **CON-002**: No new cross-module assembly references.
- **CON-003**: No destructive git without explicit approval.

### Guidelines

- **GUD-001**: Keep state-machine logic in `Order.Method.Checkout.cs` partial class; no new services.
- **GUD-002**: SPA uses a single shared string-literal union + `z.enum`, not a runtime TS `enum`.

---

## 2. Implementation Steps

### Phase 1 — Domain enums, state machine, timestamps

- GOAL-001: Rename `CheckoutState`, add `OrderPaymentState`/`OrderShipmentState`, delete dead helpers, fix the processing timestamp, and update domain tests.

#### TASK-001: Rename `CheckoutState` and add payment/shipment enums

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Enumerate.cs`
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Constant.cs`

**Interfaces:**
- Produces: `enum CheckoutState { Address, PickDeliveryMethod, PickPaymentMethod, Confirm, Complete }`, `enum OrderPaymentState { Completed, Failed, Void, BalanceDue, CreditOwed, Paid, Pending, Checkout, Invalid }`, `enum OrderShipmentState { Pending, Delivered, Partial, Ready, Backorder, Canceled }`. Removes `OrderConstant.PaymentState`, `OrderConstant.ShipmentState`, `OrderConstant.CheckoutStep`, `OrderConstant.Defaults.PaymentState`, `OrderConstant.Defaults.ShipmentState`.

- [ ] **Step 1: Rewrite the enums**

Replace the entire `CheckoutState` enum in `Order.Enumerate.cs` with:

```csharp
// Enumerate: Checkout state machine progression — Address → PickDeliveryMethod → PickPaymentMethod → Confirm → Complete
public enum CheckoutState
{
    Address,
    PickDeliveryMethod,
    PickPaymentMethod,
    Confirm,
    Complete
}

// Enumerate: Derived aggregate payment status — set by UpdatePaymentState / MarkPaymentAsPaid
public enum OrderPaymentState
{
    Completed,
    Failed,
    Void,
    BalanceDue,
    CreditOwed,
    Paid,
    Pending,
    Checkout,
    Invalid
}

// Enumerate: Fulfillment status — set by UpdateOrderShipmentState
public enum OrderShipmentState
{
    Pending,
    Delivered,
    Partial,
    Ready,
    Backorder,
    Canceled
}
```

- [ ] **Step 2: Remove the string-constant classes**

In `Order.Constant.cs`, delete these four members entirely (they are replaced by the enums):

- `OrderConstant.Defaults.PaymentState` (`public const string PaymentState = "pending";`)
- `OrderConstant.Defaults.ShipmentState` (`public const string ShipmentState = "pending";`)
- `OrderConstant.PaymentState` (the whole `public static class PaymentState { ... }` block, lines 29–40)
- `OrderConstant.ShipmentState` (the whole block, lines 42–50)
- `OrderConstant.CheckoutStep` (the whole block, lines 52–59)

- [ ] **Step 3: Verify the failures are expected**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: FAIL — compile errors in files still referencing `OrderConstant.PaymentState`, `OrderConstant.ShipmentState`, `OrderConstant.CheckoutStep`, `CheckoutState.Delivery`, `CheckoutState.Payment`, `PaymentSelected`. This is expected; do not commit yet.

#### TASK-002: Update the domain state machine and delete dead step helpers

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Checkout.cs`
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Validation.cs`
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.cs`
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Computation.cs`
- Modify: `service/Api/src/Module/Ordering/Persistence/Configurations/OrderConfiguration.cs`

**Interfaces:**
- Produces: `Order.PaymentState` typed `OrderPaymentState?`, `Order.ShipmentState` typed `OrderShipmentState?`; `OrderMethod.UpdatePaymentState` sets enum members; `OrderMethod.MarkPaymentAsPaid` sets `OrderPaymentState.Paid`; `AllowCancel` compares `OrderShipmentState` members.

- [ ] **Step 1: Delete the dead `CheckoutStep` block**

In `Order.Method.Checkout.cs`, delete these members (they have no consumers outside this file): `DefaultCheckoutSteps` (line 9), `ResolvedCheckoutSteps` (11–22), `CurrentCheckoutStep` (29–30), `CompletedCheckoutSteps` (33–41), `HasCheckoutStep` (44–45), `PassedCheckoutStep` (48–49), `CheckoutStepIndex` (52–53), `CanGoToState` (56–59), and the now-unused `#region Checkout Steps` (6–24) and `#region Checkout Queries` (26–61) markers.

- [ ] **Step 2: Fix the transitions**

In `AdvanceCheckoutState` (the `validTransition` switch), replace with:

```csharp
var validTransition = (CheckoutState, target) switch
{
    (CheckoutState.Address, CheckoutState.PickDeliveryMethod) => true,
    (CheckoutState.PickDeliveryMethod, CheckoutState.PickPaymentMethod) => true,
    (CheckoutState.PickPaymentMethod, CheckoutState.Confirm) => true,
    (CheckoutState.PickPaymentMethod, CheckoutState.Complete) => true,
    (CheckoutState.Confirm, CheckoutState.Complete) => true,
    _ => false
};
```

In `RegressCheckoutIfAmountChanged`, replace the guard with:

```csharp
if (Status == OrderStatus.Draft && CheckoutState >= CheckoutState.PickPaymentMethod && Total != previousTotal)
    CheckoutState = CheckoutState.PickDeliveryMethod;
```

In `RegressCheckoutState` (the `validTransition` switch), replace with:

```csharp
var validTransition = (CheckoutState, target) switch
{
    (CheckoutState.PickPaymentMethod, CheckoutState.PickDeliveryMethod) => true,
    (CheckoutState.PickPaymentMethod, CheckoutState.Address) => true,
    (CheckoutState.PickDeliveryMethod, CheckoutState.Address) => true,
    _ => false
};
```

- [ ] **Step 3: Fix `RequireEmail` and `AllowCancel`**

```csharp
public bool RequireEmail() =>
    Status != OrderStatus.Draft &&
    CheckoutState is CheckoutState.PickPaymentMethod or CheckoutState.Confirm or CheckoutState.Complete;

public bool AllowCancel() =>
    Status == OrderStatus.Placed &&
    (ShipmentState is null || ShipmentState is OrderShipmentState.Ready or OrderShipmentState.Backorder or OrderShipmentState.Pending or OrderShipmentState.Canceled);
```

- [ ] **Step 4: Fix `MarkPaymentAsPaid`** (same file, `OrderMethod` partial)

```csharp
public static Result MarkPaymentAsPaid(this Order order)
{
    order.PaymentState = OrderPaymentState.Paid;
    return Result.Ok(OrderResult.Success.Updated(order.Id));
}
```

- [ ] **Step 5: Type the entity properties** in `Order.cs` (lines 29–30)

```csharp
public OrderPaymentState? PaymentState { get; set; }
public OrderShipmentState? ShipmentState { get; set; }
```

- [ ] **Step 6: Add EF conversions** in `OrderConfiguration.cs` (after `CheckoutState` config, line ~30)

```csharp
builder.Property(x => x.PaymentState).HasConversion<string>();
builder.Property(x => x.ShipmentState).HasConversion<string>();
```

- [ ] **Step 7: Fix `UpdatePaymentState`** in `Order.Method.Computation.cs`

```csharp
public static Result UpdatePaymentState(this Order order)
{
    if (order.Status == OrderStatus.Canceled && order.PaymentTotal == 0m)
        order.PaymentState = OrderPaymentState.Void;
    else if (order.OutstandingBalance > 0m)
        order.PaymentState = OrderPaymentState.BalanceDue;
    else if (order.OutstandingBalance < 0m)
        order.PaymentState = OrderPaymentState.CreditOwed;
    else
        order.PaymentState = OrderPaymentState.Paid;

    return Result.Ok(OrderResult.Success.PaymentStateUpdated(order.Id));
}
```

- [ ] **Step 8: Fix `Order.Validation.cs`**

Replace the `state switch` (lines 17–24):

```csharp
return state switch
{
    CheckoutState.PickDeliveryMethod => o.BillAddressId != null && o.ShipAddressId != null,
    CheckoutState.PickPaymentMethod => o.ShippingMethodId != null,
    CheckoutState.Confirm => true,
    CheckoutState.Complete => true,
    _ => true
};
```

- [ ] **Step 9: Commit**

```bash
git add service/Api/src/Module/Ordering/Domain/Orders/Order.Enumerate.cs \
        service/Api/src/Module/Ordering/Domain/Orders/Order.Constant.cs \
        service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Checkout.cs \
        service/Api/src/Module/Ordering/Domain/Orders/Order.Validation.cs \
        service/Api/src/Module/Ordering/Domain/Orders/Order.cs \
        service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Computation.cs \
        service/Api/src/Module/Ordering/Persistence/Configurations/OrderConfiguration.cs
git commit -m "refactor(ordering): rename CheckoutState to pick-method and add payment/shipment enums"
```

#### TASK-003: Update domain unit tests

**Files:**
- Modify: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Method.Tests.cs`
- Modify: `service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/Order.Validation.Tests.cs`

**Interfaces:**
- Consumes: `CheckoutState.PickDeliveryMethod`/`PickPaymentMethod`, `OrderPaymentState`, `OrderShipmentState` (TASK-001/TASK-002).

- [ ] **Step 1: Update `Order.Method.Tests.cs`**

Apply these replacements across the file (every occurrence):
- `CheckoutState.Delivery` → `CheckoutState.PickDeliveryMethod`
- `CheckoutState.Payment` → `CheckoutState.PickPaymentMethod`

Add these new tests before the closing brace:

```csharp
[Fact(DisplayName = "AdvanceCheckoutState to PickPaymentMethod does not stamp PaymentProcessingAt")]
public void AdvanceCheckoutState_PickPaymentMethod_DoesNotStampProcessing()
{
    var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
    order.AdvanceCheckoutState(CheckoutState.PickDeliveryMethod);
    order.AdvanceCheckoutState(CheckoutState.PickPaymentMethod);

    order.PaymentProcessingAt.Should().BeNull();
}

[Fact(DisplayName = "UpdatePaymentState derives BalanceDue/Paid/Void from balance")]
public void UpdatePaymentState_DerivesFromBalance()
{
    var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
    order.OutstandingBalance = 10m;
    order.UpdatePaymentState();
    order.PaymentState.Should().Be(OrderPaymentState.BalanceDue);

    order.OutstandingBalance = 0m;
    order.UpdatePaymentState();
    order.PaymentState.Should().Be(OrderPaymentState.Paid);
}
```

- [ ] **Step 2: Update `Order.Validation.Tests.cs`**

Replace `CheckoutState.Confirm` in the `ApplyCheckoutStateTransitionRules_WhenValid_ShouldPass` test with `CheckoutState.PickPaymentMethod` (and ensure the model `M` instance provides `ShippingMethodId` if the rule requires it — the existing test passes `State = CheckoutState.Confirm`; change to `PickPaymentMethod`).

- [ ] **Step 3: Run the domain tests**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Ordering.Domain" -v q`
Expected: build succeeds; domain tests pass (feature-level tests still fail to compile — handled next phase).

- [ ] **Step 4: Commit**

```bash
git add service/Api/tests/Module.UnitTests/Ordering/Domain/Orders/
git commit -m "test(ordering): update domain tests for pick-method checkout state"
```

### Phase 2 — Ordering checkout DTOs/commands: string → enum

- GOAL-002: Convert the checkout-state-bearing strings in Ordering commands and response DTOs to `CheckoutState`, and rename remaining references.

#### TASK-004: `AdvanceCheckoutState` — enum + drop the wrong timestamp

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/AdvanceCheckoutState/AdvanceCheckoutState.Command.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/AdvanceCheckoutState/AdvanceCheckoutState.cs`

**Interfaces:**
- Produces: `AdvanceCheckoutStateCommand { Guid CartId; CheckoutState TargetState }`.

- [ ] **Step 1: Type the command**

```csharp
public sealed record AdvanceCheckoutStateCommand : ICommand
{
    public Guid CartId { get; init; }
    public CheckoutState TargetState { get; init; }
}
```

- [ ] **Step 2: Rewrite the handler**

```csharp
public sealed class AdvanceCheckoutStateCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<AdvanceCheckoutStateCommand>
{
    public async Task<Result> Handle(
        AdvanceCheckoutStateCommand command, CancellationToken cancellationToken)
    {
        var cart = await dbContext.Set<Order>()
            .FirstOrDefaultAsync(
                x => x.Id == command.CartId && x.Status == OrderStatus.Draft,
                cancellationToken);

        if (cart is null)
            return OrderResult.Errors.NotFound(command.CartId);

        var result = cart.AdvanceCheckoutState(command.TargetState);
        if (result.IsFailure)
            return result.Errors;

        // Note: entering PickPaymentMethod means "method picked", not "processing" —
        // PaymentProcessingAt is stamped by RecordOrderPaymentState{Processing} instead.

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(OrderResult.Success.CheckoutAdvanced(cart.Id));
    }
}
```

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/AdvanceCheckoutState/
git commit -m "refactor(ordering): type AdvanceCheckoutState TargetState and drop processing stamp"
```

#### TASK-005: `RegressCheckoutState` — enum

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/RegressCheckoutState/RegressCheckoutState.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/RegressCheckoutState/RegressCheckoutStateHandler.cs`

**Interfaces:**
- Produces: `RegressCheckoutStateCommand { Guid CartId; CheckoutState TargetState }`.

- [ ] **Step 1: Type the command**

```csharp
public sealed record RegressCheckoutStateCommand : ICommand
{
    public Guid CartId { get; init; }
    public CheckoutState TargetState { get; init; }
}
```

- [ ] **Step 2: Rewrite the handler** (remove `Enum.TryParse`)

```csharp
public sealed class RegressCheckoutStateCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<RegressCheckoutStateCommand>
{
    public async Task<Result> Handle(
        RegressCheckoutStateCommand command, CancellationToken cancellationToken)
    {
        var cart = await dbContext.Set<Order>()
            .FirstOrDefaultAsync(
                x => x.Id == command.CartId && x.Status == OrderStatus.Draft,
                cancellationToken);

        if (cart is null)
            return OrderResult.Errors.NotFound(command.CartId);

        var result = cart.RegressCheckoutState(command.TargetState);
        if (result.IsFailure)
            return result.Errors;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(OrderResult.Success.CheckoutAdvanced(cart.Id));
    }
}
```

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/RegressCheckoutState/
git commit -m "refactor(ordering): type RegressCheckoutState TargetState"
```

#### TASK-006: Cart + checkout DTOs — enum

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/Models/Cart.Model.Response.Base.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/Mappings/Cart.Mapping.Model.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/GetCartForCheckout/GetCartForCheckout.Response.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/GetCartForCheckout/GetCartForCheckout.cs`

**Interfaces:**
- Produces: `CartResponseBase.CheckoutState` typed `CheckoutState`; `GetCartForCheckoutResponse.State` typed `CheckoutState`.

- [ ] **Step 1: Type `CartResponseBase.CheckoutState`**

```csharp
public CheckoutState CheckoutState { get; init; }
```

- [ ] **Step 2: Fix the mapping** (drop `.ToString()`)

```csharp
CheckoutState = entity.CheckoutState,
```

- [ ] **Step 3: Type `GetCartForCheckoutResponse.State`**

```csharp
public CheckoutState State { get; init; }
```

- [ ] **Step 4: Fix the handler** (drop `.ToString()`)

```csharp
return new GetCartForCheckoutResponse
{
    State = cart.CheckoutState,
    LineItems = cart.LineItems
        .Select(li => new CartLineItem { VariantId = li.VariantId, Quantity = li.Quantity })
        .ToList(),
    Total = cart.Total,
    Email = cart.Email
};
```

- [ ] **Step 5: Update `Cart.Mapping.Tests.cs`**

Modify `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/Shared/Mappings/Cart.Mapping.Tests.cs`:
- `cart.CheckoutState = CheckoutState.Delivery;` → `CheckoutState.PickDeliveryMethod`
- `response.CheckoutState.Should().Be("Delivery");` → `response.CheckoutState.Should().Be(CheckoutState.PickDeliveryMethod);`

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/ \
        service/Api/src/Module/Ordering/Features/Storefront/GetCartForCheckout/ \
        service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/Shared/Mappings/Cart.Mapping.Tests.cs
git commit -m "refactor(ordering): type checkout state in cart and checkout DTOs"
```

#### TASK-007: Rename remaining Ordering handler references

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/CompleteCheckoutForPayment/CompleteCheckoutForPayment.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/SelectShippingRate/SelectShippingRate.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateCheckout/UpdateCheckout.cs`

**Interfaces:**
- Consumes: `CheckoutState.PickPaymentMethod`/`PickDeliveryMethod` (TASK-001).

- [ ] **Step 1: Apply mechanical renames**

In each file, replace every occurrence:
- `CheckoutState.Payment` → `CheckoutState.PickPaymentMethod`
- `CheckoutState.Delivery` → `CheckoutState.PickDeliveryMethod`

Affected spots (from current source): `CompleteCheckoutForPayment.cs:41`; `CreateOrderFromCart.cs:32`; `SelectShippingRate.cs:75,84,86`; `UpdateCheckout.cs:86,94,96`.

- [ ] **Step 2: Update feature-level tests**

In these test files, apply the same two renames and fix string assertions:

- `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/SelectShippingRate/SelectShippingRateRegressionTests.cs` (`CheckoutState.Payment`→`PickPaymentMethod`, `CheckoutState.Delivery`→`PickDeliveryMethod`)
- `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/UpdateCheckout/UpdateCheckout.Tests.cs`
- `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCartTests.cs`
- `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCartStockTests.cs`
- `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCartTransactionTests.cs`
- `service/Api/tests/Api.Tests/Scenarios/Billing/CreateIntent/CreateIntent.IntegrationTests.cs:183` (`CheckoutState.Delivery`→`PickDeliveryMethod`)

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/CompleteCheckoutForPayment/ \
        service/Api/src/Module/Ordering/Features/Storefront/Cart/ \
        service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/ \
        service/Api/tests/Api.Tests/Scenarios/Billing/CreateIntent/
git commit -m "refactor(ordering): rename remaining checkout-state references to pick-method"
```

### Phase 3 — Payment/shipment enum wiring in Ordering

- GOAL-003: Convert shipment-state request/handler/validator and the payment-timeline mirror to enums, and type the admin order responses.

#### TASK-008: `UpdateOrderShipmentState` — enum

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateShipmentState/UpdateOrderShipmentState.Request.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateShipmentState/UpdateOrderShipmentState.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateShipmentState/UpdateOrderShipmentState.Validator.cs`

**Interfaces:**
- Produces: `UpdateOrderShipmentState.Request { OrderShipmentState ShipmentState }`.

- [ ] **Step 1: Type the request**

```csharp
public sealed record Request
{
    public OrderShipmentState ShipmentState { get; init; }
}
```

- [ ] **Step 2: Rewrite the handler** (remove the string `validStates` array and `OrderConstant.ShipmentState.*`)

```csharp
public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
{
    var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);
    if (order is null)
        return OrderResult.Errors.NotFound(command.Id);

    if (!Enum.IsDefined(command.Request.ShipmentState))
        return OrderResult.Errors.InvalidShipmentState;

    order.ShipmentState = command.Request.ShipmentState;
    var now = DateTimeOffset.UtcNow;

    if (command.Request.ShipmentState is OrderShipmentState.Delivered)
        order.MarkDelivered(now);
    if (command.Request.ShipmentState is OrderShipmentState.Ready
        or OrderShipmentState.Partial
        or OrderShipmentState.Delivered)
        order.MarkShipped(now);

    await dbContext.SaveChangesAsync(cancellationToken);

    return Result<Response>.Ok(order.MapToDetail<Response>(), OrderResult.Success.ShipmentStateUpdated(order.Id));
}
```

- [ ] **Step 3: Rewrite the validator** (replace the `OrderConstant.ShipmentState.*` list with `Enum.GetValues<OrderShipmentState>()` membership or `IsInEnum`)

```csharp
RuleFor(x => x.Request.ShipmentState)
    .IsInEnum()
    .WithErrorCode(OrderResult.Errors.InvalidShipmentState.Code)
    .WithMessage(OrderResult.Errors.InvalidShipmentState.Message);
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateShipmentState/
git commit -m "refactor(ordering): type shipment state request as OrderShipmentState"
```

#### TASK-009: `RecordOrderPaymentState` — rename static class to `PaymentTimelineState` enum

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/RecordOrderPaymentState/RecordOrderPaymentState.Command.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/RecordOrderPaymentState/RecordOrderPaymentState.cs`

**Interfaces:**
- Produces: `enum PaymentTimelineState { Completed, Failed, Processing }`; `RecordOrderPaymentStateCommand { Guid OrderId; PaymentTimelineState PaymentState; DateTimeOffset AtUtc }`.

- [ ] **Step 1: Replace the static class with an enum**

In `RecordOrderPaymentState.Command.cs`, replace the `public static class OrderPaymentState { ... }` block with:

```csharp
public enum PaymentTimelineState
{
    Completed,
    Failed,
    Processing
}
```

and change the command property:

```csharp
public sealed record RecordOrderPaymentStateCommand : ICommand
{
    public Guid OrderId { get; init; }
    public PaymentTimelineState PaymentState { get; init; }
    public DateTimeOffset AtUtc { get; init; }
}
```

- [ ] **Step 2: Update the handler switch** (`RecordOrderPaymentState.cs`)

```csharp
var result = command.PaymentState switch
{
    PaymentTimelineState.Completed => order.MarkPaymentCompleted(command.AtUtc),
    PaymentTimelineState.Failed => order.MarkPaymentFailed(command.AtUtc),
    PaymentTimelineState.Processing => order.MarkPaymentProcessing(command.AtUtc),
    _ => Result.Ok()
};
```

- [ ] **Step 3: Update `RecordOrderPaymentStateTests.cs`**

Replace `OrderPaymentState.Completed/Failed/Processing` with `PaymentTimelineState.Completed/Failed/Processing` in `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/RecordOrderPaymentState/RecordOrderPaymentStateTests.cs`.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/RecordOrderPaymentState/ \
        service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/RecordOrderPaymentState/
git commit -m "refactor(ordering): rename OrderPaymentState mirror to PaymentTimelineState enum"
```

#### TASK-010: Admin order responses — enum

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/Models/Order.Model.Response.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/Mappings/Order.Mapping.Model.cs`

**Interfaces:**
- Produces: `OrderDetailResponse.PaymentState`/`ShipmentState` and `OrderListItemResponse.PaymentState`/`ShipmentState` typed `OrderPaymentState?`/`OrderShipmentState?`.

- [ ] **Step 1: Type the response fields** (4 occurrences in `Order.Model.Response.cs`)

```csharp
public OrderPaymentState? PaymentState { get; init; }
public OrderShipmentState? ShipmentState { get; init; }
```

- [ ] **Step 2: Verify the mapping**

`Order.Mapping.Model.cs` already assigns `PaymentState = entity.PaymentState` / `ShipmentState = entity.ShipmentState` — no change needed once the entity types are enums (TASK-002). Compile to confirm.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/
git commit -m "refactor(ordering): type payment/shipment state in admin order responses"
```

### Phase 4 — Billing call sites

- GOAL-004: Send enums (not strings) from Billing, and replace the `PaymentForCheckoutResponse.State` string with `IsPending`.

#### TASK-011: `ProcessStripeWebhookEventJob` — enum call sites

**Files:**
- Modify: `service/Api/src/Module/Billing/Backgrounds/ProcessStripeWebhookEventJob.cs`

**Interfaces:**
- Consumes: `RegressCheckoutStateCommand { ... TargetState = CheckoutState.PickDeliveryMethod }`, `PaymentTimelineState.Completed/Failed`.

- [ ] **Step 1: Update the `TryNotifyOrderPaymentStateAsync` signature**

```csharp
private async Task TryNotifyOrderPaymentStateAsync(PaymentCapture payment, PaymentTimelineState paymentState, CancellationToken ct)
{
    var atUtc = paymentState switch
    {
        PaymentTimelineState.Completed => payment.CompletedAtUtc,
        PaymentTimelineState.Failed => payment.FailedAtUtc,
        _ => null
    } ?? DateTimeOffset.UtcNow;
    // ... rest unchanged (PaymentState = paymentState)
}
```

- [ ] **Step 2: Update the call sites** (lines 124 and 156)

`OrderPaymentState.Completed` → `PaymentTimelineState.Completed`; `OrderPaymentState.Failed` → `PaymentTimelineState.Failed`.

- [ ] **Step 3: Update the regress command** (line 362)

```csharp
new RegressCheckoutStateCommand { CartId = payment.OrderId, TargetState = CheckoutState.PickDeliveryMethod }, ct);
```

- [ ] **Step 4: Add usings**

Add `using Module.Ordering.Domain.Orders;` (for `CheckoutState`) if not already present, and ensure `using Module.Ordering.Features.Storefront.RecordOrderPaymentState;` exists (for `PaymentTimelineState`).

- [ ] **Step 5: Update `ProcessStripeWebhookEventJobTests.cs`**

In `service/Api/tests/Module.UnitTests/Billing/Backgrounds/ProcessStripeWebhookEventJobTests.cs`:
- `c.TargetState == "Delivery"` → `c.TargetState == CheckoutState.PickDeliveryMethod` (lines 484, 586)

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Billing/Backgrounds/ProcessStripeWebhookEventJob.cs \
        service/Api/tests/Module.UnitTests/Billing/Backgrounds/ProcessStripeWebhookEventJobTests.cs
git commit -m "refactor(billing): send enum states to ordering from webhook job"
```

#### TASK-012: `CreatePaymentIntent` — enum + processing notify

**Files:**
- Modify: `service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs`

**Interfaces:**
- Consumes: `GetCartForCheckoutResponse.State` as `CheckoutState`; `AdvanceCheckoutStateCommand { TargetState = CheckoutState.PickPaymentMethod }`; `RecordOrderPaymentStateCommand { PaymentState = PaymentTimelineState.Processing }`.

- [ ] **Step 1: Replace the `Enum.TryParse` block (lines 42–46) with typed access**

```csharp
var currentState = cart.State;
if (currentState is not (CheckoutState.PickDeliveryMethod or CheckoutState.PickPaymentMethod))
    return OrderResult.Errors.InvalidCheckoutTransition(currentState, CheckoutState.PickPaymentMethod);
```

- [ ] **Step 2: Rename `currentState == CheckoutState.Payment`** (line 50) → `currentState == CheckoutState.PickPaymentMethod`.

- [ ] **Step 3: Replace the advance command (line 181)**

```csharp
await sender.Send(
    new AdvanceCheckoutStateCommand { CartId = command.Request.OrderId, TargetState = CheckoutState.PickPaymentMethod }, cancellationToken);
```

- [ ] **Step 4: Add the processing notify on the gateway path**

Immediately after `payment.Process();` in the `else` (gateway) branch (line 161), add:

```csharp
payment.Process();

// Mirror: the payment now awaits the checkout webhook — stamp the order's processing time.
await sender.Send(new RecordOrderPaymentStateCommand
{
    OrderId = command.Request.OrderId,
    PaymentState = PaymentTimelineState.Processing,
    AtUtc = DateTimeOffset.UtcNow
}, cancellationToken);
```

Add `using Module.Ordering.Features.Storefront.RecordOrderPaymentState;` if not present. Do NOT add a notify on the COD path (`Process(); Pend();` goes straight to `Pending`).

- [ ] **Step 5: Update `CreatePaymentIntentTests.cs`**

In `service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs`, update the `AdvanceCheckoutStateCommand` mock expectations (lines 207, 226) to assert `TargetState == CheckoutState.PickPaymentMethod`, and add a verification that a `RecordOrderPaymentStateCommand` with `PaymentState == PaymentTimelineState.Processing` is sent on the Stripe path.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/ \
        service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/CreateIntent/
git commit -m "refactor(billing): type checkout state in CreatePaymentIntent and stamp processing"
```

#### TASK-013: `CapturePayment` — enum

**Files:**
- Modify: `service/Api/src/Module/Billing/Features/Admin/Payments/Capture/CapturePayment.cs`

**Interfaces:**
- Consumes: `PaymentTimelineState.Completed`.

- [ ] **Step 1: Update the notify (line 72)**

`PaymentState = OrderPaymentState.Completed,` → `PaymentState = PaymentTimelineState.Completed,`

- [ ] **Step 2: Commit**

```bash
git add service/Api/src/Module/Billing/Features/Admin/Payments/Capture/CapturePayment.cs
git commit -m "refactor(billing): use PaymentTimelineState in capture payment"
```

#### TASK-014: `GetPaymentForCheckout` — replace `State` string with `IsPending`

**Files:**
- Modify: `service/Api/src/Module/Billing/Features/Storefront/GetPaymentForCheckout/GetPaymentForCheckout.Response.cs`
- Modify: `service/Api/src/Module/Billing/Features/Storefront/GetPaymentForCheckout/GetPaymentForCheckout.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs`

**Interfaces:**
- Produces: `PaymentForCheckoutResponse { decimal Amount; bool IsCompleted; bool IsPending; bool IsOffline; DateTimeOffset? CompletedAtUtc }` (removes `string State`).

- [ ] **Step 1: Update the response record**

```csharp
public sealed record PaymentForCheckoutResponse
{
    public decimal Amount { get; init; }
    public bool IsCompleted { get; init; }
    public bool IsPending { get; init; }
    public bool IsOffline { get; init; }
    public DateTimeOffset? CompletedAtUtc { get; init; }
}
```

- [ ] **Step 2: Update the handler** (replace `State` assignment)

```csharp
return new PaymentForCheckoutResponse
{
    Amount = payment?.Amount ?? 0m,
    IsCompleted = payment?.State == PaymentRecordState.Completed,
    IsPending = payment?.State == PaymentRecordState.Pending,
    IsOffline = payment is not null && GatewayConstants.Providers.IsOffline(payment.ProviderKey),
    CompletedAtUtc = payment?.CompletedAtUtc
};
```

- [ ] **Step 3: Update `CreateOrderFromCart`** (line 41)

```csharp
var isPaid = p.IsCompleted || (p.IsPending && p.IsOffline);
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Billing/Features/Storefront/GetPaymentForCheckout/ \
        service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs
git commit -m "refactor(billing): replace payment state string with IsPending bool"
```

- [ ] **Step 5: Verify the full backend is green**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: 0 warnings, 0 errors.
Run: `dotnet test service/Api/tests/Module.UnitTests -v q`
Expected: all pass (1 pre-existing skip is acceptable).

### Phase 5 — Store SPA

- GOAL-005: Type the Store SPA checkout/payment/shipment states with unions + zod.

#### TASK-015: Store types + validations

**Files:**
- Modify: `app/Store/src/features/ordering/types/order.ts`
- Modify: `app/Store/src/features/ordering/types/cart.ts`
- Modify: `app/Store/src/features/ordering/types/index.ts`
- Modify: `app/Store/src/features/ordering/validations/order.ts`
- Modify: `app/Store/src/features/ordering/validations/cart.ts`
- Modify: `app/Store/src/features/ordering/validations/index.ts`

**Interfaces:**
- Produces: `CheckoutState = 'Address' | 'PickDeliveryMethod' | 'PickPaymentMethod' | 'Confirm' | 'Complete'`; `OrderPaymentState` union (9 values); `OrderShipmentState` union (6 values); `CartResponse.checkoutState: CheckoutState`.

- [ ] **Step 1: Update `types/order.ts`**

```ts
export type OrderStatus = 'Draft' | 'Placed' | 'Canceled' | 'Expired'
export type CheckoutState = 'Address' | 'PickDeliveryMethod' | 'PickPaymentMethod' | 'Confirm' | 'Complete'
export type OrderPaymentState = 'Completed' | 'Failed' | 'Void' | 'BalanceDue' | 'CreditOwed' | 'Paid' | 'Pending' | 'Checkout' | 'Invalid'
export type OrderShipmentState = 'Pending' | 'Delivered' | 'Partial' | 'Ready' | 'Backorder' | 'Canceled'
```

Change `OrderDetail.paymentState`/`shipmentState` from `string | null` to `OrderPaymentState | null` / `OrderShipmentState | null`.

- [ ] **Step 2: Update `types/cart.ts`**

`checkoutState: string` → `checkoutState: CheckoutState` (import `CheckoutState` from `./order`).

- [ ] **Step 3: Re-export from `types/index.ts`**

Add `OrderPaymentState`, `OrderShipmentState` to the existing re-export list alongside `CheckoutState`.

- [ ] **Step 4: Update zod schemas**

In `validations/order.ts`:

```ts
export const CheckoutStateSchema = z.enum(['Address', 'PickDeliveryMethod', 'PickPaymentMethod', 'Confirm', 'Complete'])
export const OrderPaymentStateSchema = z.enum(['Completed', 'Failed', 'Void', 'BalanceDue', 'CreditOwed', 'Paid', 'Pending', 'Checkout', 'Invalid'])
export const OrderShipmentStateSchema = z.enum(['Pending', 'Delivered', 'Partial', 'Ready', 'Backorder', 'Canceled'])
```

Change `paymentState: z.string().nullable()` → `paymentState: OrderPaymentStateSchema.nullable()` and `shipmentState: z.string().nullable()` → `shipmentState: OrderShipmentStateSchema.nullable()`.

In `validations/cart.ts`: `checkoutState: z.string()` → `checkoutState: CheckoutStateSchema` (import from `./order`).

- [ ] **Step 5: Re-export schemas** in `validations/index.ts` (`OrderPaymentStateSchema`, `OrderShipmentStateSchema`).

- [ ] **Step 6: Commit**

```bash
git add app/Store/src/features/ordering/types/ app/Store/src/features/ordering/validations/
git commit -m "refactor(store): type checkout/payment/shipment states as unions"
```

#### TASK-016: Store composables + fixtures

**Files:**
- Modify: `app/Store/src/features/ordering/composables/useCheckout.ts`
- Modify: `app/Store/src/features/ordering/composables/useCart.ts`
- Modify: `app/Store/src/features/ordering/views/__tests__/CheckoutView.spec.ts`
- Modify: `app/Store/src/features/ordering/views/__tests__/OrderDetailView.spec.ts`
- Modify: `app/Store/src/features/ordering/views/__tests__/CartView.spec.ts`
- Modify: `app/Store/src/features/ordering/components/__tests__/CartDrawer.spec.ts`

**Interfaces:**
- Consumes: `CheckoutState` union (TASK-015).

- [ ] **Step 1: Update `stepOf`** in `useCheckout.ts` (lines 16–25)

```ts
function stepOf(state: string | null): Step {
  switch (state) {
    case 'Address': return 1
    case 'PickDeliveryMethod': return 2
    case 'PickPaymentMethod': return 3
    case 'Confirm': return 4
    case 'Complete': return 5
    default: return 1
  }
}
```

- [ ] **Step 2: Update `useCart.ts`**

`checkoutState = ref<string | null>(null)` → `ref<CheckoutState | null>(null)` (import type from `../types/order`).

- [ ] **Step 3: Update test fixtures**

Across the four `.spec.ts` files, replace fixture string literals:
- `checkoutState: 'Payment'` → `'PickPaymentMethod'`
- `checkoutState: 'Delivery'` → `'PickDeliveryMethod'`
- `checkoutState: 'Complete'` → `'Complete'` (unchanged)
- `checkoutState: 'address'` → `'Address'` (normalize casing to the union)
- `paymentState: 'Paid'` → keep `'Paid'` (already PascalCase) but type as `OrderPaymentState`.

- [ ] **Step 4: Verify**

Run: `cd app/Store && pnpm run lint && pnpm run test:unit`
Expected: zero warnings, all tests pass.

- [ ] **Step 5: Commit**

```bash
git add app/Store/src/features/ordering/
git commit -m "refactor(store): update composables and fixtures for pick-method checkout state"
```

### Phase 6 — Admin SPA

- GOAL-006: Type the Admin SPA checkout/payment/shipment states.

#### TASK-017: Admin types + views

**Files:**
- Modify: `app/Admin/src/features/ordering/types/order.ts`
- Modify: `app/Admin/src/features/ordering/types/index.ts`
- Modify: `app/Admin/src/features/ordering/views/OrdersList.vue`
- Modify: `app/Admin/src/features/ordering/views/OrderDetail.vue`
- Modify: `app/Admin/src/features/ordering/__tests__/types/order.spec.ts`

**Interfaces:**
- Consumes: `CheckoutState`, `OrderPaymentState`, `OrderShipmentState` unions.

- [ ] **Step 1: Update `types/order.ts`**

```ts
export type CheckoutState = 'Address' | 'PickDeliveryMethod' | 'PickPaymentMethod' | 'Confirm' | 'Complete'
export type OrderPaymentState = 'Completed' | 'Failed' | 'Void' | 'BalanceDue' | 'CreditOwed' | 'Paid' | 'Pending' | 'Checkout' | 'Invalid'
export type ShipmentState = 'Pending' | 'Delivered' | 'Partial' | 'Ready' | 'Backorder' | 'Canceled'
export const SHIPMENT_STATE_OPTIONS: ShipmentState[] = ['Pending', 'Delivered', 'Partial', 'Ready', 'Backorder', 'Canceled']
```

Type `paymentState?: string` → `paymentState?: OrderPaymentState` and `shipmentState?: string` → `shipmentState?: ShipmentState` in `OrderListItem`/`OrderDetail`.

- [ ] **Step 2: Update `OrdersList.vue`**

`CHECKOUT_STATE_OPTIONS: CheckoutState[] = ['Address', 'PickDeliveryMethod', 'PickPaymentMethod', 'Confirm', 'Complete']`.

- [ ] **Step 3: Update `OrderDetail.vue`**

Update `onShipmentStateChange(value: ShipmentState)` to compare PascalCase values; ensure the dropdown options use `SHIPMENT_STATE_OPTIONS` (PascalCase).

- [ ] **Step 4: Update `order.spec.ts`**

`checkoutState: 'Payment'` → `'PickPaymentMethod'` (line 25), and any `paymentState`/`shipmentState` fixtures to PascalCase union values.

- [ ] **Step 5: Verify**

Run: `cd app/Admin && pnpm run lint && pnpm run test:unit`
Expected: zero warnings, all tests pass.

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/features/ordering/
git commit -m "refactor(admin): type checkout/payment/shipment states as unions"
```

### Phase 7 — Migration consolidation + verification

- GOAL-007: Drop the in-progress migrations, generate one consolidated backfill migration, and run the full quality gate.

#### TASK-018: Drop migrations and create the consolidated backfill

**Files:**
- Delete: `service/Api/src/Migrations/Migrations/20260813090249_RemoveTaxCategoryIdFromShippingMethod.cs`
- Delete: `service/Api/src/Migrations/Migrations/20260813090249_RemoveTaxCategoryIdFromShippingMethod.Designer.cs`
- Delete: `service/Api/src/Migrations/Migrations/20260814011730_AddPaymentBusinessTimestamps.cs`
- Delete: `service/Api/src/Migrations/Migrations/20260814011730_AddPaymentBusinessTimestamps.Designer.cs`
- Reset: `service/Api/src/Migrations/Migrations/ApplicationDbContextModelSnapshot.cs` (to last committed baseline)
- Create: `service/Api/src/Migrations/Migrations/*_RenameCheckoutStateAndBackfillStatusEnums.cs`

> **⚠️ Human approval required (AGENTS.md rule 6):** resetting the snapshot requires `git checkout -- <path>` or `git restore`. Ask the human for an explicit "yes" before running it.

- [ ] **Step 1: Delete the four untracked migration files** (plain `rm`, not git)

```bash
rm service/Api/src/Migrations/Migrations/20260813090249_RemoveTaxCategoryIdFromShippingMethod.cs \
   service/Api/src/Migrations/Migrations/20260813090249_RemoveTaxCategoryIdFromShippingMethod.Designer.cs \
   service/Api/src/Migrations/Migrations/20260814011730_AddPaymentBusinessTimestamps.cs \
   service/Api/src/Migrations/Migrations/20260814011730_AddPaymentBusinessTimestamps.Designer.cs
```

- [ ] **Step 2: Reset the snapshot (with explicit human approval)**

```bash
git checkout -- service/Api/src/Migrations/Migrations/ApplicationDbContextModelSnapshot.cs
```

- [ ] **Step 3: Generate the consolidated migration**

```bash
dotnet ef migrations add RenameCheckoutStateAndBackfillStatusEnums \
  --project service/Api/src/Migrations/Api.Migrations.csproj \
  --startup-project service/Api/src/Api/Api.csproj
```

Expected: the generated `Up()` re-adds the payment business-timestamp columns (PaymentProcessingAt, PaymentCompletedAt, PaymentFailedAt, ShippedAt, DeliveredAt, DeliveryExceptionAt, EstimatedDeliveryAt, etc.) that were in the dropped migration, because the `Order` model still declares them.

- [ ] **Step 4: Append the backfill SQL to the generated `Up()`**

Add `migrationBuilder.Sql(...)` calls carrying the value map (before or after the column adds, as applicable):

```csharp
migrationBuilder.Sql("UPDATE ordering.orders SET \"CheckoutState\" = 'PickDeliveryMethod' WHERE \"CheckoutState\" = 'Delivery';");
migrationBuilder.Sql("UPDATE ordering.orders SET \"CheckoutState\" = 'PickPaymentMethod'    WHERE \"CheckoutState\" = 'Payment';");
migrationBuilder.Sql("UPDATE ordering.orders SET \"PaymentState\" = 'Completed'  WHERE \"PaymentState\" = 'completed';");
migrationBuilder.Sql("UPDATE ordering.orders SET \"PaymentState\" = 'Failed'     WHERE \"PaymentState\" = 'failed';");
migrationBuilder.Sql("UPDATE ordering.orders SET \"PaymentState\" = 'Void'       WHERE \"PaymentState\" = 'void';");
migrationBuilder.Sql("UPDATE ordering.orders SET \"PaymentState\" = 'BalanceDue' WHERE \"PaymentState\" = 'balance_due';");
migrationBuilder.Sql("UPDATE ordering.orders SET \"PaymentState\" = 'CreditOwed' WHERE \"PaymentState\" = 'credit_owed';");
migrationBuilder.Sql("UPDATE ordering.orders SET \"PaymentState\" = 'Paid'       WHERE \"PaymentState\" = 'paid';");
migrationBuilder.Sql("UPDATE ordering.orders SET \"PaymentState\" = 'Pending'    WHERE \"PaymentState\" = 'pending';");
migrationBuilder.Sql("UPDATE ordering.orders SET \"PaymentState\" = 'Checkout'   WHERE \"PaymentState\" = 'checkout';");
migrationBuilder.Sql("UPDATE ordering.orders SET \"PaymentState\" = 'Invalid'    WHERE \"PaymentState\" = 'invalid';");
migrationBuilder.Sql("UPDATE ordering.orders SET \"ShipmentState\" = 'Pending'   WHERE \"ShipmentState\" = 'pending';");
migrationBuilder.Sql("UPDATE ordering.orders SET \"ShipmentState\" = 'Delivered' WHERE \"ShipmentState\" = 'delivered';");
migrationBuilder.Sql("UPDATE ordering.orders SET \"ShipmentState\" = 'Partial'   WHERE \"ShipmentState\" = 'partial';");
migrationBuilder.Sql("UPDATE ordering.orders SET \"ShipmentState\" = 'Ready'     WHERE \"ShipmentState\" = 'ready';");
migrationBuilder.Sql("UPDATE ordering.orders SET \"ShipmentState\" = 'Backorder' WHERE \"ShipmentState\" = 'backorder';");
migrationBuilder.Sql("UPDATE ordering.orders SET \"ShipmentState\" = 'Canceled'  WHERE \"ShipmentState\" = 'canceled';");
```

Confirm the exact table/schema and column casing against the generated migration and `ApplicationDbContextModelSnapshot` before finalizing.

- [ ] **Step 5: Verify build + generate snapshot**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Migrations/Migrations/
git commit -m "refactor(migrations): consolidate payment timestamps and status backfill into one migration"
```

#### TASK-019: Full verification

- [ ] **Step 1: Backend**

```bash
dotnet build
dotnet test service/Api/tests/Module.UnitTests
dotnet test service/Api/tests/Shared.UnitTests
bash scripts/check-feature-conventions.sh
bash scripts/check-cross-module-refs.sh
```

- [ ] **Step 2: SPAs**

```bash
cd app/Admin && pnpm run lint && pnpm run test:unit
cd app/Store && pnpm run lint && pnpm run test:unit
```

- [ ] **Step 3: Confirm no residuals**

```bash
rg -n "PaymentSelected|CheckoutState\.Payment|OrderConstant\.PaymentState|OrderConstant\.ShipmentState|TargetState = \"(Payment|Delivery)\"|p\.State == \"Pending\"" service/Api app/Store/src app/Admin/src --glob '*.{cs,ts,vue}'
```

Expected: zero matches.

- [ ] **Step 4: Commit any straggler fixes**

---

## 3. Alternatives

- **ALT-001**: Keep `CheckoutState.Payment` and only remove the `MarkPaymentProcessing` call. Rejected: the mislabel is the root cause; renaming makes the state machine self-documenting and prevents recurrence.
- **ALT-002**: Add a 6th `Payment` state for processing. Rejected: processing is asynchronous and already modeled by `PaymentRecordState` + order timestamps.
- **ALT-003**: Preserve lowercase snake_case wire values via `EnumMember` + a custom converter (no data migration). Rejected: inconsistent with `OrderStatus`/`CheckoutState` PascalCase convention; a backfill migration is mechanical.
- **ALT-004**: Keep `PaymentForCheckoutResponse.State` and only add `IsPending`. Rejected: leaves a cross-module `PaymentRecordState` string leak; removing it eliminates the `p.State == "Pending"` magic string.

## 4. Dependencies

- **DEP-001**: Existing `Order` partial classes (`Order.Method.Checkout.cs`, `Order.Method.Computation.cs`, `Order.Method.Timestamps.cs`) — modified in Phase 1.
- **DEP-002**: `RecordOrderPaymentStateCommand` + handler — the cross-module mirror renamed in TASK-009 and consumed by Billing (TASK-011/012/013).
- **DEP-003**: `GetPaymentForCheckout` query/response — consumed by `CreateOrderFromCart`/`CompleteCheckoutForPayment`; changed in TASK-014.
- **DEP-004**: Global `JsonStringEnumConverter` (`Program.cs:33`) — required for PascalCase enum wire serialization (already present).
- **DEP-005**: EF Core migration tooling (`dotnet ef`, `Api.Migrations.csproj`) — TASK-018.
- **DEP-006**: `PaymentCapture` / `PaymentRecordState` (Billing) — unchanged; only the Ordering mirror and call sites change.

## 5. Files

- **FILE-001**: `service/Api/src/Module/Ordering/Domain/Orders/Order.Enumerate.cs` — enums (TASK-001).
- **FILE-002**: `service/Api/src/Module/Ordering/Domain/Orders/Order.Constant.cs` — remove string constants (TASK-001).
- **FILE-003**: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Checkout.cs` — transitions + delete dead helpers (TASK-002).
- **FILE-004**: `service/Api/src/Module/Ordering/Domain/Orders/Order.Validation.cs` — enum switch (TASK-002).
- **FILE-005**: `service/Api/src/Module/Ordering/Domain/Orders/Order.cs` — property types (TASK-002).
- **FILE-006**: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Computation.cs` — `UpdatePaymentState` (TASK-002).
- **FILE-007**: `service/Api/src/Module/Ordering/Persistence/Configurations/OrderConfiguration.cs` — `.HasConversion<string>()` (TASK-002).
- **FILE-008**: `service/Api/src/Module/Ordering/Features/Storefront/AdvanceCheckoutState/*` (TASK-004).
- **FILE-009**: `service/Api/src/Module/Ordering/Features/Storefront/RegressCheckoutState/*` (TASK-005).
- **FILE-010**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/*` (TASK-006).
- **FILE-011**: `service/Api/src/Module/Ordering/Features/Storefront/GetCartForCheckout/*` (TASK-006).
- **FILE-012**: `service/Api/src/Module/Ordering/Features/Storefront/CompleteCheckoutForPayment/CompleteCheckoutForPayment.cs` (TASK-007).
- **FILE-013**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` (TASK-007, TASK-014).
- **FILE-014**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/SelectShippingRate/SelectShippingRate.cs` (TASK-007).
- **FILE-015**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateCheckout/UpdateCheckout.cs` (TASK-007).
- **FILE-016**: `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateShipmentState/*` (TASK-008).
- **FILE-017**: `service/Api/src/Module/Ordering/Features/Storefront/RecordOrderPaymentState/*` (TASK-009).
- **FILE-018**: `service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/*` (TASK-010).
- **FILE-019**: `service/Api/src/Module/Billing/Backgrounds/ProcessStripeWebhookEventJob.cs` (TASK-011).
- **FILE-020**: `service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs` (TASK-012).
- **FILE-021**: `service/Api/src/Module/Billing/Features/Admin/Payments/Capture/CapturePayment.cs` (TASK-013).
- **FILE-022**: `service/Api/src/Module/Billing/Features/Storefront/GetPaymentForCheckout/*` (TASK-014).
- **FILE-023**: `app/Store/src/features/ordering/types/*`, `validations/*` (TASK-015).
- **FILE-024**: `app/Store/src/features/ordering/composables/*`, `views/__tests__/*`, `components/__tests__/*` (TASK-016).
- **FILE-025**: `app/Admin/src/features/ordering/types/*`, `views/*`, `__tests__/*` (TASK-017).
- **FILE-026**: `service/Api/src/Migrations/Migrations/*` (TASK-018).
- **FILE-027**: Test files: `Order.Method.Tests.cs`, `Order.Validation.Tests.cs`, `RecordOrderPaymentStateTests.cs`, `Cart.Mapping.Tests.cs`, `SelectShippingRateRegressionTests.cs`, `UpdateCheckout.Tests.cs`, `CreateOrderFromCart*Tests.cs`, `CreatePaymentIntentTests.cs`, `ProcessStripeWebhookEventJobTests.cs`, `CreateIntent.IntegrationTests.cs`.

## 6. Testing

- **TEST-001**: `dotnet build` — 0 warnings, 0 errors.
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests` — all pass (1 pre-existing skip acceptable).
- **TEST-003**: `dotnet test service/Api/tests/Shared.UnitTests` — pass.
- **TEST-004**: `bash scripts/check-feature-conventions.sh` — pass.
- **TEST-005**: `bash scripts/check-cross-module-refs.sh` — baseline unchanged (33).
- **TEST-006**: `cd app/Store && pnpm run lint && pnpm run test:unit` — pass.
- **TEST-007**: `cd app/Admin && pnpm run lint && pnpm run test:unit` — pass.
- **TEST-008**: Migration smoke (integration, requires Docker): apply the consolidated migration to a seeded DB and assert no `orders` row retains a legacy lowercase/old-name status value.
- **TEST-009**: Residual scan (`rg` in TASK-019 Step 3) returns zero matches for removed identifiers.

## 7. Risks & Assumptions

- **RISK-001**: The enum rename is cross-cutting — the backend is red until all of Tasks 1–14 complete. Mitigated by committing only at green phase boundaries.
- **RISK-002**: Snapshot reset (TASK-018 Step 2) requires `git checkout --`, forbidden by rule 6 without explicit approval. Mitigated by an explicit approval gate.
- **RISK-003**: The dropped `AddPaymentBusinessTimestamps` migration also added unrelated timestamp columns; the regenerated consolidated migration must re-add them or data is lost. Mitigated by verifying the generated `Up()` re-adds all columns before applying.
- **RISK-004**: `RemoveTaxCategoryIdFromShippingMethod` may belong to a separate in-progress shipping refactor; dropping it discards that unrelated change. Confirm ownership with the human before deleting (OQ-5).
- **ASSUMPTION-001**: No concurrent editor is modifying the tree; verify before starting (OQ-5).
- **ASSUMPTION-002**: `JsonStringEnumConverter` is registered globally (verified at `Program.cs:33`).
- **ASSUMPTION-003**: `OrderConstant.Defaults.PaymentState`/`ShipmentState` are unused (verified by grep — only their definitions matched).

## 8. Related Specifications / Further Reading

- [spec/spec-checkout-state-enum-alignment.md](../spec/spec-checkout-state-enum-alignment.md)
- [plan/refactor-checkout-state-sync-1.md](./refactor-checkout-state-sync-1.md)
- [plan/feature-payment-method-selection-1.md](./feature-payment-method-selection-1.md)
- [docs/codebase/ARCHITECTURE.md](../docs/codebase/ARCHITECTURE.md)
- [docs/codebase/CONVENTIONS.md](../docs/codebase/CONVENTIONS.md)
- [AGENTS.md](../AGENTS.md)
