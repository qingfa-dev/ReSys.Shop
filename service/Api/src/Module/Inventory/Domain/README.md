# Inventory Domain

DDD domain models for inventory: Stock, StockLocations, StockReservations.

## Aggregates

| Aggregate | Path | Purpose |
|-----------|------|---------|
| Stock | `Stock/` | Overall stock state and availability validation |
| StockLocations | `StockLocations/` | Warehouse/location management (parent aggregate) |
| └─ StockItems | `StockLocations/StockItems/` | Per-product stock tracking at a location |
| &nbsp;&nbsp;&nbsp;└─ StockMovements | `StockLocations/StockItems/StockMovements/` | Stock in/out audit trail per stock item |
| StockReservations | `StockReservations/` | Order reservation tracking (independent aggregate) |

## Services

| Service | Path | Purpose |
|---------|------|---------|
| StockChecker | `../Services/StockChecker.cs` | EF-dependent stock queries, reservations, and adjustments |

## Category

Domain-Driven Design · Inventory
