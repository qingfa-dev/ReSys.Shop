---
goal: Fix remaining 120 type-check errors from API layer and model refactors
version: 1.0
date_created: 2026-07-07
status: 'Planned'
tags: fix, typecheck, admin, alignment
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The Phase 1–5 refactor (`plan/refactor-admin-api-layer-1.md`) changed all model types to camelCase and the `ApiResult.error` type from `Partial<ApiResponse<unknown>>` to `ParsedApiError`. While `pnpm build-only` (Vite) passes, `pnpm type-check` (`vue-tsc`) reveals 138 errors — 18 pre-existing (missing `@primevue/core/api` and `metadata-manager` module declarations, `GlobalSearch.$t`) and **120 new** errors from template/store references still using old property names.

This plan fixes every actionable error by category, targeting only field renames and type constructor fixes.

## 1. Requirements & Constraints

- **REQ-001**: `pnpm type-check` must report zero errors for all files not in the pre-existing ignore list
- **CON-001**: Do NOT fix pre-existing errors: `@primevue/core/api` module not found, `metadata-manager.component.vue` module not found, `GlobalSearch.vue` `$t` property
- **CON-002**: Read each file before editing — never guess file contents
- **GUD-001**: Use `replaceAll` or targeted edits — do not rewrite entire files

## 2. Implementation Steps

### Implementation Phase 1: PaginationMeta `total_count` → `totalCount`

- GOAL-001: Fix all 16 stores and their `?.total_count` references to use `totalCount`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | `src/features/catalog/products/stores/product.store.ts:56` — change `meta?.total_count` → `meta?.totalCount` | | |
| TASK-002 | `src/features/catalog/option-types/stores/option-type.store.ts:34` — change `meta?.total_count` → `meta?.totalCount` | | |
| TASK-003 | `src/features/catalog/option-types/option-values/stores/option-value.store.ts:46` — same | | |
| TASK-004 | `src/features/catalog/property-types/stores/property-type.store.ts:33` — same | | |
| TASK-005 | `src/features/catalog/taxonomies/stores/taxonomy.store.ts:46` — same | | |
| TASK-006 | `src/features/catalog/taxonomies/taxa/stores/taxon.store.ts:106` — same | | |
| TASK-007 | `src/features/inventories/stores/inventory.store.ts:64,79,99,114` — 4 occurrences | | |
| TASK-008 | `src/features/ordering/stores/order.store.ts:47` — same | | |
| TASK-009 | `src/features/ordering/fulfillment/stores/fulfillment.store.ts:19` — same | | |
| TASK-010 | `src/features/users/stores/user.store.ts:41,56` — 2 occurrences | | |
| TASK-011 | `src/features/location/stores/country.store.ts:19` — same | | |
| TASK-012 | `src/features/location/stores/state.store.ts:19` — same | | |

### Implementation Phase 2: Search param object field renames in stores

- GOAL-002: Fix store query param objects using `page_size`/`sort_by`/`is_descending`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | `src/features/ordering/stores/order.store.ts:27` — `page_size: page.value` → `pageSize: page.value` | | |
| TASK-014 | `src/features/users/stores/user.store.ts:25` — `page_size: page.value` → `pageSize: page.value` | | |

### Implementation Phase 3: Search param field renames in .vue templates

- GOAL-003: Fix all .vue templates passing `page_size`/`sort_by`/`is_descending`/`low_stock` in search param objects.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | `src/features/inventories/views/InventoryUnitList.view.vue:23,29,67,73-75` — rename `page_size`→`pageSize`, `sort_by`→`sortBy`, `is_descending`→`isDescending` in all locations (search params + template accesses) | | |
| TASK-016 | `src/features/inventories/views/StockItemList.view.vue:45,51,69,74,102,109-111,132-134` — rename `page_size`→`pageSize`, `sort_by`→`sortBy`, `is_descending`→`isDescending`, `low_stock`→`lowStock` | | |
| TASK-017 | `src/features/inventories/views/StockTransferList.view.vue:24,61` — rename `page_size`→`pageSize` | | |
| TASK-018 | `src/features/ordering/views/order-list.view.vue:51,62,144,152-154` — rename `page_size`→`pageSize`, `sort_by`→`sortBy`, `is_descending`→`isDescending` | | |
| TASK-019 | `src/features/users/views/admin-user-list.view.vue:35,41,108-111` — rename `page_size`→`pageSize`, `sort_by`→`sortBy`, `is_descending`→`isDescending` | | |
| TASK-020 | `src/features/users/views/customer-list.view.vue:28,34,81,89-91` — rename `page_size`→`pageSize`, `sort_by`→`sortBy`, `is_descending`→`isDescending` | | |

### Implementation Phase 4: Order/address/payment component field renames

- GOAL-004: Fix ordering components referencing old field names on AddressDetail, PaymentDetail, LineItemDetail, etc.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-021 | `src/features/ordering/components/AddressDialog.vue` — rename ALL occurrences of `firstname`→`firstName`, `lastname`→`lastName`, `zipcode`→`zipCode`, `country_code`→`countryCode`, `shipping_address`→`shippingAddress`. Read the full file and replace all 14+ occurrences. | | |
| TASK-022 | `src/features/ordering/components/ItemDialog.vue:51` — rename `variant_id`→`variantId` in object literal | | |
| TASK-023 | `src/features/ordering/components/RefundDialog.vue:17,22,34,38,44` — rename `amount_cents`→`amountCents`, `method_type`→`methodType` | | |
| TASK-024 | `src/features/ordering/components/ShipmentDialog.vue:26,28,53` — rename `line_items`→`lineItems`, `stock_location_id`→`stockLocationId`. Fix implicit `any` on line 28 and 30 callback params by adding `(item: any)` and `(unit: any)` types. | | |
| TASK-025 | `src/features/ordering/views/order-form.view.vue:116` — rename `line_items`→`lineItems` | | |

### Implementation Phase 5: Inventory component field renames

- GOAL-005: Fix inventory views referencing old field names.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-026 | `src/features/inventories/views/StockLocationForm.view.vue:179,183` — rename `zip_code`→`zipCode`, `country_code`→`countryCode` | | |
| TASK-027 | `src/features/inventories/views/StockLocationList.view.vue:54` — rename `country_code`→`countryCode` | | |

### Implementation Phase 6: Catalog form field renames

- GOAL-006: Fix catalog forms referencing old/removed fields.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-028 | `src/features/catalog/option-types/views/option-type-form.view.vue:146,150,151` — remove references to `description`, `publicMetadata`, `privateMetadata` (no longer on `OptionTypeDetail` type). Read the file to determine how these were used — if they were v-model bindings, just remove them; if display-only, remove the display sections. | | |
| TASK-029 | `src/features/catalog/taxonomies/views/taxonomy-form.view.vue:55,56` — remove references to `public_metadata`, `private_metadata` (no longer on `TaxonomyDetail` type) | | |
| TASK-030 | `src/features/catalog/dashboard/views/CatalogDashboard.vue:40,44,57,67,71,84` — rename all 6 snake_case fields to camelCase: `active_products`→`activeProducts`, `total_products`→`totalProducts`, `total_variants`→`totalVariants`, `total_taxonomies`→`totalTaxonomies`, `total_taxons`→`totalTaxons`, `total_digital_products`→`totalDigitalProducts` | | |

### Implementation Phase 7: Product form field renames (complex)

- GOAL-007: Fix all field references in `product-form.view.vue` — this is the most complex single file.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-031 | Read `src/features/catalog/products/views/product-form.view.vue` fully. Fix all these errors systematically: | | |
| TASK-031a | Line 38: object literal with `is_active` — change to reflect new form shape. If `is_active` was a boolean toggle, migrate to `availableOn` (date string) or keep as a local computed. | | |
| TASK-031b | Lines 56-57: `'is_active'` and `'is_visible'` string arguments — remove or replace with new field names | | |
| TASK-031c | Line 62: `'brand'` string argument — remove (not a field anymore) | | |
| TASK-031d | Lines 63-65: `'meta_title'` → `'metaTitle'`, `'meta_description'` → `'metaDescription'`, `'meta_keywords'` → `'metaKeywords'` | | |
| TASK-031e | Line 87: object literal with `is_active` — same fix as line 38 | | |
| TASK-031f | Lines 238, 317, 321, 325: `Nullable<string>` type assignment errors — cast values as needed (these are template bindings where generic API values need narrowing) | | |
| TASK-031g | Lines 253, 260: `string | boolean | undefined` type assignment errors — same fix | | |

### Implementation Phase 8: Profile, User, and other component field renames

- GOAL-008: Fix remaining component field references.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-032 | `src/features/profile/views/Profile.view.vue:38,129` — rename `phone`→`phoneNumber` (read the file, both occurrences in template/directive) | | |
| TASK-033 | `src/features/users/components/UserSecurityManager.vue:60` — rename `verify_email`→`verifyEmail` in object literal | | |

### Implementation Phase 9: Test/spec data fixes

- GOAL-009: Fix test files that construct `ApiResult` error objects with old field names.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-034 | `src/features/auth/_tests/auth.store.spec.ts:113` — the error object `{ title: string, status: number }` needs all `ParsedApiError` required fields: add `statusCode`, `message`, `detail`, `isSuccess`, `errors`, `error_code` | | |
| TASK-035 | `src/features/auth/stores/auth.store.ts:64` — the error object `{ title: string, status: number }` needs all `ParsedApiError` required fields | | |
| TASK-036 | `src/features/catalog/products/tests/product.store.spec.ts:62` — the error object `{ title: string }` needs all `ParsedApiError` required fields | | |
| TASK-037 | `src/shared/composables/api-error-handler.use.spec.ts:83,99` — the `Partial<ServerResult<unknown>>` objects need to be `ParsedApiError` — add all required fields | | |

### Implementation Phase 10: Verify

- GOAL-010: Run full type-check and ensure zero actionable errors remain.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-038 | Run `pnpm type-check` — count remaining errors. Only `@primevue/core/api`, `metadata-manager`, and `GlobalSearch.$t` errors should remain (18 total). | | |
| TASK-039 | Run `pnpm build-only` — ensure Vite build still passes | | |
| TASK-040 | Run `pnpm test:unit` — ensure all 107 tests still pass | | |

## 3. Alternatives

- **ALT-001**: Suppress the header file errors globally — rejected because the model changes are correct and the template errors must be fixed to maintain type safety.

## 4. Dependencies

- **DEP-001**: Phase 1–5 refactor from `plan/refactor-admin-api-layer-1.md` must be complete (it is).

## 5. Files

- **FILE-001 to FILE-030**: All .vue template files, store .ts files, test .ts files listed in the tasks above.

## 6. Testing

- **TEST-001**: `pnpm type-check` — zero actionable errors
- **TEST-002**: `pnpm build-only` — passes
- **TEST-003**: `pnpm test:unit` — all 107 tests pass

## 7. Risks & Assumptions

- **RISK-001**: `product-form.view.vue` has complex form logic — the `is_active`→`availableOn` migration may need runtime behavior adjustment beyond just renaming. If `is_active` was a boolean toggle and `availableOn` is a date string, the form semantics change. In that case, keep the local ref as a boolean but map it differently when calling the API.
- **ASSUMPTION-001**: The 18 pre-existing errors (`@primevue/core/api`, `metadata-manager`, `GlobalSearch.$t`) are unrelated to this work and are safe to ignore.

## 8. Related Specifications / Further Reading

- `plan/refactor-admin-api-layer-1.md` — prior phase that introduced all camelCase renames
