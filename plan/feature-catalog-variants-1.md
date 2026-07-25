---
goal: Implement admin catalog Variants, Prices, OptionValues, and Images frontend API services
version: 1.0
date_created: 2026-07-25
status: Planned
tags: feature, api, catalog, admin
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Implement frontend service layer for Variants and 3 nested child entities (Prices, OptionValues, Images) in `app/Admin/src/features/catalog/`. Follows established patterns: types -> schemas -> mappers -> API -> store -> composable -> pages -> components -> routes -> barrels.

## 1. Requirements & Constraints

- **REQ-001**: Every backend CRUD endpoint must have a corresponding frontend API method
- **REQ-002**: All API methods use shared `apiClient` (axios) and `getPagedList` helper
- **REQ-003**: Response types as camelCase interfaces matching backend C# records
- **REQ-004**: Request types alias form schemas or define standalone interfaces
- **REQ-005**: Zod validation schemas for entities with create/update forms
- **REQ-006**: Form-to-request mapper classes with static `toCreate`/`toUpdate`
- **REQ-007**: Child entities take parent ID as first API parameter
- **REQ-008**: Child entities with forms get standalone form components, not pages
- **REQ-009**: Top-level entities with list pages get Pinia stores; children do not
- **REQ-010**: New route patterns in `routes.ts`, new exports in `index.ts` barrels
- **CON-001**: Follow existing conventions exactly (see OptionType/OptionValue patterns)
- **CON-002**: Zero TypeScript errors allowed (TreatWarningsAsErrors)
- **PAT-001**: API classes with static methods wrapping apiClient calls
- **PAT-002**: Store with `defineStore('catalog-{entity}', () => { ... })`, readonly refs
- **PAT-003**: Form components use vee-validate + zod schema
- **PAT-004**: Mapper classes with static `toCreate`/`toUpdate`

## 2. Implementation Steps

### Phase 1: Variants core (types, schemas, mappers, API, store, composable, pages, components)

- GOAL-001: Implement complete Variants CRUD feature

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `types/variant.response.ts` — `VariantDetailResponse`: id, productId, isMaster, sku, position, trackInventory, weight?, weightUnit?, height?, width?, depth?, dimensionsUnit?, price?, costPrice?, costCurrency?, discontinuedOn?, pricesCount, createdAt, updatedAt. `VariantListItemResponse`: id, productId, isMaster, sku, position, trackInventory, weight?, weightUnit?, price?, createdAt | | |
| TASK-002 | Create `types/variant.request.ts` — `VariantRequest`: sku, position, trackInventory, weight?, weightUnit?, height?, width?, depth?, dimensionsUnit?, price?, costPrice?, costCurrency?, isMaster, optionValueIds?: string[] | | |
| TASK-003 | Create `schemas/variant.fields.ts` — `VariantFields`: sku (required string), position (number min 0), trackInventory (boolean), weight/height/width/depth (optional decimal), weightUnit (optional string), dimensionsUnit (optional string), price/costPrice (optional decimal), costCurrency (optional string), isMaster (boolean) | | |
| TASK-004 | Create `schemas/variant.forms.ts` — `VariantForms` class with create()/update() schemas; export `CreateVariantForm`, `UpdateVariantForm` | | |
| TASK-005 | Create `mappers/variant.mapper.ts` — `VariantFormMapper` with static toCreate, toUpdate | | |
| TASK-006 | Create `api/variant.api.ts` — `VariantApi`: getMany(productId, query) via getPagedList `/catalog/products/${productId}/variants`; create(productId, data) POST same url; get(id) GET `/catalog/products/variants/${id}`; update(id, data) PUT same; delete(id) DELETE same | | |
| TASK-007 | Create `store/variant.store.ts` — `useVariantStore` with items, loading, error, totalRecords, query, fetchMany(productId), setPage, setSearch, setSort, setFilter, resetQuery | | |
| TASK-008 | Create `composables/useVariant.ts` — returns { id, mode, route, router, toast, api: VariantApi, priceApi, optionValueApi, imageApi } | | |
| TASK-009 | Create `components/VariantForm.vue` — vee-validate form with: sku, position, trackInventory, weight+unit, height/width/depth+unit, price/costPrice/costCurrency, isMaster; load->getVariantById; save->create/update; placeholder sections for prices/optionValues/images | | |
| TASK-010 | Create `components/VariantListTable.vue` — DataTable with store, columns: sku, position, isMaster, trackInventory, price, ActionMenu | | |
| TASK-011 | Create `pages/VariantListPage.vue` — PageHeader + VariantListTable | | |
| TASK-012 | Create `pages/VariantDetailPage.vue` — VariantForm | | |
| TASK-013 | Update `routes.ts` — add VARIANT constant: LIST, CREATE, VIEW, EDIT; routes: `catalog/products/:productId/variants`, `.../new`, `.../:id`, `.../:id/edit` | | |
| TASK-014 | Update `schemas/index.ts`, `types/index.ts`, `mappers/index.ts`, `api/index.ts`, `index.ts` — add all barrel exports | | |
| TASK-015 | Verify: `pnpm build` passes | | |

### Phase 2: Variant Prices (child entity)

- GOAL-002: Implement Variant Prices API layer + inline UI component

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-016 | Create `types/variant-price.response.ts` — `VariantPriceResponse`: id, variantId, amount?, currency, compareAtAmount?, countryIso?; `SyncPricesResponse`: extends VariantPriceResponse + added, updated, removed (number) | | |
| TASK-017 | Create `types/variant-price.request.ts` — `VariantPriceRequest`: amount?: number, currency: string, compareAtAmount?: number, countryIso?: string; `SyncPriceItem` same fields; `SyncPricesRequest`: prices: SyncPriceItem[] | | |
| TASK-018 | Create `schemas/variant-price.fields.ts` — `VariantPriceFields`: amount (optional decimal), currency (required string), compareAtAmount (optional decimal), countryIso (optional string) | | |
| TASK-019 | Create `schemas/variant-price.forms.ts` — `VariantPriceForms` with create() schema; export `VariantPriceForm` | | |
| TASK-020 | Create `mappers/variant-price.mapper.ts` — `VariantPriceFormMapper` with toCreate | | |
| TASK-021 | Create `api/variant-price.api.ts` — `VariantPriceApi`: list(variantId) GET `/catalog/products/variants/${variantId}/prices`; set(variantId, data) POST same; remove(variantId, priceId) DELETE `.../prices/${priceId}`; sync(variantId, data) POST `.../prices/sync` | | |
| TASK-022 | Create `components/VariantPriceManager.vue` — DataTable inside VariantForm; columns: amount, currency, compareAtAmount, countryIso, actions; Add button -> slideover form; set/remove/sync operations | | |
| TASK-023 | Integrate into VariantForm.vue, update all barrel exports | | |
| TASK-024 | Verify: `pnpm build` passes | | |

### Phase 3: Variant OptionValues (child entity, assign/revoke pattern)

- GOAL-003: Implement Variant OptionValues API layer + inline manager (no forms, just ID lists)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-025 | Create `types/variant-option-value.response.ts` — `VariantOptionValueItem`: optionValueId, optionTypeId, optionTypeName, name, presentation?, isAssigned; `VariantOptionValuesResponse`: items: VariantOptionValueItem[] | | |
| TASK-026 | Create `types/variant-option-value.request.ts` — `OptionValueIdsRequest`: optionValueIds: string[] | | |
| TASK-027 | Create `api/variant-option-value.api.ts` — `VariantOptionValueApi`: get(variantId) GET `/catalog/products/variants/${variantId}/option-values`; assign POST `.../assign`; revoke POST `.../revoke`; sync PUT same url (no sub-path) | | |
| TASK-028 | Create `components/VariantOptionValueManager.vue` — list of option values grouped by optionType with checkboxes; toggle assign/revoke on change | | |
| TASK-029 | Integrate into VariantForm.vue, update barrels | | |
| TASK-030 | Verify: `pnpm build` passes | | |

### Phase 4: Variant Images (child entity, multipart upload)

- GOAL-004: Implement Variant Images API layer + image gallery component

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-031 | Create `types/variant-image.response.ts` — `VariantImageDetailResponse`: id, variantId?, alt?, position, type, url, contentType, fileName, fileSize, width?, height?, dimensionsUnit?, createdAt; `VariantImageListResponse`: images: VariantImageDetailResponse[]; `EmbeddingDetailResponse`: id, variantImageId, modelName, modelVersion, dimensions, createdAt | | |
| TASK-032 | Create `types/variant-image.request.ts` — `UploadImageRequest`: file: File, alt?: string, position: number, type: string; `UpdateImageMetadataRequest`: alt?: string, position: number, type: string; `EmbeddingRequest`: modelName: string, modelVersion: string | | |
| TASK-033 | Create `schemas/variant-image.fields.ts` — alt (optional string), position (number), type (required string, must be valid image type) | | |
| TASK-034 | Create `schemas/variant-image.forms.ts` — `VariantImageForms` with update() schema; export `VariantImageForm` | | |
| TASK-035 | Create `mappers/variant-image.mapper.ts` — `VariantImageFormMapper` with toUpdate | | |
| TASK-036 | Create `api/variant-image.api.ts` — `VariantImageApi`: list(variantId) GET `/catalog/products/variants/${variantId}/images`; get(imageId) GET `.../images/${imageId}`; upload(variantId, formData) POST `.../images` (multipart); update(imageId, data) PUT `.../images/${imageId}`; delete(imageId) DELETE same; download(imageId) GET `.../download` (blob); createEmbedding(imageId, data) POST `.../embeddings`; regenerateEmbedding(imageId, data) PUT `.../embeddings` | | |
| TASK-037 | Create `components/VariantImageGallery.vue` — image grid with upload, thumbnail, delete, metadata edit slideover, embedding status + regenerate | | |
| TASK-038 | Integrate into VariantForm.vue, update barrels | | |
| TASK-039 | Verify: `pnpm build` passes | | |

## 3. Alternatives

- **ALT-001**: Merge Variant, Price, OptionValue, Image into single monolithic VariantForm — rejected: violates single-responsibility component pattern
- **ALT-002**: Use composable-based state instead of Pinia store for Variants — rejected: consistent with existing entity store pattern

## 4. Dependencies

- **DEP-001**: Existing `apiClient` and `getPagedList` helpers in `@/shared/api/utils/`
- **DEP-002**: Existing `ListQuery`, `PagedResult`, `Result` types in `@/shared/models/`
- **DEP-003**: Existing `ProductForm.vue` — Variant integration depends on product detail page
- **DEP-004**: Existing OptionValueApi — VariantOptionValueManager may need to fetch available option values

## 5. Files

- **FILE-001**: `types/variant.response.ts`, `types/variant.request.ts`
- **FILE-002**: `schemas/variant.fields.ts`, `schemas/variant.forms.ts`
- **FILE-003**: `mappers/variant.mapper.ts`
- **FILE-004**: `api/variant.api.ts`
- **FILE-005**: `store/variant.store.ts`
- **FILE-006**: `composables/useVariant.ts`
- **FILE-007**: `components/VariantForm.vue`, `components/VariantListTable.vue`
- **FILE-008**: `pages/VariantListPage.vue`, `pages/VariantDetailPage.vue`
- **FILE-009**: `types/variant-price.response.ts`, `types/variant-price.request.ts`
- **FILE-010**: `schemas/variant-price.fields.ts`, `schemas/variant-price.forms.ts`
- **FILE-011**: `mappers/variant-price.mapper.ts`
- **FILE-012**: `api/variant-price.api.ts`
- **FILE-013**: `components/VariantPriceManager.vue`
- **FILE-014**: `types/variant-option-value.response.ts`, `types/variant-option-value.request.ts`
- **FILE-015**: `api/variant-option-value.api.ts`
- **FILE-016**: `components/VariantOptionValueManager.vue`
- **FILE-017**: `types/variant-image.response.ts`, `types/variant-image.request.ts`
- **FILE-018**: `schemas/variant-image.fields.ts`, `schemas/variant-image.forms.ts`
- **FILE-019**: `mappers/variant-image.mapper.ts`
- **FILE-020**: `api/variant-image.api.ts`
- **FILE-021**: `components/VariantImageGallery.vue`
- **FILE-022**: `routes.ts` (updated)
- **FILE-023**: `index.ts` (updated barrels), `schemas/index.ts`, `types/index.ts`, `mappers/index.ts`, `api/index.ts`

## 6. Testing

- **TEST-001**: `api/__tests__/variants.spec.ts` — mock apiClient, verify all 5 methods call correct URL/method
- **TEST-002**: `api/__tests__/variant-prices.spec.ts` — mock apiClient, verify list/set/remove/sync
- **TEST-003**: `api/__tests__/variant-option-values.spec.ts` — mock apiClient, verify get/assign/revoke/sync
- **TEST-004**: `api/__tests__/variant-images.spec.ts` — mock apiClient, verify all 8 methods
- **TEST-005**: `store/__tests__/variant.store.spec.ts` — Pinia store unit test with mock API

## 7. Risks & Assumptions

- **RISK-001**: Variant routes use mixed URL patterns (`products/{productId}/variants` for list/create vs `products/variants/{id}` for get/update/delete) — easy to get wrong in API class
- **ASSUMPTION-001**: Backend `ListVariantsByProduct` returns `PagedResult<VariantListItemResponse>` — if returns `Result<List<VariantListItemResponse>>` instead, API method signature must change
- **ASSUMPTION-002**: Existing `TaxonResponse`/`OptionTypeResponse` types are sufficient for building classify/assign UIs

## 8. Related Specifications / Further Reading

Backend endpoint models: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/`
