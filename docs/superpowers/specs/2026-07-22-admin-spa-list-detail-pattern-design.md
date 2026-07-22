# Admin SPA — Consistent List + Detail Page Pattern

**Date:** 2026-07-22  
**Scope:** All 9 admin modules (~28 pages)  
**Status:** Design approved

## Goal

Replace all 32 admin SPA stub pages with real list + detail pages following a
single consistent pattern across every module. Drop specialized page variants
(like `TreeManagerPage`) so every entity follows the same `ListPage` +
`DetailPage` convention.

## Principles

1. **One page pattern for all entities** — no special tree managers, no
   one-off layouts. List + Detail only.
2. **Sub-entities inline on parent detail** — Variants live on
   ProductDetailPage, Taxons live on TaxonomyDetailPage. No separate routes
   for sub-entities.
3. **Single detail page, toggle view/edit** — same `.vue` file handles
   create, view, and edit modes via route-driven mode detection.
4. **Convention over abstraction** — no generic composable layer. Pages use
   shared components directly. Consistency comes from the template, not a
   framework.
5. **No new dependencies** — PrimeVue v5, Tailwind v4, the existing shared
   component suite. Nothing else.

---

## Page Model

### Entity → Page mapping

For each top-level entity: 1 `ListPage` + 1 `DetailPage` (handles create/view/edit).

Sub-entities rendered as sections within the parent `DetailPage`. Read-only logs
(StockMovement, Payment history) get `ListPage` only.

### Catalog

| Entity | Pages | Inline sub-entities |
|--------|-------|-------------------|
| Product | `ProductListPage`, `ProductDetailPage` | Variants, Prices, Images, OptionTypes, Classifications |
| Taxonomy | `TaxonomyListPage`, `TaxonomyDetailPage` | Taxons (flat table, depth indentation) |
| OptionType | `OptionTypeListPage`, `OptionTypeDetailPage` | OptionValues |
| Dashboard | `DashboardPage` | — |

**Dropped:** `TaxonListPage`, `TreeManagerPage`, `OptionValueListPage`,
`ProductCreatePage` — all merged into parent detail pages.

### Inventory

| Entity | Pages | Note |
|--------|-------|------|
| StockLocation | `ListPage` + `DetailPage` | |
| StockItem | `ListPage` | View-only |
| StockTransfer | `ListPage` + `DetailPage` | TransferItems inline |
| StockMovement | `ListPage` | Read-only log |
| Dashboard | `DashboardPage` | |

**Dropped:** `StockImportPage` (becomes action on list), `UnitListPage` (no
backend entity).

### Ordering, Payment, Shipping, Location, Users, Profile, Reports

Every top-level entity gets `ListPage` + `DetailPage`. Read-only entities
(Payment history, Fulfillment queue) get `ListPage` only. Dashboards get
`DashboardPage`.

Total: ~30 page components (some routes consolidated but new detail pages added).

---

## Routing

### Pattern (per module, per entity)

```
/module/entity              → XxxListPage
/module/entity/new          → XxxDetailPage (mode: create)
/module/entity/:id          → XxxDetailPage (mode: view)
/module/entity/:id/edit     → XxxDetailPage (mode: edit)
```

4 routes, 2 components (`ListPage` + `DetailPage`).

### Mode detection (in DetailPage)

```ts
const route = useRoute()
const id = computed(() => route.params.id as string | undefined)
const mode = computed(() =>
  !id.value ? 'create'
    : route.name?.toString().endsWith('.edit') ? 'edit'
    : 'view'
)
```

No route guards. No meta fields needed.

### Menu

Sidebar menu stays. Only change: Catalog > Categories drops "Manager" entry
(`TreeManagerPage` gone). "All Taxonomies" links to `taxonomies.list`.
Taxon management = click taxonomy row → detail page → taxons sub-table.

---

## Component Tree

### ListPage

```
PageHeader (breadcrumb, title, subtitle, actions slot)
TableToolbar (search input, filter buttons, "+ Create" button)
DataTable (lazy load, sort, pagination, striped rows)
  ├── EmptyState (no data)
  ├── LoadingSkeleton (fetching)
  ├── ErrorState (fetch failed, retry button)
  ├── row-click → navigate to :id (view mode)
  └── ActionMenu per row (edit, delete)
BulkActionBar (floating, visible when rows selected)
```

### DetailPage

```
PageHeader (breadcrumb, title, subtitle)
Card — entity fields
  └── FormField (label, required indicator, input slot, hint, error)
      repeated per field
├── Fieldset "Sub-entity A" (if applicable)
│   ├── TableToolbar (search, "+ Add")
│   ├── DataTable (sub-entity rows)
│   └── EmptyState / LoadingSkeleton / ErrorState
├── Fieldset "Sub-entity B" ...
└── FormActions (sticky bottom: Save + Cancel)
```

---

## Sub-Entity Pattern

### Layout

Each sub-entity is a `<Fieldset>` with a `<legend>` inside the parent
`DetailPage`. Self-contained: its own `TableToolbar`, `DataTable`, and
state slots.

### Add/Edit flow

Clicking "+ Add Variant" or row "Edit" opens an inline **slideover** (PrimeVue
`Drawer`) with the sub-entity form. On save, the sub-table refreshes without
leaving the parent page. No separate route.

### Nested sub-entities (Variant → Prices, Images)

Variant row expands inline OR opens a slideover with tabs (Prices / Images).
No 3-level `Fieldset` nesting on the same page.

### Taxon depth indentation

In TaxonomyDetailPage's Taxons sub-table: rows are a flat array sorted by
`lft` ASC. Visual indent = `depth × 1.5rem` with a tree-line `::before`
pseudo on the first column. No `TreeTable` component used.

| depth | display       |
|-------|---------------|
| 0     | Clothing      |
| 1     | ├─ Men's      |
| 2     | ├── Tops      |
| 2     | ├── Bottoms    |
| 1     | ├─ Women's    |

---

## API & Error Handling

### HTTP wrapper

```ts
// shared/api/http.ts
async function api<T>(url, init?): Promise<T> // fetch + JSON + error normalize
```

Throws `ApiError { status, message }` on non-2xx. Pages set reactive `error`
state → `ErrorState` component renders with retry.

### Form validation

Client-side only. Inline per-page function, no validation library:

```ts
const errors = reactive<Record<string, string>>({})
function validate(data): boolean {
  errors.name = !data.name ? 'Required' : ''
  return !Object.values(errors).some(Boolean)
}
```

`FormField` component already accepts an `error` prop — wired directly.

### Notifications & confirmations

Use existing `useToastNotify` for success/error toasts after every save/delete.
Use existing `useConfirm` for destructive actions.

---

## Testing

- **Shared component tests** (existing): `DataTable`, `FormField`,
  `TableToolbar`, `PageHeader` — test slots, props, events
- **Page smoke tests**: mount without crashing, render `PageHeader` with
  correct title, handle loading / empty / error states
- **No integration tests**: API calls mocked via Vitest, no browser needed

---

## Files Affected

### New shared infra

```
src/shared/api/http.ts                  # fetch wrapper
src/shared/api/                          # domain API modules (catalog.ts, inventory.ts, ...)
```

### Catalog pages

```
src/features/catalog/pages/ProductListPage.vue
src/features/catalog/pages/ProductDetailPage.vue
src/features/catalog/pages/TaxonomyListPage.vue
src/features/catalog/pages/TaxonomyDetailPage.vue
src/features/catalog/pages/OptionTypeListPage.vue
src/features/catalog/pages/OptionTypeDetailPage.vue
src/features/catalog/pages/DashboardPage.vue
```

### Catalog pages removed

```
src/features/catalog/pages/TaxonListPage.vue
src/features/catalog/pages/TaxonTreeManagerPage.vue
src/features/catalog/pages/OptionValueListPage.vue
src/features/catalog/pages/ProductCreatePage.vue
```

### Routes updated

```
src/app/routes/catalog.routes.ts        # new route structure
src/app/routes/inventory.routes.ts
src/app/routes/ordering.routes.ts
src/app/routes/payment.routes.ts
src/app/routes/shipping.routes.ts
src/app/routes/location.routes.ts
src/app/routes/users.routes.ts
src/app/routes/profile.routes.ts
src/app/routes/reports.routes.ts
```

### Menu updated

```
src/app/config/admin-menu.config.ts     # drop TreeManager menu entry
```

### Pages in remaining modules

Same pattern as Catalog, one ListPage + one DetailPage per top-level entity.
