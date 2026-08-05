# Location Admin UI — Design Spec

## Objective

Build the Vue 3 Admin SPA management interface for the Location module's Country and State entities. The backend API is fully implemented (18 endpoints, 104 files). The Admin SPA route scaffold exists (4 routes, menu items, placeholder views). This spec replaces the 4 placeholder views with real CRUD UIs and adds the supporting types, services, validations, and stores.

## File Structure

```
app/Admin/src/features/location/
├── types/
│   ├── country.ts           # Country interfaces + fluent querying model
│   └── state.ts             # State interfaces + fluent querying model
├── services/
│   ├── countryApi.ts        # CountryApi static class (CRUD + paged list)
│   └── stateApi.ts          # StateApi static class (CRUD + paged list)
├── validations/
│   ├── country.ts           # Zod per-field schemas + composed form schema
│   └── state.ts             # Zod per-field schemas + composed form schema
├── stores/
│   ├── countryStore.ts      # Pinia store: active countries cache for dropdowns
│   └── stateStore.ts        # (empty, reserved for future use)
├── views/
│   ├── CountriesList.vue    # DataTable: list, filter, sort, search, delete
│   ├── CountryDetail.vue    # Form: create/edit country
│   ├── StatesList.vue       # DataTable: list, filter (by country), sort, search, delete
│   └── StateDetail.vue      # Form: create/edit state with country dropdown
├── routes/index.ts          # Already exists — unchanged
└── index.ts                 # Update barrel to export new modules
```

## Entity Types and Fluent Querying Models

### `types/country.ts`

```typescript
export interface CountryRequest {
  name: string
  isoCode: string
  callingCode: string | null
  statesRequired: boolean
  isActive: boolean
}

export interface CountryListItem extends CountryRequest {
  id: string
}

export interface CountryDetail extends CountryListItem {
  createdAtUtc: string
  modifiedAtUtc: string | null
  createdBy: string | null
  modifiedBy: string | null
}

export interface CountryQuery {
  name?: string
  isoCode?: string
  callingCode?: string
  isActive?: boolean
  statesRequired?: boolean
  search?: string
  sortBy?: 'name' | 'isoCode' | 'callingCode' | 'createdAtUtc' | 'modifiedAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const COUNTRY_FILTER_FIELDS: string[]
export const COUNTRY_SORT_FIELDS: string[]
export function toCountryQueryParams(query: CountryQuery): QueryingParameters
```

### `types/state.ts`

```typescript
export interface StateRequest {
  name: string
  abbreviation: string
  countryId: string
  isActive: boolean
}

export interface StateListItem extends StateRequest {
  id: string
  countryName: string | null
}

export interface StateDetail extends Omit<StateListItem, 'countryName'> {
  createdAtUtc: string
  modifiedAtUtc: string | null
  createdBy: string | null
  modifiedBy: string | null
}

export interface StateQuery {
  name?: string
  abbreviation?: string
  countryId?: string
  isActive?: boolean
  search?: string
  sortBy?: 'name' | 'abbreviation' | 'countryId' | 'createdAtUtc' | 'modifiedAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const STATE_FILTER_FIELDS: string[]
export const STATE_SORT_FIELDS: string[]
export function toStateQueryParams(query: StateQuery): QueryingParameters
```

## API Services

### `services/countryApi.ts`

`export class CountryApi` with static methods:

| Method | HTTP | Returns |
|--------|------|---------|
| `getCountries(query)` | GET `/api/locations/countries` | `PagedResult<CountryListItem>` |
| `getCountry(id)` | GET `/api/locations/countries/{id}` | `Result<CountryDetail>` |
| `createCountry(req)` | POST `/api/locations/countries` | `Result<CountryDetail>` |
| `updateCountry(id, req)` | PUT `/api/locations/countries/{id}` | `Result<CountryDetail>` |
| `deleteCountry(id)` | DELETE `/api/locations/countries/{id}` | `Result<CountryListItem>` |

### `services/stateApi.ts`

`export class StateApi` with static methods:

| Method | HTTP | Returns |
|--------|------|---------|
| `getStates(query)` | GET `/api/locations/states` | `PagedResult<StateListItem>` |
| `getState(id)` | GET `/api/locations/states/{id}` | `Result<StateDetail>` |
| `createState(req)` | POST `/api/locations/states` | `Result<StateDetail>` |
| `updateState(id, req)` | PUT `/api/locations/states/{id}` | `Result<StateDetail>` |
| `deleteState(id)` | DELETE `/api/locations/states/{id}` | `Result<StateListItem>` |

Both use existing `get`, `getPaged`, `post`, `put`, `del` from `@/shared/api/client`.

## Validation Schemas (Zod)

### `validations/country.ts`

Per-field Zod schemas with custom validation messages:
- `countryName` — `z.string().min(1, 'Country name is required.').max(100, 'Country name must not exceed 100 characters.')`
- `countryIsoCode` — `z.string().min(1, 'ISO code is required.').max(3, 'ISO code must not exceed 3 characters.').regex(/^[A-Z]{2,3}$/, 'ISO code must be 2-3 uppercase letters.')`
- `countryCallingCode` — `z.string().max(10, 'Calling code must not exceed 10 characters.').optional()`
- `countryStatesRequired` — `z.boolean()`
- `countryIsActive` — `z.boolean()`

Composed into `countrySchema = z.object({...})`. Exported type: `CountryForm = z.infer<typeof countrySchema>`.

### `validations/state.ts`

Per-field Zod schemas with custom validation messages:
- `stateName` — `z.string().min(1, 'State name is required.').max(100, 'State name must not exceed 100 characters.')`
- `stateAbbreviation` — `z.string().min(1, 'Abbreviation is required.').max(10, 'Abbreviation must not exceed 10 characters.')`
- `stateCountryId` — `z.string().min(1, 'Country is required.')`
- `stateIsActive` — `z.boolean()`

Composed into `stateSchema = z.object({...})`. Exported type: `StateForm = z.infer<typeof stateSchema>`.

## Pinia Stores

### `stores/countryStore.ts`

`useCountryStore` — lightweight cache for active countries used in dropdowns:
- `activeCountries: Ref<CountryListItem[]>` — cached active countries
- `loaded: Ref<boolean>` — prevents re-fetch
- `fetchActive()` — fetches `isActive=true, pageSize=100` if `!loaded`

### `stores/stateStore.ts`

Empty Pinia store file (barrel export reserved). States don't need a store — list uses `usePagedQuery` directly.

## View Components

### `CountriesList.vue`

- `PageShell` wrapper with title "Countries" and description "Manage supported countries"
- `CrudToolbar` with: New button, Delete button (disabled when no selection), Search input tied to `setSearch`
- `FilterableDataTable` columns: Name, ISO Code, Calling Code, States Required (Yes/No badge), Is Active (`StatusTag`), Actions column (edit + delete icon buttons)
- `usePagedQuery<CountryListItem>()` with `COUNTRY_FILTER_FIELDS`/`COUNTRY_SORT_FIELDS`, default sort `['name:asc']`, `pageSize: 20`
- New button → router push `location/countries/new`
- Row click (or edit icon) → router push `location/countries/{id}`
- Delete icon → PrimeVue ConfirmDialog → `CountryApi.deleteCountry(id)` → success toast → `fetch()`
- States Required column: shows "Yes" / "No" badge

### `CountryDetail.vue`

- Route param `:id` present → edit mode (fetch existing data); absent → create mode
- `PageHeading`: breadcrumbs "Home > Countries > {country name or 'New'}", action buttons: Save, Cancel
- `FormSection` card with fields:
  - Name: `InputText`
  - ISO Code: `InputText` with uppercase input transform, `maxlength=3`
  - Calling Code: `InputText` with `+` prefix hint
  - States Required: `ToggleSwitch`
  - Active: `ToggleSwitch`
- On mount: if `id` param exists → `CountryApi.getCountry(id)` → populate reactive `form` ref
- On Save: `countrySchema.safeParse(form.value)` → if valid → `createCountry` or `updateCountry` → success toast → `router.push('/location/countries')`
- On validation fail: display per-field errors under each `FormField`
- API error handling: `useApiErrorHandler.handleResult(result)` for inline error display
- Cancel: router back

### `StatesList.vue`

- `PageShell` with title "States" and description "Manage states/provinces for countries"
- Country filter bar above table: PrimeVue `Select` dropdown, fetches from `useCountryStore().fetchActive()`, first option "All Countries" (clears filter)
- `CrudToolbar` with New, Delete, Search
- `FilterableDataTable` columns: Name, Abbreviation, Country Name (`countryName` field), Is Active (`StatusTag`), Actions
- `usePagedQuery<StateListItem>()` with `STATE_FILTER_FIELDS`/`STATE_SORT_FIELDS`, default sort `['name:asc']`
- Country filter change → `setFilter('countryId={guid}')` or call `setFilter('')` for all
- New → `location/states/new`; edit → `location/states/{id}`
- Delete confirm → `StateApi.deleteState(id)` → refresh

### `StateDetail.vue`

- Same create/edit pattern as CountryDetail (route param presence determines mode)
- `PageHeading`: "Home > States > {state name or 'New'}"
- `FormSection` fields:
  - Name: `InputText`
  - Abbreviation: `InputText`, `maxlength=10`
  - Country: PrimeVue `Select` dropdown, populated from `useCountryStore().fetchActive()`, option label = country name, value = country id
  - Active: `ToggleSwitch`
- On Save: `stateSchema.safeParse(form.value)` → `createState` / `updateState`
- Cancel: router back to states list

## Data Flow

```
List page:
  onMount → usePagedQuery.fetch()
  filter/sort/search via toolbar/filter bar → setFilter/setSort/setSearch → auto-refetch
  delete → confirm dialog → CountryApi.deleteCountry(id) → toast → fetch()

Detail page (create):
  empty form ref → user fills → Zod safeParse → CountryApi.createCountry(req) → toast → redirect

Detail page (edit):
  onMount → CountryApi.getCountry(id) → populate form ref
  user edits → Zod safeParse → CountryApi.updateCountry(id, req) → toast → redirect
```

## Error Handling

- API errors: `useApiErrorHandler.handleResult(result)` displays field-level validation errors inline + toast for global errors
- Zod validation: `safeParse().error?.issues` mapped to per-field error strings, displayed via `FormField` error prop/slot
- Network/500 errors: toast via `useNotify.error()`
- Delete with existing states: backend returns `Cannot delete country with existing states` error → toast notification

## Testing Approach

| Test | Type | What |
|------|------|------|
| Country/State Zod schemas | Unit | `safeParse()` on valid and invalid inputs |
| `toCountryQueryParams` | Unit | Typed model → correct DSL filter string |
| `toStateQueryParams` | Unit | Typed model → correct DSL filter string with `countryId` |
| CountriesList render | Component | Table renders mock items, delete triggers confirmation |
| CountryDetail form | Component | Required fields show errors on empty submit |
| StateDetail form | Component | Country dropdown populated from store |

## Decisions Summary

| Decision | Choice |
|----------|--------|
| Country form fields | 5 fields only (Name, IsoCode, CallingCode, StatesRequired, IsActive) |
| Backend changes | None — use existing API as-is |
| States list mode | Standalone with optional country filter dropdown |
| Country selector | Inline `Select` in State form (no reusable component) |
| File organization | Split by entity: `country.ts` / `state.ts` in each directory |
| API service pattern | `export class CountryApi` with static methods |
| Validation pattern | Per-field Zod schemas with messages, composed into object schema |
| Store strategy | One store: `countryStore` for active-countries dropdown cache |
| Delete behavior | Hard delete with confirmation dialog |
