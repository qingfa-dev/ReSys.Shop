---
goal: Implement admin Location, Profile, Reports, and Dashboard modules
version: 1.0
date_created: 2026-07-25
status: Planned
tags: feature, api, location, profile, reports, dashboard, admin
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Implement 4 smaller admin modules: Location (Countries + States CRUD, 12 endpoints), Profile (Profiles + Addresses CRUD, 9 endpoints), Reports (frontend-only analytics page, 0 endpoints), and the main app Dashboard (1 endpoint). All pages are placeholder shells.

Backend route prefixes: `api/locations`, `api/profiles`, `api/dashboard`

## 1. Requirements & Constraints

- **REQ-001**: Every backend endpoint must have a frontend API method
- **REQ-002**: All API methods use shared `apiClient`
- **REQ-003**: Response types as camelCase interfaces matching backend C# records
- **REQ-004**: Zod validation for entities with create/update forms
- **REQ-005**: Form-to-request mapper classes with static toCreate/toUpdate
- **REQ-006**: List pages get Pinia stores
- **REQ-007**: Replace PlaceholderPage with real components
- **REQ-008**: Main app Dashboard (feature directory `dashboard/` under pages, not under `features/`) loads from API
- **REQ-009**: Reports module is frontend-only — no backend API calls, uses hardcoded data or analytics store
- **CON-001**: Follow catalog module patterns exactly
- **CON-002**: Store IDs: `'location-country'`, `'location-state'`, `'profile-address'`
- **CON-003**: Zero TypeScript errors
- **PAT-001** to **PAT-009**: Same as catalog patterns

## 2. Implementation Steps

### Phase 1: Location — Countries CRUD

- GOAL-001: Implement Countries CRUD: types, schemas, mappers, API, store, composable, pages, components

Backend endpoints (prefix `api/locations`):
- GET `/countries` — GetPaged
- GET `/countries/{id:guid}` — GetById
- GET `/countries/by-iso/{isoCode}` — GetByIso
- POST `/countries` — Create
- PUT `/countries/{id:guid}` — Update
- DELETE `/countries/{id:guid}` — Delete

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `types/country.response.ts` — `CountryResponse`: id, name, isoCode, iso3Code?, numericCode?, phoneCode?, isActive, statesCount?, createdAt, updatedAt | | |
| TASK-002 | Create `types/country.request.ts` — alias from form schema | | |
| TASK-003 | Create `schemas/country.fields.ts` — fields: name (required), isoCode (required, 2 chars), iso3Code (optional, 3 chars), numericCode (optional), phoneCode (optional), isActive (boolean) | | |
| TASK-004 | Create `schemas/country.forms.ts` — `CountryForms` with create()/update() | | |
| TASK-005 | Create `mappers/country.mapper.ts` — `CountryFormMapper` | | |
| TASK-006 | Create `api/country.api.ts` — `CountryApi`: getMany(query), get(id), getByIso(isoCode), create(data), update(id, data), delete(id) | | |
| TASK-007 | Create `store/country.store.ts` — `useCountryStore` | | |
| TASK-008 | Create `composables/useCountry.ts` | | |
| TASK-009 | Create `components/CountryForm.vue` — fields: name, isoCode, iso3Code, numericCode, phoneCode, isActive | | |
| TASK-010 | Create `components/CountryListTable.vue` — columns: name, isoCode, iso3Code, phoneCode, isActive (icon), statesCount, ActionMenu | | |
| TASK-011 | Replace `pages/CountryListPage.vue` — PageHeader + CountryListTable | | |
| TASK-012 | Replace `pages/CountryDetailPage.vue` — CountryForm | | |
| TASK-013 | Update routes, barrels | | |
| TASK-014 | Verify: type-check passes | | |

### Phase 2: Location — States/Provinces CRUD

- GOAL-002: Implement States CRUD: types, schemas, mappers, API, store, composable, pages, components

Backend endpoints (prefix `api/locations`):
- GET `/states` — GetPaged
- GET `/states/{id:guid}` — GetById
- GET `/states/by-iso/{isoCode}` — GetByIso
- POST `/states` — Create
- PUT `/states/{id:guid}` — Update
- DELETE `/states/{id:guid}` — Delete

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | Create `types/state.response.ts` — `StateResponse`: id, name, isoCode, countryId, countryName?, isActive, createdAt, updatedAt | | |
| TASK-016 | Create `types/state.request.ts` — alias from form schema | | |
| TASK-017 | Create `schemas/state.fields.ts` — fields: name (required), isoCode (required), countryId (required), isActive (boolean) | | |
| TASK-018 | Create `schemas/state.forms.ts` — `StateForms` with create()/update() | | |
| TASK-019 | Create `mappers/state.mapper.ts` | | |
| TASK-020 | Create `api/state.api.ts` — `StateApi`: getMany(query), get(id), getByIso(isoCode), create(data), update(id, data), delete(id) | | |
| TASK-021 | Create `store/state.store.ts` — `useStateStore` | | |
| TASK-022 | Create `composables/useState.ts` | | |
| TASK-023 | Create `components/StateForm.vue` — fields: name, isoCode, countryId (select from CountryApi), isActive | | |
| TASK-024 | Create `components/StateListTable.vue` — columns: name, isoCode, countryName, isActive (icon), ActionMenu | | |
| TASK-025 | Replace `pages/StateListPage.vue` and `StateDetailPage.vue` | | |
| TASK-026 | Update routes, barrels | | |
| TASK-027 | Verify: type-check passes | | |

### Phase 3: Profile — User Profile + Addresses

- GOAL-003: Implement Profile read/update + Addresses CRUD

Backend endpoints (prefix `api/profiles`):
- GET `/profiles` (resolves to `/profiles/all`) — GetAll
- POST `/profiles` — Create
- PUT `/profiles` — Update
- DELETE `/profiles` — Delete
- GET `/addresses` — GetAll
- GET `/addresses/{id:guid}` — GetById
- POST `/addresses` — Create
- PUT `/addresses/{id:guid}` — Update
- DELETE `/addresses/{id:guid}` — Delete

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-028 | Create `types/profile.response.ts` — `ProfileResponse`: id, userId, firstName, lastName, email, phone?, avatarUrl?, dateOfBirth?, createdAt, updatedAt | | |
| TASK-029 | Create `types/profile.request.ts` — `UpdateProfileRequest`: firstName, lastName, phone?, avatarUrl?, dateOfBirth? | | |
| TASK-030 | Create `schemas/profile.fields.ts` — fields: firstName (required), lastName (required), phone (optional), avatarUrl (optional), dateOfBirth (optional) | | |
| TASK-031 | Create `schemas/profile.forms.ts` — `ProfileForms` with update() schema | | |
| TASK-032 | Create `mappers/profile.mapper.ts` | | |
| TASK-033 | Create `api/profile.api.ts` — `ProfileApi`: get() GET `/profiles`, create(data) POST `/profiles`, update(data) PUT `/profiles`, delete() DELETE `/profiles` | | |
| TASK-034 | Create `composables/useProfile.ts` | | |
| TASK-035 | Create `components/ProfileForm.vue` — fields: firstName, lastName, phone, avatarUrl, dateOfBirth; load->get; save->update | | |
| TASK-036 | Replace `pages/ProfilePage.vue` — PageHeader + ProfileForm | | |
| TASK-037 | Create `types/address.response.ts` — `AddressResponse`: id, firstName, lastName, address1, address2?, city, state?, postalCode, country, phone?, isDefault, createdAt, updatedAt | | |
| TASK-038 | Create `types/address.request.ts` — alias from form | | |
| TASK-039 | Create `schemas/address.fields.ts` — fields: firstName (required), lastName (required), address1 (required), address2 (optional), city (required), state (optional), postalCode (required), country (required), phone (optional), isDefault (boolean) | | |
| TASK-040 | Create `schemas/address.forms.ts` — `AddressForms` with create()/update() | | |
| TASK-041 | Create `mappers/address.mapper.ts` | | |
| TASK-042 | Create `api/address.api.ts` — `AddressApi`: getMany(), get(id), create(data), update(id, data), delete(id) | | |
| TASK-043 | Create `store/address.store.ts` — `useAddressStore` | | |
| TASK-044 | Create `composables/useAddress.ts` | | |
| TASK-045 | Create `components/AddressForm.vue` — fields: firstName, lastName, address1, address2, city, state, postalCode, country, phone, isDefault | | |
| TASK-046 | Create `components/AddressListTable.vue` — columns: firstName+lastName, address1, city, country, isDefault (icon), ActionMenu | | |
| TASK-047 | Replace `pages/AddressListPage.vue` — add AddressDetailPage if missing (reuse AddressForm) | | |
| TASK-048 | Update routes, barrels | | |
| TASK-049 | Verify: type-check passes | | |

### Phase 4: Reports (frontend-only dashboard)

- GOAL-004: Replace placeholder Reports dashboard with real analytics page (frontend-only, no backend)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-050 | Replace `pages/DashboardPage.vue` — build interactive analytics dashboard with sample charts/tables using PrimeVue Chart components; show product sales trends, order volume, revenue stats using mock/static data since no backend endpoint exists | | |
| TASK-051 | Update routes, verify type-check passes | | |

### Phase 5: Main App Dashboard

- GOAL-005: Implement main app Dashboard API integration (separate from per-module dashboards)

Backend endpoint: GET `/api/dashboard` returns `AppDashboardResponse`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-052 | Locate the main app dashboard page (likely at `app/Admin/src/pages/` or similar top-level location) — read its current implementation | | |
| TASK-053 | Create types for app dashboard response: totalUsers, totalProducts, totalOrders, totalRevenue, recentOrders, etc. | | |
| TASK-054 | Create API class for main dashboard | | |
| TASK-055 | Replace static/hardcoded data with live API call, add loading/error states | | |
| TASK-056 | Verify: type-check passes | | |

## 3. Alternatives

- **ALT-001**: Separate each module into its own plan — rejected: each is small enough to group
- **ALT-002**: Reports module might eventually need a backend — for now, frontend-only is appropriate per current backend structure

## 4. Dependencies

- **DEP-001**: Shared apiClient, Result/PagedResult/ListQuery from `@/shared/`
- **DEP-002**: CountryApi needed by StateForm for country selector dropdown
- **DEP-003**: Profile module may share auth user data from auth store

## 5. Files

- **FILE-001** to **FILE-056**: One per task

## 6. Testing

- **TEST-001**: `api/__tests__/countries.spec.ts` — verify all 6 methods
- **TEST-002**: `api/__tests__/states.spec.ts` — verify all 6 methods
- **TEST-003**: `api/__tests__/profiles.spec.ts` — verify all 4 methods
- **TEST-004**: `api/__tests__/addresses.spec.ts` — verify all 5 methods

## 7. Risks & Assumptions

- **RISK-001**: Backend profile routes use `api/profiles/profiles` (double "/profiles") — verify exact URL resolution
- **RISK-002**: Main app dashboard file location may differ from feature module pages — locate before implementing
- **ASSUMPTION-001**: Reports module has no backend — confirmed by absence of Reports feature directory in backend

## 8. Related Specifications / Further Reading

Backend Location: `service/Api/src/Module/Location/Features/Admin/`
Backend Profile: `service/Api/src/Module/Profile/Features/Admin/`
Backend Dashboard: `service/Api/src/Module/Dashboard/Features/Admin/`
Route constants: `service/Api/src/Module/Location/Features/Shared/LocationFeature.Admin.cs`
Route constants: `service/Api/src/Module/Profile/Features/Shared/ProfileFeature.cs`
Route constants: `service/Api/src/Module/Dashboard/Features/Shared/DashboardFeature.cs`
