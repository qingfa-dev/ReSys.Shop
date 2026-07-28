# Location Admin UI — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the 4 Location placeholder views with real CRUD UIs for countries and states, plus types, services, validations, and a store.

**Architecture:** Follow the auth module pattern — Zod per-field validations, static-class API services, Pinia Composition API stores, Vue SFC views using shared components (PageShell, CrudToolbar, FilterableDataTable, FormSection, FormField, PageHeading) and the usePagedQuery/useNotify/useApiErrorHandler composables.

**Tech Stack:** Vue 3 + TypeScript, PrimeVue, Zod, Pinia, Vitest, @/shared/api client

## Global Constraints

- Backend API is already implemented — no backend changes in this plan
- All files live under `app/Admin/src/features/location/`
- Follow auth module patterns: `export class` for API services, per-field Zod schemas composed into object schemas
- File naming: split by entity (country.ts, state.ts per directory)
- Tests live in `__tests__/` subdirectory alongside source
- Use `@/shared/api/client` for `post`, `get`, `put`, `del` and `@/shared/api` for `getPaged`
- Use `@/shared/api/client` directly (not `@/shared/api`) for get/post/put/del as per authApi pattern
- Import `QueryingParameters` from `@/shared/types/querying`, `Result`/`PagedResult` from `@/shared/types`
- Routes already exist — do not modify `routes/index.ts`
- `index.ts` barrel already re-exports from all subdirectories — only update sub-barrel files

---

### Task 1: Country and State Type Definitions with Fluent Querying Models

**Files:**
- Create: `app/Admin/src/features/location/types/country.ts`
- Create: `app/Admin/src/features/location/types/state.ts`
- Create: `app/Admin/src/features/location/types/__tests__/querying.spec.ts`
- Modify: `app/Admin/src/features/location/types/index.ts`

**Interfaces:**
- Produces: `CountryRequest`, `CountryListItem`, `CountryDetail`, `CountryQuery`, `COUNTRY_FILTER_FIELDS`, `COUNTRY_SORT_FIELDS`, `toCountryQueryParams`
- Produces: `StateRequest`, `StateListItem`, `StateDetail`, `StateQuery`, `STATE_FILTER_FIELDS`, `STATE_SORT_FIELDS`, `toStateQueryParams`

- [ ] **Step 1: Write `types/country.ts`**

```typescript
import type { QueryingParameters } from '@/shared/types/querying'

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

export const COUNTRY_FILTER_FIELDS = [
  'name',
  'isoCode',
  'callingCode',
  'isActive',
  'statesRequired',
  'createdAtUtc',
  'modifiedAtUtc',
]

export const COUNTRY_SORT_FIELDS = [
  'name',
  'isoCode',
  'callingCode',
  'createdAtUtc',
  'modifiedAtUtc',
]

export function toCountryQueryParams(query: CountryQuery): QueryingParameters {
  const filters: string[] = []

  if (query.name !== undefined && query.name !== '') {
    filters.push(`name*=${query.name}`)
  }
  if (query.isoCode !== undefined && query.isoCode !== '') {
    filters.push(`isoCode=${query.isoCode}`)
  }
  if (query.callingCode !== undefined && query.callingCode !== '') {
    filters.push(`callingCode*=${query.callingCode}`)
  }
  if (query.isActive !== undefined) {
    filters.push(`isActive=${query.isActive}`)
  }
  if (query.statesRequired !== undefined) {
    filters.push(`statesRequired=${query.statesRequired}`)
  }

  let sort: string[] | null = null
  if (query.sortBy) {
    const dir = query.sortDirection === 'desc' ? '-' : ''
    sort = [`${dir}${query.sortBy}`]
  }

  return {
    filter: filters.length > 0 ? filters.join(',') : null,
    search: query.search ?? null,
    sort,
    pageNumber: query.page ?? null,
    pageSize: query.pageSize ?? null,
  }
}
```

- [ ] **Step 2: Write `types/state.ts`**

```typescript
import type { QueryingParameters } from '@/shared/types/querying'

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
  sortBy?: 'name' | 'abbreviation' | 'countryName' | 'createdAtUtc' | 'modifiedAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const STATE_FILTER_FIELDS = [
  'name',
  'abbreviation',
  'countryId',
  'isActive',
  'createdAtUtc',
  'modifiedAtUtc',
]

export const STATE_SORT_FIELDS = [
  'name',
  'abbreviation',
  'countryId',
  'createdAtUtc',
  'modifiedAtUtc',
]

export function toStateQueryParams(query: StateQuery): QueryingParameters {
  const filters: string[] = []

  if (query.name !== undefined && query.name !== '') {
    filters.push(`name*=${query.name}`)
  }
  if (query.abbreviation !== undefined && query.abbreviation !== '') {
    filters.push(`abbreviation=${query.abbreviation}`)
  }
  if (query.countryId !== undefined && query.countryId !== '') {
    filters.push(`countryId=${query.countryId}`)
  }
  if (query.isActive !== undefined) {
    filters.push(`isActive=${query.isActive}`)
  }

  let sort: string[] | null = null
  if (query.sortBy) {
    const dir = query.sortDirection === 'desc' ? '-' : ''
    sort = [`${dir}${query.sortBy}`]
  }

  return {
    filter: filters.length > 0 ? filters.join(',') : null,
    search: query.search ?? null,
    sort,
    pageNumber: query.page ?? null,
    pageSize: query.pageSize ?? null,
  }
}
```

- [ ] **Step 3: Write `types/__tests__/querying.spec.ts`**

```typescript
import { describe, it, expect } from 'vitest'
import { toCountryQueryParams, COUNTRY_FILTER_FIELDS, COUNTRY_SORT_FIELDS } from '../country'
import { toStateQueryParams, STATE_FILTER_FIELDS, STATE_SORT_FIELDS } from '../state'

describe('toCountryQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toCountryQueryParams({})
    expect(result.filter).toBeNull()
    expect(result.search).toBeNull()
    expect(result.sort).toBeNull()
  })

  it('builds filter DSL for name (contains operator)', () => {
    const result = toCountryQueryParams({ name: 'United' })
    expect(result.filter).toBe('name*=United')
  })

  it('builds filter DSL for isoCode (equals operator)', () => {
    const result = toCountryQueryParams({ isoCode: 'US' })
    expect(result.filter).toBe('isoCode=US')
  })

  it('builds filter DSL for boolean isActive=true', () => {
    const result = toCountryQueryParams({ isActive: true })
    expect(result.filter).toBe('isActive=true')
  })

  it('builds filter DSL for boolean isActive=false', () => {
    const result = toCountryQueryParams({ isActive: false })
    expect(result.filter).toBe('isActive=false')
  })

  it('combines multiple filter conditions with comma', () => {
    const result = toCountryQueryParams({ name: 'Viet', isActive: true })
    expect(result.filter).toBe('name*=Viet,isActive=true')
  })

  it('builds sort ascending', () => {
    const result = toCountryQueryParams({ sortBy: 'name', sortDirection: 'asc' })
    expect(result.sort).toEqual(['name'])
  })

  it('builds sort descending', () => {
    const result = toCountryQueryParams({ sortBy: 'createdAtUtc', sortDirection: 'desc' })
    expect(result.sort).toEqual(['-createdAtUtc'])
  })

  it('includes search and pagination', () => {
    const result = toCountryQueryParams({ search: 'California', page: 2, pageSize: 10 })
    expect(result.search).toBe('California')
    expect(result.pageNumber).toBe(2)
    expect(result.pageSize).toBe(10)
  })

  it('skips empty string values in filters', () => {
    const result = toCountryQueryParams({ name: '', isoCode: 'US' })
    expect(result.filter).toBe('isoCode=US')
  })
})

describe('toStateQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toStateQueryParams({})
    expect(result.filter).toBeNull()
  })

  it('builds filter for countryId', () => {
    const result = toStateQueryParams({ countryId: 'abc-123' })
    expect(result.filter).toBe('countryId=abc-123')
  })

  it('builds filter for abbreviation', () => {
    const result = toStateQueryParams({ abbreviation: 'CA' })
    expect(result.filter).toBe('abbreviation=CA')
  })

  it('builds sort', () => {
    const result = toStateQueryParams({ sortBy: 'countryName', sortDirection: 'asc' })
    expect(result.sort).toEqual(['countryName'])
  })
})

describe('COUNTRY_FILTER_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(COUNTRY_FILTER_FIELDS).toContain('name')
    expect(COUNTRY_FILTER_FIELDS).toContain('isoCode')
    expect(COUNTRY_FILTER_FIELDS).toContain('isActive')
  })
})

describe('COUNTRY_SORT_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(COUNTRY_SORT_FIELDS).toContain('name')
    expect(COUNTRY_SORT_FIELDS).toContain('createdAtUtc')
    expect(COUNTRY_SORT_FIELDS).toContain('modifiedAtUtc')
  })
})

describe('STATE_FILTER_FIELDS', () => {
  it('contains countryId for filtering by parent', () => {
    expect(STATE_FILTER_FIELDS).toContain('countryId')
  })
})

describe('STATE_SORT_FIELDS', () => {
  it('contains countryId for sorting by parent', () => {
    expect(STATE_SORT_FIELDS).toContain('countryId')
  })
})
```

- [ ] **Step 4: Run tests**

```bash
cd app/Admin && pnpm run test:unit -- run --reporter=verbose 2>&1 | tail -30
```
Expected: all new querying tests pass.

- [ ] **Step 5: Update `types/index.ts` barrel**

```typescript
export type {
  CountryRequest,
  CountryListItem,
  CountryDetail,
  CountryQuery,
} from './country'
export {
  COUNTRY_FILTER_FIELDS,
  COUNTRY_SORT_FIELDS,
  toCountryQueryParams,
} from './country'
export type {
  StateRequest,
  StateListItem,
  StateDetail,
  StateQuery,
} from './state'
export {
  STATE_FILTER_FIELDS,
  STATE_SORT_FIELDS,
  toStateQueryParams,
} from './state'
```

- [ ] **Step 6: Verify build + commit**

```bash
cd app/Admin && pnpm run build 2>&1 | grep '✓ built\|error'
```

```bash
git add app/Admin/src/features/location/types/
git commit -m "feat(location): add Country/State types with fluent querying models

Includes toCountryQueryParams and toStateQueryParams mappers that
translate typed query objects into the QueryingParameters wire format
consumed by getPaged/usePagedQuery."
```

---

### Task 2: Zod Validation Schemas

**Files:**
- Create: `app/Admin/src/features/location/validations/country.ts`
- Create: `app/Admin/src/features/location/validations/state.ts`
- Create: `app/Admin/src/features/location/validations/__tests__/country.spec.ts`
- Create: `app/Admin/src/features/location/validations/__tests__/state.spec.ts`
- Modify: `app/Admin/src/features/location/validations/index.ts`

**Interfaces:**
- Produces: `countryName`, `countryIsoCode`, `countryCallingCode`, `countryStatesRequired`, `countryIsActive`, `countrySchema`, `CountryForm`
- Produces: `stateName`, `stateAbbreviation`, `stateCountryId`, `stateIsActive`, `stateSchema`, `StateForm`

- [ ] **Step 1: Write `validations/country.ts`**

```typescript
import { z } from 'zod'

export const countryName = z.string()
  .min(1, 'Country name is required.')
  .max(100, 'Country name must not exceed 100 characters.')

export const countryIsoCode = z.string()
  .min(1, 'ISO code is required.')
  .max(3, 'ISO code must not exceed 3 characters.')
  .regex(/^[A-Z]{2,3}$/, 'ISO code must be 2-3 uppercase letters.')

export const countryCallingCode = z.string()
  .max(10, 'Calling code must not exceed 10 characters.')

export const countryStatesRequired = z.boolean()
export const countryIsActive = z.boolean()

export const countrySchema = z.object({
  name: countryName,
  isoCode: countryIsoCode,
  callingCode: countryCallingCode.optional(),
  statesRequired: countryStatesRequired,
  isActive: countryIsActive,
})

export type CountryForm = z.infer<typeof countrySchema>
```

- [ ] **Step 2: Write `validations/state.ts`**

```typescript
import { z } from 'zod'

export const stateName = z.string()
  .min(1, 'State name is required.')
  .max(100, 'State name must not exceed 100 characters.')

export const stateAbbreviation = z.string()
  .min(1, 'Abbreviation is required.')
  .max(10, 'Abbreviation must not exceed 10 characters.')

export const stateCountryId = z.string()
  .min(1, 'Country is required.')

export const stateIsActive = z.boolean()

export const stateSchema = z.object({
  name: stateName,
  abbreviation: stateAbbreviation,
  countryId: stateCountryId,
  isActive: stateIsActive,
})

export type StateForm = z.infer<typeof stateSchema>
```

- [ ] **Step 3: Write `validations/__tests__/country.spec.ts`**

```typescript
import { describe, it, expect } from 'vitest'
import {
  countryName,
  countryIsoCode,
  countryCallingCode,
  countrySchema,
} from '../country'

describe('countryName', () => {
  it('accepts a valid name', () => {
    expect(countryName.safeParse('United States').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(countryName.safeParse('').success).toBe(false)
  })

  it('rejects string over 100 characters', () => {
    expect(countryName.safeParse('A'.repeat(101)).success).toBe(false)
  })

  it('accepts string of exactly 100 characters', () => {
    expect(countryName.safeParse('A'.repeat(100)).success).toBe(true)
  })
})

describe('countryIsoCode', () => {
  it('accepts valid 2-letter code', () => {
    expect(countryIsoCode.safeParse('US').success).toBe(true)
  })

  it('accepts valid 3-letter code', () => {
    expect(countryIsoCode.safeParse('USA').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(countryIsoCode.safeParse('').success).toBe(false)
  })

  it('rejects lowercase', () => {
    expect(countryIsoCode.safeParse('us').success).toBe(false)
  })

  it('rejects single letter', () => {
    expect(countryIsoCode.safeParse('U').success).toBe(false)
  })

  it('rejects 4 letters', () => {
    expect(countryIsoCode.safeParse('USAA').success).toBe(false)
  })

  it('returns correct error message', () => {
    const result = countryIsoCode.safeParse('us')
    if (!result.success) {
      expect(result.error.issues[0].message).toBe('ISO code must be 2-3 uppercase letters.')
    }
  })
})

describe('countryCallingCode', () => {
  it('accepts a valid calling code', () => {
    expect(countryCallingCode.safeParse('+84').success).toBe(true)
  })

  it('rejects code over 10 characters', () => {
    expect(countryCallingCode.safeParse('+12345678901').success).toBe(false)
  })
})

describe('countrySchema', () => {
  it('accepts valid country form', () => {
    const result = countrySchema.safeParse({
      name: 'Vietnam',
      isoCode: 'VN',
      callingCode: '+84',
      statesRequired: true,
      isActive: true,
    })
    expect(result.success).toBe(true)
  })

  it('accepts form without optional callingCode', () => {
    const result = countrySchema.safeParse({
      name: 'Vietnam',
      isoCode: 'VN',
      statesRequired: false,
      isActive: true,
    })
    expect(result.success).toBe(true)
  })

  it('rejects missing required name', () => {
    const result = countrySchema.safeParse({
      name: '',
      isoCode: 'VN',
      statesRequired: false,
      isActive: true,
    })
    expect(result.success).toBe(false)
  })

  it('returns per-field errors', () => {
    const result = countrySchema.safeParse({
      name: '',
      isoCode: 'v',
      statesRequired: false,
      isActive: true,
    })
    if (!result.success) {
      const fields = result.error.issues.map(i => String(i.path[0]))
      expect(fields).toContain('name')
      expect(fields).toContain('isoCode')
    }
  })
})
```

- [ ] **Step 4: Write `validations/__tests__/state.spec.ts`**

```typescript
import { describe, it, expect } from 'vitest'
import {
  stateName,
  stateAbbreviation,
  stateCountryId,
  stateSchema,
} from '../state'

describe('stateName', () => {
  it('accepts a valid name', () => {
    expect(stateName.safeParse('California').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(stateName.safeParse('').success).toBe(false)
  })

  it('rejects string over 100 characters', () => {
    expect(stateName.safeParse('A'.repeat(101)).success).toBe(false)
  })
})

describe('stateAbbreviation', () => {
  it('accepts a valid abbreviation', () => {
    expect(stateAbbreviation.safeParse('CA').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(stateAbbreviation.safeParse('').success).toBe(false)
  })

  it('rejects abbreviation over 10 characters', () => {
    expect(stateAbbreviation.safeParse('CALIFORNIAX').success).toBe(false)
  })
})

describe('stateCountryId', () => {
  it('accepts a valid GUID', () => {
    expect(stateCountryId.safeParse('abc-123-def').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(stateCountryId.safeParse('').success).toBe(false)
  })
})

describe('stateSchema', () => {
  it('accepts valid state form', () => {
    const result = stateSchema.safeParse({
      name: 'California',
      abbreviation: 'CA',
      countryId: 'abc-123',
      isActive: true,
    })
    expect(result.success).toBe(true)
  })

  it('rejects missing required fields', () => {
    const result = stateSchema.safeParse({
      name: '',
      abbreviation: '',
      countryId: '',
      isActive: true,
    })
    expect(result.success).toBe(false)
  })

  it('returns per-field errors', () => {
    const result = stateSchema.safeParse({
      name: '',
      abbreviation: '',
      countryId: '',
      isActive: true,
    })
    if (!result.success) {
      const fields = result.error.issues.map(i => String(i.path[0]))
      expect(fields).toContain('name')
      expect(fields).toContain('abbreviation')
      expect(fields).toContain('countryId')
    }
  })
})
```

- [ ] **Step 5: Run tests**

```bash
cd app/Admin && pnpm run test:unit -- run --reporter=verbose 2>&1 | tail -30
```
Expected: all validation tests pass.

- [ ] **Step 6: Update `validations/index.ts` barrel**

```typescript
export {
  countryName,
  countryIsoCode,
  countryCallingCode,
  countryStatesRequired,
  countryIsActive,
  countrySchema,
} from './country'
export type { CountryForm } from './country'
export {
  stateName,
  stateAbbreviation,
  stateCountryId,
  stateIsActive,
  stateSchema,
} from './state'
export type { StateForm } from './state'
```

- [ ] **Step 7: Verify build + commit**

```bash
cd app/Admin && pnpm run build 2>&1 | grep '✓ built\|error'
```

```bash
git add app/Admin/src/features/location/validations/
git commit -m "feat(location): add Zod validation schemas for Country and State forms

Per-field schemas with custom messages, composed into countrySchema and
stateSchema. Exported CountryForm and StateForm inferred types."
```

---

### Task 3: Location API Services

**Files:**
- Create: `app/Admin/src/features/location/services/countryApi.ts`
- Create: `app/Admin/src/features/location/services/stateApi.ts`
- Create: `app/Admin/src/features/location/services/__tests__/countryApi.spec.ts`
- Create: `app/Admin/src/features/location/services/__tests__/stateApi.spec.ts`
- Modify: `app/Admin/src/features/location/services/index.ts`

**Interfaces:**
- Consumes: `CountryRequest`, `CountryListItem`, `CountryDetail`, `CountryQuery`, `toCountryQueryParams`, `COUNTRY_FILTER_FIELDS`, `COUNTRY_SORT_FIELDS` from `../types/country`
- Consumes: `StateRequest`, `StateListItem`, `StateDetail`, `StateQuery`, `toStateQueryParams`, `STATE_FILTER_FIELDS`, `STATE_SORT_FIELDS` from `../types/state`
- Produces: `CountryApi` class with static methods, `StateApi` class with static methods

- [ ] **Step 1: Write `services/countryApi.ts`**

```typescript
import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { LOCATION } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type {
  CountryRequest,
  CountryListItem,
  CountryDetail,
  CountryQuery,
} from '../types/country'
import {
  toCountryQueryParams,
  COUNTRY_FILTER_FIELDS,
  COUNTRY_SORT_FIELDS,
} from '../types/country'

export class CountryApi {
  private static readonly BASE = `${LOCATION}/countries`

  static getCountries(query: CountryQuery): Promise<PagedResult<CountryListItem>> {
    return getPaged<CountryListItem>(CountryApi.BASE, toCountryQueryParams(query), {
      allowedFilterFields: COUNTRY_FILTER_FIELDS,
      allowedSortFields: COUNTRY_SORT_FIELDS,
    })
  }

  static getCountry(id: string): Promise<Result<CountryDetail>> {
    return get<Result<CountryDetail>>(`${CountryApi.BASE}/${id}`)
  }

  static createCountry(request: CountryRequest): Promise<Result<CountryDetail>> {
    return post<Result<CountryDetail>>(CountryApi.BASE, request)
  }

  static updateCountry(id: string, request: CountryRequest): Promise<Result<CountryDetail>> {
    return put<Result<CountryDetail>>(`${CountryApi.BASE}/${id}`, request)
  }

  static deleteCountry(id: string): Promise<Result<CountryListItem>> {
    return del<Result<CountryListItem>>(`${CountryApi.BASE}/${id}`)
  }
}
```

- [ ] **Step 2: Write `services/stateApi.ts`**

```typescript
import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { LOCATION } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type {
  StateRequest,
  StateListItem,
  StateDetail,
  StateQuery,
} from '../types/state'
import {
  toStateQueryParams,
  STATE_FILTER_FIELDS,
  STATE_SORT_FIELDS,
} from '../types/state'

export class StateApi {
  private static readonly BASE = `${LOCATION}/states`

  static getStates(query: StateQuery): Promise<PagedResult<StateListItem>> {
    return getPaged<StateListItem>(StateApi.BASE, toStateQueryParams(query), {
      allowedFilterFields: STATE_FILTER_FIELDS,
      allowedSortFields: STATE_SORT_FIELDS,
    })
  }

  static getState(id: string): Promise<Result<StateDetail>> {
    return get<Result<StateDetail>>(`${StateApi.BASE}/${id}`)
  }

  static createState(request: StateRequest): Promise<Result<StateDetail>> {
    return post<Result<StateDetail>>(StateApi.BASE, request)
  }

  static updateState(id: string, request: StateRequest): Promise<Result<StateDetail>> {
    return put<Result<StateDetail>>(`${StateApi.BASE}/${id}`, request)
  }

  static deleteState(id: string): Promise<Result<StateListItem>> {
    return del<Result<StateListItem>>(`${StateApi.BASE}/${id}`)
  }
}
```

- [ ] **Step 3: Write `services/__tests__/countryApi.spec.ts`**

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockGet, mockPut, mockDel, mockGetPaged } = vi.hoisted(() => ({
  mockPost: vi.fn(),
  mockGet: vi.fn(),
  mockPut: vi.fn(),
  mockDel: vi.fn(),
  mockGetPaged: vi.fn(),
}))

vi.mock('@/shared/api/client', () => ({
  post: mockPost,
  get: mockGet,
  put: mockPut,
  del: mockDel,
}))

vi.mock('@/shared/api', () => ({
  getPaged: mockGetPaged,
}))

import { CountryApi } from '../countryApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('CountryApi.getCountries', () => {
  it('calls getPaged with country query params', async () => {
    mockGetPaged.mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 20,
      totalCount: 0,
      totalPages: 0,
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
    })

    await CountryApi.getCountries({ isActive: true, page: 1, pageSize: 10 })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/locations/countries',
      { filter: 'isActive=true', search: null, sort: null, pageNumber: 1, pageSize: 10 },
      expect.objectContaining({ allowedFilterFields: expect.any(Array) }),
    )
  })
})

describe('CountryApi.getCountry', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: '1', name: 'US' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await CountryApi.getCountry('abc-123')
    expect(mockGet).toHaveBeenCalledWith('api/locations/countries/abc-123')
  })
})

describe('CountryApi.createCountry', () => {
  it('calls POST with request body', async () => {
    const req = { name: 'Canada', isoCode: 'CA', callingCode: '+1', statesRequired: true, isActive: true }
    mockPost.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })

    await CountryApi.createCountry(req)
    expect(mockPost).toHaveBeenCalledWith('api/locations/countries', req)
  })
})

describe('CountryApi.updateCountry', () => {
  it('calls PUT with request body', async () => {
    const req = { name: 'Canada', isoCode: 'CA', callingCode: '+1', statesRequired: false, isActive: true }
    mockPut.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await CountryApi.updateCountry('abc-123', req)
    expect(mockPut).toHaveBeenCalledWith('api/locations/countries/abc-123', req)
  })
})

describe('CountryApi.deleteCountry', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: { id: '1', name: 'Canada' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await CountryApi.deleteCountry('abc-123')
    expect(mockDel).toHaveBeenCalledWith('api/locations/countries/abc-123')
  })
})
```

- [ ] **Step 4: Write `services/__tests__/stateApi.spec.ts`**

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockGet, mockPut, mockDel, mockGetPaged } = vi.hoisted(() => ({
  mockPost: vi.fn(),
  mockGet: vi.fn(),
  mockPut: vi.fn(),
  mockDel: vi.fn(),
  mockGetPaged: vi.fn(),
}))

vi.mock('@/shared/api/client', () => ({
  post: mockPost,
  get: mockGet,
  put: mockPut,
  del: mockDel,
}))

vi.mock('@/shared/api', () => ({
  getPaged: mockGetPaged,
}))

import { StateApi } from '../stateApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('StateApi.getStates', () => {
  it('calls getPaged with state query and countryId filter', async () => {
    mockGetPaged.mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 20,
      totalCount: 0,
      totalPages: 0,
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
    })

    await StateApi.getStates({ countryId: 'abc-123' })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/locations/states',
      expect.objectContaining({ filter: 'countryId=abc-123' }),
      expect.any(Object),
    )
  })
})

describe('StateApi.getState', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: '1', name: 'California' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await StateApi.getState('xyz-456')
    expect(mockGet).toHaveBeenCalledWith('api/locations/states/xyz-456')
  })
})

describe('StateApi.createState', () => {
  it('calls POST with request body', async () => {
    const req = { name: 'Texas', abbreviation: 'TX', countryId: 'us-id', isActive: true }
    mockPost.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })

    await StateApi.createState(req)
    expect(mockPost).toHaveBeenCalledWith('api/locations/states', req)
  })
})

describe('StateApi.updateState', () => {
  it('calls PUT with request body', async () => {
    const req = { name: 'Texas', abbreviation: 'TX', countryId: 'us-id', isActive: true }
    mockPut.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await StateApi.updateState('xyz-456', req)
    expect(mockPut).toHaveBeenCalledWith('api/locations/states/xyz-456', req)
  })
})

describe('StateApi.deleteState', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: { id: '1', name: 'Texas' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await StateApi.deleteState('xyz-456')
    expect(mockDel).toHaveBeenCalledWith('api/locations/states/xyz-456')
  })
})
```

- [ ] **Step 5: Run tests**

```bash
cd app/Admin && pnpm run test:unit -- run --reporter=verbose 2>&1 | tail -30
```
Expected: all API service tests pass.

- [ ] **Step 6: Update `services/index.ts` barrel**

```typescript
export { CountryApi } from './countryApi'
export { StateApi } from './stateApi'
```

- [ ] **Step 7: Verify build + commit**

```bash
cd app/Admin && pnpm run build 2>&1 | grep '✓ built\|error'
```

```bash
git add app/Admin/src/features/location/services/
git commit -m "feat(location): add CountryApi/StateApi static service classes

All 5 methods per entity: getPaged list, getById, create, update, delete.
Follows auth module pattern with @/shared/api/client + getPaged."
```

---

### Task 4: Country Store (Active Countries Cache)

**Files:**
- Create: `app/Admin/src/features/location/stores/countryStore.ts`
- Create: `app/Admin/src/features/location/stores/stateStore.ts`
- Modify: `app/Admin/src/features/location/stores/index.ts`

**Interfaces:**
- Consumes: `CountryApi` from `../services`, `CountryListItem` from `../types/country`
- Produces: `useCountryStore` — `activeCountries`, `loaded`, `fetchActive()`

- [ ] **Step 1: Write `stores/countryStore.ts`**

```typescript
import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { CountryListItem } from '../types/country'
import { CountryApi } from '../services/countryApi'

export const useCountryStore = defineStore('countries', () => {
  const activeCountries = ref<CountryListItem[]>([])
  const loaded = ref(false)

  async function fetchActive(): Promise<void> {
    if (loaded.value) return

    const result = await CountryApi.getCountries({
      isActive: true,
      pageSize: 100,
      sortBy: 'name',
      sortDirection: 'asc',
    })

    if (result.isSuccess) {
      activeCountries.value = result.items
      loaded.value = true
    }
  }

  return { activeCountries, loaded, fetchActive }
})
```

- [ ] **Step 2: Write `stores/stateStore.ts`** (empty placeholder)

```typescript
// Placeholder store — reserved for future state-specific state
export {}
```

- [ ] **Step 3: Update `stores/index.ts` barrel**

```typescript
export { useCountryStore } from './countryStore'
```

- [ ] **Step 4: Verify build + commit**

```bash
cd app/Admin && pnpm run build 2>&1 | grep '✓ built\|error'
```

```bash
git add app/Admin/src/features/location/stores/
git commit -m "feat(location): add countryStore for active-countries dropdown cache

useCountryStore wraps CountryApi.getCountries with a one-shot fetch
guard. States list and State detail form both consume this for their
country dropdown."
```

---

### Task 5: CountriesList View

**Files:**
- Modify: `app/Admin/src/features/location/views/CountriesList.vue`

**Interfaces:**
- Consumes: `CountryApi` from `../services`, `CountryListItem` from `../types/country`, `COUNTRY_FILTER_FIELDS`, `COUNTRY_SORT_FIELDS` from `../types/country`
- Consumes: `usePagedQuery` from `@/shared/composables/usePagedQuery`, `useNotify` from `@/shared/composables/useNotify`
- Consumes: `PageShell` from `@panel`, `CrudToolbar`, `FilterableDataTable` from `@data`
- Produces: named Vue SFC view component

- [ ] **Step 1: Replace `views/CountriesList.vue`**

```vue
<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import { PageShell } from '@panel'
import { CrudToolbar, FilterableDataTable } from '@data'

interface ColumnDef {
  field: string
  header: string
  sortable?: boolean
  filter?: boolean
  filterField?: string
  bodyStyle?: string
  style?: string
}
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useNotify } from '@/shared/composables/useNotify'
import { CountryApi } from '../services/countryApi'
import type { CountryListItem } from '../types/country'
import { COUNTRY_FILTER_FIELDS, COUNTRY_SORT_FIELDS } from '../types/country'
import Tag from 'primevue/tag'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()

const selectedItems = ref<CountryListItem[]>([])

const {
  items,
  loading,
  totalCount,
  page,
  pageSize,
  setSearch,
  refresh,
} = usePagedQuery<CountryListItem>('api/locations/countries', {
  allowedFilterFields: COUNTRY_FILTER_FIELDS,
  allowedSortFields: COUNTRY_SORT_FIELDS,
  defaultSort: ['name'],
  defaultPageSize: 20,
})

const columns: ColumnDef[] = [
  { field: 'name', header: 'Name', sortable: true, filter: true },
  { field: 'isoCode', header: 'ISO Code', sortable: true, filter: true },
  { field: 'callingCode', header: 'Calling Code', sortable: true },
  {
    field: 'statesRequired',
    header: 'States Required',
    sortable: true,
    bodyStyle: 'text-align: center',
  },
  {
    field: 'isActive',
    header: 'Active',
    sortable: true,
    filter: true,
    bodyStyle: 'text-align: center',
  },
  { field: 'actions', header: '', bodyStyle: 'text-align: right; width: 6rem' },
]

function navigateToNew() {
  router.push('/location/countries/new')
}

function navigateToEdit(id: string) {
  router.push(`/location/countries/${id}`)
}

function onSearch(value: string) {
  setSearch(value)
}

function confirmDelete() {
  if (selectedItems.value.length === 0) return

  confirm.require({
    message: `Are you sure you want to delete ${selectedItems.value.length > 1 ? 'these countries' : 'this country'}?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const target = selectedItems.value[0]
      const result = await CountryApi.deleteCountry(target.id)
      if (result.isSuccess) {
        notify.success('Country deleted', `${target.name} has been removed.`)
      } else {
        notify.error('Delete failed', result.errors?.[0]?.message ?? 'Could not delete country.')
      }
      selectedItems.value = []
      refresh()
    },
  })
}
</script>

<template>
  <PageShell title="Countries" description="Manage supported countries">
    <CrudToolbar
      new-label="New Country"
      delete-label="Delete"
      :delete-disabled="selectedItems.length === 0"
      :search-placeholder="'Search countries...'"
      @new="navigateToNew"
      @delete="confirmDelete"
      @update:search="onSearch"
    />
    <FilterableDataTable
      :columns="columns"
      :data="items"
      :loading="loading"
      :rows="pageSize"
      :filters="{}"
      :global-filter-fields="['name', 'isoCode', 'callingCode']"
    >
      <template #body-statesRequired="{ data }">
        <Tag :value="data.statesRequired ? 'Yes' : 'No'" :severity="data.statesRequired ? 'info' : 'secondary'" />
      </template>
      <template #body-isActive="{ data }">
        <Tag :value="data.isActive ? 'Active' : 'Inactive'" :severity="data.isActive ? 'success' : 'danger'" />
      </template>
      <template #body-actions="{ data }">
        <div class="flex justify-end gap-2">
          <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEdit(data.id)" />
          <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="selectedItems = [data]; confirmDelete()" />
        </div>
      </template>
      <template #empty>
        <div class="text-center py-8 text-muted-color">No countries found.</div>
      </template>
    </FilterableDataTable>
  </PageShell>
</template>
```

- [ ] **Step 2: Verify build**

```bash
cd app/Admin && pnpm run build 2>&1 | grep '✓ built\|error'
```

- [ ] **Step 3: Run full tests**

```bash
cd app/Admin && pnpm run test:unit -- run 2>&1 | tail -5
```
Expected: all tests still pass.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/location/views/CountriesList.vue
git commit -m "feat(location): replace CountriesList placeholder with real CRUD view

usePagedQuery-powered DataTable with search, inline delete with
confirmation dialog, and StatusTag-style badges for States Required
and Is Active columns."
```

---

### Task 6: CountryDetail View (Create + Edit)

**Files:**
- Modify: `app/Admin/src/features/location/views/CountryDetail.vue`

**Interfaces:**
- Consumes: `CountryApi` from `../services`, `CountryDetail`/`CountryRequest` from `../types/country`
- Consumes: `countrySchema`, `CountryForm` from `../validations/country`
- Consumes: `useNotify` from `@/shared/composables/useNotify`, `useApiErrorHandler` from `@/shared/composables/useApiErrorHandler`
- Consumes: `PageShell`, `PageHeading` from `@panel`, `FormSection`, `FormField` from `@form`

- [ ] **Step 1: Replace `views/CountryDetail.vue`**

```vue
<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { PageShell, PageHeading } from '@panel'
import { FormSection, FormField } from '@form'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { CountryApi } from '../services/countryApi'
import { countrySchema } from '../validations/country'
import type { CountryForm } from '../validations/country'
import type { CountryDetail } from '../types/country'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()

const isEdit = computed(() => !!route.params.id)
const pageTitle = computed(() => isEdit.value ? 'Edit Country' : 'New Country')

const form = ref<CountryForm>({
  name: '',
  isoCode: '',
  callingCode: '',
  statesRequired: false,
  isActive: true,
})

const fieldErrors = ref<Record<string, string>>({})
const loading = ref(false)

onMounted(async () => {
  if (isEdit.value) {
    const result = await CountryApi.getCountry(route.params.id as string)
    if (result.isSuccess) {
      const c = result.value
      form.value = {
        name: c.name,
        isoCode: c.isoCode,
        callingCode: c.callingCode ?? '',
        statesRequired: c.statesRequired,
        isActive: c.isActive,
      }
    } else {
      handleResult(result)
      router.push('/location/countries')
    }
  }
})

function onIsoCodeInput(value: string) {
  form.value.isoCode = value.toUpperCase()
}

async function onSave() {
  fieldErrors.value = {}
  const parsed = countrySchema.safeParse(form.value)

  if (!parsed.success) {
    for (const issue of parsed.error.issues) {
      const field = String(issue.path[0])
      if (!fieldErrors.value[field]) {
        fieldErrors.value[field] = issue.message
      }
    }
    return
  }

  loading.value = true
  const data = parsed.data
  const request = {
    name: data.name,
    isoCode: data.isoCode,
    callingCode: data.callingCode || null,
    statesRequired: data.statesRequired,
    isActive: data.isActive,
  }

  const result = isEdit.value
    ? await CountryApi.updateCountry(route.params.id as string, request)
    : await CountryApi.createCountry(request)

  loading.value = false

  if (result.isSuccess) {
    notify.success(isEdit.value ? 'Country updated' : 'Country created')
    router.push('/location/countries')
  } else {
    handleResult(result)
  }
}

function onCancel() {
  router.push('/location/countries')
}
</script>

<template>
  <PageShell :title="pageTitle">
    <PageHeading
      title=""
      :breadcrumbs="[
        { label: 'Home', to: '/' },
        { label: 'Countries', to: '/location/countries' },
        { label: pageTitle },
      ]"
      :actions="[
        { label: 'Save', icon: 'pi pi-check', severity: 'primary' },
        { label: 'Cancel', icon: 'pi pi-times' },
      ]"
      @action="(i: number) => i === 0 ? onSave() : onCancel()"
    />
    <FormSection title="Country Details">
      <FormField label="Name" :required="true" :invalid="!!fieldErrors.name">
        <InputText v-model="form.name" fluid class="w-full" />
        <small v-if="fieldErrors.name" class="text-red-500">{{ fieldErrors.name }}</small>
      </FormField>
      <FormField label="ISO Code" :required="true" :invalid="!!fieldErrors.isoCode" help-text="2-3 uppercase letters (e.g. US, VN)">
        <InputText v-model="form.isoCode" fluid maxlength="3" class="w-full" @update:model-value="(v: string) => onIsoCodeInput(v)" />
        <small v-if="fieldErrors.isoCode" class="text-red-500">{{ fieldErrors.isoCode }}</small>
      </FormField>
      <FormField label="Calling Code" :invalid="!!fieldErrors.callingCode" help-text="Optional (e.g. +1, +84)">
        <InputText v-model="form.callingCode" fluid maxlength="10" class="w-full" />
        <small v-if="fieldErrors.callingCode" class="text-red-500">{{ fieldErrors.callingCode }}</small>
      </FormField>
      <FormField label="States Required">
        <ToggleSwitch v-model="form.statesRequired" />
      </FormField>
      <FormField label="Active">
        <ToggleSwitch v-model="form.isActive" />
      </FormField>
    </FormSection>
  </PageShell>
</template>
```

- [ ] **Step 2: Verify build + run tests**

```bash
cd app/Admin && pnpm run build 2>&1 | grep '✓ built\|error'
cd app/Admin && pnpm run test:unit -- run 2>&1 | tail -5
```

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/location/views/CountryDetail.vue
git commit -m "feat(location): replace CountryDetail placeholder with create/edit form

Zod client-side validation with per-field errors, uppercase transform
on ISO code input, create/edit mode based on route param presence."
```

---

### Task 7: StatesList View

**Files:**
- Modify: `app/Admin/src/features/location/views/StatesList.vue`

**Interfaces:**
- Consumes: `StateApi` from `../services`, `StateListItem` from `../types/state`, `STATE_FILTER_FIELDS`, `STATE_SORT_FIELDS` from `../types/state`
- Consumes: `useCountryStore` from `../stores` for country dropdown
- Consumes: `usePagedQuery` from `@/shared/composables/usePagedQuery`, `useNotify` from `@/shared/composables/useNotify`

- [ ] **Step 1: Replace `views/StatesList.vue`**

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import { PageShell } from '@panel'
import { CrudToolbar, FilterableDataTable } from '@data'

interface ColumnDef {
  field: string
  header: string
  sortable?: boolean
  filter?: boolean
  filterField?: string
  bodyStyle?: string
  style?: string
}
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useNotify } from '@/shared/composables/useNotify'
import { StateApi } from '../services/stateApi'
import { useCountryStore } from '../stores/countryStore'
import type { StateListItem } from '../types/state'
import { STATE_FILTER_FIELDS, STATE_SORT_FIELDS } from '../types/state'
import Select from 'primevue/select'
import Tag from 'primevue/tag'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()
const countryStore = useCountryStore()

const selectedCountryId = ref<string | null>(null)
const selectedItems = ref<StateListItem[]>([])

const { items, loading, totalCount, page, pageSize, setSearch, setFilter, refresh } =
  usePagedQuery<StateListItem>('api/locations/states', {
    allowedFilterFields: STATE_FILTER_FIELDS,
    allowedSortFields: STATE_SORT_FIELDS,
    defaultSort: ['name'],
    defaultPageSize: 20,
  })

const columns: ColumnDef[] = [
  { field: 'name', header: 'Name', sortable: true, filter: true },
  { field: 'abbreviation', header: 'Abbreviation', sortable: true, filter: true },
  { field: 'countryName', header: 'Country', sortable: true, filter: true },
  {
    field: 'isActive',
    header: 'Active',
    sortable: true,
    filter: true,
    bodyStyle: 'text-align: center',
  },
  { field: 'actions', header: '', bodyStyle: 'text-align: right; width: 6rem' },
]

onMounted(() => {
  countryStore.fetchActive()
})

function navigateToNew() {
  router.push('/location/states/new')
}

function navigateToEdit(id: string) {
  router.push(`/location/states/${id}`)
}

function onSearch(value: string) {
  setSearch(value)
}

function onCountryFilterChange(countryId: string | null) {
  selectedCountryId.value = countryId
  if (countryId) {
    setFilter(`countryId=${countryId}`)
  } else {
    setFilter('')
  }
}

function confirmDelete() {
  if (selectedItems.value.length === 0) return

  confirm.require({
    message: `Are you sure you want to delete ${selectedItems.value.length > 1 ? 'these states' : 'this state'}?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const target = selectedItems.value[0]
      const result = await StateApi.deleteState(target.id)
      if (result.isSuccess) {
        notify.success('State deleted', `${target.name} has been removed.`)
      } else {
        notify.error('Delete failed', result.errors?.[0]?.message ?? 'Could not delete state.')
      }
      selectedItems.value = []
      refresh()
    },
  })
}
</script>

<template>
  <PageShell title="States" description="Manage states and provinces for countries">
    <CrudToolbar
      new-label="New State"
      delete-label="Delete"
      :delete-disabled="selectedItems.length === 0"
      :search-placeholder="'Search states...'"
      @new="navigateToNew"
      @delete="confirmDelete"
      @update:search="onSearch"
    >
      <template #header-left>
        <div class="flex items-center gap-2">
          <label class="text-sm text-muted-color whitespace-nowrap">Country:</label>
          <Select
            v-model="selectedCountryId"
            :options="countryStore.activeCountries"
            option-label="name"
            option-value="id"
            placeholder="All Countries"
            show-clear
            class="w-56"
            @change="onCountryFilterChange($event.value)"
          />
        </div>
      </template>
    </CrudToolbar>
    <FilterableDataTable
      :columns="columns"
      :data="items"
      :loading="loading"
      :rows="pageSize"
      :filters="{}"
      :global-filter-fields="['name', 'abbreviation', 'countryName']"
    >
      <template #body-isActive="{ data }">
        <Tag :value="data.isActive ? 'Active' : 'Inactive'" :severity="data.isActive ? 'success' : 'danger'" />
      </template>
      <template #body-actions="{ data }">
        <div class="flex justify-end gap-2">
          <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEdit(data.id)" />
          <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="selectedItems = [data]; confirmDelete()" />
        </div>
      </template>
      <template #empty>
        <div class="text-center py-8 text-muted-color">No states found.</div>
      </template>
    </FilterableDataTable>
  </PageShell>
</template>
```

- [ ] **Step 2: Verify build + run tests**

```bash
cd app/Admin && pnpm run build 2>&1 | grep '✓ built\|error'
cd app/Admin && pnpm run test:unit -- run 2>&1 | tail -5
```

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/location/views/StatesList.vue
git commit -m "feat(location): replace StatesList placeholder with real CRUD view

Country filter dropdown powered by useCountryStore, filterable datatable
with countryName column, inline delete with confirmation dialog."
```

---

### Task 8: StateDetail View (Create + Edit)

**Files:**
- Modify: `app/Admin/src/features/location/views/StateDetail.vue`

- [ ] **Step 1: Replace `views/StateDetail.vue`**

```vue
<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { PageShell, PageHeading } from '@panel'
import { FormSection, FormField } from '@form'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { StateApi } from '../services/stateApi'
import { useCountryStore } from '../stores/countryStore'
import { stateSchema } from '../validations/state'
import type { StateForm } from '../validations/state'
import Select from 'primevue/select'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()
const countryStore = useCountryStore()

const isEdit = computed(() => !!route.params.id)
const pageTitle = computed(() => isEdit.value ? 'Edit State' : 'New State')

const form = ref<StateForm>({
  name: '',
  abbreviation: '',
  countryId: '',
  isActive: true,
})

const fieldErrors = ref<Record<string, string>>({})
const loading = ref(false)

onMounted(async () => {
  countryStore.fetchActive()

  if (isEdit.value) {
    const result = await StateApi.getState(route.params.id as string)
    if (result.isSuccess) {
      const s = result.value
      form.value = {
        name: s.name,
        abbreviation: s.abbreviation,
        countryId: s.countryId,
        isActive: s.isActive,
      }
    } else {
      handleResult(result)
      router.push('/location/states')
    }
  }
})

async function onSave() {
  fieldErrors.value = {}
  const parsed = stateSchema.safeParse(form.value)

  if (!parsed.success) {
    for (const issue of parsed.error.issues) {
      const field = String(issue.path[0])
      if (!fieldErrors.value[field]) {
        fieldErrors.value[field] = issue.message
      }
    }
    return
  }

  loading.value = true
  const data = parsed.data
  const request = {
    name: data.name,
    abbreviation: data.abbreviation,
    countryId: data.countryId,
    isActive: data.isActive,
  }

  const result = isEdit.value
    ? await StateApi.updateState(route.params.id as string, request)
    : await StateApi.createState(request)

  loading.value = false

  if (result.isSuccess) {
    notify.success(isEdit.value ? 'State updated' : 'State created')
    router.push('/location/states')
  } else {
    handleResult(result)
  }
}

function onCancel() {
  router.push('/location/states')
}
</script>

<template>
  <PageShell :title="pageTitle">
    <PageHeading
      title=""
      :breadcrumbs="[
        { label: 'Home', to: '/' },
        { label: 'States', to: '/location/states' },
        { label: pageTitle },
      ]"
      :actions="[
        { label: 'Save', icon: 'pi pi-check', severity: 'primary' },
        { label: 'Cancel', icon: 'pi pi-times' },
      ]"
      @action="(i: number) => i === 0 ? onSave() : onCancel()"
    />
    <FormSection title="State Details">
      <FormField label="Name" :required="true" :invalid="!!fieldErrors.name">
        <InputText v-model="form.name" fluid class="w-full" />
        <small v-if="fieldErrors.name" class="text-red-500">{{ fieldErrors.name }}</small>
      </FormField>
      <FormField label="Abbreviation" :required="true" :invalid="!!fieldErrors.abbreviation" help-text="Short code (e.g. CA, NY, TX)">
        <InputText v-model="form.abbreviation" fluid maxlength="10" class="w-full" />
        <small v-if="fieldErrors.abbreviation" class="text-red-500">{{ fieldErrors.abbreviation }}</small>
      </FormField>
      <FormField label="Country" :required="true" :invalid="!!fieldErrors.countryId">
        <Select
          v-model="form.countryId"
          :options="countryStore.activeCountries"
          option-label="name"
          option-value="id"
          placeholder="Select a country"
          class="w-full"
        />
        <small v-if="fieldErrors.countryId" class="text-red-500">{{ fieldErrors.countryId }}</small>
      </FormField>
      <FormField label="Active">
        <ToggleSwitch v-model="form.isActive" />
      </FormField>
    </FormSection>
  </PageShell>
</template>
```

- [ ] **Step 2: Verify build + run tests**

```bash
cd app/Admin && pnpm run build 2>&1 | grep '✓ built\|error'
cd app/Admin && pnpm run test:unit -- run 2>&1 | tail -5
```

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/location/views/StateDetail.vue
git commit -m "feat(location): replace StateDetail placeholder with create/edit form

Country dropdown from useCountryStore, Zod validation, create/edit mode
based on route param."
```

---

### Task 9: Final Integration Check

- [ ] **Step 1: Verify build passes with zero errors**

```bash
cd app/Admin && pnpm run build
if [ $? -ne 0 ]; then
  echo "BUILD FAILED"
  exit 1
fi
```

- [ ] **Step 2: Run all unit tests**

```bash
cd app/Admin && pnpm run test:unit -- run
```
Expected: all 357+ tests pass.

- [ ] **Step 3: Run linter**

```bash
cd app/Admin && pnpm run lint
```
Expected: no errors.

- [ ] **Step 4: Audit file structure**

```bash
ls -la app/Admin/src/features/location/types/
ls -la app/Admin/src/features/location/validations/
ls -la app/Admin/src/features/location/services/
ls -la app/Admin/src/features/location/stores/
ls -la app/Admin/src/features/location/views/
```

- [ ] **Step 5: Commit if all passes**

```bash
git add app/Admin/src/features/location/views/ app/Admin/src/features/location/validations/__tests__/
git commit -m "chore(location): verify final integration — build, tests, lint all pass"
```
