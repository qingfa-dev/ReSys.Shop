# Inventory Services Consolidation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Delete 3 redundant Inventory CQRS handler directories (10 files), extend `IStockReservationService` with `ConsumeForOrderAsync` and `ReleaseReservationAsync`, extend `IStockItemService` with `GetAvailabilityForCartAsync`, create `CartTokenMiddleware`, switch Ordering and Billing modules to direct service injection, create 4 inventory storefront REST endpoints, delete `OrderInventoryService.cs`.

**Architecture:** Inventory module already has `IStockReservationService` and `IStockItemService` with proper `Result<T>` contracts and `Serializable` isolation. The CQRS handlers duplicate their logic. After this plan: services are the single source of inventory operations. Storefront endpoints are thin wrappers (Endpoint → Service → Result, no MediatR handler). Ordering and Billing inject services directly.

**Tech Stack:** .NET 10, C#, EF Core + Npgsql, Carter minimal APIs, FluentValidation, Result<T> pattern, System.Data.IsolationLevel.Serializable

## Global Constraints

- .NET 10, TreatWarningsAsErrors=true (zero warnings)
- All service methods return `Result<T>` with domain error codes from `StockReservationResult.Errors` / `StockItemResult.Errors`
- All new storefront endpoints use `ICarterModule` pattern, `.Produces<Result<T>>()`, no `.HasPermission()` (public)
- `CartTokenMiddleware` reads `X-Cart-Token` header → `HttpContext.Items["CartToken"]`
- No `using Module.Inventory.Domain.*` in Ordering/Billing files after migration
- `dotnet build` must pass with zero warnings after each task
- Feature files follow vertical slice convention: `{ActionName}.Endpoint.cs`, `.Validator.cs`, `.Request.cs` (Handler.cs omitted for thin endpoints)

---

### Task 1: Delete 3 CQRS Handler Directories (10 files)

**Files:**
- Delete: `service/Api/src/Module/Inventory/Features/Storefront/StockReservations/ReserveCart/ReserveCartStock.cs`
- Delete: `service/Api/src/Module/Inventory/Features/Storefront/StockReservations/ReserveCart/ReserveCartStock.Request.cs`
- Delete: `service/Api/src/Module/Inventory/Features/Storefront/StockReservations/ReserveCart/ReserveCartStock.Response.cs`
- Delete: `service/Api/src/Module/Inventory/Features/Storefront/StockReservations/ReserveCart/ReserveCartStock.Validator.cs`
- Delete: `service/Api/src/Module/Inventory/Features/Storefront/StockReservations/ConsumeCart/ConsumeCartStockReservations.cs`
- Delete: `service/Api/src/Module/Inventory/Features/Storefront/StockReservations/ConsumeCart/ConsumeCartStockReservations.Request.cs`
- Delete: `service/Api/src/Module/Inventory/Features/Storefront/StockReservations/ConsumeCart/ConsumeCartStockReservations.Validator.cs`
- Delete: `service/Api/src/Module/Inventory/Features/Storefront/StockReservations/ReleaseCart/ReleaseCartStockReservations.cs`
- Delete: `service/Api/src/Module/Inventory/Features/Storefront/StockReservations/ReleaseCart/ReleaseCartStockReservations.Request.cs`
- Delete: `service/Api/src/Module/Inventory/Features/Storefront/StockReservations/ReleaseCart/ReleaseCartStockReservations.Validator.cs`

**Produces:** Empty directories — removed in next step.

- [ ] **Step 1: Delete directories**

```bash
rm -rf service/Api/src/Module/Inventory/Features/Storefront/StockReservations/ReserveCart/
rm -rf service/Api/src/Module/Inventory/Features/Storefront/StockReservations/ConsumeCart/
rm -rf service/Api/src/Module/Inventory/Features/Storefront/StockReservations/ReleaseCart/
```

- [ ] **Step 2: Build — expect build failures from missing namespace references**

```bash
dotnet build 2>&1 | head -50
```

Build WILL fail — Ordering and Billing modules reference deleted types. These are fixed in Tasks 7-10.

- [ ] **Step 3: Commit**

```bash
git add -A service/Api/src/Module/Inventory/Features/Storefront/StockReservations/
git commit -m "refactor(inventory): delete redundant CQRS handler directories

Remove ReserveCart/, ConsumeCart/, ReleaseCart/ (10 files).
These duplicated logic already in IStockReservationService.
Build temporarily broken — Ordering/Billing consumers fixed in next tasks."
```

### Task 2: Delete OrderInventoryService.cs and Empty Model File

**Files:**
- Delete: `service/Api/src/Module/Ordering/Services/OrderInventoryService.cs`
- Delete: `service/Api/src/Module/Inventory/Features/Storefront/Shared/Models/Store.StockItem.Model.cs`

- [ ] **Step 1: Delete files**

```bash
rm service/Api/src/Module/Ordering/Services/OrderInventoryService.cs
rm service/Api/src/Module/Inventory/Features/Storefront/Shared/Models/Store.StockItem.Model.cs
```

- [ ] **Step 2: Commit**

```bash
git add -A
git commit -m "refactor: delete OrderInventoryService and empty Store.StockItem.Model.cs

OrderInventoryService replaced by IStockItemService.AdjustStockAsync.
Empty model file pruned."
```

### Task 3: Add ConsumeForOrderAsync to IStockReservationService

**Files:**
- Modify: `service/Api/src/Module/Inventory/Services/StockReservations/StockReservation.Service.Interface.cs`
- Modify: `service/Api/src/Module/Inventory/Services/StockReservations/StockReservation.Service.Implementation.cs`

**Interfaces:**
- Produces: `Task<Result> ConsumeForOrderAsync(Guid orderId, CancellationToken ct = default)`
- **DECISION (pre-flight review):** Reservations are keyed by `CartToken` (= cart.Id.ToString()), NOT by `OrderId`. The caller passes `cart.Id` (the draft order id) which IS the cart token value. Implementation MUST match on `CartToken == orderId.ToString()` — never on `OrderId` (all rows have `OrderId = null`).

- [ ] **Step 1: Add interface method**

In `StockReservation.Service.Interface.cs`, after `ReleaseReservationsAsync`:

```csharp
Task<Result> ConsumeForOrderAsync(Guid orderId, CancellationToken ct = default);
```

- [ ] **Step 2: Add implementation**

In `StockReservation.Service.Implementation.cs`, add method before the closing brace of the class. **Match by CartToken, not OrderId** — this mirrors the deleted `ConsumeCartStockReservations` handler exactly:

```csharp
public async Task<Result> ConsumeForOrderAsync(Guid orderId, CancellationToken ct = default)
{
    var reservations = await dbContext.Set<StockReservation>()
        .Where(r => r.CartToken == orderId.ToString()
                    && r.State == ReservationState.Reserved)
        .ToListAsync(ct);

    if (reservations.Count == 0)
        return StockReservationResult.Errors.NoActiveReservations;

    foreach (var reservation in reservations)
    {
        var stockItem = await dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(
                si => si.VariantId == reservation.VariantId
                      && si.StockLocationId == reservation.StockLocationId,
                ct);

        if (stockItem is null)
            return StockReservationResult.Errors.StockItemNotFound(reservation.VariantId);

        var pickResult = stockItem.Pick(reservation.Quantity);
        if (pickResult.IsFailure)
            return pickResult.Errors;

        reservation.State = ReservationState.Fulfilled;
        reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;
    }

    await dbContext.SaveChangesAsync(ct);
    return Result.Ok();
}
```

- [ ] **Step 3: Build**

```bash
dotnet build
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Inventory/Services/StockReservations/
git commit -m "feat(inventory): add ConsumeForOrderAsync to IStockReservationService

Transitions all Reserved-state reservations for an order to Fulfilled,
picking stock via StockItem.Pick() domain method.
Logically copies the deleted ConsumeCartStockReservations handler."
```

### Task 4: Add ReleaseReservationAsync to IStockReservationService

**Files:**
- Modify: `service/Api/src/Module/Inventory/Services/StockReservations/StockReservation.Service.Interface.cs`
- Modify: `service/Api/src/Module/Inventory/Services/StockReservations/StockReservation.Service.Implementation.cs`

**Interfaces:**
- Produces: `Task<Result> ReleaseReservationAsync(Guid reservationId, CancellationToken ct = default)`

- [ ] **Step 1: Add interface method**

In `StockReservation.Service.Interface.cs`, after `ConsumeForOrderAsync`:

```csharp
Task<Result> ReleaseReservationAsync(Guid reservationId, CancellationToken ct = default);
```

- [ ] **Step 2: Add implementation**

In `StockReservation.Service.Implementation.cs`. **NOTE:** `StockReservationResult.Errors.NotFound` takes a `Guid` argument (`NotFound(reservationId)`), and `InvalidStateTransition` is a parameterless property (not a method) — use them accordingly:

```csharp
public async Task<Result> ReleaseReservationAsync(Guid reservationId, CancellationToken ct = default)
{
    var reservation = await dbContext.Set<StockReservation>()
        .FirstOrDefaultAsync(r => r.Id == reservationId, ct);

    if (reservation is null)
        return StockReservationResult.Errors.NotFound(reservationId);

    if (reservation.State != ReservationState.Reserved)
        return StockReservationResult.Errors.InvalidStateTransition;

    var releaseResult = reservation.Release();
    if (releaseResult.IsFailure)
        return releaseResult.Errors;

    reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;
    await dbContext.SaveChangesAsync(ct);
    return Result.Ok();
}
```

- [ ] **Step 3: Build**

```bash
dotnet build
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Inventory/Services/StockReservations/
git commit -m "feat(inventory): add ReleaseReservationAsync to IStockReservationService

Releases a single reservation by ID. Validates it is in Reserved state
before calling reservation.Release() domain method."
```

### Task 5: Add GetAvailabilityForCartAsync to IStockItemService

**Files:**
- Modify: `service/Api/src/Module/Inventory/Services/StockItems/StockItem.Service.Interface.cs`
- Modify: `service/Api/src/Module/Inventory/Services/StockItems/StockItem.Service.Implementation.cs`

**Interfaces:**
- Produces: `Task<Result<VariantStockAvailability>> GetAvailabilityForCartAsync(Guid variantId, string? cartToken, CancellationToken ct = default)`
- Existing type: `VariantStockAvailability` from `Module.Inventory.Services` already has `VariantId`, `TotalOnHand`, `TotalReserved`, `TotalAvailable`, `Backorderable`, `Locations` (IReadOnlyList<LocationStockSnapshot>)

- [ ] **Step 1: Add interface method**

In `StockItem.Service.Interface.cs`, after `GetStockAvailabilityAsync`:

```csharp
Task<Result<VariantStockAvailability>> GetAvailabilityForCartAsync(
    Guid variantId, string? cartToken, CancellationToken ct = default);
```

- [ ] **Step 2: Add implementation**

In `StockItem.Service.Implementation.cs`. **NOTE (pre-flight review):** `LocationStockSnapshot` uses `StockLocationName`, `ReservedCount`, `AvailableCount`, `Active` — NOT `LocationName`/`Reserved`/`Available`/`IsAvailable`. Do NOT add new properties to the record; use the existing ones:

```csharp
public async Task<Result<VariantStockAvailability>> GetAvailabilityForCartAsync(
    Guid variantId, string? cartToken, CancellationToken ct = default)
{
    var stockItems = await dbContext.Set<StockItem>()
        .Where(si => si.VariantId == variantId)
        .ToListAsync(ct);

    var locations = new List<LocationStockSnapshot>();
    var totalOnHand = 0;
    var totalReserved = 0;
    var backorderable = false;

    foreach (var item in stockItems)
    {
        var reservedQuery = dbContext.Set<StockReservation>()
            .Where(r => r.VariantId == variantId
                        && r.StockLocationId == item.StockLocationId
                        && r.State == ReservationState.Reserved
                        && r.ExpiresAtUtc > DateTimeOffset.UtcNow);

        // Exclude this cart's own reservations from the "reserved" count
        if (!string.IsNullOrWhiteSpace(cartToken))
            reservedQuery = reservedQuery.Where(r => r.CartToken != cartToken);

        var reserved = await reservedQuery.SumAsync(r => r.Quantity, ct);

        totalOnHand += item.CountOnHand;
        totalReserved += reserved;
        if (item.Backorderable) backorderable = true;

        locations.Add(new LocationStockSnapshot
        {
            StockLocationId = item.StockLocationId,
            StockLocationName = string.Empty,
            CountOnHand = item.CountOnHand,
            ReservedCount = reserved,
            AvailableCount = item.CountOnHand - reserved,
            Backorderable = item.Backorderable,
            Active = item.CountOnHand - reserved > 0 || item.Backorderable
        });
    }

    return new VariantStockAvailability
    {
        VariantId = variantId,
        TotalOnHand = totalOnHand,
        TotalReserved = totalReserved,
        TotalAvailable = totalOnHand - totalReserved,
        Backorderable = backorderable,
        Locations = locations
    };
}
```

- [ ] **Step 3: Build + verify property names compile**

```bash
dotnet build
```

Verify `LocationStockSnapshot` (defined in `StockSnapshot.cs`, NOT `VariantStockAvailability.cs`) has the properties used above.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Inventory/Services/StockItems/
git commit -m "feat(inventory): add GetAvailabilityForCartAsync to IStockItemService

Returns per-location stock availability with the cart's own reservations
excluded from the reserved count. Used by GET /inventory/stock-items/{id}/availability."
```

### Task 6: Create CartTokenMiddleware

**Files:**
- Create: `service/Api/src/Shared/Security/Cart/CartTokenMiddleware.cs`
- Modify: `service/Api/src/Shared/Security/Security.Extension.cs`

**Interfaces:**
- Produces: ASP.NET middleware setting `HttpContext.Items["CartToken"]` = string | null
- Registration: inside `SecurityExtension.UseSecurity()`, BEFORE `app.UseApplicationAuthentication()`

- [ ] **Step 1: Create middleware**

```csharp
// Shared/Security/Cart/CartTokenMiddleware.cs
namespace Shared.Security.Cart;

public sealed class CartTokenMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Cart-Token", out var values))
        {
            context.Items["CartToken"] = values.FirstOrDefault();
        }
        await next(context);
    }
}
```

- [ ] **Step 2: Register in SecurityExtension.UseSecurity()**

**NOTE (pre-flight review):** `Program.cs` does NOT call `app.UseAuthentication()` inline — the pipeline is abstracted into `UseSecurity()` in `service/Api/src/Shared/Security/Security.Extension.cs`, which calls `UseApplicationAuthentication()` → `UseAuthentication()`. Register the middleware there, before `UseApplicationAuthentication()`:

```csharp
public static WebApplication UseSecurity(this WebApplication app)
{
    app.UseSecurityHeaders();
    app.UseSecurityCors();
    app.UseMiddleware<Shared.Security.Cart.CartTokenMiddleware>();
    app.UseApplicationAuthentication();
    app.UseApplicationAuthorization();
    return app;
}
```

- [ ] **Step 3: Build**

```bash
dotnet build
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Shared/Security/Cart/ service/Api/src/Shared/Security/Security.Extension.cs
git commit -m "feat(security): add CartTokenMiddleware

Extracts X-Cart-Token header into HttpContext.Items['CartToken'].
Registered in UseSecurity before UseAuthentication so both guest
(cart token) and auth (JWT) requests can access the cart identifier."
```

### Task 7: Update Ordering.AddToCart to Use IStockReservationService

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs`

**Consumes:** `IStockReservationService.ReserveForVariantAsync` (added in this task)
**Produces:** No MediatR calls to Inventory module; no `Module.Inventory.Domain.*` reference in Ordering
**DECISION (pre-flight review):** Add a new batch/multi-location service method `ReserveForVariantAsync` to `IStockReservationService` that ports the deleted `ReserveCartStock` handler's location-picking logic internally. This preserves multi-location splitting and avoids any cross-module domain reference from Ordering.

- [ ] **Step 1: Add ReserveForVariantAsync to IStockReservationService**

In `StockReservation.Service.Interface.cs`:

```csharp
Task<Result<StockReservation>> ReserveForVariantAsync(
    Guid variantId, int quantity, string? cartToken = null,
    int ttlMinutes = 30, CancellationToken ct = default);
```

In `StockReservation.Service.Implementation.cs`, add the implementation. Port the deleted `ReserveCartStock` handler logic (loop locations ordered by `CountOnHand` descending, split `remaining` across them, `StockReservationMethod.Reserve(...)` per location with `cartToken`), using `IsolationLevel.RepeatableRead` transaction. Do NOT reference `Module.Inventory.Domain.*` from Ordering — this lives inside the Inventory module so the domain reference is fine here.

- [ ] **Step 2: Replace MediatR reserve with service call**

In `AddToCart.cs`:

Remove imports:
```csharp
using Module.Inventory.Features.Storefront.Shared.Models;
using Module.Inventory.Features.Storefront.StockReservations.ReserveCart;
```
**KEEP** `using Module.Inventory.Features.Shared;` — AddToCart uses `InventoryFeature.Storefront.StockReservations.TtlMinutesDefault` from it (the current code already imports it for this). This is a feature-constants namespace (not a domain reference); the cross-module script's baseline tolerates the existing count.

Add import:
```csharp
using Module.Inventory.Services;
using Module.Inventory.Services.StockReservations;
```

Add constructor parameter:
```csharp
IStockReservationService stockReservationService,
```

**Also remove the now-dead `ISender sender` constructor parameter** — the `sender.Send(new ReserveCartStock.Command(...))` call was the handler's ONLY use of `ISender`. The handler no longer dispatches any MediatR command. Remove `ISender sender,` from the constructor (and verify no other `sender.` usage remains in the file).

Replace lines 70-82 (the `sender.Send(new ReserveCartStock.Command(...))` block):

```csharp
// Reserve: Delegate stock reservation to Inventory service.
// The service picks the best location(s) with available stock internally
// and splits the quantity across them when needed.
var reserveResult = await stockReservationService.ReserveForVariantAsync(
    variantId: request.VariantId,
    quantity: request.Quantity,
    cartToken: cart.Id.ToString(),
    ttlMinutes: InventoryFeature.Storefront.StockReservations.TtlMinutesDefault,
    ct: cancellationToken);

if (reserveResult.IsFailure)
    return reserveResult.Errors;
```

**Note:** `InventoryFeature.Storefront.StockReservations.TtlMinutesDefault` constant remains available via the retained `using Module.Inventory.Features.Shared;` import (constant defined in `InventoryFeature.Storefront.cs` — Task 14 rewrites that file and keeps the constant). Do NOT reference `Module.Inventory.Domain.*` in this file.

The cart ID is used as cart token here since the handler creates/uses a cart entity with `Id`. The `IHttpContextAccessor` pattern (reading from `HttpContext.Items["CartToken"]`) is for endpoints that receive the token from the browser header. Internal handlers that already have the cart entity use the cart ID directly.

- [ ] **Step 2: Update AddToCart unit tests (plan amendment — pre-flight review found the deleted CQRS types are referenced by 7 test files)**

Three test files mock/verify `sender.Send(new ReserveCartStock.Command(...))` from AddToCart. They must be updated to mock `IStockReservationService.ReserveForVariantAsync` instead:

- `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/AddItem/AddToCartTests.cs`
- `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/AddItem/AddToCartDefaultsTests.cs`
- `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/AddItem/AddToCart.Reservation.Tests.cs`

Pattern: replace the `sender.Setup(...).ReturnsAsync(Result<ReserveCartStock.Response>.Ok(...))` mock with an `IStockReservationService` mock:

```csharp
// BEFORE
sender.Setup(s => s.Send(It.IsAny<IRequest<Result<ReserveCartStock.Response>>>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(Result<ReserveCartStock.Response>.Ok(new ReserveCartStock.Response()));

// AFTER
var reservationService = new Mock<IStockReservationService>();
reservationService.Setup(s => s.ReserveForVariantAsync(
        It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(Result<StockReservation>.Ok(StockReservationMethod.Reserve(
        Guid.NewGuid(), 1, Guid.NewGuid(), null, 15, cartToken: "test")));
```

And replace any `Verify(s => s.Send(...ReserveCartStock.Command...))` with a `Verify(s => s.ReserveForVariantAsync(...))` on the service mock. The handler's constructor gains `IStockReservationService` — pass the mock in the handler construction within tests.

Run tests:
```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~AddToCart"
```

Note: `Module.UnitTests` may not compile until Tasks 8-10 fix the remaining handler references. If the test project won't build yet, verify by building the test project and confirm the ONLY remaining test-compile errors are in the other 4 test files (CreateOrderFromCart ×3, CreatePaymentIntent ×1) that are fixed by Tasks 8/10. Report this precisely.

- [ ] **Step 3: Build**

```bash
dotnet build
```

If build fails with "Module.Inventory.Features.Storefront.StockReservations not found", verify imports are updated.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs
git add service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/AddItem/
git commit -m "refactor(ordering): switch AddToCart to IStockReservationService

Replace MediatR ReserveCartStock command with direct service call.
Removes cross-module namespace import from Ordering to Inventory features.
Updates AddToCart unit tests to mock ReserveForVariantAsync."
```

### Task 8: Update Ordering.CreateOrderFromCart to Use IStockReservationService

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs`

**Consumes:** `IStockReservationService.ConsumeForOrderAsync` (Task 3)

- [ ] **Step 1: Replace MediatR consume with service call**

Remove import:
```csharp
using Module.Inventory.Features.Storefront.StockReservations.ConsumeCart;
```

Add imports:
```csharp
using Module.Inventory.Services;
using Module.Inventory.Services.StockReservations;
```

Add constructor parameter:
```csharp
IStockReservationService stockReservationService,
```

Replace lines 66-71:
```csharp
// Consume: Stock reservations via Inventory service (replaces inline CQRS handler).
var consumeResult = await stockReservationService.ConsumeForOrderAsync(
    cart.Id, cancellationToken);
if (consumeResult.IsFailure)
    return consumeResult.Errors;
```

- [ ] **Step 2: Update CreateOrderFromCart unit tests (plan amendment)**

Three test files mock/verify `sender.Send(new ConsumeCartStockReservations.Command(...))` from CreateOrderFromCart. Update them to mock `IStockReservationService.ConsumeForOrderAsync`:

- `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCartTests.cs`
- `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCartStockTests.cs`
- `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCartTransactionTests.cs`

Pattern:
```csharp
// BEFORE
sender.Setup(s => s.Send(It.IsAny<ConsumeCartStockReservations.Command>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(Result.Ok());

// AFTER
var reservationService = new Mock<IStockReservationService>();
reservationService.Setup(s => s.ConsumeForOrderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(Result.Ok());
```

And replace any `Verify(s => s.Send(...ConsumeCartStockReservations.Command...))` with `Verify(s => s.ConsumeForOrderAsync(cart.Id, It.IsAny<CancellationToken>()))` on the service mock.

Run tests:
```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~CreateOrderFromCart"
```

Note: `Module.UnitTests` may not compile until Task 10 fixes CreatePaymentIntent. If so, verify by building the test project and confirm the ONLY remaining test-compile error is in CreatePaymentIntentTests.cs.

- [ ] **Step 3: Build**

```bash
dotnet build
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs
git add service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Cart/Checkout/
git commit -m "refactor(ordering): switch Checkout to IStockReservationService.ConsumeForOrderAsync

Updates CreateOrderFromCart unit tests to mock ConsumeForOrderAsync."
```

### Task 9: Update Ordering.CancelOrder + Admin Cancel/UpdateStatus to Use Service

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Orders/Cancel/CancelOrder.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/Cancel/CancelOrderAdmin.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateStatus/UpdateOrderStatus.cs`

**Consumes:** `IStockItemService.AdjustStockAsync` (existing), `IStockItemService.GetStockLocationIdForVariantAsync` (added in this task)
**DECISION (pre-flight review):** For placed orders, reservations are already `Fulfilled` at cancel time, so `ReleaseReservationsAsync` would find nothing. The current `OrderInventoryService.RemoveAsync` correctly returns stock via `AdjustStockAsync(+qty)`. Replace the wrapper with an inlined per-line-item `AdjustStockAsync` call. To determine the stock location WITHOUT a `Module.Inventory.Domain.*` reference from Ordering, add a small location-lookup method to `IStockItemService`.

- [ ] **Step 1: Add GetStockLocationIdForVariantAsync to IStockItemService**

In `StockItem.Service.Interface.cs`:

```csharp
Task<Result<Guid?>> GetStockLocationIdForVariantAsync(Guid variantId, CancellationToken ct = default);
```

In `StockItem.Service.Implementation.cs` (simple first-match lookup). **Return `Result<Guid?>.Ok(null)` when no stock item exists — NOT `NotFound`.** Human decision (2026-08-11): the deleted `OrderInventoryService.RemoveAsync` silently skipped variants with no stock item (test `RemoveAsync_ShouldNotCrash_WhenNoStockLocation`), and the callers' `Value is null → continue` branch relies on success-with-null. Returning `NotFound` would abort order cancellation when any line item's variant lacks a stock item — a regression:

```csharp
public async Task<Result<Guid?>> GetStockLocationIdForVariantAsync(Guid variantId, CancellationToken ct = default)
{
    var stockItem = await dbContext.Set<StockItem>()
        .FirstOrDefaultAsync(si => si.VariantId == variantId, ct);
    return stockItem is null
        ? Result<Guid?>.Ok(null)
        : Result<Guid?>.Ok((Guid?)stockItem.StockLocationId);
}
```

(Verify `Result<T>.Ok` static constructor shape — use `(Guid?)null` or `Result<Guid?>.Ok((Guid?)stockItem.StockLocationId)` as needed.)

- [ ] **Step 2: Update CancelOrder.cs (storefront)**

Remove `using Module.Inventory.Services;`? **NO — keep it**: `IStockItemService` lives in `Module.Inventory.Services` and is still needed. Remove only `using Module.Ordering.Services;` (the `OrderInventoryService` namespace) and the `OrderInventoryService` usage.

Keep constructor parameter `IStockItemService stockItem` (do NOT remove it).

Replace the inventory release block (currently constructs `new OrderInventoryService(entity, lineItem, dbContext, stockItem)` then calls `.RemoveAsync()`) with:

```csharp
// Release: Return consumed stock for previously placed orders.
if (wasPlaced)
{
    foreach (var lineItem in entity.LineItems)
    {
        var locationResult = await stockItem.GetStockLocationIdForVariantAsync(lineItem.VariantId, cancellationToken);
        if (locationResult.IsFailure)
            return locationResult.Errors;
        if (locationResult.Value is null)
            continue;

        var adjustResult = await stockItem.AdjustStockAsync(
            lineItem.VariantId, lineItem.Quantity, locationResult.Value.Value, entity.Id, cancellationToken);
        if (adjustResult.IsFailure)
            return adjustResult.Errors;
    }
}
```

- [ ] **Step 3: Same change for CancelOrderAdmin.cs**

Apply identical pattern: keep `IStockItemService stockItem`, remove `OrderInventoryService` wrapper, call `GetStockLocationIdForVariantAsync` + `AdjustStockAsync` per line item. Remove `using Module.Ordering.Services;`.

- [ ] **Step 4: Same change for UpdateOrderStatus.cs**

Apply identical pattern in the `OrderStatus.Canceled` branch.

- [ ] **Step 5: Build**

```bash
dotnet build
```

- [ ] **Step 6: Delete OrderInventoryServiceTests.cs (plan amendment)**

Task 2 deleted `OrderInventoryService.cs`, but `service/Api/tests/Module.UnitTests/Ordering/Features/Shared/Services/OrderInventoryServiceTests.cs` still constructs `new OrderInventoryService(...)` — it will break `Module.UnitTests` compilation. Delete it (the wrapper class it tests no longer exists; its coverage is replaced by the inlined `AdjustStockAsync` behavior):

```bash
rm service/Api/tests/Module.UnitTests/Ordering/Features/Shared/Services/OrderInventoryServiceTests.cs
```

Also check `service/Api/src/Module/Ordering/README.yaml` for a stale `OrderInventoryService` reference and remove it if present.

- [ ] **Step 7: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Orders/Cancel/CancelOrder.cs
git add service/Api/src/Module/Ordering/Features/Admin/Orders/Cancel/CancelOrderAdmin.cs
git add service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateStatus/UpdateOrderStatus.cs
git add service/Api/tests/Module.UnitTests/Ordering/Features/Shared/Services/OrderInventoryServiceTests.cs
git commit -m "refactor(ordering): inline AdjustStockAsync, drop OrderInventoryService wrapper

Cancel/UpdateStatus now return stock via IStockItemService.AdjustStockAsync
per line item (location resolved via GetStockLocationIdForVariantAsync),
replacing the OrderInventoryService wrapper and its direct StockItem
entity access. Deletes OrderInventoryServiceTests (tested removed class)."
```

### Task 10: Update Billing.CreatePaymentIntent to Use Service

**Files:**
- Modify: `service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs`

**Consumes:** `IStockReservationService.ReserveForVariantAsync` (Task 7), `IStockReservationService.ReleaseReservationsAsync` (existing, called with `cartToken`)
**DECISION (pre-flight review):** Reservations are keyed by `CartToken`. `command.OrderId` is the cart/order id and its string form is used as the cart token. Do NOT pass `orderId:` to `ReserveAsync`/`ReleaseReservationsAsync` — always pass `cartToken: command.OrderId.ToString()`. No `Module.Inventory.Domain.*` reference from Billing.

- [ ] **Step 1: Replace MediatR reserve/release with service calls**

Remove imports:
```csharp
using Module.Inventory.Features.Storefront.Shared.Models;
using Module.Inventory.Features.Storefront.StockReservations.ReleaseCart;
using Module.Inventory.Features.Storefront.StockReservations.ReserveCart;
```

Add import:
```csharp
using Module.Inventory.Services;
using Module.Inventory.Services.StockReservations;
```

Add constructor parameter:
```csharp
IStockReservationService stockReservationService,
```

Replace the reserve block (lines 48-59 — the `sender.Send(new ReserveCartStock.Command(...))` call):

```csharp
// Reserve: Stock batched via Inventory service before gateway call
foreach (var li in cart.LineItems)
{
    var reserveResult = await stockReservationService.ReserveForVariantAsync(
        variantId: li.VariantId,
        quantity: li.Quantity,
        cartToken: command.OrderId.ToString(),
        ttlMinutes: 30,
        ct: cancellationToken);

    if (reserveResult.IsFailure) return reserveResult.Errors;
}
```

Replace the release block(s) — find `await sender.Send(new ReleaseCartStockReservations.Command(...))` and replace with:
```csharp
await stockReservationService.ReleaseReservationsAsync(
    cartToken: command.OrderId.ToString(), ct: CancellationToken.None);
```

There are two release locations: one after gateway failure, one in the catch block. Replace both.

- [ ] **Step 2: Update CreatePaymentIntent unit tests (plan amendment)**

`service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs` mocks/verifies `sender.Send(new ReserveCartStock.Command(...))` and `sender.Send(new ReleaseCartStockReservations.Command(...))`. Update it to mock `IStockReservationService.ReserveForVariantAsync` and `.ReleaseReservationsAsync`:

```csharp
// BEFORE
sender.Setup(s => s.Send(It.Is<ReserveCartStock.Command>(c => c.Request.CartId == cartId), It.IsAny<CancellationToken>()))
    .ReturnsAsync(Result<ReserveCartStock.Response>.Ok(new ReserveCartStock.Response()));
sender.Setup(s => s.Send(It.IsAny<ReleaseCartStockReservations.Command>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(Result.Ok());

// AFTER
var reservationService = new Mock<IStockReservationService>();
reservationService.Setup(s => s.ReserveForVariantAsync(
        It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(Result<StockReservation>.Ok(StockReservationMethod.Reserve(
        Guid.NewGuid(), 1, Guid.NewGuid(), null, 30, cartToken: "test")));
reservationService.Setup(s => s.ReleaseReservationsAsync(
        It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(Result<int>.Ok(1));
```

Replace the `Verify` calls on the sender for these commands with service-mock verifies. The handler constructor gains `IStockReservationService` — pass the mock.

Run tests:
```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~CreatePaymentIntent"
```

- [ ] **Step 3: Build**

```bash
dotnet build
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs
git add service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs
git commit -m "refactor(billing): switch CreatePaymentIntent to IStockReservationService

Replace ReserveCartStock + ReleaseCartStockReservations MediatR commands
with direct service calls keyed on cartToken. Removes cross-module
namespace imports and direct StockItem entity access. Updates
CreatePaymentIntent unit tests to mock the service methods."
```

### Task 11: Create Inventory Storefront Endpoints — ReserveStockReservation

**Files:**
- Create: `service/Api/src/Module/Inventory/Features/Storefront/StockReservations/Reserve/ReserveStockReservation.Endpoint.cs`
- Create: `service/Api/src/Module/Inventory/Features/Storefront/StockReservations/Reserve/ReserveStockReservation.Validator.cs`
- Create: `service/Api/src/Module/Inventory/Features/Storefront/StockReservations/Reserve/ReserveStockReservation.Request.cs`

**Interfaces:**
- Consumes: `IStockReservationService.ReserveAsync` (existing)
- Produces: `POST /api/storefront/inventory/stock-reservations` → `Result<StockReservation>`

- [ ] **Step 1: Create Request model**

```csharp
// ReserveStockReservation.Request.cs
namespace Module.Inventory.Features.Storefront.StockReservations.Reserve;

public static partial class ReserveStockReservation
{
    public sealed record Request
    {
        public Guid VariantId { get; init; }
        public Guid StockLocationId { get; init; }
        public int Quantity { get; init; }
        public int TtlMinutes { get; init; } = 15;
    }
}
```

- [ ] **Step 2: Create Validator**

```csharp
// ReserveStockReservation.Validator.cs
using FluentValidation;

namespace Module.Inventory.Features.Storefront.StockReservations.Reserve;

public static partial class ReserveStockReservation
{
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.VariantId).NotEmpty();
            RuleFor(x => x.StockLocationId).NotEmpty();
            RuleFor(x => x.Quantity).GreaterThan(0);
            RuleFor(x => x.TtlMinutes).InclusiveBetween(1, 10080);
        }
    }
}
```

- [ ] **Step 3: Create Endpoint (thin wrapper, no Handler.cs)**

```csharp
// ReserveStockReservation.Endpoint.cs
using Carter;
using Module.Inventory.Features.Shared;
using Module.Inventory.Services.StockReservations;

namespace Module.Inventory.Features.Storefront.StockReservations.Reserve;

public static partial class ReserveStockReservation
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(InventoryFeature.Storefront.StockReservations.Reserve.Route, async (
                [FromBody] Request request,
                IStockReservationService reservationService,
                HttpContext httpContext,
                CancellationToken ct) =>
            {
                var cartToken = httpContext.Items["CartToken"]?.ToString();
                var result = await reservationService.ReserveAsync(
                    request.VariantId, request.Quantity, request.StockLocationId,
                    cartToken: cartToken, ttlMinutes: request.TtlMinutes, ct: ct);
                return result.ToResult();
            })
            .WithName(nameof(ReserveStockReservation))
            .WithTags(InventoryFeature.Tags.StockReservation)
            .WithSummary(InventoryFeature.Storefront.StockReservations.Reserve.Summary)
            .WithDescription(InventoryFeature.Storefront.StockReservations.Reserve.Description)
            .Produces<Result<StockReservation>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}
```

- [ ] **Step 4: Build**

```bash
dotnet build
```

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Storefront/StockReservations/Reserve/
git commit -m "feat(inventory): add POST /inventory/stock-reservations endpoint

Thin wrapper around IStockReservationService.ReserveAsync.
Reads X-Cart-Token from HttpContext.Items set by CartTokenMiddleware."
```

### Task 12: Create Inventory Storefront Endpoints — GetCartReservations

**Files:**
- Create: `service/Api/src/Module/Inventory/Features/Storefront/StockReservations/Get/GetCartReservations.Endpoint.cs`
- Create: `service/Api/src/Module/Inventory/Features/Storefront/StockReservations/Get/GetCartReservations.Response.cs`

**Note (implemented per AGENTS.md):** No Validator.cs — this is a read-only GET with no Request body; cartToken comes from HttpContext.Items and is validated inline via `Results.BadRequest`. Per AGENTS.md rule 3, read-only queries may omit Validator files.

**Consumes:** `IStockReservationService.GetReservationsForCartAsync` (existing)
**DECISION (pre-flight review):** The endpoint must NOT serialize the raw `StockReservation` domain entity. Define a `CartReservationStatus` response DTO (mirrors the SPA `CartReservationStatus` type in `app/Store/src/features/inventory/types/availability.ts`).

- [ ] **Step 1: Create Response DTO**

```csharp
// GetCartReservations.Response.cs
namespace Module.Inventory.Features.Storefront.StockReservations.Get;

public static partial class GetCartReservations
{
    public sealed record CartReservationStatus
    {
        public Guid Id { get; init; }
        public Guid VariantId { get; init; }
        public Guid? StockLocationId { get; init; }
        public Guid? OrderId { get; init; }
        public int Quantity { get; init; }
        public string State { get; init; } = string.Empty;
        public DateTimeOffset? ExpiresAtUtc { get; init; }
        public string? Reason { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }
        public DateTimeOffset? ModifiedAtUtc { get; init; }
        public int RemainingSeconds { get; init; }
    }
}
```

- [ ] **Step 2: Create Endpoint**

```csharp
// GetCartReservations.Endpoint.cs
using Carter;
using Module.Inventory.Features.Shared;
using Module.Inventory.Services.StockReservations;

namespace Module.Inventory.Features.Storefront.StockReservations.Get;

public static partial class GetCartReservations
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(InventoryFeature.Storefront.StockReservations.Get.Route, async (
                HttpContext httpContext,
                IStockReservationService reservationService,
                CancellationToken ct) =>
            {
                var cartToken = httpContext.Items["CartToken"]?.ToString();
                if (string.IsNullOrWhiteSpace(cartToken))
                    return Results.BadRequest(Result.Failure(
                        Error.BadRequest("CartToken.Required", "X-Cart-Token header is required")));

                var result = await reservationService.GetReservationsForCartAsync(cartToken, ct);
                if (result.IsFailure)
                    return result.ToResult();

                var response = result.Value.Select(r => new CartReservationStatus
                {
                    Id = r.Reservation.Id,
                    VariantId = r.Reservation.VariantId,
                    StockLocationId = r.Reservation.StockLocationId,
                    OrderId = r.Reservation.OrderId,
                    Quantity = r.Reservation.Quantity,
                    State = r.Reservation.State.ToString(),
                    ExpiresAtUtc = r.Reservation.ExpiresAtUtc,
                    Reason = r.Reservation.Reason,
                    CreatedAtUtc = r.Reservation.CreatedAtUtc,
                    ModifiedAtUtc = r.Reservation.ModifiedAtUtc,
                    RemainingSeconds = r.RemainingSeconds
                }).ToList();

                return Result<List<CartReservationStatus>>.Ok(response).ToResult();
            })
            .WithName(nameof(GetCartReservations))
            .WithTags(InventoryFeature.Tags.StockReservation)
            .WithSummary(InventoryFeature.Storefront.StockReservations.Get.Summary)
            .WithDescription(InventoryFeature.Storefront.StockReservations.Get.Description)
            .Produces<Result<List<CartReservationStatus>>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}
```

- [ ] **Step 3: Build**

```bash
dotnet build
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Storefront/StockReservations/Get/
git commit -m "feat(inventory): add GET /inventory/stock-reservations endpoint

Returns active reservations (mapped to CartReservationStatus DTO) for the
cart identified by X-Cart-Token header."
```

### Task 13: Create Inventory Storefront Endpoints — ReleaseStockReservation + GetStockAvailability

**Files:**
- Create: `service/Api/src/Module/Inventory/Features/Storefront/StockReservations/Release/ReleaseStockReservation.Endpoint.cs`
- Create: `service/Api/src/Module/Inventory/Features/Storefront/StockReservations/Release/ReleaseStockReservation.Validator.cs`
- Create: `service/Api/src/Module/Inventory/Features/Storefront/StockItems/GetAvailability/GetStockAvailability.Endpoint.cs`
- Create: `service/Api/src/Module/Inventory/Features/Storefront/StockItems/GetAvailability/GetStockAvailability.Validator.cs`

**Consumes:** `IStockReservationService.ReleaseReservationAsync` (Task 4), `IStockItemService.GetAvailabilityForCartAsync` (Task 5)

- [ ] **Step 1: Create ReleaseStockReservation.Endpoint.cs**

```csharp
using Carter;
using Module.Inventory.Features.Shared;
using Module.Inventory.Services.StockReservations;

namespace Module.Inventory.Features.Storefront.StockReservations.Release;

public static partial class ReleaseStockReservation
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(InventoryFeature.Storefront.StockReservations.Release.Route, async (
                [FromRoute] Guid id,
                IStockReservationService reservationService,
                CancellationToken ct) =>
            {
                var result = await reservationService.ReleaseReservationAsync(id, ct);
                return result.ToResult();
            })
            .WithName(nameof(ReleaseStockReservation))
            .WithTags(InventoryFeature.Tags.StockReservation)
            .WithSummary(InventoryFeature.Storefront.StockReservations.Release.Summary)
            .WithDescription(InventoryFeature.Storefront.StockReservations.Release.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }

    public sealed class Validator : AbstractValidator<Guid>
    {
        public Validator() { RuleFor(x => x).NotEmpty(); }
    }
}
```

- [ ] **Step 2: Create GetStockAvailability.Endpoint.cs**

```csharp
using Carter;
using Module.Inventory.Features.Shared;
using Module.Inventory.Services;

namespace Module.Inventory.Features.Storefront.StockItems.GetAvailability;

public static partial class GetStockAvailability
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(InventoryFeature.Storefront.StockItems.GetAvailability.Route, async (
                [FromRoute] Guid variantId,
                IStockItemService stockItemService,
                HttpContext httpContext,
                CancellationToken ct) =>
            {
                var cartToken = httpContext.Items["CartToken"]?.ToString();
                var result = await stockItemService.GetAvailabilityForCartAsync(
                    variantId, cartToken, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetStockAvailability))
            .WithTags(InventoryFeature.Tags.StockItem)
            .WithSummary(InventoryFeature.Storefront.StockItems.GetAvailability.Summary)
            .WithDescription(InventoryFeature.Storefront.StockItems.GetAvailability.Description)
            .Produces<Result<VariantStockAvailability>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }

    public sealed class Validator : AbstractValidator<Guid>
    {
        public Validator() { RuleFor(x => x).NotEmpty(); }
    }
}
```

- [ ] **Step 3: Build**

```bash
dotnet build
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Storefront/StockReservations/Release/
git add service/Api/src/Module/Inventory/Features/Storefront/StockItems/GetAvailability/
git commit -m "feat(inventory): add Release reservation + Get availability endpoints

DELETE /inventory/stock-reservations/{id} releases single reservation.
GET /inventory/stock-items/{variantId}/availability returns per-location
stock with optional cart token exclusion."
```

### Task 14: Update InventoryFeature.Storefront.cs with Route Constants

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Shared/InventoryFeature.Storefront.cs`

- [ ] **Step 1: Add route constants**

Replace the entire file content:

```csharp
using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Features.Shared;

public static partial class InventoryFeature
{
    public static class Storefront
    {
        public static class StockItems
        {
            public static class GetAvailability
            {
                public const string Route = "api/storefront/inventory/stock-items/{variantId:guid}/availability";
                public const string Description = "Get per-location stock availability for a variant with optional cart token exclusion";
                public const string Summary = "Get variant stock availability";
            }
        }

        public static class StockReservations
        {
            public const int TtlMinutesDefault = StockReservationConstant.Defaults.DefaultTtlMinutes;

            public static class Reserve
            {
                public const string Route = "api/storefront/inventory/stock-reservations";
                public const string Description = "Reserve stock for a specific variant and location";
                public const string Summary = "Reserve stock";
            }

            public static class Get
            {
                public const string Route = "api/storefront/inventory/stock-reservations";
                public const string Description = "List active stock reservations for the current cart";
                public const string Summary = "Get cart reservations";
            }

            public static class Release
            {
                public const string Route = "api/storefront/inventory/stock-reservations/{id:guid}";
                public const string Description = "Release a single stock reservation by identifier";
                public const string Summary = "Release reservation";
            }
        }
    }
}
```

- [ ] **Step 2: Build — verify all 4 endpoints reference valid route constants**

```bash
dotnet build
```

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Shared/InventoryFeature.Storefront.cs
git commit -m "feat(inventory): add storefront route constants

Define routes for 4 inventory storefront endpoints:
StockItems/GetAvailability, StockReservations/{Reserve, Get, Release}"
```

### Task 15: Remove Order.StoreId Across Module (pre-existing defect fix)

> **Pre-flight/human decision (2026-08-11):** `Order.StoreId` was a partially-implemented feature at baseline (`109bb4939 "Before big big"`) that never had its property/factory param completed. It blocks a clean `dotnet test` (Task 16). The human chose to REMOVE all `StoreId` references rather than complete the feature.

**Files:**
- Modify: `service/Api/src/Module/Ordering/Persistence/Configurations/OrderConfiguration.cs` — remove `builder.Property(x => x.StoreId);` (line 71)
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/CreateCart/CreateCart.cs` — remove `var storeId = Guid.Empty;` and drop the `storeId` positional arg from `OrderMethod.Create`
- Modify: `service/Api/src/Module/Ordering/Persistence/Seeders/Order.Seeder.cs` — remove `storeId` local, the `CreateOrder(..., Guid storeId, ...)` param, the 3 call-site args, and the `OrderMethod.Create` arg
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/Create/CreateOrder.Request.cs` — remove `public Guid StoreId { get; set; }`
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/Create/CreateOrder.cs` — remove `storeId: request.StoreId` from `MapToDomain`
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/Mappings/Order.Mapping.Domain.cs` — remove the `Guid storeId` parameter from `MapToDomain`
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/Models/Order.Model.Response.cs` — remove `public Guid? StoreId { get; init; }`
- Tests: `Order.Method.Tests.cs`, `Order.Mapping.Tests.cs`, `ApproveOrderTests.cs`, `ResumeOrderTests.cs`, `UpdateOrderStatusTests.cs`, `GetOrderLineItemByIdTests.cs`, `EventHandlerInvocationTests.cs`

**KEY FACTS (verified):** `OrderMethod.Create` signature is `Create(string currency, Guid? userId, Guid? id = null, string? sessionId = null, Guid? shipAddressId = null)` — the 3rd positional param is `id`, NOT `storeId`. The current callers pass `storeId` positionally into `id`, and the mapping's `MapToDomain` `storeId` param is unused (its body only passes `currency` + `userId`). So removal is purely deleting the stray references; the factory keeps its `id` param.

- [ ] **Step 1: Remove StoreId from domain/configuration**

In `OrderConfiguration.cs`, delete line `builder.Property(x => x.StoreId);`.

- [ ] **Step 2: Remove StoreId from storefront cart creation**

In `CreateCart.cs`:
- Delete `var storeId = Guid.Empty;`
- Change `OrderMethod.Create(OrderConstant.Defaults.Currency, userId, storeId, sessionId: sessionId)` → `OrderMethod.Create(OrderConstant.Defaults.Currency, userId, sessionId: sessionId)`

- [ ] **Step 3: Remove StoreId from seeder**

In `Order.Seeder.cs`:
- Delete `var storeId = Guid.Empty;`
- Remove `storeId` from the 3 `CreateOrder(...)` call-site args (keep `addresses, variants, cancellationToken`)
- Remove `Guid storeId,` from the `CreateOrder` private method signature
- Change `OrderMethod.Create(OrderConstant.Defaults.Currency, user.Id, storeId)` → `OrderMethod.Create(OrderConstant.Defaults.Currency, user.Id)`

- [ ] **Step 4: Remove StoreId from admin create order flow**

In `CreateOrder.Request.cs`: delete `public Guid StoreId { get; set; }`
In `CreateOrder.cs`: change `request.MapToDomain(userId: Guid.Empty, storeId: request.StoreId)` → `request.MapToDomain(userId: Guid.Empty)`
In `Order.Mapping.Domain.cs`: change signature `MapToDomain<T>(this T request, Guid userId, Guid storeId)` → `MapToDomain<T>(this T request, Guid userId)` and remove the `storeId` `<param>` doc line
In `Order.Model.Response.cs`: delete `public Guid? StoreId { get; init; }`

- [ ] **Step 5: Remove StoreId from unit tests**

- `Order.Method.Tests.cs` (`Create_WithValidParams_ShouldReturnOrder`): delete `var storeId = Guid.NewGuid();`, `order.StoreId.Should().Be(storeId);`, and change `OrderMethod.Create("USD", Guid.NewGuid(), storeId)` → `OrderMethod.Create("USD", Guid.NewGuid())`. In `Finalize_WithItems_ShouldSucceed`, `OrderMethod.Create("USD", null, Guid.NewGuid())` → `OrderMethod.Create("USD", null)` (the `Guid.NewGuid()` was the stray storeId/id positional arg — dropping it uses the factory's default id).
- `Order.Mapping.Tests.cs`: `ToDomain_ShouldMapRequestToEntity` — delete `var storeId = Guid.NewGuid();`, `request.MapToDomain(userId, storeId)` → `request.MapToDomain(userId)`, delete `order.StoreId.Should().Be(storeId);`. In `CreateOrder` helper, `storeId: Guid.NewGuid()` arg removed from `OrderMethod.Create(...)`.
- `ApproveOrderTests.cs`, `ResumeOrderTests.cs`, `UpdateOrderStatusTests.cs` (×2), `GetOrderLineItemByIdTests.cs` (×2), `EventHandlerInvocationTests.cs`: change `OrderMethod.Create("USD", userId: Guid.NewGuid(), storeId: Guid.Empty)` → `OrderMethod.Create("USD", userId: Guid.NewGuid())`.

- [ ] **Step 6: Build + run Ordering tests**

```bash
dotnet build
dotnet test service/Api/tests/Module.UnitTests --filter-class "OrderMethod|OrderMapping|ApproveOrder|ResumeOrder|UpdateOrderStatus|GetOrderLineItemById|EventHandlerInvocation"
```

If `Module` still fails on the 3 pre-existing namespace errors (Tasks 8-10 must be done first), verify with the temp-stub method used in Task 7: confirm your changed files have zero errors and the ONLY remaining test errors are the known masked ones.

- [ ] **Step 7: Commit**

```bash
git add service/Api/src/Module/Ordering/
git add service/Api/tests/Module.UnitTests/Ordering/
git commit -m "fix(ordering): remove partially-implemented Order.StoreId feature

StoreId was referenced in config/callers/tests but never added to the
Order entity or OrderMethod.Create. Removing all stray references
across the module (source + tests). OrderMethod.Create keeps its id
param; callers that passed storeId positionally now omit it."
```

### Task 16: Verify Cross-Module References and Full Build

- [ ] **Step 1: Run cross-module ref check**

```bash
bash scripts/check-cross-module-refs.sh
```

Expected: Zero violations between Inventory and Ordering modules. Service interface imports (`Module.Inventory.Services`) are permitted — the script should not flag `using Module.Inventory.Services` in Ordering files.

If violations remain, search for the pattern:

```bash
rg "using Module.Inventory.Features.Storefront" service/Api/src/Module/Ordering/ service/Api/src/Module/Billing/
```

Every match must be addressed — either the import is gone (old CQRS handlers deleted) or it's been replaced with service injection.

- [ ] **Step 2: Full build + unit tests**

```bash
dotnet build
dotnet test service/Api/tests/Module.UnitTests
```

- [ ] **Step 3: Commit**

```bash
git add -A
git commit -m "chore: verify cross-module refs and full build pass after inventory consolidation"
```
