# Ordering Domain

DDD domain models: Orders, LineItems, Adjustments with events.

## Aggregates

| Aggregate | Path | Key Events |
|-----------|------|-----------|
| Orders | `Orders/` | OrderPlaced, OrderCancelled, OrderCompleted |
| LineItems | `LineItems/` | LineItemAdded, LineItemRemoved |
| Adjustments | `Adjustments/` | Adjustment applied/removed |

## Category

Domain-Driven Design · Ordering
