---
goal: Rename all *.repository.ts → *.api.ts, nest variants under products, flatten repositories/ → api/
version: 1.0
date_created: 2026-07-18
owner: Agent
status: Completed
tags: refactor, admin-spa, api-layer
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Rename every `*.repository.ts` to `*.api.ts` and every `repositories/` dir to `api/` in `app/Admin/src/`. Nest `products/variants/` under products (mirroring backend structure). Update all imports.

## 1. Requirements & Constraints

- **REQ-001**: Every `*.repository.ts` file → `*.api.ts` with same export names
- **REQ-002**: Every `repositories/` directory → `api/`
- **REQ-003**: Extract `products/variants/` subtree from `products/repositories/`:
  - `variant.repository.ts` → `products/variants/api/variant.api.ts`
  - `variant.service.ts` → `products/variants/services/variant.service.ts`
- **REQ-004**: All 38 import references across 18 consumer files must be updated
- **REQ-005**: Zero behavior change — only file renames + import path updates
- **CON-001**: `catalog/option-types/option-values/api/option-value.api.ts` already correct — skip
- **CON-002**: `identity/services/identity.api.ts` already `.api.ts` — skip
- **CON-003**: Payment and shipping repositories have no consumers yet — rename only, no import updates needed
- **GUD-001**: Use `git mv` to preserve file history

## 2. Implementation Steps

### Phase 1: Create products/variants/ subtree

- GOAL-001: Move variant files from `products/repositories/` and `products/services/` into `products/variants/`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `products/variants/api/` and `products/variants/services/` dirs | | |
| TASK-002 | `git mv products/repositories/variant.repository.ts products/variants/api/variant.api.ts` | | |
| TASK-003 | `git mv products/services/variant.service.ts products/variants/services/variant.service.ts` | | |
| TASK-004 | Update `variant.service.ts` import: `'../repositories/variant.repository'` → `'../api/variant.api'` | | |

### Phase 2: Rename all repositories/ → api/ and *.repository.ts → *.api.ts

- GOAL-002: Rename 23 files in 23 directories across 10 feature modules

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | **auth**: `git mv auth/repositories/ auth/api/` + rename `auth.repository.ts` → `auth.api.ts` | | |
| TASK-006 | **catalog/option-types**: `git mv option-types/repositories/ option-types/api/` + rename | | |
| TASK-007 | **catalog/products**: `git mv products/repositories/product.repository.ts products/api/product.api.ts`; remove empty `products/repositories/` | | |
| TASK-008 | **catalog/products/variants**: (already done in Phase 1) | | |
| TASK-009 | **catalog/taxonomies**: `git mv taxonomies/repositories/ taxonomies/api/` + rename | | |
| TASK-010 | **catalog/taxonomies/taxa**: `git mv taxa/repositories/ taxa/api/` + rename | | |
| TASK-011 | **inventories/inventory-units**: `git mv inventory-units/repositories/ inventory-units/api/` + rename | | |
| TASK-012 | **inventories/stock-items**: `git mv stock-items/repositories/ stock-items/api/` + rename | | |
| TASK-013 | **inventories/stock-locations**: `git mv stock-locations/repositories/ stock-locations/api/` + rename | | |
| TASK-014 | **inventories/stock-movements**: `git mv stock-movements/repositories/ stock-movements/api/` + rename | | |
| TASK-015 | **inventories/stock-transfers**: `git mv stock-transfers/repositories/ stock-transfers/api/` + rename | | |
| TASK-016 | **location/countries**: `git mv countries/repositories/ countries/api/` + rename | | |
| TASK-017 | **location/states**: `git mv states/repositories/ states/api/` + rename | | |
| TASK-018 | **ordering/fulfillment**: `git mv fulfillment/repositories/ fulfillment/api/` + rename | | |
| TASK-019 | **ordering/orders**: `git mv orders/repositories/ orders/api/` + rename | | |
| TASK-020 | **payment/payment-methods**: `git mv payment-methods/repositories/ payment-methods/api/` + rename | | |
| TASK-021 | **payment/payments**: `git mv payments/repositories/ payments/api/` + rename | | |
| TASK-022 | **profile**: `git mv profile/repositories/ profile/api/` + rename | | |
| TASK-023 | **shipping/shipping-methods**: `git mv shipping-methods/repositories/ shipping-methods/api/` + rename | | |
| TASK-024 | **shipping/shipping-rates**: `git mv shipping-rates/repositories/ shipping-rates/api/` + rename | | |
| TASK-025 | **users**: `git mv users/repositories/ users/api/` + rename | | |
| TASK-026 | **users/roles**: `git mv roles/repositories/ roles/api/` + rename | | |
| TASK-027 | **users/permissions**: `git mv permissions/repositories/ permissions/api/` + rename | | |

### Phase 3: Update all 38 import references

- GOAL-003: Fix every import path across 18 consumer files to match new locations

| Task | File | Old Import | New Import |
|------|------|-----------|------------|
| TASK-028 | `auth/services/auth.service.ts` | `../repositories/auth.repository` | `../api/auth.api` |
| TASK-029 | `catalog/option-types/services/option-type.service.ts` | `../repositories/option-type.repository` | `../api/option-type.api` |
| TASK-030 | `catalog/products/services/product.service.ts` | `../repositories/product.repository` | `../api/product.api` |
| TASK-031 | `catalog/taxonomies/services/taxonomy.service.ts` | `../repositories/taxonomy.repository` | `../api/taxonomy.api` |
| TASK-032 | `catalog/taxonomies/stores/taxonomy.store.ts` | `../repositories/taxonomy.repository` | `../api/taxonomy.api` |
| TASK-033 | `catalog/taxonomies/taxa/services/taxon.service.ts` | `../repositories/taxon.repository` | `../api/taxon.api` |
| TASK-034 | `inventories/services/inventory.service.ts` — 4 imports | `../inventory-units/repositories/reservation.repository` | `../inventory-units/api/reservation.api` |
| | | `../stock-items/repositories/stock.repository` | `../stock-items/api/stock.api` |
| | | `../stock-locations/repositories/location.repository` | `../stock-locations/api/location.api` |
| | | `../stock-movements/repositories/movement.repository` | `../stock-movements/api/movement.api` |
| | | `../stock-transfers/repositories/transfer.repository` | `../stock-transfers/api/transfer.api` |
| TASK-035 | `location/services/country.service.ts` | `../countries/repositories/country.repository` | `../countries/api/country.api` |
| TASK-036 | `location/services/state.service.ts` | `../states/repositories/state.repository` | `../states/api/state.api` |
| TASK-037 | `ordering/services/order.service.ts` | `../orders/repositories/order.repository` | `../orders/api/order.api` |
| TASK-038 | `ordering/fulfillment/services/fulfillment.service.ts` — 2 imports | `../repositories/fulfillment.repository` | `../api/fulfillment.api` |
| | | `../../orders/repositories/order.repository` | `../../orders/api/order.api` |
| TASK-039 | `profile/services/profile.service.ts` | `../repositories/profile.repository` | `../api/profile.api` |
| TASK-040 | `users/services/user.service.ts` | `../repositories/user.repository` | `../api/user.api` |
| TASK-041 | `users/services/role.service.ts` | `../roles/repositories/role.repository` | `../roles/api/role.api` |
| TASK-042 | `users/services/permission.service.ts` | `../permissions/repositories/permission.repository` | `../permissions/api/permission.api` |

### Phase 4: Update test/spec imports

- GOAL-004: Fix all 6 spec file import references

| Task | File | Old Import | New Import |
|------|------|-----------|------------|
| TASK-043 | `catalog/_tests/catalog.api.spec.ts` — 5 imports | `../option-types/repositories/option-type.repository` | `../option-types/api/option-type.api` |
| | | `../products/repositories/product.repository` | `../products/api/product.api` |
| | | `../products/repositories/variant.repository` | `../products/variants/api/variant.api` |
| | | `../taxonomies/repositories/taxonomy.repository` | `../taxonomies/api/taxonomy.api` |
| | | `../taxonomies/taxa/repositories/taxon.repository` | `../taxonomies/taxa/api/taxon.api` |
| TASK-044 | `inventories/_tests/inventory.api.spec.ts` — 3 imports | `../stock-items/repositories/stock.repository` | `../stock-items/api/stock.api` |
| | | `../stock-locations/repositories/location.repository` | `../stock-locations/api/location.api` |
| | | `../stock-transfers/repositories/transfer.repository` | `../stock-transfers/api/transfer.api` |
| TASK-045 | `ordering/_tests/ordering.api.spec.ts` — 2 imports | `../orders/repositories/order.repository` | `../orders/api/order.api` |
| | | `../fulfillment/repositories/fulfillment.repository` | `../fulfillment/api/fulfillment.api` |

### Phase 5: Verification

- GOAL-005: Confirm zero broken imports, zero stale files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-046 | `git diff --stat` — confirm only moved/renamed files (no content changes beyond import paths) | | |
| TASK-047 | `rg '\.repository\.ts' app/Admin/src/` — confirm zero matches | | |
| TASK-048 | `rg 'repositories/' app/Admin/src/'` — confirm zero matches (import paths) | | |
| TASK-049 | `pnpm run lint` — must pass | | |
| TASK-050 | `pnpm run test:unit` — must pass (same failures as baseline: `useI18n` mock + catalog routing) | | |
| TASK-051 | `pnpm run typecheck` — must pass | | |

## 3. Alternatives

- **ALT-001**: Keep `repositories/` naming — rejected because backend uses no "repository" suffix; SPA should mirror `api/` convention
- **ALT-002**: Keep all variants at `products/repositories/` level — rejected because backend nests `Variants/` under `Products/`; SPA should mirror
- **ALT-003**: Mass rename with a bash one-liner — riskier for git history; `git mv` per file keeps traceability

## 4. Dependencies

- **DEP-001**: `git mv` requires clean working tree (no uncommitted changes to affected files)
- **DEP-002**: All 18 consumer files must be updated atomically — partial rename breaks the build

## 5. Files

- **FILE-001** to **FILE-023**: 23 `*.repository.ts` files across 10 feature modules (listed in Phase 2)
- **FILE-024** to **FILE-041**: 18 consumer files with import updates (listed in Phase 3-4)
- **FILE-042**: `products/variants/api/variant.api.ts` — new location for variant repository
- **FILE-043**: `products/variants/services/variant.service.ts` — new location for variant service

## 6. Testing

- **TEST-001**: `pnpm run lint` — no errors
- **TEST-002**: `pnpm run test:unit` — same pre-existing failures only
- **TEST-003**: `rg '\.repository\.ts' app/Admin/src/` — zero matches
- **TEST-004**: `rg '/repositories/' app/Admin/src/'` — zero matches in import paths
- **TEST-005**: Confirm each renamed file still exports the same named exports (`git diff --name-only` + check exports)

## 7. Risks & Assumptions

- **RISK-001**: Missed imports in `.vue` template script blocks — mitigated by comprehensive `rg` search
- **RISK-002**: Dynamic import strings like `` `../repositories/${x}` `` — none found in search
- **ASSUMPTION-001**: All repositories export named exports, not default — verified against all 23 files
- **ASSUMPTION-002**: No other services outside `app/Admin/src/` import these files — verified (store SPA is separate)

## 8. Related Specifications / Further Reading

- Backend folder convention: `service/Api/src/Module/*/Features/Admin/` — entities nested under parents (e.g., `Products/Variants/Prices/`, `OptionTypes/OptionValues/`)
- SPA module convention: `app/Admin/src/features/*/api/*.api.ts` — established by `option-value.api.ts`
