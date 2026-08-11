---
title: Inventory Services Consolidation — Drop CQRS, Extend Services
version: 1.0
date_created: 2025-08-11
last_updated: 2025-08-11
owner: ReSys.Shop Platform
tags: design, inventory, services, cqrs, refactor
---

# Introduction

This specification defines the consolidation of Inventory module's storefront CQRS handlers into the existing domain service layer. Three redundant CQRS feature directories are deleted. Two new methods are added to `IStockReservationService`. One new method is added to `IStockItemService`. Other modules switch from MediatR commands to direct service injection.

## 1. Purpose & Scope

### Purpose

Eliminate the indirection layer where Inventory CQRS handlers duplicate logic already present in `IStockReservationService` and `IStockItemService`. Route all inter-module inventory operations through service interfaces with `Result<T>` contracts.

### Scope

- Delete 3 CQRS handler directories (10 files)
- Add `ConsumeForOrderAsync` and `ReleaseReservationAsync` to `IStockReservationService`
- Add `GetAvailabilityForCartAsync` to `IStockItemService`
- Update Ordering module: 4 handlers switch from MediatR to service DI
- Update Billing module: 2 handlers switch from MediatR to service DI
- Create `CartTokenMiddleware`
- Delete `OrderInventoryService.cs`
- Create 4 inventory storefront REST endpoints

### Out of Scope

- Admin inventory features (unchanged)
- Catalog module's existing `IStockItemService` usage (already correct)
- Reservation expiry background job (already uses `IStockReservationService.ExpireReservationsAsync`)

## 2. Definitions

| Term | Definition |
|------|------------|
| **CQRS Handler** | MediatR `ICommandHandler` or `IQueryHandler` in a feature directory acting as intermediary between endpoint and domain |
| **Service Layer** | Domain service interface with `Result<T>` methods, injectable via DI |
| **Consume** | Transition reservation from `Reserved` → `Fulfilled`, pick stock via `stockItem.Pick(quantity)` |
| **Serializable** | PostgreSQL isolation level preventing phantom reads — required for concurrent reservation safety |
| **CartTokenMiddleware** | ASP.NET middleware extracting `X-Cart-Token` header into `HttpContext.Items["CartToken"]` |

## 3. Requirements, Constraints & Guidelines

### Deletion Requirements

- **DEL-001**: `Inventory/Features/Storefront/StockReservations/ReserveCart/` — all 4 files deleted
- **DEL-002**: `Inventory/Features/Storefront/StockReservations/ConsumeCart/` — all 3 files deleted
- **DEL-003**: `Inventory/Features/Storefront/StockReservations/ReleaseCart/` — all 3 files deleted
- **DEL-004**: `Ordering/Services/OrderInventoryService.cs` — deleted, replaced by `IStockItemService.AdjustStockAsync`
- **DEL-005**: `Inventory/Features/Storefront/Shared/Models/Store.StockItem.Model.cs` — empty file (0 bytes), deleted

### Service Extension Requirements

- **SVC-001**: `IStockReservationService.ConsumeForOrderAsync(Guid orderId, CancellationToken ct)` — finds all `Reserved`-state reservations for the order, calls `stockItem.Pick(quantity)` for each, sets state to `Fulfilled`. Returns `Result`.
- **SVC-002**: `IStockReservationService.ReleaseReservationAsync(Guid reservationId, CancellationToken ct)` — finds single reservation by ID, verifies it is `Reserved` state, calls `reservation.Release()`. Returns `Result`.
- **SVC-003**: `IStockItemService.GetAvailabilityForCartAsync(Guid variantId, string? cartToken, CancellationToken ct)` — returns per-location `VariantStockAvailability` with `Available` count subtracting the cart's own active reservations.

### Middleware Requirements

- **MID-001**: `CartTokenMiddleware` reads `X-Cart-Token` header → `HttpContext.Items["CartToken"]` (string or null)
- **MID-002**: Registered in `Program.cs` after `UseRouting()`, before `UseAuthentication()`
- **MID-003**: No-op when header absent (authenticated users identify cart by JWT subject)

### Cross-Module Injection Requirements

- **CMR-001**: Ordering handlers inject `IStockReservationService` directly — drop `ISender` usage for inventory commands
- **CMR-002**: Billing handlers inject `IStockReservationService` directly — drop `ISender` usage for inventory commands
- **CMR-003**: No `using Module.Inventory.Domain.*` in Ordering or Billing files after migration

### Storefront Endpoint Requirements

- **EP-001**: 4 inventory storefront endpoints created as thin wrappers (Endpoint → Service → Result, no MediatR handler)
- **EP-002**: All inventory endpoints read cart token from `HttpContext.Items["CartToken"]`, never from route/query params
- **EP-003**: No permission checks on storefront inventory endpoints (public browsing + cart-auth)

## 4. Interfaces & Data Contracts

### 4.1 New Service Methods — Signatures

```csharp
// IStockReservationService additions
Task<Result> ConsumeForOrderAsync(Guid orderId, CancellationToken ct = default);
Task<Result> ReleaseReservationAsync(Guid reservationId, CancellationToken ct = default);

// IStockItemService additions
Task<Result<VariantStockAvailability>> GetAvailabilityForCartAsync(
    Guid variantId, string? cartToken, CancellationToken ct = default);
```

### 4.2 ConsumeForOrderAsync — Behavior Contract

```
Input:  orderId: Guid
Steps:
  1. Query all StockReservation where OrderId == orderId AND State == Reserved
  2. If count == 0, return StockReservationResult.Errors.NoActiveReservations
  3. For each reservation:
     a. Query StockItem where VariantId + StockLocationId match
     b. If stockItem is null, return StockReservationResult.Errors.StockItemNotFound
     c. Call stockItem.Pick(reservation.Quantity) — domain method validates countOnHand >= quantity
     d. If Pick fails, return errors
     e. Set reservation.State = Fulfilled, ModifiedAtUtc = now
  4. SaveChangesAsync
  5. Return Result.Ok()
```

### 4.3 GetAvailabilityForCartAsync — Behavior Contract

```
Input:  variantId: Guid, cartToken: string?
Steps:
  1. Query StockItem where VariantId == variantId AND CountOnHand > 0
  2. For each stock item:
     a. Query SUM(Quantity) from StockReservation where VariantId + StockLocationId match
        AND State == Reserved AND ExpiresAtUtc > now
        AND (cartToken is null OR CartToken != cartToken)
     b. Available = CountOnHand - reserved (excluding this cart's reservations)
  3. Build VariantStockAvailability with per-location breakdown
  4. Return
```

### 4.4 Thin Endpoint Pattern — No Handler.cs

```csharp
// ReserveStockReservation.Endpoint.cs
// No separate Handler.cs — endpoint calls service directly
app.MapPost(route, async (
    [FromBody] Request request,
    IStockReservationService reservationService,
    IHttpContextAccessor httpContext,
    CancellationToken ct) =>
{
    var cartToken = httpContext.HttpContext?.Items["CartToken"]?.ToString();
    var result = await reservationService.ReserveAsync(
        request.VariantId, request.Quantity, request.StockLocationId,
        cartToken: cartToken, ttlMinutes: request.TtlMinutes, ct: ct);
    return result.ToResult();
});
```

### 4.5 Files to Create

```
CREATE Shared/Security/Cart/CartTokenMiddleware.cs
CREATE Inventory/Features/Shared/InventoryFeature.Storefront.cs  (extend with routes)
CREATE Inventory/Features/Storefront/StockItems/GetAvailability/GetStockAvailability.Endpoint.cs
CREATE Inventory/Features/Storefront/StockItems/GetAvailability/GetStockAvailability.Validator.cs
CREATE Inventory/Features/Storefront/StockReservations/Reserve/ReserveStockReservation.Endpoint.cs
CREATE Inventory/Features/Storefront/StockReservations/Reserve/ReserveStockReservation.Validator.cs
CREATE Inventory/Features/Storefront/StockReservations/Reserve/ReserveStockReservation.Request.cs
CREATE Inventory/Features/Storefront/StockReservations/Get/GetCartReservations.Endpoint.cs
CREATE Inventory/Features/Storefront/StockReservations/Get/GetCartReservations.Validator.cs
CREATE Inventory/Features/Storefront/StockReservations/Release/ReleaseStockReservation.Endpoint.cs
CREATE Inventory/Features/Storefront/StockReservations/Release/ReleaseStockReservation.Validator.cs
```

### 4.6 Files to Modify

```
MODIFY Inventory/Services/StockReservations/StockReservation.Service.Interface.cs  (+ConsumeForOrderAsync, +ReleaseReservationAsync)
MODIFY Inventory/Services/StockReservations/StockReservation.Service.Implementation.cs  (+implementations)
MODIFY Inventory/Services/StockItems/StockItem.Service.Interface.cs  (+GetAvailabilityForCartAsync)
MODIFY Inventory/Services/StockItems/StockItem.Service.Implementation.cs  (+implementation)
MODIFY Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs  (IStockReservationService DI)
MODIFY Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs  (IStockReservationService DI)
MODIFY Ordering/Features/Storefront/Orders/Cancel/CancelOrder.cs  (IStockReservationService DI)
MODIFY Ordering/Features/Admin/Orders/Cancel/CancelOrderAdmin.cs  (IStockReservationService DI)
MODIFY Ordering/Features/Admin/Orders/UpdateStatus/UpdateOrderStatus.cs  (IStockReservationService DI)
MODIFY Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs  (IStockReservationService DI, drop ReserveCartStock + ReleaseCartStockReservations MediatR calls)
MODIFY Billing/Features/Storefront/Payment/Status/GetPaymentStatus.cs  (replace direct Order entity query with MediatR GetCustomerOrder.Query)
```

### 4.7 Files to Delete

```
DELETE Inventory/Features/Storefront/StockReservations/ReserveCart/ReserveCartStock.cs
DELETE Inventory/Features/Storefront/StockReservations/ReserveCart/ReserveCartStock.Request.cs
DELETE Inventory/Features/Storefront/StockReservations/ReserveCart/ReserveCartStock.Response.cs
DELETE Inventory/Features/Storefront/StockReservations/ReserveCart/ReserveCartStock.Validator.cs
DELETE Inventory/Features/Storefront/StockReservations/ConsumeCart/ConsumeCartStockReservations.cs
DELETE Inventory/Features/Storefront/StockReservations/ConsumeCart/ConsumeCartStockReservations.Request.cs
DELETE Inventory/Features/Storefront/StockReservations/ConsumeCart/ConsumeCartStockReservations.Validator.cs
DELETE Inventory/Features/Storefront/StockReservations/ReleaseCart/ReleaseCartStockReservations.cs
DELETE Inventory/Features/Storefront/StockReservations/ReleaseCart/ReleaseCartStockReservations.Request.cs
DELETE Inventory/Features/Storefront/StockReservations/ReleaseCart/ReleaseCartStockReservations.Validator.cs
DELETE Ordering/Services/OrderInventoryService.cs
DELETE Inventory/Features/Storefront/Shared/Models/Store.StockItem.Model.cs
```

## 5. Acceptance Criteria

- **AC-001**: `ReserveCart/` directory does not exist. `dotnet build` passes.
- **AC-002**: `ConsumeCart/` directory does not exist. `dotnet build` passes.
- **AC-003**: `ReleaseCart/` directory does not exist. `dotnet build` passes.
- **AC-004**: `OrderInventoryService.cs` does not exist. `dotnet build` passes.
- **AC-005**: Calling `IStockReservationService.ConsumeForOrderAsync(orderId)` transitions all `Reserved`-state reservations for that order to `Fulfilled` and decrements `stock_items.count_on_hand` for each.
- **AC-006**: Calling `IStockReservationService.ReleaseReservationAsync(reservationId)` transitions a single reservation from `Reserved` to `Released`.
- **AC-007**: Calling `IStockItemService.GetAvailabilityForCartAsync(variantId, "cart_token_A")` returns `availableCount` that excludes `cart_token_A`'s own reservations.
- **AC-008**: `POST /api/storefront/inventory/stock-reservations` with `X-Cart-Token` header creates a reservation and returns `Result<CartReservation>`.
- **AC-009**: `GET /api/storefront/inventory/stock-reservations` with `X-Cart-Token` header returns paged active reservations for that cart.
- **AC-010**: `DELETE /api/storefront/inventory/stock-reservations/{id}` releases a single reservation by ID.
- **AC-011**: `GET /api/storefront/inventory/stock-items/{variantId}/availability` returns per-location stock with available count.
- **AC-012**: Ordering.AddToCart handler injects `IStockReservationService`, not `ISender` for reserve operations.
- **AC-013**: Billing.CreatePaymentIntent handler injects `IStockReservationService`, not `ISender` for reserve/release operations.
- **AC-014**: `scripts/check-cross-module-refs.sh` reports zero violations between Inventory and Ordering.

## 6. Test Automation Strategy

### Unit Tests

- **IStockReservationService.ConsumeForOrderAsync:**
  - Happy path: All reservations fulfilled, stock decremented
  - No active reservations: returns `NoActiveReservations` error
  - Stock item not found: returns `StockItemNotFound` error
  - Insufficient stock on Pick: returns `InsufficientStock` error

- **IStockReservationService.ReleaseReservationAsync:**
  - Happy path: Single reservation released
  - Already released reservation: returns error
  - Non-existent reservation: returns `NotFound` error

- **IStockItemService.GetAvailabilityForCartAsync:**
  - No cart token: returns raw availability (all reservations subtracted)
  - With cart token: own reservations excluded from subtraction
  - No stock items for variant: empty locations list
  - Multiple locations: per-location breakdown correct

### Integration Tests

- End-to-end reserve + consume + release cycle via service layer
- Concurrent reservation test: 2 parallel `ReserveAsync` calls for same variant-location, verify only one succeeds when insufficient stock

### Mock Strategy

- **Unit tests:** Mock `IApplicationDbContext` with in-memory list of `StockItem` and `StockReservation`. No database.
- **Integration tests:** Use Testcontainers PostgreSQL for real transaction behavior (`Serializable` isolation, `BeginTransactionAsync`, concurrent access).

## 7. Rationale & Context

### Why CQRS is wrong for inventory inter-module calls

The 3 CQRS handlers were created when the cart flow was first implemented. They duplicate Entity Framework queries identical to the service layer:

| Handler | Service Equivalent |
|---------|-------------------|
| `ReserveCartStock.Handle()` | `IStockReservationService.ReserveAsync()` × N (loop) |
| `ConsumeCartStockReservations.Handle()` | `IStockReservationService.ConsumeForOrderAsync()` (new) |
| `ReleaseCartStockReservations.Handle()` | `IStockReservationService.ReleaseReservationsAsync()` |

Each handler: opens EF transaction, queries StockItem, queries StockReservation, calls domain factory, saves. The service does exactly the same. The only difference: the handler uses `RepeatableRead` (bug) vs service's `Serializable` (correct).

### Why Serialization isolation matters

Two users add the same last-in-stock item to cart simultaneously:

```
StockItem: count_on_hand=1, active reservations=0

Thread A: Check available (1-0=1 >= 1) → OK, insert reservation
Thread B: Check available (1-0=1 >= 1) → OK, insert reservation
Result:   Both succeed. 2 reservations for 1 item. Oversold.

With Serializable:
Thread A: Check available → INSERT → COMMIT
Thread B: Check available (1-1=0 < 1) → FAIL
Result:   Only one succeeds. Correct.
```

### Why thin endpoints without handlers

The 4 inventory storefront REST endpoints do zero business logic. They: extract cart token from HttpContext.Items, call a service method, return the result. A MediatR Handler.cs adds no value — it's a pass-through. Skipping it saves 4 handler files and reduces indirection.

### Why CartTokenMiddleware before auth

Guest users have no JWT. They identify by `X-Cart-Token` header. The middleware must run before auth so that both guest (cart-token-only) and auth (JWT) requests can access `Items["CartToken"]`. The handler decides: use cart token if present (guest), else use auth user ID to find cart.
