# Admin SPA Refactor — Design Spec

**Date**: 2026-07-21
**Status**: Draft
**Approach**: Big Bang by Layer (A)

---

## Goal

Restructure the Admin SPA (`app/Admin/`) to a 4-layer architecture (`app` → `features` → `common` → `shared`) with consistent naming conventions, shared Zod field inheritance, and reusable component organization. Fix 18 review issues during the refactor.

---

## §1 — Target Layer Architecture

```
app ──→ common ──→ shared
  │                  │
  └──→ features ────┘
```

| Layer | Responsibility | Depends On |
|-------|---------------|------------|
| `shared/` | Reusable UI components, Zod fields, styles, locales, directives, enums | Nothing (third-party only) |
| `common/` | Core infrastructure: API client, interceptors, composables, services, errors, config | `shared/` only |
| `features/` | Independent business domains (catalog, ordering, payment, etc.) | `common/`, `shared/` |
| `app/` | Bootstrap: router, layouts, auth bootstrap, plugins | All layers |

### `src/common/` — what moves here

| From `src/shared/` | To `src/common/` |
|---------------------|-------------------|
| `api/` (client, interceptors, handlers, constants, types, utils, module-api.factory, api.file.service) | `api/` |
| `composables/` (usePagedList, useToast, useApiErrorHandler, useFilePreview, useFormatter) | `composables/` |
| `services/search.service.ts` | `services/` |
| `mapper/mapper.utils.ts` | `mapper/` (or merged into utils) |
| `utils/query-builder.utils.ts`, `utils/transform.ts` | `utils/` |
| `config/app.ts` | `config/` |
| `test/mock-types.ts` | `test/` |
| — | `errors/ApiError.ts` (new) |
| — | `auth/` (new — extracted from features/auth/) |

### What stays in `src/shared/`

| Directory | Content |
|-----------|---------|
| `components/` | Reusable UI components (reorganized into subdirs) |
| `fields/` | New — shared Zod base field schemas |
| `locales/` | i18n translation files |
| `utils/` | Presentation utils: `currency.ts`, `enums.ts` |
| `assets/` | SCSS, Tailwind, images |

---

## §2 — Component Naming Convention

| Type | Convention | Example (before → after) |
|------|-----------|---------------------------|
| Pages | `PascalCasePage.vue` | `ProductList.View.vue` → `ProductListPage.vue` |
| Layouts | `PascalCaseLayout.vue` | `Main.Layout.vue` → `MainLayout.vue` |
| Feature components | `PascalCase.vue` | `ConfirmButton.Component.vue` → `ConfirmButton.vue` |
| Shared components | `PascalCase.vue` | Same — organized into subdirs |
| Composables | `useXxx.ts` | Unchanged |
| Stores | `xxx.store.ts` | Unchanged (plus `.actions.ts`, `.getters.ts`) |
| API | `xxx.api.ts` | Unchanged |
| Types | `xxx.{field,request,response,parameters,query}.ts` | Drop `.type` infix |
| Models | `xxx.model.ts` | Unchanged from current |
| Tests | `*.spec.ts` in `__tests__/` | Unchanged, directory standardized |

---

## §3 — Feature Internal Structure

```text
features/{domain}/{entity}/
├── pages/                 # Route-level pages, PascalCasePage.vue
├── components/            # Feature-specific sub-components, PascalCase.vue
├── store/                 # Pinia store + split actions/getters
│   ├── xxx.store.ts
│   ├── xxx.actions.ts
│   └── xxx.getters.ts
├── api/
│   └── xxx.api.ts         # HTTP + mapping (absorbs former services/ + mappers/)
├── models/
│   └── xxx.model.ts       # UI domain models + mappers
├── types/
│   ├── xxx.field.ts       # Zod field definitions (was schemas/)
│   ├── xxx.request.ts     # Zod schema + inferred types
│   ├── xxx.response.ts    # Backend DTO shapes
│   ├── xxx.parameters.ts  # Route params
│   └── xxx.query.ts       # List filter/sort/search
├── composables/           # Feature-specific composables
├── __tests__/             # Colocated tests, *.spec.ts
├── routes.ts              # Feature route definitions
└── index.ts               # Barrel re-export
```

### Changes from current

- `views/` → `pages/`
- `stores/` → `store/` (singular)
- `services/` + `mappers/` → merged into `api/` + `models/`
- `schemas/` → `types/*.field.ts`
- `_tests/`, `tests/` → `__tests__/`
- Added: `composables/`, `models/`, `index.ts`

---

## §4 — Shared Component Reorganization

```text
shared/components/
├── base/
│   └── ConfirmButton.vue
├── form/
│   ├── FormField.vue
│   └── SearchInput.vue
├── tables/
│   ├── DataTableShell.vue
│   └── LoadingOverlay.vue
├── data-display/
│   ├── StatCard.vue
│   ├── DetailField.vue
│   ├── TabbedDetail.vue
│   ├── MetadataManager.vue
│   ├── DescriptionList.vue
│   ├── DetailGroup.vue
│   └── CopyButton.vue
├── feedback/
│   ├── EmptyState.vue
│   ├── StatusBadge.vue
│   ├── SkeletonLoader.vue
│   ├── DeleteDialog.vue
│   ├── ErrorState.vue          (new)
│   └── Drawer.vue              (new — wraps PrimeVue Drawer)
├── navigation/
│   ├── Breadcrumb.vue
│   ├── PageShell.vue
│   ├── PageHeader.vue
│   ├── PageContainer.vue
│   ├── Section.vue
│   └── ManagerWelcome.vue
```

Components already added by recent commits (`SearchInput`, `DescriptionList`, `DetailGroup`, `CopyButton`, `SkeletonLoader`, `DeleteDialog`, `LoadingOverlay`, `PageContainer`, `Section`) — just sort into subdirs.

**Deferred (Phase 2)** for follow-up: charts, filters, overlays, actions, media, badges.

---

## §5 — Shared Zod Field Inheritance

```text
shared/fields/
├── index.ts
├── base.field.ts          # id, createdAtUtc, updatedAtUtc
├── name.field.ts          # name, slug, description
├── address.field.ts       # line1, line2, city, state, postalCode, countryCode
├── money.field.ts         # amount, currency
└── seo.field.ts           # metaTitle, metaDescription
```

Feature field files compose from shared:

```typescript
// features/catalog/products/types/product.field.ts
import { baseFields, nameFields, seoFields } from '@/shared/fields'

export const productFields = baseFields.merge(nameFields).merge(seoFields).extend({
  styleCode: z.string().min(1),
  status: z.number(),
})
```

```typescript
// features/catalog/products/types/product.request.ts
import { productFields } from './product.field'

export const createProductSchema = productFields.pick({
  name: true, styleCode: true, description: true, status: true,
})
export type CreateProductRequest = z.infer<typeof createProductSchema>
```

---

## §6 — Review Issues Fix Map

All 18 code review findings mapped to execution steps:

### Fixed in Step 1 (common/ split)

| # | File | Issue | Fix |
|---|------|-------|-----|
| 1 | `api.types.ts` + `result.mapper.ts` | Duplicate `SuccessResult`/`FailureResult`/`mapToErrors` | Delete `api.types.ts`, keep `result.mapper.ts` |
| 4 | `module-api.factory.ts` | Unused `TCreate, TUpdate` generics | Remove |
| 5 | `auth.api.ts:21-29` | `fetchSession` silently swallows all errors | Log error before returning `null` |
| 6 | `api-error-handler.use.ts:19+` | `console.log` debug traces in production | Guard with `import.meta.env.DEV` |
| 7 | `paged-list.use.ts:69-71` | `as unknown as Partial<TParams>` unsafe cast | Use explicit `Pick<TParams, ...>` |
| 8 | `paged-list.use.ts:56-57` | Sets `error.value` then re-throws | Drop `throw err` |
| 9 | `query-builder.utils.ts:145` | `formatValue` doesn't escape `,` `(` `)` `\|` | URL-encode special chars |
| 10 | `mapper.utils.ts:14` | `toCamelCaseKeys` only top-level, nested stays snake_case | Add recursive `deepCamelCaseKeys` |
| 11 | `toast.use.ts:7` + `App.vue:9` | Custom `toastBus` ref + bridge indirection | Replace with direct `useToast()` from PrimeVue, remove bridge |
| 13 | `constants.ts:1-8` | Bare string paths (no prefix) | Add `/` prefix: `'/catalog'` |
| 15 | `error-handler.ts:33-116` | 84-line function checks 4 variants per field | Normalize casing at interceptor level, simplify |
| 16 | `auth.api.ts:38-41` | `as ServerResult<AuthSession>` unsafe cast | Return properly typed mapper result |
| 17 | `transform.ts:4-6` | Failure path keeps `.value` with wrong type `T` | Return `{ ...result, value: undefined as never }` on failure |

### Fixed in Step 3 (feature restructure)

| # | File | Issue | Fix |
|---|------|-------|-----|
| 2 | `error-wrapper.interceptor.ts:13` | URL guard checks `/auth/session/refresh` but actual is `sessions` (plural) | Change to `.includes('/sessions/refresh')` |
| 3 | `refresh-handler.ts:7` | `window.location.href` side-effect inside utility | Move redirect to caller, emit event |
| 12 | `DataTableShell.vue` | `createRoute: any` | Type as `RouteLocationRaw` |
| 14 | (multiple) | Inconsistent test dirs: `_tests/`, `__tests__/`, `tests/` | Standardize to `__tests__/` |
| 18 | `product.api.ts` | Mixed `async/await` + `.then()` | Pick one style |

---

## §7 — ESLint Boundaries (4-Layer)

```typescript
settings: {
  'boundaries/elements': [
    { type: 'shared',   pattern: 'src/shared/**/*',   mode: 'folder' },
    { type: 'common',   pattern: 'src/common/**/*',   mode: 'folder' },
    { type: 'features', pattern: 'src/features/**/*', mode: 'folder' },
    { type: 'app',      pattern: 'src/app/**/*',      mode: 'folder' },
  ],
},
rules: {
  'boundaries/element-types': ['error', {
    default: 'allow',
    rules: [
      { from: 'shared',   disallow: ['common', 'features', 'app'] },
      { from: 'common',   disallow: ['features', 'app'] },
      { from: 'features', disallow: ['features', 'app'] },
      { from: 'app',      allow:    ['shared', 'common', 'features'] },
    ],
  }],
},
```

`unplugin-auto-import` dirs update: `src/shared/composables` → `src/common/composables`.

---

## §8 — Execution Plan (4 Steps)

### Step 1: Create `src/common/` (highest risk)

1. Create directory tree under `src/common/`
2. Move `src/shared/api/` → `src/common/api/`
3. Move `src/shared/composables/` → `src/common/composables/`
4. Move `src/shared/services/` → `src/common/services/`
5. Move `src/shared/mapper/` → `src/common/mapper/`
6. Move `src/shared/utils/query-builder.utils.ts`, `transform.ts` → `src/common/utils/`
7. Move `src/shared/config/app.ts` → `src/common/config/`
8. Move `src/shared/test/` → `src/common/test/`
9. Create `src/common/errors/ApiError.ts`
10. Update all `@/shared/` → `@/common/` imports across features + app
11. Fix review issues: #1, #4, #5, #6, #7, #8, #9, #10, #11, #13, #15, #16, #17
12. Update `eslint.config.ts` boundaries (4-layer)
13. Update `unplugin-auto-import` config
14. Verify: `pnpm run type-check && pnpm run lint && pnpm run test:unit`

### Step 2: Reorganize `src/shared/` + add shared fields

1. Create component subdirs: `base/`, `form/`, `tables/`, `data-display/`, `feedback/`, `navigation/`
2. Move existing components into subdirs, rename: drop `.Component` suffix
3. Create `src/shared/fields/` with `base.field.ts`, `name.field.ts`, `address.field.ts`, `money.field.ts`, `seo.field.ts`
4. Update all feature imports to new component paths
5. Verify: `pnpm run type-check && pnpm run lint && pnpm run test:unit`

### Step 3: Restructure features

1. Per feature: `views/` → `pages/`, rename `*.View.vue` → `*Page.vue`
2. `stores/` → `store/` (singular). Split into `*.actions.ts` + `*.getters.ts` **only** if store exceeds 150 lines or 10 actions (current stores are small — only `order.store.ts` at ~184 lines likely needs splitting)
3. `schemas/` → `types/*.field.ts` with shared field inheritance
4. Rename type files: drop `.type` infix (`.model.type.ts` → `.model.ts`, `.request.type.ts` → `.request.ts`, etc.)
5. Feature components: drop `.Component` suffix
6. Merge `services/` + `mappers/`: HTTP-calling logic → `api/xxx.api.ts`, mapping/conversion functions → `models/xxx.model.ts`
7. Standardize `__tests__/`
8. Add `index.ts` barrel + `composables/` directory per feature
9. App layout: rename `*.Layout.vue` → `*Layout.vue`
10. Update app router/layout imports
11. Fix review issues: #2, #3, #12, #14, #18
12. Verify: `pnpm run type-check && pnpm run lint && pnpm run test:unit`

### Step 4: Add new shared components

1. Add `ErrorState.vue` in `shared/components/feedback/` with `__tests__/`
2. Add `Drawer.vue` in `shared/components/feedback/` (wraps PrimeVue Drawer) with `__tests__/`
3. Verify: `pnpm run type-check && pnpm run lint && pnpm run test:unit`

---

## §9 — Verification Gates

Per-step verification:
```bash
cd app/Admin
pnpm run type-check     # vue-tsc --build (catches import errors)
pnpm run lint           # eslint + oxlint (catches boundary violations)
pnpm run test:unit      # vitest (catches behavioral regressions)
```

**Gate**: all three must pass green before proceeding to next step. If a step fails, fix inline before moving on. If a fix requires backprop (same bug pattern found elsewhere), record in this spec's appendix and fix all instances.
