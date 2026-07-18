---
goal: Fix API response types and create correct mappers from serializer DTOs to frontend models
version: 1.0
date_created: 2026-07-18
status: 'Completed'
tags: refactor, mappers, api-models, data-layer
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Audit revealed significant misalignment between what the .NET backend API actually sends and what the TypeScript `*.response.type.ts` files define. Across 9 domains, 25+ field mismatches exist (integer enums vs string unions, decimal currency vs cents integers, missing display-name fields, field name mismatches, missing nested collections). Only 9 mappers exist, and 8 are identity pass-throughs. 24 domains have no mapper at all.

Create correct `*.response.type.ts` files that accurately reflect API payloads, then create consistent `*.mapper.ts` files for all domains to transform raw API DTOs into frontend-friendly model shapes.

## 1. Requirements & Constraints

- **REQ-001**: `*.response.type.ts` must match the C# response DTO field-for-field (type, name, optionality)
- **REQ-002**: Each domain gets a `*.mapper.ts` with at least one main `mapXxx(dto): Model` function
- **REQ-003**: Mappers handle: enum integers → human-readable strings, decimal dollars → cents integers, missing defaults, field renames
- **REQ-004**: Identity (pass-through) mappers are acceptable where no transformation is needed
- **REQ-005**: Services must call mapper functions (not pass raw response types to stores/components)
- **REQ-006**: Tests for each mapper function
- **CON-001**: Do NOT change component templates — they consume raw response fields; plan must update them if field names change
- **CON-002**: enum → integer mapping uses lookup objects, not `switch`/`if-else` chains
- **CON-003**: Currency values stay as `number` in cents (not string-typed)
- **PAT-001**: `export const mapXxx = (dto: XxxResponse): XxxModel => ({ ... })` pattern
- **PAT-002**: Shared enum lookup maps defined in `shared/utils/enums.ts`

## 2. Implementation Steps

### Phase 1: Create shared enum lookup maps

- GOAL-001: Create shared integer→string enum converters used by all mappers

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `app/Admin/src/shared/utils/enums.ts` with lookup maps for `ProductStatus` (0→Draft,1→Active,2→Archived), `OrderStatus` (0→Draft,1→Placed,2→Canceled,4→Expired), `CheckoutState`, `PaymentState`, `ShipmentState`, `TransferState`, `ReservationState` | | |
| TASK-002 | Create `app/Admin/src/shared/utils/currency.ts` with `centsToDisplay(cents: number, currency: string): string` and `decimalToCents(amount: number): number` | | |
| TASK-003 | Verify: `pnpm run type-check` passes after adding shared utils | | |

### Phase 2: Fix response types to match API reality

- GOAL-002: Align all `*.response.type.ts` files with actual C# DTO shapes

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | **Catalog Product**: Fix `ProductSummary` — remove `imageUrl`, `sku`, `price` (not in C#). Fix `status` → `number`. Add `masterVariantId`, `availableOn`, `discontinueOn`, `trackInventory`. Fix `ProductDetail` — remove `weight`/`height`/`width`/`depth`/`variants`/`images`/`classifications` (in nested responses). | | |
| TASK-005 | **Catalog Variant**: Fix `VariantSummary`/`VariantDetail` — remove `barcode`, `compareAtPrice`, `options`, `optionValueIds` (not in C#). Add `pricesCount`, `discontinuedOn`. | | |
| TASK-006 | **Catalog OptionType / OptionValue**: Verify align with C#. Likely match. Add any missing fields. | | |
| TASK-007 | **Catalog Taxonomy / Taxon / TaxonRule**: Verify align with C#. Likely match. Add any missing fields. | | |
| TASK-008 | **Identity User**: Fix `AdminUserSummary` — remove `accessFailedCount`, `lockoutEnd`, `lastSignInAtUtc`, `lastIpAddress`, `roleNames`. Add `emailConfirmed`, `phoneNumberConfirmed`, `customerGroups`. | | |
| TASK-009 | **Identity Login**: Fix `LoginResponse` — remove `expiresAt`, `user`. Add `accessToken`, `accessTokenExpiresIn`, `refreshToken`, `refreshTokenExpiresIn`. Create separate `SessionResponse` for session data. | | |
| TASK-010 | **Ordering Order**: Fix `OrderDetail` — rename `state`→`status` (number), `totalCents`→`total` (decimal), `itemTotalCents`→`itemTotal`, `shipmentTotalCents`→`shipmentTotal`. Remove `totalDisplay`/`itemTotalDisplay`/`shipmentTotalDisplay`. Remove nested `lineItems`/`payments`/`shipments`/`history`/`shippingAddress`/`billingAddress` (separate endpoints). | | |
| TASK-011 | **Ordering Fulfillment**: Verify align with C#. | | |
| TASK-012 | **Location Country**: Add `zipcodeRequired` if it exists in C#; remove if not. | | |
| TASK-013 | **Location State**: Already aligned (verified). | | |
| TASK-014 | **Inventory StockItem**: Remove `sku`, `variantName`, `stockLocationName`, `quantityReserved`, `countAvailable`, `backorderLimit` (not in C# base response). These are display/computed fields. | | |
| TASK-015 | **Inventory StockLocation / StockMovement / StockTransfer / InventoryUnit**: Verify align with C#. | | |
| TASK-016 | **Payment Payment / PaymentMethod**: Verify align with C#. | | |
| TASK-017 | **Profile Profile / Address**: Verify align with C#. | | |
| TASK-018 | **Shipping ShippingRate / ShippingMethod**: Verify align with C#. | | |
| TASK-019 | **Users User / Role / Permission**: Verify align with C#. | | |
| TASK-020 | **Reports**: Verify align with C#. | | |
| TASK-021 | **Identity Identity**: Verify `UserSessionInfo`/`LoginResponse` align with C# session model. | | |
| TASK-022 | **Auth**: Already aligned (token-only response types). | | |
| TASK-023 | Verify: `pnpm run type-check` passes after all response type fixes | | |

### Phase 3: Create model types (frontend-optimized shapes)

- GOAL-003: Create `*.model.type.ts` files with frontend-friendly shapes (computed fields, joined data, formatted values)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-024 | **Catalog Product**: Create `catalog/products/types/product.model.type.ts` with `ProductSummaryModel` (`+displayName`, `+statusLabel`), `ProductDetailModel` (`+statusLabel`, `+variants[]`, `+images[]`, `+classifications[]` — joined from sub-endpoints) | | |
| TASK-025 | **Catalog Variant**: Create `catalog/products/variants/types/variant.model.type.ts` with `VariantModel` (`+priceDisplay`, `+options` joined from option-values) | | |
| TASK-026 | **Ordering Order**: Create `ordering/orders/types/order.model.type.ts` with `OrderListItemModel` (`+totalDisplay`, `+statusLabel`, `+customerName`), `OrderDetailModel` (all display fields + nested collections from sub-endpoints) | | |
| TASK-027 | **Identity User**: Create `users/types/user.model.type.ts` with `UserModel` (`+fullName` computed, `+statusLabel`, `+rolesLabel`) | | |
| TASK-028 | **Inventory StockItem**: Create `inventories/stock-items/types/stock-item.model.type.ts` with `StockItemModel` (`+variantName`, `+stockLocationName`, `+countAvailable` computed, `+sku` joined) | | |
| TASK-029 | **All other domains**: Create `*.model.type.ts` for domains where components need derived fields (payment, profile, shipping, location, reports) | | |
| TASK-030 | Verify: `pnpm run type-check` passes | | |

### Phase 4: Create/update mappers for all domains

- GOAL-004: Every domain has at least one `mapXxx(dto: ResponseType): ModelType` function

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-031 | **Auth**: Update `auth.mapper.ts` — `mapLoginResponse(LoginResponse)→AuthSession` (composite of login + session data), keep existing `mapJwtToProfile` | | |
| TASK-032 | **Catalog Product**: Create `catalog/products/mappers/product.mapper.ts` — `mapProductSummary(ProductSummaryResponse)→ProductSummaryModel`, `mapProductDetail(ProductDetailResponse)→ProductDetailModel` | | |
| TASK-033 | **Catalog Variant**: Create `catalog/products/variants/mappers/variant.mapper.ts` | | |
| TASK-034 | **Catalog OptionType**: Create `catalog/option-types/mappers/option-type.mapper.ts` | | |
| TASK-035 | **Catalog OptionValue**: Create `catalog/option-types/option-values/mappers/option-value.mapper.ts` | | |
| TASK-036 | **Catalog Taxonomy**: Create `catalog/taxonomies/mappers/taxonomy.mapper.ts` | | |
| TASK-037 | **Catalog Taxon**: Create `catalog/taxonomies/taxa/mappers/taxon.mapper.ts` | | |
| TASK-038 | **Catalog TaxonRule**: Create `catalog/taxonomies/taxa/mappers/taxon-rule.mapper.ts` | | |
| TASK-039 | **Catalog Classification**: Create `catalog/products/classifications/mappers/classification.mapper.ts` | | |
| TASK-040 | **Catalog ProductOptionType**: Create `catalog/products/option-types/mappers/product-option-type.mapper.ts` | | |
| TASK-041 | **Catalog Image**: Create `catalog/products/variants/images/mappers/image.mapper.ts` | | |
| TASK-042 | **Catalog Price**: Create `catalog/products/variants/prices/mappers/price.mapper.ts` | | |
| TASK-043 | **Identity**: Create `identity/mappers/identity.mapper.ts` — `mapLoginResponse`, `mapSessionResponse` | | |
| TASK-044 | **Ordering Order**: Create `ordering/orders/mappers/order.mapper.ts` — handle decimal→cents, enum→label | | |
| TASK-045 | **Ordering Fulfillment**: Create `ordering/fulfillment/mappers/fulfillment.mapper.ts` | | |
| TASK-046 | **Payment Payment**: Create `payment/payments/mappers/payment.mapper.ts` | | |
| TASK-047 | **Payment PaymentMethod**: Create `payment/payment-methods/mappers/payment-method.mapper.ts` | | |
| TASK-048 | **Profile**: Update `profile.mapper.ts` — explicit field mapping (replace identity spread) | | |
| TASK-049 | **Profile Address**: Create `profile/addresses/mappers/address.mapper.ts` | | |
| TASK-050 | **Location Country**: Update `country.mapper.ts` — explicit field mapping | | |
| TASK-051 | **Location State**: Update `state.mapper.ts` — explicit field mapping | | |
| TASK-052 | **Inventory StockItem**: Update `stock-item.mapper.ts` — add display-name joins | | |
| TASK-053 | **Inventory StockLocation**: Update `stock-location.mapper.ts` — explicit field mapping | | |
| TASK-054 | **Inventory StockMovement**: Update `stock-movement.mapper.ts` — explicit field mapping | | |
| TASK-055 | **Inventory StockTransfer**: Update `stock-transfer.mapper.ts` — explicit field mapping | | |
| TASK-056 | **Inventory InventoryUnit**: Update `inventory-unit.mapper.ts` — explicit field mapping | | |
| TASK-057 | **Shipping ShippingRate**: Create `shipping/shipping-rates/mappers/shipping-rate.mapper.ts` | | |
| TASK-058 | **Shipping ShippingMethod**: Create `shipping/shipping-methods/mappers/shipping-method.mapper.ts` | | |
| TASK-059 | **Users User**: Create `users/mappers/user.mapper.ts` | | |
| TASK-060 | **Users Role**: Create `users/roles/mappers/role.mapper.ts` | | |
| TASK-061 | **Users Permission**: Create `users/permissions/mappers/permission.mapper.ts` | | |
| TASK-062 | **Reports**: Create `reports/mappers/report.mapper.ts` | | |
| TASK-063 | Verify: `pnpm run type-check` passes — zero errors | | |

### Phase 5: Wire mappers into services

- GOAL-005: All data-fetching services apply mapper before returning data

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-064 | Update `product.service.ts` — wrap repository results with `mapProductSummary`/`mapProductDetail` | | |
| TASK-065 | Update `variant.service.ts` — wrap with `mapVariant` | | |
| TASK-066 | Update `option-type.service.ts` — wrap with mapper | | |
| TASK-067 | Update `option-value.service.ts` — wrap with mapper | | |
| TASK-068 | Update `taxonomy.service.ts` — wrap with mapper | | |
| TASK-069 | Update `taxon.service.ts` — wrap with mapper | | |
| TASK-070 | Update `order.service.ts` — wrap with mapper (decimal→cents, enum→label) | | |
| TASK-071 | Update `payment.service.ts` — wrap with mapper | | |
| TASK-072 | Update `payment-method.service.ts` — wrap with mapper | | |
| TASK-073 | Update `profile.service.ts` — already uses mapper | | |
| TASK-074 | Update `address.service.ts` — wrap with mapper | | |
| TASK-075 | Update `user.service.ts` — wrap with mapper | | |
| TASK-076 | Update `role.service.ts` — wrap with mapper | | |
| TASK-077 | Update `permission.service.ts` — wrap with mapper | | |
| TASK-078 | Update `report.service.ts` — wrap with mapper | | |
| TASK-079 | Update `shipping-rate.service.ts` — wrap with mapper | | |
| TASK-080 | Update `shipping-method.service.ts` — wrap with mapper | | |
| TASK-081 | Update `auth.service.ts` — refactor to use mapper for login response | | |
| TASK-082 | Update `stock.service.ts` — already uses mapper | | |
| TASK-083 | Update `country.service.ts` — already uses mapper | | |
| TASK-084 | Update `state.service.ts` — already uses mapper | | |
| TASK-085 | Update `location.service.ts`, `movement.service.ts`, `transfer.service.ts` — wrap with mappers | | |
| TASK-086 | Update `fulfillment.service.ts` — wrap with mapper | | |
| TASK-087 | Verify: `pnpm run type-check` passes | | |

### Phase 6: Update views/components to use model types

- GOAL-006: Components reference `*.model.type.ts` instead of `*.response.type.ts`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-088 | Update all views that consume order data — `state`→`status` rename, `totalCents`→`total`, add `totalDisplay` from mapper | | |
| TASK-089 | Update all views that consume product data — remove references to removed fields (`imageUrl`, `sku`, `price` on ProductSummary) | | |
| TASK-090 | Update all views that consume stock item data — `sku`/`variantName`/`stockLocationName` now come from mapper | | |
| TASK-091 | Update all views that consume user data — remove references to removed fields | | |
| TASK-092 | Update all views that consume login response — split into login + session | | |
| TASK-093 | Update all views that consume countries — add `zipcodeRequired` handling | | |
| TASK-094 | Update test files that mock response data | | |
| TASK-095 | Verify: `pnpm run type-check` passes | | |

### Phase 7: Add mapper tests

- GOAL-007: Each mapper function has a unit test

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-096 | `auth.mapper.spec.ts` — test JWT extraction + login response mapping | | |
| TASK-097 | `product.mapper.spec.ts` — test summary and detail mapping | | |
| TASK-098 | `order.mapper.spec.ts` — test decimal→cents, enum→label, field rename | | |
| TASK-099 | `user.mapper.spec.ts` — test field mapping | | |
| TASK-100 | `stock-item.mapper.spec.ts` — test display name computation | | |
| TASK-101 | All other domain mapper tests | | |
| TASK-102 | Verify: `pnpm run test:unit` passes | | |

## 3. Alternatives

- **ALT-001**: Keep response types as-is and only add mappers — rejected because the misalignment between types and actual API data causes runtime bugs that type-check cannot catch.
- **ALT-002**: Create a single `apiClient` interceptor layer for all transformations — rejected because per-domain transformations are too varied.
- **ALT-003**: Add mappers only where transformation is needed (skip identity domains) — rejected in favor of consistency (user preference).

## 4. Dependencies

- **DEP-001**: C# response DTO files under `service/Api/src/Module/` — used as source of truth for response type alignment
- **DEP-002**: Existing `*.response.type.ts` files — modified in place (not duplicated)
- **DEP-003**: All stores and services that consume response data — wired to new model types

## 5. Files

- **FILE-001**: `app/Admin/src/shared/utils/enums.ts` — shared enum lookup maps (new)
- **FILE-002**: `app/Admin/src/shared/utils/currency.ts` — currency formatting utils (new)
- **FILE-003**: All 140 `*.response.type.ts` files in `app/Admin/src/features/` — field alignment fixes
- **FILE-004**: All 33 `*.model.type.ts` files in `app/Admin/src/features/` — new frontend model types
- **FILE-005**: All 33 `*.mapper.ts` files in `app/Admin/src/features/` — 24 new + 9 updated
- **FILE-006**: All service files in `app/Admin/src/features/` — mapper wiring
- **FILE-007**: All Vue view/component files — update consumed types
- **FILE-008**: All test files — update mock data

## 6. Testing

- **TEST-001**: `pnpm run type-check` — zero errors across all phases
- **TEST-002**: `pnpm run test:unit` — all existing + new mapper tests pass
- **TEST-003**: Each mapper has a spec file covering: happy path, null/undefined fields, edge case enums

## 7. Risks & Assumptions

- **RISK-001**: Component templates access deprecated fields (`imageUrl`, `sku`, `price` on ProductSummary) — will break unless all callers are updated simultaneously. Mitigation: Phase 6 runs immediately after Phase 2 (no partial state).
- **RISK-002**: Some C# response DTOs may have changed since the audit — verify each against current C# before editing TS.
- **RISK-003**: The `LoginResponse` composite currently assembled in `auth.service.ts` — need to verify what calls it and ensure the mapper produces the same shape.
- **ASSUMPTION-001**: Field changes are backward-compatible because components use dynamic object access — validated by component audit showing direct field access patterns.
- **ASSUMPTION-002**: All enum values (0, 1, 2) map to exact string labels — validated against C# enum definitions.
