# Admin SPA — Saikai/PrimeVue Migration Design

**Date:** 2026-07-06
**Status:** Approved
**Supersedes:** `2026-07-06-admin-vertical-slice-design.md`
**Scope:** `app/Admin/` (Vue 3 + TypeScript + Vite + PrimeVue v4 + Tailwind v4)

## Executive Summary

Migrate the Admin SPA from its current Vuetify 3 (MD3) foundation to the PrimeVue 4 (Aura) / Saikai template pattern established in `app/ReSys.Admin/`. This is a full-stack swap: UI framework, layout system, data-fetching pattern (TanStack Query composables → Pinia stores), and form handling (custom → VeeValidate + Zod). Existing features are ported in-place; missing features (inventory, ordering, fulfillment, reports) are added from the ReSys.Admin reference.

The migration runs in 3 phases: **(1)** foundation swap (PrimeVue + layout + shared layer), **(2)** port existing features, **(3)** add new features. Each phase is independently shippable.

---

## 1. Architecture & Directory Structure

Keep the current `app/ + features/ + shared/` layout. Only `app/` internals change (layout extracted from `app/layout/` to top-level `app/layout/`, plugins consolidated).

```
src/
├── app/                              # Bootstrap + cross-cutting
│   ├── main.ts                       # Entry point
│   ├── App.vue                       # Root: RouterView + Toast + ConfirmDialog
│   ├── plugins/
│   │   └── primevue.ts              # PrimeVue + Aura + Tailwind
│   ├── router/
│   │   └── index.ts                 # Root router + beforeEach auth guard
│   ├── stores/                      # Client-only Pinia
│   │   ├── theme.store.ts
│   │   ├── sidebar.store.ts
│   │   └── tenant.store.ts
│   ├── layout/                      # App shell (from ReSys.Admin)
│   │   ├── main.layout.vue
│   │   ├── topbar.layout.vue
│   │   ├── sidebar.layout.vue
│   │   ├── menu.layout.vue
│   │   ├── menu-item.layout.vue
│   │   ├── footer.layout.vue
│   │   ├── configurator.layout.vue
│   │   ├── composables/
│   │   │   └── layout.composable.ts
│   │   └── components/
│   │       └── GlobalSearch.vue
│   └── auth/                        # Token hydration bootstrap
│       └── auth-bootstrap.ts
│
├── features/                         # One folder per backend module
│   ├── auth/                         # Login, Profile
│   ├── catalog/                      # Products, taxonomies, taxa, option-types, property-types
│   ├── dashboard/                    # Landing page
│   ├── identity/                     # Users, roles, permissions
│   ├── inventory/                    # Stocks, units, locations, transfers
│   ├── location/                     # Countries, states
│   ├── ordering/                     # Orders, fulfillment
│   ├── profile/                      # Profile, addresses, wishlists, notifications
│   └── reports/                      # Dashboard with charts
│
├── shared/                           # Cross-cutting primitives
│   ├── api/                          # Axios client, ApiResult types, utils
│   ├── components/                   # Breadcrumb, ManagerWelcome
│   ├── composables/                  # Toast, formatter, file-preview, api-error-handler
│   ├── locales/                      # General locale strings
│   └── utils/                        # Query builder
│
├── assets/
│   ├── tailwind.css                  # Tailwind v4 + tailwindcss-primeui plugin
│   └── scss/
│       ├── main.scss                 # Manifest (import order)
│       ├── abstracts/                # Variables (common, light, dark), mixins
│       ├── base/                     # Core reset, typography
│       ├── layout/                   # Topbar, menu, footer, main, responsive, utils, preloading
│       └── demo/                     # Demo/example styles
│
├── router/                           # (empty — routes live in feature modules)
├── layouts/                          # (empty — layouts in app/layout/)
└── __tests__/                        # Smoke tests
```

### Feature module pattern

```
features/{module}/
├── {module}.routes.ts
├── views/
├── components/
├── stores/
├── services/
├── types/
├── schemas/
├── locales/
└── tests/
```

---

## 2. UI Framework & Layout

### PrimeVue 4 + Aura Theme
- Preset: Aura (switchable via configurator to Lara/Nora)
- Dark mode: `.app-dark` class on `<html>`, View Transition API toggle
- Icons: PrimeIcons (replaces @mdi/font)
- Auto-import via unplugin-vue-components + PrimeVueResolver
- Plugins: Toast, ConfirmDialog, StyleClass

### Tailwind CSS v4
- tailwindcss-primeui plugin
- Custom breakpoints: sm:576, md:768, lg:992, xl:1200, 2xl:1920
- Font: Inter (rsms.me)
- Dark variant: `@custom-variant dark (&:where([class*="app-dark"], ...))`

### Layout Components (from ReSys.Admin)
- `main.layout.vue` — Shell: Topbar + Sidebar + RouterView + Footer + ConfirmDialog
- `topbar.layout.vue` — Logo, GlobalSearch, dark mode, configurator, user menu
- `sidebar.layout.vue` — Nav sidebar wrapper
- `menu.layout.vue` — Navigation tree from route config
- `menu-item.layout.vue` — Recursive menu item with active-route + submenu transitions
- `footer.layout.vue` — Copyright
- `configurator.layout.vue` — Theme preset/primary/surface/menu-mode picker
- `GlobalSearch` — Popup AutoComplete search

### Routing
- `/login` public → LoginView
- `/` auth-required → layout shell with feature children
- Auth guard: `router.beforeEach` → redirect to `/login?redirect=`
- All routes lazy-loaded

---

## 3. Data Layer & State Management

### API Client (`shared/api/api.client.ts`)
- Axios instance at `/api`
- Request interceptor: `Authorization: Bearer` from localStorage
- Response interceptor: unwraps `Envelope<T>` → `ApiResult<T>` discriminated union
- 401 → auto-refresh → redirect to `/login` on failure

### State Management
| Concern | Tool | Location |
|---------|------|----------|
| Server data | Pinia store (composition API) | `features/*/stores/*.store.ts` |
| Client UI state | Pinia | `app/stores/{theme,sidebar,tenant}.store.ts` |
| Form state | VeeValidate useForm + Zod | `features/*/{schemas, views}` |
| Auth tokens | Module-scoped ref + localStorage | `features/auth/stores/auth.store.ts` |

### Form Validation
- Zod schemas per feature
- `useForm({ validationSchema: toTypedSchema(MySchema) })` in views
- `useApiErrorHandler()` maps server validation errors back to form fields

### Toast Pattern
- `toastBus` ref in `shared/composables/toast.use.ts`
- `App.vue` watches and calls `toast.add()` on PrimeVue Toast

---

## 4. Feature Modules

### Auth
- Store (login/logout/refresh/hydrate), Service, Types, Schemas, Locales
- Views: login.view.vue, Profile.view.vue

### Catalog
- Routes: `/catalog/*` — dashboard, products, taxonomies, taxa, option-types, option-values, property-types
- 20+ views, stores, services, types, schemas

### Inventory (NEW)
- Routes: `/inventory/stocks`, `/inventory/units`, `/inventory/locations`, `/inventory/transfers`
- 8 views, 3 components

### Ordering (NEW)
- Routes: `/ordering/orders`, `/ordering/fulfillment`
- 4 views, 4 components, fulfillment sub-module

### Reports (NEW)
- Route: `/reports/dashboard`
- Dashboard with chart.js widgets

### Users / Roles / Permissions
- Routes: `/users/staff`, `/users/customers`, `/roles`, `/permissions`
- 8 views, 3 components

### Location
- Routes: `/location/countries`, `/location/states`
- CRUD with DataTable + Dialog

### Profile
- Route: `/profile`
- Profile page + address/wishlist/notification stubs

---

## 5. Dependencies

### Add
primevue ^4.5.4, @primeuix/themes ^1.0.0, primeicons ^7.0.0,
vee-validate ^4.15.1, @vee-validate/zod ^4.15.1, zod ^3.24.0,
tailwindcss-primeui ^0.6.1, chart.js ^4.5.1, jwt-decode ^4.0.0

### Remove
vuetify ^3.7.0, vite-plugin-vuetify ^2.0.0, @mdi/font ^7.4.47

---

## 6. Migration Phases

### Phase 1 — Foundation
1. Remove Vuetify + @mdi/font, add PrimeVue + deps
2. Create primevue.ts plugin with Aura preset
3. Copy layout/ from ReSys.Admin (8 files)
4. Replace shared/api/ with ReSys.Admin's API client
5. Replace shared/composables/ (toast, formatter, error-handler, file-preview)
6. Add shared/components/ (breadcrumb, ManagerWelcome), locales, utils
7. Configure tailwind.css + SCSS assets from ReSys.Admin
8. Rewrite main.ts + App.vue + router with auth guard
9. Port auth feature (login + profile)

### Phase 2 — Port Existing Features
1. Catalog (products, taxonomies, taxa, option-types, property-types)
2. Users / Roles / Permissions
3. Location (current admin → PrimeVue)
4. Profile (current admin → PrimeVue)

### Phase 3 — Add New Features
1. Inventory (stocks, units, locations, transfers)
2. Ordering + Fulfillment
3. Reports dashboard

### Cleanup
- Remove remaining Vuetify code
- Verify no Vuetify in bundle

---

## 7. Testing Strategy
- Vitest + jsdom + Vue Test Utils
- API calls mocked via `vi.mock('@/shared/api/api.client')`
- Coverage opt-in at 70% for shared/ modules
- Gate per phase: `pnpm test:unit && pnpm type-check && pnpm lint`

---

## 8. Acceptance Criteria
- `pnpm dev` boots with PrimeVue layout (no Vuetify)
- Login page renders with PrimeVue components
- Layout renders with topbar, sidebar, content, footer
- All existing features render with PrimeVue
- New features (inventory, ordering, reports) render
- `pnpm build` has no Vuetify code
- `pnpm lint && pnpm test:unit && pnpm type-check` all pass

---

## 9. Out of Scope
- Store SPA (`app/Store/`)
- E2E tests (Playwright)
- Storybook
- i18n beyond current scope
- Backend API changes
