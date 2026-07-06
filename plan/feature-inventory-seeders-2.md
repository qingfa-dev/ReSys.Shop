---
goal: Create Inventory seeders connected to Catalog and Location seeders
version: 1.0
date_created: 2026-07-07
status: Completed
last_updated: 2026-07-07
tags: feature, inventory, seeders, cross-module
---

# Introduction

![Status: In progress](https://img.shields.io/badge/status-In%20progress-yellow)

The Inventory module has a single `StockLocationSeeder` but creates no `StockItem` records for the demo product variant seeded by `CatalogDemoSeeder`. This means the storefront shows 0 stock for the only product. This plan adds seeders that wire Inventory data to existing Catalog (variants by SKU) and Location (countries/states) seeders.

## 1. Requirements & Constraints

- **REQ-001**: Create `StockItem` records for demo variants from `CatalogDemoSeeder`
- **REQ-002**: Link `StockItem` records to the default `StockLocation` from `StockLocationSeeder`
- **REQ-003**: Create `StockMovement` audit trail for initial stock seeding
- **REQ-004**: Enhance `StockLocationSeeder` to reference `Country`/`State` from Location seeders
- **CON-001**: All seeders must be idempotent (skip if data exists)
- **CON-002**: Follow existing seeder pattern: `AbstractDataSeeder`, `Order`, `HasDataAsync<T>()` guard
- **CON-003**: Use domain factory methods (`StockItemMethod.Create`, `StockMovementMethod.Create`) — do not construct entities directly
- **CON-004**: Respect execution order — StockLocationSeeder (100), CatalogDemoSeeder (130), InventoryStockItemSeeder (140), InventoryStockMovementSeeder (150)

## 2. Implementation Steps

### Implementation Phase 1: Enhance StockLocationSeeder with country reference

- GOAL-001: Make the default warehouse address reference the US country for a more realistic demo

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Modify `StockLocation.Seeder.cs` to look up US Country by IsoCode "US" and set `CountryId` on the default warehouse | ✅ | 2026-07-07 |
| TASK-002 | Build and verify no regressions | ✅ | 2026-07-07 |

### Implementation Phase 2: Create InventoryStockItemSeeder

- GOAL-002: Create StockItem records for each demo variant from CatalogDemoSeeder

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | Create `InventoryStockItem.Seeder.cs` at Order 140 | ✅ | 2026-07-07 |
| TASK-004 | In `SeedAsync`: guard via `HasDataAsync<StockItem>()`, skip if stock exists | ✅ | 2026-07-07 |
| TASK-005 | Look up variant by SKU "TEE-COTTON-001" from `Context.Set<Variant>()` | ✅ | 2026-07-07 |
| TASK-006 | Look up default stock location (`.FirstOrDefault(sl => sl.Default)`) | ✅ | 2026-07-07 |
| TASK-007 | Create StockItem with `countOnHand: 50`, `backorderable: true` | ✅ | 2026-07-07 |
| TASK-008 | Register `InventoryStockItemSeeder` in `Inventory.Extension.cs` via `builder.AddSeeder<InventoryStockItemSeeder>()` | ✅ | 2026-07-07 |
| TASK-009 | Build and verify | ✅ | 2026-07-07 |

### Implementation Phase 3: Create InventoryStockMovementSeeder

- GOAL-003: Create initial StockMovement audit trail for seeded stock items

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | Create `InventoryStockMovement.Seeder.cs` at Order 150 | ✅ | 2026-07-07 |
| TASK-011 | In `SeedAsync`: guard via `HasDataAsync<StockMovement>()`, skip if movements exist | ✅ | 2026-07-07 |
| TASK-012 | Look up the seeded StockItem by variant SKU "TEE-COTTON-001" and default stock location | ✅ | 2026-07-07 |
| TASK-013 | Call `StockMovementMethod.Create(stockItemId, quantity: 50, previousCountOnHand: 0, originatorType: "Adjustment", reason: "Initial stock")` | ✅ | 2026-07-07 |
| TASK-014 | Register `InventoryStockMovementSeeder` in `Inventory.Extension.cs` | ✅ | 2026-07-07 |
| TASK-015 | Build and verify | ✅ | 2026-07-07 |

### Implementation Phase 4: Verification

- GOAL-004: Verify all seeders run correctly together

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-016 | Run `dotnet build` — 0 warnings, 0 errors | ✅ | 2026-07-07 |
| TASK-017 | Run `dotnet test tests/Module.UnitTests` — all tests pass | ✅ | 2026-07-07 |
| TASK-018 | Verify end-to-end: seeders run in order 10→20→30→40→50→60→100→110→120→130→140→150 | ✅ | 2026-07-07 |

## 3. Alternatives

- **ALT-001**: Skip StockMovement seeder entirely and rely on system auto-creation. Rejected because the initial stock count has no audit trail without it, making demo less realistic.
- **ALT-002**: Seed multiple products/variants in CatalogDemoSeeder instead of creating Inventory seeders. Rejected because it blurs module boundaries — Catalog seeders should not create Inventory data.

## 4. Dependencies

- **DEP-001**: `CatalogDemoSeeder` (Order 130) must run before `InventoryStockItemSeeder` (Order 140) — ensured by Order values
- **DEP-002**: `CountrySeeder` (Order 10) must run before enhanced `StockLocationSeeder` (Order 100) — already satisfied
- **DEP-003**: `StockLocationSeeder` (Order 100) must run before `InventoryStockItemSeeder` (Order 140) — ensured by Order values
- **DEP-004**: `InventoryStockItemSeeder` (Order 140) must run before `InventoryStockMovementSeeder` (Order 150) — ensured by Order values

## 5. Files

- **FILE-001**: `service/Api/src/Module/Inventory/Persistence/Seeders/StockLocation.Seeder.cs` — modify to add CountryId reference
- **FILE-002**: `service/Api/src/Module/Inventory/Persistence/Seeders/InventoryStockItem.Seeder.cs` — new file
- **FILE-003**: `service/Api/src/Module/Inventory/Persistence/Seeders/InventoryStockMovement.Seeder.cs` — new file
- **FILE-004**: `service/Api/src/Module/Inventory/Inventory.Extension.cs` — register new seeders

## 6. Testing

- **TEST-001**: All 2046+ existing unit tests continue passing
- **TEST-002**: New seeder idempotency — running twice produces same result (verified by `HasDataAsync` guard pattern)
- **TEST-003**: Build passes with `TreatWarningsAsErrors=true`

## 7. Risks & Assumptions

- **RISK-001**: If `CatalogDemoSeeder` changes the SKU from "TEE-COTTON-001", the InventoryStockItemSeeder will silently skip (variant not found → returns Ok). The lookup must match exactly.
- **RISK-002**: If the `Country` entity with IsoCode "US" is removed from `CountrySeeder`, the enhanced StockLocationSeeder will silently create a location without a country reference.
- **ASSUMPTION-001**: The `DatabaseInitializer` resolves seeders by `Order` and runs them sequentially. The order chain 10→150 is sufficient.

## 8. Related Specifications / Further Reading

- `/home/qingfa/Repos/ReSys.Shop/plan/feature-inventory-mvp-cut-1.md` — MVP plan that created StockLocationSeeder
- `/home/qingfa/Repos/ReSys.Shop/service/Api/src/Shared/Operational/Persistence/Seeders/Seeder.Abstract.cs` — AbstractDataSeeder base class
- `/home/qingfa/Repos/ReSys.Shop/service/Api/src/Shared/Operational/Persistence/Seeders/Seeder.Extension.cs` — AddSeeder registration extension
