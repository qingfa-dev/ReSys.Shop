---
goal: Inventory MVP cut — wire module + StockLocation + StockItem + CartReservations + StockAvailability.Check
version: 1.0
date_created: 2026-07-07
status: Completed
tags: feature, inventory, mvp, plan
---

# Inventory MVP Cut — Plan

Based on selections: Admin stock mgmt + storefront availability check (smallest shippable),
transfers deferred, backorder/restock deferred, audit-writes-only, no CSV import.

---

## 0. Critical pre-requisites (do these first, no MVP without them)

| # | Task | Status |
|---|---|---|
| P0-1 | Call `builder.AddInventoryModule()` in `service/Api/src/Api/Program.cs` after `AddCatalogModule()` | |
| P0-2 | Add migration `AddInventoryModuleEntities` under `service/Api/src/Migrations/Migrations/` | |
| P0-3 | Implement missing `Features/Storefront/CartReservations/Release/*.cs` handler | |
| P0-4 | Verify Carter discovery picks up Inventory features | |

---

## 1. MVP entities

| Entity | Status | Notes |
|---|---|---|
| `StockLocation` | IN | Required by `StockItem`. Includes `SetDefault`, `Active` flag, `LowStockThreshold`. |
| `StockItem` | IN | Per-(Variant, StockLocation) row. `CountOnHand` + `Backorderable` flag. |
| `StockReservation` | IN | Cart-hold only. Anonymous; keyed by `CartToken`. |
| `StockMovement` | IN (write-only) | Table created; rows emitted by `BulkAdjust` and reservation-expiry service. No admin read endpoints. |
| `StockTransfer` | DEFER | AR + `TransferItem` child + all 6 admin endpoints. |
| `TransferItem` | DEFER | Subsumed by parent. |

---

## 2. MVP features

### 2.1 Admin

| Endpoint | Status |
|---|---|
| `POST /stock-locations` Create | IN |
| `GET /stock-locations` GetPaged | IN |
| `GET /stock-locations/{id}` GetById | IN |
| `PUT /stock-locations/{id}` Update | IN |
| `POST /stock-locations/{id}/default` SetDefault | IN |
| `DELETE /stock-locations/{id}` | DEFER |
| `POST /stock-items` Create | IN |
| `GET /stock-items` GetPaged | IN |
| `GET /stock-items/{id}` GetById | IN |
| `PUT /stock-items/{id}` Update | IN |
| `POST /stock-items/bulk-adjust` BulkAdjust | IN |
| `POST /stock-items/{id}/restock` Restock | DEFER |
| `GET /stock-items/low-stock` LowStock | DEFER |
| `GET /stock-items/summary` Summary | DEFER |
| `POST /stock-items/import` Import | DEFER |
| `DELETE /stock-items/{id}` | DEFER |
| `GET /stock-movements` GetPaged | DEFER |
| `GET /stock-movements/{id}` GetById | DEFER |
| `GET /stock-reservations` GetPaged | DEFER |
| `GET /stock-reservations/{id}` GetById | DEFER |
| `POST /stock-reservations/{id}/cancel` Cancel | DEFER |

### 2.2 Storefront

| Endpoint | Status |
|---|---|
| `GET /availability/{variantId}` StockAvailability.Check | IN |
| `POST /cart/reserve` CartReservations.Reserve | IN |
| `GET /cart/reserve` CartReservations.Status | IN |
| `DELETE /cart/reserve/{reservationId}` CartReservations.Release | IN (P0-3) |

### 2.3 Background service

| Service | Status |
|---|---|
| `ReservationExpiryService` (HostedService) | IN |

---

## 3. Execution order

1. P0-1: Wire `AddInventoryModule()` in `Program.cs`
2. P0-2: Generate `AddInventoryModuleEntities` migration
3. P0-3: Implement `CartReservations.Release` handler + unit test
4. P0-4: Verify Carter discovery
5. Verification: `dotnet build`, `dotnet test service/Api/tests/Module.UnitTests`
6. Post-MVP: admin UI, StockLocationSeeder, integration tests, Restock/Transfers/LowStock
