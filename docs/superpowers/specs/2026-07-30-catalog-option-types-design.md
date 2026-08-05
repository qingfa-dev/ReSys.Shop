# Catalog Option Types Admin UI

**Date**: 2026-07-30
**Scope**: Admin SPA — Catalog module, OptionType + OptionValue management
**Decision**: Full Location-module replication (types, services, stores, validations, views), embedded OptionValue table in detail view with tab layout, dialog slide-out for OptionValue create/edit.

## Motivation

- Catalog backend has full CRUD endpoints for OptionType (5) and OptionValue (5), but the Admin SPA has only stub views.
- The Location module establishes a proven pattern (types -> services -> stores -> validations -> views) that keeps each file small, focused, and independently testable.
- Naming and structure consistency across modules reduces cognitive load for future development.
- The `Filterable` flag on OptionType drives the storefront filter panel — admins need a UI to manage it.

## Architecture

### File Structure

All new files under `app/Admin/src/features/catalog/option-types/`. No modifications to shared components or composables.

```
option-types/
├── types/
│   ├── optionType.ts          # Request, ListItem, Detail, Query, const arrays, converter
│   ├── optionValue.ts         # Request, ListItem, Detail, Query, const arrays, converter
│   └── index.ts               # barrel
├── services/
│   ├── optionTypeApi.ts       # OptionType CRUD + paged
│   ├── optionValueApi.ts      # OptionValue CRUD + paged
│   └── index.ts               # barrel
├── stores/
│   ├── optionTypeStore.ts     # Pinia: cached active option types for dropdowns
│   └── index.ts               # barrel
├── validations/
│   ├── optionType.ts          # Zod: name, presentation, position, filterable
│   ├── optionValue.ts         # Zod: name, presentation, position
│   └── index.ts               # barrel
├── components/
│   ├── OptionValueFormDialog.vue   # dialog for create/edit OptionValue
│   └── index.ts
├── composables/
│   └── index.ts               # empty barrel (reuses shared composables)
├── views/
│   ├── OptionTypesList.vue    # standalone DataTable page
│   ├── OptionTypeDetail.vue   # form + tabbed OptionValues DataTable + hosts dialog
│   └── index.ts               # barrel (replace stubs)
└── __tests__/
    ├── types/
    │   ├── optionType.spec.ts
    │   └── optionValue.spec.ts
    ├── services/
    │   ├── optionTypeApi.spec.ts
    │   └── optionValueApi.spec.ts
    └── validations/
        ├── optionType.spec.ts
        └── optionValue.spec.ts
```

### Existing Files to Replace

| File | Current | New |
|------|---------|-----|
| `views/OptionTypesList.vue` | Stub (`<PageShell>` wrapper) | Full DataTable page |
| `views/OptionTypeDetail.vue` | Stub (`<PageShell>` wrapper) | Form + tabbed OptionValues |

Routes in `features/catalog/routes/index.ts` and menu items in `AppMenu.vue` are already wired and need no changes.

## Types Layer

### OptionType

```typescript
interface OptionTypeRequest {
  name: string
  presentation: string
  position: number
  filterable: boolean
}

interface OptionTypeListItem extends OptionTypeRequest {
  id: string
  optionValuesCount: number
  productsCount: number
}

interface OptionTypeDetail extends OptionTypeListItem {
  createdAtUtc: string
  modifiedAtUtc: string
  createdBy: string
  modifiedBy: string
}

interface OptionTypeQuery {
  name?: string
  filterable?: boolean
  search?: string
  sortBy?: 'name' | 'presentation' | 'position' | 'optionValuesCount' | 'productsCount' | 'createdAtUtc' | 'modifiedAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

const OPTION_TYPE_FILTER_FIELDS = ['name', 'filterable', 'optionValuesCount', 'productsCount']
const OPTION_TYPE_SORT_FIELDS = ['name', 'presentation', 'position', 'optionValuesCount', 'productsCount', 'createdAtUtc', 'modifiedAtUtc']
const OPTION_TYPE_SEARCH_FIELDS = ['name', 'presentation']

function toOptionTypeQueryParams(query: OptionTypeQuery): QueryingParameters
```

**Filter DSL (converter):**
- `name*=value` → substring match on name
- `filterable=true|false` → exact boolean match
- No filter on counts (informational columns only)
- Sort: prefix `-` for desc, no prefix for asc.
- Search: uses `OPTION_TYPE_SEARCH_FIELDS` whitelist (name, presentation).

### OptionValue

```typescript
interface OptionValueRequest {
  optionTypeId: string
  name: string
  presentation: string
  position: number
}

interface OptionValueListItem extends OptionValueRequest {
  id: string
}

interface OptionValueDetail extends OptionValueListItem {
  createdAtUtc: string
  modifiedAtUtc: string
}

interface OptionValueQuery {
  optionTypeId?: string       // filter by parent
  name?: string
  search?: string
  sortBy?: 'name' | 'presentation' | 'position' | 'createdAtUtc' | 'modifiedAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

const OPTION_VALUE_FILTER_FIELDS = ['optionTypeId', 'name']
const OPTION_VALUE_SORT_FIELDS = ['name', 'presentation', 'position', 'createdAtUtc', 'modifiedAtUtc']
const OPTION_VALUE_SEARCH_FIELDS = ['name', 'presentation']

function toOptionValueQueryParams(query: OptionValueQuery): QueryingParameters
```

**Filter DSL:**
- `optionTypeId=:guid` → exact match (primary filter from parent)
- `name*=value` → substring match

## Services Layer

Static API classes following `CountryApi`/`StateApi` pattern.

### OptionTypeApi

Base: `${CATALOG}/option-types`

| Method | HTTP | URL | Response |
|--------|------|-----|----------|
| `getOptionTypes` | GET | `/api/catalog/option-types` | `PagedResult<OptionTypeListItem>` |
| `getOptionType` | GET | `/api/catalog/option-types/{id}` | `Result<OptionTypeDetail>` |
| `createOptionType` | POST | `/api/catalog/option-types` | `Result<OptionTypeDetail>` |
| `updateOptionType` | PUT | `/api/catalog/option-types/{id}` | `Result<OptionTypeDetail>` |
| `deleteOptionType` | DELETE | `/api/catalog/option-types/{id}` | `Result<OptionTypeListItem>` |

Paged endpoint passes `OPTION_TYPE_FILTER_FIELDS` and `OPTION_TYPE_SORT_FIELDS` to `getPaged()` for server-side whitelist enforcement.

### OptionValueApi

Base: `${CATALOG}/option-types/option-values`

| Method | HTTP | URL | Response |
|--------|------|-----|----------|
| `getOptionValues` | GET | `/api/catalog/option-types/option-values` | `PagedResult<OptionValueListItem>` |
| `getOptionValue` | GET | `/api/catalog/option-types/option-values/{id}` | `Result<OptionValueDetail>` |
| `createOptionValue` | POST | `/api/catalog/option-types/option-values` | `Result<OptionValueDetail>` |
| `updateOptionValue` | PUT | `/api/catalog/option-types/option-values/{id}` | `Result<OptionValueDetail>` |
| `deleteOptionValue` | DELETE | `/api/catalog/option-types/option-values/{id}` | `Result<OptionValueListItem>` |

## Store Layer

### optionTypeStore

Pinia composition store for dropdown reuse (e.g., Product forms needing OptionType picker).

```
useOptionTypeStore()
  activeOptionTypes: Ref<OptionTypeListItem[]>
  loaded: Ref<boolean>
  fetchActive(): Promise<void>  — fetch once (pageSize=100, sortBy=name)
```

Lazy-once pattern identical to `useCountryStore`.

## Validations Layer

### optionType (Zod)

| Field | Rule | Error |
|-------|------|-------|
| `name` | min 1, max 100 | required / too long |
| `presentation` | min 1, max 100 | required / too long |
| `position` | int, min -1, default 1 | invalid |
| `filterable` | boolean, default false | — |

Combined `optionTypeSchema` → `z.infer` → `OptionTypeForm` type.

### optionValue (Zod)

| Field | Rule | Error |
|-------|------|-------|
| `optionTypeId` | string, required | required |
| `name` | min 1, max 100 | required / too long |
| `presentation` | min 1, max 100 | required / too long |
| `position` | int, min -1, default 1 | invalid |

## Views

### OptionTypesList.vue

Same pattern as `CountriesList.vue`:
- Composable stack: `useRouter`, `useConfirm`, `useNotify`, `useDataTableExport`, `usePagedQuery`
- `selectedItems` ref, `searchTerm` ref
- Card > Toolbar (New + Delete + Export) > DataTable with checkbox column + header search
- Columns: name (sortable, filterable), presentation, position, filterable (Tag with success/danger severity), optionValuesCount, productsCount, actions (edit + delete)
- Search whitelist: `OPTION_TYPE_SEARCH_FIELDS`
- Delete: `selectedItems` loop → sequential single deletes → partial-failure notice
- CSV export via `useDataTableExport`

### OptionTypeDetail.vue

Form page with two tabs:
- Composable stack: `useRoute`, `useRouter`, `useNotify`, `useApiErrorHandler`, `useConfirm`, `usePagedQuery` (for embedded OptionValues table)
- `isEdit` computed from `route.params.id`
- `OptionTypeForm` ref, `fieldErrors` ref
- On mount (edit): fetch OptionType → populate form
- On save: Zod parse → create or update → notify → redirect

Tab layout:
```
PageShell
  PageHeading: "Option Types" > "{Name or New}"
    actions: [Save] [Cancel]
  Tabs (v-model:active = tab)
    Tab "General":
      FormSection
        FormField name, presentation, position, filterable
    Tab "Option Values" (hidden on create until OptionType saved):
      Toolbar: [+ Add Value]
      DataTable:
        data-key="id"
        v-model:selection (not used for toolbar delete, but Column left for alignment)
        header: search input
        columns: name, presentation, position, actions (edit/delete)
        empty: "No option values defined."
      OptionValueFormDialog (v-if visible)
```

### OptionValueFormDialog.vue

Standalone component, accepts props and emits:
- Props: `visible: boolean`, `editingValue: OptionValueListItem | null` (null = create mode)
- Emits: `update:visible`, `saved`
- Uses `Dialog` wrapper, Zod-validated form for name/presentation/position
- On save: calls `OptionValueApi.createOptionValue` or `OptionValueApi.updateOptionValue`
- On success: emits `saved`, closes
- On failure: maps Zod errors or API errors to field messages

### Data Flow — OptionValues in Detail View

```
OptionTypeDetail                     OptionValueFormDialog
─────────────────                    ──────────────────────
onMounted (edit mode)
  → fetch OptionType (populate form)
  → fetch OptionValues (populate table)

[Edit] click on row
  → editingRow = value
  → dialogVisible = true
  → dialog opens pre-populated

[Add] click on toolbar
  → editingRow = null
  → dialogVisible = true
  → dialog opens blank

Dialog @saved
  → dialogVisible = false
  → refresh OptionValues table

[Delete] click on row
  → confirm.require(...)
  → accept: deleteValue(id)
  → refresh table
```

### Edge Cases

| Scenario | Behavior |
|----------|----------|
| Delete OptionType with values | Backend returns `OptionType.CannotDeleteWithValues`. Display error toast with the message. |
| Delete last OptionValue | Table shows "No option values defined." empty state. |
| Create new OptionType | "Option Values" tab hidden (no id yet). User saves, is redirected to edit page with the new id, tab becomes visible. |
| Duplicate OptionType name | Backend returns `OptionType.DuplicateName`. Show error toast. |
| Duplicate OptionValue name | Backend returns `OptionValue.NameAlreadyExists`. Show error toast from dialog. |
| Concurrent edit | Backend returns error. Dialog stays open, error message displayed. |

## Testing Strategy

### Unit Tests (Vitest)

| File | What to test |
|------|-------------|
| `types/optionType.spec.ts` | `toOptionTypeQueryParams` produces correct DSL; `OPTION_TYPE_FILTER_FIELDS`/`SORT_FIELDS`/`SEARCH_FIELDS` contain expected values |
| `types/optionValue.spec.ts` | `toOptionValueQueryParams` produces correct DSL |
| `services/optionTypeApi.spec.ts` | All 5 methods call correct HTTP methods and URLs |
| `services/optionValueApi.spec.ts` | All 5 methods call correct HTTP methods and URLs |
| `validations/optionType.spec.ts` | Zod schema: required fields, max lengths, position min, valid/invalid data |
| `validations/optionValue.spec.ts` | Zod schema: required fields, max lengths, position min |

### Manual Verification

- OptionTypes list: search, sort, filter, pagination, CSV export
- OptionType create: navigate via New button, fill form, save, verify redirect
- OptionType edit: navigate from list, modify fields, save, verify
- OptionType delete: single and multi-select from list, confirm dialog, verify removed
- OptionType delete with values: verify error toast, entity remains
- OptionValues: add via dialog, edit via dialog, delete with confirmation, table refresh
- Tab navigation: General ↔ Option Values, form state preserved across tab switches
- Breadcrumbs and back navigation correct

## Constraints

- Must pass `pnpm run lint` and `pnpm run build-only` with zero errors
- Must pass all existing 416 tests (no regressions)
- New OptionType/OptionValue routes require `catalog.index` permission (from `CatalogFeature.Admin.cs`)
- No new dependencies — reuse existing PrimeVue, Zod, Pinia, Vitest, @primeicons/vue

## Backend API Reference

| Method | URL | Permission |
|--------|-----|------------|
| GET | `/api/catalog/option-types` | `catalog.index` |
| POST | `/api/catalog/option-types` | `catalog.index` |
| GET | `/api/catalog/option-types/{id}` | `catalog.index` |
| PUT | `/api/catalog/option-types/{id}` | `catalog.index` |
| DELETE | `/api/catalog/option-types/{id}` | `catalog.index` |
| GET | `/api/catalog/option-types/option-values` | `catalog.index` |
| POST | `/api/catalog/option-types/option-values` | `catalog.index` |
| GET | `/api/catalog/option-types/option-values/{id}` | `catalog.index` |
| PUT | `/api/catalog/option-types/option-values/{id}` | `catalog.index` |
| DELETE | `/api/catalog/option-types/option-values/{id}` | `catalog.index` |
