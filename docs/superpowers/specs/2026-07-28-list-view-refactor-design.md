# List View Refactor — Inline PrimeVue DataTable & Toolbar

**Date**: 2026-07-28  
**Scope**: Admin SPA — CountriesList.vue, StatesList.vue  
**Decision**: Remove CrudToolbar.vue and FilterableDataTable.vue wrapper components, inline PrimeVue DataTable + Toolbar + Card directly, extract a small shared composable.

## Motivation

- `CrudToolbar` and `FilterableDataTable` are thin wrappers that constrain PrimeVue's API surface without adding meaningful abstraction.
- CrudToolbar's search input and FilterableDataTable's global search input duplicate search functionality (client-side + server-side) — confusing with server-side pagination where client-side filter only sees the current page.
- The `ColumnDef[]` array approach prevents use of PrimeVue's native column template syntax.
- Only 2 views use them — the abstraction isn't paying for itself.

## Architecture

### Deletions

| File | Reason |
|------|--------|
| `shared/components/data/CrudToolbar.vue` | Inline Card + Toolbar + search directly in views |
| `shared/components/data/FilterableDataTable.vue` | Inline DataTable + Column directly in views |
| Barrel re-exports from `shared/components/data/index.ts` | Remove both entries |

### Additions

| File | Purpose |
|------|---------|
| `shared/composables/useDataTableExport.ts` | Shared `dt` ref for CSV export |

### Composable design

```typescript
// shared/composables/useDataTableExport.ts
export function useDataTableExport() {
  const dt = ref()           // DataTable ref — required for exportCSV()

  function exportCSV() {
    dt.value?.exportCSV()
  }

  return { dt, exportCSV }
}
```

Search is server-side via `usePagedQuery.setSearch()`. Each view manages its own `searchTerm` ref locally. Filter reset is a local inline function. The composable provides only the CSV export plumbing that needs the `dt` template ref.

## CountriesList Layout (after)

```
PageShell(title="Countries", description="Manage supported countries")
├── Card
│   └── Toolbar
│       ├── #start: [Button: New Country] [Button: Delete (disabled if none selected)]
│       └── #end:   [Button: Export CSV]
└── DataTable(:value="items" :loading="loading" paginator :rows="pageSize"
              filterDisplay="menu" dataKey="id"
              paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
              :rowsPerPageOptions="[5, 10, 25]"
              currentPageReportTemplate="Showing {first} to {last} of {totalRecords}")
    ├── #header
    │   ├── (left)  [InputText v-model:searchTerm placeholder="Search countries..." @input → setSearch()]
    │   └── (right) [Button: Clear filter]
    ├── Column(field="name"           header="Name"           sortable filter)
    ├── Column(field="isoCode"        header="ISO Code"       sortable filter)
    ├── Column(field="callingCode"    header="Calling Code"   sortable)
    ├── Column(field="statesRequired" header="States Required" sortable bodyStyle="text-align: center")
    │   └── #body: Tag(value=Yes/No :severity="data ? 'info' : 'secondary'")
    ├── Column(field="isActive"       header="Active"         sortable filter bodyStyle="text-align: center")
    │   └── #body: Tag(value=Active/Inactive :severity="data ? 'success' : 'danger'")
    └── Column(header="" bodyStyle="text-align: right; width: 6rem")
        └── #body: [Button: edit] [Button: delete row]
```

## StatesList Layout (after)

```
PageShell(title="States", description="Manage states and provinces for countries")
├── Card
│   └── Toolbar
│       ├── #start: [Button: New State] [Button: Delete (disabled if none selected)]
│       └── #end:   [Button: Export CSV]
└── DataTable(:value="items" :loading="loading" paginator :rows="pageSize"
              filterDisplay="menu" dataKey="id"
              ...)
    ├── #header
    │   ├── (left)  [InputText v-model:searchTerm placeholder="Search states..." @input → setSearch()]
    │   └── (right) [Select v-model:selectedCountryId :options="activeCountries" showClear
    │                 @change → setFilter("countryId=X")] [Button: Clear filter]
    ├── Column(field="name"         header="Name"         sortable filter)
    ├── Column(field="abbreviation" header="Abbreviation" sortable filter)
    ├── Column(field="countryName"  header="Country"      sortable filter)
    ├── Column(field="isActive"     header="Active"       sortable filter bodyStyle="text-align: center")
    │   └── #body: Tag(value=Active/Inactive :severity="data ? 'success' : 'danger'")
    └── Column(header="" bodyStyle="text-align: right; width: 6rem")
        └── #body: [Button: edit] [Button: delete row]
```

## Data Flow

1. `usePagedQuery` composable drives server-side pagination, sort, search, and filter.
2. Search input in DataTable header calls `setSearch(value)` on debounced input — replaces CrudToolbar's `@update:search`.
3. Country dropdown in StatesList calls `setFilter("countryId=X")` — replaces CrudToolbar's `#header-left` slot.
4. Export CSV calls `dt.value?.exportCSV()` — same as before but from the composable.
5. Action buttons (new, delete, edit) wired directly in the view template — no event emit chain through a wrapper.

## Error Handling

No change to error handling. `usePagedQuery`, `useNotify`, and `useConfirm` patterns remain the same. API calls already return `Result<T>` with `isSuccess`/`errors`.

## Constraints

- Must follow existing Vue 3 Composition API patterns (`<script setup lang="ts">`)
- Must use PrimeVue v5 components with proper named-slot syntax (Card needs `#content`)
- Must preserve all existing lint, type-check, and test rules
- `usePagedQuery` composable interface must remain unchanged
- `ColumnDef` interface in both list views must be removed (no longer needed with explicit Column tags)

## Files Changed

| File | Change |
|------|--------|
| `shared/composables/useDataTableExport.ts` | New composable |
| `shared/composables/index.ts` | Add `useDataTableExport` re-export |
| `features/location/views/CountriesList.vue` | Inline DataTable + Column + Toolbar |
| `features/location/views/StatesList.vue` | Inline DataTable + Column + Toolbar |
| `shared/components/data/CrudToolbar.vue` | Delete |
| `shared/components/data/FilterableDataTable.vue` | Delete |
| `shared/components/data/index.ts` | Remove 2 exports |

## Verification

```bash
cd app/Admin
pnpm run build-only          # must pass
pnpm run lint                 # must pass, zero errors
pnpm run test:unit -- run    # 413/413 tests must pass
```
