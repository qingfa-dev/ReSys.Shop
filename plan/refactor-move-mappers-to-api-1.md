---
goal: Move all mapper calls from services into API/repository layer and add full unit test coverage
version: 1.0
date_created: 2026-07-18
status: 'Planned'
tags: refactor, mappers, api, architecture, testing
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Current data flow: `API (raw DTO) → Service (applies mapper) → Store`. Target: `API (applies mapper) → Service (passthrough) → Store`. This consolidates transformation logic in one layer and keeps services thin.

25 service-api pairs across 10 domains need mapper calls moved from service → api layer. Prices and Images already demonstrate the target pattern.

## 1. Requirements & Constraints

- **REQ-001**: Every `*.api.ts` method that returns data must apply the mapper internally before returning
- **REQ-002**: Every `*.service.ts` method that read data becomes a passthrough: no mapper imports, no mapper calls
- **REQ-003**: Every mapper function has at least one unit test in a `*.mapper.spec.ts` file
- **REQ-004**: `ServerResult<T>` / `ServerPagedResult<T>` return types in api files change from response type to model type
- **CON-001**: Void-returning methods (delete, cancel, etc.) need no mapper — pass through unchanged
- **CON-002**: Services that aggregate data from multiple APIs (e.g., `product.service.ts` calls `productOptionTypeApi`) — move per-call mapping to each api file, keep aggregation logic in service
- **CON-003**: Mapper files stay in their current domain location — api files import them
- **PAT-001**: Api pattern: `async list(): ServerPagedResult<Model> => { const res = apiClient.get(…); return { ...res, items: res.items.map(mapXxx) }; }`
- **PAT-002**: Service pattern after move: `async list(): ServerPagedResult<Model> => repository.list(params)`
- **PAT-003**: Test pattern: `describe('mapXxx', () => { it('transforms dto correctly', () => { … }) })`

## 2. Implementation Steps

### Phase 1: Auth + Identity

- GOAL-001: Move auth mapping to auth.api.ts

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Read `auth/api/auth.api.ts` — add imports for `mapAuthSession`, `mapProfileResponse`, `mapSessionResponse`. Wrap `login`, `refresh` returns with `mapAuthSession`. Wrap `getProfile` return with `mapProfileResponse`. Update return types from `LoginResponse`/`Record` to `AuthSession`/`Partial<UserProfile>`. | | |
| TASK-002 | Update `auth/services/auth.service.ts` — remove mapper imports, remove mapping calls. Service becomes pure passthrough: `login(request) => authRepository.login(request)`. Keep `getProfileFromToken` (JWT decode is not API work). | | |
| TASK-003 | Create `auth/_tests/auth.mapper.spec.ts` — test `mapProfileResponse`, `mapSessionResponse`, `mapJwtToProfile` | | |
| TASK-004 | Verify: `pnpm run type-check` | | |

### Phase 2: Catalog — Products + Variants + OptionTypes + OptionValues

- GOAL-002: Move catalog mapping to api layer

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Read `catalog/products/api/product.api.ts` — add imports for `mapProductSummary`, `mapProductDetail`. Wrap list/getById/create/update. Update return types. | | |
| TASK-006 | Read `catalog/products/variants/api/variant.api.ts` — add imports for `mapVariantSummary`, `mapVariantDetail`. Wrap list/getById/create/update. | | |
| TASK-007 | Read `catalog/option-types/api/option-type.api.ts` — add imports for `mapOptionTypeListItem`, `mapOptionTypeDetail`. Wrap. | | |
| TASK-008 | Read `catalog/option-types/option-values/api/option-value.api.ts` — add imports for `mapOptionValueListItem`. Wrap. | | |
| TASK-009 | Update `product.service.ts`, `variant.service.ts`, `option-type.service.ts`, `option-value.service.ts` — remove mapper imports, remove mapping calls. Passthrough. | | |
| TASK-010 | Create `catalog/products/_tests/product.mapper.spec.ts` — test `mapProductSummary`, `mapProductDetail` | | |
| TASK-011 | Create `catalog/products/variants/_tests/variant.mapper.spec.ts` — test both variant mappers | | |
| TASK-012 | Create `catalog/option-types/_tests/option-type.mapper.spec.ts` | | |
| TASK-013 | Create `catalog/option-types/option-values/_tests/option-value.mapper.spec.ts` | | |
| TASK-014 | Verify: `pnpm run type-check` | | |

### Phase 3: Catalog — Taxonomies + Taxa + Classifications + ProductOptionTypes

- GOAL-003: Move taxonomy/taxon mapping to api layer

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | Read `catalog/taxonomies/api/taxonomy.api.ts` — add mapper imports, wrap list/getById/create/update | | |
| TASK-016 | Read `catalog/taxonomies/taxa/api/taxon.api.ts` — add mappers for list/tree/getById/create/update/rules | | |
| TASK-017 | Read `catalog/products/classifications/api/classification.api.ts` — wrap | | |
| TASK-018 | Read `catalog/products/option-types/api/product-option-type.api.ts` — wrap | | |
| TASK-019 | Update corresponding services — passthrough | | |
| TASK-020 | Create mapper spec files for each | | |
| TASK-021 | Verify | | |

### Phase 4: Ordering + Fulfillment

- GOAL-004: Move ordering mapping to api layer

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-022 | Read `ordering/orders/api/order.api.ts` — add mapper imports, wrap list/getById/create/update | | |
| TASK-023 | Read `ordering/fulfillment/api/fulfillment.api.ts` — wrap getQueue | | |
| TASK-024 | Update `order.service.ts`, `fulfillment.service.ts` — passthrough | | |
| TASK-025 | Create mapper spec files | | |
| TASK-026 | Verify | | |

### Phase 5: Payment + Shipping

- GOAL-005: Move payment/shipping mapping to api layer

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-027 | Read `payment/payments/api/payment.api.ts` — add mapper imports | | |
| TASK-028 | Read `payment/payment-methods/api/payment-method.api.ts` — add mapper imports | | |
| TASK-029 | Read `shipping/shipping-rates/api/shipping-rate.api.ts` — add mapper imports | | |
| TASK-030 | Read `shipping/shipping-methods/api/shipping-method.api.ts` — add mapper imports | | |
| TASK-031 | Update corresponding services — passthrough | | |
| TASK-032 | Create mapper spec files | | |
| TASK-033 | Verify | | |

### Phase 6: Users + Roles + Permissions

- GOAL-006: Move users mapping to api layer

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-034 | Read `users/api/user.api.ts` — add `toAdminUserSummaryModel` | | |
| TASK-035 | Read `users/roles/api/role.api.ts` — add `mapRoleSummary` | | |
| TASK-036 | Read `users/permissions/api/permission.api.ts` — add `mapPermissionSummary` | | |
| TASK-037 | Update corresponding services — passthrough | | |
| TASK-038 | Create mapper spec files | | |
| TASK-039 | Verify | | |

### Phase 7: Profile + Addresses

- GOAL-007: Move profile mapping to api layer

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-040 | Read `profile/api/profile.api.ts` — add `mapProfileResponse` | | |
| TASK-041 | Read `profile/addresses/api/address.api.ts` — add `mapAddressResponse` | | |
| TASK-042 | Update services — passthrough | | |
| TASK-043 | Create mapper spec files | | |
| TASK-044 | Verify | | |

### Phase 8: Location

- GOAL-008: Move location mapping to api layer

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-045 | Read `location/countries/api/country.api.ts` — add `mapCountryResponse` | | |
| TASK-046 | Read `location/states/api/state.api.ts` — add `mapStateResponse` | | |
| TASK-047 | Update services — passthrough | | |
| TASK-048 | Create mapper spec files | | |
| TASK-049 | Verify | | |

### Phase 9: Inventory + Reports

- GOAL-009: Move inventory/reports mapping to api layer, refactor reports to standard pattern

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-050 | Read `inventories/stock-items/api/stock.api.ts` — add `mapStockItem`, `mapStockItemDetail` | | |
| TASK-051 | Read `inventories/stock-locations/api/location.api.ts` — add `mapStockLocation` | | |
| TASK-052 | Read `inventories/stock-movements/api/movement.api.ts` — add `mapStockMovement` | | |
| TASK-053 | Read `inventories/stock-transfers/api/transfer.api.ts` — add `mapStockTransfer` | | |
| TASK-054 | Read `inventories/inventory-units/api/reservation.api.ts` — add `mapInventoryUnit` | | |
| TASK-055 | Update services — passthrough. For `inventories/services/inventory.service.ts` (duplicate), remove or consolidate with sub-services. | | |
| TASK-056 | Create `app/Admin/src/features/reports/api/report.api.ts` — extract apiClient calls from `report.service.ts` into a proper api file with mapper | | |
| TASK-057 | Update `report.service.ts` — passthrough via new api file | | |
| TASK-058 | Create mapper spec files | | |
| TASK-059 | Verify | | |

### Phase 10: Final verification

- GOAL-010: Ensure zero regressions across all domains

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-060 | `pnpm run type-check` — zero errors (baseline ~6 TreeNode) | | |
| TASK-061 | `pnpm run test:unit` — all tests pass | | |
| TASK-062 | Audit: `rg 'import.*mapper' */services/*.ts` — zero mapper imports in service files | | |

## 3. Alternatives

- **ALT-001**: Keep mappers in services — rejected because the api layer is the correct single-responsibility boundary for response transformation. Services should orchestrate, not transform.
- **ALT-002**: Create wrapper repository classes — rejected because the existing `*.api.ts` files already serve as the repository layer.

## 4. Dependencies

- **DEP-001**: All existing `*.mapper.ts` and `*.model.type.ts` files already in place from previous refactors
- **DEP-002**: `ServerResult<T>` and `ServerPagedResult<T>` types in `shared/api/types/`

## 5. Files

- **FILE-001**: 30 `*.api.ts` files — add mapper imports + wrapping
- **FILE-002**: 25 `*.service.ts` files — remove mapper imports + mapping calls, replace with passthrough
- **FILE-003**: 25 new `*.mapper.spec.ts` test files

## 6. Testing

- **TEST-001**: `pnpm run type-check` — zero errors
- **TEST-002**: `pnpm run test:unit` — all tests pass, including new mapper spec files
- **TEST-003**: Each mapper spec tests: standard transformation, null fields, edge case values

## 7. Risks & Assumptions

- **RISK-001**: Services that aggregate from multiple APIs (product.service.ts calls productOptionTypeApi + productClassificationApi) — the aggregation stays in the service, only individual API mapping moves
- **RISK-002**: `inventories/services/inventory.service.ts` duplicates 4 sub-services — should be deprecated/consolidated, not just passthrough
- **ASSUMPTION-001**: All api files return `ServerResult<T>` / `ServerPagedResult<T>` compatible with spreading
