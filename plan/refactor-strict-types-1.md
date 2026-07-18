---
goal: Eliminate all 'any' type usages across Admin SPA (test + production code)
version: 1.0
date_created: 2026-07-18
status: 'Planned'
tags: refactor, typescript, strict-typing, admin-spa
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Replace every TypeScript `any` (type annotation, cast, generic, array) in
`app/Admin/src/` with proper types — strict-mode compliant, no regressions.
The Store SPA (`app/Store/src/`) already has zero `any` usages.

63 occurrences across 30 files: ~50% in test files (`*.spec.ts`), ~50% in
production code (views, components, services, stores, shared utilities).

## 1. Requirements & Constraints

- **REQ-001**: Every `any` must be replaced with a specific type or a
  well-defined type alias — no blanket `unknown` without narrowing.
- **REQ-002**: Test files may keep `as any` only where mocking a class
  constructor or module — all other test `as any` must be replaced with
  explicit mock types.
- **REQ-003**: `Record<string, any>` must become `Record<string, unknown>`
  with explicit narrowing at consumption sites.
- **REQ-004**: PrimeVue DataTable filter workarounds
  (`(filters.global as any).value`) must use a shared type utility or
  unwrap helper.
- **REQ-005**: No behavioral change — tests must pass, views must render.
- **REQ-006**: `vue-tsc --noEmit` must pass with zero `any`-related errors.
- **CON-001**: `defineSlots<>` return types (`() => any`) are a Vue 3
  limitation — may keep `any` if no `VueSlots` approach works (rare).
- **GUD-001**: Prefer `interface` over `type` for object shapes.
- **GUD-002**: Prefer `unknown` over `any` when type is genuinely
  dynamic; narrow with type guards or `as` at the usage site.

## 2. Implementation Steps

### Phase 1 — Shared utilities & type definitions

- GOAL-001: Create shared utility types for filter access, metadata,
  and mock helpers so downstream phases can import them.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `shared/api/types/filter.types.ts` — `FilterValue` helper type to unwrap PrimeVue `DataTableFilterMeta` without `any` | | |
| TASK-002 | Create `shared/api/types/metadata.types.ts` — `Metadata` = `Record<string, unknown>` + `getMetadata(key)` / `setMetadata(key, val)` helper | | |
| TASK-003 | Create `shared/test/mock-types.ts` — generic `MockData<T>` / `MockResponse<T>` wrappers so tests avoid `as any` on mock data | | |

### Phase 2 — Production code: PrimeVue filter casts

- GOAL-002: Replace all `(filters.{name} as any).value` patterns with
  the `FilterValue` helper. 10+ occurrences across 6 list views.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | `CustomerList.View.vue:96` — replace `(filters.global as any).value` | | |
| TASK-005 | `AdminUserList.View.vue:120` — same pattern | | |
| TASK-006 | `OrderList.View.vue:160` — same pattern | | |
| TASK-007 | `TaxonList.View.vue:68,69,143,150` — `global` + `taxonomyId` filters | | |
| TASK-008 | `TaxonomyList.View.vue:161` — global filter | | |
| TASK-009 | `ProductList.View.vue:176` — global filter | | |
| TASK-010 | `OptionTypeList.View.vue:155` — global filter | | |
| TASK-011 | `OptionValueList.View.vue:68,145,193,245,252` — `global` + `optionTypeId` filters | | |

### Phase 3 — Production code: Metadata `Record<string, any>`

- GOAL-003: Replace all `Record<string, any>` with
  `Record<string, unknown>` + narrow at each read site.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-012 | `MetadataManager.Component.vue:10,15,33` — model type, `MetadataEntry.value`, local accumulator | | |
| TASK-013 | `TaxonForm.Component.vue:55,56` — public/private metadata refs | | |
| TASK-014 | `OptionTypeForm.View.vue:32,33` — public/private metadata refs | | |

### Phase 4 — Production code: Component event/param/slot types

- GOAL-004: Replace function parameter `: any` annotations and slot
  returns with proper interfaces.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | `PageHeader.Component.vue:13,14` — `badge?(): any` / `actions?(): any` — attempt `VNodeTypes` or `VNode[]`; keep `any` only if Vue `defineSlots` rejects alternatives | | |
| TASK-016 | `Configurator.Layout.vue:210` — `(e: any)` → `(e: { value: string })` for PrimeVue `SelectButton` change | | |
| TASK-017 | `TaxonForm.Component.vue:19` — emit `(values: any)` → typed form values interface | | |
| TASK-018 | `ProductImageUploader.Component.vue:29` — `(event: any)` → `(event: Event)` + typed file extraction | | |
| TASK-019 | `ProductImageList.Component.vue:31` — `(roleVal: any)` → typed image role union | | |

### Phase 5 — Production code: Inventory tree & view handlers

- GOAL-005: Type all tree node parameters and handler arguments in
  Inventory views — PrimeVue `Tree` / `TreeTable` node types.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-020 | Create `features/inventories/types/tree.types.ts` — `TreeNodeData` interface matching PrimeVue `Tree` node shape | | |
| TASK-021 | `StockLocationManager.View.vue:30,37,41,116` — replace `: any` params with `TreeNodeData` | | |
| TASK-022 | `StockLocationManager.View.vue:114` — remove `as any` on `:value` | | |
| TASK-023 | `StockLocationList.View.vue:75` — remove `as any` on `:value` | | |
| TASK-024 | `StockLocationForm.View.vue:85` — remove `(res.value as any).parent_id` with typed response access | | |
| TASK-025 | `StockItemList.View.vue:23,27,33` — type `selectedStockItem` ref + `showHistory`/`showAdjust` params | | |
| TASK-026 | `StockTransferDetail.View.vue:29,30` — type `selectedProduct` ref + `productResults` array | | |
| TASK-027 | `StockMovementTimeline.Component.vue:22` — remove `as any` on query params | | |

### Phase 6 — Production code: Catalog taxonomy/product handlers

- GOAL-006: Type taxonomy tree node mapping, product classification,
  option type manager, variant generation dialog.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-028 | Create `features/catalog/taxonomies/types/taxonomy.types.ts` — `TaxonTreeNode` interface for tree operations | | |
| TASK-029 | `TaxonTreeManager.View.vue:123` — `{ node }: { node: any }` → typed | | |
| TASK-030 | `TaxonForm.Component.vue:78,80` — remove `as any` on `parentId` / `rulesMatchPolicy` form set | | |
| TASK-031 | `TaxonProductsPreview.Component.vue:17` — type `products` ref array | | |
| TASK-032 | `ProductClassificationManager.Component.vue:26,37,47` — type `trees` map, API cast, `mapNode` param | | |
| TASK-033 | `ProductOptionTypeManager.Component.vue:18,36` — type `availableOptionTypes` array + map callback | | |
| TASK-034 | `VariantGenerationDialog.Component.vue:30,31,32,70,78,81,89` — type all arrays, combinations, and callbacks | | |

### Phase 7 — Production code: Ordering item/shipment dialogs

- GOAL-007: Type all `any` params and refs in `ItemDialog` and
  `ShipmentDialog`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-035 | `ItemDialog.Component.vue:19,20,24,39,43,44,52` — type productResults, selectedProduct, variants, callbacks, current_product access | | |
| TASK-036 | `ShipmentDialog.Component.vue:28,31,33` — type units array, lineItem callback, unit callback | | |

### Phase 8 — Production code: Services, repositories, stores

- GOAL-008: Replace `: any` / `as any` / `Promise<any>` in service,
  repository, and store files with proper response types.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-037 | `taxon.service.ts:19` — replace `{ items: any[] }` return with typed product preview interface | | |
| TASK-038 | `fulfillment.service.ts:6` — replace `Promise<any>` return with typed shipment result | | |
| TASK-039 | `inventory.service.ts:64,65` — replace `ServerResult<any>` with typed `StockSummary` result | | |
| TASK-040 | `stock.repository.ts:35,36` — same replacement in repository layer | | |
| TASK-041 | `option-value.store.ts:25` — remove `as any` on query params spread | | |
| TASK-042 | `auth.store.ts:56,65` — replace `value: undefined as any` / `value: null as any` with `value: undefined as never` or typed `ServerResult<void>` constructor | | |
| TASK-043 | `Profile.View.vue:14` — type `user` ref with `AdminUserSummary` or a profile interface | | |

### Phase 9 — Production code: Catalog list view filter casts (remaining)

- GOAL-009: Catch any remaining PrimeVue filter cast patterns in catalog
  list views not already covered (from Phase 2 overlap).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-044 | `features/catalog/option-types/views/OptionValueList.View.vue` — verify all 5 filter casts converted (already in TASK-011) — close after verification | | |

### Phase 10 — Test files: Mock data `as any`

- GOAL-010: Replace all test-file `as any` with typed mocks using
  `MockData<T>` / `MockResponse<T>` from TASK-003 or inline factory
  functions.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-045 | `auth.store.spec.ts:117` — replace `value: null as any` | | |
| TASK-046 | `auth.service.spec.ts:37,61,74` — replace `as any` mock responses | | |
| TASK-047 | `api.client.spec.ts:25,89` — replace interceptor `as any` | | |
| TASK-048 | `taxon.store.spec.ts:25,26` — replace `as any` mock data | | |
| TASK-049 | `catalog.api.spec.ts:41` — replace payload `as any` | | |
| TASK-050 | `taxonomy.store.spec.ts:40,49` — replace `as any` | | |
| TASK-051 | `product.store.spec.ts:42,54,76` — replace `as any` | | |
| TASK-052 | `order.store.spec.ts:38,50,63,72,94,103` — replace `as any` | | |
| TASK-053 | `option-value.store.spec.ts:42,67,89,110,119,121` — replace `as any` | | |
| TASK-054 | `option-type.store.spec.ts:41,63,76,89,99,102` — replace `as any` | | |
| TASK-055 | `fulfillment.store.spec.ts:34,46,68,80,99` — replace `as any` | | |

### Phase 11 — Final verification

- GOAL-011: Build-pass, lint-pass, test-pass — zero `any` remaining.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-056 | Run `pnpm run lint` — confirm zero lint errors | | |
| TASK-057 | Run `npx vue-tsc --noEmit` — confirm zero type errors | | |
| TASK-058 | Run `pnpm run test:unit` — confirm all tests pass | | |
| TASK-059 | Grep for `: any`, `as any`, `<any>`, `any[]`, `Array<any>` in `app/Admin/src/` — confirm zero remaining | | |

## 3. Alternatives

- **ALT-001** (rejected): Keep `any` and add `eslint @typescript-eslint/no-explicit-any: error` — would leave all existing
  violations unfixed and block CI. The plan fixes proactively.
- **ALT-002** (rejected): Global `unknown` blanket replace — `unknown`
  without narrowing is as bad as `any` at consumption sites. Each site
  needs a purpose-specific type.
- **ALT-003** (rejected): Use `@ts-expect-error` to silence — hides bugs
  instead of fixing them.

## 4. Dependencies

- **DEP-001**: Phase 1 must complete before Phase 2 (shared types needed).
- **DEP-002**: Phase 3 (metadata) types are imported by Phase 4+ components
  — execution may be parallel if imports are stable.
- **DEP-003**: Test Phase 10 depends on `MockData<T>` from TASK-003.
- **DEP-004**: No external library changes needed — all types are
  codebase-native or PrimeVue declarations.

## 5. Files

- **FILE-001**: `app/Admin/src/shared/api/types/filter.types.ts` — new
- **FILE-002**: `app/Admin/src/shared/api/types/metadata.types.ts` — new
- **FILE-003**: `app/Admin/src/shared/test/mock-types.ts` — new
- **FILE-004**: `app/Admin/src/shared/components/PageHeader.Component.vue` — slot return type fix
- **FILE-005**: `app/Admin/src/shared/components/MetadataManager.Component.vue` — `any` → `unknown`
- **FILE-006**: `app/Admin/src/features/auth/stores/auth.store.ts` — `as any` fix
- **FILE-007**: `app/Admin/src/features/auth/views/Profile.View.vue` — `ref<any>`
- **FILE-008**: `app/Admin/src/features/catalog/taxonomies/types/taxonomy.types.ts` — new
- **FILE-009**: `app/Admin/src/features/catalog/taxonomies/**/*.vue` — 4 files
- **FILE-010**: `app/Admin/src/features/catalog/taxonomies/taxa/services/taxon.service.ts` — return type
- **FILE-011**: `app/Admin/src/features/catalog/products/**/*.vue` — 4 files
- **FILE-012**: `app/Admin/src/features/catalog/option-types/**/*.vue` — 3 files
- **FILE-013**: `app/Admin/src/features/catalog/option-types/option-values/**/*` — 3 files
- **FILE-014**: `app/Admin/src/features/ordering/**/*.vue` — 2 dialogs
- **FILE-015**: `app/Admin/src/features/ordering/fulfillment/services/fulfillment.service.ts` — return type
- **FILE-016**: `app/Admin/src/features/inventories/types/tree.types.ts` — new
- **FILE-017**: `app/Admin/src/features/inventories/**/*.vue` — 5 files
- **FILE-018**: `app/Admin/src/features/inventories/services/inventory.service.ts` — return type
- **FILE-019**: `app/Admin/src/features/inventories/stock-items/repositories/stock.repository.ts` — return type
- **FILE-020**: `app/Admin/src/features/users/**/*.*` — 2 views
- **FILE-021**: `app/Admin/src/app/layout/Configurator.Layout.vue` — event type
- **FILE-022**: All 15 `*.spec.ts` files with `as any` casts

## 6. Testing

- **TEST-001**: `pnpm run test:unit` — all 200+ existing tests must pass
  (no test behavior changed, only mock types).
- **TEST-002**: `npx vue-tsc --noEmit` — zero type errors.
- **TEST-003**: `pnpm run lint` — zero ESLint errors.
- **TEST-004**: Grep check — confirm zero `: any`, `as any`, `<any>`,
  `any[]`, `Array<any>` remain in `app/Admin/src/` (excluding
  `node_modules`).

## 7. Risks & Assumptions

- **RISK-001**: Vue `defineSlots` may not accept `VNode[]` return types
  in all Vue 3.5+ versions — TASK-015 may need to keep `any` with an
  eslint-disable comment if the API rejects alternatives.
- **RISK-002**: Test `as any` on mock module-level function mocks (e.g.,
  `vi.mock()`) may not be replaceable — `as any` used in `mockResolvedValue`
  wrapping is often required by Vitest internals. Each test site must be
  evaluated individually.
- **ASSUMPTION-001**: The `app/Store/` SPA has zero `any` and sets the
  correctness target — no backsliding in Admin will be accepted after this
  plan is complete.
- **ASSUMPTION-002**: PrimeVue filter types (`DataTableFilterMeta`) do not
  expose `.value` — the `FilterValue` helper from TASK-001 will use
  `unknown` with a safe-access pattern rather than fighting PrimeVue's
  declarations.

## 8. Related Specifications / Further Reading

- TypeScript Strict Mode: https://www.typescriptlang.org/tsconfig/#strict
- Vue 3 `defineSlots` API: https://vuejs.org/api/sfc-script-setup.html#defineslots
- PrimeVue DataTable filter types reference in `node_modules/primevue/datatable`
- [ESLint `@typescript-eslint/no-explicit-any`](https://typescript-eslint.io/rules/no-explicit-any/)
