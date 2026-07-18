---
goal: Complete every entity's layer stack — schemas/types/mappers/api/services/stores/views/tests
version: 1.0
date_created: 2026-07-18
owner: Agent
status: Planned
tags: refactor, admin-spa, entity-layers, scaffolding
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Every entity (sub-feature) must have the full layer pipeline: `schemas/ → types/ → mappers/ → api/ → services/ → stores/ → components/ → views/ → tests/`. This plan fills all missing layers across 30+ entities in the Admin SPA.

## 1. Requirements & Constraints

- **REQ-001**: Every entity directory must have all 9 layers present with at least one file
- **REQ-002**: Entities with only `api/` files (payment, shipping) get full CRUD scaffolding: schemas, types, services, stores, views
- **REQ-003**: Inventory sub-entities get extracted `services/` and `stores/` from the aggregated `inventory.service.ts` and `inventory.store.ts`
- **REQ-004**: Variant prices + images get `schemas/`, `stores/`, `views/`
- **REQ-005**: Roles + permissions get `stores/`
- **REQ-006**: Profile addresses get `schemas/`, `stores/`, `views/`
- **REQ-007**: Identity gets proper entity structure (not just a single service file)
- **REQ-008**: Empty `components/` dirs removed if no components belong there
- **CON-001**: Never break existing imports — new files must not change existing exports
- **CON-002**: Follow existing patterns — each layer file mirrors the conventions of existing entities
- **CON-003**: `tests/` files that would only contain boilerplate are optional — only create if they would test real logic
- **GUD-001**: Every entity must validate with `vue-tsc --build` after changes
- **GUD-002**: Prefer extracting from existing aggregated files over creating empty scaffolding

## 2. Implementation Steps

### Phase 1: Payment — full entity scaffolding

- GOAL-001: Payment sub-entities get schemas, types, services, stores, views

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| **PMT-001** | Create `payment/payment-methods/schemas/PaymentMethod.Schema.ts` — zod schema for name, description, provider, isActive, displayOrder fields | | |
| **PMT-002** | Create `payment/payment-methods/types/PaymentMethod.Parameters.Type.ts`, `.Query.Type.ts`, `.Request.Type.ts`, `.Response.Type.ts` — `PaymentMethodListItem` (id, name, description, provider, isActive, displayOrder), `PaymentMethodDetail` (extends list item with configuration) | | |
| **PMT-003** | Create `payment/payment-methods/services/payment-method.service.ts` — wraps `paymentMethodApi` 1:1 | | |
| **PMT-004** | Create `payment/payment-methods/stores/payment-method.store.ts` — Pinia store with list/detail state, CRUD actions using `usePagedList` | | |
| **PMT-005** | Create `payment/payment-methods/views/PaymentMethodList.View.vue` and `PaymentMethodForm.View.vue` — basic list + form views | | |
| **PMT-006** | Create `payment/payments/schemas/Payment.Schema.ts` — zod schema for notes, transactionId | | |
| **PMT-007** | Create `payment/payments/types/Payment.*.Type.ts` — `PaymentListItem` (id, orderId, amount, currency, status, methodName, createdAtUtc), `PaymentDetail` (extends with gatewayResponse, transactions) | | |
| **PMT-008** | Create `payment/payments/services/payment.service.ts` — wraps `paymentApi` | | |
| **PMT-009** | Create `payment/payments/stores/payment.store.ts` — Pinia store with list, capture/void/refund actions | | |
| **PMT-010** | Create `payment/payments/views/PaymentList.View.vue` and `PaymentDetail.View.vue` | | |

### Phase 2: Shipping — full entity scaffolding

- GOAL-002: Shipping sub-entities get schemas, types, services, stores, views

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| **SHP-001** | Create `shipping/shipping-methods/schemas/ShippingMethod.Schema.ts` — zod schema for name, description, carrier, isActive, displayOrder | | |
| **SHP-002** | Create `shipping/shipping-methods/types/ShippingMethod.*.Type.ts` — `ShippingMethodListItem` (id, name, carrier, isActive), `ShippingMethodDetail` | | |
| **SHP-003** | Create `shipping/shipping-methods/services/shipping-method.service.ts` — wraps `shippingMethodApi` | | |
| **SHP-004** | Create `shipping/shipping-methods/stores/shipping-method.store.ts` — Pinia store with list/detail/CRUD | | |
| **SHP-005** | Create `shipping/shipping-methods/views/ShippingMethodList.View.vue` and `ShippingMethodForm.View.vue` | | |
| **SHP-006** | Create `shipping/shipping-rates/schemas/ShippingRate.Schema.ts` — zod schema for shippingMethodId, rate, fromWeight, toWeight, etc. | | |
| **SHP-007** | Create `shipping/shipping-rates/types/ShippingRate.*.Type.ts` | | |
| **SHP-008** | Create `shipping/shipping-rates/services/shipping-rate.service.ts` | | |
| **SHP-009** | Create `shipping/shipping-rates/stores/shipping-rate.store.ts` | | |
| **SHP-010** | Create `shipping/shipping-rates/views/ShippingRateList.View.vue` and `ShippingRateForm.View.vue` | | |

### Phase 3: Inventory — extract per-sub-entity services and stores

- GOAL-003: Each inventory sub-entity gets its own `services/` and `stores/`, extracted from `inventory.service.ts` and `inventory.store.ts`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| **INV-001** | Create `inventories/stock-items/services/stock.service.ts` — extract stock-related methods from `inventory.service.ts`: `listStocks`, `getStockDetail`, `createStock`, `restock`, `deleteStock`, `getLowStock`, `getStockSummary`, `bulkAdjust` | | |
| **INV-002** | Create `inventories/stock-items/stores/stock.store.ts` — extract stock state (`stocks`, `totalStocks`, `stockQuery`) + actions (`fetchStocks`) from `inventory.store.ts` | | |
| **INV-003** | Create `inventories/stock-locations/services/location.service.ts` — extract: `listLocations`, `getLocationDetail`, `createLocation`, `updateLocation`, `deleteLocation`, `setDefaultLocation` | | |
| **INV-004** | Create `inventories/stock-locations/stores/location.store.ts` — extract location state + `fetchLocations` | | |
| **INV-005** | Create `inventories/stock-transfers/services/transfer.service.ts` — extract: `listTransfers`, `getTransferDetail`, `createTransfer`, `transferStock`, `receiveTransfer`, `cancelTransfer` | | |
| **INV-006** | Create `inventories/stock-transfers/stores/transfer.store.ts` — extract transfer state + `fetchTransfers` | | |
| **INV-007** | Create `inventories/inventory-units/services/reservation.service.ts` — extract: `listReservations`, `getReservationDetail`, `cancelReservation` | | |
| **INV-008** | Create `inventories/inventory-units/stores/reservation.store.ts` — extract unit state + `fetchUnits` | | |
| **INV-009** | Create `inventories/stock-movements/services/movement.service.ts` — extract: `listMovements`, `getMovementDetail` | | |
| **INV-010** | Update `inventories/services/inventory.service.ts` — delegate to per-entity services (keep for backward compat), deprecate with JSDoc `@deprecated` | | |
| **INV-011** | Update `inventories/stores/inventory.store.ts` — delegate to per-entity stores, deprecate | | |

### Phase 4: Catalog — fill missing layers in sub-entities

- GOAL-004: Products classifications, option-types, variants/prices, variants/images get their missing layers

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| **CAT-001** | Create `products/classifications/types/Classification.Response.Type.ts` — re-export `ProductClassification` from `../../types/Product.Response.Type` or define locally | | |
| **CAT-002** | Create `products/classifications/stores/classification.store.ts` — Pinia store wrapping `classificationService` | | |
| **CAT-003** | Create `products/option-types/schemas/ProductOptionType.Schema.ts` — zod schema for optionTypeIds array | | |
| **CAT-004** | Create `products/option-types/types/ProductOptionType.*.Type.ts` — type files for option-type assignment | | |
| **CAT-005** | Create `products/option-types/stores/product-option-type.store.ts` — Pinia store wrapping `productOptionTypeService` | | |
| **CAT-006** | Create `products/variants/prices/schemas/Price.Schema.ts` | | |
| **CAT-007** | Create `products/variants/prices/stores/price.store.ts` — Pinia store with price list + CRUD | | |
| **CAT-008** | Create `products/variants/images/schemas/Image.Schema.ts` | | |
| **CAT-009** | Create `products/variants/images/stores/image.store.ts` — Pinia store wrapping `imageService` | | |

### Phase 5: Profile addresses — complete scaffolding

- GOAL-005: `profile/addresses/` gets schemas, stores, views, component

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| **PRF-001** | Create `profile/addresses/schemas/Address.Schema.ts` — zod schema for address1, address2, city, stateProvince, postalCode, country, isDefault | | |
| **PRF-002** | Create `profile/addresses/types/Address.Request.Type.ts` — `CreateAddressRequest`, `UpdateAddressRequest` | | |
| **PRF-003** | Create `profile/addresses/types/Address.Parameters.Type.ts` — re-export from schema | | |
| **PRF-004** | Create `profile/addresses/stores/address.store.ts` — Pinia store | | |
| **PRF-005** | Create `profile/addresses/views/AddressList.View.vue` and `AddressForm.View.vue` | | |

### Phase 6: Users roles + permissions stores

- GOAL-006: Roles and permissions sub-entities get dedicated Pinia stores

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| **USR-001** | Create `users/roles/stores/role.store.ts` — Pinia store with list, roles tree, CRUD actions using `roleService` | | |
| **USR-002** | Create `users/permissions/stores/permission.store.ts` — Pinia store with list, group tree, assignment actions | | |

### Phase 7: Identity — proper entity structure

- GOAL-007: `identity/` gets types, api, services, stores (currently only has one `identity.api.ts`)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| **IDN-001** | Move `identity/services/identity.api.ts` → `identity/api/identity.api.ts` (it's an API file, not a service) | | |
| **IDN-002** | Create `identity/services/identity.service.ts` — wraps `identityApi`, adds meaningful service methods | | |
| **IDN-003** | Create `identity/types/Identity.Response.Type.ts` — `UserSessionInfo`, `PermissionCheck` types | | |

### Phase 8: Remove empty/bogus directories

- GOAL-008: Clean up empty component dirs, unused scaffolding

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| **CLN-001** | Remove `products/components/` — empty after variant components moved out | | |
| **CLN-002** | Remove `products/classifications/types/` — empty, types imported from parent | | |
| **CLN-003** | Remove `products/option-types/types/` — empty, types imported from parent | | |
| **CLN-004** | Remove `products/variants/stores/` — empty, no variant-level store yet | | |
| **CLN-005** | Remove `products/variants/__tests__/` — empty | | |

### Phase 9: Verification

- GOAL-009: All entities pass typecheck, lint, unit tests

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| **VER-001** | `rg '\.repository\.ts' app/Admin/src/` — zero matches | | |
| **VER-002** | Audit complete entity layer matrix — every entity has all 9 layers or a documented exception | | |
| **VER-003** | `pnpm run lint` — must pass (same pre-existing errors only) | | |
| **VER-004** | `vue-tsc --build` — zero `Cannot find module` errors | | |
| **VER-005** | `pnpm run test:unit` — same pre-existing failures only | | |

## 3. Alternatives

- **ALT-001**: Create all scaffolding as empty stub files — rejected; stubs without real logic are noise. Files should contain real types, real schemas, real service/store logic matching the backend API shape
- **ALT-002**: Keep aggregated `inventory.service.ts` and `inventory.store.ts` forever — rejected; the service file is 154 lines and the store is 162 lines, mixing concerns across 5 sub-entities. Per-entity services are more maintainable
- **ALT-003**: Add `mappers/` to every entity — rejected; mappers are only needed where API responses differ from client-side representations. Most entities don't transform data

## 4. Dependencies

- **DEP-001**: All phases are independent — can execute in any order, no cross-phase conflicts
- **DEP-002**: Phase 3 (inventory split) requires careful extraction to not break existing consumers — new per-entity services + stores coexist with deprecated aggregated ones
- **DEP-003**: Phase 4 (catalog) depends on the nested structure from previous plans being in place (already completed)

## 5. Files

| Scope | Files Created | Files Modified | Files Removed |
|-------|--------------|----------------|---------------|
| Payment | ~20 | 0 | 0 |
| Shipping | ~20 | 0 | 0 |
| Inventory | ~10 | 2 | 0 |
| Catalog | ~10 | 0 | 0 |
| Profile | ~6 | 0 | 0 |
| Users | ~2 | 0 | 0 |
| Identity | ~3 | 1 | 0 |
| Cleanup | 0 | 0 | ~5 |

## 6. Testing

- **TEST-001**: `vue-tsc --build` must pass with zero new errors
- **TEST-002**: `pnpm run lint` must pass with same pre-existing errors only
- **TEST-003**: `pnpm run test:unit` must pass with same pre-existing failures only
- **TEST-004**: Verify each entity has fully populated layers with `for d in \`find app/Admin/src/features -maxdepth 4 -type d | sort\`; do echo "$d: \`ls $d/ 2>/dev/null | wc -l\` files"; done`

## 7. Risks & Assumptions

- **RISK-001**: Payment/shipping scaffolding may not match backend response shapes exactly — mitigated by backend endpoint audit data already collected
- **RISK-002**: Inventory service/store extraction may miss some edge cases where the aggregated file has non-obvious logic — mitigated by keeping deprecated delegations as fallback
- **ASSUMPTION-001**: The backend API response shapes for payment/shipping match the types defined in this plan
- **ASSUMPTION-002**: Payment and shipping entities need standard CRUD UI (list + form views)

## 8. Related Specifications / Further Reading

- Backend endpoint mapping with result types: `plan/refactor-nested-structure-2.md`
- Current layer audit: Full matrix above in §5 (Files)
- Completed nested structure refactoring: `plan/2026-07-18-refactor-nested-structure.md`
