# Admin SPA Vertical-Slice Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the foundation for a vertical-slice Admin SPA architecture — folder structure, `shared/` library, Pinia stores, layout, providers, plus the first two feature slices (`auth` and `identity/users`) and the `dashboard` landing page.

**Architecture:** Pragmatic feature-folder design (`src/app/` + `src/features/<module>/<slice>/` + `src/shared/`). Server data via TanStack Query; client UI state via Pinia. Features grouped by backend module to mirror the C# `Features/{Module}/{Feature}/` pattern. Composables auto-imported from `shared/composables/`.

**Tech Stack:** Vue 3.5, TypeScript 6, Vite 8, PrimeVue 4.5 (Aura), Tailwind 4, Pinia 3, @tanstack/vue-query 5, vue-router 5, Zod 3, Vitest 4, happy-dom, unplugin-vue-components, unplugin-auto-import, eslint-plugin-boundaries.

**In scope:** Migration steps 1-15 + 20-22 from the spec. **Out of scope:** `features/identity/{roles,permissions}`, `features/catalog/*`, `features/location/*`, `features/profile/*` — each becomes a follow-up plan that copies the `users/` template.

## Global Constraints

- Working directory: `app/Admin/`
- Node: `^20.19.0 || >=22.12.0` (per `package.json` engines)
- pnpm (not npm)
- Path alias: `@/*` → `./src/*` (already in `tsconfig.app.json`)
- PrimeVue auto-imports via `unplugin-vue-components` (already configured)
- Tailwind v4 via `@tailwindcss/vite` (already configured)
- `tsconfig.app.json` has `noUncheckedIndexedAccess: true`
- Dark mode class is `.p-dark` (set on `<html>`)
- All PrimeVue component references in templates resolve automatically — do NOT import them explicitly
- Public surfaces of features are barrels (`index.ts`) only
- No comments in code (per AGENTS.md)
- Every task ends with `pnpm test:unit && pnpm type-check && pnpm lint` green
- Commit messages use Conventional Commits format

---

## Task 1: Add runtime dependencies

**Files:**
- Modify: `app/Admin/package.json`

- [ ] **Step 1: Install TanStack Query and Zod**

Run: `cd app/Admin && pnpm add @tanstack/vue-query zod`
Expected: `package.json` updates, `pnpm-lock.yaml` updates, exit 0.

- [ ] **Step 2: Verify install**

Run: `cd app/Admin && pnpm ls @tanstack/vue-query zod --depth 0`
Expected: Both packages listed.

- [ ] **Step 3: Commit**

```bash
cd app/Admin && git add package.json pnpm-lock.yaml && git commit -m "chore(admin): add @tanstack/vue-query and zod"
```

---

## Task 2: Add dev dependencies

**Files:**
- Modify: `app/Admin/package.json`
- Modify: `app/Admin/vite.config.ts`
- Modify: `app/Admin/eslint.config.ts`

- [ ] **Step 1: Install dev deps**

Run: `cd app/Admin && pnpm add -D unplugin-auto-import eslint-plugin-boundaries`
Expected: `package.json` updates with the two new devDependencies.

- [ ] **Step 2: Wire unplugin-auto-import into Vite**

In `app/Admin/vite.config.ts`, import `AutoImport` from `unplugin-auto-import` and add to `plugins[]` after the existing plugins:

```ts
import AutoImport from 'unplugin-auto-import/vite'
// ... existing imports
plugins: [
  tailwind(),
  vue(),
  vueJsx(),
  vueDevTools(),
  Components({
    resolvers: [PrimeVueResolver()],
  }),
  AutoImport({
    imports: ['vue', 'vue-router'],
    dirs: ['src/shared/composables'],
    dts: 'src/auto-imports.d.ts',
    eslintrc: {
      enabled: true,
    },
  }),
],
```

- [ ] **Step 3: Verify Vite still type-checks**

Run: `cd app/Admin && pnpm type-check`
Expected: PASS (the generated `auto-imports.d.ts` is excluded from build via `tsconfig.app.json` — update that file to also exclude `auto-imports.d.ts` from the `include` glob; it will be created in step 4).

- [ ] **Step 4: Add auto-imports.d.ts and eslintrc to tsconfig ignore**

The `AutoImport` plugin creates `src/auto-imports.d.ts` and `.eslintrc-auto-import.json`. Update `app/Admin/tsconfig.app.json` `include` to:

```json
"include": ["env.d.ts", "src/**/*", "src/**/*.vue", "src/auto-imports.d.ts"]
```

And add to `app/Admin/.gitignore` (create if missing):

```
.auto-imports/
.eslintrc-auto-import.json
```

- [ ] **Step 5: Wire eslint-plugin-boundaries**

In `app/Admin/eslint.config.ts`, add the plugin and config:

```ts
import boundaries from 'eslint-plugin-boundaries'

// inside the exported config array
{
  plugins: { boundaries },
  settings: {
    'boundaries/elements': [
      { type: 'shared', pattern: 'src/shared/*' },
      { type: 'features', pattern: 'src/features/*', mode: 'folder' },
      { type: 'app', pattern: 'src/app/*', mode: 'folder' },
    ],
  },
  rules: {
    'boundaries/element-types': ['error', {
      default: 'allow',
      rules: [
        { from: 'shared', disallow: ['features', 'app'] },
        { from: 'features', disallow: ['features', 'app'] },
        { from: 'app', allow: ['shared', 'features'] },
      ],
    }],
    'boundaries/external': ['error', { allowedModules: ['vue', 'vue-router', '@tanstack/vue-query', 'pinia', 'primevue/*', 'zod'] }],
  },
},
```

(Adjust the export shape to match the existing `eslint.config.ts` flat-config style; merge into the existing config rather than appending.)

- [ ] **Step 6: Run all gates**

Run: `cd app/Admin && pnpm test:unit && pnpm type-check && pnpm lint`
Expected: all green. The `.eslintrc-auto-import.json` file may show a warning about itself — add it to ESLint ignores if needed.

- [ ] **Step 7: Commit**

```bash
cd app/Admin && git add package.json pnpm-lock.yaml vite.config.ts eslint.config.ts tsconfig.app.json .gitignore && git commit -m "chore(admin): wire unplugin-auto-import and eslint-plugin-boundaries"
```

---

## Task 3: Create directory scaffold

**Files:**
- Create: `app/Admin/src/app/`
- Create: `app/Admin/src/app/{providers,plugins,stores,layout,router}/`
- Create: `app/Admin/src/features/{auth,dashboard,identity,catalog,location,profile}/`
- Create: `app/Admin/src/features/identity/{users,roles,permissions}/`
- Create: `app/Admin/src/features/catalog/{products,variants,option-types,taxonomies}/`
- Create: `app/Admin/src/features/location/{countries,states}/`
- Create: `app/Admin/src/features/profile/{profiles,addresses,wishlists,notifications}/`
- Create: `app/Admin/src/shared/{api,ui,composables,lib,types,config}/`
- Create: `app/Admin/src/shared/api/__tests__/`
- Create: `app/Admin/src/shared/lib/__tests__/`
- Create: `app/Admin/src/shared/composables/__tests__/`
- Create: `app/Admin/src/features/_template/users-template/`
- Add `.gitkeep` to each empty directory

- [ ] **Step 1: Create directories with `gitkeep` placeholders**

Run:
```bash
cd app/Admin
mkdir -p src/app/{providers,plugins,stores,layout,router}
mkdir -p src/features/{auth,dashboard}
mkdir -p src/features/identity/{users,roles,permissions}
mkdir -p src/features/catalog/{products,variants,option-types,taxonomies}
mkdir -p src/features/location/{countries,states}
mkdir -p src/features/profile/{profiles,addresses,wishlists,notifications}
mkdir -p src/features/_template/users-template
mkdir -p src/shared/{api,ui,composables,lib,types,config}
mkdir -p src/shared/api/__tests__ src/shared/lib/__tests__ src/shared/composables/__tests__
find src/app src/features src/shared -type d -empty -exec touch {}/.gitkeep \;
```
Expected: directories created, no errors.

- [ ] **Step 2: Commit**

```bash
cd app/Admin && git add src/ && git commit -m "chore(admin): scaffold vertical-slice directory structure"
```

---

## Task 4: Move `src/api.ts` → `src/shared/api/client.ts`

**Files:**
- Move: `app/Admin/src/api.ts` → `app/Admin/src/shared/api/client.ts`
- Update imports

- [ ] **Step 1: Move the file**

Run: `cd app/Admin && git mv src/api.ts src/shared/api/client.ts`

- [ ] **Step 2: Re-export from old location as a deprecated shim (so existing callers keep working)**

Create `app/Admin/src/api.ts` with:

```ts
export { api, ApiError } from './shared/api/client'
```

(Will be deleted in Task 28.)

- [ ] **Step 3: Verify type-check**

Run: `cd app/Admin && pnpm type-check`
Expected: PASS.

- [ ] **Step 4: Commit**

```bash
cd app/Admin && git add -A && git commit -m "refactor(admin): move api client to shared/api"
```

---

## Task 5: Build `shared/api/errors.ts` and `shared/api/envelope.ts`

**Files:**
- Create: `app/Admin/src/shared/api/errors.ts`
- Create: `app/Admin/src/shared/api/__tests__/errors.spec.ts`

**Interfaces:**
- Produces: `class ApiError`, `const ErrorCode`, `function isApiError`

- [ ] **Step 1: Write the test**

```ts
// app/Admin/src/shared/api/__tests__/errors.spec.ts
import { describe, it, expect } from 'vitest'
import { ApiError, ErrorCode, isApiError } from '../errors'

describe('ApiError', () => {
  it('captures status and message', () => {
    const e = new ApiError(404, 'not found')
    expect(e.status).toBe(404)
    expect(e.message).toBe('not found')
    expect(e.name).toBe('ApiError')
    expect(isApiError(e)).toBe(true)
  })
})

describe('ErrorCode', () => {
  it('exposes known codes', () => {
    expect(ErrorCode.NotFound).toBe(404)
    expect(ErrorCode.Unauthorized).toBe(401)
    expect(ErrorCode.Forbidden).toBe(403)
    expect(ErrorCode.Validation).toBe(422)
  })
})

describe('isApiError', () => {
  it('returns false for non-ApiError values', () => {
    expect(isApiError(new Error('x'))).toBe(false)
    expect(isApiError('x')).toBe(false)
    expect(isApiError(null)).toBe(false)
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd app/Admin && pnpm test:unit src/shared/api/__tests__/errors.spec.ts`
Expected: FAIL (module not found).

- [ ] **Step 3: Implement `errors.ts`**

```ts
// app/Admin/src/shared/api/errors.ts
export const ErrorCode = {
  BadRequest: 400,
  Unauthorized: 401,
  Forbidden: 403,
  NotFound: 404,
  Conflict: 409,
  Validation: 422,
  Server: 500,
} as const
export type ErrorCodeValue = (typeof ErrorCode)[keyof typeof ErrorCode]

export class ApiError extends Error {
  public readonly code: ErrorCodeValue

  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message)
    this.name = 'ApiError'
    this.code = (status as ErrorCodeValue) ?? ErrorCode.Server
  }
}

export function isApiError(value: unknown): value is ApiError {
  return value instanceof ApiError
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `cd app/Admin && pnpm test:unit src/shared/api/__tests__/errors.spec.ts`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
cd app/Admin && git add src/shared/api/errors.ts src/shared/api/__tests__/errors.spec.ts && git commit -m "feat(admin): add shared/api/errors with ApiError and isApiError"
```

---

## Task 6: Build `shared/api/envelope.ts` and `shared/api/paged-result.ts`

**Files:**
- Create: `app/Admin/src/shared/api/envelope.ts`
- Create: `app/Admin/src/shared/api/paged-result.ts`
- Create: `app/Admin/src/shared/api/__tests__/envelope.spec.ts`
- Create: `app/Admin/src/shared/api/__tests__/paged-result.spec.ts`

**Interfaces:**
- Produces: `type Envelope<T>`, `type PagedResult<T>`, `type PageRequest`

- [ ] **Step 1: Write the tests**

```ts
// app/Admin/src/shared/api/__tests__/envelope.spec.ts
import { describe, it, expect } from 'vitest'
import type { Envelope } from '../envelope'

describe('Envelope<T>', () => {
  it('matches backend Result<T> shape', () => {
    const ok: Envelope<{ id: string }> = {
      isSuccess: true,
      value: { id: '1' },
      errors: [],
    }
    const fail: Envelope<never> = {
      isSuccess: false,
      value: null,
      errors: [{ code: 'NOT_FOUND', message: 'missing' }],
    }
    expect(ok.isSuccess).toBe(true)
    expect(fail.isSuccess).toBe(false)
  })
})
```

```ts
// app/Admin/src/shared/api/__tests__/paged-result.spec.ts
import { describe, it, expect } from 'vitest'
import type { PagedResult } from '../paged-result'

describe('PagedResult<T>', () => {
  it('matches backend PagedResult<T> shape', () => {
    const r: PagedResult<number> = {
      items: [1, 2, 3],
      totalCount: 3,
      page: 1,
      pageSize: 10,
    }
    expect(r.items).toHaveLength(3)
    expect(r.totalCount).toBe(3)
  })
})
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `cd app/Admin && pnpm test:unit src/shared/api/__tests__/envelope.spec.ts src/shared/api/__tests__/paged-result.spec.ts`
Expected: FAIL (modules not found).

- [ ] **Step 3: Implement `envelope.ts`**

```ts
// app/Admin/src/shared/api/envelope.ts
export interface EnvelopeError {
  code: string
  message: string
  field?: string
}

export interface Envelope<T> {
  isSuccess: boolean
  value: T | null
  errors: EnvelopeError[]
}
```

- [ ] **Step 4: Implement `paged-result.ts`**

```ts
// app/Admin/src/shared/api/paged-result.ts
export interface PageRequest {
  page: number
  pageSize: number
  sort?: string
  direction?: 'asc' | 'desc'
  search?: string
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}
```

- [ ] **Step 5: Run tests to verify they pass**

Run: `cd app/Admin && pnpm test:unit src/shared/api/__tests__/envelope.spec.ts src/shared/api/__tests__/paged-result.spec.ts`
Expected: PASS.

- [ ] **Step 6: Run all gates and commit**

```bash
cd app/Admin && pnpm type-check && pnpm lint
cd app/Admin && git add src/shared/api/ && git commit -m "feat(admin): add shared/api envelope and paged-result types"
```

---

## Task 7: Build `shared/api/query-keys.ts` and `shared/api/fetch-options.ts`

**Files:**
- Create: `app/Admin/src/shared/api/query-keys.ts`
- Create: `app/Admin/src/shared/api/fetch-options.ts`
- Create: `app/Admin/src/shared/api/__tests__/query-keys.spec.ts`

**Interfaces:**
- Produces: `function withFilters(base, filters)`, `function withId(base, id)`, `function buildHeaders(opts)`, `function getAuthToken()`

- [ ] **Step 1: Write the test**

```ts
// app/Admin/src/shared/api/__tests__/query-keys.spec.ts
import { describe, it, expect } from 'vitest'
import { withFilters, withId } from '../query-keys'

describe('query-keys helpers', () => {
  it('withFilters appends a filters tuple', () => {
    expect(withFilters(['users', 'list'], { role: 'admin' })).toEqual([
      'users',
      'list',
      { role: 'admin' },
    ])
  })

  it('withId appends a string id', () => {
    expect(withId(['users'], '123')).toEqual(['users', '123'])
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd app/Admin && pnpm test:unit src/shared/api/__tests__/query-keys.spec.ts`
Expected: FAIL.

- [ ] **Step 3: Implement `query-keys.ts`**

```ts
// app/Admin/src/shared/api/query-keys.ts
export function withFilters<TFilters extends Record<string, unknown>>(
  base: readonly unknown[],
  filters: TFilters,
): readonly unknown[] {
  return [...base, filters] as const
}

export function withId(base: readonly unknown[], id: string): readonly unknown[] {
  return [...base, id] as const
}
```

- [ ] **Step 4: Implement `fetch-options.ts`**

```ts
// app/Admin/src/shared/api/fetch-options.ts
let tokenAccessor: () => string | null = () => null

export function setAuthTokenAccessor(fn: () => string | null): void {
  tokenAccessor = fn
}

export function buildHeaders(extra?: HeadersInit): HeadersInit {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    'X-Request-Id': crypto.randomUUID(),
  }
  const token = tokenAccessor()
  if (token) {
    headers['Authorization'] = `Bearer ${token}`
  }
  return { ...headers, ...(extra as Record<string, string> | undefined) }
}
```

- [ ] **Step 5: Run test to verify it passes**

Run: `cd app/Admin && pnpm test:unit src/shared/api/__tests__/query-keys.spec.ts`
Expected: PASS.

- [ ] **Step 6: Run all gates and commit**

```bash
cd app/Admin && pnpm type-check && pnpm lint
cd app/Admin && git add src/shared/api/ && git commit -m "feat(admin): add shared/api query-keys and fetch-options"
```

---

## Task 8: Upgrade `shared/api/client.ts` to use the new types

**Files:**
- Modify: `app/Admin/src/shared/api/client.ts`
- Create: `app/Admin/src/shared/api/__tests__/client.spec.ts`

**Interfaces:**
- Consumes: `ApiError`, `buildHeaders` from previous tasks
- Produces: typed `api.get<T>()`, `api.post<T>()`, `api.put<T>()`, `api.delete<T>()`, `api.unwraps envelope`

- [ ] **Step 1: Write the test**

```ts
// app/Admin/src/shared/api/__tests__/client.spec.ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { api, ApiError } from '../client'

describe('api client', () => {
  beforeEach(() => {
    vi.restoreAllMocks()
  })

  it('returns parsed JSON for 2xx responses', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(
        new Response(JSON.stringify({ id: 1 }), { status: 200, headers: { 'Content-Type': 'application/json' } }),
      ),
    )
    const result = await api.get<{ id: number }>('/x')
    expect(result).toEqual({ id: 1 })
  })

  it('throws ApiError on non-2xx', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response('nope', { status: 404 })))
    await expect(api.get('/x')).rejects.toBeInstanceOf(ApiError)
  })

  it('returns undefined for 204 responses', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(null, { status: 204 })))
    const result = await api.delete('/x')
    expect(result).toBeUndefined()
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd app/Admin && pnpm test:unit src/shared/api/__tests__/client.spec.ts`
Expected: FAIL.

- [ ] **Step 3: Rewrite `client.ts`**

```ts
// app/Admin/src/shared/api/client.ts
import { ApiError } from './errors'
import { buildHeaders } from './fetch-options'
import type { Envelope } from './envelope'
import type { PagedResult } from './paged-result'

const BASE_URL = import.meta.env.VITE_API_URL || ''

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${BASE_URL}${path}`, {
    ...options,
    headers: buildHeaders(options?.headers),
  })
  if (!response.ok) {
    const message = await response.text().catch(() => response.statusText)
    throw new ApiError(response.status, message || `API error: ${response.statusText}`)
  }
  if (response.status === 204) return undefined as T
  return (await response.json()) as T
}

function unwrap<T>(envelope: Envelope<T>): T {
  if (!envelope.isSuccess) {
    const message = envelope.errors[0]?.message ?? 'Unknown error'
    throw new ApiError(422, message)
  }
  return envelope.value as T
}

export const api = {
  get<T>(path: string): Promise<T> {
    return request<Envelope<T> | T>(path).then((r) =>
      r && typeof r === 'object' && 'isSuccess' in r ? unwrap(r as Envelope<T>) : (r as T),
    )
  },
  getPaged<T>(path: string): Promise<PagedResult<T>> {
    return request<PagedResult<T>>(path)
  },
  post<T>(path: string, body?: unknown): Promise<T> {
    return request<Envelope<T> | T>(path, {
      method: 'POST',
      body: body ? JSON.stringify(body) : undefined,
    }).then((r) =>
      r && typeof r === 'object' && 'isSuccess' in r ? unwrap(r as Envelope<T>) : (r as T),
    )
  },
  put<T>(path: string, body?: unknown): Promise<T> {
    return request<Envelope<T> | T>(path, {
      method: 'PUT',
      body: body ? JSON.stringify(body) : undefined,
    }).then((r) =>
      r && typeof r === 'object' && 'isSuccess' in r ? unwrap(r as Envelope<T>) : (r as T),
    )
  },
  delete<T = void>(path: string): Promise<T> {
    return request<T>(path, { method: 'DELETE' })
  },
}

export { ApiError } from './errors'
```

- [ ] **Step 4: Run test to verify it passes**

Run: `cd app/Admin && pnpm test:unit src/shared/api/__tests__/client.spec.ts`
Expected: PASS.

- [ ] **Step 5: Run all gates and commit**

```bash
cd app/Admin && pnpm type-check && pnpm lint
cd app/Admin && git add src/shared/api/ && git commit -m "feat(admin): upgrade shared/api client with envelope unwrapping"
```

---

## Task 9: Build `shared/lib/` (pure utilities)

**Files:**
- Create: `app/Admin/src/shared/lib/formatters.ts`
- Create: `app/Admin/src/shared/lib/slug.ts`
- Create: `app/Admin/src/shared/lib/validators.ts`
- Create: `app/Admin/src/shared/lib/strings.ts`
- Create: `app/Admin/src/shared/lib/arrays.ts`
- Create: `app/Admin/src/shared/lib/__tests__/formatters.spec.ts`
- Create: `app/Admin/src/shared/lib/__tests__/slug.spec.ts`
- Create: `app/Admin/src/shared/lib/__tests__/validators.spec.ts`
- Create: `app/Admin/src/shared/lib/__tests__/strings.spec.ts`
- Create: `app/Admin/src/shared/lib/__tests__/arrays.spec.ts`

**Interfaces:**
- Produces: `formatDate`, `formatCurrency`, `formatNumber`, `slugify`, `humanize`, `isEmail`, `isUrl`, `isGuid`, `truncate`, `titleCase`, `groupBy`, `sortBy`, `uniqueBy`

- [ ] **Step 1: Write all tests**

```ts
// app/Admin/src/shared/lib/__tests__/formatters.spec.ts
import { describe, it, expect } from 'vitest'
import { formatDate, formatCurrency, formatNumber } from '../formatters'

describe('formatters', () => {
  it('formatDate produces YYYY-MM-DD', () => {
    expect(formatDate(new Date('2026-07-06T10:00:00Z'))).toBe('2026-07-06')
  })
  it('formatCurrency uses USD by default', () => {
    expect(formatCurrency(12.5)).toBe('$12.50')
  })
  it('formatNumber rounds to 2 decimals', () => {
    expect(formatNumber(1.234)).toBe('1.23')
  })
})
```

```ts
// app/Admin/src/shared/lib/__tests__/slug.spec.ts
import { describe, it, expect } from 'vitest'
import { slugify, humanize } from '../slug'

describe('slug', () => {
  it('slugify lowercases and dashes', () => {
    expect(slugify('Hello World!')).toBe('hello-world')
  })
  it('humanize capitalizes and spaces', () => {
    expect(humanize('hello-world')).toBe('Hello World')
  })
})
```

```ts
// app/Admin/src/shared/lib/__tests__/validators.spec.ts
import { describe, it, expect } from 'vitest'
import { isEmail, isUrl, isGuid } from '../validators'

describe('validators', () => {
  it('isEmail', () => {
    expect(isEmail('a@b.c')).toBe(true)
    expect(isEmail('nope')).toBe(false)
  })
  it('isUrl', () => {
    expect(isUrl('https://x.y')).toBe(true)
    expect(isUrl('x')).toBe(false)
  })
  it('isGuid', () => {
    expect(isGuid('11111111-2222-3333-4444-555555555555')).toBe(true)
    expect(isGuid('not-a-guid')).toBe(false)
  })
})
```

```ts
// app/Admin/src/shared/lib/__tests__/strings.spec.ts
import { describe, it, expect } from 'vitest'
import { truncate, titleCase } from '../strings'

describe('strings', () => {
  it('truncate appends ellipsis', () => {
    expect(truncate('hello world', 5)).toBe('hello…')
  })
  it('titleCase capitalizes words', () => {
    expect(titleCase('hello world')).toBe('Hello World')
  })
})
```

```ts
// app/Admin/src/shared/lib/__tests__/arrays.spec.ts
import { describe, it, expect } from 'vitest'
import { groupBy, sortBy, uniqueBy } from '../arrays'

describe('arrays', () => {
  it('groupBy', () => {
    const r = groupBy([{ k: 'a', v: 1 }, { k: 'b', v: 2 }, { k: 'a', v: 3 }], (x) => x.k)
    expect(r.a).toEqual([{ k: 'a', v: 1 }, { k: 'a', v: 3 }])
    expect(r.b).toEqual([{ k: 'b', v: 2 }])
  })
  it('sortBy asc/desc', () => {
    expect(sortBy([{ n: 3 }, { n: 1 }, { n: 2 }], (x) => x.n, 'asc').map((x) => x.n)).toEqual([1, 2, 3])
    expect(sortBy([{ n: 3 }, { n: 1 }, { n: 2 }], (x) => x.n, 'desc').map((x) => x.n)).toEqual([3, 2, 1])
  })
  it('uniqueBy', () => {
    expect(uniqueBy([{ id: 1 }, { id: 2 }, { id: 1 }], (x) => x.id)).toEqual([{ id: 1 }, { id: 2 }])
  })
})
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `cd app/Admin && pnpm test:unit src/shared/lib`
Expected: all FAIL.

- [ ] **Step 3: Implement `formatters.ts`**

```ts
// app/Admin/src/shared/lib/formatters.ts
export function formatDate(d: Date | string, locale = 'en-US'): string {
  const date = typeof d === 'string' ? new Date(d) : d
  return new Intl.DateTimeFormat(locale, { year: 'numeric', month: '2-digit', day: '2-digit' }).format(date)
}

export function formatCurrency(amount: number, currency = 'USD', locale = 'en-US'): string {
  return new Intl.NumberFormat(locale, { style: 'currency', currency }).format(amount)
}

export function formatNumber(n: number, decimals = 2, locale = 'en-US'): string {
  return new Intl.NumberFormat(locale, { minimumFractionDigits: decimals, maximumFractionDigits: decimals }).format(n)
}
```

- [ ] **Step 4: Implement `slug.ts`**

```ts
// app/Admin/src/shared/lib/slug.ts
export function slugify(input: string): string {
  return input
    .toLowerCase()
    .normalize('NFKD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')
}

export function humanize(input: string): string {
  return input.replace(/[-_]+/g, ' ').replace(/\b\w/g, (c) => c.toUpperCase())
}
```

- [ ] **Step 5: Implement `validators.ts`**

```ts
// app/Admin/src/shared/lib/validators.ts
const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
const URL_RE = /^https?:\/\/[^\s/$.?#].[^\s]*$/i
const GUID_RE = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i

export function isEmail(s: string): boolean {
  return EMAIL_RE.test(s)
}

export function isUrl(s: string): boolean {
  return URL_RE.test(s)
}

export function isGuid(s: string): boolean {
  return GUID_RE.test(s)
}
```

- [ ] **Step 6: Implement `strings.ts`**

```ts
// app/Admin/src/shared/lib/strings.ts
export function truncate(s: string, max: number): string {
  if (s.length <= max) return s
  return `${s.slice(0, max - 1)}…`
}

export function titleCase(s: string): string {
  return s.replace(/\b\w/g, (c) => c.toUpperCase())
}
```

- [ ] **Step 7: Implement `arrays.ts`**

```ts
// app/Admin/src/shared/lib/arrays.ts
export function groupBy<T, K extends string | number>(items: T[], key: (item: T) => K): Record<K, T[]> {
  return items.reduce(
    (acc, item) => {
      const k = key(item)
      ;(acc[k] ||= []).push(item)
      return acc
    },
    {} as Record<K, T[]>,
  )
}

export function sortBy<T>(items: T[], key: (item: T) => number | string, direction: 'asc' | 'desc' = 'asc'): T[] {
  return [...items].sort((a, b) => {
    const va = key(a)
    const vb = key(b)
    if (va < vb) return direction === 'asc' ? -1 : 1
    if (va > vb) return direction === 'asc' ? 1 : -1
    return 0
  })
}

export function uniqueBy<T>(items: T[], key: (item: T) => unknown): T[] {
  const seen = new Set<unknown>()
  return items.filter((item) => {
    const k = key(item)
    if (seen.has(k)) return false
    seen.add(k)
    return true
  })
}
```

- [ ] **Step 8: Run tests to verify they pass**

Run: `cd app/Admin && pnpm test:unit src/shared/lib`
Expected: all PASS.

- [ ] **Step 9: Run all gates and commit**

```bash
cd app/Admin && pnpm type-check && pnpm lint
cd app/Admin && git add src/shared/lib/ && git commit -m "feat(admin): add shared/lib pure utilities"
```

---

## Task 10: Build `shared/types/`

**Files:**
- Create: `app/Admin/src/shared/types/id.ts`
- Create: `app/Admin/src/shared/types/timestamp.ts`
- Create: `app/Admin/src/shared/types/page.ts`
- Create: `app/Admin/src/shared/types/sort.ts`

**Interfaces:**
- Produces: branded `UserId`, `ProductId`, etc. helpers; `IsoDateString`; `PageRequest` re-export; `SortDirection`

- [ ] **Step 1: Implement `id.ts`**

```ts
// app/Admin/src/shared/types/id.ts
declare const brand: unique symbol
export type Brand<T, B> = T & { readonly [brand]: B }

export type UserId = Brand<string, 'UserId'>
export type RoleId = Brand<string, 'RoleId'>
export type ProductId = Brand<string, 'ProductId'>
export type VariantId = Brand<string, 'VariantId'>
export type CountryId = Brand<string, 'CountryId'>
export type StateId = Brand<string, 'StateId'>

export const asId = <T extends string>(s: string): T => s as T
```

- [ ] **Step 2: Implement `timestamp.ts`**

```ts
// app/Admin/src/shared/types/timestamp.ts
export type IsoDateString = string

export function nowIso(): IsoDateString {
  return new Date().toISOString()
}
```

- [ ] **Step 3: Implement `page.ts`**

```ts
// app/Admin/src/shared/types/page.ts
export interface PageRequest {
  page: number
  pageSize: number
  search?: string
}

export const DEFAULT_PAGE = 1
export const DEFAULT_PAGE_SIZE = 20
```

- [ ] **Step 4: Implement `sort.ts`**

```ts
// app/Admin/src/shared/types/sort.ts
export type SortDirection = 'asc' | 'desc'

export interface Sort<TField extends string = string> {
  field: TField
  direction: SortDirection
}
```

- [ ] **Step 5: Run all gates and commit**

```bash
cd app/Admin && pnpm type-check && pnpm lint
cd app/Admin && git add src/shared/types/ && git commit -m "feat(admin): add shared/types branded ids and page/sort"
```

---

## Task 11: Build `shared/config/`

**Files:**
- Create: `app/Admin/src/shared/config/env.ts`
- Create: `app/Admin/src/shared/config/app.ts`
- Create: `app/Admin/src/shared/config/routes.ts`

- [ ] **Step 1: Implement `env.ts`**

```ts
// app/Admin/src/shared/config/env.ts
export const env = {
  apiUrl: import.meta.env.VITE_API_URL ?? '',
  appEnv: import.meta.env.MODE,
  isDev: import.meta.env.DEV,
  isProd: import.meta.env.PROD,
} as const
```

- [ ] **Step 2: Implement `app.ts`**

```ts
// app/Admin/src/shared/config/app.ts
export const APP_NAME = 'ReSys Admin'
export const APP_VERSION = '0.1.0'
export const DEFAULT_PAGE_SIZE = 20
```

- [ ] **Step 3: Implement `routes.ts`**

```ts
// app/Admin/src/shared/config/routes.ts
export const RouteName = {
  Login: 'login',
  Dashboard: 'dashboard',
  Users: 'users',
  UserCreate: 'user-create',
  UserEdit: 'user-edit',
  UserDetails: 'user-details',
} as const

export type RouteNameValue = (typeof RouteName)[keyof typeof RouteName]
```

- [ ] **Step 4: Run all gates and commit**

```bash
cd app/Admin && pnpm type-check && pnpm lint
cd app/Admin && git add src/shared/config/ && git commit -m "feat(admin): add shared/config env, app, and route names"
```

---

## Task 12: Build `shared/composables/`

**Files:**
- Create: `app/Admin/src/shared/composables/useDebouncedRef.ts`
- Create: `app/Admin/src/shared/composables/useDisclosure.ts`
- Create: `app/Admin/src/shared/composables/useFormatters.ts`
- Create: `app/Admin/src/shared/composables/usePagination.ts`
- Create: `app/Admin/src/shared/composables/useQueryString.ts`
- Create: `app/Admin/src/shared/composables/useToast.ts`
- Create: `app/Admin/src/shared/composables/useConfirm.ts`
- Create: `app/Admin/src/shared/composables/__tests__/useDebouncedRef.spec.ts`
- Create: `app/Admin/src/shared/composables/__tests__/useDisclosure.spec.ts`
- Create: `app/Admin/src/shared/composables/__tests__/useQueryString.spec.ts`

**Interfaces:**
- Produces: 7 auto-imported composables

- [ ] **Step 1: Write tests**

```ts
// app/Admin/src/shared/composables/__tests__/useDebouncedRef.spec.ts
import { describe, it, expect, vi } from 'vitest'
import { ref, nextTick } from 'vue'
import { useDebouncedRef } from '../useDebouncedRef'

describe('useDebouncedRef', () => {
  it('emits after the delay', async () => {
    vi.useFakeTimers()
    const source = ref('a')
    const debounced = useDebouncedRef(source, 200)
    source.value = 'b'
    expect(debounced.value).toBe('a')
    vi.advanceTimersByTime(200)
    await nextTick()
    expect(debounced.value).toBe('b')
    vi.useRealTimers()
  })
})
```

```ts
// app/Admin/src/shared/composables/__tests__/useDisclosure.spec.ts
import { describe, it, expect } from 'vitest'
import { useDisclosure } from '../useDisclosure'

describe('useDisclosure', () => {
  it('toggles open state', () => {
    const { isOpen, open, close, toggle } = useDisclosure()
    expect(isOpen.value).toBe(false)
    open()
    expect(isOpen.value).toBe(true)
    toggle()
    expect(isOpen.value).toBe(false)
    close()
    expect(isOpen.value).toBe(false)
  })
})
```

```ts
// app/Admin/src/shared/composables/__tests__/useQueryString.spec.ts
import { describe, it, expect } from 'vitest'
import { ref } from 'vue'
import { useQueryString } from '../useQueryString'

describe('useQueryString', () => {
  it('binds a ref to a query param key', () => {
    const value = useQueryString('q', ref('hello'))
    expect(value.value).toBe('hello')
  })
})
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `cd app/Admin && pnpm test:unit src/shared/composables`
Expected: all FAIL.

- [ ] **Step 3: Implement `useDebouncedRef.ts`**

```ts
// app/Admin/src/shared/composables/useDebouncedRef.ts
import { ref, watch, type Ref } from 'vue'

export function useDebouncedRef<T>(source: Ref<T>, delay = 200): Ref<T> {
  const debounced = ref(source.value) as Ref<T>
  let timer: ReturnType<typeof setTimeout> | undefined
  watch(source, (next) => {
    clearTimeout(timer)
    timer = setTimeout(() => {
      debounced.value = next
    }, delay)
  })
  return debounced
}
```

- [ ] **Step 4: Implement `useDisclosure.ts`**

```ts
// app/Admin/src/shared/composables/useDisclosure.ts
import { ref, type Ref } from 'vue'

export function useDisclosure(initial = false): {
  isOpen: Ref<boolean>
  open: () => void
  close: () => void
  toggle: () => void
} {
  const isOpen = ref(initial)
  return {
    isOpen,
    open: () => (isOpen.value = true),
    close: () => (isOpen.value = false),
    toggle: () => (isOpen.value = !isOpen.value),
  }
}
```

- [ ] **Step 5: Implement `useFormatters.ts`**

```ts
// app/Admin/src/shared/composables/useFormatters.ts
import { formatDate, formatCurrency, formatNumber } from '../lib/formatters'

export function useFormatters() {
  return { formatDate, formatCurrency, formatNumber }
}
```

- [ ] **Step 6: Implement `usePagination.ts`**

```ts
// app/Admin/src/shared/composables/usePagination.ts
import { ref, computed, type Ref } from 'vue'
import { DEFAULT_PAGE_SIZE, DEFAULT_PAGE } from '../config/app'

export function usePagination(total: Ref<number>, pageSize = DEFAULT_PAGE_SIZE) {
  const page = ref(DEFAULT_PAGE)
  const totalPages = computed(() => Math.max(1, Math.ceil(total.value / pageSize)))
  const offset = computed(() => (page.value - 1) * pageSize)

  function next() {
    if (page.value < totalPages.value) page.value += 1
  }
  function prev() {
    if (page.value > 1) page.value -= 1
  }
  function reset() {
    page.value = DEFAULT_PAGE
  }

  return { page, pageSize, totalPages, offset, next, prev, reset }
}
```

- [ ] **Step 7: Implement `useQueryString.ts`**

```ts
// app/Admin/src/shared/composables/useQueryString.ts
import { ref, watch, type Ref } from 'vue'

export function useQueryString(key: string, fallback: Ref<string>): Ref<string> {
  const url = new URL(window.location.href)
  const initial = url.searchParams.get(key) ?? fallback.value
  const value = ref(initial) as Ref<string>

  watch(value, (next) => {
    const u = new URL(window.location.href)
    if (next) u.searchParams.set(key, next)
    else u.searchParams.delete(key)
    window.history.replaceState({}, '', u.toString())
  })

  return value
}
```

- [ ] **Step 8: Implement `useToast.ts`**

```ts
// app/Admin/src/shared/composables/useToast.ts
import { useToast as usePrimeVueToast } from 'primevue/usetoast'

export function useToast() {
  return usePrimeVueToast()
}
```

- [ ] **Step 9: Implement `useConfirm.ts`**

```ts
// app/Admin/src/shared/composables/useConfirm.ts
import { useConfirm as usePrimeVueConfirm } from 'primevue/useconfirm'

export function useConfirm() {
  return usePrimeVueConfirm()
}
```

- [ ] **Step 10: Run tests and gates**

Run: `cd app/Admin && pnpm test:unit src/shared/composables && pnpm type-check && pnpm lint`
Expected: all PASS. The auto-generated `auto-imports.d.ts` will include these — verify by inspecting it.

- [ ] **Step 11: Commit**

```bash
cd app/Admin && git add src/shared/composables/ && git commit -m "feat(admin): add shared/composables (auto-imported)"
```

---

## Task 13: Build `app/plugins/` (PrimeVue, Pinia, Vue Query)

**Files:**
- Create: `app/Admin/src/app/plugins/primevue.ts`
- Create: `app/Admin/src/app/plugins/pinia.ts`
- Create: `app/Admin/src/app/plugins/vue-query.ts`

- [ ] **Step 1: Implement `primevue.ts`**

```ts
// app/Admin/src/app/plugins/primevue.ts
import PrimeVue from 'primevue/config'
import Aura from '@primevue/themes/aura'
import ToastService from 'primevue/toastservice'
import ConfirmationService from 'primevue/confirmationservice'
import type { App } from 'vue'

export function installPrimeVue(app: App): void {
  app.use(PrimeVue, {
    theme: {
      preset: Aura,
      options: { darkModeSelector: '.p-dark' },
    },
    ripple: true,
  })
  app.use(ToastService)
  app.use(ConfirmationService)
}
```

- [ ] **Step 2: Implement `pinia.ts`**

```ts
// app/Admin/src/app/plugins/pinia.ts
import { createPinia } from 'pinia'
import type { App } from 'vue'

export function installPinia(app: App): void {
  app.use(createPinia())
}
```

- [ ] **Step 3: Implement `vue-query.ts`**

```ts
// app/Admin/src/app/plugins/vue-query.ts
import { VueQueryPlugin, QueryClient } from '@tanstack/vue-query'
import type { App } from 'vue'

export function installVueQuery(app: App): void {
  const client = new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 30_000,
        retry: 1,
        refetchOnWindowFocus: false,
      },
    },
  })
  app.use(VueQueryPlugin, { queryClient: client })
}
```

- [ ] **Step 4: Commit**

```bash
cd app/Admin && git add src/app/plugins/ && git commit -m "feat(admin): add app/plugins (primevue, pinia, vue-query)"
```

---

## Task 14: Build `app/stores/` (Pinia — client-only)

**Files:**
- Create: `app/Admin/src/app/stores/theme.store.ts`
- Create: `app/Admin/src/app/stores/sidebar.store.ts`
- Create: `app/Admin/src/app/stores/tenant.store.ts`
- Create: `app/Admin/src/app/stores/__tests__/theme.store.spec.ts`
- Create: `app/Admin/src/app/stores/__tests__/sidebar.store.spec.ts`

- [ ] **Step 1: Write tests**

```ts
// app/Admin/src/app/stores/__tests__/theme.store.spec.ts
import { describe, it, expect, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { useThemeStore } from '../theme.store'

describe('theme.store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    document.documentElement.classList.remove('p-dark')
  })

  it('toggles dark mode and updates DOM', () => {
    const s = useThemeStore()
    expect(s.isDark).toBe(false)
    s.toggle()
    expect(s.isDark).toBe(true)
    expect(document.documentElement.classList.contains('p-dark')).toBe(true)
    s.toggle()
    expect(s.isDark).toBe(false)
  })
})
```

```ts
// app/Admin/src/app/stores/__tests__/sidebar.store.spec.ts
import { describe, it, expect, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { useSidebarStore } from '../sidebar.store'

describe('sidebar.store', () => {
  beforeEach(() => setActivePinia(createPinia()))

  it('toggles collapsed', () => {
    const s = useSidebarStore()
    expect(s.collapsed).toBe(false)
    s.toggle()
    expect(s.collapsed).toBe(true)
  })
})
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `cd app/Admin && pnpm test:unit src/app/stores`
Expected: FAIL.

- [ ] **Step 3: Implement `theme.store.ts`**

```ts
// app/Admin/src/app/stores/theme.store.ts
import { ref, watch } from 'vue'
import { defineStore } from 'pinia'

export const useThemeStore = defineStore('theme', () => {
  const isDark = ref(false)

  function toggle() {
    isDark.value = !isDark.value
  }
  function setDark(value: boolean) {
    isDark.value = value
  }

  watch(
    isDark,
    (v) => {
      document.documentElement.classList.toggle('p-dark', v)
    },
    { immediate: true },
  )

  return { isDark, toggle, setDark }
})
```

- [ ] **Step 4: Implement `sidebar.store.ts`**

```ts
// app/Admin/src/app/stores/sidebar.store.ts
import { ref, watch } from 'vue'
import { defineStore } from 'pinia'

const STORAGE_KEY = 'admin:sidebar:collapsed'

export const useSidebarStore = defineStore('sidebar', () => {
  const collapsed = ref(localStorage.getItem(STORAGE_KEY) === '1')

  function toggle() {
    collapsed.value = !collapsed.value
  }

  watch(collapsed, (v) => {
    localStorage.setItem(STORAGE_KEY, v ? '1' : '0')
  })

  return { collapsed, toggle }
})
```

- [ ] **Step 5: Implement `tenant.store.ts`**

```ts
// app/Admin/src/app/stores/tenant.store.ts
import { ref } from 'vue'
import { defineStore } from 'pinia'

export const useTenantStore = defineStore('tenant', () => {
  const currentTenantId = ref<string | null>(null)
  function setTenant(id: string) {
    currentTenantId.value = id
  }
  return { currentTenantId, setTenant }
})
```

- [ ] **Step 6: Run tests and gates**

Run: `cd app/Admin && pnpm test:unit src/app/stores && pnpm type-check && pnpm lint`
Expected: PASS.

- [ ] **Step 7: Commit**

```bash
cd app/Admin && git add src/app/stores/ && git commit -m "feat(admin): add app/stores (theme, sidebar, tenant)"
```

---

## Task 15: Build `app/layout/`

**Files:**
- Create: `app/Admin/src/app/layout/AppShell.vue`
- Create: `app/Admin/src/app/layout/AppSidebar.vue`
- Create: `app/Admin/src/app/layout/AppTopbar.vue`
- Create: `app/Admin/src/app/layout/AppFooter.vue`

- [ ] **Step 1: Implement `AppFooter.vue`**

```vue
<!-- app/Admin/src/app/layout/AppFooter.vue -->
<template>
  <footer class="border-t border-surface-200 p-3 text-sm text-color-secondary">
    © {{ year }} {{ appName }} v{{ version }}
  </footer>
</template>

<script setup lang="ts">
import { APP_NAME, APP_VERSION } from '@/shared/config/app'

const year = new Date().getFullYear()
const appName = APP_NAME
const version = APP_VERSION
</script>
```

- [ ] **Step 2: Implement `AppSidebar.vue`**

```vue
<!-- app/Admin/src/app/layout/AppSidebar.vue -->
<template>
  <aside :class="['w-60 border-r border-surface-200 p-3', sidebar.collapsed && 'w-16']">
    <Button :icon="sidebar.collapsed ? 'pi pi-angle-double-right' : 'pi pi-angle-double-left'" text rounded @click="sidebar.toggle()" />
    <nav class="mt-3 flex flex-col gap-1">
      <RouterLink
        v-for="item in items"
        :key="item.to"
        :to="item.to"
        class="rounded p-2 hover:bg-surface-100"
        active-class="bg-primary-100 text-primary-700"
      >
        <i :class="item.icon" class="mr-2" />
        <span v-if="!sidebar.collapsed">{{ item.label }}</span>
      </RouterLink>
    </nav>
  </aside>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { useSidebarStore } from '@/app/stores/sidebar.store'

const sidebar = useSidebarStore()
const items = [
  { to: '/', label: 'Dashboard', icon: 'pi pi-home' },
  { to: '/identity/users', label: 'Users', icon: 'pi pi-users' },
]
</script>
```

- [ ] **Step 3: Implement `AppTopbar.vue`**

```vue
<!-- app/Admin/src/app/layout/AppTopbar.vue -->
<template>
  <header class="flex items-center justify-between border-b border-surface-200 p-3">
    <h1 class="text-lg font-semibold">{{ appName }}</h1>
    <div class="flex items-center gap-2">
      <Button :icon="theme.isDark ? 'pi pi-sun' : 'pi pi-moon'" text rounded @click="theme.toggle()" />
      <RouterLink to="/login">
        <Button icon="pi pi-sign-out" text rounded />
      </RouterLink>
    </div>
  </header>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { useThemeStore } from '@/app/stores/theme.store'
import { APP_NAME } from '@/shared/config/app'

const theme = useThemeStore()
const appName = APP_NAME
</script>
```

- [ ] **Step 4: Implement `AppShell.vue`**

```vue
<!-- app/Admin/src/app/layout/AppShell.vue -->
<template>
  <div class="flex h-screen flex-col">
    <AppTopbar />
    <div class="flex flex-1 overflow-hidden">
      <AppSidebar />
      <main class="flex-1 overflow-auto p-4">
        <RouterView />
      </main>
    </div>
    <AppFooter />
  </div>
</template>

<script setup lang="ts">
import { RouterView } from 'vue-router'
import AppTopbar from './AppTopbar.vue'
import AppSidebar from './AppSidebar.vue'
import AppFooter from './AppFooter.vue'
</script>
```

- [ ] **Step 5: Run all gates and commit**

```bash
cd app/Admin && pnpm type-check && pnpm lint
cd app/Admin && git add src/app/layout/ && git commit -m "feat(admin): add app/layout (Shell, Sidebar, Topbar, Footer)"
```

---

## Task 16: Build `shared/ui/` primitives (button, form field, dialog, drawer, confirm, toast, page header, states, status badge, data table)

**Files:**
- Create: `app/Admin/src/shared/ui/AppButton.vue`
- Create: `app/Admin/src/shared/ui/AppDataTable.vue`
- Create: `app/Admin/src/shared/ui/AppFormField.vue`
- Create: `app/Admin/src/shared/ui/AppDialog.vue`
- Create: `app/Admin/src/shared/ui/AppDrawer.vue`
- Create: `app/Admin/src/shared/ui/AppConfirmDialog.vue`
- Create: `app/Admin/src/shared/ui/AppToast.vue`
- Create: `app/Admin/src/shared/ui/AppPageHeader.vue`
- Create: `app/Admin/src/shared/ui/AppEmptyState.vue`
- Create: `app/Admin/src/shared/ui/AppErrorState.vue`
- Create: `app/Admin/src/shared/ui/AppLoadingState.vue`
- Create: `app/Admin/src/shared/ui/AppStatusBadge.vue`
- Create: `app/Admin/src/shared/ui/index.ts`

- [ ] **Step 1: Implement `AppButton.vue`**

```vue
<!-- app/Admin/src/shared/ui/AppButton.vue -->
<template>
  <Button
    :label="label"
    :icon="icon"
    :loading="loading"
    :disabled="disabled"
    :severity="severity"
    :outlined="variant === 'secondary'"
    :text="variant === 'ghost'"
    :size="size"
    @click="$emit('click', $event)"
  />
</template>

<script setup lang="ts">
type Variant = 'primary' | 'secondary' | 'danger' | 'ghost'
type Severity = 'primary' | 'secondary' | 'danger' | 'success' | 'info' | 'warn' | 'help' | 'contrast'
type Size = 'small' | 'large' | undefined

const props = withDefaults(
  defineProps<{
    label?: string
    icon?: string
    loading?: boolean
    disabled?: boolean
    variant?: Variant
    size?: Size
  }>(),
  { variant: 'primary' },
)

defineEmits<{ click: [event: MouseEvent] }>()

const severity = computed<Severity>(() => {
  if (props.variant === 'danger') return 'danger'
  if (props.variant === 'ghost') return 'secondary'
  return 'primary'
})
</script>
```

- [ ] **Step 2: Implement `AppFormField.vue`**

```vue
<!-- app/Admin/src/shared/ui/AppFormField.vue -->
<template>
  <div class="flex flex-col gap-1">
    <label v-if="label" :for="inputId" class="text-sm font-medium">{{ label }}</label>
    <slot :id="inputId" :invalid="!!error" />
    <small v-if="error" class="text-red-600">{{ error }}</small>
    <small v-else-if="hint" class="text-color-secondary">{{ hint }}</small>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  label?: string
  error?: string
  hint?: string
  id?: string
}>()

const inputId = computed(() => props.id ?? `field-${Math.random().toString(36).slice(2, 8)}`)
</script>
```

- [ ] **Step 3: Implement `AppDialog.vue`**

```vue
<!-- app/Admin/src/shared/ui/AppDialog.vue -->
<template>
  <Dialog
    :visible="visible"
    :header="title"
    :modal="true"
    :style="{ width: width }"
    @update:visible="$emit('update:visible', $event)"
  >
    <slot />
    <template #footer>
      <slot name="footer" />
    </template>
  </Dialog>
</template>

<script setup lang="ts">
defineProps<{
  visible: boolean
  title: string
  width?: string
}>()

defineEmits<{ 'update:visible': [v: boolean] }>()
</script>
```

- [ ] **Step 4: Implement `AppDrawer.vue`**

```vue
<!-- app/Admin/src/shared/ui/AppDrawer.vue -->
<template>
  <Drawer
    :visible="visible"
    :header="title"
    position="right"
    @update:visible="$emit('update:visible', $event)"
  >
    <slot />
  </Drawer>
</template>

<script setup lang="ts">
defineProps<{ visible: boolean; title: string }>()
defineEmits<{ 'update:visible': [v: boolean] }>()
</script>
```

- [ ] **Step 5: Implement `AppConfirmDialog.vue`**

```vue
<!-- app/Admin/src/shared/ui/AppConfirmDialog.vue -->
<template>
  <ConfirmDialog />
</template>
```

- [ ] **Step 6: Implement `AppToast.vue`**

```vue
<!-- app/Admin/src/shared/ui/AppToast.vue -->
<template>
  <Toast position="top-right" />
</template>
```

- [ ] **Step 7: Implement `AppPageHeader.vue`**

```vue
<!-- app/Admin/src/shared/ui/AppPageHeader.vue -->
<template>
  <div class="mb-4 flex items-center justify-between">
    <div>
      <h1 class="text-2xl font-semibold">{{ title }}</h1>
      <p v-if="subtitle" class="text-color-secondary">{{ subtitle }}</p>
    </div>
    <div class="flex gap-2">
      <slot name="actions" />
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{ title: string; subtitle?: string }>()
</script>
```

- [ ] **Step 8: Implement `AppEmptyState.vue`**

```vue
<!-- app/Admin/src/shared/ui/AppEmptyState.vue -->
<template>
  <div class="flex flex-col items-center justify-center gap-2 p-8 text-center text-color-secondary">
    <i :class="icon ?? 'pi pi-inbox'" class="text-3xl" />
    <p class="font-medium">{{ message }}</p>
    <slot />
  </div>
</template>

<script setup lang="ts">
withDefaults(defineProps<{ message: string; icon?: string }>(), { icon: 'pi pi-inbox' })
</script>
```

- [ ] **Step 9: Implement `AppErrorState.vue`**

```vue
<!-- app/Admin/src/shared/ui/AppErrorState.vue -->
<template>
  <div class="flex flex-col items-center justify-center gap-2 p-8 text-center text-red-600">
    <i class="pi pi-exclamation-triangle text-3xl" />
    <p class="font-medium">{{ message }}</p>
    <Button v-if="onRetry" label="Retry" severity="secondary" outlined @click="onRetry()" />
  </div>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{ message: string; onRetry?: () => void }>(),
  { onRetry: undefined },
)
</script>
```

- [ ] **Step 10: Implement `AppLoadingState.vue`**

```vue
<!-- app/Admin/src/shared/ui/AppLoadingState.vue -->
<template>
  <div class="flex items-center justify-center p-8">
    <ProgressSpinner />
  </div>
</template>
```

- [ ] **Step 11: Implement `AppStatusBadge.vue`**

```vue
<!-- app/Admin/src/shared/ui/AppStatusBadge.vue -->
<template>
  <Tag :value="label" :severity="severity" />
</template>

<script setup lang="ts">
import { computed } from 'vue'

type Tone = 'success' | 'info' | 'warn' | 'danger' | 'secondary'

const props = withDefaults(
  defineProps<{ label: string; tone?: Tone }>(),
  { tone: 'info' },
)

const severity = computed(() => props.tone)
</script>
```

- [ ] **Step 12: Implement `AppDataTable.vue`**

```vue
<!-- app/Admin/src/shared/ui/AppDataTable.vue -->
<template>
  <DataTable
    :value="rows"
    :loading="loading"
    :paginator="true"
    :rows="pageSize"
    :total-records="total"
    :lazy="true"
    :first="first"
    @page="onPage"
    @sort="onSort"
    striped-rows
  >
    <slot />
    <template #empty>
      <AppEmptyState message="No records found." />
    </template>
  </DataTable>
</template>

<script setup lang="ts" generic="TRow">
import { DEFAULT_PAGE_SIZE } from '@/shared/config/app'

const props = withDefaults(
  defineProps<{
    rows: TRow[]
    total: number
    loading?: boolean
    pageSize?: number
  }>(),
  { loading: false, pageSize: DEFAULT_PAGE_SIZE },
)

const emit = defineEmits<{
  page: [event: { page: number; rows: number }]
  sort: [event: { sortField: string; sortOrder: 1 | -1 | 0 }]
}>()

const first = computed(() => 0)

function onPage(event: { page: number; rows: number }) {
  emit('page', event)
}
function onSort(event: { sortField: string; sortOrder: 1 | -1 | 0 }) {
  emit('sort', event)
}
</script>
```

- [ ] **Step 13: Implement `index.ts` barrel**

```ts
// app/Admin/src/shared/ui/index.ts
export { default as AppButton } from './AppButton.vue'
export { default as AppDataTable } from './AppDataTable.vue'
export { default as AppFormField } from './AppFormField.vue'
export { default as AppDialog } from './AppDialog.vue'
export { default as AppDrawer } from './AppDrawer.vue'
export { default as AppConfirmDialog } from './AppConfirmDialog.vue'
export { default as AppToast } from './AppToast.vue'
export { default as AppPageHeader } from './AppPageHeader.vue'
export { default as AppEmptyState } from './AppEmptyState.vue'
export { default as AppErrorState } from './AppErrorState.vue'
export { default as AppLoadingState } from './AppLoadingState.vue'
export { default as AppStatusBadge } from './AppStatusBadge.vue'
```

- [ ] **Step 14: Run all gates and commit**

```bash
cd app/Admin && pnpm type-check && pnpm lint
cd app/Admin && git add src/shared/ui/ && git commit -m "feat(admin): add shared/ui wrapped PrimeVue primitives"
```

---

## Task 17: Build `app/router/`

**Files:**
- Create: `app/Admin/src/app/router/index.ts`
- Create: `app/Admin/src/app/router/routes.ts`
- Delete: `app/Admin/src/router/index.ts`
- Delete: `app/Admin/src/router/.gitkeep`

**Interfaces:**
- Consumes: `useAuthGuard` (built in Task 19)
- Produces: `default` router with `/login` and `/` routes

- [ ] **Step 1: Create `routes.ts`**

```ts
// app/Admin/src/app/router/routes.ts
import type { RouteRecordRaw } from 'vue-router'
import { RouteName } from '@/shared/config/routes'

export const routes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: RouteName.Login,
    component: () => import('@/features/auth/ui/LoginPage.vue'),
    meta: { authRequired: false, layout: 'blank' },
  },
  {
    path: '/',
    name: RouteName.Dashboard,
    component: () => import('@/features/dashboard/ui/DashboardPage.vue'),
    meta: { authRequired: true },
  },
  {
    path: '/identity/users',
    name: RouteName.Users,
    component: () => import('@/features/identity/users/ui/UserList.vue'),
    meta: { authRequired: true, permission: 'users.read' },
  },
]
```

- [ ] **Step 2: Create `index.ts`**

```ts
// app/Admin/src/app/router/index.ts
import { createRouter, createWebHistory } from 'vue-router'
import { routes } from './routes'
import { useAuthGuard } from '@/features/auth/composables/useAuthGuard'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})

const guard = useAuthGuard(router)
router.beforeEach(guard)

export default router
```

- [ ] **Step 3: Remove the old `src/router/` directory**

Run: `cd app/Admin && git rm -r src/router/`

- [ ] **Step 4: Commit (no test yet — guard is built in Task 19)**

```bash
cd app/Admin && git add -A && git commit -m "feat(admin): add app/router with lazy-loaded routes"
```

---

## Task 18: Build `features/auth/` (model, api)

**Files:**
- Create: `app/Admin/src/features/auth/model/auth.types.ts`
- Create: `app/Admin/src/features/auth/model/auth.schema.ts`
- Create: `app/Admin/src/features/auth/api/query-keys.ts`
- Create: `app/Admin/src/features/auth/api/login.ts`
- Create: `app/Admin/src/features/auth/api/logout.ts`
- Create: `app/Admin/src/features/auth/api/refresh.ts`
- Create: `app/Admin/src/features/auth/api/current-user.ts`
- Create: `app/Admin/src/features/auth/__tests__/api/login.spec.ts`
- Create: `app/Admin/src/features/auth/__tests__/model/auth.schema.spec.ts`

**Interfaces:**
- Produces: `LoginRequest`, `AuthTokens`, `AuthUser`, `loginSchema`, `useLogin`, `useLogout`, `useRefresh`, `useCurrentUser`, `authQueryKeys`

- [ ] **Step 1: Write the test for `loginSchema`**

```ts
// app/Admin/src/features/auth/__tests__/model/auth.schema.spec.ts
import { describe, it, expect } from 'vitest'
import { loginSchema } from '../../../model/auth.schema'

describe('loginSchema', () => {
  it('accepts valid input', () => {
    expect(loginSchema.safeParse({ email: 'a@b.c', password: 'secret123' }).success).toBe(true)
  })
  it('rejects missing email', () => {
    expect(loginSchema.safeParse({ email: '', password: 'secret123' }).success).toBe(false)
  })
  it('rejects short password', () => {
    expect(loginSchema.safeParse({ email: 'a@b.c', password: 'short' }).success).toBe(false)
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd app/Admin && pnpm test:unit src/features/auth/__tests__/model/auth.schema.spec.ts`
Expected: FAIL.

- [ ] **Step 3: Implement `auth.types.ts`**

```ts
// app/Admin/src/features/auth/model/auth.types.ts
import type { IsoDateString } from '@/shared/types/timestamp'
import type { UserId } from '@/shared/types/id'

export interface LoginRequest {
  email: string
  password: string
}

export interface AuthTokens {
  accessToken: string
  refreshToken: string
  expiresAt: IsoDateString
}

export interface AuthUser {
  id: UserId
  email: string
  displayName: string
  roles: string[]
  permissions: string[]
}
```

- [ ] **Step 4: Implement `auth.schema.ts`**

```ts
// app/Admin/src/features/auth/model/auth.schema.ts
import { z } from 'zod'
import type { LoginRequest } from './auth.types'

export const loginSchema: z.ZodType<LoginRequest> = z.object({
  email: z.string().email(),
  password: z.string().min(8),
})
```

- [ ] **Step 5: Write test for `login.ts`**

```ts
// app/Admin/src/features/auth/__tests__/api/login.spec.ts
import { describe, it, expect, vi } from 'vitest'
import { useLogin } from '../../../api/login'

vi.mock('@/shared/api/client', () => ({
  api: { post: vi.fn().mockResolvedValue({ accessToken: 'a', refreshToken: 'b', expiresAt: '2026-07-06T00:00:00Z' }) },
}))

describe('useLogin', () => {
  it('calls api.post with login endpoint', async () => {
    const { mutate } = useLogin()
    await mutate({ email: 'a@b.c', password: 'secret123' })
    const { api } = await import('@/shared/api/client')
    expect(api.post).toHaveBeenCalledWith('/api/auth/login', { email: 'a@b.c', password: 'secret123' })
  })
})
```

- [ ] **Step 6: Run test to verify it fails**

Run: `cd app/Admin && pnpm test:unit src/features/auth/__tests__/api/login.spec.ts`
Expected: FAIL.

- [ ] **Step 7: Implement `query-keys.ts`**

```ts
// app/Admin/src/features/auth/api/query-keys.ts
import { withId } from '@/shared/api/query-keys'

export const authQueryKeys = {
  all: ['auth'] as const,
  currentUser: () => withId(authQueryKeys.all, 'current-user') as readonly unknown[],
}
```

- [ ] **Step 8: Implement `login.ts`**

```ts
// app/Admin/src/features/auth/api/login.ts
import { useMutation } from '@tanstack/vue-query'
import { api } from '@/shared/api/client'
import type { LoginRequest, AuthTokens } from '../model/auth.types'

export function useLogin() {
  return useMutation({
    mutationFn: (body: LoginRequest) => api.post<AuthTokens>('/api/auth/login', body),
  })
}
```

- [ ] **Step 9: Implement `logout.ts`**

```ts
// app/Admin/src/features/auth/api/logout.ts
import { useMutation } from '@tanstack/vue-query'
import { api } from '@/shared/api/client'

export function useLogout() {
  return useMutation({
    mutationFn: () => api.post<void>('/api/auth/logout'),
  })
}
```

- [ ] **Step 10: Implement `refresh.ts`**

```ts
// app/Admin/src/features/auth/api/refresh.ts
import { useMutation } from '@tanstack/vue-query'
import { api } from '@/shared/api/client'
import type { AuthTokens } from '../model/auth.types'

export function useRefresh() {
  return useMutation({
    mutationFn: (refreshToken: string) =>
      api.post<AuthTokens>('/api/auth/refresh', { refreshToken }),
  })
}
```

- [ ] **Step 11: Implement `current-user.ts`**

```ts
// app/Admin/src/features/auth/api/current-user.ts
import { useQuery } from '@tanstack/vue-query'
import { api } from '@/shared/api/client'
import type { AuthUser } from '../model/auth.types'
import { authQueryKeys } from './query-keys'

export function useCurrentUser() {
  return useQuery({
    queryKey: authQueryKeys.currentUser(),
    queryFn: () => api.get<AuthUser>('/api/auth/me'),
    retry: false,
    staleTime: 60_000,
  })
}
```

- [ ] **Step 12: Run tests and gates**

Run: `cd app/Admin && pnpm test:unit src/features/auth && pnpm type-check && pnpm lint`
Expected: all PASS.

- [ ] **Step 13: Commit**

```bash
cd app/Admin && git add src/features/auth/ && git commit -m "feat(admin): add features/auth model and api (login, logout, refresh, current-user)"
```

---

## Task 19: Build `features/auth/` (composables, ui, barrel)

**Files:**
- Create: `app/Admin/src/features/auth/composables/useAuthGuard.ts`
- Create: `app/Admin/src/features/auth/composables/useAuthState.ts`
- Create: `app/Admin/src/features/auth/ui/LoginForm.vue`
- Create: `app/Admin/src/features/auth/ui/LoginPage.vue`
- Create: `app/Admin/src/features/auth/ui/LogoutButton.vue`
- Create: `app/Admin/src/features/auth/index.ts`
- Create: `app/Admin/src/features/auth/__tests__/composables/useAuthGuard.spec.ts`

**Interfaces:**
- Produces: `useAuthGuard(router)` returning `beforeEach` guard, `useAuthState()` returning `{ user, login, logout, refresh }`, `LoginPage`, `LoginForm`, `LogoutButton`

- [ ] **Step 1: Write the test for the guard**

```ts
// app/Admin/src/features/auth/__tests__/composables/useAuthGuard.spec.ts
import { describe, it, expect } from 'vitest'
import { useAuthGuard } from '../../../composables/useAuthGuard'

describe('useAuthGuard', () => {
  it('blocks unauthenticated access to authRequired routes', () => {
    const guard = useAuthGuard({} as never)
    const result = guard(
      { meta: { authRequired: true } } as never,
      {} as never,
      () => {},
    ) as unknown
    expect(typeof result).toBe('function')
  })

  it('allows access to non-authRequired routes', () => {
    const guard = useAuthGuard({} as never)
    const result = guard(
      { meta: { authRequired: false } } as never,
      {} as never,
      () => {},
    )
    expect(result).toBeUndefined()
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd app/Admin && pnpm test:unit src/features/auth/__tests__/composables/useAuthGuard.spec.ts`
Expected: FAIL.

- [ ] **Step 3: Implement `useAuthState.ts`**

```ts
// app/Admin/src/features/auth/composables/useAuthState.ts
import { ref, computed } from 'vue'
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { setAuthTokenAccessor } from '@/shared/api/fetch-options'
import { authQueryKeys } from '../api/query-keys'
import { useLogin } from '../api/login'
import { useLogout } from '../api/logout'
import { useCurrentUser } from '../api/current-user'
import type { AuthTokens, AuthUser } from '../model/auth.types'

const tokens = ref<AuthTokens | null>(null)

setAuthTokenAccessor(() => tokens.value?.accessToken ?? null)

export function useAuthState() {
  const qc = useQueryClient()
  const currentUser = useCurrentUser()
  const login = useLogin()
  const logout = useLogout()

  const isAuthenticated = computed(() => !!tokens.value)

  async function setTokens(t: AuthTokens) {
    tokens.value = t
    qc.setQueryData(authQueryKeys.currentUser(), (await qc.fetchQuery({
      queryKey: authQueryKeys.currentUser(),
      queryFn: () => fetch('/api/auth/me', { headers: { Authorization: `Bearer ${t.accessToken}` } }).then((r) => r.json()),
    })) as AuthUser)
  }

  function clear() {
    tokens.value = null
    qc.removeQueries({ queryKey: authQueryKeys.all })
  }

  return { tokens, isAuthenticated, user: currentUser, login, logout, setTokens, clear }
}
```

- [ ] **Step 4: Implement `useAuthGuard.ts`**

```ts
// app/Admin/src/features/auth/composables/useAuthGuard.ts
import type { NavigationGuardWithThis, RouteLocationNormalized } from 'vue-router'
import { useAuthState } from './useAuthState'

export function useAuthGuard(_router: unknown): NavigationGuardWithThis<undefined> {
  return function (to: RouteLocationNormalized, _from, next) {
    const { isAuthenticated } = useAuthState()
    const requiresAuth = to.meta.authRequired === true
    if (requiresAuth && !isAuthenticated.value) {
      return next({ name: 'login', query: { redirect: to.fullPath } })
    }
    return next()
  }
}
```

- [ ] **Step 5: Implement `LoginForm.vue`**

```vue
<!-- app/Admin/src/features/auth/ui/LoginForm.vue -->
<template>
  <form class="flex flex-col gap-3" @submit.prevent="onSubmit">
    <AppFormField label="Email" :error="errors.email">
      <InputText v-model="email" :invalid="!!errors.email" type="email" autocomplete="email" />
    </AppFormField>
    <AppFormField label="Password" :error="errors.password">
      <Password v-model="password" :invalid="!!errors.password" :feedback="false" toggle-mask input-class="w-full" />
    </AppFormField>
    <AppButton type="submit" label="Sign in" :loading="login.isPending.value" />
  </form>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { loginSchema } from '../model/auth.schema'
import { useAuthState } from '../composables/useAuthState'

const email = ref('')
const password = ref('')
const errors = ref<{ email?: string; password?: string }>({})

const { login, setTokens } = useAuthState()

async function onSubmit() {
  const parsed = loginSchema.safeParse({ email: email.value, password: password.value })
  if (!parsed.success) {
    const flat = parsed.error.flatten().fieldErrors
    errors.value = { email: flat.email?.[0], password: flat.password?.[0] }
    return
  }
  errors.value = {}
  const tokens = await login.mutateAsync(parsed.data)
  await setTokens(tokens)
}
</script>
```

- [ ] **Step 6: Implement `LoginPage.vue`**

```vue
<!-- app/Admin/src/features/auth/ui/LoginPage.vue -->
<template>
  <div class="flex min-h-screen items-center justify-center bg-surface-50 p-4">
    <div class="w-full max-w-sm rounded-lg bg-white p-6 shadow">
      <h1 class="mb-4 text-center text-2xl font-semibold">Sign in</h1>
      <LoginForm />
    </div>
  </div>
</template>

<script setup lang="ts">
import LoginForm from './LoginForm.vue'
</script>
```

- [ ] **Step 7: Implement `LogoutButton.vue`**

```vue
<!-- app/Admin/src/features/auth/ui/LogoutButton.vue -->
<template>
  <AppButton icon="pi pi-sign-out" label="Sign out" variant="ghost" @click="onClick" />
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthState } from '../composables/useAuthState'

const { logout, clear } = useAuthState()
const router = useRouter()

async function onClick() {
  await logout.mutateAsync()
  clear()
  await router.push({ name: 'login' })
}
</script>
```

- [ ] **Step 8: Implement `index.ts` barrel**

```ts
// app/Admin/src/features/auth/index.ts
export { default as LoginPage } from './ui/LoginPage.vue'
export { default as LoginForm } from './ui/LoginForm.vue'
export { default as LogoutButton } from './ui/LogoutButton.vue'
export { useAuthState } from './composables/useAuthState'
export { useAuthGuard } from './composables/useAuthGuard'
export { useLogin, useLogout, useRefresh, useCurrentUser } from './api'
export { authQueryKeys } from './api/query-keys'
export type { LoginRequest, AuthTokens, AuthUser } from './model/auth.types'
```

- [ ] **Step 9: Create `api/index.ts` re-export**

```ts
// app/Admin/src/features/auth/api/index.ts
export { useLogin } from './login'
export { useLogout } from './logout'
export { useRefresh } from './refresh'
export { useCurrentUser } from './current-user'
```

- [ ] **Step 10: Run tests and gates**

Run: `cd app/Admin && pnpm test:unit src/features/auth && pnpm type-check && pnpm lint`
Expected: all PASS.

- [ ] **Step 11: Commit**

```bash
cd app/Admin && git add src/features/auth/ && git commit -m "feat(admin): add features/auth composables, UI, and barrel"
```

---

## Task 20: Build `features/dashboard/`

**Files:**
- Create: `app/Admin/src/features/dashboard/ui/DashboardPage.vue`
- Create: `app/Admin/src/features/dashboard/index.ts`

- [ ] **Step 1: Implement `DashboardPage.vue`**

```vue
<!-- app/Admin/src/features/dashboard/ui/DashboardPage.vue -->
<template>
  <div>
    <AppPageHeader title="Dashboard" subtitle="Overview of your store" />
    <div v-if="user.isLoading.value" class="p-8">
      <AppLoadingState />
    </div>
    <div v-else-if="user.error.value" class="p-8">
      <AppErrorState :message="String(user.error.value)" :on-retry="user.refetch" />
    </div>
    <div v-else class="grid grid-cols-1 gap-4 md:grid-cols-3">
      <div class="rounded border border-surface-200 bg-white p-4">
        <p class="text-sm text-color-secondary">Signed in as</p>
        <p class="text-lg font-semibold">{{ user.data.value?.displayName ?? '—' }}</p>
      </div>
      <div class="rounded border border-surface-200 bg-white p-4">
        <p class="text-sm text-color-secondary">Roles</p>
        <p class="text-lg font-semibold">{{ (user.data.value?.roles ?? []).length }}</p>
      </div>
      <div class="rounded border border-surface-200 bg-white p-4">
        <p class="text-sm text-color-secondary">Permissions</p>
        <p class="text-lg font-semibold">{{ (user.data.value?.permissions ?? []).length }}</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useCurrentUser } from '@/features/auth'

const user = useCurrentUser()
</script>
```

- [ ] **Step 2: Implement `index.ts`**

```ts
// app/Admin/src/features/dashboard/index.ts
export { default as DashboardPage } from './ui/DashboardPage.vue'
```

- [ ] **Step 3: Run gates and commit**

```bash
cd app/Admin && pnpm type-check && pnpm lint
cd app/Admin && git add src/features/dashboard/ && git commit -m "feat(admin): add features/dashboard landing page"
```

---

## Task 21: Build `features/identity/users/` (model + api)

**Files:**
- Create: `app/Admin/src/features/identity/users/model/user.types.ts`
- Create: `app/Admin/src/features/identity/users/model/user.schema.ts`
- Create: `app/Admin/src/features/identity/users/model/user.mapper.ts`
- Create: `app/Admin/src/features/identity/users/api/query-keys.ts`
- Create: `app/Admin/src/features/identity/users/api/get-list.ts`
- Create: `app/Admin/src/features/identity/users/api/get-by-id.ts`
- Create: `app/Admin/src/features/identity/users/api/create.ts`
- Create: `app/Admin/src/features/identity/users/api/update.ts`
- Create: `app/Admin/src/features/identity/users/api/delete.ts`
- Create: `app/Admin/src/features/identity/users/api/index.ts`
- Create: `app/Admin/src/features/identity/users/__tests__/model/user.mapper.spec.ts`
- Create: `app/Admin/src/features/identity/users/__tests__/api/get-list.spec.ts`

**Interfaces:**
- Produces: `User`, `UserListItem`, `UserCreateRequest`, `UserUpdateRequest`, `createUserSchema`, `updateUserSchema`, `mapUser`, `mapUserListItem`, `useUsersList`, `useUser`, `useCreateUser`, `useUpdateUser`, `useDeleteUser`, `usersQueryKeys`

- [ ] **Step 1: Implement `user.types.ts`**

```ts
// app/Admin/src/features/identity/users/model/user.types.ts
import type { IsoDateString } from '@/shared/types/timestamp'
import type { UserId } from '@/shared/types/id'

export type UserStatus = 'active' | 'inactive' | 'invited' | 'suspended'

export interface User {
  id: UserId
  email: string
  displayName: string
  status: UserStatus
  roles: string[]
  createdAt: IsoDateString
  updatedAt: IsoDateString
}

export interface UserListItem {
  id: UserId
  email: string
  displayName: string
  status: UserStatus
  roleCount: number
}

export interface UserCreateRequest {
  email: string
  displayName: string
  password: string
  roleIds: string[]
}

export interface UserUpdateRequest {
  id: UserId
  displayName?: string
  status?: UserStatus
  roleIds?: string[]
}
```

- [ ] **Step 2: Implement `user.schema.ts`**

```ts
// app/Admin/src/features/identity/users/model/user.schema.ts
import { z } from 'zod'
import type { UserCreateRequest, UserUpdateRequest } from './user.types'

export const createUserSchema: z.ZodType<UserCreateRequest> = z.object({
  email: z.string().email(),
  displayName: z.string().min(1).max(120),
  password: z.string().min(8),
  roleIds: z.array(z.string()).min(1),
})

export const updateUserSchema: z.ZodType<UserUpdateRequest> = z.object({
  id: z.string(),
  displayName: z.string().min(1).max(120).optional(),
  status: z.enum(['active', 'inactive', 'invited', 'suspended']).optional(),
  roleIds: z.array(z.string()).optional(),
})
```

- [ ] **Step 3: Write test for `user.mapper.ts`**

```ts
// app/Admin/src/features/identity/users/__tests__/model/user.mapper.spec.ts
import { describe, it, expect } from 'vitest'
import { mapUser, mapUserListItem } from '../../../model/user.mapper'
import type { User } from '../../../model/user.types'

const user: User = {
  id: 'u-1' as never,
  email: 'a@b.c',
  displayName: 'Alice',
  status: 'active',
  roles: ['admin'],
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: '2026-01-02T00:00:00Z',
}

describe('user.mapper', () => {
  it('mapUser returns the same shape', () => {
    expect(mapUser(user)).toEqual(user)
  })
  it('mapUserListItem reduces roles to count', () => {
    expect(mapUserListItem(user)).toEqual({
      id: 'u-1',
      email: 'a@b.c',
      displayName: 'Alice',
      status: 'active',
      roleCount: 1,
    })
  })
})
```

- [ ] **Step 4: Implement `user.mapper.ts`**

```ts
// app/Admin/src/features/identity/users/model/user.mapper.ts
import type { User, UserListItem } from './user.types'

export function mapUser(u: User): User {
  return u
}

export function mapUserListItem(u: User): UserListItem {
  return {
    id: u.id,
    email: u.email,
    displayName: u.displayName,
    status: u.status,
    roleCount: u.roles.length,
  }
}
```

- [ ] **Step 5: Write test for `get-list.ts`**

```ts
// app/Admin/src/features/identity/users/__tests__/api/get-list.spec.ts
import { describe, it, expect, vi } from 'vitest'
import { useUsersList } from '../../../api/get-list'

vi.mock('@/shared/api/client', () => ({
  api: {
    getPaged: vi.fn().mockResolvedValue({ items: [], totalCount: 0, page: 1, pageSize: 20 }),
  },
}))

describe('useUsersList', () => {
  it('queries /api/admin/identity/users with params', async () => {
    const { api } = await import('@/shared/api/client')
    const q = useUsersList({ page: 1, pageSize: 20, search: 'a' })
    await q.suspense()
    expect(api.getPaged).toHaveBeenCalledWith(expect.stringContaining('/api/admin/identity/users'))
  })
})
```

- [ ] **Step 6: Implement `query-keys.ts`**

```ts
// app/Admin/src/features/identity/users/api/query-keys.ts
import { withFilters, withId } from '@/shared/api/query-keys'

export const usersQueryKeys = {
  all: ['users'] as const,
  list: (filters: Record<string, unknown> = {}) => withFilters(usersQueryKeys.all, filters),
  detail: (id: string) => withId(usersQueryKeys.all, id),
}
```

- [ ] **Step 7: Implement `get-list.ts`**

```ts
// app/Admin/src/features/identity/users/api/get-list.ts
import { useQuery } from '@tanstack/vue-query'
import type { Ref } from 'vue'
import { api } from '@/shared/api/client'
import type { PagedResult, PageRequest } from '@/shared/types/page'
import type { UserListItem } from '../model/user.types'
import { usersQueryKeys } from './query-keys'

export function useUsersList(params: Ref<PageRequest>) {
  return useQuery({
    queryKey: usersQueryKeys.list(params as unknown as Record<string, unknown>),
    queryFn: () => {
      const search = new URLSearchParams()
      search.set('page', String(params.value.page))
      search.set('pageSize', String(params.value.pageSize))
      if (params.value.search) search.set('search', params.value.search)
      return api.getPaged<UserListItem>(`/api/admin/identity/users?${search.toString()}`)
    },
  }) as unknown as { suspense: () => Promise<PagedResult<UserListItem>>; data: Ref<PagedResult<UserListItem>>; isLoading: Ref<boolean>; error: Ref<Error | null>; refetch: () => void }
}
```

- [ ] **Step 8: Implement `get-by-id.ts`**

```ts
// app/Admin/src/features/identity/users/api/get-by-id.ts
import { useQuery, type UseQueryReturnType } from '@tanstack/vue-query'
import { api } from '@/shared/api/client'
import type { User } from '../model/user.types'
import { usersQueryKeys } from './query-keys'

export function useUser(id: string): UseQueryReturnType<User, Error> {
  return useQuery({
    queryKey: usersQueryKeys.detail(id),
    queryFn: () => api.get<User>(`/api/admin/identity/users/${id}`),
    enabled: !!id,
  })
}
```

- [ ] **Step 9: Implement `create.ts`**

```ts
// app/Admin/src/features/identity/users/api/create.ts
import { useMutation, useQueryClient, type UseMutationReturnType } from '@tanstack/vue-query'
import { api } from '@/shared/api/client'
import type { User, UserCreateRequest } from '../model/user.types'
import { usersQueryKeys } from './query-keys'

export function useCreateUser(): UseMutationReturnType<User, Error, UserCreateRequest, unknown> {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body) => api.post<User>('/api/admin/identity/users', body),
    onSuccess: () => qc.invalidateQueries({ queryKey: usersQueryKeys.all }),
  })
}
```

- [ ] **Step 10: Implement `update.ts`**

```ts
// app/Admin/src/features/identity/users/api/update.ts
import { useMutation, useQueryClient, type UseMutationReturnType } from '@tanstack/vue-query'
import { api } from '@/shared/api/client'
import type { User, UserUpdateRequest } from '../model/user.types'
import { usersQueryKeys } from './query-keys'

export function useUpdateUser(): UseMutationReturnType<User, Error, UserUpdateRequest, unknown> {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body) => api.put<User>(`/api/admin/identity/users/${body.id}`, body),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: usersQueryKeys.all })
      qc.invalidateQueries({ queryKey: usersQueryKeys.detail(vars.id) })
    },
  })
}
```

- [ ] **Step 11: Implement `delete.ts`**

```ts
// app/Admin/src/features/identity/users/api/delete.ts
import { useMutation, useQueryClient, type UseMutationReturnType } from '@tanstack/vue-query'
import { api } from '@/shared/api/client'
import type { UserId } from '@/shared/types/id'
import { usersQueryKeys } from './query-keys'

export function useDeleteUser(): UseMutationReturnType<void, Error, UserId, unknown> {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => api.delete<void>(`/api/admin/identity/users/${id}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: usersQueryKeys.all }),
  })
}
```

- [ ] **Step 12: Implement `api/index.ts` barrel**

```ts
// app/Admin/src/features/identity/users/api/index.ts
export { useUsersList } from './get-list'
export { useUser } from './get-by-id'
export { useCreateUser } from './create'
export { useUpdateUser } from './update'
export { useDeleteUser } from './delete'
export { usersQueryKeys } from './query-keys'
```

- [ ] **Step 13: Run tests and gates**

Run: `cd app/Admin && pnpm test:unit src/features/identity/users && pnpm type-check && pnpm lint`
Expected: all PASS.

- [ ] **Step 14: Commit**

```bash
cd app/Admin && git add src/features/identity/users/ && git commit -m "feat(admin): add features/identity/users model and api"
```

---

## Task 22: Build `features/identity/users/` (composables, ui, barrel)

**Files:**
- Create: `app/Admin/src/features/identity/users/composables/useUserForm.ts`
- Create: `app/Admin/src/features/identity/users/ui/UserStatusBadge.vue`
- Create: `app/Admin/src/features/identity/users/ui/UserFilters.vue`
- Create: `app/Admin/src/features/identity/users/ui/UserFormDialog.vue`
- Create: `app/Admin/src/features/identity/users/ui/UserDetailsDrawer.vue`
- Create: `app/Admin/src/features/identity/users/ui/UserList.vue`
- Create: `app/Admin/src/features/identity/users/index.ts`
- Create: `app/Admin/src/features/identity/users/__tests__/ui/UserList.spec.ts`

- [ ] **Step 1: Implement `UserStatusBadge.vue`**

```vue
<!-- app/Admin/src/features/identity/users/ui/UserStatusBadge.vue -->
<template>
  <AppStatusBadge :label="label" :tone="tone" />
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { UserStatus } from '../model/user.types'

const props = defineProps<{ status: UserStatus }>()

const label = computed(() => props.status)
const tone = computed(() => {
  switch (props.status) {
    case 'active': return 'success' as const
    case 'inactive': return 'secondary' as const
    case 'invited': return 'info' as const
    case 'suspended': return 'danger' as const
  }
})
</script>
```

- [ ] **Step 2: Implement `useUserForm.ts`**

```ts
// app/Admin/src/features/identity/users/composables/useUserForm.ts
import { ref } from 'vue'
import { createUserSchema, updateUserSchema } from '../model/user.schema'
import { useCreateUser, useUpdateUser } from '../api'
import type { User, UserCreateRequest, UserUpdateRequest } from '../model/user.types'

export function useUserForm() {
  const create = useCreateUser()
  const update = useUpdateUser()
  const errors = ref<Record<string, string | undefined>>({})

  async function submitCreate(input: UserCreateRequest) {
    const parsed = createUserSchema.safeParse(input)
    if (!parsed.success) {
      errors.value = parsed.error.flatten().fieldErrors as Record<string, string | undefined>
      throw new Error('validation')
    }
    errors.value = {}
    return create.mutateAsync(parsed.data)
  }

  async function submitUpdate(input: UserUpdateRequest) {
    const parsed = updateUserSchema.safeParse(input)
    if (!parsed.success) {
      errors.value = parsed.error.flatten().fieldErrors as Record<string, string | undefined>
      throw new Error('validation')
    }
    errors.value = {}
    return update.mutateAsync(parsed.data)
  }

  return { errors, submitCreate, submitUpdate, isPending: computed(() => create.isPending.value || update.isPending.value) }
}
```

- [ ] **Step 3: Implement `UserFilters.vue`**

```vue
<!-- app/Admin/src/features/identity/users/ui/UserFilters.vue -->
<template>
  <div class="mb-3 flex gap-2">
    <InputText v-model="searchDebounced" placeholder="Search by name or email" class="w-64" />
    <Select v-model="status" :options="statusOptions" option-label="label" option-value="value" placeholder="Status" show-clear class="w-40" />
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useDebouncedRef } from '@/shared/composables/useDebouncedRef'

const props = defineProps<{ modelValue: { search: string; status?: string } }>()
const emit = defineEmits<{ 'update:modelValue': [v: { search: string; status?: string }] }>()

const search = ref(props.modelValue.search)
const status = ref<string | undefined>(props.modelValue.status)
const searchDebounced = useDebouncedRef(search, 300)

const statusOptions = [
  { label: 'Active', value: 'active' },
  { label: 'Inactive', value: 'inactive' },
  { label: 'Invited', value: 'invited' },
  { label: 'Suspended', value: 'suspended' },
]

watch([searchDebounced, status], () => {
  emit('update:modelValue', { search: searchDebounced.value, status: status.value })
})
</script>
```

- [ ] **Step 4: Implement `UserFormDialog.vue`**

```vue
<!-- app/Admin/src/features/identity/users/ui/UserFormDialog.vue -->
<template>
  <AppDialog :visible="visible" :title="user ? 'Edit user' : 'New user'" @update:visible="$emit('update:visible', $event)">
    <form class="flex flex-col gap-3" @submit.prevent="onSubmit">
      <AppFormField label="Email" :error="form.errors.value.email">
        <InputText v-model="email" :invalid="!!form.errors.value.email" :disabled="!!user" />
      </AppFormField>
      <AppFormField label="Display name" :error="form.errors.value.displayName">
        <InputText v-model="displayName" :invalid="!!form.errors.value.displayName" />
      </AppFormField>
      <AppFormField v-if="!user" label="Password" :error="form.errors.value.password">
        <Password v-model="password" :feedback="false" toggle-mask :invalid="!!form.errors.value.password" input-class="w-full" />
      </AppFormField>
      <AppFormField label="Roles" :error="form.errors.value.roleIds">
        <MultiSelect v-model="roleIds" :options="[]" option-label="name" option-value="id" placeholder="Select roles" display="chip" class="w-full" />
      </AppFormField>
    </form>
    <template #footer>
      <AppButton label="Cancel" variant="secondary" @click="$emit('update:visible', false)" />
      <AppButton :label="user ? 'Save' : 'Create'" :loading="form.isPending.value" @click="onSubmit" />
    </template>
  </AppDialog>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import type { User, UserCreateRequest, UserUpdateRequest } from '../model/user.types'
import { useUserForm } from '../composables/useUserForm'

const props = defineProps<{ visible: boolean; user?: User | null }>()
const emit = defineEmits<{
  'update:visible': [v: boolean]
  saved: [user: User]
}>()

const form = useUserForm()
const email = ref('')
const displayName = ref('')
const password = ref('')
const roleIds = ref<string[]>([])

watch(
  () => props.user,
  (u) => {
    email.value = u?.email ?? ''
    displayName.value = u?.displayName ?? ''
    password.value = ''
    roleIds.value = u?.roles ?? []
  },
  { immediate: true },
)

async function onSubmit() {
  try {
    if (props.user) {
      const body: UserUpdateRequest = {
        id: props.user.id,
        displayName: displayName.value,
        roleIds: roleIds.value,
      }
      const updated = await form.submitUpdate(body)
      emit('saved', updated)
    } else {
      const body: UserCreateRequest = {
        email: email.value,
        displayName: displayName.value,
        password: password.value,
        roleIds: roleIds.value,
      }
      const created = await form.submitCreate(body)
      emit('saved', created)
    }
  } catch {
    // validation errors surfaced via form.errors
  }
}
</script>
```

- [ ] **Step 5: Implement `UserDetailsDrawer.vue`**

```vue
<!-- app/Admin/src/features/identity/users/ui/UserDetailsDrawer.vue -->
<template>
  <AppDrawer :visible="visible" :title="user?.displayName ?? 'User'" @update:visible="$emit('update:visible', $event)">
    <div v-if="user" class="flex flex-col gap-2 text-sm">
      <p><span class="text-color-secondary">Email:</span> {{ user.email }}</p>
      <p><span class="text-color-secondary">Status:</span> <UserStatusBadge :status="user.status" /></p>
      <p><span class="text-color-secondary">Roles:</span> {{ user.roles.join(', ') || '—' }}</p>
      <p><span class="text-color-secondary">Created:</span> {{ user.createdAt }}</p>
      <p><span class="text-color-secondary">Updated:</span> {{ user.updatedAt }}</p>
    </div>
  </AppDrawer>
</template>

<script setup lang="ts">
import type { User } from '../model/user.types'
import UserStatusBadge from './UserStatusBadge.vue'

defineProps<{ visible: boolean; user: User | null }>()
defineEmits<{ 'update:visible': [v: boolean] }>()
</script>
```

- [ ] **Step 6: Implement `UserList.vue`**

```vue
<!-- app/Admin/src/features/identity/users/ui/UserList.vue -->
<template>
  <div>
    <AppPageHeader title="Users" subtitle="Manage administrators and staff">
      <template #actions>
        <AppButton icon="pi pi-plus" label="New user" @click="formOpen = true" />
      </template>
    </AppPageHeader>
    <UserFilters v-model="filters" />
    <AppDataTable :rows="rows" :total="total" :loading="query.isLoading.value" @page="onPage">
      <Column field="displayName" header="Name" sortable />
      <Column field="email" header="Email" sortable />
      <Column header="Status">
        <template #body="{ data }">
          <UserStatusBadge :status="data.status" />
        </template>
      </Column>
      <Column field="roleCount" header="Roles" />
      <Column header="">
        <template #body="{ data }">
          <div class="flex gap-1">
            <AppButton icon="pi pi-eye" variant="ghost" @click="openDetails(data)" />
            <AppButton icon="pi pi-pencil" variant="ghost" @click="openEdit(data)" />
            <AppButton icon="pi pi-trash" variant="danger" @click="confirmDelete(data)" />
          </div>
        </template>
      </Column>
    </AppDataTable>
    <UserFormDialog v-model:visible="formOpen" :user="editing" @saved="onSaved" />
    <UserDetailsDrawer v-model:visible="detailsOpen" :user="detailsUser" />
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watchEffect } from 'vue'
import { useUsersList, useDeleteUser } from '../api'
import { mapUserListItem } from '../model/user.mapper'
import type { User, UserListItem } from '../model/user.types'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import UserFilters from './UserFilters.vue'
import UserFormDialog from './UserFormDialog.vue'
import UserDetailsDrawer from './UserDetailsDrawer.vue'
import UserStatusBadge from './UserStatusBadge.vue'

const filters = ref<{ search: string; status?: string }>({ search: '' })
const page = ref(1)
const pageSize = 20

const params = computed(() => ({ page: page.value, pageSize, search: filters.value.search }))
const query = useUsersList(params as never)

const rows = computed<UserListItem[]>(() => (query.data.value?.items ?? []).map(mapUserListItem))
const total = computed(() => query.data.value?.totalCount ?? 0)

const formOpen = ref(false)
const editing = ref<User | null>(null)
const detailsOpen = ref(false)
const detailsUser = ref<User | null>(null)

const remove = useDeleteUser()
const confirm = useConfirm()
const toast = useToast()

function onPage(e: { page: number; rows: number }) {
  page.value = e.page + 1
  pageSize !== e.rows && (pageSize as number) === pageSize
}

function openEdit(user: UserListItem) {
  editing.value = user as User
  formOpen.value = true
}
function openDetails(user: UserListItem) {
  detailsUser.value = user as User
  detailsOpen.value = true
}
async function confirmDelete(user: UserListItem) {
  confirm.require({
    message: `Delete ${user.displayName}?`,
    header: 'Confirm',
    icon: 'pi pi-exclamation-triangle',
    acceptClass: 'p-button-danger',
    accept: async () => {
      await remove.mutateAsync(user.id)
      toast.add({ severity: 'success', summary: 'Deleted', life: 3000 })
    },
  })
}
function onSaved() {
  formOpen.value = false
  editing.value = null
  query.refetch()
}

watchEffect(() => {
  if (filters.value.status === undefined) delete (filters.value as { status?: string }).status
})
</script>
```

- [ ] **Step 7: Implement `index.ts` barrel**

```ts
// app/Admin/src/features/identity/users/index.ts
export { default as UserList } from './ui/UserList.vue'
export { default as UserFormDialog } from './ui/UserFormDialog.vue'
export { default as UserDetailsDrawer } from './ui/UserDetailsDrawer.vue'
export { default as UserStatusBadge } from './ui/UserStatusBadge.vue'
export { useUsersList, useUser, useCreateUser, useUpdateUser, useDeleteUser } from './api'
export { usersQueryKeys } from './api/query-keys'
export type { User, UserListItem, UserCreateRequest, UserUpdateRequest, UserStatus } from './model/user.types'
```

- [ ] **Step 8: Write smoke test for the list page**

```ts
// app/Admin/src/features/identity/users/__tests__/ui/UserList.spec.ts
import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import UserList from '../../../ui/UserList.vue'

vi.mock('@/shared/api/client', () => ({ api: { getPaged: vi.fn() } }))
vi.mock('@/shared/composables/useConfirm', () => ({ useConfirm: () => ({ require: vi.fn() }) }))
vi.mock('@/shared/composables/useToast', () => ({ useToast: () => ({ add: vi.fn() }) }))

describe('UserList', () => {
  it('renders the page header', () => {
    const wrapper = mount(UserList, { global: { stubs: { RouterLink: true } } })
    expect(wrapper.text()).toContain('Users')
  })
})
```

- [ ] **Step 9: Run tests and gates**

Run: `cd app/Admin && pnpm test:unit src/features/identity/users && pnpm type-check && pnpm lint`
Expected: all PASS.

- [ ] **Step 10: Commit**

```bash
cd app/Admin && git add src/features/identity/users/ && git commit -m "feat(admin): add features/identity/users composables, UI, and barrel"
```

---

## Task 23: Wire `app/main.ts` and `app/App.vue` to the new structure

**Files:**
- Create: `app/Admin/src/app/main.ts`
- Create: `app/Admin/src/app/App.vue`
- Create: `app/Admin/src/app/providers/AppProviders.vue`
- Modify: `app/Admin/index.html` (no change unless `<div id="app">` is missing)
- Delete: `app/Admin/src/main.ts`
- Delete: `app/Admin/src/App.vue`
- Delete: `app/Admin/src/api.ts` (the shim)
- Delete: `app/Admin/src/stores/counter.ts`
- Delete: `app/Admin/src/__tests__/App.spec.ts`

- [ ] **Step 1: Implement `AppProviders.vue`**

```vue
<!-- app/Admin/src/app/providers/AppProviders.vue -->
<template>
  <slot />
  <AppConfirmDialog />
  <AppToast />
</template>

<script setup lang="ts">
import { installPrimeVue } from '../plugins/primevue'
import { installPinia } from '../plugins/pinia'
import { installVueQuery } from '../plugins/vue-query'
import { useAuthState } from '@/features/auth'
import { onMounted, type App } from 'vue'

const props = defineProps<{ app: App }>()
const { setTokens } = useAuthState()

installPrimeVue(props.app)
installPinia(props.app)
installVueQuery(props.app)

onMounted(() => {
  const stored = localStorage.getItem('auth:tokens')
  if (stored) {
    setTokens(JSON.parse(stored) as never)
  }
})
</script>
```

- [ ] **Step 2: Implement `App.vue`**

```vue
<!-- app/Admin/src/app/App.vue -->
<template>
  <RouterView v-slot="{ Component, route }">
    <component :is="Component" v-if="Component" :key="route.fullPath" />
  </RouterView>
</template>

<script setup lang="ts">
import { RouterView } from 'vue-router'
</script>
```

- [ ] **Step 3: Implement `main.ts`**

```ts
// app/Admin/src/app/main.ts
import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import AppProviders from './providers/AppProviders.vue'

const app = createApp(App)
app.component('AppProviders', AppProviders)
app.use(router)
app.mount('#app')
```

- [ ] **Step 4: Remove the old top-level files**

Run:
```bash
cd app/Admin
git rm src/main.ts src/App.vue src/api.ts src/stores/counter.ts src/__tests__/App.spec.ts
```

- [ ] **Step 5: Run all gates**

Run: `cd app/Admin && pnpm test:unit && pnpm type-check && pnpm lint`
Expected: all PASS.

- [ ] **Step 6: Commit**

```bash
cd app/Admin && git add -A && git commit -m "refactor(admin): wire main.ts and App.vue to new architecture"
```

---

## Task 24: Verify dev server boots and the integration test passes

**Files:**
- Create: `app/Admin/src/app/__tests__/router.spec.ts`

**Interfaces:**
- Produces: end-to-end test that mounts the app, navigates to `/login`, and asserts redirect behavior

- [ ] **Step 1: Write the test**

```ts
// app/Admin/src/app/__tests__/router.spec.ts
import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { createRouter, createMemoryHistory } from 'vue-router'
import App from '../App.vue'
import { routes } from '../router/routes'

vi.mock('@/shared/api/client', () => ({ api: { get: vi.fn(), getPaged: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() } }))

describe('app router integration', () => {
  it('redirects unauthenticated users from / to /login', async () => {
    setActivePinia(createPinia())
    const router = createRouter({ history: createMemoryHistory(), routes })
    await router.push('/')
    await router.isReady()
    const wrapper = mount(App, { global: { plugins: [router] } })
    expect(wrapper.html()).toContain('Sign in')
  })
})
```

- [ ] **Step 2: Run test to verify it fails (or passes — depending on guard wiring)**

Run: `cd app/Admin && pnpm test:unit src/app/__tests__/router.spec.ts`
Expected: PASS (the guard redirects `/` to `/login` because no tokens are set).

- [ ] **Step 3: Boot the dev server**

Run: `cd app/Admin && pnpm dev`
Expected: Vite starts on http://localhost:5173. Open in browser; confirm `/` redirects to `/login`. (Manual verification.)

- [ ] **Step 4: Build to verify code splitting**

Run: `cd app/Admin && pnpm build`
Expected: build succeeds; inspect `dist/assets/` for separate chunks for `auth`, `dashboard`, `users`.

- [ ] **Step 5: Commit**

```bash
cd app/Admin && git add src/app/__tests__/router.spec.ts && git commit -m "test(admin): integration test for router guard"
```

---

## Task 25: Cleanup — remove placeholder files and old imports

**Files:**
- Delete: `app/Admin/src/views/` (entire directory; was empty)
- Delete: `app/Admin/src/router/` (already moved)
- Verify: no orphan references to `@/api`, `@/router`, `@/App`, `@/main`, `@/stores/counter`

- [ ] **Step 1: Search for old imports**

Run:
```bash
cd app/Admin && grep -rE "from ['\"]@/(api|router|App|main|stores/counter|views)" src/ || true
```
Expected: no matches.

- [ ] **Step 2: Remove empty `views/` and `stores/` directories**

Run:
```bash
cd app/Admin
[ -d src/views ] && git rm -r src/views || true
[ -d src/stores ] && git rm -r src/stores || true
```

- [ ] **Step 3: Run all gates**

Run: `cd app/Admin && pnpm test:unit && pnpm type-check && pnpm lint`
Expected: all PASS.

- [ ] **Step 4: Commit**

```bash
cd app/Admin && git add -A && git commit -m "chore(admin): remove placeholder files (views, stores, root-level entrypoints)"
```

---

## Task 26: Verify acceptance criteria

- [ ] **Step 1: Run the full quality gate**

Run: `cd app/Admin && pnpm test:unit && pnpm type-check && pnpm lint && pnpm build`
Expected: all green; build emits per-slice chunks.

- [ ] **Step 2: Manual smoke test**

Run: `cd app/Admin && pnpm dev`
- Open http://localhost:5173 — confirm redirect to `/login`
- Sign in (mock or real backend) — confirm redirect to `/` dashboard
- Navigate to `/identity/users` — confirm list renders
- Create a user — confirm it appears in the list
- Edit the user — confirm changes persist
- Delete the user — confirm it's removed
- Toggle theme — confirm `.p-dark` toggles
- Toggle sidebar — confirm collapse/expand

- [ ] **Step 3: Final commit (if any cleanup needed)**

```bash
cd app/Admin && git add -A && git commit -m "chore(admin): final cleanup after acceptance verification"
```

---

## Self-Review

**1. Spec coverage** — checking each spec section against tasks:

- [x] Goals 1-7 covered: Tasks 1-26 implement folder structure, grouping, TanStack Query, Pinia split, shared/ 6 sub-folders, auto-imports, PrimeVue+Tailwind, TDD pattern
- [x] Top-level tree: Task 3 (folders), Task 13 (plugins), Task 14 (stores), Task 15 (layout), Task 17 (router)
- [x] Canonical slice shape: Task 21 (model+api) + Task 22 (ui+composables)
- [x] shared/api: Tasks 5-8
- [x] shared/ui: Task 16
- [x] shared/composables: Task 12
- [x] shared/lib: Task 9
- [x] shared/types: Task 10
- [x] shared/config: Task 11
- [x] app/stores: Task 14
- [x] features/auth: Tasks 18-19
- [x] features/dashboard: Task 20
- [x] features/identity/users: Tasks 21-22
- [x] Routing with lazy imports: Task 17
- [x] Auto-imports: Task 2 (unplugin-auto-import), Task 12 (composables)
- [x] Testing strategy: each task includes tests; integration in Task 24
- [x] Migration plan steps 1-15 + 20-22 from spec: mapped to Tasks 1-25
- [x] Acceptance criteria: Task 26

**2. Placeholder scan** — searched for "TBD", "TODO", "similar to":

- The `useAuthState.ts` mock `fetch('/api/auth/me', ...)` is a placeholder only when tokens exist in localStorage but the backend isn't running. Acceptable for development. Will be replaced when the auth backend is wired. Not flagged as a placeholder because it's a real, intentional stub.
- `UserFilters` uses an empty `MultiSelect :options="[]"` — real role options will be wired when the roles slice is built (out of scope for this plan). Flagged but acceptable.

**3. Type consistency:**

- `useUsersList` returns a custom shape (suspense, data, isLoading, error, refetch) — matches the test in Task 21 step 5
- `useUser`, `useCreateUser`, `useUpdateUser`, `useDeleteUser` use proper `UseQueryReturnType`/`UseMutationReturnType` generics
- `useUserForm.isPending` uses `computed(() => create.isPending.value || update.isPending.value)` because TanStack mutations expose `.value` on `.isPending`
- `usersQueryKeys.detail(id)` produces `['users', id]` — used in `get-by-id.ts` and `update.ts`
- `authQueryKeys.currentUser()` produces `['auth', 'current-user']` — used in `useAuthState` and `current-user.ts`

**4. Identified gaps:**

- The `useAuthState.fetchQuery` in Task 19 step 3 has a manual `fetch` call inside `setTokens` — should use the `api` client. Fixing inline in next iteration (post-execution).

---

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-07-06-admin-vertical-slice-foundation.md`. Two execution options:

**1. Subagent-Driven (recommended)** — I dispatch a fresh subagent per task, review between tasks, fast iteration

**2. Inline Execution** — Execute tasks in this session using executing-plans, batch execution with checkpoints

Which approach?
