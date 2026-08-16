# Fix Payment-Success → Inventory Correctness — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: use `superpowers:subagent-driven-development` (recommended) or `superpowers:executing-plans`. Steps use `- [ ]` checkbox syntax.

**Goal:** Fix double-deduction, missing sale stock-movement, and release/expiry stock inflation in the payment-success → inventory flow.

**Architecture:** Fix the reservation lifecycle at its two boundaries (create-intent releases the prior set; consume caps at the ordered quantity and releases surplus), remove erroneous `CountOnHand` mutations on release/expiry, add the sale `StockMovement` write, and wire up the expiry sweep + session-expiry release. Availability remains a derived value (`CountOnHand − Σ activeReserved`); no stored `Reserved`/`Available` columns are introduced.

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

## Self-Review

- **Spec coverage:** R1→T3, R2→T2, R3→T1, R4→T4, R5→T5, R6→T6, R7→T3 (unordered-variant release test), R8→deferred (NG1/O1). All P0 covered.
- **Placeholder scan:** no TBD/TODO; concrete code in every implement step.
- **Type consistency:** `StockConsumeLine(Guid VariantId, int Quantity)` matches `StockReservation.Service.Interface.cs:6`; `StockMovementMethod.Create` parameter order matches `StockMovement.Method.cs:20-29`; `ReleaseCartReservationsAsync(string, Guid?, CancellationToken)` vs `ReleaseReservationsAsync(Guid?, string?, CancellationToken)` overloads distinguished; `SeedCartReservation(quantity, cartToken)` helper exists at `StockReservationServiceTests.cs:60-68`; `SeedForTest` signature verified at `StockReservation.Method.cs:45-55`.
- **Test-fixture gap:** `CreatePaymentIntentTests` constructor does not yet mock `ReleaseCartReservationsAsync` — Task 2 Step 1 notes the required constructor setup.

## Execution Handoff

Plan complete and saved to `plan/fix-payment-inventory-correctness-1.md`. Two execution options:

1. **Subagent-Driven (recommended)** — dispatch a fresh subagent per task, review between tasks.
2. **Inline Execution** — execute tasks in this session with checkpoints.

Which approach?
