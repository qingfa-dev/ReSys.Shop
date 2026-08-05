# Catalog Taxonomies, Taxons & Taxon Rules — Admin UI Design

**Date**: 2026-07-30
**Scope**: Admin SPA — Catalog module, Taxonomy/Taxon/TaxonRule management
**Decision**: Full Location-module replication (types, services, stores, validations, views). Three-layer hierarchy: Taxonomies list, standalone Taxons list with dual DataTable/TreeTable toggle, and Taxon detail with 5-tab form + embedded Rules table.

## Motivation

- Catalog backend has 19 admin endpoints for Taxonomies (6), Taxons (8), and TaxonRules (5), but the Admin SPA has only stub views.
- Taxons are hierarchical (nested-set model: Lft/Rgt/Depth) requiring a tree-aware UI. PrimeVue TreeTable supports expand/collapse natively.
- The Location module's layered pattern is proven — types → services → stores → validations → views.
- Taxon has 23 fields across 5 logical groups, warranting tabbed organization.

## Architecture

### File Structure

All new files under `app/Admin/src/features/catalog/`. No modifications to shared components or composables.

```
catalog/
├── types/
│   ├── taxonomy.ts
│   ├── taxon.ts
│   ├── taxonRule.ts
│   └── index.ts                         # replace empty barrel
├── services/
│   ├── taxonomyApi.ts
│   ├── taxonApi.ts
│   ├── taxonRuleApi.ts
│   └── index.ts                         # replace empty barrel
├── stores/
│   ├── taxonomyStore.ts                 # Pinia dropdown cache
│   └── index.ts                         # replace empty barrel
├── validations/
│   ├── taxonomy.ts
│   ├── taxon.ts
│   ├── taxonRule.ts
│   └── index.ts                         # replace empty barrel
├── components/
│   ├── TaxonRuleFormDialog.vue
│   └── index.ts                         # replace empty barrel
├── views/
│   ├── TaxonomiesList.vue               # replace stub
│   ├── TaxonomyDetail.vue               # replace stub (form + inline TreeTable)
│   ├── TaxonDetail.vue                  # new (5-tab form + Rules table)
│   └── index.ts                         # replace stub barrel
├── __tests__/
│   ├── types/      (3 spec files)
│   ├── services/   (3 spec files)
│   └── validations/ (3 spec files)
└── routes/
    └── index.ts                         # add 2 taxon routes
```

### Routes

Add before the existing `catalog/taxonomies/:id` route (higher specificity wins):

```ts
{
  path: 'catalog/taxons',
  name: 'catalog-taxons',
  component: TaxonsList,
  meta: { title: 'Taxons' },
},
{
  path: 'catalog/taxons/:id',
  name: 'catalog-taxon-detail',
  component: TaxonDetail,
  meta: { title: 'Taxon Detail' },
},
```

Existing taxonomy routes remain unchanged. Create flow uses `catalog/taxons/new?taxonomyId=xxx&parentId=yyy` — the `:id` param catches `"new"` and `isEdit` excludes it with `!== 'new'`.

Update `catalogMenuItems` to add a "Taxons" entry under the Catalog menu section.

---

## Types Layer

### Taxonomy (taxonomy.ts)

```typescript
interface TaxonomyRequest {
  name: string
  presentation: string
  position: number
}

interface TaxonomyListItem extends TaxonomyRequest {
  id: string
  taxonsCount: number
  createdAtUtc: string
  modifiedAtUtc: string | null
}

interface TaxonomyDetail extends TaxonomyListItem {
  createdBy: string | null
  modifiedBy: string | null
}

interface TaxonomyQuery {
  name?: string
  search?: string
  sortBy?: 'name' | 'presentation' | 'position' | 'taxonsCount' | 'createdAtUtc' | 'modifiedAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

const TAXONOMY_FILTER_FIELDS = ['name', 'taxonsCount', 'createdAtUtc', 'modifiedAtUtc']
const TAXONOMY_SORT_FIELDS = ['name', 'presentation', 'position', 'taxonsCount', 'createdAtUtc', 'modifiedAtUtc']

function toTaxonomyQueryParams(query: TaxonomyQuery): QueryingParameters
```

Filter DSL: `name*=value` (contains), no boolean filters. Same converter pattern as OptionType.

### Taxon (taxon.ts)

```typescript
interface TaxonRequest {
  taxonomyId: string
  parentId: string | null
  name: string
  presentation: string
  description: string | null
  slug: string
  position: number
  metaTitle: string | null
  metaDescription: string | null
  metaKeywords: string | null
  imageUrl: string | null
  squareImageUrl: string | null
  automatic: boolean
  rulesMatchPolicy: 'All' | 'Any'
  sortOrder: string          // one of TAXON_SORT_ORDERS
  hideFromNav: boolean
}

interface TaxonListItem extends TaxonRequest {
  id: string
  parentName: string | null
  taxonomyName: string | null
  lft: number
  rgt: number
  depth: number
  childrenCount: number
  taxonRuleCount: number
  productCount: number
  permalink: string
  prettyName: string
  createdAtUtc: string
  modifiedAtUtc: string | null
}

type TaxonDetail = TaxonListItem

interface TaxonQuery {
  taxonomyId?: string
  name?: string
  search?: string
  sortBy?: 'name' | 'slug' | 'position' | 'depth' | 'createdAtUtc' | 'modifiedAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

interface TaxonTreeItem extends TaxonListItem {
  children: TaxonTreeItem[]
}

const TAXON_FILTER_FIELDS = ['taxonomyId', 'name', 'slug', 'depth', 'createdAtUtc', 'modifiedAtUtc']
const TAXON_SORT_FIELDS = ['name', 'slug', 'position', 'depth', 'createdAtUtc', 'modifiedAtUtc']
const TAXON_SORT_ORDERS = ['Manual', 'BestSelling', 'AlphabeticallyAZ', 'AlphabeticallyZA', 'PriceHigh2Low', 'PriceLow2High', 'Newest', 'Oldest']
const TAXON_MATCH_POLICIES = ['All', 'Any']

function toTaxonQueryParams(query: TaxonQuery): QueryingParameters
```

Filter DSL: `taxonomyId=:guid` (exact), `name*=value` (contains), `depth=N` (exact).

### TaxonRule (taxonRule.ts)

```typescript
interface TaxonRuleRequest {
  type: string            // one of TAXON_RULE_TYPES
  matchPolicy: string     // one of TAXON_RULE_MATCH_POLICIES
  value: string
}

interface TaxonRuleListItem extends TaxonRuleRequest {
  id: string
  taxonId: string
}

type TaxonRuleDetail = TaxonRuleListItem  // same shape in backend

interface TaxonRuleQuery {
  taxonId?: string
}

const TAXON_RULE_TYPES = [
  'product_name', 'product_sku', 'product_description',
  'product_price', 'product_weight', 'product_available',
  'product_archived', 'variant_price', 'variant_sku', 'product_status',
]

const TAXON_RULE_MATCH_POLICIES = [
  'is_equal_to', 'is_not_equal_to', 'contains', 'does_not_contain',
  'starts_with', 'ends_with', 'greater_than', 'less_than',
  'greater_than_or_equal', 'less_than_or_equal', 'in', 'not_in',
  'is_null', 'is_not_null',
]

function toTaxonRuleQueryParams(query: TaxonRuleQuery): QueryingParameters
```

---

## Services Layer

### TaxonomyApi

Base: `${CATALOG}/taxonomies`

| Method | HTTP | URL | Response |
|--------|------|-----|----------|
| `getTaxonomies(query)` | GET | `/api/catalog/taxonomies` | `PagedResult<TaxonomyListItem>` |
| `getTaxonomy(id)` | GET | `/api/catalog/taxonomies/{id}` | `Result<TaxonomyDetail>` |
| `createTaxonomy(req)` | POST | `/api/catalog/taxonomies` | `Result<TaxonomyDetail>` |
| `updateTaxonomy(id, req)` | PUT | `/api/catalog/taxonomies/{id}` | `Result<TaxonomyDetail>` |
| `deleteTaxonomy(id)` | DELETE | `/api/catalog/taxonomies/{id}` | `Result<TaxonomyListItem>` |
| `restoreTaxonomy(id)` | PATCH | `/api/catalog/taxonomies/{id}/restore` | `Result<void>` |

### TaxonApi

Base: `${CATALOG}/taxonomies/taxons`

| Method | HTTP | URL | Response |
|--------|------|-----|----------|
| `getTaxons(query)` | GET | `/api/catalog/taxonomies/taxons` | `PagedResult<TaxonListItem>` |
| `getTaxon(id)` | GET | `/api/catalog/taxonomies/taxons/{id}` | `Result<TaxonDetail>` |
| `getTree()` | GET | `/api/catalog/taxonomies/taxons/tree` | `Result<TaxonTreeItem[]>` |
| `createTaxon(req)` | POST | `/api/catalog/taxonomies/taxons` | `Result<TaxonDetail>` |
| `updateTaxon(id, req)` | PUT | `/api/catalog/taxonomies/taxons/{id}` | `Result<TaxonDetail>` |
| `deleteTaxon(id)` | DELETE | `/api/catalog/taxonomies/taxons/{id}` | `Result<TaxonListItem>` |
| `restoreTaxon(id)` | PATCH | `/api/catalog/taxonomies/taxons/{id}/restore` | `Result<void>` |
| `repositionTaxon(id, req)` | POST | `/api/catalog/taxonomies/taxons/{id}/reposition` | `Result<{ id: string }>` |

Note: `getTree()` returns a nested array from `result.value.tree`, which is the `Tree` property of `TaxonTreeResponse`. The response wrapper is `Result<{ tree: TaxonTreeItem[] }>`.

### TaxonRuleApi

Base: `${CATALOG}/taxonomies/taxons`

| Method | HTTP | URL | Response |
|--------|------|-----|----------|
| `getRules(taxonId)` | GET | `/api/catalog/taxonomies/taxons/{id}/rules` | `PagedResult<TaxonRuleListItem>` |
| `createRule(taxonId, req)` | POST | `/api/catalog/taxonomies/taxons/{id}/rules` | `Result<TaxonRuleDetail>` |
| `updateRule(taxonId, ruleId, req)` | PUT | `/api/catalog/taxonomies/taxons/{id}/rules/{ruleId}` | `Result<TaxonRuleDetail>` |
| `deleteRule(taxonId, ruleId)` | DELETE | `/api/catalog/taxonomies/taxons/{id}/rules/{ruleId}` | `Result<TaxonRuleListItem>` |

Note: `getRules()` is a GET endpoint but uses paged result with all items in a single page (ordered by Type). The `PagedResult` wrapper is standard even if pagination isn't used.

---

## Store Layer

### taxonomyStore (Pinia)

```
useTaxonomyStore()
  activeTaxonomies: Ref<TaxonomyListItem[]>
  loaded: Ref<boolean>
  fetchActive(): Promise<void>  — calls getTaxonomies with no pageSize (returns all)
```

Used by TaxonDetail's taxonomy Select dropdown. Lazy-once pattern identical to `useCountryStore`.

---

## Validations Layer

### taxonomy.ts (Zod)

| Field | Rule |
|-------|------|
| `name` | min 1, max 100, required |
| `presentation` | min 1, max 100, required |
| `position` | int, min -1, default 1 |

Combined `taxonomySchema`, inferred `TaxonomyForm` type.

### taxon.ts (Zod)

| Field | Rule |
|-------|------|
| `taxonomyId` | string, required |
| `parentId` | string, nullable |
| `name` | min 1, max 255, required |
| `presentation` | min 1, max 255, required |
| `slug` | min 1, max 255, regex `^[a-z0-9]+(?:-[a-z0-9]+)*$` |
| `description` | max 2000, nullable |
| `position` | int, min -1, default 0 |
| `metaTitle` | max 100, nullable |
| `metaDescription` | max 255, nullable |
| `metaKeywords` | max 255, nullable |
| `imageUrl` | string, nullable |
| `squareImageUrl` | string, nullable |
| `automatic` | boolean, default false |
| `rulesMatchPolicy` | enum: All, Any |
| `sortOrder` | enum: Manual, BestSelling, etc. |
| `hideFromNav` | boolean, default false |

Combined `taxonSchema`, inferred `TaxonForm` type.

### taxonRule.ts (Zod)

| Field | Rule |
|-------|------|
| `type` | string, required, one of `TAXON_RULE_TYPES` |
| `matchPolicy` | string, required, one of `TAXON_RULE_MATCH_POLICIES` |
| `value` | string, required, max 255 |

Combined `taxonRuleSchema`, inferred `TaxonRuleForm` type.

---

## Views

### TaxonomiesList.vue

Same pattern as OptionTypesList:
- `usePagedQuery` + `useDataTableExport` + `useConfirm`
- Card + Toolbar (New, Delete, Export)
- DataTable: checkbox, header search, columns (name, presentation, position, taxonsCount, actions)
- Search whitelist: `['name', 'presentation']`
- Multi-select delete via sequential single deletes

### TaxonomyDetail.vue

Same pattern as OptionTypeDetail (single tab form):
- FormSection: name, presentation, position
- Below form: inline TreeTable of taxons using `taxonApi.getTree()`
  - PrimeVue TreeTable with expand/collapse
  - Columns: name, slug, position, rules count, products count, actions
  - Toolbar: [+ Add Taxon] → `/catalog/taxons/new?taxonomyId=<id>`
  - Edit → `/catalog/taxons/<id>`
  - Delete → confirm dialog → `taxonApi.deleteTaxon(id)` → refresh tree

### TaxonsList.vue

Dual-view page with toolbar toggle:
- Toolbar: [+ New Taxon] [Delete] [Export] [Toggle 🌲/📋]
- `viewMode` ref: `'table'` | `'tree'`

**DataTable mode (viewMode === 'table'):**
- `usePagedQuery` with `GET /api/catalog/taxonomies/taxons`
- Server paging, sorting, filtering via query params
- Columns: name, slug, taxonomyName, parentName, depth, position, taxonRuleCount, productCount, actions
- Search whitelist: `['name', 'slug']`
- Multi-select delete via sequential single deletes
- New → `/catalog/taxons/new` (taxonomyId picked in the form)
- Edit → `/catalog/taxons/<id>`

**TreeTable mode (viewMode === 'tree'):**
- Fetch via `taxonApi.getTree()` in `onMounted`
- `treeData` ref holds the response
- PrimeVue TreeTable with `:value="treeData"`, expand/collapse
- Columns: name, slug, position, taxonRuleCount, productCount, actions
- Client-side text filter (simple input filters tree items by name/slug)
- No pagination — all nodes in one response
- Delete → confirm dialog → `taxonApi.deleteTaxon(id)` → refetch tree

### TaxonDetail.vue

5-tab form page:
- `isEdit` computed from `route.params.id !== 'new'`
- On mount (edit): fetch taxon detail → populate all form fields
- On mount (create): read `taxonomyId` and `parentId` from `route.query`
- Taxonomy Select: populated from `useTaxonomyStore.fetchActive()`
- Parent Select: populated from `taxonApi.getTree()` with depth-indented labels (e.g., "  |-- Child Name")

**Tab "General":**
- FormField: name, presentation, slug, description (textarea)
- FormField: taxonomy (Select), parent (Select), position

**Tab "Settings":**
- FormField: sortOrder (Select from `TAXON_SORT_ORDERS`)
- FormField: hideFromNav (ToggleSwitch), automatic (ToggleSwitch)
- FormField: rulesMatchPolicy (Select from `TAXON_MATCH_POLICIES`)

**Tab "SEO":**
- FormField: metaTitle, metaDescription (textarea), metaKeywords

**Tab "Images":**
- FormField: imageUrl, squareImageUrl

**Tab "Rules"** (hidden when `!isEdit`):
- Toolbar: [+ Add Rule]
- DataTable: type, matchPolicy, value, actions (edit, delete)
- `usePagedQuery` filtered by `taxonId=<route.params.id>`
- TaxonRuleFormDialog: same pattern as OptionValueFormDialog
  - Props: `visible`, `taxonId`, `editingRule`
  - Emits: `update:visible`, `saved`
  - 3 fields: Type (Select), MatchPolicy (Select), Value (InputText)

**Save:** Zod parse → `createTaxon` or `updateTaxon` → notify → redirect to list or stay for edit

### TaxonRuleFormDialog.vue

Same pattern as OptionValueFormDialog:
- 3 Select/Input fields with Zod validation
- Create mode: `taxonId` from prop, blank fields
- Edit mode: pre-populated from `editingRule` prop
- On save: calls `createRule` or `updateRule` → emits `saved`
- On error: `handleResult` stays open with error toast

---

## Edge Cases

| Scenario | Behavior |
|----------|----------|
| Delete Taxon with children | Backend returns 409 Conflict. Display error toast. |
| Delete Taxonomy with taxons | 409 Conflict. Display error toast. |
| Taxon reposition | Not exposed in initial UI (backend endpoint exists, may be added later). |
| Taxon restore | Not exposed in initial UI (deleted items are invisible in tree). |
| TreeTable with 500+ nodes | Client-side rendering. Acceptable for initial version. |
| Create Taxon without taxonomy query param | Taxonomy Select is required, empty validation blocks save. |
| Duplicate slug | Backend error. Display toast. |

---

## Testing Strategy

### Unit Tests (Vitest)

| File | What to test |
|------|-------------|
| `types/taxonomy.spec.ts` | `toTaxonomyQueryParams` DSL, const arrays |
| `types/taxon.spec.ts` | `toTaxonQueryParams` DSL, const arrays |
| `types/taxonRule.spec.ts` | `toTaxonRuleQueryParams`, const arrays |
| `services/taxonomyApi.spec.ts` | 6 methods: HTTP verb + URL correctness |
| `services/taxonApi.spec.ts` | 8 methods: HTTP verb + URL correctness |
| `services/taxonRuleApi.spec.ts` | 4 methods: HTTP verb + URL correctness |
| `validations/taxonomy.spec.ts` | Zod: required fields, max lengths, position bounds |
| `validations/taxon.spec.ts` | Zod: required fields, max lengths, slug regex, enum values |
| `validations/taxonRule.spec.ts` | Zod: required fields, enum validation, max length |

All existing 472 tests must continue to pass.

## Constraints

- Must pass `pnpm run lint` and `pnpm run build-only` with zero errors
- No new npm dependencies
- Routes must be added BEFORE `catalog/taxonomies/:id` in routes array
- `catalog-taxon-detail` route uses `:id` — create via `/catalog/taxons/new?taxonomyId=xxx`
- `isEdit` excludes `route.params.id === 'new'` (same pattern as OptionTypeDetail fix)
- `getTree()` wraps the response: `Result<{ tree: TaxonTreeItem[] }>` — extract via `result.value.tree`

## Backend API Reference

| Method | URL | Permission |
|--------|-----|------------|
| GET | `/api/catalog/taxonomies` | `Taxonomies.List` |
| POST | `/api/catalog/taxonomies` | `Taxonomies.Create` |
| GET | `/api/catalog/taxonomies/{id}` | `Taxonomies.List` |
| PUT | `/api/catalog/taxonomies/{id}` | `Taxonomies.Update` |
| DELETE | `/api/catalog/taxonomies/{id}` | `Taxonomies.Delete` |
| PATCH | `/api/catalog/taxonomies/{id}/restore` | `Taxonomies.Restore` |
| GET | `/api/catalog/taxonomies/taxons` | `Taxons.List` |
| POST | `/api/catalog/taxonomies/taxons` | `Taxons.Create` |
| GET | `/api/catalog/taxonomies/taxons/{id}` | `Taxons.List` |
| GET | `/api/catalog/taxonomies/taxons/tree` | `Taxons.List` |
| PUT | `/api/catalog/taxonomies/taxons/{id}` | `Taxons.Update` |
| DELETE | `/api/catalog/taxonomies/taxons/{id}` | `Taxons.Delete` |
| PATCH | `/api/catalog/taxonomies/taxons/{id}/restore` | `Taxons.Restore` |
| POST | `/api/catalog/taxonomies/taxons/{id}/reposition` | `Taxons.Update` |
| GET | `/api/catalog/taxonomies/taxons/{id}/rules` | `Taxons.List` |
| POST | `/api/catalog/taxonomies/taxons/{id}/rules` | `Taxons.ManageRules` |
| PUT | `/api/catalog/taxonomies/taxons/{id}/rules/{ruleId}` | `Taxons.ManageRules` |
| DELETE | `/api/catalog/taxonomies/taxons/{id}/rules/{ruleId}` | `Taxons.ManageRules` |
