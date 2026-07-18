---
goal: Restructure SPA features to mirror backend endpoint nesting with correct Result/PagedResult types
version: 1.0
date_created: 2026-07-18
owner: Agent
status: Planned
tags: refactor, admin-spa, structure, nesting
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Restructure `app/Admin/src/features/` to mirror backend `Features/Admin/` directory nesting. Each feature follows the pipeline: `schema → types → mappers → api → services → store → views`. Correct every API type signature to use `ServerResult<T>` (single) or `ServerPagedResult<T>` (paginated) per backend endpoint audit.

## 1. Requirements & Constraints

- **REQ-001**: Feature directories mirror backend `Features/Admin/` nesting (e.g., `Products/Variants/Prices/`, `Products/Variants/Images/`, `OptionTypes/OptionValues/`)
- **REQ-002**: Each feature/sub-feature has this pipeline: `schemas/` → `types/` → `mappers/` → `api/` → `services/` → `stores/` → `views/` (+ `components/`, `tests/`)
- **REQ-003**: Every API file's return types match backend — `ServerResult<T>` for single, `ServerPagedResult<T>` for paginated, `ServerResult<void>` for void, `ServerResult<T[]>` for plain lists
- **REQ-004**: Flat shared directories at feature root (like `inventories/schemas/`, `inventories/views/`) pushed into sub-feature directories
- **REQ-005**: `inventories/services/inventory.service.ts` and `inventories/stores/inventory.store.ts` — split into per-sub-feature files
- **REQ-006**: All component `.vue` files moved to appropriate sub-feature `components/` dirs
- **REQ-007**: Move `products/types/Variant.*.ts` into `products/variants/types/`
- **REQ-008**: Move `products/schemas/Variant.Schema.ts` into `products/variants/schemas/`
- **REQ-009**: Move variant-related components into `products/variants/components/`
- **CON-001**: Zero behavior change — only file moves, import path updates, and type corrections
- **CON-002**: `identity/services/identity.api.ts` stays — it's a single service, not a full feature
- **CON-003**: `auth/` stays flat — no backend admin auth endpoints (uses storefront)
- **CON-004**: `reports/` stays flat — single dashboard view, no backend sub-features
- **GUD-001**: Use `git mv` to preserve file history
- **GUD-002**: One sub-feature per subdirectory — maximum nesting depth = 4 (e.g., `products/variants/prices/api/`)

## 2. Implementation Steps

### Phase 1: Catalog — products/variants sub-feature extraction

- GOAL-001: Move variant-related types, schemas, and components into `products/variants/` sub-feature

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `products/variants/types/`, `products/variants/schemas/`, `products/variants/stores/`, `products/variants/views/`, `products/variants/__tests__/` dirs | | |
| TASK-002 | `git mv products/types/Variant.Parameters.Type.ts products/variants/types/` | | |
| TASK-003 | `git mv products/types/Variant.Query.Type.ts products/variants/types/` | | |
| TASK-004 | `git mv products/types/Variant.Request.Type.ts products/variants/types/` | | |
| TASK-005 | `git mv products/types/Variant.Response.Type.ts products/variants/types/` | | |
| TASK-006 | `git mv products/schemas/Variant.Schema.ts products/variants/schemas/` | | |
| TASK-007 | `git mv products/components/ProductVariantManager.Component.vue products/variants/components/` | | |
| TASK-008 | `git mv products/components/VariantFormDialog.Component.vue products/variants/components/` | | |
| TASK-009 | `git mv products/components/dialogs/VariantGenerationDialog.Component.vue products/variants/components/` (remove empty `dialogs/`) | | |
| TASK-010 | `git mv products/components/images/ products/variants/components/images/` | | |
| TASK-011 | `git mv products/components/ProductImageManager.Component.vue products/variants/components/` | | |
| TASK-012 | `git mv products/components/ProductInventoryManager.Component.vue products/variants/components/` | | |
| TASK-013 | Update imports in `products/variants/api/variant.api.ts` — types path `../../types/` → `../types/` | | |
| TASK-014 | Update imports in `products/variants/services/variant.service.ts` — no change (already `../api/`) | | |
| TASK-015 | Update imports in `products/variants/components/*.vue` — fix `../services/variant.service` → `../../variants/services/variant.service` or `../services/` depending on depth | | |
| TASK-016 | Update imports in `products/variants/components/images/*.vue` — fix variant service + image type paths | | |
| TASK-017 | Update imports in `products/services/product.service.ts` — no change needed (doesn't import variant) | | |
| TASK-018 | Update imports in `products/components/ProductClassificationManager.Component.vue` (no variant import) | | |
| TASK-019 | Update imports in `products/components/ProductOptionTypeManager.Component.vue` (no variant import) | | |
| TASK-020 | Update `catalog/_tests/catalog.api.spec.ts` — already correct (Phase 1 of previous plan) | | |

### Phase 2: Catalog — add prices + images sub-features under variants

- GOAL-002: Create `products/variants/prices/` and `products/variants/images/` sub-feature directories with api/types scaffolding

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-021 | Create `products/variants/prices/api/`, `products/variants/prices/types/`, `products/variants/prices/services/` | | |
| TASK-022 | Create `products/variants/images/api/`, `products/variants/images/types/`, `products/variants/images/services/` | | |
| TASK-023 | Extract price methods from `products/variants/api/variant.api.ts` into `products/variants/prices/api/price.api.ts` — export a `priceApi` object with `listPrices`, `setPrice`, `deletePrice`, `syncPrices` | | |
| TASK-024 | Create `products/variants/prices/types/Price.Response.Type.ts` with `PriceRecord` interface (id, amount, currency) | | |
| TASK-025 | Create `products/variants/prices/services/price.service.ts` wrapping `priceApi` | | |
| TASK-026 | Extract image methods from `products/variants/api/variant.api.ts` into `products/variants/images/api/image.api.ts` — export `imageApi` with `listByVariant`, `upload`, `update`, `delete` | | |
| TASK-027 | Create `products/variants/images/types/Image.Response.Type.ts` with `VariantImage` interface | | |
| TASK-028 | Create `products/variants/images/services/image.service.ts` wrapping `imageApi` | | |
| TASK-029 | Update `products/variants/api/variant.api.ts` — remove price/image methods, keep variant CRUD + option-values | | |
| TASK-030 | Update `products/variants/services/variant.service.ts` — remove price/image delegations | | |
| TASK-031 | Update component imports to use new `priceService`/`imageService` instead of `variantService` | | |

### Phase 3: Catalog — products option-types + classifications sub-features

- GOAL-003: Nest `option-types` and `classifications` under `products/` (mirroring Products/Classifications and Products/OptionTypes in backend)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-032 | Create `products/option-types/api/`, `products/option-types/types/`, `products/classifications/api/`, `products/classifications/types/` | | |
| TASK-033 | Extract option-type methods from `products/api/product.api.ts` into `products/option-types/api/product-option-type.api.ts` — `list`, `sync` | | |
| TASK-034 | Extract classification methods from `products/api/product.api.ts` into `products/classifications/api/product-classification.api.ts` — `list`, `sync` | | |
| TASK-035 | Move `products/schemas/ProductClassification.Schema.ts` → `products/classifications/schemas/` | | |
| TASK-036 | Move `products/components/ProductClassificationManager.Component.vue` → `products/classifications/components/` | | |
| TASK-037 | Move `products/components/ProductOptionTypeManager.Component.vue` → `products/option-types/components/` | | |
| TASK-038 | Update imports in moved components | | |
| TASK-039 | Update `products/api/product.api.ts` — remove classification/option-type methods | | |

### Phase 4: Inventories — push flat files into sub-feature directories

- GOAL-004: Move flat `inventories/schemas/`, `inventories/types/`, `inventories/views/` into respective sub-feature dirs; split `inventory.service.ts` and `inventory.store.ts`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-040 | `git mv inventories/schemas/StockItem.Schema.ts inventories/stock-items/schemas/` | | |
| TASK-041 | `git mv inventories/schemas/StockLocation.Schema.ts inventories/stock-locations/schemas/` | | |
| TASK-042 | `git mv inventories/schemas/StockMovement.Schema.ts inventories/stock-movements/schemas/` | | |
| TASK-043 | `git mv inventories/schemas/StockTransfer.Schema.ts inventories/stock-transfers/schemas/` | | |
| TASK-044 | `git mv inventories/schemas/InventoryUnit.Schema.ts inventories/inventory-units/schemas/` | | |
| TASK-045 | Remove empty `inventories/schemas/` | | |
| TASK-046 | `git mv inventories/types/StockItem.*.ts inventories/stock-items/types/` | | |
| TASK-047 | `git mv inventories/types/StockLocation.*.ts inventories/stock-locations/types/` | | |
| TASK-048 | `git mv inventories/types/StockMovement.*.ts inventories/stock-movements/types/` | | |
| TASK-049 | `git mv inventories/types/StockTransfer.*.ts inventories/stock-transfers/types/` | | |
| TASK-050 | `git mv inventories/types/InventoryUnit.*.ts inventories/inventory-units/types/` | | |
| TASK-051 | Remove empty `inventories/types/` | | |
| TASK-052 | `git mv inventories/views/StockItemList.View.vue inventories/stock-items/views/` | | |
| TASK-053 | `git mv inventories/views/StockLocationForm.View.vue inventories/stock-locations/views/` | | |
| TASK-054 | `git mv inventories/views/StockLocationList.View.vue inventories/stock-locations/views/` | | |
| TASK-055 | `git mv inventories/views/StockLocationManager.View.vue inventories/stock-locations/views/` | | |
| TASK-056 | `git mv inventories/views/StockTransferDetail.View.vue inventories/stock-transfers/views/` | | |
| TASK-057 | `git mv inventories/views/StockTransferForm.View.vue inventories/stock-transfers/views/` | | |
| TASK-058 | `git mv inventories/views/StockTransferList.View.vue inventories/stock-transfers/views/` | | |
| TASK-059 | `git mv inventories/views/InventoryUnitList.View.vue inventories/inventory-units/views/` | | |
| TASK-060 | Remove empty `inventories/views/` | | |
| TASK-061 | Split `inventories/services/inventory.service.ts` into: `stock-items/services/stock.service.ts`, `stock-locations/services/location.service.ts`, `stock-transfers/services/transfer.service.ts`, `stock-movements/services/movement.service.ts`, `inventory-units/services/reservation.service.ts` | | |
| TASK-062 | Split `inventories/stores/inventory.store.ts` into per-sub-feature store files | | |
| TASK-063 | Update all import paths in spec file `inventories/_tests/inventory.api.spec.ts` | | |
| TASK-064 | Update all component imports referencing inventory service/store | | |

### Phase 5: Ordering — push flat files into orders/ sub-feature

- GOAL-005: Move `ordering/schemas/`, `ordering/types/`, `ordering/views/`, shared components into `ordering/orders/`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-065 | `git mv ordering/schemas/Order.Schema.ts ordering/orders/schemas/` | | |
| TASK-066 | Remove empty `ordering/schemas/` | | |
| TASK-067 | `git mv ordering/types/Order.*.ts ordering/orders/types/` | | |
| TASK-068 | Remove empty `ordering/types/` | | |
| TASK-069 | `git mv ordering/views/OrderDetail.View.vue ordering/orders/views/` | | |
| TASK-070 | `git mv ordering/views/OrderForm.View.vue ordering/orders/views/` | | |
| TASK-071 | `git mv ordering/views/OrderList.View.vue ordering/orders/views/` | | |
| TASK-072 | Remove empty `ordering/views/` | | |
| TASK-073 | `git mv ordering/components/ItemDialog.Component.vue ordering/orders/components/` | | |
| TASK-074 | `git mv ordering/components/ShipmentDialog.Component.vue ordering/orders/components/` | | |
| TASK-075 | `git mv ordering/components/AddressDialog.Component.vue ordering/orders/components/` | | |
| TASK-076 | `git mv ordering/components/RefundDialog.Component.vue ordering/orders/components/` | | |
| TASK-077 | `git mv ordering/services/order.service.ts ordering/orders/services/` | | |
| TASK-078 | `git mv ordering/stores/order.store.ts ordering/orders/stores/` | | |
| TASK-079 | Move `ordering/tests/order.service.spec.ts` → `ordering/orders/tests/` | | |
| TASK-080 | Move `ordering/tests/order.store.spec.ts` → `ordering/orders/tests/` | | |
| TASK-081 | Remove empty `ordering/tests/` | | |
| TASK-082 | Update all import paths in moved files + consumers | | |

### Phase 6: Location — push flat files into countries/ and states/

- GOAL-006: Move `location/schemas/`, `location/types/`, `location/services/`, `location/stores/`, `location/views/` into `countries/` and `states/`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-083 | `git mv location/schemas/Country.Schema.ts location/countries/schemas/` | | |
| TASK-084 | `git mv location/schemas/State.Schema.ts location/states/schemas/` | | |
| TASK-085 | Remove empty `location/schemas/` | | |
| TASK-086 | `git mv location/services/country.service.ts location/countries/services/` | | |
| TASK-087 | `git mv location/services/state.service.ts location/states/services/` | | |
| TASK-088 | Remove empty `location/services/` | | |
| TASK-089 | `git mv location/stores/country.store.ts location/countries/stores/` | | |
| TASK-090 | `git mv location/stores/state.store.ts location/states/stores/` | | |
| TASK-091 | Remove empty `location/stores/` | | |
| TASK-092 | `git mv location/types/Country.*.ts location/countries/types/` | | |
| TASK-093 | `git mv location/types/State.*.ts location/states/types/` | | |
| TASK-094 | Remove empty `location/types/` | | |
| TASK-095 | `git mv location/views/CountryForm.View.vue location/countries/views/` | | |
| TASK-096 | `git mv location/views/CountryList.View.vue location/countries/views/` | | |
| TASK-097 | `git mv location/views/StateForm.View.vue location/states/views/` | | |
| TASK-098 | `git mv location/views/StateList.View.vue location/states/views/` | | |
| TASK-099 | Remove empty `location/views/` | | |
| TASK-100 | Update all import paths in moved files + route files | | |

### Phase 7: Users — push services into sub-feature directories

- GOAL-007: Move `users/services/role.service.ts` → `users/roles/services/`, `users/services/permission.service.ts` → `users/permissions/services/`. Keep `user.service.ts` at top level (it aggregates)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-101 | `git mv users/services/role.service.ts users/roles/services/` | | |
| TASK-102 | `git mv users/services/permission.service.ts users/permissions/services/` | | |
| TASK-103 | Add `users/roles/stores/` and `users/permissions/stores/` dirs — move role store logic from `users/stores/user.store.ts` | | |
| TASK-104 | Update all import paths in consumers | | |

### Phase 8: Profile — add addresses sub-feature scaffolding

- GOAL-008: Create `profile/addresses/` sub-feature with types matching backend Profile/Addresses/ endpoints

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-105 | Create `profile/addresses/api/`, `profile/addresses/types/`, `profile/addresses/services/` | | |
| TASK-106 | Create `profile/addresses/types/Address.Response.Type.ts` with `AddressDetail` interface (id, userId, address1, address2, city, stateProvince, postalCode, country, isDefault) | | |
| TASK-107 | Create `profile/addresses/api/address.api.ts` with CRUD + `serverResult<AddressDetail[]>` for get all, `ServerResult<AddressDetail>` for get/create/update/delete | | |
| TASK-108 | Wire address api into `profile/services/profile.service.ts` or create separate `address.service.ts` | | |

### Phase 9: Correct ServerResult vs ServerPagedResult type signatures

- GOAL-009: Audit every `api/*.api.ts` file and correct return types per backend mapping

| Task | API File | Current Return Type (likely) | Correct Return Type |
|------|----------|------------------------------|---------------------|
| TASK-109 | `catalog/products/api/product.api.ts` — `list` | `ServerResult<>` | `ServerPagedResult<ProductSummary>` |
| TASK-110 | `catalog/option-types/api/option-type.api.ts` — `list` | `ServerResult<>` | `ServerPagedResult<OptionTypeListItem>` |
| TASK-111 | `catalog/option-types/option-values/api/option-value.api.ts` — `list` | `ServerResult<>` | `ServerPagedResult<OptionValueListItem>` |
| TASK-112 | `catalog/taxonomies/api/taxonomy.api.ts` — `list` | `ServerResult<>` | `ServerPagedResult<TaxonomyListItem>` |
| TASK-113 | `catalog/taxonomies/taxa/api/taxon.api.ts` — `list` | `ServerResult<>` | `ServerPagedResult<TaxonListItem>` |
| TASK-114 | `catalog/taxonomies/taxa/api/taxon.api.ts` — `rules list` | `ServerResult<>` | `ServerPagedResult<TaxonRuleListItem>` |
| TASK-115 | `catalog/taxonomies/taxa/api/taxon.api.ts` — `tree` | `ServerResult<>` | `ServerResult<TaxonTreeResponse>` (single, has .Items) |
| TASK-116 | `catalog/products/variants/prices/api/price.api.ts` — `listPrices` | `ServerResult<>` | `ServerPagedResult<PriceRecord>` |
| TASK-117 | `inventories/stock-locations/api/location.api.ts` — `list` | `ServerResult<>` | `ServerPagedResult<StockLocationListItem>` |
| TASK-118 | `inventories/stock-movements/api/movement.api.ts` — `list` | `ServerResult<>` | `ServerPagedResult<StockMovementListItem>` |
| TASK-119 | `inventories/stock-transfers/api/transfer.api.ts` — `list` | `ServerResult<>` | `ServerPagedResult<StockTransferListItem>` |
| TASK-120 | `inventories/stock-items/api/stock.api.ts` — `list` | `ServerResult<>` | `ServerResult<StockSummary[]>` (plain list, not paged) |
| TASK-121 | `inventories/stock-items/api/stock.api.ts` — `getLowStock`, `getSummary` | `ServerResult<>` | `ServerResult<StockSummary[]>` (plain list) |
| TASK-122 | `ordering/orders/api/order.api.ts` — `list` | `ServerResult<>` | `ServerPagedResult<OrderSummary>` |
| TASK-123 | `ordering/orders/api/order.api.ts` — `listLineItems` | `ServerResult<>` | `ServerPagedResult<LineItemDetail>` |
| TASK-124 | `payment/payment-methods/api/payment-method.api.ts` — `list` | `ServerResult<>` | `ServerPagedResult<PaymentMethodListItem>` |
| TASK-125 | `payment/payments/api/payment.api.ts` — `list` | `ServerResult<>` | `ServerPagedResult<PaymentListItem>` |
| TASK-126 | `shipping/shipping-methods/api/shipping-method.api.ts` — `list` | `ServerResult<>` | `ServerPagedResult<ShippingMethodListItem>` |
| TASK-127 | `shipping/shipping-rates/api/shipping-rate.api.ts` — `list` | `ServerResult<>` | `ServerPagedResult<ShippingRateListItem>` |
| TASK-128 | `location/countries/api/country.api.ts` — `list` | `ServerResult<>` | `ServerPagedResult<CountryListItem>` |
| TASK-129 | `location/states/api/state.api.ts` — `list` | `ServerResult<>` | `ServerPagedResult<StateListItem>` |
| TASK-130 | `identity/users/api/user.api.ts` — `list` | `ServerResult<>` | `ServerPagedResult<UserListItem>` |
| TASK-131 | `identity/roles/api/role.api.ts` — `list` | `ServerResult<>` | `ServerPagedResult<RoleListItem>` |
| TASK-132 | Update spec files to expect `ServerPagedResult` where appropriate | | |

### Phase 10: Verification

- GOAL-010: Confirm zero broken imports, correct types, lint + typecheck pass

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-133 | `rg '\.repository\.ts' app/Admin/src/` — zero matches | | |
| TASK-134 | `rg '/repositories/' app/Admin/src/'` — zero matches | | |
| TASK-135 | `git diff --stat` — review all moved files | | |
| TASK-136 | Audit every `api/*.api.ts` GET list endpoint — verify `ServerPagedResult` vs `ServerResult` matches backend table in §1 | | |
| TASK-137 | `pnpm run lint` — must pass | | |
| TASK-138 | `vue-tsc --build` — zero `Cannot find module` errors, zero type assignment errors from wrong Result type | | |

## 3. Alternatives

- **ALT-001**: Keep current flat + semi-nested structure — rejected because backend uses deep nesting; frontend should mirror for maintainability
- **ALT-002**: Use symlinks or barrel `index.ts` re-exports instead of moving files — simpler but breaks the one-to-one file-to-endpoint mapping
- **ALT-003**: Do Result/PagedResult corrections in a separate plan — included here because type signatures are part of the same `api/` files being moved

## 4. Dependencies

- **DEP-001**: Phase 1 (this plan) builds on top of `refactor-repo-to-api-1.md` output (already completed)
- **DEP-002**: Each phase must complete before the next — moving files breaks imports for subsequent phases
- **DEP-003**: `git mv` requires clean working tree

## 5. Files

- **FILE-001** to **FILE-100**: ~100 files move across catalog, inventories, ordering, location, users, profile
- **FILE-101** to **FILE-120**: 20 API files with type signature corrections
- **FILE-121** to **FILE-140**: ~20 new scaffolding files (sub-feature service/store extraction)

## 6. Testing

- **TEST-001**: `rg '\.repository\.ts' app/Admin/src/` — zero matches
- **TEST-002**: `rg '/repositories/' app/Admin/src/'` — zero matches
- **TEST-003**: `pngn run lint` — must pass
- **TEST-004**: `vue-tsc --build` — zero `Cannot find module` errors
- **TEST-005**: `pnpm run test:unit` — same pre-existing failures only (no new regressions)
- **TEST-006**: Verify each `get list` endpoint in `api/*.api.ts` uses `ServerPagedResult<T>` where backend returns `PagedResult<T>`

## 7. Risks & Assumptions

- **RISK-001**: Vue `@/` path aliases may need rechecking if file depth changes import resolution
- **RISK-002**: Route files (`*.routes.ts`) reference views by path — all such references must be updated
- **RISK-003**: The `inventories/services/inventory.service.ts` split requires careful extraction — it currently aggregates all 5 sub-repos; breaking it leaks an internal API
- **ASSUMPTION-001**: No code outside `app/Admin/src/` imports from these paths (Store SPA is independent)
- **ASSUMPTION-002**: All type files match the backend response shape — only the Result wrapper type changes, not the inner data types

## 8. Related Specifications / Further Reading

- Backend endpoint structure: `service/Api/src/Module/*/Features/Admin/` — hierarchical nesting by entity
- Backend endpoint `Result<T>` vs `PagedResult<T>` audit: See §1 (Requirements) in this document — 148 endpoints mapped
- `plan/refactor-repo-to-api-1.md` — previous plan (completed): `repositories/` → `api/` rename + variants under products
