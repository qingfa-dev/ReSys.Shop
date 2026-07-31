# Catalog Taxon Store + View Endpoint Corrections — Admin UI Design

**Date**: 2026-07-31
**Scope**: Admin SPA — Catalog module, Taxon management (TaxonsList, TaxonDetail, taxonStore)
**Decision**: Introduce a dedicated `taxonStore` that owns all taxon data (list, tree, detail, rules) and correct the taxon views to use proper endpoints through the store, eliminating a broken empty-URL request and a hardcoded-URL bypass of the service layer.

## Motivation

- `TaxonDetail.vue` uses `usePagedQuery<TaxonRuleListItem>('', ...)` — the empty URL fires a request to `GET /` on mount. Rules are also redundantly fetched by `loadRules()`, so the composable is both broken and duplicated.
- `TaxonsList.vue` passes a hardcoded URL string `'api/catalog/taxonomies/taxons'` to `usePagedQuery`, bypassing the `TaxonApi` service layer that every other catalog page uses.
- `TaxonsList.vue` makes no taxonomy API call at all — the taxonomy selector is missing, so the page can only be scoped via `?taxonomyId=` route query.
- There is no dedicated taxon store. `taxonomyStore`, `optionTypeStore`, and `productStore` exist; taxons are the only entity without one.
- Backend has a purpose-built taxonomy-scoped listing endpoint (`GET /taxons/list?taxonomyId=X`) that the store can use when a taxonomy is selected.

## Architecture

### New file: `stores/taxonStore.ts`

Pinia store (composition style, same as `taxonomyStore`) owning all taxon data. Mirrors the `usePagedQuery` surface area so the list view loses no behavior.

```
useTaxonStore()
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

  // tree state (shared by list view + detail parent dropdown)
  tree: Ref<TaxonTreeItem[]>
  treeLoading: Ref<boolean>
  treeTaxonomyId: Ref<string | null>
  fetchTree(taxonomyId): Promise<void>

  // detail state (TaxonDetail)
  currentTaxon: Ref<TaxonDetail | null>
  detailLoading: Ref<boolean>
  fetchDetail(id): Promise<void>

  // rules state (TaxonDetail rules tab)
  rules: Ref<TaxonRuleListItem[]>
  rulesLoading: Ref<boolean>
  fetchRules(taxonId): Promise<void>
```

**Endpoint selection in `fetchList`** — the "correct endpoint" rule:

- `selectedTaxonomyId` set → `TaxonApi.getList(selectedTaxonomyId, query)` → `GET /api/catalog/taxonomies/taxons/list?taxonomyId=X`
- `selectedTaxonomyId` null → `TaxonApi.getTaxons(query)` → `GET /api/catalog/taxonomies/taxons`

**Query building**: The store builds a `TaxonQuery` from its refs (`filter`, `sort`, `search`, `searchFields`, `searchMode`, `page`, `pageSize`) and passes it through the service, so `toTaxonQueryParams` / filter-field whitelisting stays in one place. The taxonomy scope is expressed via `selectedTaxonomyId` (→ `getList`), not via a `taxonomyId=...` filter string.

**Tree caching**: `fetchTree(taxonomyId)` refetches only when `treeTaxonomyId !== taxonomyId`; otherwise returns the cached `tree`. Set `treeTaxonomyId` on success.

**fetchActive**: `TaxonApi.getTaxons({ pageSize: 100, sortBy: 'name', sortDirection: 'asc' })` with lazy-once `loaded` guard — identical to `taxonomyStore`.

### Companion fix: `taxonomyStore.ts`

The backend clamps paged results to a default page size of **10** (max 100, see `Page.Bounds.cs`). `taxonomyStore.fetchActive()` currently sends no `pageSize`, so `activeTaxonomies` holds only the first 10 taxonomies. Add `pageSize: 100` to the `getTaxonomies` call so the new TaxonsList selector (and TaxonDetail's existing dropdown) is not truncated.

**fetchDetail / fetchRules**: store `Result` failures by returning them to the caller (`handleResult` in the view shows the toast), matching today's behavior.

### `stores/index.ts`

Add `export { useTaxonStore } from './taxonStore'`.

## View Changes

### `TaxonsList.vue`

- **Remove** `usePagedQuery` import and usage; **remove** the hardcoded URL.
- Bind table to store: `items`, `loading`, `totalCount`, `page`, `pageSize`; search box → `store.setSearch`; sortable columns → `store.setSort`; paging → `store.setPage` / `store.setPageSize`; Reload → `store.refresh`.
- **Add taxonomy selector** in the toolbar: a `Select` bound to `store.selectedTaxonomyId`, options from `useTaxonomyStore.activeTaxonomies` (`fetchActive()` on mount). On change → `store.setSelectedTaxonomy(id)` + `router.replace({ query: { ...route.query, taxonomyId: id || undefined } })`.
- Prefill `selectedTaxonomyId` from `?taxonomyId=` route query in `onMounted` before the first list fetch.
- Tree view uses `store.fetchTree(store.selectedTaxonomyId)` when toggled; `addTreeNodeKeys` mapping stays local.
- Delete flow unchanged at the service level (`TaxonApi.deleteTaxon`), then `store.refresh()` + reload tree if in tree mode.
- "New Taxon" button carries the current `selectedTaxonomyId` in the query.

### `TaxonDetail.vue`

- **Remove** the broken `usePagedQuery<TaxonRuleListItem>('', ...)` block (lines 88-96) and its `refreshRules` reference in `onRuleSaved`.
- Edit mode `onMounted`: `store.fetchDetail(id)` → populate form from `store.currentTaxon`; then `store.fetchTree(taxonomyId)` (parent dropdown) + `store.fetchRules(id)`.
- `loadParents` becomes a local flattening of `store.tree` into `parentOptions` (same depth-indented labels).
- Rules table binds `store.rules` / `store.rulesLoading`; `onRuleSaved` calls `store.fetchRules(route.params.id)`.
- `taxonomyStore.fetchActive()` stays for the taxonomy Select dropdown.
- Create/save/delete still call `TaxonApi` directly from the view (mutations stay in views, matching `TaxonomyDetail`).

## Data Flow

1. `TaxonsList` mount → `taxonomyStore.fetchActive()` (selector options) + prefill `selectedTaxonomyId` from route → `store.fetchList()`.
2. Selector change → `setSelectedTaxonomy` → `fetchList()` → `router.replace` syncs URL.
3. Tree toggle → `fetchTree(selectedTaxonomyId)` (cached per taxonomy).
4. `TaxonDetail` mount (edit) → `taxonomyStore.fetchActive()` → `fetchDetail(id)` → `fetchTree(taxonomyId)` → `fetchRules(id)`.
5. Rule add/edit/delete → `TaxonRuleApi` mutation → `onRuleSaved` → `store.fetchRules(id)`.

## Error Handling

- `fetchList` failures populate `store.error` (mirrors `usePagedQuery`); the empty-state template renders as today.
- `fetchDetail` / `fetchRules` / `fetchTree` failures return the failed `Result` to the calling view so `useApiErrorHandler.handleResult` displays the toast — no silent swallowing.

## Testing

New `__tests__/stores/taxonStore.spec.ts` (first store spec in the codebase):

| Test | Assertion |
|------|-----------|
| `fetchList` with `selectedTaxonomyId` | calls `TaxonApi.getList(taxId, query)` → `/taxons/list?taxonomyId=X` |
| `fetchList` without `selectedTaxonomyId` | calls `TaxonApi.getTaxons(query)` → `/taxons` |
| `setPage` / `setSort` / `setSearch` | updates state and refetches list |
| `setSelectedTaxonomy` | clears/sets scope and refetches |
| `fetchTree` caching | refetches only when `treeTaxonomyId` differs |
| `fetchActive` | lazy-once guard, populates `activeTaxons` |

Use `createTestingPinia` (already available per `unit-test-vue-pinia` conventions); mock `TaxonApi` / `TaxonomyApi`.

Existing specs unchanged: `services/taxonApi.spec.ts`, `services/taxonomyApi.spec.ts`, `services/taxonRuleApi.spec.ts` all verify URLs that the store now routes through — no service modifications.

## Constraints

- Must pass `pnpm run lint`, `pnpm run test:unit`, and `pnpm run build` with zero errors.
- No new npm dependencies.
- `TaxonomyDetail.vue` and `TaxonRuleFormDialog.vue` are out of scope (already use services correctly).
- `taxonApi.ts` / `taxonomyApi.ts` / `taxonRuleApi.ts` are unchanged.
- `taxonomyStore.ts` changes only the `fetchActive` query to add `pageSize: 100` — no other behavior change.
- The tree response wrapper stays `Result<{ tree: TaxonTreeItem[] }>` — extract via `result.value.tree`.

## Out of Scope

- Taxon restore / reposition UI.
- Reusing `taxonStore` in `TaxonomyDetail` (kept on direct service calls).
- Adding column-filter wiring to the TaxonsList DataTable (search + taxonomy scope only, same as today).
