---
goal: Delete 19 identity mappers, collapse 6 computed-only to inline lambdas, keep 7 real mappers
version: 1.0
date_created: 2026-07-18
status: 'Completed'
tags: refactor, mappers, simplification
---

# Introduction

![Status: In progress](https://img.shields.io/badge/status-In%20progress-yellow)

Of 32 mapper files, only 7 do real work. 19 are identity functions returning data unchanged. 6 spread all fields + add 1 computed field. Collapse all trivial transformations into inline lambdas via a shared `transformResult`/`transformItems` utility. Delete 25 mapper files.

## 1. Requirements & Constraints

- **REQ-001**: Delete all 19 identity mapper files — no `{ ...dto }` passthrough functions
- **REQ-002**: Collapse 6 computed-only mappers into inline `(dto) => ({ ...dto, field: fn(dto) })` lambdas in API files
- **REQ-003**: Keep 7 real mappers as explicit functions
- **REQ-004**: Create `shared/utils/transform.ts` with `transformResult` and `transformItems` helpers
- **REQ-005**: All existing tests must pass with zero changes to test assertions
- **CON-001**: Model type factory functions (e.g., `toOrderListItemModel`) stay in `*.model.type.ts` files — only the mapper wrapper files are deleted
- **CON-002**: Inline lambdas in API files call model factory functions directly (not duplicate the logic)
- **PAT-001**: `transformResult(result, dto => ({ ...dto, field: transformFn(dto) }))` — one line in API file

## 2. Implementation Steps

### Phase 1: Create shared transformer utility

- GOAL-001: Generic helpers to replace repetitive `{ ...result, value: fn(result.value) }` patterns

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `app/Admin/src/shared/utils/transform.ts` with both helpers | ✅ | 2026-07-18 |

```typescript
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'

export function mapValue<T, R>(result: ServerResult<T>, fn: (dto: T) => R): ServerResult<R> {
  return result.isSuccess && result.value != null
    ? { ...result, value: fn(result.value) }
    : result as unknown as ServerResult<R>
}

export function mapItems<T, R>(result: ServerPagedResult<T>, fn: (dto: T) => R): ServerPagedResult<R> {
  return result.isSuccess && result.items
    ? { ...result, items: result.items.map(fn) }
    : result as unknown as ServerPagedResult<R>
}
```

### Phase 2: Delete 19 identity mappers + update API files to skip mapping

- GOAL-002: Remove mappers that do nothing and remove their map calls from API files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-002 | Delete `profile/addresses/mappers/address.mapper.ts` — remove `mapAddressResponse` imports+usage from `address.api.ts`. The API returns `AddressDetail` directly — no transform needed. | | |
| TASK-003 | Delete `users/roles/mappers/role.mapper.ts` — remove `mapRoleSummary` from `role.api.ts` | | |
| TASK-004 | Delete `users/permissions/mappers/permission.mapper.ts` — remove `mapPermissionSummary` from `permission.api.ts` | | |
| TASK-005 | Delete `reports/mappers/report.mapper.ts` — remove `mapSalesSummary` from `report.api.ts` | | |
| TASK-006 | Delete `ordering/fulfillment/mappers/fulfillment.mapper.ts` — remove from `fulfillment.api.ts` | | |
| TASK-007 | Delete `catalog/taxonomies/mappers/taxonomy.mapper.ts` — remove from `taxonomy.api.ts` | | |
| TASK-008 | Delete `location/states/mappers/state.mapper.ts` — remove from `state.api.ts` | | |
| TASK-009 | Delete `location/countries/mappers/country.mapper.ts` — remove from `country.api.ts` | | |
| TASK-010 | Delete `catalog/taxonomies/taxa/mappers/taxon-rule.mapper.ts` — remove from `taxon.api.ts` | | |
| TASK-011 | Delete `catalog/taxonomies/taxa/mappers/taxon.mapper.ts` — remove from `taxon.api.ts` | | |
| TASK-012 | Delete `catalog/products/option-types/mappers/product-option-type.mapper.ts` — remove from `product-option-type.api.ts` | | |
| TASK-013 | Delete `inventories/inventory-units/mappers/inventory-unit.mapper.ts` — remove from `reservation.api.ts` | | |
| TASK-014 | Delete `catalog/products/classifications/mappers/classification.mapper.ts` — remove from `product-classification.api.ts` | | |
| TASK-015 | Delete `inventories/stock-movements/mappers/stock-movement.mapper.ts` — remove from `movement.api.ts` | | |
| TASK-016 | Delete `catalog/option-types/mappers/option-type.mapper.ts` — remove from `option-type.api.ts` | | |
| TASK-017 | Delete `catalog/option-types/option-values/mappers/option-value.mapper.ts` — remove from `option-value.api.ts` | | |
| TASK-018 | Delete `catalog/products/variants/images/mappers/image.mapper.ts` — remove from `image.api.ts` | | |
| TASK-019 | Delete `inventories/stock-transfers/mappers/stock-transfer.mapper.ts` — remove from `transfer.api.ts` | | |
| TASK-020 | Delete `inventories/stock-locations/mappers/stock-location.mapper.ts` — remove from `location.api.ts` | | |
| TASK-021 | Delete `catalog/products/variants/prices/mappers/price.mapper.ts` — remove from `price.api.ts` | | |
| TASK-022 | Delete their corresponding `*.mapper.spec.ts` test files (unless they tested real logic) | | |
| TASK-023 | Verify: `pnpm run type-check` — zero new errors | | |

### Phase 3: Collapse 6 computed-only mappers into inline lambdas

- GOAL-003: Replace mapper files that do spread+1-computed-field with inline lambdas in API files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-024 | **Shipping rate**: Delete `shipping/shipping-rates/mappers/shipping-rate.mapper.ts`. In `shipping-rate.api.ts`: remove `mapShippingRateListItem`/`mapShippingRateDetail` imports, use `mapItems(result, d => ({ ...d, costDisplay: decimalToDisplay(d.cost, d.currency) }))` | | |
| TASK-025 | **Shipping method**: Delete `shipping/shipping-methods/mappers/shipping-method.mapper.ts`. In `shipping-method.api.ts`: inline `d => ({ ...d, statusLabel: d.isActive ? 'Active' : 'Inactive' })` | | |
| TASK-026 | **Payment method**: Delete `payment/payment-methods/mappers/payment-method.mapper.ts`. In `payment-method.api.ts`: inline the same statusLabel lambda | | |
| TASK-027 | **User**: Delete `users/mappers/user.mapper.ts`. In `user.api.ts`: inline `d => ({ ...d, hasRole: false, isLocked: false })` | | |
| TASK-028 | **Product**: Delete `catalog/products/mappers/product.mapper.ts` + `catalog/products/types/product.model.type.ts`. In `product.api.ts`: inline `d => ({ ...d, statusLabel: ProductStatusMap[d.status] ?? 'Unknown' })` | | |
| TASK-029 | **Variant**: Delete `catalog/products/variants/mappers/variant.mapper.ts` + `catalog/products/variants/types/variant.model.type.ts`. In `variant.api.ts`: inline `d => ({ ...d, priceDisplay: decimalToDisplay(d.price) })` | | |
| TASK-030 | Delete their `*.mapper.spec.ts` test files | | |
| TASK-031 | Verify: `pnpm run type-check` — zero new errors | | |

### Phase 4: Keep 7 real mappers

- GOAL-004: No changes — these stay as-is

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-032 | Keep `auth/mappers/auth.mapper.ts` — raw `Record` input with case fallback | | |
| TASK-033 | Keep `identity/mappers/identity.mapper.ts` — multi-DTO join | | |
| TASK-034 | Keep `ordering/orders/mappers/order.mapper.ts` — 8 computed fields | | |
| TASK-035 | Keep `payment/payments/mappers/payment.mapper.ts` — 2 computed + enum map | | |
| TASK-036 | Keep `inventories/stock-items/mappers/stock-item.mapper.ts` — `countAvailable` alias | | |
| TASK-037 | Keep `profile/mappers/profile.mapper.ts` — `?? false` transform + field whitelist | | |

### Phase 5: Delete stale test + spec files

- GOAL-005: Remove test files for deleted mappers

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-038 | Delete all `*.mapper.spec.ts` files for deleted mappers | | |
| TASK-039 | Keep spec files for the 7 surviving real mappers | | |
| TASK-040 | Verify: `pnpm run type-check` + `pnpm run test:unit` | | |

## 3. Alternatives

- **ALT-001**: Keep all mappers for "consistency" — rejected, they add noise without value
- **ALT-002**: Auto-generate identity mappers — rejected, deleting them is simpler
- **ALT-003**: Keep model.type.ts factory functions but remove mapper wrappers — this IS the approach

## 4. Dependencies

- **DEP-001**: `shared/utils/transform.ts` (created in Phase 1)
- **DEP-002**: All `*.api.ts` files already call mappers — update call sites

## 5. Files

- **FILE-001**: `shared/utils/transform.ts` (new) — 15 lines
- **FILE-002**: 25 mapper files deleted (19 identity + 6 computed-only)
- **FILE-003**: 25 mapper spec files deleted
- **FILE-004**: 25+ API files updated (remove imports, replace with inline or remove map call)
- **FILE-005**: 4 model.type.ts files deleted (product.model.type, variant.model.type, etc.)
- **FILE-006**: 7 mapper files kept

## 6. Testing

- **TEST-001**: `pnpm run type-check` — zero errors
- **TEST-002**: Surviving mapper tests still pass (auth, identity, order, payment, stock-item, profile)

## 7. Risks & Assumptions

- **RISK-001**: Deleting model.type.ts files may break services/stores that import from them — verify imports before deletion
- **RISK-002**: The `mapValue`/`mapItems` helpers use `as unknown as` internally — acceptable since the type narrowing is safe
