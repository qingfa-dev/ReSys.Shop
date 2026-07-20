# Admin SPA — 4-Layer Architecture Restructure Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restructure `app/Admin/src/` into 4 layers (`app/` → `features/` → `common/` → `shared/`), migrate all files per design spec, update ~35 imports, delete duplicated barrels, inline thin wrappers, scaffold empty directories.

**Architecture:** Layer 1 `shared/types/` + `shared/utils/` (zero deps). Layer 2 `common/api/` + `common/auth/` (depends on `shared/`). Layer 3 `features/` (depends on `common/` + `shared/`). Layer 4 `app/` (depends on all). File naming follows dotted convention: `entity.purpose.ext` for TS, `Pascal.Role.vue` for Vue components.

**Tech Stack:** TypeScript 6, Vue 3.5, Vite 8, Vue Router 5, Pinia 3, Axios 1.18, vitest 4.

## Global Constraints

- `shared/` must not import from `common/`, `features/`, or `app/`
- `common/` must not import from `features/` or `app/`
- All filenames use dotted-convention: `token.service.ts`, `error.normalizer.ts`, `Login.Page.vue`, `AppFooter.Layout.vue`
- Barrel `index.ts` at every directory level
- Zero type errors: `vue-tsc --build --noEmit` must pass after each task
- All 32 existing unit tests must pass after final task
- Production build must succeed after final task

---

### Task 1: Create Directory Scaffold for common/ and new shared/ + features/

**Files:**
- Create: `src/common/index.ts`, `src/common/api/index.ts`, `src/common/api/interceptors/index.ts`, `src/common/api/handlers/index.ts`, `src/common/interceptors/__tests__/` (dir), `src/common/handlers/__tests__/` (dir), `src/common/auth/index.ts`, `src/common/auth/__tests__/` (dir), `src/common/services/index.ts`, `src/common/composables/index.ts`, `src/common/validation/index.ts`, `src/common/errors/index.ts`, `src/common/constants/index.ts`, `src/common/types/index.ts`
- Create: `src/shared/utils/index.ts`, `src/shared/utils/__tests__/` (dir), `src/shared/types/index.ts`, `src/shared/components/index.ts`, `src/shared/composables/index.ts`, `src/shared/directives/index.ts`, `src/shared/enums/index.ts`
- Create: `src/features/auth/index.ts`, `src/features/dashboard/index.ts`, `src/features/error/index.ts`

**Interfaces:**
- Produces: Empty barrel files `common/index.ts`, `shared/utils/index.ts`, `shared/types/index.ts`, `features/auth/index.ts` (populated in later tasks)

- [ ] **Step 1: Create all directories and empty barrel files**

```bash
mkdir -p src/common/api/interceptors/__tests__ \
  src/common/api/handlers/__tests__ \
  src/common/auth/__tests__ \
  src/common/services \
  src/common/composables \
  src/common/validation \
  src/common/errors \
  src/common/constants \
  src/common/types \
  src/shared/utils/__tests__ \
  src/shared/types \
  src/shared/components \
  src/shared/composables \
  src/shared/directives \
  src/shared/enums
```

- [ ] **Step 2: Write empty barrel files**

Write `src/common/index.ts`:
```ts
export * from './api'
export * from './auth'
```

Write `src/common/api/index.ts`:
```ts
export { default as apiClient } from './client'
export * from './interceptors'
export * from './handlers'
```

Write `src/common/api/interceptors/index.ts`:
```ts
export { authInterceptor } from './auth.interceptor'
export { camelCaseInterceptor } from './camel-case.interceptor'
export { errorInterceptor } from './error.interceptor'
```

Write `src/common/api/handlers/index.ts`:
```ts
export { parseApiError, normalizeServerErrors } from './error.normalizer'
```

Write `src/common/auth/index.ts`:
```ts
export { tokenService } from './token.service'
```

Write `src/common/services/index.ts`, `src/common/composables/index.ts`, `src/common/validation/index.ts`, `src/common/errors/index.ts`, `src/common/constants/index.ts`, `src/common/types/index.ts` — each with `export {}`.

Write `src/shared/utils/index.ts`:
```ts
export {}
```

Write `src/shared/types/index.ts`:
```ts
export {}
```

Write `src/shared/components/index.ts`, `src/shared/composables/index.ts`, `src/shared/directives/index.ts`, `src/shared/enums/index.ts` — each with `export {}`.

Write `src/features/auth/index.ts`:
```ts
export * from './store/auth.store'
export * from './api/auth.api'
export * from './schemas/login.schema'
export * from './types/login.request.type'
```

Write `src/features/dashboard/index.ts`:
```ts
export {}
```

Write `src/features/error/index.ts`:
```ts
export {}
```

- [ ] **Step 3: Verify directories exist**

Run: `find src/common src/shared/utils src/shared/types -name 'index.ts' | sort`
Expected: 19 barrel files listed.

- [ ] **Step 4: Commit**

```bash
git add src/common/ src/shared/utils/ src/shared/types/ src/shared/components/ src/shared/composables/ src/shared/directives/ src/shared/enums/ src/features/auth/index.ts src/features/dashboard/index.ts src/features/error/index.ts
git commit -m "chore: scaffold common/, shared/utils/, shared/types/ directories with barrels"
```

---

### Task 2: Move shared/api/types/result.type.ts → shared/types/result.type.ts

**Files:**
- Create: `src/shared/types/result.type.ts` (copy from `src/shared/api/types/result.type.ts`, update internal import)
- Modify: `src/shared/types/result.type.ts` — change internal import path

**Interfaces:**
- Consumes: Current `result.type.ts` content (imports `ErrorModel` from `../../models/error.model`)
- Produces: `src/shared/types/result.type.ts` importing from `./error.model`

- [ ] **Step 1: Copy and fix internal import**

Run: `cp src/shared/api/types/result.type.ts src/shared/types/result.type.ts`

Edit `src/shared/types/result.type.ts` — change line 1:
```ts
// OLD
import type { ErrorModel } from '../../models/error.model'
// NEW
import type { ErrorModel } from './error.model'
```

- [ ] **Step 2: Verify type-check passes with new path (no consumers broken — old file still exists)**

Run: `npx vue-tsc --build --noEmit`
Expected: zero errors.

- [ ] **Step 3: Commit**

```bash
git add src/shared/types/result.type.ts
git commit -m "feat: add result.type.ts to shared/types/ (pre-migration copy)"
```

---

### Task 3: Move shared/models/ → shared/types/

**Files:**
- Create: `src/shared/types/error.model.ts`, `src/shared/types/filtering.model.ts`, `src/shared/types/pagination.model.ts`, `src/shared/types/sorting.model.ts`, `src/shared/types/searching.model.ts`, `src/shared/types/parameter.model.ts`, `src/shared/types/response.model.ts`
- Delete: `src/shared/models/error.model.ts`, ... `src/shared/models/response.model.ts`, `src/shared/models/index.ts`

**Interfaces:**
- Consumes: Existing model files — no internal import changes (they don't import other shared modules)
- Produces: Same exports under `shared/types/`

- [ ] **Step 1: Move all model files**

```bash
mv src/shared/models/error.model.ts src/shared/types/error.model.ts
mv src/shared/models/filtering.model.ts src/shared/types/filtering.model.ts
mv src/shared/models/pagination.model.ts src/shared/types/pagination.model.ts
mv src/shared/models/sorting.model.ts src/shared/types/sorting.model.ts
mv src/shared/models/searching.model.ts src/shared/types/searching.model.ts
mv src/shared/models/parameter.model.ts src/shared/types/parameter.model.ts
mv src/shared/models/response.model.ts src/shared/types/response.model.ts
```

- [ ] **Step 2: Update shared/types/index.ts barrel**

Write `src/shared/types/index.ts`:
```ts
export * from './error.model'
export * from './filtering.model'
export * from './pagination.model'
export * from './sorting.model'
export * from './searching.model'
export * from './parameter.model'
export * from './response.model'
export * from './result.type'
```

- [ ] **Step 3: Delete old models/ directory**

```bash
rm -rf src/shared/models/
```

- [ ] **Step 4: Verify type-check**

Run: `npx vue-tsc --build --noEmit`
Expected: zero errors (barrel in `shared/types/index.ts` already re-exports from local paths).

- [ ] **Step 5: Commit**

```bash
git add src/shared/types/*.model.ts src/shared/models/ src/shared/types/index.ts
git commit -m "refactor: move shared/models/ → shared/types/"
```

---

### Task 4: Move shared/mapper/ → shared/utils/

**Files:**
- Create: `src/shared/utils/string.transforms.ts`, `src/shared/utils/object.transforms.ts`
- Move: `src/shared/mapper/__tests__/` → `src/shared/utils/__tests__/`
- Delete: `src/shared/mapper/`

**Interfaces:**
- Produces: `toCamelCase`, `mapKeys`, `toCamelCaseKeys` under `shared/utils/`
- Updated consumers: `shared/api/http/interceptors/camel-case.interceptor.ts` (import from `@/shared/mapper/object.transforms` → `@/shared/utils/object.transforms`)
- Updated consumers: `shared/index.ts` (import from `./mapper/string.transforms` → `./utils/string.transforms`)

- [ ] **Step 1: Move source files**

```bash
mv src/shared/mapper/string.transforms.ts src/shared/utils/string.transforms.ts
mv src/shared/mapper/object.transforms.ts src/shared/utils/object.transforms.ts
mv src/shared/mapper/__tests__/string.transforms.spec.ts src/shared/utils/__tests__/string.transforms.spec.ts
mv src/shared/mapper/__tests__/object.transforms.spec.ts src/shared/utils/__tests__/object.transforms.spec.ts
```

- [ ] **Step 2: Update consumers of mapper/**

Edit `src/shared/api/http/interceptors/camel-case.interceptor.ts` line 1:
```ts
// OLD
import { toCamelCaseKeys } from '@/shared/mapper/object.transforms'
// NEW
import { toCamelCaseKeys } from '@/shared/utils/object.transforms'
```

Edit `src/shared/index.ts` lines 2-3:
```ts
// OLD
export { toCamelCase } from './mapper/string.transforms'
export { mapKeys, toCamelCaseKeys } from './mapper/object.transforms'
// NEW
export { toCamelCase } from './utils/string.transforms'
export { mapKeys, toCamelCaseKeys } from './utils/object.transforms'
```

- [ ] **Step 3: Populate shared/utils/index.ts barrel**

Write `src/shared/utils/index.ts`:
```ts
export { toCamelCase } from './string.transforms'
export { mapKeys, toCamelCaseKeys } from './object.transforms'
```

- [ ] **Step 4: Delete old mapper/ directory**

```bash
rm -rf src/shared/mapper/
```

- [ ] **Step 5: Verify type-check**

Run: `npx vue-tsc --build --noEmit`
Expected: zero errors.

- [ ] **Step 6: Commit**

```bash
git add src/shared/utils/ src/shared/mapper/ src/shared/api/http/interceptors/camel-case.interceptor.ts src/shared/index.ts
git commit -m "refactor: move shared/mapper/ → shared/utils/"
```

---

### Task 5: Delete duplicated barrels (api/utils/, api/query/index.ts, api/index.ts)

**Files:**
- Delete: `src/shared/api/utils/api.utils.ts`, `src/shared/api/utils/index.ts`, `src/shared/api/query/index.ts`, `src/shared/api/index.ts`, `src/shared/api/types/index.ts`, `src/shared/api/http/index.ts`, `src/shared/api/http/handlers/index.ts`, `src/shared/api/http/interceptors/index.ts`, `src/shared/api/http/services/index.ts`

**Interfaces:**
- Updated consumer: `src/shared/index.ts` (stops importing from `./api/utils/api.utils`, `./api/query`)

- [ ] **Step 1: Delete duplicated barrel files**

```bash
rm -rf src/shared/api/utils/
rm src/shared/api/query/index.ts
rm src/shared/api/index.ts
rm src/shared/api/types/index.ts
rm src/shared/api/http/index.ts
rm src/shared/api/http/handlers/index.ts
rm src/shared/api/http/interceptors/index.ts
rm src/shared/api/http/services/index.ts
```

- [ ] **Step 2: Rewrite src/shared/index.ts to only reference new paths**

Write `src/shared/index.ts`:
```ts
export * from './types'
export * from './utils'
```

- [ ] **Step 3: Verify type-check**

Run: `npx vue-tsc --build --noEmit`
Expected: zero errors (shared/index.ts no longer references deleted paths).

- [ ] **Step 4: Commit**

```bash
git add src/shared/api/ src/shared/index.ts
git commit -m "refactor: delete duplicated barrels, rewrite shared/index.ts"
```

---

### Task 6: Move token.service.ts → common/auth/

**Files:**
- Create: `src/common/auth/token.service.ts` (from `src/shared/api/http/services/token.service.ts`)
- Create: `src/common/auth/__tests__/token.service.spec.ts` (from `src/shared/api/http/services/__tests__/token.service.spec.ts`)
- Delete: `src/shared/api/http/services/` (entire directory)

**Interfaces:**
- Consumes: `token.service.ts` — no internal imports, just `localStorage`
- Produces: `tokenService` under `common/auth/`

- [ ] **Step 1: Move files**

```bash
mv src/shared/api/http/services/token.service.ts src/common/auth/token.service.ts
mv src/shared/api/http/services/__tests__/token.service.spec.ts src/common/auth/__tests__/token.service.spec.ts
rm -rf src/shared/api/http/services/
```

- [ ] **Step 2: Verify type-check (no interceptor references yet — they still import from old path)**

Run: `npx vue-tsc --build --noEmit`
Expected: errors for interceptors still importing from old path. Ignore for now — Task 7 fixes them.

- [ ] **Step 3: Commit**

```bash
git add src/common/auth/ src/shared/api/http/services/
git commit -m "refactor: move token.service → common/auth/"
```

### Task 7: Move API client + interceptors + handlers to common/api/

**Files:**
- Create: `src/common/api/client.ts`, `src/common/api/interceptors/auth.interceptor.ts`, `src/common/api/interceptors/camel-case.interceptor.ts`, `src/common/api/interceptors/error.interceptor.ts`, `src/common/api/handlers/error.normalizer.ts`, `src/common/api/handlers/refresh.handler.ts`
- Delete: `src/shared/api/http/api.client.ts`, `src/shared/api/http/interceptors/auth.interceptor.ts`, `src/shared/api/http/interceptors/camel-case.interceptor.ts`, `src/shared/api/http/interceptors/error.interceptor.ts`, `src/shared/api/http/handlers/error.normalizer.ts`, `src/shared/api/http/handlers/refresh.handler.ts`, `src/shared/api/http/handlers/error.type.ts`

**Interfaces:**
- Consumes: Files from old paths, to be moved and import paths fixed
- Produces: Same exports under `common/api/`
- Updated consumers: `src/features/auth/api/auth.api.ts` (import from `@/shared/api/...` → `@/common/api/...`)
- Updated internal: `client.ts` imports from `./interceptors/` (unchanged), `error.interceptor.ts` imports from `../handlers/` (unchanged), `error.normalizer.ts` imports from `../types/result.type` → `@/shared/types/result.type`

- [ ] **Step 1: Move files to common/api/**

```bash
mv src/shared/api/http/api.client.ts src/common/api/client.ts
mv src/shared/api/http/interceptors/auth.interceptor.ts src/common/api/interceptors/auth.interceptor.ts
mv src/shared/api/http/interceptors/camel-case.interceptor.ts src/common/api/interceptors/camel-case.interceptor.ts
mv src/shared/api/http/interceptors/error.interceptor.ts src/common/api/interceptors/error.interceptor.ts
mv src/shared/api/http/handlers/error.normalizer.ts src/common/api/handlers/error.normalizer.ts
mv src/shared/api/http/handlers/refresh.handler.ts src/common/api/handlers/refresh.handler.ts
```

- [ ] **Step 2: Merge error.type.ts into error.normalizer.ts**

Append the `ParsedApiError` interface at the top of `src/common/api/handlers/error.normalizer.ts` (after the existing imports, before `normalizeServerErrors`):

```ts
export interface ParsedApiError {
  statusCode: number
  title: string | null
  message: string | null
  detail: string | null
  isSuccess: boolean
  errors: Record<string, string[]>
  errorCode: string | undefined
}
```

Delete `src/shared/api/http/handlers/error.type.ts`.

- [ ] **Step 3: Fix import paths in moved files**

Edit `src/common/api/handlers/error.normalizer.ts` line 1:
```ts
// OLD
import type { ServerError } from '../../../api/types/result.type'
// NEW
import type { ServerError } from '@/shared/types/result.type'
```

Edit `src/common/api/handlers/refresh.handler.ts` lines 2-3:
```ts
// OLD
import type { ServerResult } from '../../../api/types/result.type'
import { tokenService } from '../services/token.service'
// NEW
import type { ServerResult } from '@/shared/types/result.type'
import { tokenService } from '@/common/auth/token.service'
```

Edit `src/common/api/interceptors/auth.interceptor.ts` line 2:
```ts
// OLD
import { tokenService } from '../services/token.service'
// NEW
import { tokenService } from '@/common/auth/token.service'
```

Edit `src/common/api/interceptors/error.interceptor.ts` lines 2-5:
```ts
// OLD
import type { ServerResult } from '../../../api/types/result.type'
import { parseApiError } from '../handlers/error.normalizer'
import { tokenService } from '../services/token.service'
import { refreshTokens } from '../handlers/refresh.handler'
// NEW
import type { ServerResult } from '@/shared/types/result.type'
import { parseApiError } from '@/common/api/handlers/error.normalizer'
import { tokenService } from '@/common/auth/token.service'
import { refreshTokens } from '@/common/api/handlers/refresh.handler'
```

Edit `src/common/api/interceptors/error.interceptor.ts` — update the dynamic import on line 38:
```ts
// OLD
const { default: apiClient } = await import('../api.client')
// NEW
const { apiClient } = await import('@/common/api')
```

Edit `src/common/api/interceptors/camel-case.interceptor.ts` line 1:
```ts
// OLD
import { toCamelCaseKeys } from '@/shared/mapper/object.transforms'
// NEW
import { toCamelCaseKeys } from '@/shared/utils/object.transforms'
```

Edit `src/common/api/client.ts` line 2-4 (relative imports stay correct since file structure mirrors original):
```ts
// OLD
import { authInterceptor } from './interceptors/auth.interceptor'
import { camelCaseInterceptor } from './interceptors/camel-case.interceptor'
import { errorInterceptor } from './interceptors/error.interceptor'
// NEW — same, relative paths unchanged
import { authInterceptor } from './interceptors/auth.interceptor'
import { camelCaseInterceptor } from './interceptors/camel-case.interceptor'
import { errorInterceptor } from './interceptors/error.interceptor'
```

- [ ] **Step 4: Update auth.api.ts to import from common/**

Edit `src/features/auth/api/auth.api.ts` lines 1-2:
```ts
// OLD
import apiClient from '@/shared/api/http/api.client'
import type { ServerResult } from '@/shared/api/types/result.type'
// NEW
import { apiClient } from '@/common/api'
import type { ServerResult } from '@/shared/types/result.type'
```

- [ ] **Step 5: Move test files**

```bash
mv src/shared/api/http/interceptors/__tests__/auth.interceptor.spec.ts src/common/api/interceptors/__tests__/auth.interceptor.spec.ts
mv src/shared/api/http/interceptors/__tests__/camel-case.interceptor.spec.ts src/common/api/interceptors/__tests__/camel-case.interceptor.spec.ts
mv src/shared/api/http/handlers/__tests__/error.normalizer.spec.ts src/common/api/handlers/__tests__/error.normalizer.spec.ts
```

- [ ] **Step 6: Update test file imports**

Edit `src/common/api/interceptors/__tests__/auth.interceptor.spec.ts` line 9:
```ts
// OLD
vi.mock('../../services/token.service', () => ({
// NEW
vi.mock('@/common/auth/token.service', () => ({
```

Edit `src/common/api/interceptors/__tests__/camel-case.interceptor.spec.ts` line 4:
```ts
// OLD
vi.mock('@/shared/mapper/object.transforms', () => ({
// NEW
vi.mock('@/shared/utils/object.transforms', () => ({
```

- [ ] **Step 7: Verify type-check**

Run: `npx vue-tsc --build --noEmit`
Expected: zero errors.

- [ ] **Step 8: Commit**

```bash
git add src/common/api/ src/shared/api/http/ src/features/auth/api/auth.api.ts
git commit -m "refactor: move api client + interceptors + handlers to common/api/"
```

---

### Task 8: Move query-string.builder.ts to shared/utils/

**Files:**
- Create: `src/shared/utils/query-string.builder.ts` (from `src/shared/api/query/query-string.builder.ts`)
- Modify: `src/shared/utils/query-string.builder.ts` — update internal import paths

**Interfaces:**
- Produces: `buildFilterParam`, `buildSearchParams`, `buildSortParams`, `buildPageParams` under `shared/utils/`
- Internal imports: `from '../../models/filtering.model'` → `from '../types/filtering.model'`, etc.

- [ ] **Step 1: Move file**

```bash
mv src/shared/api/query/query-string.builder.ts src/shared/utils/query-string.builder.ts
rm -rf src/shared/api/query/
```

- [ ] **Step 2: Fix internal import paths**

Edit `src/shared/utils/query-string.builder.ts` lines 1-4:
```ts
// OLD
import type { FilterModel, FilterGroup, FilterCondition, FilterLogic } from '../../models/filtering.model'
import type { SortModel, SortClause } from '../../models/sorting.model'
import type { SearchModel } from '../../models/searching.model'
import type { PageModel } from '../../models/pagination.model'
// NEW
import type { FilterModel, FilterGroup, FilterCondition, FilterLogic } from '../types/filtering.model'
import type { SortModel, SortClause } from '../types/sorting.model'
import type { SearchModel } from '../types/searching.model'
import type { PageModel } from '../types/pagination.model'
```

- [ ] **Step 3: Update shared/utils/index.ts barrel**

Append to `src/shared/utils/index.ts` (add after existing exports):
```ts
export {
  buildFilterParam,
  buildSearchParams,
  buildSortParams,
  buildPageParams,
} from './query-string.builder'
```

- [ ] **Step 4: Verify type-check**

Run: `npx vue-tsc --build --noEmit`
Expected: zero errors.

- [ ] **Step 4: Commit**

```bash
git add src/shared/utils/query-string.builder.ts src/shared/api/query/
git commit -m "refactor: move query-string.builder → shared/utils/"
```

---

### Task 9: Rename feature subdirectories (views/ → pages/, stores/ → store/) + inline auth.service.ts + auth.mapper.ts

**Files:**
- Rename: `features/auth/views/Login.View.vue` → `features/auth/pages/Login.Page.vue`
- Rename: `features/dashboard/views/Dashboard.View.vue` → `features/dashboard/pages/Dashboard.Page.vue`
- Rename: `features/error/views/NotFound.View.vue` → `features/error/pages/NotFound.Page.vue`
- Rename: `features/auth/stores/auth.store.ts` → `features/auth/store/auth.store.ts`
- Modify: `features/auth/pages/Login.Page.vue` — update relative import from `../stores/auth.store` → `../store/auth.store`
- Modify: `features/auth/api/auth.api.ts` — inline `mapAuthSession` function, rename `authRepository` → `authService`, remove import of `../mappers/auth.mapper`
- Modify: `features/auth/store/auth.store.ts` — update import from `../services/auth.service` → `../api/auth.api`, call `authService.login()` (now exported from auth.api.ts)
- Modify: `src/app/router/index.ts` — update lazy import paths
- Delete: `features/auth/services/auth.service.ts`, `features/auth/mappers/auth.mapper.ts`
- Create: `features/auth/types/index.ts`

**Interfaces:**
- Consumes: `auth.api.ts` exports `authService` (renamed from `authRepository`)
- Produces: `authService`, `store/` path, `pages/` path

- [ ] **Step 1: Create feature subdirectories and move files**

```bash
mkdir -p src/features/auth/pages src/features/auth/store src/features/dashboard/pages src/features/error/pages
mv src/features/auth/views/Login.View.vue src/features/auth/pages/Login.Page.vue
mv src/features/dashboard/views/Dashboard.View.vue src/features/dashboard/pages/Dashboard.Page.vue
mv src/features/error/views/NotFound.View.vue src/features/error/pages/NotFound.Page.vue
mv src/features/auth/stores/auth.store.ts src/features/auth/store/auth.store.ts
rm -rf src/features/auth/views/ src/features/auth/stores/ src/features/dashboard/views/ src/features/error/views/
```

- [ ] **Step 2: Update Login.Page.vue import**

Edit `src/features/auth/pages/Login.Page.vue` line 4:
```ts
// OLD
import { useAuthStore } from '../stores/auth.store'
// NEW
import { useAuthStore } from '../store/auth.store'
```

- [ ] **Step 3: Inline auth.mapper.ts into auth.api.ts + rename authRepository → authService**

Edit `src/features/auth/api/auth.api.ts` — delete line 7 (`import { mapAuthSession } from '../mappers/auth.mapper'`), add the `mapAuthSession` function inline, rename `authRepository` to `authService`:

```ts
import { apiClient } from '@/common/api'
import type { ServerResult } from '@/shared/types/result.type'
import type { LoginResponse } from '../types/login.response.type'
import type { LoginRequest } from '../types/login.request.type'
import type { RefreshTokenRequest, AuthProfileResponse } from '../types/auth.request.type'
import type { AuthSession } from '../types/auth.model.type'

const BASE_URL = '/store/identity/auth'

function path(sub: string): string {
  return `${BASE_URL}/${sub}`
}

function mapAuthSession(login: LoginResponse, session: { id: string; roles: string[]; permissions: string[] } | null): AuthSession {
  return {
    accessToken: login.accessToken,
    accessTokenExpiresIn: login.accessTokenExpiresIn,
    refreshToken: login.refreshToken,
    refreshTokenExpiresIn: login.refreshTokenExpiresIn,
    user: session ? { id: session.id, roles: session.roles, permissions: session.permissions } : null,
  }
}

async function fetchSession(): Promise<{ id: string; roles: string[]; permissions: string[] } | null> {
  try {
    const res = await apiClient.get(path('profile'))
    const data = res.data as ServerResult<AuthProfileResponse>
    if (data.isSuccess && data.value) {
      return {
        id: data.value.id,
        roles: Array.isArray(data.value.roles) ? data.value.roles : [],
        permissions: Array.isArray(data.value.permissions) ? data.value.permissions : [],
      }
    }
  } catch {
    /* ignore */
  }
  return null
}

export const authService = {
  async login(request: LoginRequest): Promise<ServerResult<AuthSession>> {
    const res = await apiClient.post(path('login/password'), request)
    const result = res.data as ServerResult<LoginResponse>
    if (!result.isSuccess) return result as unknown as ServerResult<AuthSession>
    const session = await fetchSession()
    return {
      ...result,
      value: mapAuthSession(result.value, session),
    } as ServerResult<AuthSession>
  },

  async refresh(request: RefreshTokenRequest): Promise<ServerResult<AuthSession>> {
    const res = await apiClient.post(path('sessions/refresh'), request)
    const result = res.data as ServerResult<LoginResponse>
    if (!result.isSuccess) return result as unknown as ServerResult<AuthSession>
    const session = await fetchSession()
    return {
      ...result,
      value: mapAuthSession(result.value, session),
    } as ServerResult<AuthSession>
  },

  async logout(): Promise<ServerResult<void>> {
    const res = await apiClient.post(path('logout'), {})
    return res.data as ServerResult<void>
  },

  async getProfile(): Promise<ServerResult<unknown>> {
    const res = await apiClient.get(path('profile'))
    return res.data as ServerResult<unknown>
  },
}
```

- [ ] **Step 4: Update auth.store.ts to import from auth.api.ts**

Edit `src/features/auth/store/auth.store.ts` lines 3 and 6:
```ts
// OLD
import { authService } from '../services/auth.service'
import type { ServerResult } from '@/shared/api/types/result.type'
// NEW
import { authService } from '../api/auth.api'
import type { ServerResult } from '@/shared/types/result.type'
```

- [ ] **Step 5: Delete auth.service.ts and auth.mapper.ts**

```bash
rm src/features/auth/services/auth.service.ts src/features/auth/mappers/auth.mapper.ts
rm -rf src/features/auth/services/ src/features/auth/mappers/
```

- [ ] **Step 6: Update router lazy imports**

Edit `src/app/router/index.ts` lines 11, 21, 28:
```ts
// OLD
component: () => import('@/features/auth/views/Login.View.vue'),
component: () => import('@/features/dashboard/views/Dashboard.View.vue'),
component: () => import('@/features/error/views/NotFound.View.vue'),
// NEW
component: () => import('@/features/auth/pages/Login.Page.vue'),
component: () => import('@/features/dashboard/pages/Dashboard.Page.vue'),
component: () => import('@/features/error/pages/NotFound.Page.vue'),
```

Edit `src/app/router/index.ts` line 3:
```ts
// OLD
import { useAuthStore } from '@/features/auth/stores/auth.store'
// NEW
import { useAuthStore } from '@/features/auth/store/auth.store'
```

- [ ] **Step 7: Create features/auth/types/index.ts barrel**

Write `src/features/auth/types/index.ts`:
```ts
export type { AuthSession } from './auth.model.type'
export type { RefreshTokenRequest, AuthProfileResponse } from './auth.request.type'
export type { LoginRequest } from './login.request.type'
export type { LoginResponse } from './login.response.type'
```

- [ ] **Step 8: Update features/auth/index.ts**

Write `src/features/auth/index.ts`:
```ts
export { useAuthStore } from './store/auth.store'
export { authService } from './api/auth.api'
export { createLoginSchema } from './schemas/login.schema'
export type { LoginParameters } from './schemas/login.schema'
export type { LoginRequest } from './types/login.request.type'
```

- [ ] **Step 9: Verify type-check**

Run: `npx vue-tsc --build --noEmit`
Expected: zero errors.

- [ ] **Step 10: Commit**

```bash
git add src/features/ src/app/router/index.ts
git commit -m "refactor: views/→pages/, stores/→store/, inline auth.service+mapper"
```

---

### Task 10: Run Tests and Build

**Files:**
- None (verification only)

- [ ] **Step 1: Run unit tests**

Run: `pnpm run test:unit`
Expected: all 32 tests pass.

- [ ] **Step 2: Run production build**

Run: `npx vite build`
Expected: build succeeds.

- [ ] **Step 3: Run lint**

Run: `pnpm run lint`
Expected: passes (or only pre-existing warnings).

- [ ] **Step 4: Run type-check one final time**

Run: `npx vue-tsc --build --noEmit`
Expected: zero errors.

- [ ] **Step 5: Commit any final changes**

```bash
git status
git add -A
git commit -m "chore: final verification — tests, build, type-check pass after 4-layer restructure"
```
