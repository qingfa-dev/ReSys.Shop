# Catalog Taxon Stores + View Endpoint Corrections — Admin UI Design

**Date**: 2026-07-31
**Scope**: Admin SPA — Catalog module, Taxon management (TaxonsList, TaxonDetail, three taxon stores)
**Decision**: Split taxon state across three dedicated Pinia stores — `taxonStore` (list), `taxonTreeStore` (tree), `taxonDetailStore` (detail + rules) — and correct the taxon views to use proper endpoints through the stores, eliminating a broken empty-URL request and a hardcoded-URL bypass of the service layer.

## Motivation

- `TaxonDetail.vue` uses `usePagedQuery<TaxonRuleListItem>('', ...)` — the empty URL fires a request to `GET /` on mount. Rules are also redundantly fetched by `loadRules()`, so the composable is both broken and duplicated.
- `TaxonsList.vue` passes a hardcoded URL string `'api/catalog/taxonomies/taxons'` to `usePagedQuery`, bypassing the `TaxonApi` service layer that every other catalog page uses.
- `TaxonsList.vue` makes no taxonomy API call at all — the taxonomy selector is missing, so the page can only be scoped via `?taxonomyId=` route query.
- There is no dedicated taxon store. `taxonomyStore`, `optionTypeStore`, and `productStore` exist; taxons are the only entity without one.
- A single mega-store would bundle five unrelated concerns (active cache, list/paging, tree, detail, rules). Splitting into three keeps each store single-purpose and independently testable.
- Backend has a purpose-built taxonomy-scoped listing endpoint (`GET /taxons/list?taxonomyId=X`) that `taxonStore` can use when a taxonomy is selected.

## Architecture

### New file: `stores/taxonStore.ts`

Pinia store (composition style, same as `taxonomyStore`) owning the cached active list and the list/paging state for `TaxonsList`. Mirrors the `usePagedQuery` surface area so the list view loses no behavior.

```
useTaxonStore()                              // Pinia id: 'taxons'
  // cached active list (dropdown/selection use)
  activeTaxons: Ref<TaxonListItem[]>
  loaded: Ref<boolean>
  fetchActive(): Promise<void>

  // list state (replaces usePagedQuery in TaxonsList)
  items: Ref<TaxonListItem[]>
  loading: Ref<boolean>
  error: Ref<string | null>
  page, pageSize, totalCount, totalPages: Ref<number>
  filter, sort, search, searchFields, searchMode: Ref<...>
  selectedTaxonomyId: Ref<string | null>
  fetchList(): Promise<void>
  setPage(p), setPageSize(s), setSort(s), setSearch(s), setFilter(f), setSearchFields(sf), setSearchMode(m)
  setSelectedTaxonomy(id): void
  refresh(): Promise<void>
  reset(): void
```

**Endpoint selection in `fetchList`** — the "correct endpoint" rule:

- `selectedTaxonomyId` set → `TaxonApi.getList(selectedTaxonomyId, query)` → `GET /api/catalog/taxonomies/taxons/list?taxonomyId=X`
- `selectedTaxonomyId` null → `TaxonApi.getTaxons(query)` → `GET /api/catalog/taxonomies/taxons`

**Query building**: The store builds a `TaxonQuery` from its refs (`filter`, `sort`, `search`, `searchFields`, `searchMode`, `page`, `pageSize`) and passes it through the service, so `toTaxonQueryParams` / filter-field whitelisting stays in one place. The taxonomy scope is expressed via `selectedTaxonomyId` (→ `getList`), not via a `taxonomyId=...` filter string.

**fetchActive**: `TaxonApi.getTaxons({ pageSize: 100, sortBy: 'name', sortDirection: 'asc' })` with lazy-once `loaded` guard — identical to `taxonomyStore`.

### New file: `stores/taxonTreeStore.ts`

Pinia store owning only the taxon tree, shared by `TaxonsList` (tree view) and `TaxonDetail` (parent dropdown).

```
useTaxonTreeStore()                          // Pinia id: 'taxonTree'
  tree: Ref<TaxonTreeItem[]>
  treeLoading: Ref<boolean>
  treeTaxonomyId: Ref<string | null>
  fetchTree(taxonomyId): Promise<Result<{ tree: TaxonTreeItem[] }>>
```

**Tree caching**: `fetchTree(taxonomyId)` refetches only when `treeTaxonomyId !== taxonomyId`; otherwise returns the cached `tree`. Set `treeTaxonomyId` on success. The tree response wrapper stays `Result<{ tree: TaxonTreeItem[] }>` — extract via `result.value.tree`. Failures return the failed `Result` to the caller so the view can call `handleResult`.

### New file: `stores/taxonDetailStore.ts`

Pinia store owning the currently-edited taxon and its rules, used by `TaxonDetail`.

```
useTaxonDetailStore()                        // Pinia id: 'taxonDetail'
  currentTaxon: Ref<TaxonDetail | null>
  detailLoading: Ref<boolean>
  fetchDetail(id): Promise<Result<TaxonDetail>>

  rules: Ref<TaxonRuleListItem[]>
  rulesLoading: Ref<boolean>
  fetchRules(taxonId): Promise<PagedResult<TaxonRuleListItem>>
```

`fetchDetail` / `fetchRules` return the failed `Result` to the view so `useApiErrorHandler.handleResult` displays the toast — no silent swallowing.

### Companion fix: `taxonomyStore.ts`

The backend clamps paged results to a default page size of **10** (max 100, see `Page.Bounds.cs`). `taxonomyStore.fetchActive()` currently sends no `pageSize`, so `activeTaxonomies` holds only the first 10 taxonomies. Add `pageSize: 100` to the `getTaxonomies` call so the new TaxonsList selector (and TaxonDetail's existing dropdown) is not truncated.

### `stores/index.ts`

Add exports:

```ts
export { useTaxonStore } from './taxonStore'
export { useTaxonTreeStore } from './taxonTreeStore'
export { useTaxonDetailStore } from './taxonDetailStore'
```

## View Changes

### `TaxonsList.vue`

- **Remove** `usePagedQuery` import and usage; **remove** the hardcoded URL.
- Bind table to `useTaxonStore`: `items`, `loading`, `totalCount`, `page`, `pageSize`; search box → `store.setSearch`; sortable columns → `store.setSort`; paging → `store.setPage` / `store.setPageSize`; Reload → `store.refresh`.
- **Add taxonomy selector** in the toolbar: a `Select` bound to `store.selectedTaxonomyId`, options from `useTaxonomyStore.activeTaxonomies` (`fetchActive()` on mount). On change → `store.setSelectedTaxonomy(id)` + `router.replace({ query: { ...route.query, taxonomyId: id || undefined } })`.
- Prefill `selectedTaxonomyId` from `?taxonomyId=` route query in `onMounted` before the first list fetch.
- Tree view uses `useTaxonTreeStore` (`fetchTree(store.selectedTaxonomyId)` when toggled); `addTreeNodeKeys` mapping stays local.
- Delete flow unchanged at the service level (`TaxonApi.deleteTaxon`), then `store.refresh()` + reload tree if in tree mode.
- "New Taxon" button carries the current `selectedTaxonomyId` in the query.

### `TaxonDetail.vue`

- **Remove** the broken `usePagedQuery<TaxonRuleListItem>('', ...)` block (lines 88-96) and its `refreshRules` reference in `onRuleSaved`.
- Edit mode `onMounted`: `detailStore.fetchDetail(id)` → populate form from `detailStore.currentTaxon`; then `treeStore.fetchTree(taxonomyId)` (parent dropdown) + `detailStore.fetchRules(id)`.
- `loadParents` becomes a local flattening of `treeStore.tree` into `parentOptions` (same depth-indented labels).
- Rules table binds `detailStore.rules` / `detailStore.rulesLoading`; `onRuleSaved` calls `detailStore.fetchRules(route.params.id)`.
- `taxonomyStore.fetchActive()` stays for the taxonomy Select dropdown.
- Create/save/delete still call `TaxonApi` directly from the view (mutations stay in views, matching `TaxonomyDetail`).

## Data Flow

1. `TaxonsList` mount → `taxonomyStore.fetchActive()` (selector options) + prefill `taxonStore.selectedTaxonomyId` from route → `taxonStore.fetchList()`.
2. Selector change → `taxonStore.setSelectedTaxonomy` → `fetchList()` → `router.replace` syncs URL.
3. Tree toggle → `taxonTreeStore.fetchTree(selectedTaxonomyId)` (cached per taxonomy).
4. `TaxonDetail` mount (edit) → `taxonomyStore.fetchActive()` → `taxonDetailStore.fetchDetail(id)` → `taxonTreeStore.fetchTree(taxonomyId)` → `taxonDetailStore.fetchRules(id)`.
5. Rule add/edit/delete → `TaxonRuleApi` mutation → `onRuleSaved` → `taxonDetailStore.fetchRules(id)`.

## Error Handling

- `fetchList` failures populate `taxonStore.error` (mirrors `usePagedQuery`); the empty-state template renders as today.
- `fetchDetail` / `fetchRules` / `fetchTree` failures return the failed `Result` to the calling view so `useApiErrorHandler.handleResult` displays the toast — no silent swallowing.

## Testing

Three new store specs (first store specs in the codebase), one per store:

`__tests__/stores/taxonStore.spec.ts`:

| Test | Assertion |
|------|-----------|
| `fetchList` with `selectedTaxonomyId` | calls `TaxonApi.getList(taxId, query)` → `/taxons/list?taxonomyId=X` |
| `fetchList` without `selectedTaxonomyId` | calls `TaxonApi.getTaxons(query)` → `/taxons` |
| `setPage` / `setSort` / `setSearch` | updates state and refetches list |
| `setSelectedTaxonomy` | clears/sets scope and refetches |
| `fetchActive` | lazy-once guard, populates `activeTaxons` |

`__tests__/stores/taxonTreeStore.spec.ts`:

| Test | Assertion |
|------|-----------|
| `fetchTree` first call | fetches and caches `tree` + `treeTaxonomyId` |
| `fetchTree` same taxonomy | does not refetch (cached) |
| `fetchTree` different taxonomy | refetches and replaces tree |

`__tests__/stores/taxonDetailStore.spec.ts`:

| Test | Assertion |
|------|-----------|
| `fetchDetail` success | populates `currentTaxon` |
| `fetchDetail` failure | returns failed `Result`, `currentTaxon` unchanged |
| `fetchRules` success | populates `rules` |

Use `createTestingPinia` (already available per `unit-test-vue-pinia` conventions); mock `TaxonApi` / `TaxonomyApi`.

Existing specs unchanged: `services/taxonApi.spec.ts`, `services/taxonomyApi.spec.ts`, `services/taxonRuleApi.spec.ts` all verify URLs that the stores now route through — no service modifications.

## Constraints

- Must pass `pnpm run lint`, `pnpm run test:unit`, and `pnpm run build` with zero errors.
- No new npm dependencies.
- `TaxonomyDetail.vue` and `TaxonRuleFormDialog.vue` are out of scope (already use services correctly).
- `taxonApi.ts` / `taxonomyApi.ts` / `taxonRuleApi.ts` are unchanged.
- `taxonomyStore.ts` changes only the `fetchActive` query to add `pageSize: 100` — no other behavior change.
- The tree response wrapper stays `Result<{ tree: TaxonTreeItem[] }>` — extract via `result.value.tree`.

## Out of Scope

- Taxon restore / reposition UI.
- Reusing `taxonTreeStore` in `TaxonomyDetail` (kept on direct service calls).
- Adding column-filter wiring to the TaxonsList DataTable (search + taxonomy scope only, same as today).
