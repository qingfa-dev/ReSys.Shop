# Admin SPA Shared Infrastructure Migration — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Migrate all shared infrastructure (types, API client, auth, composables, directives, utils, services, validation, i18n, styles, assets) from legacy `app/lagacy/Admin/` to new `app/Admin/src/shared/` in three independently-verifiable phases.

**Architecture:** Flat `src/shared/` directory with barrel exports per subdirectory. Frontend types mirror backend naming exactly (`Result<T>`, `Error`, `PagedResult<T>`, `QueryingParameters`). Axios-based HTTP client with auth/camelCase/error interceptors. Auth via JWT token service. All code ported from legacy with PrimeVue 5 adjustments where needed.

**Tech Stack:** Vue 3.5 + TypeScript 6 + PrimeVue 5 + Axios + vue-i18n + vee-validate + zod + unplugin-auto-import

## Global Constraints

- All files under `app/Admin/src/shared/` — flat structure, no `common/` split
- Types mirror backend: `Result<T>`, `Error`, `PagedResult<T>`, `QueryingParameters`, `FilterOperator`
- Libraries: axios, vue-i18n, vee-validate, zod, unplugin-auto-import, chart.js, eslint-plugin-boundaries, @vitest/coverage-v8
- Convert legacy `@/common/` imports to `@/shared/`
- Rename `ServerResult<T>` → `Result<T>`, `ServerError` → `Error` struct, `ServerPagedResult<T>` → `PagedResult<T>`
- Each phase must pass: `cd app/Admin && pnpm run lint && pnpm run test:unit && pnpm run build`
- Legacy reference: `app/lagacy/Admin/src/common/` and `app/lagacy/Admin/src/shared/`

---

## Phase 1: Foundation

### Task 1: Install packages and configure auto-import

**Files:**
- Modify: `app/Admin/package.json`
- Modify: `app/Admin/vite.config.ts`
- Modify: `app/Admin/src/app/main.ts` (or `app/Admin/src/main.ts`)

**Interfaces:**
- Consumes: Nothing
- Produces: All runtime + dev deps installed, `unplugin-auto-import` configured, `@/shared/` path alias works

- [ ] **Step 1: Install runtime dependencies**

```bash
cd app/Admin
pnpm add axios vue-i18n vee-validate zod @vee-validate/zod jwt-decode chart.js
```

Run: `pnpm add axios vue-i18n vee-validate zod @vee-validate/zod jwt-decode chart.js`
Expected: Packages installed, `package.json` and `pnpm-lock.yaml` updated.

- [ ] **Step 2: Install dev dependencies**

```bash
pnpm add -D unplugin-auto-import eslint-plugin-boundaries @vitest/coverage-v8
```

Expected: Dev packages installed.

- [ ] **Step 3: Add `unplugin-auto-import` to Vite config**

Read `app/Admin/vite.config.ts`. Add the `unplugin-auto-import` plugin after `vueJsx()`:

```ts
import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueJsx from '@vitejs/plugin-vue-jsx'
import vueDevTools from 'vite-plugin-vue-devtools'
import tailwindcss from '@tailwindcss/vite'
import AutoImport from 'unplugin-auto-import/vite'

export default defineConfig({
  plugins: [
    vue(),
    vueJsx(),
    AutoImport({
      imports: ['vue', 'vue-router'],
      dts: 'src/auto-imports.d.ts',
    }),
    vueDevTools(),
    tailwindcss(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: process.env.services__api__https__0
          || process.env.services__api__http__0
          || process.env.VITE_API_URL
          || 'http://localhost:5035',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
```

- [ ] **Step 4: Verify build**

```bash
pnpm run build
```

Expected: Build succeeds.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/package.json app/Admin/pnpm-lock.yaml app/Admin/vite.config.ts
git commit -m "chore(admin): install shared infra dependencies and configure auto-import"
```

---

### Task 2: Backend-aligned type definitions

**Files:**
- Create: `app/Admin/src/shared/models/result.ts`
- Create: `app/Admin/src/shared/models/querying.ts`
- Create: `app/Admin/src/shared/models/pagination.ts`
- Create: `app/Admin/src/shared/models/api.ts`
- Create: `app/Admin/src/shared/models/index.ts`

**Interfaces:**
- Consumes: Nothing
- Produces: `Result<T>`, `PagedResult<T>`, `Error`, `QueryingModel`, `FilterModel`, `SearchModel`, `SortModel`, `PageModel`, `FilterOperator`, `FilterLogic`, `SortDirection`, `SortNulls`, `SearchMode`, `PaginationMeta`, `RequestOptions`

- [ ] **Step 1: Create `shared/models/result.ts`**

```ts
export interface Error {
  code: string
  message: string
  type: number
  metadata: Record<string, unknown> | null
}

export interface Result<T> {
  isSuccess: boolean
  statusCode: number
  errors: Error[]
  message: string | null
  metadata: Record<string, unknown> | null
  value: T
}

export interface PagedResult<T> {
  isSuccess: boolean
  statusCode: number
  errors: Error[]
  message: string | null
  metadata: Record<string, unknown> | null
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}
```

- [ ] **Step 2: Create `shared/models/querying.ts`**

```ts
export type FilterOperator =
  | 'Equal'
  | 'EqualCaseSensitive'
  | 'NotEqual'
  | 'GreaterThan'
  | 'GreaterThanOrEqual'
  | 'LessThan'
  | 'LessThanOrEqual'
  | 'Contains'
  | 'ContainsCaseSensitive'
  | 'NotContains'
  | 'StartsWith'
  | 'StartsWithCaseSensitive'
  | 'NotStartsWith'
  | 'EndsWith'
  | 'EndsWithCaseSensitive'
  | 'NotEndsWith'

export type FilterLogic = 'And' | 'Or'

export interface FilterCondition {
  field: string
  operator: FilterOperator
  value: string
}

export interface FilterGroup {
  logic: FilterLogic
  conditions: FilterCondition[]
  groups: FilterGroup[]
}

export interface FilterModel {
  root?: FilterGroup
  conditions: FilterCondition[]
  allowedFields: string[]
  violations: string[]
}

export type SortDirection = 'Ascending' | 'Descending'

export type SortNulls = 'First' | 'Last'

export interface SortClause {
  field: string
  direction: SortDirection
  nulls?: SortNulls
}

export interface SortModel {
  clauses: SortClause[]
  allowedFields: string[]
  violations: string[]
}

export type SearchMode = 'Any' | 'All'

export interface SearchTerm {
  value: string
  caseSensitive: boolean
}

export interface SearchModel {
  term: SearchTerm
  fields: string[]
  mode: SearchMode
  allowedFields: string[]
  violations: string[]
}

export interface PageBounds {
  defaultPage: number
  defaultPageSize: number
  maxPageSize: number
}

export interface PageModel {
  page: number
  pageSize: number
  isEmpty: boolean
  bounds: PageBounds
  violations: string[]
}

export interface QueryingModel {
  filter: FilterModel
  search: SearchModel
  sort: SortModel
  page: PageModel
}
```

- [ ] **Step 3: Create `shared/models/pagination.ts`**

```ts
export interface PaginationMeta {
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}
```

- [ ] **Step 4: Create `shared/models/api.ts`**

```ts
import type { Error } from './result'
import type { QueryingModel } from './querying'

export interface ApiError {
  statusCode: number
  message: string
  errors: Error[]
}

export interface RequestOptions {
  query?: QueryingModel
  signal?: AbortSignal
  headers?: Record<string, string>
}
```

- [ ] **Step 5: Create `shared/models/index.ts`**

```ts
export type {
  Error,
  Result,
  PagedResult,
} from './result'

export type {
  FilterOperator,
  FilterLogic,
  FilterCondition,
  FilterGroup,
  FilterModel,
  SortDirection,
  SortNulls,
  SortClause,
  SortModel,
  SearchMode,
  SearchTerm,
  SearchModel,
  PageBounds,
  PageModel,
  QueryingModel,
} from './querying'

export type { PaginationMeta } from './pagination'
export type { ApiError, RequestOptions } from './api'
```

- [ ] **Step 6: Verify compilation**

```bash
cd app/Admin && pnpm run build
```

Expected: Build succeeds.

- [ ] **Step 7: Commit**

```bash
git add app/Admin/src/shared/models/
git commit -m "feat(admin): add backend-aligned shared types (Result<T>, QueryingModel)"
```

---

### Task 3: Shared constants

**Files:**
- Create: `app/Admin/src/shared/constants/api.ts`
- Create: `app/Admin/src/shared/constants/routes.ts`
- Create: `app/Admin/src/shared/constants/permissions.ts`
- Create: `app/Admin/src/shared/constants/regex.ts`
- Create: `app/Admin/src/shared/constants/storage.ts`
- Create: `app/Admin/src/shared/constants/index.ts`

**Interfaces:**
- Consumes: Nothing
- Produces: API path constants, route constants, permission strings, regex patterns, localStorage keys

- [ ] **Step 1: Create `shared/constants/api.ts`**

Port from `app/lagacy/Admin/src/common/api/constants.ts`:

```ts
export const CATALOG = 'catalog'
export const IDENTITY = 'identity'
export const INVENTORY = 'inventory'
export const LOCATION = 'location'
export const ORDERING = 'ordering'
export const PAYMENT = 'payment'
export const PROFILE = 'profile'
export const SHIPPING = 'shipping'
export const USERS = 'users'

export const API_MODULES = {
  CATALOG,
  IDENTITY,
  INVENTORY,
  LOCATION,
  ORDERING,
  PAYMENT,
  PROFILE,
  SHIPPING,
  USERS,
} as const

export type ApiModule = (typeof API_MODULES)[keyof typeof API_MODULES]
```

- [ ] **Step 2: Create `shared/constants/routes.ts`**

```ts
export const ROUTES = {
  LOGIN: '/login',
  DASHBOARD: '/',
  CATALOG: {
    DASHBOARD: '/catalog',
    PRODUCTS: '/catalog/products',
    PRODUCT_CREATE: '/catalog/products/create',
    PRODUCT_DETAIL: '/catalog/products/:id',
    TAXA: '/catalog/taxa',
    OPTION_TYPES: '/catalog/option-types',
    OPTION_VALUES: '/catalog/option-values',
  },
  INVENTORY: {
    DASHBOARD: '/inventory',
    STOCK: '/inventory/stock',
    LOCATIONS: '/inventory/locations',
    MOVEMENTS: '/inventory/movements',
    TRANSFERS: '/inventory/transfers',
    UNITS: '/inventory/units',
  },
  LOCATION: {
    COUNTRIES: '/location/countries',
    STATES: '/location/states',
  },
  ORDERING: {
    DASHBOARD: '/ordering',
    ORDERS: '/ordering/orders',
    ORDER_CREATE: '/ordering/orders/create',
    ORDER_DETAIL: '/ordering/orders/:id',
    FULFILLMENT: '/ordering/fulfillment',
  },
  PAYMENT: {
    PAYMENTS: '/payment/payments',
    METHODS: '/payment/methods',
  },
  SHIPPING: {
    METHODS: '/shipping/methods',
    RATES: '/shipping/rates',
  },
  PROFILE: {
    PROFILE: '/profile',
    ADDRESSES: '/profile/addresses',
  },
  USERS: {
    STAFF: '/users/staff',
    STAFF_CREATE: '/users/staff/create',
    CUSTOMERS: '/users/customers',
    ROLES: '/users/roles',
    PERMISSIONS: '/users/permissions',
  },
  REPORTS: {
    DASHBOARD: '/reports',
  },
} as const
```

- [ ] **Step 3: Create `shared/constants/permissions.ts`**

```ts
export const PERMISSIONS = {
  CATALOG: {
    VIEW: 'catalog.view',
    CREATE: 'catalog.create',
    EDIT: 'catalog.edit',
    DELETE: 'catalog.delete',
  },
  INVENTORY: {
    VIEW: 'inventory.view',
    CREATE: 'inventory.create',
    EDIT: 'inventory.edit',
    DELETE: 'inventory.delete',
  },
  ORDERING: {
    VIEW: 'ordering.view',
    CREATE: 'ordering.create',
    EDIT: 'ordering.edit',
    DELETE: 'ordering.delete',
    FULFILL: 'ordering.fulfill',
  },
  USERS: {
    VIEW: 'users.view',
    CREATE: 'users.create',
    EDIT: 'users.edit',
    DELETE: 'users.delete',
    MANAGE_ROLES: 'users.manage_roles',
  },
  SETTINGS: {
    VIEW: 'settings.view',
    EDIT: 'settings.edit',
  },
} as const

export type Permission = (typeof PERMISSIONS)[keyof typeof PERMISSIONS][keyof (typeof PERMISSIONS)[keyof typeof PERMISSIONS]]
```

- [ ] **Step 4: Create `shared/constants/regex.ts`**

```ts
export const REGEX = {
  EMAIL: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
  PHONE: /^\+?[\d\s\-()]{7,15}$/,
  SLUG: /^[a-z0-9]+(?:-[a-z0-9]+)*$/,
  URL: /^https?:\/\/.+/,
  STRONG_PASSWORD: /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/,
} as const
```

- [ ] **Step 5: Create `shared/constants/storage.ts`**

```ts
export const STORAGE_KEYS = {
  ACCESS_TOKEN: 'accessToken',
  REFRESH_TOKEN: 'refreshToken',
  USER: 'currentUser',
  LAYOUT: 'resys-admin-layout',
  THEME: 'resys-admin-theme',
  LOCALE: 'resys-admin-locale',
} as const
```

- [ ] **Step 6: Create `shared/constants/index.ts`**

```ts
export { API_MODULES, CATALOG, IDENTITY, INVENTORY, LOCATION, ORDERING, PAYMENT, PROFILE, SHIPPING, USERS } from './api'
export type { ApiModule } from './api'
export { ROUTES } from './routes'
export { PERMISSIONS } from './permissions'
export type { Permission } from './permissions'
export { REGEX } from './regex'
export { STORAGE_KEYS } from './storage'
```

- [ ] **Step 7: Verify build**

```bash
cd app/Admin && pnpm run build
```

Expected: Build succeeds.

- [ ] **Step 8: Commit**

```bash
git add app/Admin/src/shared/constants/
git commit -m "feat(admin): add shared constants (api paths, routes, permissions, regex, storage)"
```

---

### Task 4: Object transform utilities

**Files:**
- Create: `app/Admin/src/shared/utils/transform.ts`
- Create: `app/Admin/src/shared/utils/index.ts`

**Interfaces:**
- Consumes: Nothing
- Produces: `toCamelCase(str)`, `toCamelCaseKeys(obj)`, `mapKeys(obj, transform)`

- [ ] **Step 1: Create `shared/utils/transform.ts`**

Port from `app/lagacy/Admin/src/common/mapper/mapper.utils.ts`, removing legacy `@/common/` imports:

```ts
export function toCamelCase(str: string): string {
  return str.charAt(0).toLowerCase() + str.slice(1).replace(/_([a-z])/g, (_, c) => c.toUpperCase())
}

export function mapKeys<T extends Record<string, unknown>>(obj: T, transform: (key: string) => string): Record<string, unknown> {
  const result: Record<string, unknown> = {}
  for (const key of Object.keys(obj)) {
    result[transform(key)] = obj[key]
  }
  return result
}

export function toCamelCaseKeys<T extends Record<string, unknown>>(obj: T): Record<string, unknown> {
  const result: Record<string, unknown> = {}
  for (const key of Object.keys(obj)) {
    const newKey = toCamelCase(key)
    const val = obj[key]
    if (val !== null && typeof val === 'object' && !Array.isArray(val)) {
      result[newKey] = toCamelCaseKeys(val as Record<string, unknown>)
    } else if (Array.isArray(val)) {
      result[newKey] = val.map(item =>
        item !== null && typeof item === 'object' && !Array.isArray(item)
          ? toCamelCaseKeys(item as Record<string, unknown>)
          : item
      )
    } else {
      result[newKey] = val
    }
  }
  return result
}
```

- [ ] **Step 2: Create `shared/utils/index.ts`** (initial barrel — will grow in later tasks)

```ts
export { toCamelCase, toCamelCaseKeys, mapKeys } from './transform'
```

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/shared/utils/
git commit -m "feat(admin): add object transform utilities (camelCase conversion)"
```

---

### Task 5: API error handler and token refresh

**Files:**
- Create: `app/Admin/src/shared/api/handlers/error-handler.ts`
- Create: `app/Admin/src/shared/api/handlers/refresh-handler.ts`
- Create: `app/Admin/src/shared/api/handlers/index.ts`

**Interfaces:**
- Consumes: `toCamelCaseKeys` from Task 4, `STORAGE_KEYS` from Task 3, `Error` type from Task 2
- Produces: `parseApiError(error: unknown): ParsedApiError`, `refreshTokens(): Promise<boolean>`, `ParsedApiError` interface

- [ ] **Step 1: Create `shared/api/handlers/error-handler.ts`**

Port from legacy `app/lagacy/Admin/src/common/api/http/handlers/error-handler.ts`. Update imports: `@/common/mapper/mapper.utils` → `@/shared/utils/transform`, `../../types/result.types` → `@/shared/models`:

```ts
import type { Error } from '@/shared/models'
import { toCamelCaseKeys } from '@/shared/utils/transform'

export interface ParsedApiError {
  statusCode: number
  title: string | null
  message: string | null
  detail: string | null
  isSuccess: boolean
  errors: Record<string, string[]>
  errorCode: string | undefined
}

function convertServerErrors(errors: unknown): Record<string, string[]> {
  if (!errors) return {}

  if (Array.isArray(errors)) {
    const se = errors as Error[]
    if (se.length > 0 && se[0]?.code !== undefined) {
      const result: Record<string, string[]> = {}
      for (const err of se) {
        const key = err.code || 'general'
        if (!result[key]) result[key] = []
        result[key].push(err.message)
      }
      return result
    }
    return {}
  }

  return errors as Record<string, string[]>
}

export function parseApiError(error: unknown): ParsedApiError {
  if (!error || typeof error !== 'object') {
    return {
      statusCode: 500,
      title: 'Connection Error',
      message: null,
      detail: 'An unexpected error occurred.',
      isSuccess: false,
      errors: {},
      errorCode: undefined,
    }
  }

  const axiosError = error as {
    isAxiosError?: boolean
    response?: { data?: Record<string, unknown>; status?: number }
    request?: unknown
    message?: string
  }

  if (axiosError.isAxiosError || axiosError.response || axiosError.request) {
    const apiData = axiosError.response?.data

    if (apiData && typeof apiData === 'object') {
      const data = toCamelCaseKeys(apiData as Record<string, unknown>)

      const statusCode = (data.statusCode ?? data.status ?? axiosError.response?.status) as number | undefined
      const message = data.message as string | undefined
      const isSuccess = data.isSuccess as boolean | undefined
      const rawErrors = data.errors

      const title = (data.title ?? message) as string | undefined
      const detail = (data.detail ?? message) as string | undefined
      const errorCode = data.errorCode as string | undefined
      const resolvedCode = statusCode ?? 500

      return {
        statusCode: resolvedCode,
        title: title ?? (resolvedCode >= 500 ? 'Server Error' : 'Request Error'),
        message: message ?? title ?? null,
        detail: detail ?? null,
        isSuccess: isSuccess ?? false,
        errors: convertServerErrors(rawErrors),
        errorCode: errorCode,
      }
    }

    if (axiosError.request && !axiosError.response) {
      return {
        statusCode: 500,
        title: 'Connection Error',
        message: null,
        detail: axiosError.message || 'Network Error. Please check your internet connection.',
        isSuccess: false,
        errors: {},
        errorCode: undefined,
      }
    }
  }

  const e = error as Record<string, unknown>
  if (e.status !== undefined || e.statusCode !== undefined) {
    const rawErrors = e.errors ?? e.Errors
    return {
      statusCode: (e.statusCode ?? e.status ?? 500) as number,
      title: ((e.title ?? e.message) as string | undefined) ?? null,
      message: ((e.message ?? e.title) as string | undefined) ?? null,
      detail: (e.detail as string | undefined) ?? null,
      isSuccess: (e.isSuccess ?? false) as boolean,
      errors: convertServerErrors(rawErrors),
      errorCode: (e.error_code ?? e.errorCode) as string | undefined,
    }
  }

  return {
    statusCode: 500,
    title: null,
    message: null,
    detail: null,
    isSuccess: false,
    errors: {},
    errorCode: undefined,
  }
}
```

- [ ] **Step 2: Create `shared/api/handlers/refresh-handler.ts`**

Port from legacy. Update imports: `../../types/result.types` → `@/shared/models`, use `STORAGE_KEYS`:

```ts
import axios from 'axios'
import type { Result } from '@/shared/models'
import { STORAGE_KEYS } from '@/shared/constants'

export async function refreshTokens(): Promise<boolean> {
  const token = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN)
  if (!token) {
    return false
  }

  try {
    const refreshResponse = await axios.post('/api/store/identity/auth/sessions/refresh', {
      refreshToken: token,
    })

    const body = refreshResponse.data as Record<string, unknown>
    if (body && 'value' in body) {
      const value = (body as unknown as Result<Record<string, unknown>>).value
      const accessToken = value.accessToken as string
      const newRefreshToken = value.refreshToken as string

      localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, accessToken)
      localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, newRefreshToken)

      return true
    }

    return false
  } catch {
    localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN)
    localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN)
    return false
  }
}
```

- [ ] **Step 3: Create `shared/api/handlers/index.ts`**

```ts
export { parseApiError } from './error-handler'
export type { ParsedApiError } from './error-handler'
export { refreshTokens } from './refresh-handler'
```

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/shared/api/handlers/
git commit -m "feat(admin): add API error handler and token refresh logic"
```

---

### Task 6: API client and interceptors

**Files:**
- Create: `app/Admin/src/shared/api/client.ts`
- Create: `app/Admin/src/shared/api/interceptors/auth.interceptor.ts`
- Create: `app/Admin/src/shared/api/interceptors/camelcase.interceptor.ts`
- Create: `app/Admin/src/shared/api/interceptors/error-wrapper.interceptor.ts`
- Create: `app/Admin/src/shared/api/interceptors/index.ts`

**Interfaces:**
- Consumes: `error-handler.ts` (Task 6), `refresh-handler.ts` (Task 6), `toCamelCaseKeys` from Task 4, `Result<T>` and `Error` from Task 2, `STORAGE_KEYS` from Task 3
- Produces: `apiClient` (default export: configured Axios instance), `authInterceptor`, `camelCaseInterceptor`, `errorWrapperInterceptor`

- [ ] **Step 1: Create `shared/api/client.ts`**

Port from `app/lagacy/Admin/src/common/api/http/api.client.ts`, updating imports:

```ts
import axios, { type AxiosInstance } from 'axios'
import { authInterceptor } from './interceptors/auth.interceptor'
import { camelCaseInterceptor } from './interceptors/camelcase.interceptor'
import { errorWrapperInterceptor } from './interceptors/error-wrapper.interceptor'

const apiBaseUrl = import.meta.env.VITE_API_URL
  ? `${import.meta.env.VITE_API_URL}/api`
  : '/api'

const apiClient: AxiosInstance = axios.create({
  baseURL: apiBaseUrl,
  headers: { 'Content-Type': 'application/json' },
  paramsSerializer: { indexes: null },
})

apiClient.interceptors.request.use(authInterceptor)
apiClient.interceptors.response.use(camelCaseInterceptor, errorWrapperInterceptor)

export default apiClient
```

- [ ] **Step 2: Create `shared/api/interceptors/auth.interceptor.ts`**

Port from legacy, updating to use `STORAGE_KEYS`:

```ts
import type { InternalAxiosRequestConfig } from 'axios'
import { STORAGE_KEYS } from '@/shared/constants'

export function authInterceptor(config: InternalAxiosRequestConfig): InternalAxiosRequestConfig {
  const token = localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN)
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
}
```

- [ ] **Step 3: Create `shared/api/interceptors/camelcase.interceptor.ts`**

Port from legacy, updating import path:

```ts
import type { AxiosResponse } from 'axios'
import { toCamelCaseKeys } from '@/shared/utils/transform'

export function camelCaseInterceptor(response: AxiosResponse): AxiosResponse {
  if (response.data && typeof response.data === 'object') {
    response.data = toCamelCaseKeys(response.data as Record<string, unknown>)
  }
  return response
}
```

- [ ] **Step 4: Create `shared/api/interceptors/error-wrapper.interceptor.ts`**

Port from legacy. Key changes: use `STORAGE_KEYS`, change `@/common/` → `@/shared/`, rename `ServerResult<null>` → `Result<null>`:

```ts
import { type AxiosError, type AxiosResponse, type InternalAxiosRequestConfig } from 'axios'
import type { Result } from '@/shared/models'
import { parseApiError } from '../handlers/error-handler'
import { refreshTokens } from '../handlers/refresh-handler'
import apiClient from '../client'
import router from '@/router'
import { STORAGE_KEYS } from '@/shared/constants'

export async function errorWrapperInterceptor(error: AxiosError): Promise<AxiosResponse> {
  const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean }
  const apiError = parseApiError(error)

  if (apiError.statusCode === 401 && originalRequest && !originalRequest._retry) {
    if (originalRequest.url?.includes('/sessions/refresh')) {
      return Promise.resolve({
        data: {
          isSuccess: false,
          statusCode: 401,
          errors: [
            {
              code: 'UNAUTHORIZED',
              message: apiError.detail || 'Unauthorized',
              type: 0,
              metadata: null,
            },
          ],
          message: apiError.title,
          metadata: null,
          value: null,
        } as Result<null>,
      } as AxiosResponse)
    }

    console.warn('Session expired. Attempting to refresh token...')

    originalRequest._retry = true
    const refreshed = await refreshTokens()
    if (refreshed) {
      const newToken = localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN)
      if (newToken && originalRequest.headers) {
        originalRequest.headers.Authorization = `Bearer ${newToken}`
      }
      return apiClient(originalRequest)
    } else {
      router.push('/login')
      return Promise.reject(error)
    }
  }

  return Promise.resolve({
    data: {
      isSuccess: false,
      statusCode: apiError.statusCode,
      errors: [
        {
          code: apiError.errorCode || 'ERROR',
          message: apiError.detail || apiError.title || 'Request failed',
          type: 0,
          metadata: null,
        },
      ],
      message: apiError.title,
      metadata: null,
      value: null,
    } as Result<null>,
  } as AxiosResponse)
}
```

- [ ] **Step 5: Create `shared/api/interceptors/index.ts`**

```ts
export { authInterceptor } from './auth.interceptor'
export { camelCaseInterceptor } from './camelcase.interceptor'
export { errorWrapperInterceptor } from './error-wrapper.interceptor'
```

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/shared/api/client.ts app/Admin/src/shared/api/interceptors/
git commit -m "feat(admin): add API client with auth, camelCase, and error interceptors"
```

---

### Task 7: Result mapper

**Files:**
- Create: `app/Admin/src/shared/api/utils/result.mapper.ts`
- Create: `app/Admin/src/shared/api/utils/api.utils.ts`

**Interfaces:**
- Consumes: `Result<T>`, `PagedResult<T>`, `Error` from Task 2, `PaginationMeta` from Task 2
- Produces: `mapToErrors(errors: Error[]): Record<string, string[]>`, `resultToMapped<T>(result: Result<T>): MappedResult<T>`, `pagedResultToMapped<T>(result: PagedResult<T>): MappedResult<T[]> & { meta?: PaginationMeta }`

- [ ] **Step 1: Create `shared/api/utils/result.mapper.ts`**

Port from legacy, renaming `ServerResult<T>` → `Result<T>`, `ServerPagedResult<T>` → `PagedResult<T>`, `ServerError` → `Error`:

```ts
import type { Result, PagedResult, Error, PaginationMeta } from '@/shared/models'

export interface SuccessResult<T> {
  data: T
  success: true
  meta?: PaginationMeta
}

export interface FailureResult {
  data: null
  success: false
  error: {
    statusCode: number
    title: string | null
    message: string | null
    detail: string | null
    errors: Record<string, string[]>
    errorCode: string | undefined
  }
}

export type MappedResult<T> = SuccessResult<T> | FailureResult

export function isSuccess<T>(result: Result<T> | PagedResult<T>): boolean {
  return result.isSuccess
}

export function isFailure<T>(result: Result<T> | PagedResult<T>): boolean {
  return !result.isSuccess
}

export function mapToErrors(errors: Error[]): Record<string, string[]> {
  const result: Record<string, string[]> = {}
  for (const err of errors) {
    const key = err.code || 'general'
    if (!result[key]) result[key] = []
    result[key].push(err.message)
  }
  return result
}

export function resultToMapped<T>(result: Result<T>): MappedResult<T> {
  if (result.isSuccess) {
    return { data: result.value, success: true as const }
  }
  return {
    data: null,
    success: false as const,
    error: {
      statusCode: result.statusCode,
      title: result.message,
      message: result.message,
      detail: null,
      errors: mapToErrors(result.errors),
      errorCode: result.errors[0]?.code,
    },
  }
}

export function pagedResultToMapped<T>(
  result: PagedResult<T>
): MappedResult<T[]> & { meta?: PaginationMeta } {
  if (result.isSuccess) {
    return {
      data: result.items,
      success: true as const,
      meta: {
        page: result.page,
        pageSize: result.pageSize,
        totalCount: result.totalCount,
        totalPages: Math.ceil(result.totalCount / result.pageSize),
      },
    }
  }
  return {
    data: null,
    success: false as const,
    error: {
      statusCode: result.statusCode,
      title: result.message,
      message: result.message,
      detail: null,
      errors: mapToErrors(result.errors),
      errorCode: result.errors[0]?.code,
    },
  }
}
```

- [ ] **Step 2: Create `shared/api/utils/api.utils.ts`**

Port from legacy (thin re-export file):

```ts
export { parseApiError } from '../handlers/error-handler'
export type { ParsedApiError } from '../handlers/error-handler'
```

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/shared/api/utils/
git commit -m "feat(admin): add result mapper and API utilities"
```

---

### Task 8: Module API factory

**Files:**
- Create: `app/Admin/src/shared/api/services/module-api.factory.ts`

**Interfaces:**
- Consumes: `apiClient` from Task 5, `Result<T>` from Task 2
- Produces: `createModuleApi<T>(config: { basePath: string })` → typed CRUD proxy

- [ ] **Step 1: Create `shared/api/services/module-api.factory.ts`**

Port from legacy, rename `ServerResult<T>` → `Result<T>`:

```ts
import apiClient from '../client'
import type { Result } from '@/shared/models'

export interface ModuleApiConfig {
  basePath: string
}

export function createModuleApi<_T>(config: ModuleApiConfig) {
  return {
    async getSubResource<T>(path: string, params?: Record<string, unknown>): Promise<Result<T>> {
      const res = await apiClient.get(`${config.basePath}/${path}`, { params })
      return res.data as Result<T>
    },

    async postSubResource<T>(path: string, data?: unknown): Promise<Result<T>> {
      const res = await apiClient.post(`${config.basePath}/${path}`, data)
      return res.data as Result<T>
    },

    async putSubResource<T>(path: string, data?: unknown): Promise<Result<T>> {
      const res = await apiClient.put(`${config.basePath}/${path}`, data)
      return res.data as Result<T>
    },

    async deleteSubResource<T>(path: string, params?: Record<string, unknown>): Promise<Result<T>> {
      const res = await apiClient.delete(`${config.basePath}/${path}`, { params })
      return res.data as Result<T>
    },

    async postAction<T>(path: string, data?: unknown): Promise<Result<T>> {
      const res = await apiClient.post(`${config.basePath}/${path}`, data)
      return res.data as Result<T>
    },
  }
}
```

- [ ] **Step 2: Commit**

```bash
git add app/Admin/src/shared/api/services/
git commit -m "feat(admin): add module API factory (typed CRUD proxy)"
```

---

### Task 9: API barrel exports

**Files:**
- Create: `app/Admin/src/shared/api/index.ts`

**Interfaces:**
- Consumes: All API module files from Tasks 5-8
- Produces: Clean barrel exports for the API layer

- [ ] **Step 1: Create `shared/api/index.ts`**

```ts
export { default as apiClient } from './client'
export { createModuleApi } from './services/module-api.factory'
export type { ModuleApiConfig } from './services/module-api.factory'
export { parseApiError, refreshTokens } from './handlers'
export type { ParsedApiError } from './handlers'
export { mapToErrors, resultToMapped, pagedResultToMapped, isSuccess, isFailure } from './utils/result.mapper'
export type { MappedResult, SuccessResult, FailureResult } from './utils/result.mapper'
```

- [ ] **Step 2: Commit**

```bash
git add app/Admin/src/shared/api/index.ts
git commit -m "feat(admin): add API layer barrel exports"
```

---

### Task 10: Auth services

**Files:**
- Create: `app/Admin/src/shared/auth/token.service.ts`
- Create: `app/Admin/src/shared/auth/auth.service.ts`
- Create: `app/Admin/src/shared/auth/permissions.ts`
- Create: `app/Admin/src/shared/auth/roles.ts`
- Create: `app/Admin/src/shared/auth/session.ts`
- Create: `app/Admin/src/shared/auth/index.ts`

**Interfaces:**
- Consumes: `STORAGE_KEYS` from Task 3, `jwt-decode`, `apiClient` from Task 5
- Produces: `TokenService`, `AuthService`, `hasPermission()`, `hasRole()`, `SessionState`

- [ ] **Step 1: Create `shared/auth/token.service.ts`**

```ts
import { jwtDecode } from 'jwt-decode'
import { STORAGE_KEYS } from '@/shared/constants'

interface JwtPayload {
  sub: string
  jti: string
  exp: number
  iat: number
  [key: string]: unknown
}

export class TokenService {
  static getAccessToken(): string | null {
    return localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN)
  }

  static getRefreshToken(): string | null {
    return localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN)
  }

  static setTokens(accessToken: string, refreshToken: string): void {
    localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, accessToken)
    localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, refreshToken)
  }

  static clearTokens(): void {
    localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN)
    localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN)
  }

  static getAccessTokenPayload(): JwtPayload | null {
    const token = this.getAccessToken()
    if (!token) return null
    try {
      return jwtDecode<JwtPayload>(token)
    } catch {
      return null
    }
  }

  static isAccessTokenExpired(): boolean {
    const payload = this.getAccessTokenPayload()
    if (!payload) return true
    const now = Math.floor(Date.now() / 1000)
    return payload.exp < now
  }

  static hasValidAccessToken(): boolean {
    const token = this.getAccessToken()
    if (!token) return false
    return !this.isAccessTokenExpired()
  }
}
```

- [ ] **Step 2: Create `shared/auth/auth.service.ts`**

```ts
import apiClient from '@/shared/api/client'
import type { Result } from '@/shared/models'
import { TokenService } from './token.service'
import { STORAGE_KEYS } from '@/shared/constants'

interface LoginRequest {
  email: string
  password: string
}

interface LoginResponse {
  accessToken: string
  refreshToken: string
}

interface CurrentUser {
  id: string
  email: string
  name: string
  role: string
  permissions: string[]
}

export class AuthService {
  static async login(request: LoginRequest): Promise<Result<LoginResponse>> {
    const response = await apiClient.post<Result<LoginResponse>>(
      '/api/store/identity/auth/sessions/login',
      request
    )
    const result = response.data
    if (result.isSuccess) {
      TokenService.setTokens(result.value.accessToken, result.value.refreshToken)
    }
    return result
  }

  static async logout(): Promise<void> {
    const refreshToken = TokenService.getRefreshToken()
    if (refreshToken) {
      await apiClient.post('/api/store/identity/auth/sessions/logout', {
        refreshToken,
      }).catch(() => {})
    }
    TokenService.clearTokens()
    localStorage.removeItem(STORAGE_KEYS.USER)
  }

  static async getCurrentUser(): Promise<Result<CurrentUser>> {
    const response = await apiClient.get<Result<CurrentUser>>(
      '/api/store/identity/auth/sessions/me'
    )
    return response.data
  }

  static isAuthenticated(): boolean {
    return TokenService.hasValidAccessToken()
  }
}
```

- [ ] **Step 3: Create `shared/auth/permissions.ts`**

```ts
import type { Permission } from '@/shared/constants'

export function hasPermission(required: Permission, userPermissions: string[]): boolean {
  return userPermissions.includes(required) || userPermissions.includes('*')
}

export function hasAnyPermission(required: Permission[], userPermissions: string[]): boolean {
  return required.some(p => hasPermission(p, userPermissions))
}

export function hasAllPermissions(required: Permission[], userPermissions: string[]): boolean {
  return required.every(p => hasPermission(p, userPermissions))
}
```

- [ ] **Step 4: Create `shared/auth/roles.ts`**

```ts
export const ROLES = {
  SUPER_ADMIN: 'SuperAdmin',
  ADMIN: 'Admin',
  MANAGER: 'Manager',
  STAFF: 'Staff',
  VIEWER: 'Viewer',
} as const

export type Role = (typeof ROLES)[keyof typeof ROLES]

export const ROLE_HIERARCHY: Record<Role, number> = {
  [ROLES.SUPER_ADMIN]: 100,
  [ROLES.ADMIN]: 80,
  [ROLES.MANAGER]: 60,
  [ROLES.STAFF]: 40,
  [ROLES.VIEWER]: 20,
}

export function hasRole(userRole: string, requiredRole: Role): boolean {
  const userLevel = ROLE_HIERARCHY[userRole as Role] ?? 0
  const requiredLevel = ROLE_HIERARCHY[requiredRole]
  return userLevel >= requiredLevel
}
```

- [ ] **Step 5: Create `shared/auth/session.ts`**

```ts
import { reactive } from 'vue'

interface CurrentUser {
  id: string
  email: string
  name: string
  role: string
  permissions: string[]
}

interface SessionState {
  user: CurrentUser | null
  isAuthenticated: boolean
  isLoading: boolean
}

export const sessionState = reactive<SessionState>({
  user: null,
  isAuthenticated: false,
  isLoading: true,
})

export function setSessionUser(user: CurrentUser): void {
  sessionState.user = user
  sessionState.isAuthenticated = true
  sessionState.isLoading = false
}

export function clearSession(): void {
  sessionState.user = null
  sessionState.isAuthenticated = false
  sessionState.isLoading = false
}
```

- [ ] **Step 6: Create `shared/auth/index.ts`**

```ts
export { TokenService } from './token.service'
export { AuthService } from './auth.service'
export { hasPermission, hasAnyPermission, hasAllPermissions } from './permissions'
export { ROLES, ROLE_HIERARCHY, hasRole } from './roles'
export type { Role } from './roles'
export { sessionState, setSessionUser, clearSession } from './session'
```

- [ ] **Step 7: Commit**

```bash
git add app/Admin/src/shared/auth/
git commit -m "feat(admin): add auth services (token, login, permissions, roles, session)"
```

---

### Task 11: Error classes

**Files:**
- Create: `app/Admin/src/shared/errors/ApiError.ts`
- Create: `app/Admin/src/shared/errors/ValidationError.ts`
- Create: `app/Admin/src/shared/errors/UnauthorizedError.ts`
- Create: `app/Admin/src/shared/errors/index.ts`

**Interfaces:**
- Consumes: Nothing
- Produces: `ApiError`, `ValidationError`, `UnauthorizedError` classes

- [ ] **Step 1: Create `shared/errors/ApiError.ts`**

Port from legacy:

```ts
export class ApiError extends Error {
  constructor(
    message: string,
    public readonly statusCode: number,
    public readonly errorCode?: string,
    public readonly errors?: Record<string, string[]>,
  ) {
    super(message)
    this.name = 'ApiError'
  }
}
```

- [ ] **Step 2: Create `shared/errors/ValidationError.ts`**

```ts
export class ValidationError extends Error {
  constructor(
    message: string,
    public readonly fieldErrors: Record<string, string[]>,
  ) {
    super(message)
    this.name = 'ValidationError'
  }

  getFieldError(field: string): string | undefined {
    const messages = this.fieldErrors[field]
    return messages?.[0]
  }
}
```

- [ ] **Step 3: Create `shared/errors/UnauthorizedError.ts`**

```ts
export class UnauthorizedError extends Error {
  constructor(message = 'Authentication required') {
    super(message)
    this.name = 'UnauthorizedError'
  }
}
```

- [ ] **Step 4: Create `shared/errors/index.ts`**

```ts
export { ApiError } from './ApiError'
export { ValidationError } from './ValidationError'
export { UnauthorizedError } from './UnauthorizedError'
```

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/shared/errors/
git commit -m "feat(admin): add error classes (ApiError, ValidationError, UnauthorizedError)"
```

---



---

### Task 12: Port Phase 1 tests

**Files:**
- Create: `app/Admin/src/shared/api/__tests__/client.spec.ts`
- Create: `app/Admin/src/shared/api/__tests__/interceptors.spec.ts`
- Create: `app/Admin/src/shared/api/__tests__/result.mapper.spec.ts`
- Create: `app/Admin/src/shared/auth/__tests__/token.service.spec.ts`
- Create: `app/Admin/src/shared/auth/__tests__/permissions.spec.ts`

**Interfaces:**
- Consumes: All Phase 1 source files from Tasks 2-11
- Produces: Test coverage for API client, interceptors, result mapper, auth

- [ ] **Step 1: Port `client.spec.ts` from legacy**

Read `app/lagacy/Admin/src/common/api/http/api.client.spec.ts`. Copy to `app/Admin/src/shared/api/__tests__/client.spec.ts`. Update imports:
- `'./api.client'` → `'../client'`
- `'../utils/api.utils'` → `'../handlers/error-handler'`
- `'../types/result.types'` → `'@/shared/models'`
- Rename `ServerResult<null>` → `Result<null>` in all type assertions

```bash
mkdir -p app/Admin/src/shared/api/__tests__
cp app/lagacy/Admin/src/common/api/http/api.client.spec.ts app/Admin/src/shared/api/__tests__/client.spec.ts
# Then manually update imports and type names as noted above
```

- [ ] **Step 2: Create `interceptors.spec.ts`**

Test that `authInterceptor` attaches Bearer token from localStorage:

```ts
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { authInterceptor } from '../interceptors/auth.interceptor'
import type { InternalAxiosRequestConfig } from 'axios'

describe('authInterceptor', () => {
  beforeEach(() => {
    localStorage.clear()
  })

  it('should attach Authorization header when access token exists', () => {
    localStorage.setItem('accessToken', 'test-token-123')
    const config = { headers: {} } as InternalAxiosRequestConfig
    const result = authInterceptor(config)
    expect(result.headers?.['Authorization' as keyof typeof result.headers]).toBe('Bearer test-token-123')
  })

  it('should not attach Authorization header when no token', () => {
    const config = { headers: {} } as InternalAxiosRequestConfig
    const result = authInterceptor(config)
    expect(result.headers?.['Authorization' as keyof typeof result.headers]).toBeUndefined()
  })
})
```

- [ ] **Step 3: Create `result.mapper.spec.ts`**

Test result mapping functions:

```ts
import { describe, it, expect } from 'vitest'
import { mapToErrors, resultToMapped, pagedResultToMapped, isSuccess, isFailure } from '../utils/result.mapper'
import type { Result, PagedResult, Error } from '@/shared/models'

describe('mapToErrors', () => {
  it('should map Error array to Record<string, string[]>', () => {
    const errors: Error[] = [
      { code: 'Name', message: 'Name is required', type: 0, metadata: null },
      { code: 'Name', message: 'Name too short', type: 0, metadata: null },
      { code: 'Email', message: 'Invalid email', type: 0, metadata: null },
    ]
    const result = mapToErrors(errors)
    expect(result).toEqual({
      Name: ['Name is required', 'Name too short'],
      Email: ['Invalid email'],
    })
  })

  it('should use "general" key for errors without code', () => {
    const errors: Error[] = [
      { code: '', message: 'Something went wrong', type: 0, metadata: null },
    ]
    const result = mapToErrors(errors)
    expect(result).toEqual({ general: ['Something went wrong'] })
  })
})

describe('resultToMapped', () => {
  it('should map success result', () => {
    const result: Result<string> = {
      isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: 'test'
    }
    const mapped = resultToMapped(result)
    expect(mapped.success).toBe(true)
    expect(mapped.data).toBe('test')
  })

  it('should map failure result', () => {
    const result: Result<string> = {
      isSuccess: false,
      statusCode: 400,
      errors: [{ code: 'Name', message: 'Required', type: 0, metadata: null }],
      message: 'Validation failed',
      metadata: null,
      value: '' as string,
    }
    const mapped = resultToMapped(result)
    expect(mapped.success).toBe(false)
    expect(mapped.error).toBeDefined()
  })
})

describe('isSuccess / isFailure', () => {
  it('should return true for success result', () => {
    const r: Result<null> = { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: null }
    expect(isSuccess(r)).toBe(true)
    expect(isFailure(r)).toBe(false)
  })

  it('should return false for failure result', () => {
    const r: Result<null> = { isSuccess: false, statusCode: 400, errors: [], message: null, metadata: null, value: null }
    expect(isSuccess(r)).toBe(false)
    expect(isFailure(r)).toBe(true)
  })
})
```

- [ ] **Step 4: Create `token.service.spec.ts`**

```ts
import { describe, it, expect, beforeEach } from 'vitest'
import { TokenService } from '../token.service'
import { STORAGE_KEYS } from '@/shared/constants'

describe('TokenService', () => {
  beforeEach(() => {
    localStorage.clear()
  })

  it('should store and retrieve access token', () => {
    TokenService.setTokens('access-1', 'refresh-1')
    expect(TokenService.getAccessToken()).toBe('access-1')
    expect(TokenService.getRefreshToken()).toBe('refresh-1')
  })

  it('should clear tokens', () => {
    TokenService.setTokens('access-1', 'refresh-1')
    TokenService.clearTokens()
    expect(TokenService.getAccessToken()).toBeNull()
    expect(TokenService.getRefreshToken()).toBeNull()
  })

  it('should detect expired token', () => {
    expect(TokenService.hasValidAccessToken()).toBe(false)
  })
})
```

- [ ] **Step 5: Create `permissions.spec.ts`**

```ts
import { describe, it, expect } from 'vitest'
import { hasPermission, hasAnyPermission, hasAllPermissions } from '../permissions'

describe('hasPermission', () => {
  it('should match exact permission', () => {
    expect(hasPermission('catalog.view', ['catalog.view', 'catalog.edit'])).toBe(true)
  })

  it('should match wildcard permission', () => {
    expect(hasPermission('catalog.create', ['*'])).toBe(true)
  })

  it('should reject missing permission', () => {
    expect(hasPermission('catalog.delete', ['catalog.view'])).toBe(false)
  })
})

describe('hasAnyPermission', () => {
  it('should return true if any matches', () => {
    expect(hasAnyPermission(['catalog.view', 'catalog.delete'], ['catalog.view'])).toBe(true)
  })
})

describe('hasAllPermissions', () => {
  it('should return true if all match', () => {
    expect(hasAllPermissions(['catalog.view', 'catalog.edit'], ['catalog.view', 'catalog.edit', 'catalog.create'])).toBe(true)
  })

  it('should return false if any missing', () => {
    expect(hasAllPermissions(['catalog.view', 'catalog.delete'], ['catalog.view'])).toBe(false)
  })
})
```

- [ ] **Step 6: Run tests**

```bash
cd app/Admin && pnpm run test:unit
```

Expected: All tests pass.

- [ ] **Step 7: Commit**

```bash
git add app/Admin/src/shared/api/__tests__/ app/Admin/src/shared/auth/__tests__/
git commit -m "test(admin): port Phase 1 tests (client, interceptors, result.mapper, auth)"
```

---

### Task 13: Phase 1 verification


- [ ] **Step 1: Run lint**

```bash
cd app/Admin && pnpm run lint
```

Expected: Zero warnings/errors.

- [ ] **Step 2: Run build**

```bash
cd app/Admin && pnpm run build
```

Expected: Build succeeds.

- [ ] **Step 3: Phase 1 commit** (if any lint fixes were needed)

The Phase 1 gate: Foundation is complete. All types, constants, API client, auth, and errors are in place.

---

## Phase 2: Developer Experience

### Task 14: Toast and formatter composables

**Files:**
- Create: `app/Admin/src/shared/composables/useToast.ts`
- Create: `app/Admin/src/shared/composables/useFormatter.ts`

**Interfaces:**
- Consumes: PrimeVue `useToast`
- Produces: `useToast(): { showToast }`, `useFormatter(): { formatCurrency, formatDate, truncate }`
- Note: Existing `useToastNotify.ts` and `useConfirm.ts` will be merged in later in Task 17

- [ ] **Step 1: Create `shared/composables/useToast.ts`**

Port from legacy `app/lagacy/Admin/src/common/composables/toast.use.ts`:

```ts
import { useToast as usePrimeToast } from 'primevue/usetoast'

export function useToast() {
  const toast = usePrimeToast()
  const showToast = (
    severity: 'success' | 'info' | 'warn' | 'error',
    summary: string,
    detail: string,
    life = 3000,
  ) => {
    toast.add({ severity, summary, detail, life })
  }
  return { showToast }
}
```

- [ ] **Step 2: Create `shared/composables/useFormatter.ts`**

Port from legacy `app/lagacy/Admin/src/common/composables/formatter.use.ts`:

```ts
export function useFormatter() {
  const formatCurrency = (value: number | null | undefined): string => {
    if (value === null || value === undefined) return '$0.00'
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(value)
  }

  const formatDate = (value: string | Date | null | undefined): string => {
    if (!value) return '-'
    const date = typeof value === 'string' ? new Date(value) : value
    return new Intl.DateTimeFormat('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    }).format(date)
  }

  const formatNumber = (value: number | null | undefined, decimals = 0): string => {
    if (value === null || value === undefined) return '-'
    return new Intl.NumberFormat('en-US', {
      minimumFractionDigits: decimals,
      maximumFractionDigits: decimals,
    }).format(value)
  }

  const truncate = (text: string | null | undefined, length: number): string => {
    if (!text) return ''
    if (text.length <= length) return text
    return text.substring(0, length) + '...'
  }

  return {
    formatCurrency,
    formatDate,
    formatNumber,
    truncate,
  }
}
```

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/shared/composables/useToast.ts app/Admin/src/shared/composables/useFormatter.ts
git commit -m "feat(admin): add useToast and useFormatter composables"
```

---

### Task 15: useApi and usePagination composables

**Files:**
- Create: `app/Admin/src/shared/composables/useApi.ts`
- Create: `app/Admin/src/shared/composables/usePagination.ts`
- Create: `app/Admin/src/shared/composables/usePagedList.ts`

**Interfaces:**
- Consumes: `Result<T>`, `PagedResult<T>` from Task 2, `useToast` from Task 13
- Produces: `useApi<T>()`, `usePagination()`, `usePagedList<T>()`

- [ ] **Step 1: Create `shared/composables/useApi.ts`**

New composable wrapping API calls with loading/error/data state:

```ts
import { ref } from 'vue'
import type { Result, Error } from '@/shared/models'

export function useApi<T>() {
  const data = ref<T | null>(null)
  const loading = ref(false)
  const error = ref<Error | null>(null)

  async function execute(apiCall: () => Promise<Result<T>>): Promise<Result<T>> {
    loading.value = true
    error.value = null
    try {
      const result = await apiCall()
      if (result.isSuccess) {
        data.value = result.value
      } else if (result.errors.length > 0) {
        error.value = result.errors[0] ?? null
      }
      return result
    } catch (e) {
      error.value = {
        code: 'UNEXPECTED',
        message: e instanceof Error ? e.message : 'An unexpected error occurred',
        type: 500,
        metadata: null,
      }
      return {
        isSuccess: false,
        statusCode: 500,
        errors: [error.value],
        message: error.value.message,
        metadata: null,
        value: null as T,
      }
    } finally {
      loading.value = false
    }
  }

  return { data, loading, error, execute }
}
```

- [ ] **Step 2: Create `shared/composables/usePagination.ts`**

```ts
import { ref, computed } from 'vue'

export function usePagination(defaultPageSize = 10) {
  const page = ref(1)
  const pageSize = ref(defaultPageSize)
  const totalCount = ref(0)

  const totalPages = computed(() => Math.max(1, Math.ceil(totalCount.value / pageSize.value)))

  const isFirstPage = computed(() => page.value <= 1)
  const isLastPage = computed(() => page.value >= totalPages.value)

  function goToPage(newPage: number) {
    page.value = Math.max(1, Math.min(newPage, totalPages.value))
  }

  function nextPage() {
    if (!isLastPage.value) page.value++
  }

  function prevPage() {
    if (!isFirstPage.value) page.value--
  }

  function reset() {
    page.value = 1
    pageSize.value = defaultPageSize
    totalCount.value = 0
  }

  return {
    page,
    pageSize,
    totalCount,
    totalPages,
    isFirstPage,
    isLastPage,
    goToPage,
    nextPage,
    prevPage,
    reset,
  }
}
```

- [ ] **Step 3: Create `shared/composables/usePagedList.ts`**

Port from legacy `app/lagacy/Admin/src/common/composables/paged-list.use.ts`, renaming types:

```ts
import { ref } from 'vue'
import type { Result, PagedResult, QueryingModel } from '@/shared/models'

type PagedFetchResult<T> = PagedResult<T> | Result<T[]>

function isPaged<T>(r: PagedFetchResult<T>): r is PagedResult<T> {
  return 'items' in r && 'totalCount' in r
}

export function usePagedList<TItem>(fetchFn: (params: QueryingModel) => Promise<PagedFetchResult<TItem>>) {
  const items = ref<TItem[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const totalRecords = ref(0)
  const params = ref<QueryingModel>({
    filter: { conditions: [], allowedFields: [], violations: [] },
    search: { term: { value: '', caseSensitive: false }, fields: [], mode: 'Any', allowedFields: [], violations: [] },
    sort: { clauses: [], allowedFields: [], violations: [] },
    page: { page: 1, pageSize: 10, isEmpty: false, bounds: { defaultPage: 1, defaultPageSize: 10, maxPageSize: 100 }, violations: [] },
  })

  async function fetch() {
    loading.value = true
    error.value = null
    try {
      const result = await fetchFn(params.value)
      if (result.isSuccess) {
        if (isPaged(result)) {
          items.value = result.items
          totalRecords.value = result.totalCount || 0
        } else if (result.value) {
          items.value = result.value
          totalRecords.value = result.value.length || 0
        }
      } else {
        error.value = result.errors?.[0]?.message || 'Failed to fetch'
      }
      return result
    } catch {
      error.value = 'An unexpected error occurred'
    } finally {
      loading.value = false
    }
  }

  function refresh() {
    return fetch()
  }

  return {
    items,
    loading,
    error,
    totalRecords,
    params,
    fetch,
    refresh,
  }
}
```

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/shared/composables/useApi.ts app/Admin/src/shared/composables/usePagination.ts app/Admin/src/shared/composables/usePagedList.ts
git commit -m "feat(admin): add useApi, usePagination, and usePagedList composables"
```

---

### Task 16: useApiErrorHandler and useFilePreview composables

**Files:**
- Create: `app/Admin/src/shared/composables/useApiErrorHandler.ts`
- Create: `app/Admin/src/shared/composables/useFilePreview.ts`

**Interfaces:**
- Consumes: `mapToErrors` from Task 7, `parseApiError` from Task 6, `useToast` from Task 13, `Result<T>` from Task 2
- Produces: `useApiErrorHandler(): { handleFormErrors, handleApiResult }`, `useFilePreview(): { previewUrl, createPreview, revokePreview }`

- [ ] **Step 1: Create `shared/composables/useApiErrorHandler.ts`**

Port from legacy `app/lagacy/Admin/src/common/composables/api-error-handler.use.ts`, updating imports. Rename `ServerResult<T>` → `Result<T>`:

```ts
import type { Result } from '@/shared/models'
import { mapToErrors } from '@/shared/api/utils/result.mapper'
import { parseApiError } from '@/shared/api/handlers/error-handler'
import { useToast } from './useToast'

export function useApiErrorHandler() {
  const { showToast } = useToast()

  const handleFormErrors = (
    error: unknown,
    setErrors: ((errors: Record<string, string | undefined>) => void) | undefined,
    fieldNames: string[],
    locales?: { errorTitle?: string; genericError?: string },
  ) => {
    if (!error) return
    const apiError = parseApiError(error)
    if (import.meta.env.DEV) console.log('[API Trace] Handler received parsed error:', apiError)

    if (apiError.errors && Object.keys(apiError.errors).length > 0) {
      if (import.meta.env.DEV) console.log('[API Trace] Validation error dictionary detected.')
      const formErrors: Record<string, string> = {}
      const unmappedMessages: string[] = []

      Object.entries(apiError.errors).forEach(([key, messages]) => {
        const normalizedKey = key.toLowerCase()
        const messagesArray = messages as string[]

        const field = fieldNames.find((f) => {
          const lowerF = f.toLowerCase()
          return normalizedKey === lowerF || normalizedKey.endsWith(`.${lowerF}`)
        })

        if (field && setErrors) {
          formErrors[field] = messagesArray[0] || 'Invalid value'
        } else {
          unmappedMessages.push(...messagesArray)
        }
      })

      if (setErrors) {
        if (import.meta.env.DEV) console.log('[API Trace] Mapping errors to fields:', formErrors)
        setErrors(formErrors)
      }

      const isGenericDetail = apiError.detail?.toLowerCase()?.includes('one or more validation errors') ?? false
      const toastDetail =
        (isGenericDetail && unmappedMessages.length > 0)
          ? unmappedMessages.join('. ')
          : (apiError.detail || (unmappedMessages.length > 0 ? unmappedMessages.join('. ') : (locales?.genericError || 'Validation Error')))

      const baseTitle = apiError.title || locales?.errorTitle || 'Error'
      const toastTitle = apiError.errorCode ? `${baseTitle} (${apiError.errorCode})` : baseTitle

      showToast('warn', toastTitle, toastDetail)
    } else {
      const severity = apiError.statusCode && apiError.statusCode < 500 ? 'warn' : 'error'
      const baseTitle = apiError.title || locales?.errorTitle || 'Error'
      const toastTitle = apiError.errorCode ? `${baseTitle} (${apiError.errorCode})` : baseTitle
      const toastDetail = apiError.detail || locales?.genericError || 'An unexpected error occurred.'

      if (import.meta.env.DEV) {
        console.log(
          `[API Trace] Showing global toast. Severity: ${severity}, Title: ${toastTitle}, Detail: ${toastDetail}`,
        )
      }

      showToast(severity, toastTitle, toastDetail)
    }
  }

  const handleApiResult = <T>(
    result: Result<T>,
    options?: {
      setErrors?: (errors: Record<string, string | undefined>) => void
      fieldNames?: string[]
      successMessage?: string
      successTitle?: string
      errorTitle?: string
      genericError?: string
    },
  ) => {
    if (result.isSuccess) {
      if (options?.successMessage) {
        showToast('success', options.successTitle || 'Success', options.successMessage)
      }
      return true
    }

    handleFormErrors(
      {
        statusCode: result.statusCode,
        title: result.message,
        message: result.message,
        detail: result.message,
        isSuccess: result.isSuccess,
        errors: mapToErrors(result.errors),
        errorCode: undefined,
      },
      options?.setErrors,
      options?.fieldNames || [],
      { errorTitle: options?.errorTitle, genericError: options?.genericError },
    )
    return false
  }

  return {
    handleFormErrors,
    handleApiResult,
  }
}
```

- [ ] **Step 2: Create `shared/composables/useFilePreview.ts`**

New composable for file/image preview URL management:

```ts
import { ref, onBeforeUnmount } from 'vue'

export function useFilePreview() {
  const previewUrl = ref<string | null>(null)
  let objectUrl: string | null = null

  function createPreview(file: File): string {
    revokePreview()
    objectUrl = URL.createObjectURL(file)
    previewUrl.value = objectUrl
    return objectUrl
  }

  function revokePreview(): void {
    if (objectUrl) {
      URL.revokeObjectURL(objectUrl)
      objectUrl = null
    }
    previewUrl.value = null
  }

  onBeforeUnmount(() => {
    revokePreview()
  })

  return {
    previewUrl,
    createPreview,
    revokePreview,
  }
}
```

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/shared/composables/useApiErrorHandler.ts app/Admin/src/shared/composables/useFilePreview.ts
git commit -m "feat(admin): add useApiErrorHandler and useFilePreview composables"
```

---

### Task 17: Merge existing composables and create barrel

**Files:**
- Modify: `app/Admin/src/shared/composables/useToastNotify.ts` (replace with re-export)
- Modify: `app/Admin/src/shared/composables/useConfirm.ts` (keep, already good)
- Modify: `app/Admin/src/shared/composables/useDebounce.ts` (keep, already good)
- Create: `app/Admin/src/shared/composables/index.ts`

**Interfaces:**
- Consumes: All existing composables (useConfirm, useDebounce, useToastNotify) + new ones from Tasks 13-15
- Produces: Unified barrel

- [ ] **Step 1: Replace `useToastNotify.ts` with a re-export wrapper**

The existing `useToastNotify.ts` has `success`, `error`, `warn`, `info` helpers. The new `useToast.ts` has `showToast(severity, ...)`. Replace `useToastNotify.ts` to delegate to `useToast`:

```ts
import { useToast } from './useToast'

export function useToastNotify() {
  const { showToast } = useToast()

  const success = (detail: string, summary = 'Success') =>
    showToast('success', summary, detail)

  const error = (detail: string, summary = 'Error') =>
    showToast('error', summary, detail, 5000)

  const warn = (detail: string, summary = 'Warning') =>
    showToast('warn', summary, detail, 4000)

  const info = (detail: string, summary = 'Info') =>
    showToast('info', summary, detail)

  return { success, error, warn, info }
}
```

- [ ] **Step 2: Create `shared/composables/index.ts`**

```ts
export { useConfirm } from './useConfirm'
export { useDebounce } from './useDebounce'
export { useToastNotify } from './useToastNotify'
export { useToast } from './useToast'
export { useFormatter } from './useFormatter'
export { useApi } from './useApi'
export { usePagination } from './usePagination'
export { usePagedList } from './usePagedList'
export { useApiErrorHandler } from './useApiErrorHandler'
export { useFilePreview } from './useFilePreview'
```

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/shared/composables/
git commit -m "feat(admin): merge existing composables with new ones, add barrel exports"
```

---

### Task 18: Custom directives

**Files:**
- Create: `app/Admin/src/shared/directives/clickOutside.ts`
- Create: `app/Admin/src/shared/directives/autofocus.ts`
- Create: `app/Admin/src/shared/directives/index.ts`

**Interfaces:**
- Consumes: Vue `Directive` type
- Produces: `v-click-outside`, `v-autofocus` directives, `createDirectivesPlugin()`

- [ ] **Step 1: Create `shared/directives/clickOutside.ts`**

```ts
import type { Directive, DirectiveBinding } from 'vue'

interface ClickOutsideElement extends HTMLElement {
  __clickOutsideHandler?: (event: MouseEvent) => void
}

export const clickOutside: Directive = {
  mounted(el: ClickOutsideElement, binding: DirectiveBinding) {
    const handler = (event: MouseEvent) => {
      if (!(el === event.target || el.contains(event.target as Node))) {
        binding.value(event)
      }
    }
    el.__clickOutsideHandler = handler
    document.addEventListener('click', handler)
  },
  unmounted(el: ClickOutsideElement) {
    if (el.__clickOutsideHandler) {
      document.removeEventListener('click', el.__clickOutsideHandler)
    }
  },
}
```

- [ ] **Step 2: Create `shared/directives/autofocus.ts`**

```ts
import type { Directive } from 'vue'

export const autofocus: Directive = {
  mounted(el: HTMLElement) {
    if (el instanceof HTMLInputElement || el instanceof HTMLTextAreaElement || el instanceof HTMLSelectElement) {
      el.focus()
    } else {
      const input = el.querySelector('input, textarea, select') as HTMLElement | null
      input?.focus()
    }
  },
}
```

- [ ] **Step 3: Create `shared/directives/index.ts`**

```ts
import type { App } from 'vue'
import { clickOutside } from './clickOutside'
import { autofocus } from './autofocus'

export function createDirectivesPlugin() {
  return {
    install(app: App) {
      app.directive('click-outside', clickOutside)
      app.directive('autofocus', autofocus)
    },
  }
}

export { clickOutside, autofocus }
```

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/shared/directives/
git commit -m "feat(admin): add clickOutside and autofocus custom directives"
```

---

### Task 19: Utility functions

**Files:**
- Create: `app/Admin/src/shared/utils/currency.ts`
- Create: `app/Admin/src/shared/utils/enums.ts`
- Create: `app/Admin/src/shared/utils/query-builder.ts`
- Create: `app/Admin/src/shared/utils/status.ts`
- Create: `app/Admin/src/shared/utils/debounce.ts`
- Create: `app/Admin/src/shared/utils/throttle.ts`
- Modify: `app/Admin/src/shared/utils/index.ts`

**Interfaces:**
- Consumes: `QueryingModel` types from Task 2
- Produces: `QueryBuilder<T>`, currency helpers, enum mappers, status helpers, debounce, throttle

- [ ] **Step 1: Create `shared/utils/currency.ts`**

```ts
export function formatCurrency(value: number | null | undefined, currency = 'USD', locale = 'en-US'): string {
  if (value === null || value === undefined) return '$0.00'
  return new Intl.NumberFormat(locale, { style: 'currency', currency }).format(value)
}

export function parseCurrency(value: string): number {
  return parseFloat(value.replace(/[^0-9.-]/g, '')) || 0
}
```

- [ ] **Step 2: Create `shared/utils/enums.ts`**

```ts
export function enumToOptions<T extends Record<string, string>>(
  enumObj: T
): { label: string; value: T[keyof T] }[] {
  return Object.values(enumObj).map((value) => ({
    label: value.replace(/([A-Z])/g, ' $1').trim(),
    value,
  }))
}

export function enumLabel<T extends Record<string, string>>(
  enumObj: T,
  value: T[keyof T] | undefined
): string {
  if (!value) return ''
  return value.replace(/([A-Z])/g, ' $1').trim()
}
```

- [ ] **Step 3: Create `shared/utils/query-builder.ts`**

Port from `app/lagacy/Admin/src/common/utils/query-builder.utils.ts`. Update the DSL tokens to match backend's DSL format (the backend uses DSL tokens like `=`, `!=`, `*>`, `^`, `$` for operators — keep the legacy query builder DSL as-is since it's the frontend's job to emit compatible DSL strings):

```ts
import type { SearchMode } from '@/shared/models'

export type FilterOperator = '=' | '!=' | '>' | '<' | '>=' | '<=' | '!*' | '*' | '^' | '$'

export type NestedKeyOf<T extends object> = {
  [K in keyof T & (string | number)]: T[K] extends object
    ? `${K}` | `${K}.${NestedKeyOf<T[K]>}`
    : `${K}`
}[keyof T & (string | number)]

interface ServerQueryingParams {
  filter?: string
  sort?: string[]
  search?: string
  searchFields?: string[]
  searchMode?: SearchMode
  page?: number
  pageSize?: number
}

export class QueryBuilder<T extends object = Record<string, unknown>> {
  private _filterParts: string[] = []
  private _sorts: string[] = []
  private _searchText?: string
  private _searchFields: string[] = []
  private _searchMode?: SearchMode
  private _page?: number
  private _pageSize?: number
  private _mappings: Map<string, string> = new Map()

  addMap(from: string, to: NestedKeyOf<T> | string): this {
    this._mappings.set(from, to as string)
    return this
  }

  where(field: NestedKeyOf<T> | string, operator: FilterOperator, value: unknown): this {
    if (value === undefined || value === '') return this
    this.appendSeparator()
    const mappedField = this._mappings.get(field as string) || field
    this._filterParts.push(`${mappedField}${operator}${this.formatValue(value)}`)
    return this
  }

  or(): this {
    if (this._filterParts.length > 0) {
      this._filterParts.push('|')
    }
    return this
  }

  startGroup(): this {
    this.appendSeparator()
    this._filterParts.push('(')
    return this
  }

  endGroup(): this {
    this._filterParts.push(')')
    return this
  }

  addRaw(filter: string): this {
    if (filter) {
      this.appendSeparator()
      this._filterParts.push(filter)
    }
    return this
  }

  orderBy(field: NestedKeyOf<T> | string, direction: 'asc' | 'desc' = 'asc'): this {
    const mappedField = this._mappings.get(field as string) || field
    if (direction === 'desc') {
      this._sorts.push(`${mappedField} desc`)
    } else {
      this._sorts.push(mappedField as string)
    }
    return this
  }

  orderByDescending(field: NestedKeyOf<T> | string): this {
    return this.orderBy(field, 'desc')
  }

  search(text: string, fields: (NestedKeyOf<T> | string)[]): this {
    if (!text) return this
    this._searchText = text
    this._searchFields = fields.map((f) => this._mappings.get(f as string) || f) as string[]
    return this
  }

  searchMode(mode: SearchMode): this {
    this._searchMode = mode
    return this
  }

  page(index: number, size: number): this {
    this._page = index
    this._pageSize = size
    return this
  }

  build(): ServerQueryingParams {
    const params: Record<string, unknown> = {}

    if (this._filterParts.length > 0) {
      params.filter = this._filterParts.join('')
    }

    if (this._sorts.length > 0) {
      params.sort = this._sorts.map(s => {
        if (s.endsWith(' desc')) {
          return `-${s.slice(0, -5)}`
        }
        return s
      })
    }

    if (this._searchText) {
      params.search = this._searchText
      if (this._searchFields.length > 0) {
        params.searchFields = this._searchFields
      }
    }

    if (this._page !== undefined) params.page = this._page
    if (this._pageSize !== undefined) params.pageSize = this._pageSize

    if (this._searchMode) {
      params.searchMode = this._searchMode
    }

    return params as ServerQueryingParams
  }

  buildFilterString(): string {
    return this._filterParts.join('')
  }

  private appendSeparator(): void {
    if (this._filterParts.length > 0) {
      const last = this._filterParts[this._filterParts.length - 1]
      if (last !== '(' && last !== '|') {
        this._filterParts.push(',')
      }
    }
  }

  private formatValue(value: unknown): string {
    if (value === null || value === undefined) return 'null'
    if (value instanceof Date) return value.toISOString()
    const str = String(value)
    if (str.includes(',') || str.includes('(') || str.includes(')') || str.includes('|')) {
      return encodeURIComponent(str)
    }
    return str
  }
}
```

- [ ] **Step 4: Create `shared/utils/status.ts`**

Port from legacy:

```ts
import type { StatusDef } from './types'

export const booleanStatusMap: Record<string, StatusDef> = {
  true: { label: 'Active', severity: 'success' },
  false: { label: 'Inactive', severity: 'secondary' },
}
```

- [ ] **Step 5: Create `shared/utils/types.ts`** (supporting types file)

```ts
export interface StatusDef {
  label: string
  severity: string
}
```

- [ ] **Step 6: Create `shared/utils/debounce.ts`**

```ts
export function debounce<T extends (...args: never[]) => void>(fn: T, delayMs = 300): (...args: Parameters<T>) => void {
  let timer: ReturnType<typeof setTimeout> | undefined
  return (...args: Parameters<T>) => {
    if (timer) clearTimeout(timer)
    timer = setTimeout(() => fn(...args), delayMs)
  }
}
```

- [ ] **Step 7: Create `shared/utils/throttle.ts`**

```ts
export function throttle<T extends (...args: never[]) => void>(fn: T, limitMs = 300): (...args: Parameters<T>) => void {
  let inThrottle = false
  return (...args: Parameters<T>) => {
    if (!inThrottle) {
      fn(...args)
      inThrottle = true
      setTimeout(() => {
        inThrottle = false
      }, limitMs)
    }
  }
}
```

- [ ] **Step 8: Update `shared/utils/index.ts`**

Replace the existing content:

```ts
export { toCamelCase, toCamelCaseKeys, mapKeys } from './transform'
export { formatCurrency, parseCurrency } from './currency'
export { enumToOptions, enumLabel } from './enums'
export { QueryBuilder } from './query-builder'
export { booleanStatusMap } from './status'
export { debounce } from './debounce'
export { throttle } from './throttle'
export type { FilterOperator, NestedKeyOf } from './query-builder'
export type { StatusDef } from './types'
```

- [ ] **Step 9: Commit**

```bash
git add app/Admin/src/shared/utils/
git commit -m "feat(admin): add utility functions (currency, enums, query-builder, status, debounce, throttle)"
```

---

### Task 20: Application services

**Files:**
- Create: `app/Admin/src/shared/services/notification.service.ts`
- Create: `app/Admin/src/shared/services/modal.service.ts`
- Create: `app/Admin/src/shared/services/event-bus.service.ts`
- Create: `app/Admin/src/shared/services/logger.service.ts`
- Create: `app/Admin/src/shared/services/index.ts`

**Interfaces:**
- Consumes: Vue `reactive`
- Produces: `NotificationService`, `ModalService`, `EventBusService`, `LoggerService`

- [ ] **Step 1: Create `shared/services/notification.service.ts`**

```ts
import { reactive } from 'vue'

export interface AppNotification {
  id: string
  type: 'info' | 'success' | 'warning' | 'error'
  title: string
  message: string
  timestamp: Date
  read: boolean
}

export const notificationState = reactive({
  notifications: [] as AppNotification[],
})

export function useNotificationService() {
  let nextId = 1

  function addNotification(type: AppNotification['type'], title: string, message: string): void {
    notificationState.notifications.unshift({
      id: `notif-${nextId++}`,
      type,
      title,
      message,
      timestamp: new Date(),
      read: false,
    })
    if (notificationState.notifications.length > 50) {
      notificationState.notifications.pop()
    }
  }

  function markAsRead(id: string): void {
    const notif = notificationState.notifications.find(n => n.id === id)
    if (notif) notif.read = true
  }

  function markAllAsRead(): void {
    notificationState.notifications.forEach(n => (n.read = true))
  }

  function clearAll(): void {
    notificationState.notifications.length = 0
  }

  const unreadCount = computed(() => notificationState.notifications.filter(n => !n.read).length)

  return {
    notificationState,
    addNotification,
    markAsRead,
    markAllAsRead,
    clearAll,
    unreadCount,
  }
}
```

- [ ] **Step 2: Create `shared/services/modal.service.ts`**

```ts
import { ref } from 'vue'

export function useModalService() {
  const isOpen = ref(false)
  const modalData = ref<unknown>(null)

  function open(data?: unknown): void {
    modalData.value = data ?? null
    isOpen.value = true
  }

  function close(): void {
    isOpen.value = false
  }

  function toggle(): void {
    isOpen.value = !isOpen.value
  }

  return { isOpen, modalData, open, close, toggle }
}
```

- [ ] **Step 3: Create `shared/services/event-bus.service.ts`**

```ts
type EventHandler = (...args: unknown[]) => void

class EventBus {
  private handlers: Map<string, Set<EventHandler>> = new Map()

  on(event: string, handler: EventHandler): void {
    if (!this.handlers.has(event)) {
      this.handlers.set(event, new Set())
    }
    this.handlers.get(event)!.add(handler)
  }

  off(event: string, handler: EventHandler): void {
    this.handlers.get(event)?.delete(handler)
  }

  emit(event: string, ...args: unknown[]): void {
    this.handlers.get(event)?.forEach(handler => handler(...args))
  }

  clear(): void {
    this.handlers.clear()
  }
}

export const eventBus = new EventBus()
```

- [ ] **Step 4: Create `shared/services/logger.service.ts`**

```ts
type LogLevel = 'debug' | 'info' | 'warn' | 'error'

const LOG_LEVELS: Record<LogLevel, number> = {
  debug: 0,
  info: 1,
  warn: 2,
  error: 3,
}

class LoggerService {
  private level: LogLevel = import.meta.env.DEV ? 'debug' : 'warn'

  setLevel(level: LogLevel): void {
    this.level = level
  }

  debug(message: string, ...args: unknown[]): void {
    if (LOG_LEVELS[this.level] <= LOG_LEVELS.debug) {
      console.debug(`[DEBUG] ${message}`, ...args)
    }
  }

  info(message: string, ...args: unknown[]): void {
    if (LOG_LEVELS[this.level] <= LOG_LEVELS.info) {
      console.info(`[INFO] ${message}`, ...args)
    }
  }

  warn(message: string, ...args: unknown[]): void {
    if (LOG_LEVELS[this.level] <= LOG_LEVELS.warn) {
      console.warn(`[WARN] ${message}`, ...args)
    }
  }

  error(message: string, ...args: unknown[]): void {
    if (LOG_LEVELS[this.level] <= LOG_LEVELS.error) {
      console.error(`[ERROR] ${message}`, ...args)
    }
  }
}

export const logger = new LoggerService()
```

- [ ] **Step 5: Create `shared/services/index.ts`**

```ts
export { notificationState, useNotificationService } from './notification.service'
export type { AppNotification } from './notification.service'
export { useModalService } from './modal.service'
export { eventBus } from './event-bus.service'
export { logger } from './logger.service'
```

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/shared/services/
git commit -m "feat(admin): add application services (notification, modal, event-bus, logger)"
```

---

### Task 21: Validation infrastructure

**Files:**
- Create: `app/Admin/src/shared/validation/schemas/index.ts`
- Create: `app/Admin/src/shared/validation/rules.ts`
- Create: `app/Admin/src/shared/validation/validators.ts`
- Create: `app/Admin/src/shared/validation/messages.ts`
- Create: `app/Admin/src/shared/validation/index.ts`

**Interfaces:**
- Consumes: `zod`, `REGEX` from Task 3
- Produces: Zod schemas facade, validation rules, vee-validate adapters, error message templates

- [ ] **Step 1: Create `shared/validation/rules.ts`**

```ts
import { REGEX } from '@/shared/constants'

export type ValidationRule = (value: unknown) => true | string

export const rules = {
  required: (label = 'This field') =>
    (value: unknown): true | string =>
      (value !== null && value !== undefined && value !== '') ? true : `${label} is required`,

  minLength: (min: number, label = 'This field') =>
    (value: unknown): true | string =>
      typeof value === 'string' && value.length >= min ? true : `${label} must be at least ${min} characters`,

  maxLength: (max: number, label = 'This field') =>
    (value: unknown): true | string =>
      typeof value === 'string' && value.length <= max ? true : `${label} must not exceed ${max} characters`,

  email: (label = 'Email') =>
    (value: unknown): true | string =>
      typeof value === 'string' && REGEX.EMAIL.test(value) ? true : `${label} is not valid`,

  min: (min: number, label = 'This field') =>
    (value: unknown): true | string =>
      typeof value === 'number' && value >= min ? true : `${label} must be at least ${min}`,

  max: (max: number, label = 'This field') =>
    (value: unknown): true | string =>
      typeof value === 'number' && value <= max ? true : `${label} must not exceed ${max}`,
}
```

- [ ] **Step 2: Create `shared/validation/validators.ts`**

```ts
import { z } from 'zod'

export const baseField = z.object({
  id: z.string().uuid().optional(),
})

export const namedField = baseField.extend({
  name: z.string().min(1, 'Name is required').max(256),
})

export const activatedField = baseField.extend({
  isActive: z.boolean().default(true),
})

export const seoField = z.object({
  metaTitle: z.string().max(256).optional(),
  metaDescription: z.string().max(1024).optional(),
  metaKeywords: z.string().max(512).optional(),
})

export const sortableField = z.object({
  position: z.number().int().min(0).default(0),
})

export const moneyField = z.object({
  amount: z.number().min(0, 'Amount must be non-negative'),
  currency: z.string().length(3).default('USD'),
})

export type BaseField = z.infer<typeof baseField>
export type NamedField = z.infer<typeof namedField>
export type ActivatedField = z.infer<typeof activatedField>
export type SeoField = z.infer<typeof seoField>
export type SortableField = z.infer<typeof sortableField>
export type MoneyField = z.infer<typeof moneyField>
```

- [ ] **Step 3: Create `shared/validation/messages.ts`**

```ts
export const validationMessages: Record<string, string> = {
  required: '{field} is required.',
  email: 'Please enter a valid email address.',
  minLength: '{field} must be at least {min} characters.',
  maxLength: '{field} must not exceed {max} characters.',
  min: '{field} must be at least {min}.',
  max: '{field} must not exceed {max}.',
  url: 'Please enter a valid URL.',
  pattern: '{field} format is invalid.',
  integer: '{field} must be a whole number.',
  positive: '{field} must be a positive number.',
}

export function formatMessage(template: string, replacements: Record<string, string | number>): string {
  let result = template
  for (const [key, value] of Object.entries(replacements)) {
    result = result.replace(`{${key}}`, String(value))
  }
  return result
}
```

- [ ] **Step 4: Create `shared/validation/schemas/index.ts`** (empty placeholder — domain schemas added as features are built)

```ts
export {}
```

- [ ] **Step 5: Create `shared/validation/index.ts`**

```ts
export { rules } from './rules'
export type { ValidationRule } from './rules'
export {
  baseField,
  namedField,
  activatedField,
  seoField,
  sortableField,
  moneyField,
} from './validators'
export type {
  BaseField,
  NamedField,
  ActivatedField,
  SeoField,
  SortableField,
  MoneyField,
} from './validators'
export { validationMessages, formatMessage } from './messages'
```

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/shared/validation/
git commit -m "feat(admin): add validation infrastructure (Zod schemas, rules, messages)"
```

---

### Task 22: Hooks, Enums, and auxiliary types

**Files:**
- Create: `app/Admin/src/shared/hooks/beforeMount.ts`
- Create: `app/Admin/src/shared/hooks/beforeRoute.ts`
- Create: `app/Admin/src/shared/hooks/index.ts`
- Create: `app/Admin/src/shared/enums/status.enum.ts`
- Create: `app/Admin/src/shared/enums/role.enum.ts`
- Create: `app/Admin/src/shared/enums/theme.enum.ts`
- Create: `app/Admin/src/shared/types/ui.ts`
- Create: `app/Admin/src/shared/types/forms.ts`
- Create: `app/Admin/src/shared/types/global.ts`

**Interfaces:**
- Consumes: Vue lifecycle and router composables
- Produces: Hooks, enums, utility types

- [ ] **Step 1: Create `shared/hooks/beforeMount.ts`**

```ts
import { onBeforeMount } from 'vue'

export function useBeforeMount(fn: () => void | Promise<void>): void {
  onBeforeMount(() => {
    void Promise.resolve(fn())
  })
}
```

- [ ] **Step 2: Create `shared/hooks/beforeRoute.ts`**

```ts
import { onBeforeRouteLeave, onBeforeRouteUpdate } from 'vue-router'
import type { NavigationGuard } from 'vue-router'

export function useBeforeRoute(guard: NavigationGuard): void {
  onBeforeRouteLeave(guard)
  onBeforeRouteUpdate(guard as NavigationGuard)
}
```

- [ ] **Step 3: Create `shared/hooks/index.ts`**

```ts
export { useBeforeMount } from './beforeMount'
export { useBeforeRoute } from './beforeRoute'
```

- [ ] **Step 4: Create `shared/enums/status.enum.ts`**

```ts
export const Status = {
  ACTIVE: 'Active',
  INACTIVE: 'Inactive',
  ARCHIVED: 'Archived',
  DRAFT: 'Draft',
} as const

export type Status = (typeof Status)[keyof typeof Status]
```

- [ ] **Step 5: Create `shared/enums/role.enum.ts`**

```ts
export const Role = {
  SUPER_ADMIN: 'SuperAdmin',
  ADMIN: 'Admin',
  MANAGER: 'Manager',
  STAFF: 'Staff',
  VIEWER: 'Viewer',
} as const

export type Role = (typeof Role)[keyof typeof Role]
```

- [ ] **Step 6: Create `shared/enums/theme.enum.ts`**

```ts
export const Theme = {
  LIGHT: 'light',
  DARK: 'dark',
  SYSTEM: 'system',
} as const

export type Theme = (typeof Theme)[keyof typeof Theme]
```

- [ ] **Step 7: Create `shared/types/ui.ts`**

```ts
export type Size = 'small' | 'medium' | 'large'
export type Severity = 'success' | 'info' | 'warn' | 'error' | 'secondary' | 'contrast'
export type Position = 'top' | 'bottom' | 'left' | 'right' | 'center'
export type Alignment = 'start' | 'center' | 'end'
```

- [ ] **Step 8: Create `shared/types/forms.ts`**

```ts
export interface FormField<T = string> {
  value: T
  error: string | null
  touched: boolean
  dirty: boolean
  disabled: boolean
}

export interface FormState {
  isSubmitting: boolean
  isValid: boolean
  isDirty: boolean
}
```

- [ ] **Step 9: Create `shared/types/global.ts`**

```ts
export type DeepPartial<T> = T extends object
  ? { [P in keyof T]?: DeepPartial<T[P]> }
  : T

export type Nullable<T> = T | null

export type Optional<T, K extends keyof T> = Omit<T, K> & Partial<Pick<T, K>>

export type RequireAtLeastOne<T, Keys extends keyof T = keyof T> =
  Pick<T, Exclude<keyof T, Keys>> & { [K in Keys]-?: Required<Pick<T, K>> & Partial<Pick<T, Exclude<Keys, K>>> }[Keys]

export type NonEmptyArray<T> = [T, ...T[]]
```

- [ ] **Step 10: Commit**

```bash
git add app/Admin/src/shared/hooks/ app/Admin/src/shared/enums/ app/Admin/src/shared/types/
git commit -m "feat(admin): add hooks, enums, and auxiliary types"
```

---

---

### Task 23: Port Phase 2 tests

**Files:**
- Create: `app/Admin/src/shared/utils/__tests__/query-builder.spec.ts`
- Create: `app/Admin/src/shared/composables/__tests__/formatter.spec.ts`
- Create: `app/Admin/src/shared/composables/__tests__/paged-list.spec.ts`
- Create: `app/Admin/src/shared/composables/__tests__/api-error-handler.spec.ts`

**Interfaces:**
- Consumes: All Phase 2 source files from Tasks 13-21
- Produces: Test coverage for query-builder, formatter, paged-list, api-error-handler

- [ ] **Step 1: Port `query-builder.spec.ts` from legacy**

Read `app/lagacy/Admin/src/common/utils/query-builder.utils.spec.ts`. Copy to `app/Admin/src/shared/utils/__tests__/query-builder.spec.ts`. Update imports:
- `'./query-builder.utils'` → `'../query-builder'`
- Legacy uses `ServerQueryingParameters` — keep the test DSL unchanged (the DSL is the same)

```bash
mkdir -p app/Admin/src/shared/utils/__tests__
cp app/lagacy/Admin/src/common/utils/query-builder.utils.spec.ts app/Admin/src/shared/utils/__tests__/query-builder.spec.ts
# Update import path only
```

- [ ] **Step 2: Create `formatter.spec.ts`**

```ts
import { describe, it, expect } from 'vitest'
import { useFormatter } from '../useFormatter'

describe('useFormatter', () => {
  const { formatCurrency, formatDate, formatNumber, truncate } = useFormatter()

  it('should format currency values', () => {
    expect(formatCurrency(1234.56)).toBe('$1,234.56')
    expect(formatCurrency(0)).toBe('$0.00')
    expect(formatCurrency(null)).toBe('$0.00')
    expect(formatCurrency(undefined)).toBe('$0.00')
  })

  it('should format dates', () => {
    const date = new Date('2025-06-15T10:30:00Z')
    const result = formatDate(date)
    expect(result).toContain('2025')
    expect(result).toContain('Jun')
  })

  it('should return dash for null/undefined dates', () => {
    expect(formatDate(null)).toBe('-')
    expect(formatDate(undefined)).toBe('-')
  })

  it('should format numbers with decimals', () => {
    expect(formatNumber(1234.567, 2)).toBe('1,234.57')
    expect(formatNumber(null)).toBe('-')
  })

  it('should truncate long text', () => {
    expect(truncate('hello world', 5)).toBe('hello...')
    expect(truncate('hi', 5)).toBe('hi')
    expect(truncate(null, 5)).toBe('')
  })
})
```

- [ ] **Step 3: Create `paged-list.spec.ts`**

```ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { usePagedList } from '../usePagedList'
import type { PagedResult } from '@/shared/models'

describe('usePagedList', () => {
  const mockFetch = vi.fn()

  beforeEach(() => {
    mockFetch.mockReset()
  })

  it('should fetch and populate items on success', async () => {
    const items = [{ id: 1, name: 'A' }]
    mockFetch.mockResolvedValueOnce({
      isSuccess: true,
      statusCode: 200,
      errors: [],
      message: null,
      metadata: null,
      items,
      page: 1,
      pageSize: 10,
      totalCount: 1,
    } as PagedResult<{ id: number; name: string }>)

    const { items: result, fetch } = usePagedList(mockFetch)
    await fetch()

    expect(result.value).toEqual(items)
  })

  it('should set error on failure', async () => {
    mockFetch.mockResolvedValueOnce({
      isSuccess: false,
      statusCode: 500,
      errors: [{ code: 'ERR', message: 'Server error', type: 500, metadata: null }],
      message: null,
      metadata: null,
      items: [],
      page: 1,
      pageSize: 10,
      totalCount: 0,
    } as PagedResult<unknown>)

    const { error, fetch } = usePagedList(mockFetch)
    await fetch()

    expect(error.value).toBe('Server error')
  })
})
```

- [ ] **Step 4: Create `api-error-handler.spec.ts`**

```ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useApiErrorHandler } from '../useApiErrorHandler'

vi.mock('primevue/usetoast', () => ({
  useToast: () => ({
    add: vi.fn(),
  }),
}))

describe('useApiErrorHandler', () => {
  it('should handle successful API result', () => {
    const { handleApiResult } = useApiErrorHandler()
    const result = {
      isSuccess: true,
      statusCode: 200,
      errors: [],
      message: null,
      metadata: null,
      value: { id: 1 },
    }
    expect(handleApiResult(result)).toBe(true)
  })

  it('should handle failed API result', () => {
    const { handleApiResult } = useApiErrorHandler()
    const result = {
      isSuccess: false,
      statusCode: 400,
      errors: [{ code: 'Name', message: 'Required', type: 0, metadata: null }],
      message: 'Validation failed',
      metadata: null,
      value: null,
    }
    expect(handleApiResult(result, { fieldNames: ['name'] })).toBe(false)
  })
})
```

- [ ] **Step 5: Run tests**

```bash
cd app/Admin && pnpm run test:unit
```

Expected: All tests pass.

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/shared/utils/__tests__/ app/Admin/src/shared/composables/__tests__/
git commit -m "test(admin): port Phase 2 tests (query-builder, formatter, paged-list, api-error-handler)"
```

---

### Task 24: Phase 2 verification

- [ ] **Step 1: Run lint**

```bash
cd app/Admin && pnpm run lint
```

Expected: Zero warnings/errors. Fix any issues.

- [ ] **Step 2: Run build**

```bash
cd app/Admin && pnpm run build
```

Expected: Build succeeds.

- [ ] **Step 3: Commit any lint fixes**

The Phase 2 gate: All developer experience infrastructure is in place. Composables, directives, utils, services, validation, hooks, enums, and types are complete.

---

## Phase 3: Polish

### Task 25: i18n Vue plugin setup

**Files:**
- Create: `app/Admin/src/shared/localization/index.ts`

**Interfaces:**
- Consumes: `vue-i18n`, locale JSON files from Task 24
- Produces: `createI18nPlugin()` factory returning a Vue plugin

- [ ] **Step 1: Create `shared/localization/index.ts`**

```ts
import { createI18n } from 'vue-i18n'
import type { App } from 'vue'
import en from './messages/en/general.json'

export function createI18nPlugin() {
  const i18n = createI18n({
    legacy: false,
    locale: 'en',
    fallbackLocale: 'en',
    messages: { en },
    missingWarn: import.meta.env.DEV,
    fallbackWarn: import.meta.env.DEV,
  })

  return {
    install(app: App) {
      app.use(i18n)
    },
  }
}
```

- [ ] **Step 2: Commit**

```bash
git add app/Admin/src/shared/localization/index.ts
git commit -m "feat(admin): add i18n plugin setup (vue-i18n)"
```

---

### Task 26: i18n locale files (13 JSON files)

**Files:**
- Create: `app/Admin/src/shared/localization/messages/en/general.json`
- Create: `app/Admin/src/shared/localization/messages/en/auth.json`
- Create: `app/Admin/src/shared/localization/messages/en/catalog.json`
- Create: `app/Admin/src/shared/localization/messages/en/inventory.json`
- Create: `app/Admin/src/shared/localization/messages/en/ordering.json`
- Create: `app/Admin/src/shared/localization/messages/en/payment.json`
- Create: `app/Admin/src/shared/localization/messages/en/shipping.json`
- Create: `app/Admin/src/shared/localization/messages/en/location.json`
- Create: `app/Admin/src/shared/localization/messages/en/profile.json`
- Create: `app/Admin/src/shared/localization/messages/en/users.json`
- Create: `app/Admin/src/shared/localization/messages/en/roles.json`
- Create: `app/Admin/src/shared/localization/messages/en/error.json`
- Create: `app/Admin/src/shared/localization/messages/en/reports.json`

**Interfaces:**
- Consumes: Legacy locale files from `app/lagacy/Admin/src/shared/locales/messages/en/` (copy/adapt)
- Produces: All 13 en locale JSON files loadable by vue-i18n

**Note:** Port the actual JSON content from the legacy locale files. Read each file from `app/lagacy/Admin/src/shared/locales/messages/en/<domain>.json` and copy it to the corresponding path in the new app. Adjust any key changes if needed but the structure should be preserved as-is.

- [ ] **Step 1: Copy each locale file from legacy**

For each of the 13 files, read the source from `app/lagacy/Admin/src/shared/locales/messages/en/<name>.json` and write to `app/Admin/src/shared/localization/messages/en/<name>.json`. Add `"$schema": "../locale-schema.json"` to each if none exists (optional).

Run these copy commands:

```bash
SRC=app/lagacy/Admin/src/shared/locales/messages/en
DST=app/Admin/src/shared/localization/messages/en
mkdir -p "$DST"
for name in general auth catalog inventory ordering payment shipping location profile users roles error reports; do
  cp "$SRC/$name.json" "$DST/$name.json" 2>/dev/null || echo "WARNING: $name.json not found in legacy, creating stub"
done
```

- [ ] **Step 2: Create stubs for any missing locale files**

For each file not found in the legacy, create a minimal JSON stub:

```json
{
  "title": "<Domain>"
}
```

- [ ] **Step 3: Verify TypeScript build**

```bash
cd app/Admin && pnpm run build
```

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/shared/localization/messages/
git commit -m "feat(admin): port 13 i18n locale files from legacy admin"
```

---

### Task 27: SCSS merge

**Files:**
- Create: `app/Admin/src/shared/styles/variables.scss`
- Create: `app/Admin/src/shared/styles/mixins.scss`
- Create: `app/Admin/src/shared/styles/typography.scss`
- Create: `app/Admin/src/shared/styles/animations.scss`

**Interfaces:**
- Consumes: Legacy SCSS from `app/lagacy/Admin/src/assets/scss/` (if any custom variables/mixins beyond PrimeVue presets)
- Produces: SCSS partials that can be `@use`d in `main.scss`

**Note:** The new admin already has a token system at `assets/styles/tokens/`. Audit the legacy SCSS and only port variables/mixins that don't already exist in the PrimeVue preset or Tailwind config.

- [ ] **Step 1: Audit legacy SCSS**

```bash
ls app/lagacy/Admin/src/assets/scss/ 2>/dev/null
```

If no legacy `assets/scss/` exists beyond `tailwind.css`, create minimal stubs.

- [ ] **Step 2: Create `shared/styles/variables.scss`**

Only add variables not covered by the PrimeVue preset tokens:

```scss
// Shared SCSS variables — supplement tokens/_colors.scss and tokens/_typography.scss
// Only add variables not already defined by PrimeVue Aura preset or Tailwind config.
```

If legacy has additional custom variables, port them here. Otherwise leave as a comment-only file.

- [ ] **Step 3: Create `shared/styles/mixins.scss`**

```scss
// Shared SCSS mixins — supplement tokens/_mixins.scss
@mixin visually-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

@mixin text-ellipsis {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

@mixin flex-center {
  display: flex;
  align-items: center;
  justify-content: center;
}
```

- [ ] **Step 4: Create `shared/styles/typography.scss`**

```scss
// Font face declarations — add custom web fonts here
```

- [ ] **Step 5: Create `shared/styles/animations.scss`**

```scss
@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes fadeInUp {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

@keyframes slideInRight {
  from { transform: translateX(100%); }
  to { transform: translateX(0); }
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.animate-fade-in { animation: fadeIn 0.2s ease-out; }
.animate-fade-in-up { animation: fadeInUp 0.3s ease-out; }
.animate-slide-in-right { animation: slideInRight 0.3s ease-out; }
.animate-spin { animation: spin 1s linear infinite; }
```

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/shared/styles/
git commit -m "feat(admin): add shared SCSS (variables, mixins, typography, animations)"
```

---

### Task 28: Asset directories

**Files:**
- Create: `app/Admin/src/shared/assets/icons/.gitkeep`
- Create: `app/Admin/src/shared/assets/images/.gitkeep`
- Create: `app/Admin/src/shared/assets/fonts/.gitkeep`
- Create: `app/Admin/src/shared/assets/svg/.gitkeep`

**Note:** Port actual asset files (icons, images, fonts, SVGs) from `app/lagacy/Admin/src/assets/` if they exist. If the legacy has no custom assets beyond what PrimeIcons provides, just create the `.gitkeep` placeholders.

- [ ] **Step 1: Create directories with .gitkeep**

```bash
mkdir -p app/Admin/src/shared/assets/{icons,images,fonts,svg}
touch app/Admin/src/shared/assets/icons/.gitkeep
touch app/Admin/src/shared/assets/images/.gitkeep
touch app/Admin/src/shared/assets/fonts/.gitkeep
touch app/Admin/src/shared/assets/svg/.gitkeep
```

- [ ] **Step 2: Copy any existing legacy assets**

```bash
# Copy icons if any exist
cp -n app/lagacy/Admin/src/assets/icons/* app/Admin/src/shared/assets/icons/ 2>/dev/null || true
# Copy images
cp -n app/lagacy/Admin/src/assets/images/* app/Admin/src/shared/assets/images/ 2>/dev/null || true
# Copy fonts
cp -n app/lagacy/Admin/src/assets/fonts/* app/Admin/src/shared/assets/fonts/ 2>/dev/null || true
```

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/shared/assets/
git commit -m "feat(admin): add shared asset directories and port legacy assets"
```

---

### Task 29: Utility composables (useDate, useCurrency, useWindowSize, useResponsive)

**Files:**
- Create: `app/Admin/src/shared/composables/useDate.ts`
- Create: `app/Admin/src/shared/composables/useCurrency.ts`
- Create: `app/Admin/src/shared/composables/useWindowSize.ts`
- Create: `app/Admin/src/shared/composables/useResponsive.ts`
- Modify: `app/Admin/src/shared/composables/index.ts`

**Interfaces:**
- Consumes: Vue `ref`, `onMounted`, `onBeforeUnmount`
- Produces: `useDate()`, `useCurrency()`, `useWindowSize()`, `useResponsive()`

- [ ] **Step 1: Create `shared/composables/useDate.ts`**

```ts
import { ref } from 'vue'

export function useDate() {
  const locale = ref('en-US')

  function format(value: string | Date | null | undefined, options?: Intl.DateTimeFormatOptions): string {
    if (!value) return '-'
    const date = typeof value === 'string' ? new Date(value) : value
    return new Intl.DateTimeFormat(locale.value, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      ...options,
    }).format(date)
  }

  function formatRelative(value: string | Date): string {
    const date = typeof value === 'string' ? new Date(value) : value
    const now = new Date()
    const diffMs = date.getTime() - now.getTime()
    const diffDays = Math.round(diffMs / (1000 * 60 * 60 * 24))
    const rtf = new Intl.RelativeTimeFormat(locale.value, { numeric: 'auto' })
    if (Math.abs(diffDays) < 1) {
      const diffHours = Math.round(diffMs / (1000 * 60 * 60))
      if (Math.abs(diffHours) < 1) {
        const diffMinutes = Math.round(diffMs / (1000 * 60))
        return rtf.format(diffMinutes, 'minute')
      }
      return rtf.format(diffHours, 'hour')
    }
    if (Math.abs(diffDays) < 30) return rtf.format(diffDays, 'day')
    const diffMonths = Math.round(diffDays / 30)
    return rtf.format(diffMonths, 'month')
  }

  return { locale, format, formatRelative }
}
```

- [ ] **Step 2: Create `shared/composables/useCurrency.ts`** (delegates to `useFormatter`)

```ts
import { ref } from 'vue'

export function useCurrency() {
  const currency = ref('USD')
  const locale = ref('en-US')

  function format(value: number | null | undefined): string {
    if (value === null || value === undefined) return '$0.00'
    return new Intl.NumberFormat(locale.value, {
      style: 'currency',
      currency: currency.value,
    }).format(value)
  }

  return { currency, locale, format }
}
```

- [ ] **Step 3: Create `shared/composables/useWindowSize.ts`**

```ts
import { ref, onMounted, onBeforeUnmount } from 'vue'

export function useWindowSize() {
  const width = ref(window.innerWidth)
  const height = ref(window.innerHeight)

  function onResize() {
    width.value = window.innerWidth
    height.value = window.innerHeight
  }

  onMounted(() => window.addEventListener('resize', onResize))
  onBeforeUnmount(() => window.removeEventListener('resize', onResize))

  return { width, height }
}
```

- [ ] **Step 4: Create `shared/composables/useResponsive.ts`**

```ts
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'

const breakpoints = {
  sm: 640,
  md: 768,
  lg: 1024,
  xl: 1280,
  xxl: 1536,
} as const

export function useResponsive() {
  const width = ref(window.innerWidth)

  const isMobile = computed(() => width.value < breakpoints.md)
  const isTablet = computed(() => width.value >= breakpoints.md && width.value < breakpoints.lg)
  const isDesktop = computed(() => width.value >= breakpoints.lg)
  const isWide = computed(() => width.value >= breakpoints.xl)

  function onResize() {
    width.value = window.innerWidth
  }

  onMounted(() => window.addEventListener('resize', onResize))
  onBeforeUnmount(() => window.removeEventListener('resize', onResize))

  return { width, breakpoints, isMobile, isTablet, isDesktop, isWide }
}
```

- [ ] **Step 5: Update `shared/composables/index.ts`**

Append to the existing barrel:

```ts
export { useDate } from './useDate'
export { useCurrency } from './useCurrency'
export { useWindowSize } from './useWindowSize'
export { useResponsive } from './useResponsive'
```

(Make sure the `useCurrency` export doesn't conflict with the `formatCurrency` utility from `utils/currency.ts` — different names, different modules.)

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/shared/composables/
git commit -m "feat(admin): add useDate, useCurrency, useWindowSize, useResponsive composables"
```

---

### Task 30: Dark mode refactor

**Files:**
- Create: `app/Admin/src/shared/composables/useDarkMode.ts`
- Modify: `app/Admin/src/app/composables/layout.composable.ts`
- Modify: `app/Admin/src/shared/composables/index.ts`

**Interfaces:**
- Consumes: Existing `useLayout()` from `layout.composable.ts`
- Produces: `useDarkMode()` standalone composable extracted from layout

- [ ] **Step 1: Create `shared/composables/useDarkMode.ts`**

Extract dark mode logic from `layout.composable.ts`:

```ts
import { ref, watchEffect } from 'vue'

const DARK_MODE_CLASS = 'app-dark'
const DARK_MODE_STORAGE_KEY = 'resys-admin-dark-mode'

export function useDarkMode() {
  const stored = localStorage.getItem(DARK_MODE_STORAGE_KEY)
  const isDark = ref(stored === 'true')

  watchEffect(() => {
    localStorage.setItem(DARK_MODE_STORAGE_KEY, String(isDark.value))
    document.documentElement.classList.toggle(DARK_MODE_CLASS, isDark.value)
  })

  function toggle() {
    isDark.value = !isDark.value
  }

  function enable() {
    isDark.value = true
  }

  function disable() {
    isDark.value = false
  }

  return { isDark, toggle, enable, disable }
}
```

- [ ] **Step 2: Update `layout.composable.ts` to use `useDarkMode`**

Modify `app/Admin/src/app/composables/layout.composable.ts`. Replace the inline `isDarkTheme` and `toggleDarkMode` with `useDarkMode`:

In the file, find:
```ts
const layoutConfig = reactive<LayoutConfig>({
  preset: saved.preset || 'Aura',
  primary: saved.primary || 'emerald',
  surface: (saved.surface as string | null) || null,
  darkTheme: saved.darkTheme ?? false,
  menuMode: saved.menuMode || 'static',
})
```

Keep `layoutConfig.darkTheme` but delegate the DOM manipulation to `useDarkMode`. Add at the top of the `useLayout` function:

```ts
import { useDarkMode } from '@/shared/composables/useDarkMode'
```

And inside `useLayout()`:
```ts
const { isDark: isDarkTheme, toggle: toggleDarkMode } = useDarkMode()

// Sync layoutConfig.darkTheme with useDarkMode
watch(isDarkTheme, (val) => {
  layoutConfig.darkTheme = val
})
```

Remove the old `executeDarkModeToggle` and `toggleDarkMode` implementations from `useLayout()`. Keep the `isDarkTheme` computed and `toggleDarkMode` wrapper for backward compatibility:

```ts
const isDarkThemeComputed = computed(() => layoutConfig.darkTheme)

function toggleDarkMode() {
  layoutConfig.darkTheme = !layoutConfig.darkTheme
  if (layoutConfig.darkTheme) {
    document.documentElement.classList.add('app-dark')
  } else {
    document.documentElement.classList.remove('app-dark')
  }
}
```

**Note:** This is an incremental refactor — the layout composable still owns dark mode state in `layoutConfig` but delegates the DOM class toggle. A full extraction can follow in a future pass.

- [ ] **Step 3: Update `shared/composables/index.ts`**

Add `useDarkMode` to the barrel:

```ts
export { useDarkMode } from './useDarkMode'
```

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/shared/composables/useDarkMode.ts app/Admin/src/shared/composables/index.ts app/Admin/src/app/composables/layout.composable.ts
git commit -m "refactor(admin): extract useDarkMode composable from layout"
```

---

### Task 31: Main.ts wiring

**Files:**
- Modify: `app/Admin/src/main.ts`

**Interfaces:**
- Consumes: `createI18nPlugin` from Task 23, `createDirectivesPlugin` from Task 17
- Produces: Updated app bootstrap registering all plugins

- [ ] **Step 1: Update `main.ts`**

Read the current `app/Admin/src/main.ts` and add i18n and directives plugins:

```ts
import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import { setupPrimeVue } from '@/app/plugins/primevue'
import { createI18nPlugin } from '@/shared/localization'
import { createDirectivesPlugin } from '@/shared/directives'

import './assets/styles/tailwind.css'
import './assets/styles/main.scss'

const app = createApp(App)

app.use(createPinia())
app.use(router)
setupPrimeVue(app)
app.use(createI18nPlugin())
app.use(createDirectivesPlugin())

app.mount('#app')
```

- [ ] **Step 2: Verify build**

```bash
cd app/Admin && pnpm run build
```

Expected: Build succeeds.

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/main.ts
git commit -m "feat(admin): wire i18n and directives plugins in main.ts"
```

---

### Task 32: Phase 3 verification and smoke test

- [ ] **Step 1: Run lint**

```bash
cd app/Admin && pnpm run lint
```

Expected: Zero warnings/errors.

- [ ] **Step 2: Run build**

```bash
cd app/Admin && pnpm run build
```

Expected: Production build succeeds with no errors.

- [ ] **Step 3: Smoke test**

Start the dev server and verify:
- App shell renders (MainLayout, Sidebar, Topbar, Footer)
- Navigation between menu items works
- No console errors
- Dark mode toggle works
- No i18n missing-key warnings in console

```bash
cd app/Admin && pnpm run dev
```

- [ ] **Step 4: Final commit**

```bash
git add -A app/Admin/
git commit -m "feat(admin): complete shared infrastructure migration — all 3 phases"
```

---

## Verification Summary

After each phase, the following must pass:

```bash
cd app/Admin
pnpm run lint        # ESLint + oxlint — zero warnings
pnpm run test:unit   # Vitest — all specs green
pnpm run build       # Vite production build succeeds
```

After Phase 3, additionally:
- Manual smoke test: app shell renders, navigation works, no console errors

## Backward Compatibility

| Existing file | Treatment |
|---|---|
| `shared/composables/useConfirm.ts` | **Kept** — already correct |
| `shared/composables/useDebounce.ts` | **Kept** — already correct |
| `shared/composables/useToastNotify.ts` | **Replaced** with wrapper delegating to `useToast` |
| `app/composables/layout.composable.ts` | **Modified** — dark mode extracted (backward compatible) |
| `app/plugins/primevue.ts` | **Unchanged** |
| `shared/components/` (22 files) | **Unchanged** |
| `shared/api/handlers/` (empty) | **Populated** |
| `shared/api/interceptors/` (empty) | **Populated** |
| `shared/constants/` (empty) | **Populated** |
| `shared/directives/` (empty) | **Populated** |
| `shared/models/` (empty) | **Populated** |
| `shared/utils/` (empty) | **Populated** |
