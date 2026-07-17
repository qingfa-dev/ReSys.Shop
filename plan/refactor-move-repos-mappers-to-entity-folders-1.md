---
goal: Move repository and mapper files into per-entity `repositories/` and `mappers/` sub-directories inside existing entity folders, and create entity folders for flat modules
version: 1.0
date_created: 2026-07-17
status: 'Planned'
tags: refactor, admin-spa, repositories, mappers
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

All 20 repository files currently live in flat `{module}/repository/` directories and all 7 mapper files live in flat `{module}/mapper/` directories, even when their corresponding entities have been restructured into per-entity sub-folders (e.g. `catalog/products/`, `catalog/taxonomies/taxa/`, `ordering/fulfillment/`). This plan moves each repository and mapper into its entity's folder under `repositories/` and `mappers/` sub-directories — at the same level as `services/`, `stores/`, `schemas/`, `types/`, and `views/`. Flat modules (inventories, location) get new entity sub-folders created as part of the move.

## 1. Requirements & Constraints

- **REQ-001**: Every repository file must be moved from `{module}/repository/{entity}.repository.ts` to `{module}/{entity-folder}/repositories/{entity}.repository.ts`
- **REQ-002**: Every per-entity mapper function must be moved from `{module}/mapper/{module}.mapper.ts` to `{module}/{entity-folder}/mappers/{entity}.mapper.ts`
- **REQ-003**: All import paths in consumers (services, stores, tests) must be updated after each move
- **REQ-004**: Sub-directory names use plural form: `repositories/`, `mappers/`
- **REQ-005**: No behavior change — repository/mapper function signatures and exports remain identical; only file locations change
- **REQ-006**: Dead mappers (not imported anywhere) are deleted rather than moved
- **CON-001**: Modules that already have per-entity sub-folders (catalog, ordering, users) get repos/mappers moved directly into their existing entity folders
- **CON-002**: Flat modules (inventories, location) get entity sub-folders created as part of this plan; other files (schemas, types, services, stores, views) remain at module root
- **CON-003**: Auth and Profile are single-entity modules where `repository/` and `mapper/` already live at the entity level — they are left in place
- **CON-004**: Cross-module repos in `identity/repository/` (user.repository.ts, role.repository.ts, permission.repository.ts) are moved into `users/` entity folders since all consumers are in `users/`
- **PAT-001**: Follow the established entity folder pattern: `{entity}/repositories/`, `{entity}/mappers/`, `{entity}/services/`, `{entity}/stores/`, etc.
- **GUD-001**: Each phase must pass `pnpm run type-check` and `pnpm run lint` independently

## 2. Implementation Steps

### Phase 1 — Catalog: Move 7 repos, delete 1 dead mapper

- GOAL-001: Move all catalog repositories into their respective entity folders and delete the dead catalog.mapper.ts

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `catalog/products/repositories/`, move `catalog/repository/product.repository.ts` into it; update import in `catalog/products/services/product.service.ts` from `../../repository/product.repository` to `../repositories/product.repository` | | |
| TASK-002 | Create `catalog/products/repositories/`, move `catalog/repository/variant.repository.ts` into it; update import in `catalog/products/services/variant.service.ts` from `../../repository/variant.repository` to `../repositories/variant.repository` | | |
| TASK-003 | Create `catalog/option-types/repositories/`, move `catalog/repository/option-type.repository.ts` into it; update import in `catalog/option-types/services/option-type.service.ts` from `../../repository/option-type.repository` to `../repositories/option-type.repository` | | |
| TASK-004 | Create `catalog/option-types/option-values/repositories/`, move `catalog/repository/option-value.repository.ts` into it; update import in `catalog/option-types/option-values/services/option-value.service.ts` from `../../../repository/option-value.repository` to `../repositories/option-value.repository` | | |
| TASK-005 | Create `catalog/property-types/repositories/`, move `catalog/repository/property-type.repository.ts` into it; update import in `catalog/property-types/services/property-type.service.ts` from `../../repository/property-type.repository` to `../repositories/property-type.repository` | | |
| TASK-006 | Create `catalog/taxonomies/repositories/`, move `catalog/repository/taxonomy.repository.ts` into it; update imports in `catalog/taxonomies/services/taxonomy.service.ts` (from `../../repository/taxonomy.repository` to `../repositories/taxonomy.repository`) and `catalog/taxonomies/stores/taxonomy.store.ts` (line 5) | | |
| TASK-007 | Create `catalog/taxonomies/taxa/repositories/`, move `catalog/repository/taxon.repository.ts` into it; update import in `catalog/taxonomies/taxa/services/taxon.service.ts` from `../../../repository/taxon.repository` to `../repositories/taxon.repository` | | |
| TASK-008 | Delete `catalog/mapper/catalog.mapper.ts` (dead code — not imported anywhere); update import in `catalog/_tests/catalog.api.spec.ts`: change all 7 import paths from `'../repository/...'` to their new entity-relative paths (product: `../products/repositories/product.repository`, variant: `../products/repositories/variant.repository`, option-type: `../option-types/repositories/option-type.repository`, option-value: `../option-types/option-values/repositories/option-value.repository`, property-type: `../property-types/repositories/property-type.repository`, taxonomy: `../taxonomies/repositories/taxonomy.repository`, taxon: `../taxonomies/taxa/repositories/taxon.repository`) | | |
| TASK-009 | Remove empty `catalog/repository/` and `catalog/mapper/` directories | | |
| TASK-010 | Run `pnpm run type-check && pnpm run lint` and fix any issues | | |

### Phase 2 — Ordering: Move 2 repos, delete 1 dead mapper, create orders/ entity folder

- GOAL-002: Move order repository into a new orders/ entity folder, move fulfillment repository into existing fulfillment/ folder, delete dead mapper

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | Create `ordering/orders/` entity folder with `repositories/` sub-directory; move `ordering/repository/order.repository.ts` into `ordering/orders/repositories/order.repository.ts`; update types import path inside the repository file from `'../types/Order.Response.Type'` to `'../../types/Order.Response.Type'` and from `'../types/Order.Request.Type'` to `'../../types/Order.Request.Type'` (types remain at ordering/ root level) | | |
| TASK-012 | Update import in `ordering/services/order.service.ts` from `'../repository/order.repository'` to `'../orders/repositories/order.repository'` | | |
| TASK-013 | Update import in `ordering/fulfillment/services/fulfillment.service.ts` from `'../../repository/order.repository'` to `'../../orders/repositories/order.repository'` | | |
| TASK-014 | Create `ordering/fulfillment/repositories/`, move `ordering/repository/fulfillment.repository.ts` into `ordering/fulfillment/repositories/fulfillment.repository.ts`; update types import inside from `'../types/Order.Response.Type'` to `'../../types/Order.Response.Type'` | | |
| TASK-015 | Update import in `ordering/fulfillment/services/fulfillment.service.ts` from `'../../repository/fulfillment.repository'` to `'../repositories/fulfillment.repository'` | | |
| TASK-016 | Delete `ordering/mapper/ordering.mapper.ts` (dead code — not imported anywhere) | | |
| TASK-017 | Update imports in `ordering/_tests/ordering.api.spec.ts`: change `'../repository/order.repository'` to `'../orders/repositories/order.repository'` and `'../repository/fulfillment.repository'` to `'../fulfillment/repositories/fulfillment.repository'` | | |
| TASK-018 | Remove empty `ordering/repository/` and `ordering/mapper/` directories | | |
| TASK-019 | Run `pnpm run type-check && pnpm run lint` and fix any issues | | |

### Phase 3 — Users/Identity: Move 3 cross-module repos, delete 1 dead mapper

- GOAL-003: Move identity/repository/*.repository.ts files into users/ entity folders (where their consumers live), delete dead identity mapper

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-020 | Create `users/repositories/`, move `identity/repository/user.repository.ts` into `users/repositories/user.repository.ts`; update imports inside the file from `'../../users/types/User.Response.Type'` to `'../types/User.Response.Type'` and from `'../../users/types/User.Request.Type'` to `'../types/User.Request.Type'` | | |
| TASK-021 | Update import in `users/services/user.service.ts` from `'../../identity/repository/user.repository'` to `'../repositories/user.repository'` | | |
| TASK-022 | Create `users/roles/repositories/`, move `identity/repository/role.repository.ts` into `users/roles/repositories/role.repository.ts`; update imports inside the file from `'../../users/roles/types/Role.Response.Type'` to `'../types/Role.Response.Type'` and from `'../../users/roles/types/Role.Request.Type'` to `'../types/Role.Request.Type'` | | |
| TASK-023 | Update import in `users/services/role.service.ts` from `'../../identity/repository/role.repository'` to `'../roles/repositories/role.repository'` | | |
| TASK-024 | Create `users/permissions/repositories/`, move `identity/repository/permission.repository.ts` into `users/permissions/repositories/permission.repository.ts`; update import inside the file from `'../../users/permissions/types/Permission.Response.Type'` to `'../types/Permission.Response.Type'` | | |
| TASK-025 | Update import in `users/services/permission.service.ts` from `'../../identity/repository/permission.repository'` to `'../permissions/repositories/permission.repository'` | | |
| TASK-026 | Delete `identity/mapper/identity.mapper.ts` (dead code — not imported anywhere) | | |
| TASK-027 | Remove empty `identity/repository/` and `identity/mapper/` directories | | |
| TASK-028 | Run `pnpm run type-check && pnpm run lint` and fix any issues | | |

### Phase 4 — Inventories: Create 5 entity folders, move 5 repos & split mapper

- GOAL-004: Create per-entity folders for all 5 inventory entities (stock-items, stock-locations, stock-transfers, stock-movements, inventory-units), move respective repos into them, split the single inventory.mapper.ts into per-entity mapper files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-029 | Create `inventories/stock-items/` with `repositories/` and `mappers/` sub-directories; move `inventories/repository/stock.repository.ts` → `inventories/stock-items/repositories/stock.repository.ts`; update import inside from `'../types/StockItem.Response.Type'` to `'../../types/StockItem.Response.Type'`, `'../types/StockItem.Request.Type'` to `'../../types/StockItem.Request.Type'`, `'../types/StockItem.Query.Type'` to `'../../types/StockItem.Query.Type'` (types remain at inventories/ root) | | |
| TASK-030 | Create `inventories/stock-items/mappers/stock-item.mapper.ts` with `mapStockItem` function extracted from `inventories/mapper/inventory.mapper.ts` | | |
| TASK-031 | Create `inventories/stock-locations/` with `repositories/` and `mappers/` sub-directories; move `inventories/repository/location.repository.ts` → `inventories/stock-locations/repositories/location.repository.ts`; update import inside from `'../types/StockLocation.Response.Type'` to `'../../types/StockLocation.Response.Type'` and `'../types/StockLocation.Request.Type'` to `'../../types/StockLocation.Request.Type'` | | |
| TASK-032 | Create `inventories/stock-locations/mappers/stock-location.mapper.ts` with `mapStockLocation` function extracted from `inventories/mapper/inventory.mapper.ts` | | |
| TASK-033 | Create `inventories/stock-transfers/` with `repositories/` and `mappers/` sub-directories; move `inventories/repository/transfer.repository.ts` → `inventories/stock-transfers/repositories/transfer.repository.ts`; update import inside from `'../types/StockTransfer.Response.Type'` to `'../../types/StockTransfer.Response.Type'` and `'../types/StockTransfer.Request.Type'` to `'../../types/StockTransfer.Request.Type'` | | |
| TASK-034 | Create `inventories/stock-transfers/mappers/stock-transfer.mapper.ts` with `mapStockTransfer` function extracted from `inventories/mapper/inventory.mapper.ts` | | |
| TASK-035 | Create `inventories/stock-movements/` with `repositories/` and `mappers/` sub-directories; move `inventories/repository/movement.repository.ts` → `inventories/stock-movements/repositories/movement.repository.ts`; update import inside from `'../types/StockMovement.Response.Type'` to `'../../types/StockMovement.Response.Type'` | | |
| TASK-036 | Create `inventories/stock-movements/mappers/stock-movement.mapper.ts` with `mapStockMovement` function extracted from `inventories/mapper/inventory.mapper.ts` | | |
| TASK-037 | Create `inventories/inventory-units/` with `repositories/` and `mappers/` sub-directories; move `inventories/repository/reservation.repository.ts` → `inventories/inventory-units/repositories/reservation.repository.ts`; update import inside from `'../types/InventoryUnit.Response.Type'` to `'../../types/InventoryUnit.Response.Type'` | | |
| TASK-038 | Create `inventories/inventory-units/mappers/inventory-unit.mapper.ts` with `mapInventoryUnit` function extracted from `inventories/mapper/inventory.mapper.ts` | | |
| TASK-039 | Update all 5 repository imports in `inventories/services/inventory.service.ts`: `'../repository/stock.repository'` → `'../stock-items/repositories/stock.repository'`, `'../repository/location.repository'` → `'../stock-locations/repositories/location.repository'`, `'../repository/reservation.repository'` → `'../inventory-units/repositories/reservation.repository'`, `'../repository/transfer.repository'` → `'../stock-transfers/repositories/transfer.repository'`, `'../repository/movement.repository'` → `'../stock-movements/repositories/movement.repository'`; update mapper imports from `'../mapper/inventory.mapper'` to individual entity mapper paths | | |
| TASK-040 | Update imports in `inventories/_tests/inventory.api.spec.ts`: `'../repository/stock.repository'` → `'../stock-items/repositories/stock.repository'`, `'../repository/location.repository'` → `'../stock-locations/repositories/location.repository'`, `'../repository/transfer.repository'` → `'../stock-transfers/repositories/transfer.repository'` | | |
| TASK-041 | Delete `inventories/mapper/inventory.mapper.ts` after all 5 per-entity mappers are created | | |
| TASK-042 | Remove empty `inventories/repository/` and `inventories/mapper/` directories | | |
| TASK-043 | Run `pnpm run type-check && pnpm run lint` and fix any issues | | |

### Phase 5 — Location: Create 2 entity folders, split 1 repo & 1 mapper

- GOAL-005: Create countries/ and states/ entity folders, split the combined location.repository.ts into per-entity repos, split location.mapper.ts into per-entity mappers

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-044 | Create `location/countries/` with `repositories/` and `mappers/` sub-directories; create `location/countries/repositories/country.repository.ts` with the `countries` sub-object methods from `location/repository/location.repository.ts` flattened to top-level: `list`, `getById`, `create`, `update`, `delete` — with types referencing `../../types/Country.Response.Type` and `../../types/Country.Request.Type`; use `import { LOCATIONS } from '@/shared/api/constants'` and build paths via `api/countries` (replacing the old `countriesPath()` helper) | | |
| TASK-045 | Create `location/countries/mappers/country.mapper.ts` with `mapCountryResponse` function from `location/mapper/location.mapper.ts` | | |
| TASK-046 | Create `location/states/` with `repositories/` and `mappers/` sub-directories; create `location/states/repositories/state.repository.ts` with the `states` sub-object methods from `location/repository/location.repository.ts` flattened to top-level: `list`, `getById`, `create`, `update`, `delete` — with types referencing `../../types/State.Response.Type` and `../../types/State.Request.Type` | | |
| TASK-047 | Create `location/states/mappers/state.mapper.ts` with `mapStateResponse` function from `location/mapper/location.mapper.ts` | | |
| TASK-048 | Update imports in `location/services/country.service.ts`: remove `'../repository/location.repository'` and `'../mapper/location.mapper'`; add `import { countryRepository } from '../countries/repositories/country.repository'` and `import { mapCountryResponse } from '../countries/mappers/country.mapper'`; update all `locationRepository.countries.*` calls to `countryRepository.*` | | |
| TASK-049 | Update imports in `location/services/state.service.ts`: remove `'../repository/location.repository'` and `'../mapper/location.mapper'`; add `import { stateRepository } from '../states/repositories/state.repository'` and `import { mapStateResponse } from '../states/mappers/state.mapper'`; update all `locationRepository.states.*` calls to `stateRepository.*` | | |
| TASK-050 | Delete `location/mapper/location.mapper.ts` | | |
| TASK-051 | Delete `location/repository/location.repository.ts` | | |
| TASK-052 | Remove empty `location/repository/` and `location/mapper/` directories | | |
| TASK-053 | Run `pnpm run type-check && pnpm run lint` and fix any issues | | |

### Phase 6 — Auth: Update directory name only (already at entity level)

- GOAL-006: Auth is a single-entity module — `repository/` and `mapper/` are already inside the `auth/` entity folder. The naming `repository/` → `repositories/` and `mapper/` → `mappers/` will be applied for consistency, though directory placement is already correct.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-054 | Rename `auth/repository/` → `auth/repositories/` and `auth/mapper/` → `auth/mappers/`; update import in `auth/services/auth.service.ts` from `'../repository/auth.repository'` to `'../repositories/auth.repository'` and from `'../mapper/auth.mapper'` to `'../mappers/auth.mapper'` | | |
| TASK-055 | Run `pnpm run type-check && pnpm run lint` and fix any issues | | |

### Phase 7 — Profile: Update directory name only (already at entity level)

- GOAL-007: Profile is a single-entity module — apply naming consistency.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-056 | Rename `profile/repository/` → `profile/repositories/` and `profile/mapper/` → `profile/mappers/`; update import in `profile/services/profile.service.ts` from `'../repository/profile.repository'` to `'../repositories/profile.repository'` and from `'../mapper/profile.mapper'` to `'../mappers/profile.mapper'` | | |
| TASK-057 | Run `pnpm run type-check && pnpm run lint` and fix any issues | | |

### Phase 8 — Final verification

- GOAL-008: Verify entire project compiles, lints, and tests pass

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-058 | Run `pnpm run type-check` in `app/Admin/` and confirm zero type errors | | |
| TASK-059 | Run `pnpm run lint` in `app/Admin/` and confirm zero lint errors | | |
| TASK-060 | Run `pnpm run test:unit` in `app/Admin/` and confirm no new failures | | |

## 3. Alternatives

- **ALT-001**: Keep all repos in flat `{module}/repository/` directories — rejected because it breaks entity-level co-location established by the type/schema restructure. Services inside entity folders already import from `../../repository/` which is confusing and inconsistent.
- **ALT-002**: Restructure entire flat modules (inventories, location) into entity sub-folders, moving all schemas, types, services, stores, views, and components — rejected as out of scope. Entity folders are created with only `repositories/` and `mappers/` sub-directories; other files can be migrated in a future phase.
- **ALT-003**: Use singular `repository/` and `mapper/` names — rejected for consistency with existing plural sub-directory convention (`schemas/`, `services/`, `stores/`, `types/`, `views/`, `components/`, `tests/`).

## 4. Dependencies

- **DEP-001**: This plan depends on the completed type/schema restructure (phases 1–10) which established the per-entity folder structure for catalog, ordering/fulfillment, users/roles, users/permissions entities.

## 5. Files

- **FILE-001** to **FILE-020**: All repository files currently in `{module}/repository/` directories — see each phase for exact paths
- **FILE-021** to **FILE-027**: All mapper files currently in `{module}/mapper/` directories — 3 deleted (catalog, identity, ordering — dead code), 4 moved/split (inventories, location, auth, profile)
- **FILE-028** to **FILE-049**: Service files whose imports must be updated — see each phase for exact paths
- **FILE-050** to **FILE-052**: Test files whose imports must be updated — `catalog/_tests/catalog.api.spec.ts`, `inventories/_tests/inventory.api.spec.ts`, `ordering/_tests/ordering.api.spec.ts`
- **FILE-053** to **FILE-055**: Store files with import updates — `catalog/taxonomies/stores/taxonomy.store.ts` (line 5 references taxonomyRepository)

## 6. Testing

- **TEST-001**: `pnpm run type-check` — confirms all module resolution paths are correct after moves
- **TEST-002**: `pnpm run lint` — confirms no style regressions from moved files
- **TEST-003**: `pnpm run test:unit` — confirms existing repository tests still pass with new import paths (tests directly import repositories, not via services)

## 7. Risks & Assumptions

- **RISK-001**: `catalog/taxonomies/stores/taxonomy.store.ts` may import `taxonomyRepository` directly — verify and update path; if it imports via service instead, no change needed
- **RISK-002**: There may be additional indirect imports of repositories (e.g. from stores) not captured in this analysis — each phase's `type-check` will catch any missed imports
- **ASSUMPTION-001**: Auth and Profile are correctly treated as single-entity modules — their `repository/` and `mapper/` directories are already at the entity level; only renaming for consistency
- **ASSUMPTION-002**: `inventories/_tests/inventory.api.spec.ts` only imports `stockRepository`, `locationRepository`, and `transferRepository` (not `movementRepository` or `reservationRepository`) — confirmed by file read

## 8. Related Specifications / Further Reading

- `plan/refactor-admin-type-schema-2.md` — previous restructure that established per-entity folder pattern for catalog entities
- `app/Admin/src/features/catalog/taxonomies/` — reference implementation of the entity folder pattern (sub-directories: schemas/, services/, stores/, types/, views/, repositories/, mappers/)
