# Admin SPA — Shared Infrastructure Migration from Legacy

**Date:** 2026-07-22  
**Scope:** `app/lagacy/Admin/` shared/common → `app/Admin/src/shared/`  
**Status:** Design approved

## Goal

Migrate all cross-cutting shared infrastructure from the legacy PrimeVue 4 Admin
SPA (`app/lagacy/Admin/src/common/` + `src/shared/`) into the new PrimeVue 5
Admin SPA (`app/Admin/src/shared/`), achieving full parity so feature migration
can proceed unblocked.

## Principles

1. **Full parity** — every shared utility, composable, service, type, and test
   from legacy comes over (no selective cherry-picking).
2. **Legacy wins on duplicates** — where a file exists in both apps, the legacy
   implementation replaces the new one. No dual implementations.
3. **Flat `shared/` only** — no `common/` split. All infrastructure lives under
   `src/shared/` with barrel exports at each directory level.
4. **Backend alignment** — frontend types mirror the backend's `Result<T>`,
   `Error`, `PagedResult<T>`, and Specification pattern naming exactly. Drop
   the legacy `ServerResult<T>` convention.
5. **Same libraries** — adopt axios, vue-i18n, vee-validate+zod,
   `unplugin-auto-import`, chart.js, `eslint-plugin-boundaries`, and
   `@vitest/coverage-v8` from the legacy app.
6. **Tests ported with code** — every `.spec.ts` from legacy gets ported and
   adjusted for PrimeVue 5 API changes where needed.
7. **Phased delivery** — three independently-verifiable PRs, each passing
   `pnpm run lint && pnpm run test:unit && pnpm run build`.

## Decisions

| Decision | Choice |
|---|---|
| Scope | Full parity — migrate everything |
| Libraries | Keep all: axios, vue-i18n, vee-validate+zod, unplugin-auto-import, chart.js, eslint-plugin-boundaries, @vitest/coverage-v8 |
| Structure | Legacy wins, everything merged into `shared/` only (no `common/` split) |
| Backend types | Mirror exactly: `Result<T>`, `Error`, `PagedResult<T>`, `QueryingParameters`, `FilterOperator` |
| Tests | Port all tests alongside code |
| Delivery | Three-phase layered migration (Approach B) |

---

## Phase 1: Foundation

### Directory structure

```
app/Admin/src/shared/
├── models/                          # Backend-aligned types
│   ├── result.ts                    # Result, Result<T>, PagedResult<T>, Error struct
│   ├── querying.ts                  # FilterModel, FilterCondition, FilterGroup, SearchModel,
│   │                                  SortModel, SortClause, PageModel, QueryingModel
│   ├── pagination.ts                # PaginationMeta (synced to backend PagedResult)
│   ├── api.ts                       # ApiResponse, ApiError, RequestOptions
│   └── index.ts
│
├── constants/
│   ├── api.ts                       # Module API paths (CATALOG, IDENTITY, INVENTORY, LOCATION,
│   │                                  ORDERING, PAYMENT, PROFILE, SHIPPING, USERS)
│   ├── routes.ts                    # Route path constants
│   ├── permissions.ts               # Permission string constants
│   ├── regex.ts                     # Shared regex patterns (email, phone, slug, etc.)
│   ├── storage.ts                   # localStorage key constants
│   └── index.ts
│
├── api/
│   ├── client.ts                    # Axios instance factory (baseURL, timeout, headers)
│   ├── endpoints.ts                 # Per-module endpoint builders
│   ├── interceptors/
│   │   ├── auth.interceptor.ts      # Attach JWT Bearer header from token.service
│   │   ├── camelcase.interceptor.ts # snake_case ↔ camelCase response/request transform
│   │   ├── error-wrapper.interceptor.ts  # AxiosError → Error struct normalization
│   │   └── index.ts
│   ├── handlers/
│   │   ├── error-handler.ts         # Global error dispatch (toast/redirect/retry by status)
│   │   ├── refresh-handler.ts       # Token refresh queue (single-flight pattern)
│   │   └── index.ts
│   ├── services/
│   │   └── module-api.factory.ts    # createModuleApi(basePath) → typed CRUD proxy
│   ├── utils/
│   │   ├── api.utils.ts             # URL builder, query string helpers
│   │   └── result.mapper.ts         # Axios response → Result<T> mapping
│   └── index.ts
│
├── auth/
│   ├── auth.service.ts              # login(), logout(), refreshToken()
│   ├── token.service.ts             # JWT storage, decode, expiry check
│   ├── permissions.ts               # hasPermission(), hasRole() guard functions
│   ├── roles.ts                     # Role definitions and hierarchy
│   ├── session.ts                   # Session state (current user, isAuthenticated)
│   └── index.ts
│
└── errors/
    ├── ApiError.ts                  # HTTP error with code + details + metadata
    ├── ValidationError.ts           # Field-level validation errors (Zod issues → readable)
    ├── UnauthorizedError.ts         # 401-specific with redirect hook
    └── index.ts
```

### New packages installed

```bash
pnpm add axios vue-i18n vee-validate zod @vee-validate/zod jwt-decode chart.js
pnpm add -D unplugin-auto-import eslint-plugin-boundaries @vitest/coverage-v8
```

`chart.js` is installed but integration code is deferred (no dashboard charts needed yet).
`eslint-plugin-boundaries` config is ported from legacy and updated for the flat `shared/` structure.

### Key design

- **Axios instance is a factory**, not a singleton. `createHttpClient(baseURL, options)` returns
  a per-module instance with its own interceptor chain — avoids shared mutable state.
- **Token refresh uses single-flight queue.** Multiple concurrent 401s trigger exactly one
  refresh call; the rest await and retry with the new token.
- **Auth service is separate from API layer.** The interceptor reads tokens from
  `token.service.ts` (which wraps `localStorage`), not the auth service directly. This
  keeps auth testable in isolation.
- **Result mapper normalizes all responses** into `Result<T>` or `Error` struct before
  any feature code sees the data. Feature pages never handle raw Axios responses.

### Error handling flow

```
Feature Page → useApi() composable → module-api.factory.ts → Axios client
    ↓                                                            │
    │  [success]                                 ←── camelCase transform ←──
    │  Result<T> { isSuccess:true, value }       ←── result.mapper
    │                                                            │
    │  [failure]                                 ←── error-wrapper interceptor
    │  Error struct { code, message, type }      ←──
    ↓
error-handler.ts dispatch:
    400/422 → ValidationError → toast with field messages
    401     → refresh-handler → refresh → retry (or redirect /login)
    403     → toast "Access denied"
    404     → inline empty state or toast
    5xx     → toast "Something went wrong"
```

### What's dropped from legacy

| Item | Reason |
|---|---|
| `common/config/app.ts` | Config lives in env files + existing layout config |
| `common/services/search.service.ts` | Feature-specific, deferred to feature migration |
| `common/test/mock-types.ts` | Replaced by Vitest + `@vue/test-utils` |
| `shared/fields/*.field.ts` | Replaced by Zod schemas + `FormField.vue` |
| Legacy `shared/components/` | Already ported to new admin |

---

## Phase 2: Developer Experience

### Directory structure

```
app/Admin/src/shared/
├── composables/
│   ├── useApi.ts              # Reactive API call (loading, error, data, execute)
│   ├── usePagination.ts       # Page state, next/prev, pageSize change
│   ├── useApiErrorHandler.ts  # Error struct → toast message mapping
│   ├── useFormatter.ts        # Currency, date, number formatting (Intl-aware)
│   ├── usePagedList.ts        # Combines useApi + usePagination for list pages
│   ├── useFilePreview.ts      # File/image preview URL lifecycle management
│   ├── useToast.ts            # Unified toast wrapper (success/error/warn/info)
│   └── index.ts               # Barrel — merges existing useConfirm, useDebounce, useToastNotify
│
├── directives/
│   ├── clickOutside.ts        # Detect clicks outside bound element
│   ├── autofocus.ts           # Auto-focus element on mount
│   └── index.ts               # createDirectivesPlugin() Vue plugin factory
│
├── utils/
│   ├── currency.ts            # Currency formatting and conversion
│   ├── enums.ts               # Enum ↔ label mapping helpers
│   ├── query-builder.ts       # UI state → FilterModel/SearchModel/SortModel objects
│   ├── status.ts              # Status → color/label mapping
│   ├── transform.ts           # Object key transform (snake_case ↔ camelCase)
│   ├── debounce.ts            # Generic debounce (replaces existing)
│   ├── throttle.ts            # Generic throttle
│   └── index.ts
│
├── services/
│   ├── notification.service.ts   # In-app notification queue
│   ├── modal.service.ts          # Reactive modal state (open/close/confirm)
│   ├── event-bus.service.ts      # Typed app-wide event emitter (mitt-style)
│   ├── logger.service.ts         # Structured console logger (dev vs prod)
│   └── index.ts
│
├── validation/
│   ├── schemas/                  # Zod schemas per domain
│   │   ├── catalog.schema.ts
│   │   ├── inventory.schema.ts
│   │   ├── ordering.schema.ts
│   │   └── index.ts
│   ├── rules.ts                  # Common validation rules (required, email, url, etc.)
│   ├── validators.ts             # Zod → vee-validate adapter functions
│   ├── messages.ts               # Error message templates (i18n-ready keys)
│   └── index.ts
│
├── hooks/
│   ├── beforeMount.ts           # onBeforeMount composable wrapper
│   ├── beforeRoute.ts           # onBeforeRouteLeave/Update composable wrapper
│   └── index.ts
│
├── enums/
│   ├── status.enum.ts           # Active, Inactive, Archived, Draft
│   ├── role.enum.ts             # Admin, Manager, Staff, Viewer
│   └── theme.enum.ts            # Light, Dark, System
│
└── types/
    ├── ui.ts                    # Common UI types (Size, Severity, Position, Alignment)
    ├── forms.ts                 # Form state, field metadata types
    └── global.ts                # App-wide utility types (DeepPartial, Nullable, etc.)
```

### Key design

- **Existing composables merged, not replaced.** `useConfirm.ts`, `useDebounce.ts`, and
  `useToastNotify.ts` already exist in the new admin. Legacy `formatter.use.ts` and
  `toast.use.ts` provide richer implementations — merge the best of both.
- **Zod schemas as validation source of truth.** A thin `vee-validate/zod` adapter bridges
  Zod to form components. Schemas organized by domain, matching backend bounded contexts.
  The legacy `fields/` abstraction is fully replaced by this.
- **Directives register as a Vue plugin.** `createDirectivesPlugin()` returns a
  `v-click-outside` + `v-autofocus` plugin. Legacy `tooltip.ts` directive dropped —
  PrimeVue 5's native tooltip directive (already in `plugins/primevue.ts`) covers it.
- **Query builder bridges reactive state to backend queries.** `query-builder.ts` constructs
  `FilterModel`, `SearchModel`, `SortModel` from UI dropdown/search/sort-header state —
  no manual DSL string building in feature code.
- **Services are plain TypeScript classes.** Not Vue composables, not Pinia stores.
  Singletons, injected via `provide/inject`. Testable without mounting Vue.
- **Hooks are thin wrappers** over Vue Router / Vue lifecycle APIs for consistent API patterns.

### What's dropped or deferred

| Item | Reason |
|---|---|
| `common/services/search.service.ts` | Feature-specific — deferred |
| `common/mapper/mapper.utils.ts` | Replaced by `transform.ts` + Zod parsing |
| Custom `tooltip.ts` directive | PrimeVue 5 native tooltip directive replaces it |

---

## Phase 3: Polish

### Directory structure

```
app/Admin/src/shared/
├── localization/
│   ├── index.ts                     # createI18nPlugin() — vue-i18n instance factory
│   └── messages/
│       └── en/
│           ├── general.json         # Common labels (save, cancel, delete, confirm)
│           ├── auth.json            # Login, logout, permissions
│           ├── catalog.json         # Products, taxonomies, options, variants
│           ├── inventory.json       # Stock, transfers, movements, locations
│           ├── ordering.json        # Orders, fulfillment, statuses
│           ├── payment.json         # Payments, methods
│           ├── shipping.json        # Methods, rates
│           ├── location.json        # Countries, states
│           ├── profile.json         # Profile, addresses
│           ├── users.json           # Staff, customers, roles
│           ├── roles.json           # Role names and descriptions
│           ├── error.json           # Error codes and messages
│           └── reports.json         # Report labels
│
├── assets/
│   ├── icons/                       # PNG/SVG icon files
│   ├── images/                      # Brand images, illustrations
│   ├── fonts/                       # Custom web font files
│   └── svg/                         # Inline SVG files
│
├── styles/
│   ├── variables.scss               # SCSS variable overrides
│   ├── mixins.scss                  # Custom SCSS mixins
│   ├── typography.scss              # Font face declarations
│   └── animations.scss              # Keyframe animations
│
└── composables/
    ├── useDate.ts                   # Date formatting / Intl composable
    ├── useCurrency.ts               # Currency display (delegates to useFormatter)
    ├── useWindowSize.ts             # Reactive window dimensions
    ├── useDarkMode.ts               # Extracted from layout.composable.ts, re-imported
    └── useResponsive.ts             # Breakpoint detection (mobile/tablet/desktop)
```

### Key design

- **i18n is lazy-loaded by locale bundle**, not per-page. Each JSON is ~2-8KB — the
  entire `en/` bundle loads on first visit. No waterfall requests.
- **SCSS additions are minimal.** Audit legacy variables/mixins; only port what doesn't
  already exist in PrimeVue preset tokens or Tailwind config. Don't regress the existing
  token-based style architecture.
- **Dark mode refactor.** `useDarkMode.ts` gets extracted from `app/composables/layout.composable.ts`
  into a standalone composable, then re-imported into the layout composable. Keeps
  the layout file focused while making dark mode reusable.
- **Assets are static files only.** No Vue components in `assets/`. SVGs that need
  reactivity become `.vue` components in `shared/components/`.
- **Composables use browser primitives.** `useWindowSize` uses `resize` event; `useResponsive`
  uses `matchMedia`; `useDate`/`useCurrency` use `Intl` API. No framework coupling.
- **Locale keys use nested dot-notation** matching legacy format — preserves existing
  translation structure with minimal key refactoring.

### main.ts wiring (conceptual)

```ts
app.use(primevue)
app.use(pinia)
app.use(router)
app.use(i18nPlugin)
app.use(directivesPlugin)
// Services registered via provide/inject in App.vue
```

### Deferred

| Item | Reason |
|---|---|
| `chart.js` integration | No feature pages use charts yet — wire when dashboard needs it |
| Feature schemas beyond catalog/inventory/ordering | Add as each feature module is built |
| `localization/fr.json` | French locale from legacy; structure supports adding it, but not needed now |
| `shared/components/` legacy extras | Already ported in prior refactoring pass |

---

## Verification

Every phase must pass before proceeding:

```bash
cd app/Admin
pnpm run lint        # ESLint + oxlint (zero warnings)
pnpm run test:unit   # Vitest — all specs green
pnpm run build       # Vite production build succeeds
```

Additionally, after Phase 3: manual smoke test — the app shell (layout, menu, routing)
still functions identically.

## Risks

| Risk | Mitigation |
|---|---|
| PrimeVue 5 API mismatch in directives/composables | Tests adapted to PrimeVue 5 stubs catch at build |
| Interceptor ordering differs between Axios versions | Legacy chain order preserved; tests verify pipeline |
| Zod schema drift from backend FluentValidation rules | Schema field names match backend DTOs one-to-one |
| i18n key collisions with layout keys | Locale namespace prefix: layout uses `layout.*`, features use `domain.*` |
| `unplugin-auto-import` interferes with explicit imports | Conservative config — only auto-imports `vue` + `vue-router` APIs |
| `eslint-plugin-boundaries` conflicts with flat `shared/` | Boundaries config ported from legacy and updated to match new structure |

## Out of Scope

- Feature page logic migration (list/detail pages) — separate effort, covered by
  `2026-07-22-admin-spa-list-detail-pattern-design.md`
- Storefront SPA (`app/Store/`) — not affected
- Backend changes — zero backend changes
- CI/CD pipeline changes — existing CI covers `app/Admin/`
