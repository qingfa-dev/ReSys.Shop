# Task 2 Report: Remove Cross-Module Entity Navigation Properties

## Summary

Removed 4 cross-module navigation properties from domain entities and fixed all downstream compilation errors. Build passes with 0 warnings, 0 errors.

## Changes

### Entity Nav Property Removals

| File | Change |
|------|--------|
| `Variant.cs` | Removed `ICollection<StockItem> StockItems`, removed Inventory using |
| `StockItem.cs` | Removed `Variant Variant`, removed Catalog using |
| `Order.cs` | Removed `ICollection<PaymentCapture> Payments`, removed Payment using |
| `LineItem.cs` | Removed `Variant Variant` |

### EF Configuration Removals

| File | Change |
|------|--------|
| `VariantConfiguration.cs` | Removed `HasMany(x => x.StockItems)...WithOne(si => si.Variant)` |
| `StockItemConfiguration.cs` | Removed `HasOne(x => x.Variant)...WithMany(v => v.StockItems)` |
| `LineItemConfiguration.cs` | Removed `HasOne(x => x.Variant)` |
| `OrderConfiguration.cs` | Removed `HasMany(x => x.Payments)` |

### Handler Fixes (Nav Access → Explicit Query)

| File | Change |
|------|--------|
| `GetCart.cs` | Removed `.ThenInclude(x => x.Variant)`; query variants separately via `dbContext.Set<Variant>()` |
| `CreateOrderFromCart.cs` | Removed `.ThenInclude(x => x.Variant)`; query discontinued variants via `dbContext.Set<Variant>().Where(v => v.DiscontinuedOn != null)` |

### Domain Service Fixes (Removed PaymentCapture Dependencies)

| File | Change |
|------|--------|
| `Order.Payments.cs` | **Deleted** — `ProcessPayments()`, `HasUnprocessedPayments`, `GetUnprocessedPayments` were unused |
| `Order.Extensions.cs` | Simplified `UpdatePaymentState()` — removed `PaymentCapture` parameter, uses only status+balance |
| `OrderUpdater.cs` | Simplified `UpdatePaymentTotal()`/`UpdatePaymentState()` — removed `PaymentCapture` parameter |
| `Order.Seeder.cs` | Removed `order.Payments.Add(payment)` |

## Verification

- **Build**: `dotnet build` — 0 warnings, 0 errors (all 9 projects)
- **Architecture test**: Catalog↔Inventory and Ordering→Catalog/Payment navigation violations removed
- **Remaining test failures**: 3 pre-existing Identity test failures; 1 architecture test failure from pre-existing seeder/service cross-references (out of scope)
