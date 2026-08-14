---
goal: Introduce a first-class Shipment aggregate in the Shipping module with a per-shipment lifecycle, auto-create on order placement, and a derived OrderFulfillmentState cached on Order via a cross-module command (replacing the admin-set Order.ShipmentState).
version: 1.0
date_created: 2026-08-14
last_updated: 2026-08-14
owner: Shipping / Ordering
status: 'Planned'
tags: [feature, shipping, ordering, shipment, fulfillment, enum, migration]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Fulfillment is currently a free-floating `OrderShipmentState` enum on the Order,
set directly by the admin `UpdateOrderShipmentState` endpoint, with no tracking
number, method snapshot, or link to the Shipping module. This plan introduces a
`Shipment` aggregate owned by Shipping (1:N per order, `Pending → Ready → Shipped →
Delivered`, with `Backorder`/`Canceled`), auto-creates one Pending shipment on
placement, and derives the order's `OrderFulfillmentState` from its shipments via a
cross-module command — the same pattern as `RecordOrderPaymentState`.

**Spec:** `spec/spec-checkout-state-enum-alignment.md` §3.5, §4.2, §4.7

## 1. Requirements & Constraints

- **REQ-001**: `Shipment` aggregate in Shipping: `Id`, `OrderId` (Guid, no navigation), `ShippingMethodId` (FK), `TrackingNumber`, `Status`, `ShippedAtUtc`, `DeliveredAtUtc`, `EstimatedDeliveryAtUtc`, auditing.
- **REQ-002**: `ShipmentStatus { Pending, Ready, Shipped, Delivered, Backorder, Canceled }`; transition rules enforced in `ShipmentMethod`.
- **REQ-003**: On order placement, Shipping auto-creates one `Pending` shipment (`CreateShipmentCommand` sent from `CheckoutPlacementService`).
- **REQ-004**: Rename `OrderShipmentState` → `OrderFulfillmentState { None, Pending, Partial, Shipped, Delivered, Canceled }`; `Order.ShipmentState` becomes the derived cache set only by `RecordOrderShipmentStateCommand`.
- **REQ-005**: `RecordOrderShipmentStateCommand { OrderId, FulfillmentState, ShippedAtUtc?, DeliveredAtUtc? }` (Ordering) sent by Shipping after every shipment change.
- **REQ-006**: Remove Ordering's `UpdateOrderShipmentState` endpoint; add Shipping admin endpoints (create/advance/cancel/backorder, set tracking number, list shipments for an order).
- **CON-001**: Modules communicate only via MediatR `ISender`; `Shipment` references `OrderId` by Guid (no cross-assembly navigation).
- **CON-002**: Vertical-slice feature files; domain logic in `ShippingMethod`-style partial classes.
- **CON-003**: Result objects, not exceptions; zero-warning build.
- **PAT-001**: Cross-module mirror follows `RecordOrderPaymentStateCommand`'s best-effort pattern.

## 2. Implementation Steps

### Implementation Phase 1 — Shipment domain

- GOAL-001: `Shipment` entity + `ShipmentStatus` + transition methods + EF config + migration.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | `Shipment` entity + `ShipmentStatus` enum + `ShipmentMethod` transitions. | | |
| TASK-002 | EF config (`ShipmentConfiguration`) + migration `AddShipment`. | | |
| TASK-003 | Domain tests for transitions (Ready/Shipped/Delivered/Backorder/Cancel + tracking-number-at-Shipped rule). | | |

#### TASK-001: Shipment entity + lifecycle

**Files:**
- Create: `service/Api/src/Module/Shipping/Domain/Shipments/Shipment.cs`
- Create: `service/Api/src/Module/Shipping/Domain/Shipments/Shipment.Enumerate.cs`
- Create: `service/Api/src/Module/Shipping/Domain/Shipments/Shipment.Method.State.cs`
- Create: `service/Api/src/Module/Shipping/Domain/Shipments/Shipment.Method.Factory.cs`
- Create: `service/Api/src/Module/Shipping/Domain/Shipments/Shipment.Result.cs`

```csharp
// Shipment.Enumerate.cs
public enum ShipmentStatus { Pending, Ready, Shipped, Delivered, Backorder, Canceled }

// Shipment.Method.Factory.cs
public static Result<Shipment> Create(Guid orderId, Guid shippingMethodId)
// sets Status = Pending, CreatedAtUtc = UtcNow

// Shipment.Method.State.cs (transitions — all return Result)
// MarkReady():    Pending|Backorder -> Ready
// MarkShipped(string trackingNumber): Ready -> Shipped (trackingNumber required), ShippedAtUtc = UtcNow
// MarkDelivered(): Shipped -> Delivered, DeliveredAtUtc = UtcNow
// Backorder():    Pending -> Backorder
// Cancel():       Pending|Ready|Backorder -> Canceled
// Restock():      Backorder -> Ready
```

Shipment references `ShippingMethod` (same module) with a `ShippingMethodId` Guid + navigation.

#### TASK-002: EF config + migration

**Files:**
- Create: `service/Api/src/Module/Shipping/Persistence/Configurations/Shipments/ShipmentConfiguration.cs`

Map to `shipping.shipments`; `Status` `.HasConversion<string>()`; indexes on `OrderId` and `(OrderId, Status)`. Then:

```bash
dotnet ef migrations add AddShipment \
  --project service/Api/src/Migrations/Api.Migrations.csproj \
  --startup-project service/Api/src/Api/Api.csproj
```

#### TASK-003: Domain tests

Create `service/Api/tests/Module.UnitTests/Shipping/Domain/Shipments/Shipment.Method.Tests.cs` covering every transition + the invalid ones (`MarkShipped` with empty tracking number, `MarkDelivered` before `Shipped`, `Cancel` after `Delivered`).

### Implementation Phase 2 — Derived fulfillment state (Ordering)

- GOAL-002: Rename `OrderShipmentState` → `OrderFulfillmentState`; add the sync command; derive on the Order.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | Rename `OrderShipmentState` → `OrderFulfillmentState` (derived values) across Ordering. | | |
| TASK-005 | Add `RecordOrderShipmentStateCommand` + handler (Ordering). | | |
| TASK-006 | Add `OrderMethod.ApplyFulfillmentState` + timestamp mirrors. | | |

#### TASK-004: Rename to derived `OrderFulfillmentState`

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Enumerate.cs:38-46`

```csharp
public enum OrderFulfillmentState { None, Pending, Partial, Shipped, Delivered, Canceled }
```

Rename `Order.ShipmentState` → `Order.FulfillmentState` (typed `OrderFulfillmentState?`) and update every reference. Verified full reference set:
- `Domain/Orders/Order.cs:32`
- `Domain/Orders/Order.Enumerate.cs:38-46`
- `Domain/Orders/Order.Method.Checkout.cs:26-28` (`AllowCancel`)
- `Domain/Orders/Order.Result.cs:62,321-322`
- `Persistence/Configurations/OrderConfiguration.cs:33`
- `Features/Shared/OrderingFeature.Admin.cs:36`
- `Features/Admin/Orders/Shared/Models/Order.Model.Response.cs:18,38`
- `Features/Admin/Orders/Shared/Mappings/Order.Mapping.Model.cs:37,69`
- `Features/Admin/Orders/UpdateShipmentState/*` (removed in TASK-010)

#### TASK-005: Sync command

**Files:**
- Create: `service/Api/src/Module/Ordering/Features/Storefront/RecordOrderShipmentState/RecordOrderShipmentState.Command.cs`
- Create: `service/Api/src/Module/Ordering/Features/Storefront/RecordOrderShipmentState/RecordOrderShipmentState.cs`

```csharp
public sealed record RecordOrderShipmentStateCommand : ICommand
{
    public Guid OrderId { get; init; }
    public OrderFulfillmentState FulfillmentState { get; init; }
    public DateTimeOffset? ShippedAtUtc { get; init; }
    public DateTimeOffset? DeliveredAtUtc { get; init; }
}
// handler: load Order by Id, set FulfillmentState + mirror ShippedAt/DeliveredAt (first-write), SaveChanges
```

#### TASK-006: Derived-state computation helper

Add `ShipmentMethod.ComputeFulfillmentState(IReadOnlyCollection<ShipmentStatus>) → OrderFulfillmentState` in Shipping (None if empty; Partial if some Shipped/Delivered and some not; Shipped if all Shipped; Delivered if all Delivered; Canceled if all Canceled; Pending otherwise).

### Implementation Phase 3 — Wiring

- GOAL-003: Auto-create on placement, admin endpoints, sync on change, remove old endpoint.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | `CreateShipmentCommand` (Shipping) + call from `CheckoutPlacementService`. | | |
| TASK-008 | Shipping admin endpoints (advance/set-tracking/cancel/backorder + list). | | |
| TASK-009 | Each shipment change sends `RecordOrderShipmentStateCommand`. | | |
| TASK-010 | Remove Ordering `UpdateOrderShipmentState` + its route constant. | | |

#### TASK-007: Auto-create

`CheckoutPlacementService` currently has **no** `ISender` injected (`service/Api/src/Module/Ordering/Services/CheckoutPlacementService.cs:11-15`) — add `ISender` to its primary constructor. After `cart.Place(...)` succeeds, send `CreateShipmentCommand { OrderId = cart.Id, ShippingMethodId = cart.ShippingMethodId }`. `CreateShipmentCommand` is defined in Shipping (`Features/Shared/Commands/CreateShipment.cs`); Ordering references it (same pattern as Billing → Ordering commands).

#### TASK-008/009: Admin endpoints + sync

Create Shipping admin feature slices (create/advance/list shipments). After each status change, the handler recomputes `ComputeFulfillmentState` over the order's shipments and sends `RecordOrderShipmentStateCommand`.

#### TASK-010: Remove old endpoint

Delete `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateShipmentState/*` and its `OrderingFeature.Admin.Orders.UpdateShipmentState` route constant. Update the Admin SPA to use the new Shipping shipment endpoints.

### Implementation Phase 4 — SPA + tests

- GOAL-004: Admin SPA shipment UI + end-to-end tests.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | Admin SPA: shipment list/status control + tracking input. | | |
| TASK-012 | Integration tests (auto-create on placement; sync command updates order). | | |

## 3. Alternatives

- **ALT-001**: Keep `Order.ShipmentState` admin-set and skip the `Shipment` aggregate. Rejected — no tracking number/method snapshot/derivation.
- **ALT-002**: Derive fulfillment on read (query Shipping) instead of caching on Order. Rejected — order-list filtering/sorting would become cross-module.

## 4. Dependencies

- **DEP-001**: `refactor-status-value-converters-1` (the transient `ShipmentState` converter it adds is replaced here).
- **DEP-002**: `IStockReservationService` and `CheckoutPlacementService` (Ordering).

## 5. Files

- **FILE-001**: `service/Api/src/Module/Shipping/Domain/Shipments/*` (new).
- **FILE-002**: `service/Api/src/Module/Shipping/Persistence/Configurations/Shipments/ShipmentConfiguration.cs` (new).
- **FILE-003**: `service/Api/src/Module/Ordering/Domain/Orders/Order.Enumerate.cs` (rename enum).
- **FILE-004**: `service/Api/src/Module/Ordering/Features/Storefront/RecordOrderShipmentState/*` (new).
- **FILE-005**: `service/Api/src/Module/Ordering/Services/CheckoutPlacementService.cs` (auto-create).
- **FILE-006**: `service/Api/src/Module/Shipping/Features/Admin/Shipments/*` (new).
- **FILE-007**: `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateShipmentState/*` (delete).

## 6. Testing

- **TEST-001**: `ShipmentMethod` transition tests (TASK-003).
- **TEST-002**: `ComputeFulfillmentState` unit tests (None/Partial/Shipped/Delivered/Canceled/Pending).
- **TEST-003**: `RecordOrderShipmentState` handler writes FulfillmentState + mirrors.
- **TEST-004**: Placement auto-creates a Pending shipment (integration).
- **TEST-005**: `dotnet test service/Api/tests/Module.UnitTests` green; `bash scripts/check-feature-conventions.sh` passes.

## 7. Risks & Assumptions

- **RISK-001**: This introduces the first Ordering→Shipping **ISender command** (direct Ordering→Shipping domain references already exist — `Order.ShippingMethod` nav, `Order.Seeder`, `UpdateCheckout`, `SelectShippingRate` — among the 39 known isolation violations); keep it to the lightweight record+handler pattern.
- **ASSUMPTION-001**: v1 shipments are whole-order (no item-level lines); split is deferred (spec §3.8 P1-1).

## 8. Related Specifications / Further Reading

- [spec-checkout-state-enum-alignment.md](../spec/spec-checkout-state-enum-alignment.md) §3.5, §4.2, §4.7
- [refactor-status-value-converters-1.md](./refactor-status-value-converters-1.md)
