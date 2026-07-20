# Admin SPA — 4-Layer Architecture Restructure

**Date:** 2026-07-20
**Status:** Approved

## Goal

Restructure `app/Admin/src/` into a 4-layer architecture (`app/` → `features/` → `common/` → `shared/`) following scalable Vue 3 conventions. Address code review findings (duplicated barrels, misplaced files, missing directories). Maintain dotted-convention file naming throughout.

## Layer Responsibilities

| Layer | Contains | Depends On |
|-------|----------|------------|
| `shared/` | Pure types, utility functions, UI components, composables, directives, enums | Nothing |
| `common/` | API client, interceptors, error handlers, auth service, infrastructure services | `shared/` |
| `features/` | Independent business domains (auth, dashboard, error) | `common/`, `shared/` |
| `app/` | Bootstrap, router, layouts | All layers |

**Dependency rule:** No lower layer imports from a higher layer. `shared/` must not import from `common/`, `features/`, or `app/`.

## Design Decisions

### 1. `common/` is a new layer between `shared/` and `features/`

Previously all infrastructure lived in `shared/`, making it a dumping ground for both pure utilities and HTTP client code. `common/` houses core infrastructure that depends on `shared/` types but is not itself a reusable utility.

### 2. Models → `shared/types/`

Domain type definitions (`filtering.model.ts`, `pagination.model.ts`, etc.) are pure TypeScript interfaces with zero runtime deps. They move from `shared/models/` to `shared/types/` since `shared/` = reusable types in the target architecture.

### 3. Mapper transforms → `shared/utils/`

`string.transforms.ts` and `object.transforms.ts` are pure transform utilities, not domain mappers. They move from `mapper/` to `utils/`. Future domain DTO↔model mappers stay in `features/*/api/` alongside their API clients.

### 4. Token service → `common/auth/`

`token.service.ts` is auth infrastructure consumed by interceptors and the auth store. It moves out of `api/http/services/` (HTTP-specific) into `common/auth/` (auth-specific).

### 5. Error type merged into normalizer

`error.type.ts` (single 4-field `ParsedApiError` interface) merges into `error.normalizer.ts`. Single-interface files are overhead.

### 6. `api/utils/` deleted

`api/utils/api.utils.ts` and `api/utils/index.ts` are byte-for-byte identical re-exports of symbols from `handlers/`. Triple duplication with `handlers/index.ts`. Removed.

### 7. `features/auth/services/` + `mappers/` merged into `api/`

`auth.service.ts` (12-line thin wrapper) and `auth.mapper.ts` (session mapping) are only consumed by `auth.api.ts`. Inlined into `auth.api.ts`.

### 8. Feature subdirectories realigned

`views/` → `pages/`, `stores/` → `store/`. Vue page files use `.Page.vue` suffix (e.g., `Login.Page.vue`) per dotted convention. Matches suggested scalable Vue architecture.

### 9. Scaffold empty directories in `common/` and `shared/`

`common/services/`, `common/composables/`, `common/validation/`, `common/errors/`, `common/constants/`, `common/types/` and `shared/components/`, `shared/composables/`, `shared/directives/`, `shared/enums/` created with barrel `index.ts`. Ready for future content.

## File Migration Map

### shared/ → common/ (core infrastructure)

| Old Path | New Path |
|----------|----------|
| `shared/api/http/api.client.ts` | `common/api/client.ts` |
| `shared/api/http/interceptors/auth.interceptor.ts` | `common/api/interceptors/auth.interceptor.ts` |
| `shared/api/http/interceptors/camel-case.interceptor.ts` | `common/api/interceptors/camel-case.interceptor.ts` |
| `shared/api/http/interceptors/error.interceptor.ts` | `common/api/interceptors/error.interceptor.ts` |
| `shared/api/http/interceptors/__tests__/auth.interceptor.spec.ts` | `common/api/interceptors/__tests__/auth.interceptor.spec.ts` |
| `shared/api/http/interceptors/__tests__/camel-case.interceptor.spec.ts` | `common/api/interceptors/__tests__/camel-case.interceptor.spec.ts` |
| `shared/api/http/handlers/error.normalizer.ts` | `common/api/handlers/error.normalizer.ts` |
| `shared/api/http/handlers/refresh.handler.ts` | `common/api/handlers/refresh.handler.ts` |
| `shared/api/http/handlers/__tests__/error.normalizer.spec.ts` | `common/api/handlers/__tests__/error.normalizer.spec.ts` |
| `shared/api/http/services/token.service.ts` | `common/auth/token.service.ts` |
| `shared/api/http/services/__tests__/token.service.spec.ts` | `common/auth/__tests__/token.service.spec.ts` |

### shared/api/ → shared/types/ + shared/utils/ (pure utilities/types)

| Old Path | New Path |
|----------|----------|
| `shared/api/types/result.type.ts` | `shared/types/result.type.ts` |
| `shared/api/query/query-string.builder.ts` | `shared/utils/query-string.builder.ts` |

### shared/models/ → shared/types/

| Old Path | New Path |
|----------|----------|
| `shared/models/error.model.ts` | `shared/types/error.model.ts` |
| `shared/models/filtering.model.ts` | `shared/types/filtering.model.ts` |
| `shared/models/pagination.model.ts` | `shared/types/pagination.model.ts` |
| `shared/models/sorting.model.ts` | `shared/types/sorting.model.ts` |
| `shared/models/searching.model.ts` | `shared/types/searching.model.ts` |
| `shared/models/parameter.model.ts` | `shared/types/parameter.model.ts` |
| `shared/models/response.model.ts` | `shared/types/response.model.ts` |

### shared/mapper/ → shared/utils/

| Old Path | New Path |
|----------|----------|
| `shared/mapper/string.transforms.ts` | `shared/utils/string.transforms.ts` |
| `shared/mapper/object.transforms.ts` | `shared/utils/object.transforms.ts` |
| `shared/mapper/__tests__/string.transforms.spec.ts` | `shared/utils/__tests__/string.transforms.spec.ts` |
| `shared/mapper/__tests__/object.transforms.spec.ts` | `shared/utils/__tests__/object.transforms.spec.ts` |

### features/ realignment

| Old Path | New Path |
|----------|----------|
| `features/auth/views/Login.View.vue` | `features/auth/pages/Login.Page.vue` |
| `features/dashboard/views/Dashboard.View.vue` | `features/dashboard/pages/Dashboard.Page.vue` |
| `features/error/views/NotFound.View.vue` | `features/error/pages/NotFound.Page.vue` |
| `features/auth/stores/auth.store.ts` | `features/auth/store/auth.store.ts` |
| `features/auth/services/auth.service.ts` | **DELETED** — inlined into `auth.api.ts` |
| `features/auth/mappers/auth.mapper.ts` | **DELETED** — inlined into `auth.api.ts` |

### Deleted (no replacement — duplication/overhead)

| Old Path | Reason |
|----------|--------|
| `shared/api/http/handlers/error.type.ts` | Merged into `error.normalizer.ts` |
| `shared/api/utils/api.utils.ts` | Triple-duplicated re-export barrel |
| `shared/api/utils/index.ts` | Pass-through wrapper with zero logic |
| `shared/api/http/services/index.ts` | Directory deleted |
| `shared/api/query/index.ts` | Directory deleted |
| `shared/api/index.ts` | Top-level `shared/api/` deleted |
| `shared/api/http/index.ts` | Replaced by `common/api/index.ts` |
| `shared/api/http/handlers/index.ts` | Replaced by `common/api/handlers/index.ts` |
| `shared/api/http/interceptors/index.ts` | Replaced by `common/api/interceptors/index.ts` |
| `shared/api/types/index.ts` | Replaced by `shared/types/index.ts` |
| `shared/models/index.ts` | Replaced by `shared/types/index.ts` |
| `shared/mapper/index.ts` | Replaced by `shared/utils/index.ts` |

## Barrel Files Created

| File | Exports |
|------|---------|
| `common/index.ts` | `api`, `auth`, `services`, `composables`, `validation`, `errors`, `constants`, `types` |
| `common/api/index.ts` | `client`, `interceptors`, `handlers` |
| `common/auth/index.ts` | `tokenService` |
| `common/services/index.ts` | (empty) |
| `common/composables/index.ts` | (empty) |
| `common/validation/index.ts` | (empty) |
| `common/errors/index.ts` | (empty) |
| `common/constants/index.ts` | (empty) |
| `common/types/index.ts` | (empty) |
| `shared/utils/index.ts` | `toCamelCase`, `mapKeys`, `toCamelCaseKeys`, `buildFilterParam`, `buildSearchParams`, `buildSortParams`, `buildPageParams` |
| `shared/types/index.ts` | All model types + `result.type` |
| `shared/components/index.ts` | (empty) |
| `shared/composables/index.ts` | (empty) |
| `shared/directives/index.ts` | (empty) |
| `shared/enums/index.ts` | (empty) |
| `features/auth/index.ts` | `store`, `api`, `schemas`, `types` |
| `features/dashboard/index.ts` | `pages` |
| `features/error/index.ts` | `pages` |

## Dependency Validation Matrix

```
shared/types/result.type.ts     → nothing             ✅
shared/types/error.model.ts     → nothing             ✅
shared/utils/string.transforms.ts → nothing             ✅
shared/utils/query-string.builder.ts → shared/types/  ✅
common/auth/token.service.ts    → nothing             ✅
common/api/handlers/error.normalizer.ts → shared/types/ ✅
common/api/handlers/refresh.handler.ts → common/auth/ ✅
common/api/interceptors/auth.interceptor.ts → common/auth/ ✅
common/api/interceptors/error.interceptor.ts → shared/types/, common/api/handlers/, common/auth/ ✅
common/api/client.ts           → common/api/interceptors/, common/api/handlers/ ✅
features/auth/api/auth.api.ts  → common/api/client.ts, shared/types/ ✅
features/auth/store/auth.store.ts → features/auth/api/, shared/types/ ✅
app/router/index.ts            → common/, features/, app/layout/ ✅
```

**No `shared/` → `common/` or `common/` → `features/` violations.**

## Risks

- **RISK-001**: Import path changes across ~35 files. Mitigation: `vue-tsc --noEmit` catches all unresolved modules.
- **RISK-002**: Vue component auto-import may break with renamed `.Page.vue` files. Mitigation: Verify Vite resolves renamed SFCs correctly via build.
- **RISK-003**: Test files reference old module paths. Mitigation: Tests live alongside source — moved atomically.

## Verification

1. `vue-tsc --build --noEmit` — zero type errors
2. `pnpm run test:unit` — all 32 existing tests pass
3. `vite build` — production build succeeds
4. `pnpm run lint` — linting passes
