# Fix Payment-Success → Inventory Correctness — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: use `superpowers:subagent-driven-development` (recommended) or `superpowers:executing-plans`. Steps use `- [ ]` checkbox syntax.

**Goal:** Fix double-deduction, missing sale stock-movement, and release/expiry stock inflation in the payment-success → inventory flow, and add happy-path + branch-coverage unit tests for every module in the end-to-end flow.

**Architecture:** Fix the reservation lifecycle at its two boundaries (create-intent releases the prior set; consume caps at the ordered quantity and releases surplus), remove erroneous `CountOnHand` mutations on release/expiry, add the sale `StockMovement` write, and wire up the expiry sweep + session-expiry release. Availability remains a derived value (`CountOnHand − Σ activeReserved`); no stored `Reserved`/`Available` columns are introduced. Phase 2 closes the remaining unit-test coverage gaps (CheckoutPlacementService, CompleteCheckoutForPayment, CreateShipment, ShipmentFulfillmentSyncService, UpdateShipmentStatus, plus consume happy-path/empty/multi-location branches and CreatePaymentIntent offline/compensation branches).

**Tech Stack:** .NET 10, EF Core (InMemory test provider), MediatR, xUnit + FluentAssertions.

**Spec:** `docs/codebase/SPEC-payment-success-inventory-correctness.md`

## Global Constraints

- `TreatWarningsAsErrors=true` — every task must leave `dotnet build` clean.
- Result objects over exceptions; domain mutations via `static partial class` method files.
- `scripts/check-cross-module-refs.sh` baseline must stay at 46; `check-feature-conventions.sh` green.
- Do NOT run `git stash/restore/revert/checkout --` (AGENTS.md rule 6). Uncommitted WIP is precious.
- The working tree already contains a large uncommitted refactor (Shared consolidation, migration regen, EF relationships) plus this feature's WIP `StockConsumeLine` change. Do not mix unrelated files into these commits.
- **Test invocation:** `dotnet test --filter ...` does NOT work in this repo (xunit v3 MTP runner rejects `--filter`/`--nologo` — "Zero tests ran"). Run tests via the native runner after `dotnet build`:
  ```bash
  cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
  ./Module.UnitTests -class "Fully.Qualified.Test.ClassName"   # class-scoped
  ./Module.UnitTests -filter "/assembly/ns/class/method[trait=value]"   # query syntax
  ```
  Verified: `./Module.UnitTests -class "Module.UnitTests.Inventory.Services.StockReservationServiceTests"` runs 28 tests; `-class "Module.UnitTests.Payment.Features.Storefront.Payment.CreateIntent.CreatePaymentIntentTests"` runs 4 tests. Pre-existing failures: 3 in `OrderStatusValueConverterTests` (unrelated WIP NRE) — ignore. Exit-code check: a failing assertion returns non-zero, so rely on the `Failed: N` line in output.

## File Structure

- `service/Api/src/Module/Inventory/Services/StockReservations/StockReservation.Service.Implementation.cs` — release/expire/consume fixes + movement write (core file).
- `service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs` — release-before-reserve.
- `service/Api/src/Module/Billing/Backgrounds/ProcessStripeWebhookEventJob.cs` — session-expiry release by cartToken.
- `service/Api/src/Module/Inventory/Inventory.Extension.cs` — register expiry scheduler.
- `service/Api/tests/Module.UnitTests/Inventory/Services/StockReservationServiceTests.cs` — update 3 existing tests + add new tests.
- `service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs` — release-before-reserve test.

---

### Task 1: Stop `CountOnHand` inflation on release and expiry (R3)

**Files:**
- Modify: `service/Api/src/Module/Inventory/Services/StockReservations/StockReservation.Service.Implementation.cs:181-219` (`ReleaseReservationsAsync`) and `:251-282` (`ExpireReservationsAsync`).
- Test: `service/Api/tests/Module.UnitTests/Inventory/Services/StockReservationServiceTests.cs` (3 existing tests encode the buggy behavior and must be updated: `:255`, `:437`, `:470`).

**Interfaces:**
- Produces: `ReleaseReservationsAsync` / `ExpireReservationsAsync` no longer load or mutate `StockItem`; they only flip reservation `State`. The `StockItem` type is still imported for `ReserveForVariantAsync`/`ConsumeForOrderAsync`, so keep `using Module.Inventory.Domain.StockItems;`.

- [ ] **Step 1: Update the three existing tests to assert NO on-hand change.**

In `StockReservationServiceTests.cs`:

`ReleaseReservationsAsync_ShouldReleaseAndRestoreStock_ByCartToken` (`:255-275`): change the final assertion from `stockItem.CountOnHand.Should().Be(10);` to `stockItem.CountOnHand.Should().Be(5);` (seed was `SeedStockItem(5)`), and rename the test + DisplayName to `ReleaseReservationsAsync_ShouldReleaseWithoutChangingOnHand` / `"ReleaseReservationsAsync: Should release cart reservations without changing on-hand"`.

`ExpireReservationsAsync_ShouldExpireAndRestoreStock` (`:437-455`): change `stockItem.CountOnHand.Should().Be(7);` to `stockItem.CountOnHand.Should().Be(5);`, rename to `ExpireReservationsAsync_ShouldExpireWithoutChangingOnHand` / `"ExpireReservationsAsync: Should expire overdue reservations without changing on-hand"`.

`ExpireReservationsAsync_ShouldHandleMultipleExpired` (`:470-491`): change `stockItem.CountOnHand.Should().Be(10 + 2 + 3);` to `stockItem.CountOnHand.Should().Be(10);`, rename to `ExpireReservationsAsync_ShouldExpireMultipleWithoutChangingOnHand`.

- [ ] **Step 2: Run to verify the updated tests now FAIL.**

Run:
```bash
dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Inventory.Services.StockReservationServiceTests"
```
Expected: FAIL — `Failed: 3` (current impl still adds back to `CountOnHand`, so `5` is actually `10`/`7`/`15`).

- [ ] **Step 3: Remove the `CountOnHand +=` blocks.**

In `ReleaseReservationsAsync` delete the `if (r.StockLocationId is not null) { … stockItem.CountOnHand += r.Quantity; }` block (currently `:202-209`), leaving only `r.Release()` + `r.ModifiedAtUtc = DateTimeOffset.UtcNow;`.

In `ExpireReservationsAsync` delete the identical block (currently `:265-272`).

- [ ] **Step 4: Run the tests to verify pass.**

Run:
```bash
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Inventory.Services.StockReservationServiceTests"
```
Expected: PASS (`Failed: 0`). Also run `dotnet build` — must be clean.

- [ ] **Step 5: Commit.**

```bash
git add service/Api/src/Module/Inventory/Services/StockReservations/StockReservation.Service.Implementation.cs \
        service/Api/tests/Module.UnitTests/Inventory/Services/StockReservationServiceTests.cs
git commit -m "fix(inventory): stop inflating CountOnHand on reservation release/expiry"
```

---

### Task 2: Release prior cart reservations before reserving in CreatePaymentIntent (R2)

**Files:**
- Modify: `service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs:42-90`.
- Test: `service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs`.

**Interfaces:**
- Consumes: `IStockReservationService.ReleaseCartReservationsAsync(string cartToken, Guid? variantId = null, CancellationToken)` — already on the interface (`StockReservation.Service.Interface.cs:13`) and is the non-inflating variant.

- [ ] **Step 1: Write a failing test.**

Add to `CreatePaymentIntentTests.cs` a test that runs the **first-time** path (`PickDeliveryMethod` state, the default `SetupCartForCheckout`) with a non-empty `LineItems`, and asserts `ReleaseCartReservationsAsync` is called once before `ReserveForVariantAsync`:

```csharp
[Fact(DisplayName = "Handler: releases prior cart reservations before re-reserving on first intent")]
public async Task Handle_FirstIntent_ReleasesPriorReservations_BeforeReserving()
{
    var order = CreateOrder();
    var pm = new PaymentMethod { Name = "Credit Card", Code = "credit_card",
        ProviderKey = GatewayConstants.Providers.Stripe, Active = true };
    _dbContext.Set<PaymentMethod>().Add(pm);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    _senderMock.Setup(x => x.Send(
        It.Is<GetCartForCheckoutQuery>(q => q.CartId == order.Id),
        It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<GetCartForCheckoutResponse>.Ok(new GetCartForCheckoutResponse
        {
            State = CheckoutState.PickDeliveryMethod,
            Total = 100.00m,
            Email = "test@example.com",
            LineItems = [ new() { VariantId = Guid.NewGuid(), Quantity = 1 } ]
        }));

    var result = await _handler.Handle(
        new CreatePaymentIntent.Command(new CreatePaymentIntent.Request { OrderId = order.Id, PaymentMethodId = pm.Id }),
        TestContext.Current.CancellationToken);

    result.IsSuccess.Should().BeTrue();
    _reservationServiceMock.Verify(s => s.ReleaseCartReservationsAsync(
        order.Id.ToString(), null, It.IsAny<CancellationToken>()), Times.Once);
}
```

> Note: the test fixture does not mock `ReleaseCartReservationsAsync` yet; add `_reservationServiceMock.Setup(s => s.ReleaseCartReservationsAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result<int>.Ok(0));` in the constructor.

- [ ] **Step 2: Run to verify fail.**

Run:
```bash
dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Payment.Features.Storefront.Payment.CreateIntent.CreatePaymentIntentTests"
```
Expected: FAIL — `Failed: 1` (Times.Once on `ReleaseCartReservationsAsync` — currently never called).

- [ ] **Step 3: Add the release call.**

In `CreatePaymentIntent.cs`, immediately before the reserve loop (currently line 74), add:

```csharp
// Release any prior cart holds (add-to-cart) so exactly one reservation set exists
// at consume time. ReleaseCartReservationsAsync only flips State=Released; it does
// not touch CountOnHand (availability is derived).
await stockReservationService.ReleaseCartReservationsAsync(
    cartToken: command.Request.OrderId.ToString(), ct: cancellationToken);
```

- [ ] **Step 4: Run tests.**

Run:
```bash
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Payment.Features.Storefront.Payment.CreateIntent.CreatePaymentIntentTests"
```
Expected: PASS (`Failed: 0`). `dotnet build` clean.

- [ ] **Step 5: Commit.**

```bash
git add service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs \
        service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs
git commit -m "fix(billing): release prior cart reservations before intent-time reserve"
```

---

### Task 3: Cap consumption at ordered quantity in ConsumeForOrderAsync (R1, R7)

**Files:**
- Modify: `service/Api/src/Module/Inventory/Services/StockReservations/StockReservation.Service.Implementation.cs:333-358`.
- Test: `service/Api/tests/Module.UnitTests/Inventory/Services/StockReservationServiceTests.cs` (new tests).

**Interfaces:**
- Produces: `ConsumeForOrderAsync(Guid orderId, IReadOnlyCollection<StockConsumeLine> lines, CancellationToken)` picks only up to each variant's ordered quantity; surplus rows are `Released`.

- [ ] **Step 1: Write failing tests.**

Add to `StockReservationServiceTests.cs`:

```csharp
[Fact(DisplayName = "ConsumeForOrderAsync: consumes at most ordered quantity and releases excess")]
public async Task ConsumeForOrderAsync_CapsAtOrderedQuantity_ReleasesExcess()
{
    var ct = TestContext.Current.CancellationToken;
    var item = await SeedStockItem(10);
    await SeedCartReservation(2, _orderId.ToString());
    await SeedCartReservation(2, _orderId.ToString());

    var result = await _service.ConsumeForOrderAsync(
        _orderId, new List<StockConsumeLine> { new(_variantId, 2) }, ct);
    await _dbContext.SaveChangesAsync(ct);

    result.IsSuccess.Should().BeTrue();
    _dbContext.ChangeTracker.Clear();
    var reloaded = await _dbContext.Set<StockItem>().FirstAsync(s => s.Id == item.Id, ct);
    reloaded.CountOnHand.Should().Be(8);

    var reservations = await _dbContext.Set<StockReservation>().ToListAsync(ct);
    reservations.Count(r => r.State == ReservationState.Fulfilled).Should().Be(1);
    reservations.Count(r => r.State == ReservationState.Released).Should().Be(1);
}

[Fact(DisplayName = "ConsumeForOrderAsync: releases a reservation whose variant is not in the order")]
public async Task ConsumeForOrderAsync_ReleasesUnorderedVariantReservation()
{
    var ct = TestContext.Current.CancellationToken;
    var item = await SeedStockItem(10);
    await SeedCartReservation(2, _orderId.ToString());
    var otherVariant = Guid.NewGuid();
    _dbContext.Set<StockReservation>().Add(StockReservationMethod.SeedForTest(
        otherVariant, 3, ReservationState.Reserved, DateTimeOffset.UtcNow.AddMinutes(30),
        _stockLocationId, _orderId, _orderId.ToString(), DateTimeOffset.UtcNow));
    await _dbContext.SaveChangesAsync(ct);

    var result = await _service.ConsumeForOrderAsync(
        _orderId, new List<StockConsumeLine> { new(_variantId, 2) }, ct);
    await _dbContext.SaveChangesAsync(ct);

    result.IsSuccess.Should().BeTrue();
    var other = await _dbContext.Set<StockReservation>()
        .FirstAsync(r => r.VariantId == otherVariant, ct);
    other.State.Should().Be(ReservationState.Released);
}
```

- [ ] **Step 2: Run to verify fail.**

Run:
```bash
dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Inventory.Services.StockReservationServiceTests"
```
Expected: FAIL — `Failed: 2` (current impl picks both rows: CountOnHand becomes 6, not 8; the unordered-variant row gets `Pick`ed instead of `Released`).

- [ ] **Step 3: Rewrite the consume loop (lines 333-356).**

```csharp
// Consume: fulfill only up to the ordered quantity per variant. Any reservation
// beyond the order (duplicate rows or variants not in the order) is released, never
// picked, so a paid order can never over-deduct.
var remainingByVariant = lines
    .GroupBy(l => l.VariantId)
    .ToDictionary(g => g.Key, g => g.Sum(l => l.Quantity));

var reservedToConsume = await dbContext.Set<StockReservation>()
    .Where(r => r.CartToken == orderId.ToString()
                && r.State == ReservationState.Reserved)
    .OrderBy(r => r.CreatedAtUtc)
    .ToListAsync(ct);

foreach (var reservation in reservedToConsume)
{
    if (!remainingByVariant.TryGetValue(reservation.VariantId, out var remaining) || remaining <= 0)
    {
        reservation.State = ReservationState.Released;
        reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;
        continue;
    }

    var take = Math.Min(reservation.Quantity, remaining);
    var stockItem = await dbContext.Set<StockItem>()
        .FirstOrDefaultAsync(
            si => si.VariantId == reservation.VariantId
                  && si.StockLocationId == reservation.StockLocationId,
            ct);

    if (stockItem is null)
        return StockReservationResult.Errors.StockItemNotFound(reservation.VariantId);

    var pickResult = stockItem.Pick(take);
    if (pickResult.IsFailure)
        return pickResult.Errors;

    if (take == reservation.Quantity)
        reservation.State = ReservationState.Fulfilled;
    else
        reservation.Quantity -= take;
    reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;
    remainingByVariant[reservation.VariantId] = remaining - take;
}
```

Note: the earlier `remainingByVariant` variable (used for re-reserve shortfall at `:300-331`) and this new one serve different purposes. The re-reserve block already computed a shortfall dictionary; reusing its name here is fine because it is a fresh, recomputed dictionary scoped to this loop. If you prefer to avoid confusion, rename the new local to `toConsumeByVariant` in both Step 1 and Step 3.

- [ ] **Step 4: Run tests.**

Run:
```bash
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Inventory.Services.StockReservationServiceTests"
```
Expected: PASS (`Failed: 0`). `dotnet build` clean.

- [ ] **Step 5: Commit.**

```bash
git add service/Api/src/Module/Inventory/Services/StockReservations/StockReservation.Service.Implementation.cs \
        service/Api/tests/Module.UnitTests/Inventory/Services/StockReservationServiceTests.cs
git commit -m "fix(inventory): cap consume at ordered quantity and release surplus"
```

---

### Task 4: Create StockMovement on sale (R4)

**Files:**
- Modify: `service/Api/src/Module/Inventory/Services/StockReservations/StockReservation.Service.Implementation.cs` (inside the consume loop from Task 3).
- Test: `service/Api/tests/Module.UnitTests/Inventory/Services/StockReservationServiceTests.cs` (extend a Task 3 test).

**Interfaces:**
- Consumes: `StockMovementMethod.Create(Guid stockItemId, int quantity, int? previousCountOnHand = null, string? originatorType = null, Guid? originatorId = null, string? reason = null, string? action = null, Guid? stockLocationId = null, string? createdBy = null)` → `Result<StockMovement>`; add to `dbContext.Set<StockMovement>()`. Requires `using Module.Inventory.Domain.StockMovements;` at the top of the implementation file.

- [ ] **Step 1: Write a failing test.**

Add a `StockMovement` assertion to a new test (don't mutate the Task 3 test that checks counts, so add a dedicated test):

```csharp
[Fact(DisplayName = "ConsumeForOrderAsync: creates a sold stock movement per picked reservation")]
public async Task ConsumeForOrderAsync_CreatesSoldMovement()
{
    var ct = TestContext.Current.CancellationToken;
    var item = await SeedStockItem(10);
    await SeedCartReservation(2, _orderId.ToString());

    var result = await _service.ConsumeForOrderAsync(
        _orderId, new List<StockConsumeLine> { new(_variantId, 2) }, ct);
    await _dbContext.SaveChangesAsync(ct);

    result.IsSuccess.Should().BeTrue();
    var movement = await _dbContext.Set<StockMovement>().SingleAsync(ct);
    movement.Quantity.Should().Be(-2);
    movement.PreviousCountOnHand.Should().Be(10);
    movement.Reason.Should().Be("sold");
    movement.OriginatorType.Should().Be("Order");
    movement.OriginatorId.Should().Be(_orderId);
}
```

- [ ] **Step 2: Run to verify fail.**

Run:
```bash
dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Inventory.Services.StockReservationServiceTests"
```
Expected: FAIL — `Failed: 1` (`SingleAsync` throws: no movement row exists).

- [ ] **Step 3: Add movement creation in the consume loop.**

Add `using Module.Inventory.Domain.StockMovements;` to `StockReservation.Service.Implementation.cs`, then inside the consume loop capture the previous count before `Pick` and add a movement after a successful `Pick`:

```csharp
    var previous = stockItem.CountOnHand;
    var pickResult = stockItem.Pick(take);
    if (pickResult.IsFailure)
        return pickResult.Errors;

    var movement = StockMovementMethod.Create(
        stockItemId: stockItem.Id,
        quantity: -take,
        previousCountOnHand: previous,
        originatorType: "Order",
        originatorId: orderId,
        reason: "sold");
    if (movement.IsSuccess)
        dbContext.Set<StockMovement>().Add(movement.Value);
```

- [ ] **Step 4: Run tests.**

Run:
```bash
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Inventory.Services.StockReservationServiceTests"
```
Expected: PASS (`Failed: 0`). `dotnet build` clean.

- [ ] **Step 5: Commit.**

```bash
git add service/Api/src/Module/Inventory/Services/StockReservations/StockReservation.Service.Implementation.cs \
        service/Api/tests/Module.UnitTests/Inventory/Services/StockReservationServiceTests.cs
git commit -m "feat(inventory): write sold stock movement on order consumption"
```

---

### Task 5: Register reservation expiry sweep (R5)

**Files:**
- Modify: `service/Api/src/Module/Inventory/Inventory.Extension.cs`.

**Interfaces:**
- Consumes: `Module.Inventory.Backgrounds.ReservationExpiryJob` (scoped) and `Module.Inventory.Backgrounds.ReservationExpiryJobScheduler : IHostedService`. Mirrors `Ordering.Extension.cs:20-21` and `Paying.Extension.cs:82`.

- [ ] **Step 1: Add registrations.**

In `Inventory.Extension.cs` inside `AddInventoryModule`, after the existing service registrations:

```csharp
builder.Services.AddScoped<Backgrounds.ReservationExpiryJob>();
builder.Services.AddHostedService<Backgrounds.ReservationExpiryJobScheduler>();
```

Add `using Module.Inventory.Backgrounds;` (or reference `Backgrounds.…` via the already-implied namespace — add the using to be safe).

- [ ] **Step 2: Build.**

Run: `dotnet build`
Expected: clean (0 warnings/0 errors).

- [ ] **Step 3: Commit.**

```bash
git add service/Api/src/Module/Inventory/Inventory.Extension.cs
git commit -m "fix(inventory): register reservation expiry scheduler"
```

---

### Task 6: Release by cartToken on checkout session expiry (R6)

**Files:**
- Modify: `service/Api/src/Module/Billing/Backgrounds/ProcessStripeWebhookEventJob.cs:418`.

**Interfaces:**
- Consumes: `ReleaseReservationsAsync(Guid? orderId = null, string? cartToken = null, CancellationToken)`. Cart reservations are keyed by `CartToken = orderId.ToString()` with `OrderId == null`.

- [ ] **Step 1: Change the call.**

Replace `ReleaseReservationsAsync(orderId: payment.OrderId, ct: ct)` with:

```csharp
var releaseResult = await _stockReservationService.ReleaseReservationsAsync(
    cartToken: payment.OrderId.ToString(), ct: ct);
```

- [ ] **Step 2: Build + cross-module check.**

Run: `dotnet build` and `bash scripts/check-cross-module-refs.sh`
Expected: build clean; baseline stays 46.

- [ ] **Step 3: Commit.**

```bash
git add service/Api/src/Module/Billing/Backgrounds/ProcessStripeWebhookEventJob.cs
git commit -m "fix(billing): release reservations by cart token on session expiry"
```

---

## Phase 2: Happy-Path & Branch-Coverage Unit Tests

> Phase 1 (Tasks 1–6) fixed the bugs. Phase 2 closes unit-test coverage gaps for the orchestration and module boundaries that currently have NO direct tests. All tests are written against the fixed code and must pass immediately (no TDD red step — the implementation already exists); each task is a `write test → run → pass → commit` cycle. Run tests via the native runner (see Global Constraints).

Coverage gaps identified by inspection:
- `CheckoutPlacementService` (Ordering) — no direct test; exercised only indirectly through `CreateOrderFromCartTests`.
- `CompleteCheckoutForPayment` (Ordering) — no test.
- `CreateShipmentCommandHandler` (Shipping) — no test.
- `ShipmentFulfillmentSyncService` (Shipping) — no test (only domain `Shipment.Fulfillment.Tests.cs` exists).
- `UpdateShipmentStatus` (Shipping) — no test.
- `ConsumeForOrderAsync` (Inventory) — no happy-path single-reservation test, no empty-lines test, no multi-location split test.
- `CreatePaymentIntent` (Billing) — no offline-COD test asserting reservation release, no release-before-reserve ordering assertion (the Task 2 test asserts the call exists but not its position).

---

### Task 7: CheckoutPlacementService happy-path + branch tests (Ordering)

**Files:**
- Create: `service/Api/tests/Module.UnitTests/Ordering/Services/CheckoutPlacementServiceTests.cs`

**Interfaces:**
- Consumes: `CheckoutPlacementService(IApplicationDbContext, IStockReservationService, INotificationService, ISender, ILogger<CheckoutPlacementService>)`; `PlaceAsync(Order cart, string actor, CancellationToken ct)`.
- Mocks: `IStockReservationService.ConsumeForOrderAsync`, `INotificationService.SendAsync`, `ISender.Send(CreateShipmentCommand)`.
- Domain seeding mirrors `CreateOrderFromCartTests.cs:88-107` (draft `Order` via `OrderMethod.Create("USD", userId, Guid.Empty)`, `CheckoutState=PickPaymentMethod`, `BillAddressId`/`ShipAddressId`/`ShippingMethodId`/`Email` set, one `LineItem` with `VariantId`/`Quantity`/`Price`/`Total`/`Currency`).

- [ ] **Step 1: Write the test class.** Cover these cases:

```csharp
// Happy path: consumes stock, places order, notifies, creates shipment
PlaceAsync_ShouldPlaceOrder_ConsumeStock_Notify_AndCreateShipment
// Branch: no ShippingMethodId → CreateShipmentCommand NOT sent
PlaceAsync_ShouldSkipShipment_WhenNoShippingMethod
// Branch: consume failure → order NOT placed, error returned, shipment not sent
PlaceAsync_ShouldNotPlace_WhenConsumeFails
```

Assertions for happy path: `result.IsSuccess` true; persisted order `Status == Placed`; `Number` starts with `"R"`; `_senderMock.Verify(CreateShipmentCommand, Times.Once)`; `_reservationServiceMock.Verify(ConsumeForOrderAsync(cart.Id, It.IsAny<IReadOnlyCollection<StockConsumeLine>>(), ...), Times.Once)`; `_notificationServiceMock.Verify(SendAsync, Times.Once)`. For the no-shipping-method branch: `_senderMock.Verify(CreateShipmentCommand, Times.Never)`. For consume-failure: mock `ConsumeForOrderAsync` returns `Result.Failure(StockReservationResult.Errors.InsufficientStock)`, assert `result.IsFailure`, persisted `Status == Draft`, `CreateShipmentCommand` never sent.

- [ ] **Step 2: Build + run.**

```bash
dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Ordering.Services.CheckoutPlacementServiceTests"
```
Expected: PASS (`Failed: 0`).

- [ ] **Step 3: Commit.**

```bash
git add service/Api/tests/Module.UnitTests/Ordering/Services/CheckoutPlacementServiceTests.cs
git commit -m "test(ordering): add CheckoutPlacementService happy-path and branch coverage"
```

---

### Task 8: CompleteCheckoutForPayment tests (Ordering)

**Files:**
- Create: `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/CompleteCheckoutForPayment/CompleteCheckoutForPaymentTests.cs`

**Interfaces:**
- Consumes: `CompleteCheckoutForPaymentCommandHandler(IApplicationDbContext, ISender, CheckoutPlacementService, ILogger<...>)`; `Handle(CompleteCheckoutForPaymentCommand, CancellationToken)`; `CompleteCheckoutForPaymentCommand { Guid CartId; Guid PaymentId; }`.
- The handler re-verifies payment via `ISender.Send(GetPaymentForCheckoutQuery)` (mock it to return `IsCompleted=true`), then calls `placementService.PlaceAsync`. Use a real `CheckoutPlacementService` with mocked `IStockReservationService`/`INotificationService`/`ISender`, or mock `CheckoutPlacementService` directly if it can be constructed (it has no interface; construct it with real deps as in Task 7).

- [ ] **Step 1: Write the test class.** Cover:

```csharp
// Happy path: draft cart + completed payment → placed, Placed=true
Handle_ShouldPlaceOrder_WhenPaymentCompleted
// Branch: non-draft cart → idempotent no-op, Placed=false, no consume
Handle_ShouldReturnPlacedFalse_WhenCartNotDraft
// Branch: payment not completed → PaymentNotCompleted error
Handle_ShouldReturnPaymentNotCompleted_WhenPaymentNotCompleted
```

Assertions: happy path — `result.Value.Placed` true; persisted `Status == Placed`; `_reservationServiceMock.Verify(ConsumeForOrderAsync, Times.Once)`. Non-draft — `Placed` false; `ConsumeForOrderAsync` never called. Not-completed — `result.IsFailure`, `result.Errors[0].Code == "Order.Payment.NotCompleted"` (verify the actual code in `OrderResult.Errors.PaymentNotCompleted`).

- [ ] **Step 2: Build + run.**

```bash
./Module.UnitTests -class "Module.UnitTests.Ordering.Features.Storefront.CompleteCheckoutForPayment.CompleteCheckoutForPaymentTests"
```
Expected: PASS.

- [ ] **Step 3: Commit.**

```bash
git add service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/CompleteCheckoutForPayment/CompleteCheckoutForPaymentTests.cs
git commit -m "test(ordering): add CompleteCheckoutForPayment idempotency and branch coverage"
```

---

### Task 9: CreateShipment handler tests (Shipping)

**Files:**
- Create: `service/Api/tests/Module.UnitTests/Shipping/Features/Shared/Commands/CreateShipmentTests.cs`

**Interfaces:**
- Consumes: `CreateShipmentCommandHandler(IApplicationDbContext)`; `CreateShipmentCommand { Guid OrderId; Guid ShippingMethodId; }`.
- Seeding: add `Shipment` via `ShipmentMethod.Create(orderId, shippingMethodId)`; use `AdditionalConfigurationsAssemblies = [typeof(Shipment).Assembly]`.

- [ ] **Step 1: Write the test class.** Cover:

```csharp
// Happy path: creates a Pending shipment for order+method
Handle_ShouldCreatePendingShipment
// Branch: idempotent — existing shipment for order+method → no duplicate
Handle_ShouldNotCreateDuplicate_WhenShipmentExists
```

Assertions: happy path — one `Shipment` persisted with `Status == Pending`, `OrderId`, `ShippingMethodId`. Idempotent — after seeding one shipment and calling again, `_dbContext.Set<Shipment>().Count() == 1` and result `IsSuccess`.

- [ ] **Step 2: Build + run.**

```bash
./Module.UnitTests -class "Module.UnitTests.Shipping.Features.Shared.Commands.CreateShipmentTests"
```
Expected: PASS.

- [ ] **Step 3: Commit.**

```bash
git add service/Api/tests/Module.UnitTests/Shipping/Features/Shared/Commands/CreateShipmentTests.cs
git commit -m "test(shipping): add CreateShipment happy-path and idempotency coverage"
```

---

### Task 10: ShipmentFulfillmentSyncService + UpdateShipmentStatus tests (Shipping)

**Files:**
- Create: `service/Api/tests/Module.UnitTests/Shipping/Services/ShipmentFulfillmentSyncServiceTests.cs`
- Create: `service/Api/tests/Module.UnitTests/Shipping/Features/Admin/Shipments/UpdateStatus/UpdateShipmentStatusTests.cs`

**Interfaces:**
- `ShipmentFulfillmentSyncService(IApplicationDbContext, ISender, ILogger<...>)`; `SyncOrderFulfillmentAsync(Guid orderId, CancellationToken ct)`. Sends `RecordOrderShipmentStateCommand { OrderId, FulfillmentState, ShippedAtUtc, DeliveredAtUtc }` via `ISender`.
- `UpdateShipmentStatus.CommandHandler(IApplicationDbContext, ShipmentFulfillmentSyncService)`; `Command(Guid Id, Request Request)`; `Request : ShipmentStatusParameters { ShipmentStatus Status; string? TrackingNumber; }`.

- [ ] **Step 1: Write `ShipmentFulfillmentSyncServiceTests.cs`.** Cover:

```csharp
// Happy path: one Shipped shipment → sends RecordOrderShipmentStateCommand with FulfillmentState=Shipped
SyncOrderFulfillmentAsync_ShouldSendShippedState_WhenOneShippedShipment
// Branch: no shipments → sends FulfillmentState=None
SyncOrderFulfillmentAsync_ShouldSendNone_WhenNoShipments
// Branch: sender failure → logged, not thrown
SyncOrderFulfillmentAsync_ShouldNotThrow_WhenSenderFails
```

Seed a `Shipment` with `Status = Shipped` and `ShippedAtUtc` set (set properties directly on a `ShipmentMethod.Create(...)` result). Assert `_senderMock.Verify(RecordOrderShipmentStateCommand with FulfillmentState==Shipped, Times.Once)`. For None: no shipments seeded → `FulfillmentState == ShipmentState.None`. For failure: `_senderMock` returns `Result.Failure(...)` → method completes without throwing.

- [ ] **Step 2: Write `UpdateShipmentStatusTests.cs`.** Cover:

```csharp
// Happy path: Pending → Shipped with tracking number, order synced
Handle_ShouldMarkShipped_AndSyncOrder
// Branch: invalid transition (Pending → Delivered) → error
Handle_ShouldReturnInvalidTransition_WhenSkippingStates
// Branch: not found → error
Handle_ShouldReturnNotFound_WhenShipmentMissing
```

Assertions: happy path — persisted `Status == Shipped`, `TrackingNumber` set; `_senderMock.Verify(RecordOrderShipmentStateCommand, Times.Once)`. Invalid — `result.IsFailure`, `result.Errors[0].Code == "Shipment.InvalidStateTransition"`. Not found — `result.Errors[0].Code == "Shipment.NotFound"`.

- [ ] **Step 3: Build + run both classes.**

```bash
./Module.UnitTests -class "Module.UnitTests.Shipping.Services.ShipmentFulfillmentSyncServiceTests"
./Module.UnitTests -class "Module.UnitTests.Shipping.Features.Admin.Shipments.UpdateStatus.UpdateShipmentStatusTests"
```
Expected: PASS (both).

- [ ] **Step 4: Commit.**

```bash
git add service/Api/tests/Module.UnitTests/Shipping/Services/ShipmentFulfillmentSyncServiceTests.cs \
        service/Api/tests/Module.UnitTests/Shipping/Features/Admin/Shipments/UpdateStatus/UpdateShipmentStatusTests.cs
git commit -m "test(shipping): add fulfillment-sync and shipment-status branch coverage"
```

---

### Task 11: ConsumeForOrderAsync happy-path + branch tests (Inventory)

**Files:**
- Modify: `service/Api/tests/Module.UnitTests/Inventory/Services/StockReservationServiceTests.cs` (add to existing `#region ConsumeForOrderAsync`).

**Interfaces:**
- `ConsumeForOrderAsync(Guid orderId, IReadOnlyCollection<StockConsumeLine> lines, CancellationToken)` — existing helpers `SeedStockItem(int)`, `SeedCartReservation(int, string?)` and fields `_variantId`, `_stockLocationId`, `_orderId` are in scope.

- [ ] **Step 1: Add tests.** Cover:

```csharp
// Happy path: single reservation exactly matches line → fully Fulfilled, on-hand reduced
ConsumeForOrderAsync_ConsumesExactReservation_AndMarksFulfilled
// Branch: empty lines → Ok no-op, no changes
ConsumeForOrderAsync_EmptyLines_ReturnsOkNoop
// Branch: reservation split across two locations → both consumed, one movement each
ConsumeForOrderAsync_SplitsAcrossLocations
```

Assertions: happy path — seed `SeedStockItem(10)` + `SeedCartReservation(3, _orderId.ToString())`, consume `[new(_variantId, 3)]` → `CountOnHand == 7`, one `Fulfilled` reservation, one `StockMovement` (`Quantity == -3`). Empty — `[ ]` lines → `IsSuccess`, `CountOnHand` unchanged, no movement. Split — seed two stock items (two locations) and two reservations of 2+1 via `StockReservationMethod.SeedForTest` (matching the existing helper pattern), consume `[new(_variantId, 3)]` → total `CountOnHand` reduced by 3, two `Fulfilled` reservations, two movements (quantities -2 and -1).

- [ ] **Step 2: Build + run.**

```bash
./Module.UnitTests -class "Module.UnitTests.Inventory.Services.StockReservationServiceTests"
```
Expected: PASS (`Failed: 0`; count increases from 31).

- [ ] **Step 3: Commit.**

```bash
git add service/Api/tests/Module.UnitTests/Inventory/Services/StockReservationServiceTests.cs
git commit -m "test(inventory): add consume happy-path, empty, and multi-location coverage"
```

---

### Task 12: CreatePaymentIntent offline + compensation branch tests (Billing)

**Files:**
- Modify: `service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs`

**Interfaces:**
- `CreatePaymentIntent.CommandHandler(IApplicationDbContext, ICurrentUser, IGatewayRegistry, IStockReservationService, ISender, ILogger<...>)`. Existing mocks in the fixture (`_reservationServiceMock`, `_gatewayMock`, `_gatewayRegistryMock`, `_senderMock`). The COD test already exists (`Handle_CodMethod_CreatesPendingPayment_NoGateway`).

- [ ] **Step 1: Add tests.** Cover:

```csharp
// Branch: offline COD still releases prior reservations before reserving
Handle_CodMethod_ReleasesPriorReservations_BeforeReserving
// Branch: reservation failure releases already-reserved lines (compensation)
Handle_ReserveFailure_ReleasesReservations
```

Assertions: COD release — set up a cart in `PickDeliveryMethod` state with a non-empty `LineItems`, run with a COD `PaymentMethod`; assert `_reservationServiceMock.Verify(ReleaseCartReservationsAsync(order.Id.ToString(), null, ...), Times.Once)` AND `ReserveForVariantAsync` called once per line. Compensation — mock `ReserveForVariantAsync` to return `InsufficientStock`; assert `ReleaseCartReservationsAsync` is called (the failure compensation at `CreatePaymentIntent.cs:86-87`).

- [ ] **Step 2: Build + run.**

```bash
./Module.UnitTests -class "Module.UnitTests.Payment.Features.Storefront.Payment.CreateIntent.CreatePaymentIntentTests"
```
Expected: PASS (`Failed: 0`; count increases from 5).

- [ ] **Step 3: Commit.**

```bash
git add service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs
git commit -m "test(billing): add CreatePaymentIntent offline and compensation branch coverage"
```

---

## Self-Review

- **Spec coverage (Phase 2):** T7 → `CheckoutPlacementService` orchestration; T8 → `CompleteCheckoutForPayment` webhook-placement idempotency; T9 → `CreateShipment` idempotency; T10 → shipping fulfillment sync + status transitions; T11 → consume happy-path/empty/multi-location; T12 → CreatePaymentIntent offline/compensation. Together these give each module in the end-to-end flow a direct happy-path + branch test.
- **Placeholder scan:** no TBD/TODO; concrete assertions and class names in every step.
- **Type consistency:** `StockConsumeLine(Guid VariantId, int Quantity)`; `RecordOrderShipmentStateCommand` fields match `RecordOrderShipmentState.Command.cs`; `ShipmentStatusParameters { Status, TrackingNumber }`; `ShipmentResult.Errors` codes (`Shipment.NotFound`, `Shipment.InvalidStateTransition`, `Shipment.OrderId.Required`, `Shipment.ShippingMethod.Required`) verified against `Shipment.Result.cs`.

- **Spec coverage:** R1→T3, R2→T2, R3→T1, R4→T4, R5→T5, R6→T6, R7→T3 (unordered-variant release test), R8→deferred (NG1/O1). All P0 covered.
- **Placeholder scan:** no TBD/TODO; concrete code in every implement step.
- **Type consistency:** `StockConsumeLine(Guid VariantId, int Quantity)` matches `StockReservation.Service.Interface.cs:6`; `StockMovementMethod.Create` parameter order matches `StockMovement.Method.cs:20-29`; `ReleaseCartReservationsAsync(string, Guid?, CancellationToken)` vs `ReleaseReservationsAsync(Guid?, string?, CancellationToken)` overloads distinguished; `SeedCartReservation(quantity, cartToken)` helper exists at `StockReservationServiceTests.cs:60-68`; `SeedForTest` signature verified at `StockReservation.Method.cs:45-55`.
- **Test-fixture gap:** `CreatePaymentIntentTests` constructor does not yet mock `ReleaseCartReservationsAsync` — Task 2 Step 1 notes the required constructor setup.

## Phase 2 Completion Record

| Task | Status | Commits | New tests |
|------|--------|---------|-----------|
| T7 — CheckoutPlacementService (Ordering) | ✅ | `fdd9c26aa` | 3 |
| T8 — CompleteCheckoutForPayment (Ordering) | ✅ | `0bc00e41a` | 3 |
| T9 — CreateShipment (Shipping) | ✅ | `e79fd46a1` | 2 |
| T10 — ShipmentFulfillmentSyncService + UpdateShipmentStatus (Shipping) | ✅ | `0e07bd703` | 3 + 3 |
| T11 — ConsumeForOrderAsync (Inventory) | ✅ | `9a9fbaf40` | 3 |
| T12 — CreatePaymentIntent (Billing) | ✅ | `fb8231954` | 2 |

Verification: full `Module.UnitTests` suite = **2744 tests** (was 2721 at baseline), `Failed: 3` — the 3 failures are the pre-existing `OrderStatusValueConverterTests` WIP NREs (unchanged, unrelated). Build clean (0 warnings / 0 errors).

Note on T7: the plan's original "skip shipment when no shipping method" branch was re-scoped to "reject placement when shipping method missing" — `ValidateCheckoutPrerequisites` (`Order.Method.Checkout.cs:117`) requires `ShippingMethodId`, so the defensive `if (cart.ShippingMethodId.HasValue)` branch in `PlaceAsync` is unreachable for a placeable cart. The test asserts the actual behavior (`Order.DeliveryMethodRequired` error, `Draft` unchanged).

## Execution Handoff

Plan complete and saved to `plan/fix-payment-inventory-correctness-1.md`. Two execution options:

1. **Subagent-Driven (recommended)** — dispatch a fresh subagent per task, review between tasks.
2. **Inline Execution** — execute tasks in this session with checkpoints.

Which approach?
