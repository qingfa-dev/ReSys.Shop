# Catalog Option Types — Admin UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build Admin SPA management UI for Catalog OptionType CRUD + embedded OptionValue CRUD with tab layout and dialog slide-out.

**Architecture:** Full Location-module replication. Types -> Services -> Stores -> Validations -> Views. Two tabbed views: OptionTypesList (standalone DataTable page) and OptionTypeDetail (form + tabbed OptionValues DataTable with Dialog create/edit).

**Tech Stack:** Vue 3 + TypeScript, PrimeVue v5, Zod, Pinia, Vitest, @primeicons/vue

## Global Constraints

- Must pass `pnpm run lint` and `pnpm run build-only` with zero errors
- Must pass all existing 416 tests (no regressions)
- No new npm dependencies
- Catalog routes and menu items are already wired — no route changes needed
- Use `catalog-option-types` and `catalog-option-type-detail` route names (already defined)
- `OptionTypesList.vue` is lazy-loaded at `/catalog/option-types`
- `OptionTypeDetail.vue` is lazy-loaded at `/catalog/option-types/:id`
- Follow existing conventions: no comments in new code, static API classes, Zod individual + combined schema pattern, Pinia with loaded guard, inline PrimeVue DataTable/Column/Toolbar/Card

## File Structure

Before implementation, verify/create these directories (all under `app/Admin/src/features/catalog/option-types/`):
```
├── types/              (exists, empty)
├── services/           (exists, empty)
├── stores/             (exists, empty)
├── validations/        (exists, empty)
├── components/         (DOES NOT EXIST — must create)
├── composables/        (DOES NOT EXIST — must create)
├── views/              (exists, empty)
└── __tests__/          (exists, empty — verify subdirs)
    ├── types/
    ├── services/
    └── validations/
```

### Files Created Per Task

| Task | Files Created |
|------|--------------|
| 1 | `types/optionType.ts`, `types/optionValue.ts`, `types/index.ts`, `__tests__/types/optionType.spec.ts`, `__tests__/types/optionValue.spec.ts` |
| 2 | `validations/optionType.ts`, `validations/optionValue.ts`, `validations/index.ts`, `__tests__/validations/optionType.spec.ts`, `__tests__/validations/optionValue.spec.ts` |
| 3 | `services/optionTypeApi.ts`, `services/optionValueApi.ts`, `services/index.ts`, `__tests__/services/optionTypeApi.spec.ts`, `__tests__/services/optionValueApi.spec.ts` |
| 4 | `stores/optionTypeStore.ts`, `stores/index.ts` |
| 5 | `components/OptionValueFormDialog.vue`, `components/index.ts`, `composables/index.ts` |
| 6 | `views/OptionTypesList.vue` |
| 7 | `views/OptionTypeDetail.vue`, `views/index.ts` |

---

### Step 0: Scaffold directories and test subdirectories

- [ ] Create missing directories:
```bash
mkdir -p app/Admin/src/features/catalog/option-types/components
mkdir -p app/Admin/src/features/catalog/option-types/composables
mkdir -p app/Admin/src/features/catalog/option-types/__tests__/types
mkdir -p app/Admin/src/features/catalog/option-types/__tests__/services
mkdir -p app/Admin/src/features/catalog/option-types/__tests__/validations
```

- [ ] Verify build passes with empty files (it should — Rolldown resolves lazy imports at runtime):
```bash
cd app/Admin && pnpm run build-only 2>&1 | tail -1
```

### Task 1: Types Layer

**Files:**
- Create: `app/Admin/src/features/catalog/option-types/types/optionType.ts`
- Create: `app/Admin/src/features/catalog/option-types/types/optionValue.ts`
- Create: `app/Admin/src/features/catalog/option-types/types/index.ts`
- Create: `app/Admin/src/features/catalog/option-types/__tests__/types/optionType.spec.ts`
- Create: `app/Admin/src/features/catalog/option-types/__tests__/types/optionValue.spec.ts`

**Interfaces:**
- Consumes: `QueryingParameters` from `@/shared/types/querying`
- Produces: `OptionTypeRequest`, `OptionTypeListItem`, `OptionTypeDetail`, `OptionTypeQuery`, `OPTION_TYPE_FILTER_FIELDS`, `OPTION_TYPE_SORT_FIELDS`, `toOptionTypeQueryParams`, `OptionValueRequest`, `OptionValueListItem`, `OptionValueDetail`, `OptionValueQuery`, `OPTION_VALUE_FILTER_FIELDS`, `OPTION_VALUE_SORT_FIELDS`, `toOptionValueQueryParams`

- [ ] **Step 1: Write `types/optionType.ts`**

```ts
import type { QueryingParameters } from '@/shared/types/querying'

export interface OptionTypeRequest {
  name: string
  presentation: string
  position: number
  filterable: boolean
}

export interface OptionTypeListItem extends OptionTypeRequest {
  id: string
  optionValuesCount: number
  productsCount: number
}

export interface OptionTypeDetail extends OptionTypeListItem {
  createdAtUtc: string
  modifiedAtUtc: string | null
  createdBy: string | null
  modifiedBy: string | null
}

export interface OptionTypeQuery {
  name?: string
  filterable?: boolean
  search?: string
  sortBy?: 'name' | 'presentation' | 'position' | 'optionValuesCount' | 'productsCount' | 'createdAtUtc' | 'modifiedAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const OPTION_TYPE_FILTER_FIELDS = [
  'name',
  'filterable',
  'optionValuesCount',
  'productsCount',
  'createdAtUtc',
  'modifiedAtUtc',
]

export const OPTION_TYPE_SORT_FIELDS = [
  'name',
  'presentation',
  'position',
  'optionValuesCount',
  'productsCount',
  'createdAtUtc',
  'modifiedAtUtc',
]

export function toOptionTypeQueryParams(query: OptionTypeQuery): QueryingParameters {
  const filters: string[] = []

  if (query.name !== undefined && query.name !== '') {
    filters.push(`name*=${query.name}`)
  }
  if (query.filterable !== undefined) {
    filters.push(`filterable=${query.filterable}`)
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

- [ ] **Step 2: Write `types/optionValue.ts`**

```ts
import type { QueryingParameters } from '@/shared/types/querying'

export interface OptionValueRequest {
  optionTypeId: string
  name: string
  presentation: string
  position: number
}

export interface OptionValueListItem extends OptionValueRequest {
  id: string
}

export interface OptionValueDetail extends OptionValueListItem {
  createdAtUtc: string
  modifiedAtUtc: string | null
}

export interface OptionValueQuery {
  optionTypeId?: string
  name?: string
  search?: string
  sortBy?: 'name' | 'presentation' | 'position' | 'createdAtUtc' | 'modifiedAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const OPTION_VALUE_FILTER_FIELDS = [
  'optionTypeId',
  'name',
  'createdAtUtc',
  'modifiedAtUtc',
]

export const OPTION_VALUE_SORT_FIELDS = [
  'name',
  'presentation',
  'position',
  'createdAtUtc',
  'modifiedAtUtc',
]

export function toOptionValueQueryParams(query: OptionValueQuery): QueryingParameters {
  const filters: string[] = []

  if (query.optionTypeId !== undefined && query.optionTypeId !== '') {
    filters.push(`optionTypeId=${query.optionTypeId}`)
  }
  if (query.name !== undefined && query.name !== '') {
    filters.push(`name*=${query.name}`)
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

- [ ] **Step 3: Write `types/index.ts`**

```ts
export type {
  OptionTypeRequest,
  OptionTypeListItem,
  OptionTypeDetail,
  OptionTypeQuery,
} from './optionType'
export {
  OPTION_TYPE_FILTER_FIELDS,
  OPTION_TYPE_SORT_FIELDS,
  toOptionTypeQueryParams,
} from './optionType'
export type {
  OptionValueRequest,
  OptionValueListItem,
  OptionValueDetail,
  OptionValueQuery,
} from './optionValue'
export {
  OPTION_VALUE_FILTER_FIELDS,
  OPTION_VALUE_SORT_FIELDS,
  toOptionValueQueryParams,
} from './optionValue'
```

- [ ] **Step 4: Write `__tests__/types/optionType.spec.ts`**

```ts
import { describe, it, expect } from 'vitest'
import { toOptionTypeQueryParams, OPTION_TYPE_FILTER_FIELDS, OPTION_TYPE_SORT_FIELDS } from '../../types/optionType'

describe('toOptionTypeQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toOptionTypeQueryParams({})
    expect(result.filter).toBeNull()
    expect(result.search).toBeNull()
    expect(result.sort).toBeNull()
  })

  it('builds filter DSL for name (contains operator)', () => {
    const result = toOptionTypeQueryParams({ name: 'Size' })
    expect(result.filter).toBe('name*=Size')
  })

  it('builds filter DSL for filterable=true', () => {
    const result = toOptionTypeQueryParams({ filterable: true })
    expect(result.filter).toBe('filterable=true')
  })

  it('builds filter DSL for filterable=false', () => {
    const result = toOptionTypeQueryParams({ filterable: false })
    expect(result.filter).toBe('filterable=false')
  })

  it('combines multiple filter conditions with comma', () => {
    const result = toOptionTypeQueryParams({ name: 'Color', filterable: true })
    expect(result.filter).toBe('name*=Color,filterable=true')
  })

  it('builds sort ascending', () => {
    const result = toOptionTypeQueryParams({ sortBy: 'name', sortDirection: 'asc' })
    expect(result.sort).toEqual(['name'])
  })

  it('builds sort descending', () => {
    const result = toOptionTypeQueryParams({ sortBy: 'createdAtUtc', sortDirection: 'desc' })
    expect(result.sort).toEqual(['-createdAtUtc'])
  })

  it('skips empty string values in filters', () => {
    const result = toOptionTypeQueryParams({ name: '', filterable: true })
    expect(result.filter).toBe('filterable=true')
  })
})

describe('OPTION_TYPE_FILTER_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(OPTION_TYPE_FILTER_FIELDS).toEqual([
      'name',
      'filterable',
      'optionValuesCount',
      'productsCount',
      'createdAtUtc',
      'modifiedAtUtc',
    ])
  })
})

describe('OPTION_TYPE_SORT_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(OPTION_TYPE_SORT_FIELDS).toEqual([
      'name',
      'presentation',
      'position',
      'optionValuesCount',
      'productsCount',
      'createdAtUtc',
      'modifiedAtUtc',
    ])
  })
})
```

- [ ] **Step 5: Write `__tests__/types/optionValue.spec.ts`**

```ts
import { describe, it, expect } from 'vitest'
import { toOptionValueQueryParams, OPTION_VALUE_FILTER_FIELDS, OPTION_VALUE_SORT_FIELDS } from '../../types/optionValue'

describe('toOptionValueQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toOptionValueQueryParams({})
    expect(result.filter).toBeNull()
  })

  it('builds filter for optionTypeId', () => {
    const result = toOptionValueQueryParams({ optionTypeId: 'abc-123' })
    expect(result.filter).toBe('optionTypeId=abc-123')
  })

  it('builds filter for name (contains)', () => {
    const result = toOptionValueQueryParams({ name: 'Red' })
    expect(result.filter).toBe('name*=Red')
  })

  it('combines optionTypeId and name', () => {
    const result = toOptionValueQueryParams({ optionTypeId: 'abc-123', name: 'Red' })
    expect(result.filter).toBe('optionTypeId=abc-123,name*=Red')
  })

  it('builds sort', () => {
    const result = toOptionValueQueryParams({ sortBy: 'position', sortDirection: 'asc' })
    expect(result.sort).toEqual(['position'])
  })

  it('skips empty string values', () => {
    const result = toOptionValueQueryParams({ optionTypeId: 'abc-123', name: '' })
    expect(result.filter).toBe('optionTypeId=abc-123')
  })
})

describe('OPTION_VALUE_FILTER_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(OPTION_VALUE_FILTER_FIELDS).toEqual([
      'optionTypeId',
      'name',
      'createdAtUtc',
      'modifiedAtUtc',
    ])
  })
})

describe('OPTION_VALUE_SORT_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(OPTION_VALUE_SORT_FIELDS).toEqual([
      'name',
      'presentation',
      'position',
      'createdAtUtc',
      'modifiedAtUtc',
    ])
  })
})
```

- [ ] **Step 6: Run type tests**
```bash
cd app/Admin && pnpm run test:unit -- run --reporter=verbose 2>&1 | grep -E "optionType|optionValue|Test Files|Tests"
```
Expected: All new tests pass, 416+ tests total.

- [ ] **Step 7: Commit**
```bash
git add app/Admin/src/features/catalog/option-types/types/
git add app/Admin/src/features/catalog/option-types/__tests__/types/
git commit -m "feat(catalog): add option type and option value type definitions"
```

---

### Task 2: Validations Layer

**Files:**
- Create: `app/Admin/src/features/catalog/option-types/validations/optionType.ts`
- Create: `app/Admin/src/features/catalog/option-types/validations/optionValue.ts`
- Create: `app/Admin/src/features/catalog/option-types/validations/index.ts`
- Create: `app/Admin/src/features/catalog/option-types/__tests__/validations/optionType.spec.ts`
- Create: `app/Admin/src/features/catalog/option-types/__tests__/validations/optionValue.spec.ts`

**Interfaces:**
- Consumes: `z` from `zod`
- Produces: `optionTypeName`, `optionTypePresentation`, `optionTypePosition`, `optionTypeFilterable`, `optionTypeSchema`, `OptionTypeForm`, `optionValueOptionTypeId`, `optionValueName`, `optionValuePresentation`, `optionValuePosition`, `optionValueSchema`, `OptionValueForm`

- [ ] **Step 1: Write `validations/optionType.ts`**

```ts
import { z } from 'zod'

export const optionTypeName = z.string()
  .min(1, 'Option type name is required.')
  .max(100, 'Option type name must not exceed 100 characters.')

export const optionTypePresentation = z.string()
  .min(1, 'Presentation is required.')
  .max(100, 'Presentation must not exceed 100 characters.')

export const optionTypePosition = z.number()
  .int()
  .min(-1, 'Position must be at least -1.')

export const optionTypeFilterable = z.boolean()

export const optionTypeSchema = z.object({
  name: optionTypeName,
  presentation: optionTypePresentation,
  position: optionTypePosition,
  filterable: optionTypeFilterable,
})

export type OptionTypeForm = z.infer<typeof optionTypeSchema>
```

- [ ] **Step 2: Write `validations/optionValue.ts`**

```ts
import { z } from 'zod'

export const optionValueOptionTypeId = z.string()
  .min(1, 'Option type is required.')

export const optionValueName = z.string()
  .min(1, 'Option value name is required.')
  .max(100, 'Option value name must not exceed 100 characters.')

export const optionValuePresentation = z.string()
  .min(1, 'Presentation is required.')
  .max(100, 'Presentation must not exceed 100 characters.')

export const optionValuePosition = z.number()
  .int()
  .min(-1, 'Position must be at least -1.')

export const optionValueSchema = z.object({
  optionTypeId: optionValueOptionTypeId,
  name: optionValueName,
  presentation: optionValuePresentation,
  position: optionValuePosition,
})

export type OptionValueForm = z.infer<typeof optionValueSchema>
```

- [ ] **Step 3: Write `validations/index.ts`**

```ts
export {
  optionTypeName,
  optionTypePresentation,
  optionTypePosition,
  optionTypeFilterable,
  optionTypeSchema,
} from './optionType'
export type { OptionTypeForm } from './optionType'
export {
  optionValueOptionTypeId,
  optionValueName,
  optionValuePresentation,
  optionValuePosition,
  optionValueSchema,
} from './optionValue'
export type { OptionValueForm } from './optionValue'
```

- [ ] **Step 4: Write `__tests__/validations/optionType.spec.ts`**

```ts
import { describe, it, expect } from 'vitest'
import {
  optionTypeName,
  optionTypePresentation,
  optionTypePosition,
  optionTypeFilterable,
  optionTypeSchema,
} from '../../validations/optionType'

describe('optionTypeName', () => {
  it('accepts a valid name', () => {
    expect(optionTypeName.safeParse('Size').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(optionTypeName.safeParse('').success).toBe(false)
  })

  it('rejects string over 100 characters', () => {
    expect(optionTypeName.safeParse('A'.repeat(101)).success).toBe(false)
  })

  it('accepts string of exactly 100 characters', () => {
    expect(optionTypeName.safeParse('A'.repeat(100)).success).toBe(true)
  })
})

describe('optionTypePresentation', () => {
  it('accepts a valid presentation', () => {
    expect(optionTypePresentation.safeParse('Select a size').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(optionTypePresentation.safeParse('').success).toBe(false)
  })

  it('rejects string over 100 characters', () => {
    expect(optionTypePresentation.safeParse('A'.repeat(101)).success).toBe(false)
  })
})

describe('optionTypePosition', () => {
  it('accepts position 0', () => {
    expect(optionTypePosition.safeParse(0).success).toBe(true)
  })

  it('accepts position -1', () => {
    expect(optionTypePosition.safeParse(-1).success).toBe(true)
  })

  it('rejects position -2', () => {
    expect(optionTypePosition.safeParse(-2).success).toBe(false)
  })

  it('rejects non-integer', () => {
    expect(optionTypePosition.safeParse(1.5).success).toBe(false)
  })
})

describe('optionTypeFilterable', () => {
  it('accepts true', () => {
    expect(optionTypeFilterable.safeParse(true).success).toBe(true)
  })

  it('accepts false', () => {
    expect(optionTypeFilterable.safeParse(false).success).toBe(true)
  })
})

describe('optionTypeSchema', () => {
  it('accepts valid form', () => {
    const result = optionTypeSchema.safeParse({
      name: 'Size',
      presentation: 'Select a size',
      position: 1,
      filterable: true,
    })
    expect(result.success).toBe(true)
  })

  it('rejects missing required name', () => {
    const result = optionTypeSchema.safeParse({
      name: '',
      presentation: 'Select a size',
      position: 1,
      filterable: true,
    })
    expect(result.success).toBe(false)
  })

  it('returns per-field errors', () => {
    const result = optionTypeSchema.safeParse({
      name: '',
      presentation: '',
      position: -2,
      filterable: true,
    })
    expect(result.success).toBe(false)
    const fields = result.error!.issues.map(i => String(i.path[0]))
    expect(fields).toContain('name')
    expect(fields).toContain('presentation')
    expect(fields).toContain('position')
  })
})
```

- [ ] **Step 5: Write `__tests__/validations/optionValue.spec.ts`**

```ts
import { describe, it, expect } from 'vitest'
import {
  optionValueOptionTypeId,
  optionValueName,
  optionValuePresentation,
  optionValuePosition,
  optionValueSchema,
} from '../../validations/optionValue'

describe('optionValueOptionTypeId', () => {
  it('accepts a valid GUID', () => {
    expect(optionValueOptionTypeId.safeParse('abc-123-def').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(optionValueOptionTypeId.safeParse('').success).toBe(false)
  })
})

describe('optionValueName', () => {
  it('accepts a valid name', () => {
    expect(optionValueName.safeParse('Medium').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(optionValueName.safeParse('').success).toBe(false)
  })

  it('rejects string over 100 characters', () => {
    expect(optionValueName.safeParse('A'.repeat(101)).success).toBe(false)
  })
})

describe('optionValuePresentation', () => {
  it('accepts a valid presentation', () => {
    expect(optionValuePresentation.safeParse('Medium').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(optionValuePresentation.safeParse('').success).toBe(false)
  })
})

describe('optionValuePosition', () => {
  it('accepts position 0', () => {
    expect(optionValuePosition.safeParse(0).success).toBe(true)
  })

  it('rejects position -2', () => {
    expect(optionValuePosition.safeParse(-2).success).toBe(false)
  })

  it('rejects non-integer', () => {
    expect(optionValuePosition.safeParse(1.5).success).toBe(false)
  })
})

describe('optionValueSchema', () => {
  it('accepts valid form', () => {
    const result = optionValueSchema.safeParse({
      optionTypeId: 'abc-123',
      name: 'Medium',
      presentation: 'Medium',
      position: 2,
    })
    expect(result.success).toBe(true)
  })

  it('rejects missing required fields', () => {
    const result = optionValueSchema.safeParse({
      optionTypeId: '',
      name: '',
      presentation: '',
      position: 1,
    })
    expect(result.success).toBe(false)
    const fields = result.error!.issues.map(i => String(i.path[0]))
    expect(fields).toContain('optionTypeId')
    expect(fields).toContain('name')
    expect(fields).toContain('presentation')
  })
})
```

- [ ] **Step 6: Run validation tests**
```bash
cd app/Admin && pnpm run test:unit -- run --reporter=verbose 2>&1 | grep -E "optionType|optionValue|Test Files|Tests"
```
Expected: All 416 + new validation tests pass.

- [ ] **Step 7: Commit**
```bash
git add app/Admin/src/features/catalog/option-types/validations/
git add app/Admin/src/features/catalog/option-types/__tests__/validations/
git commit -m "feat(catalog): add option type and option value Zod validations"
```

---

### Task 3: Services Layer

**Files:**
- Create: `app/Admin/src/features/catalog/option-types/services/optionTypeApi.ts`
- Create: `app/Admin/src/features/catalog/option-types/services/optionValueApi.ts`
- Create: `app/Admin/src/features/catalog/option-types/services/index.ts`
- Create: `app/Admin/src/features/catalog/option-types/__tests__/services/optionTypeApi.spec.ts`
- Create: `app/Admin/src/features/catalog/option-types/__tests__/services/optionValueApi.spec.ts`

**Interfaces:**
- Consumes: `post`, `get`, `put`, `del` from `@/shared/api/client`, `getPaged` from `@/shared/api`, `CATALOG` from `@/shared/constants/api`, types from Task 1, `Result`, `PagedResult` from `@/shared/types`
- Produces: `OptionTypeApi` (static class), `OptionValueApi` (static class)

- [ ] **Step 1: Write `services/optionTypeApi.ts`**

```ts
import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type {
  OptionTypeRequest,
  OptionTypeListItem,
  OptionTypeDetail,
  OptionTypeQuery,
} from '../types/optionType'
import {
  toOptionTypeQueryParams,
  OPTION_TYPE_FILTER_FIELDS,
  OPTION_TYPE_SORT_FIELDS,
} from '../types/optionType'

export class OptionTypeApi {
  private static readonly BASE = `${CATALOG}/option-types`

  static getOptionTypes(query: OptionTypeQuery): Promise<PagedResult<OptionTypeListItem>> {
    return getPaged<OptionTypeListItem>(OptionTypeApi.BASE, toOptionTypeQueryParams(query), {
      allowedFilterFields: OPTION_TYPE_FILTER_FIELDS,
      allowedSortFields: OPTION_TYPE_SORT_FIELDS,
    })
  }

  static getOptionType(id: string): Promise<Result<OptionTypeDetail>> {
    return get<Result<OptionTypeDetail>>(`${OptionTypeApi.BASE}/${id}`)
  }

  static createOptionType(request: OptionTypeRequest): Promise<Result<OptionTypeDetail>> {
    return post<Result<OptionTypeDetail>>(OptionTypeApi.BASE, request)
  }

  static updateOptionType(id: string, request: OptionTypeRequest): Promise<Result<OptionTypeDetail>> {
    return put<Result<OptionTypeDetail>>(`${OptionTypeApi.BASE}/${id}`, request)
  }

  static deleteOptionType(id: string): Promise<Result<OptionTypeListItem>> {
    return del<Result<OptionTypeListItem>>(`${OptionTypeApi.BASE}/${id}`)
  }
}
```

- [ ] **Step 2: Write `services/optionValueApi.ts`**

```ts
import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type {
  OptionValueRequest,
  OptionValueListItem,
  OptionValueDetail,
  OptionValueQuery,
} from '../types/optionValue'
import {
  toOptionValueQueryParams,
  OPTION_VALUE_FILTER_FIELDS,
  OPTION_VALUE_SORT_FIELDS,
} from '../types/optionValue'

export class OptionValueApi {
  private static readonly BASE = `${CATALOG}/option-types/option-values`

  static getOptionValues(query: OptionValueQuery): Promise<PagedResult<OptionValueListItem>> {
    return getPaged<OptionValueListItem>(OptionValueApi.BASE, toOptionValueQueryParams(query), {
      allowedFilterFields: OPTION_VALUE_FILTER_FIELDS,
      allowedSortFields: OPTION_VALUE_SORT_FIELDS,
    })
  }

  static getOptionValue(id: string): Promise<Result<OptionValueDetail>> {
    return get<Result<OptionValueDetail>>(`${OptionValueApi.BASE}/${id}`)
  }

  static createOptionValue(request: OptionValueRequest): Promise<Result<OptionValueDetail>> {
    return post<Result<OptionValueDetail>>(OptionValueApi.BASE, request)
  }

  static updateOptionValue(id: string, request: OptionValueRequest): Promise<Result<OptionValueDetail>> {
    return put<Result<OptionValueDetail>>(`${OptionValueApi.BASE}/${id}`, request)
  }

  static deleteOptionValue(id: string): Promise<Result<OptionValueListItem>> {
    return del<Result<OptionValueListItem>>(`${OptionValueApi.BASE}/${id}`)
  }
}
```

- [ ] **Step 3: Write `services/index.ts`**

```ts
export { OptionTypeApi } from './optionTypeApi'
export { OptionValueApi } from './optionValueApi'
```

- [ ] **Step 4: Write `__tests__/services/optionTypeApi.spec.ts`**

```ts
import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockGet, mockPut, mockDel, mockGetPaged } = vi.hoisted(() => ({
  mockPost: vi.fn<any>(),
  mockGet: vi.fn<any>(),
  mockPut: vi.fn<any>(),
  mockDel: vi.fn<any>(),
  mockGetPaged: vi.fn<any>(),
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

import { OptionTypeApi } from '../optionTypeApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('OptionTypeApi.getOptionTypes', () => {
  it('calls getPaged with option type query params', async () => {
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

    await OptionTypeApi.getOptionTypes({ filterable: true, page: 1, pageSize: 10 })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/catalog/option-types',
      { filter: 'filterable=true', search: null, sort: null, pageNumber: 1, pageSize: 10 },
      expect.objectContaining({ allowedFilterFields: expect.any(Array) }),
    )
  })
})

describe('OptionTypeApi.getOptionType', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: '1', name: 'Size' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await OptionTypeApi.getOptionType('abc-123')
    expect(mockGet).toHaveBeenCalledWith('api/catalog/option-types/abc-123')
  })
})

describe('OptionTypeApi.createOptionType', () => {
  it('calls POST with request body', async () => {
    const req = { name: 'Size', presentation: 'Select a size', position: 1, filterable: true }
    mockPost.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })

    await OptionTypeApi.createOptionType(req)
    expect(mockPost).toHaveBeenCalledWith('api/catalog/option-types', req)
  })
})

describe('OptionTypeApi.updateOptionType', () => {
  it('calls PUT with request body', async () => {
    const req = { name: 'Size', presentation: 'Select a size', position: 1, filterable: false }
    mockPut.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await OptionTypeApi.updateOptionType('abc-123', req)
    expect(mockPut).toHaveBeenCalledWith('api/catalog/option-types/abc-123', req)
  })
})

describe('OptionTypeApi.deleteOptionType', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: { id: '1', name: 'Size' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await OptionTypeApi.deleteOptionType('abc-123')
    expect(mockDel).toHaveBeenCalledWith('api/catalog/option-types/abc-123')
  })
})
```

- [ ] **Step 5: Write `__tests__/services/optionValueApi.spec.ts`**

```ts
import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockGet, mockPut, mockDel, mockGetPaged } = vi.hoisted(() => ({
  mockPost: vi.fn<any>(),
  mockGet: vi.fn<any>(),
  mockPut: vi.fn<any>(),
  mockDel: vi.fn<any>(),
  mockGetPaged: vi.fn<any>(),
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

import { OptionValueApi } from '../optionValueApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('OptionValueApi.getOptionValues', () => {
  it('calls getPaged with optionTypeId filter', async () => {
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

    await OptionValueApi.getOptionValues({ optionTypeId: 'abc-123' })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/catalog/option-types/option-values',
      expect.objectContaining({ filter: 'optionTypeId=abc-123' }),
      expect.any(Object),
    )
  })
})

describe('OptionValueApi.getOptionValue', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: '1', name: 'Medium' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await OptionValueApi.getOptionValue('xyz-456')
    expect(mockGet).toHaveBeenCalledWith('api/catalog/option-types/option-values/xyz-456')
  })
})

describe('OptionValueApi.createOptionValue', () => {
  it('calls POST with request body', async () => {
    const req = { optionTypeId: 'abc-123', name: 'Medium', presentation: 'Medium', position: 2 }
    mockPost.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })

    await OptionValueApi.createOptionValue(req)
    expect(mockPost).toHaveBeenCalledWith('api/catalog/option-types/option-values', req)
  })
})

describe('OptionValueApi.updateOptionValue', () => {
  it('calls PUT with request body', async () => {
    const req = { optionTypeId: 'abc-123', name: 'Large', presentation: 'Large', position: 3 }
    mockPut.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await OptionValueApi.updateOptionValue('xyz-456', req)
    expect(mockPut).toHaveBeenCalledWith('api/catalog/option-types/option-values/xyz-456', req)
  })
})

describe('OptionValueApi.deleteOptionValue', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: { id: '1', name: 'Medium' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await OptionValueApi.deleteOptionValue('xyz-456')
    expect(mockDel).toHaveBeenCalledWith('api/catalog/option-types/option-values/xyz-456')
  })
})
```

- [ ] **Step 6: Run service tests**
```bash
cd app/Admin && pnpm run test:unit -- run --reporter=verbose 2>&1 | grep -E "OptionTypeApi|OptionValueApi|Test Files|Tests"
```
Expected: All tests pass, 416 + new service tests.

- [ ] **Step 7: Commit**
```bash
git add app/Admin/src/features/catalog/option-types/services/
git add app/Admin/src/features/catalog/option-types/__tests__/services/
git commit -m "feat(catalog): add option type and option value API services"
```

---

### Task 4: Store Layer

**Files:**
- Create: `app/Admin/src/features/catalog/option-types/stores/optionTypeStore.ts`
- Create: `app/Admin/src/features/catalog/option-types/stores/index.ts`

**Interfaces:**
- Consumes: `defineStore` from `pinia`, `ref` from `vue`, `OptionTypeListItem` from Task 1, `OptionTypeApi` from Task 3
- Produces: `useOptionTypeStore` (Pinia composition store)

- [ ] **Step 1: Write `stores/optionTypeStore.ts`**

```ts
import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { OptionTypeListItem } from '../types/optionType'
import { OptionTypeApi } from '../services/optionTypeApi'

export const useOptionTypeStore = defineStore('optionTypes', () => {
  const activeOptionTypes = ref<OptionTypeListItem[]>([])
  const loaded = ref(false)

  async function fetchActive(): Promise<void> {
    if (loaded.value) return

    const result = await OptionTypeApi.getOptionTypes({
      pageSize: 100,
      sortBy: 'name',
      sortDirection: 'asc',
    })

    if (result.isSuccess) {
      activeOptionTypes.value = result.items
      loaded.value = true
    }
  }

  return { activeOptionTypes, loaded, fetchActive }
})
```

- [ ] **Step 2: Write `stores/index.ts`**

```ts
export { useOptionTypeStore } from './optionTypeStore'
```

- [ ] **Step 3: Verify build compiles with store**
```bash
cd app/Admin && pnpm run build-only 2>&1 | tail -1
```

- [ ] **Step 4: Commit**
```bash
git add app/Admin/src/features/catalog/option-types/stores/
git commit -m "feat(catalog): add option type store for dropdown caching"
```

---

### Task 5: OptionValueFormDialog Component

**Files:**
- Create: `app/Admin/src/features/catalog/option-types/components/OptionValueFormDialog.vue`
- Create: `app/Admin/src/features/catalog/option-types/components/index.ts`
- Create: `app/Admin/src/features/catalog/option-types/composables/index.ts`

**Interfaces:**
- Consumes: `Dialog` from `primevue/dialog`, `useNotify`, `useApiErrorHandler`, `OptionValueApi` (Task 3), `optionValueSchema` (Task 2), `OptionValueForm` (Task 2), `OptionValueListItem` (Task 1)
- Produces: `OptionValueFormDialog` component with props `visible: boolean`, `optionTypeId: string`, `editingValue: OptionValueListItem | null`; emits `update:visible`, `saved`

- [ ] **Step 1: Write `components/OptionValueFormDialog.vue`**

```vue
<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import Dialog from 'primevue/dialog'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { OptionValueApi } from '../services/optionValueApi'
import { optionValueSchema } from '../validations/optionValue'
import type { OptionValueForm } from '../validations/optionValue'
import type { OptionValueListItem } from '../types/optionValue'

interface Props {
  visible: boolean
  optionTypeId: string
  editingValue: OptionValueListItem | null
}

const props = defineProps<Props>()

const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void
  (e: 'saved'): void
}>()

const notify = useNotify()
const { handleResult } = useApiErrorHandler()

const isEdit = computed(() => !!props.editingValue)
const dialogTitle = computed(() => isEdit.value ? 'Edit Option Value' : 'Add Option Value')

const form = ref<OptionValueForm>({
  optionTypeId: props.optionTypeId,
  name: '',
  presentation: '',
  position: 1,
})

const fieldErrors = ref<Record<string, string>>({})
const saving = ref(false)

watch(
  () => props.visible,
  (v) => {
    if (v) {
      fieldErrors.value = {}
      if (props.editingValue) {
        form.value = {
          optionTypeId: props.editingValue.optionTypeId,
          name: props.editingValue.name,
          presentation: props.editingValue.presentation,
          position: props.editingValue.position,
        }
      } else {
        form.value = {
          optionTypeId: props.optionTypeId,
          name: '',
          presentation: '',
          position: 1,
        }
      }
    }
  },
)

function close() {
  emit('update:visible', false)
}

async function onSave() {
  fieldErrors.value = {}
  const parsed = optionValueSchema.safeParse(form.value)

  if (!parsed.success) {
    for (const issue of parsed.error.issues) {
      const field = String(issue.path[0])
      if (!fieldErrors.value[field]) {
        fieldErrors.value[field] = issue.message
      }
    }
    return
  }

  saving.value = true
  const data = parsed.data
  const request = {
    optionTypeId: data.optionTypeId,
    name: data.name,
    presentation: data.presentation,
    position: data.position,
  }

  const result = isEdit.value
    ? await OptionValueApi.updateOptionValue(props.editingValue!.id, request)
    : await OptionValueApi.createOptionValue(request)

  saving.value = false

  if (result.isSuccess) {
    notify.success(isEdit.value ? 'Option value updated' : 'Option value created')
    close()
    emit('saved')
  } else {
    handleResult(result)
  }
}
</script>

<template>
  <Dialog
    :visible="visible"
    :header="dialogTitle"
    :modal="true"
    :style="{ width: '450px' }"
    @update:visible="close"
  >
    <div class="flex flex-col gap-4">
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium">Name</label>
        <InputText v-model="form.name" fluid :invalid="!!fieldErrors.name" />
        <small v-if="fieldErrors.name" class="text-red-500">{{ fieldErrors.name }}</small>
      </div>
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium">Presentation</label>
        <InputText v-model="form.presentation" fluid :invalid="!!fieldErrors.presentation" />
        <small v-if="fieldErrors.presentation" class="text-red-500">{{ fieldErrors.presentation }}</small>
      </div>
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium">Position</label>
        <InputNumber v-model="form.position" fluid :min="-1" :invalid="!!fieldErrors.position" />
        <small v-if="fieldErrors.position" class="text-red-500">{{ fieldErrors.position }}</small>
      </div>
    </div>
    <template #footer>
      <Button label="Cancel" severity="secondary" @click="close" />
      <Button label="Save" severity="primary" :loading="saving" @click="onSave" />
    </template>
  </Dialog>
</template>
```

- [ ] **Step 2: Write `components/index.ts`**

```ts
export { default as OptionValueFormDialog } from './OptionValueFormDialog.vue'
```

- [ ] **Step 3: Write `composables/index.ts`**

```ts
export {}
```

- [ ] **Step 4: Verify build compiles**
```bash
cd app/Admin && pnpm run build-only 2>&1 | tail -1
```

- [ ] **Step 5: Commit**
```bash
git add app/Admin/src/features/catalog/option-types/components/
git add app/Admin/src/features/catalog/option-types/composables/
git commit -m "feat(catalog): add OptionValue form dialog component"
```

---

### Task 6: OptionTypesList View

**Files:**
- Create: `app/Admin/src/features/catalog/option-types/views/OptionTypesList.vue`

**Interfaces:**
- Consumes: `useRouter`, `useConfirm`, `useNotify`, `useDataTableExport`, `usePagedQuery`, `PageShell`, `Card`, `DataTable`, `Column`, `Toolbar`, `Tag`, `OptionTypeApi` (Task 3), `OptionTypeListItem` (Task 1), `OPTION_TYPE_FILTER_FIELDS` (Task 1), `OPTION_TYPE_SORT_FIELDS` (Task 1)
- Produces: Lazy-loaded view at route `/catalog/option-types`

- [ ] **Step 1: Write `views/OptionTypesList.vue`**

```vue
<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Toolbar from 'primevue/toolbar'
import Tag from 'primevue/tag'
import Plus from '@primeicons/vue/plus'
import Trash from '@primeicons/vue/trash'
import Upload from '@primeicons/vue/upload'
import { PageShell } from '@panel'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useNotify } from '@/shared/composables/useNotify'
import { OptionTypeApi } from '../services/optionTypeApi'
import type { OptionTypeListItem } from '../types/optionType'
import { OPTION_TYPE_FILTER_FIELDS, OPTION_TYPE_SORT_FIELDS } from '../types/optionType'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()

const { dt, exportCSV } = useDataTableExport()
const selectedItems = ref<OptionTypeListItem[]>([])
const searchTerm = ref('')
const allowedSearchFields = ['name', 'presentation']

const {
  items,
  loading,
  totalCount,
  page,
  pageSize,
  setSearch,
  refresh,
} = usePagedQuery<OptionTypeListItem>('api/catalog/option-types', {
  allowedFilterFields: OPTION_TYPE_FILTER_FIELDS,
  allowedSortFields: OPTION_TYPE_SORT_FIELDS,
  allowedSearchFields,
  defaultSearchFields: allowedSearchFields,
  defaultSearchMode: 'any',
  defaultSort: ['name'],
  defaultPageSize: 20,
})

function navigateToNew() {
  router.push('/catalog/option-types/new')
}

function navigateToEdit(id: string) {
  router.push(`/catalog/option-types/${id}`)
}

function onSearch(value: string) {
  searchTerm.value = value
  setSearch(value)
}

function clearSearch() {
  searchTerm.value = ''
  setSearch('')
}

function confirmDelete() {
  if (selectedItems.value.length === 0) return

  confirm.require({
    message: `Are you sure you want to delete ${selectedItems.value.length > 1 ? 'these option types' : 'this option type'}?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const ids = selectedItems.value.map(i => i.id)
      const names = selectedItems.value.map(i => i.name)
      let failed = 0
      for (const id of ids) {
        const result = await OptionTypeApi.deleteOptionType(id)
        if (!result.isSuccess) failed++
      }
      selectedItems.value = []
      refresh()
      if (failed === 0) {
        notify.success(
          ids.length > 1 ? 'Option types deleted' : 'Option type deleted',
          ids.length > 1
            ? `${ids.length} option types have been removed.`
            : `${names[0]} has been removed.`,
        )
      } else {
        notify.error(
          'Delete failed',
          `${failed} of ${ids.length} could not be deleted.`,
        )
      }
    },
  })
}
</script>

<template>
  <PageShell title="Option Types" description="Manage product option types (Size, Color, Material, etc.)">
    <Card>
      <template #content>
        <Toolbar>
          <template #start>
            <Button label="New" severity="secondary" class="mr-2" @click="navigateToNew">
              <Plus />
            </Button>
            <Button label="Delete" severity="secondary" :disabled="selectedItems.length === 0" @click="confirmDelete">
              <Trash />
            </Button>
          </template>
          <template #end>
            <Button label="Export" severity="secondary" @click="exportCSV">
              <Upload />
            </Button>
          </template>
        </Toolbar>
      </template>
    </Card>

    <DataTable
      ref="dt"
      v-model:selection="selectedItems"
      :value="items"
      :loading="loading"
      :paginator="true"
      :rows="pageSize"
      filter-display="menu"
      data-key="id"
      :global-filter-fields="allowedSearchFields"
      paginator-template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
      :rows-per-page-options="[5, 10, 25]"
      current-page-report-template="Showing {first} to {last} of {totalRecords}"
    >
      <Column selection-mode="multiple" header-style="width: 3rem" />
      <template #header>
        <div class="flex justify-between items-center">
          <IconField>
            <InputIcon><i class="pi pi-search" /></InputIcon>
            <InputText
              :model-value="searchTerm"
              placeholder="Search option types..."
              @update:model-value="onSearch($event ?? '')"
            />
          </IconField>
          <Button label="Clear" outlined @click="clearSearch" />
        </div>
      </template>
      <Column field="name" header="Name" :sortable="true" :filter="true" filter-field="name" />
      <Column field="presentation" header="Presentation" :sortable="true" />
      <Column field="position" header="Position" :sortable="true" />
      <Column field="filterable" header="Filterable" :sortable="true" body-style="text-align: center">
        <template #body="{ data }">
          <Tag :value="data.filterable ? 'Yes' : 'No'" :severity="data.filterable ? 'success' : 'secondary'" />
        </template>
      </Column>
      <Column field="optionValuesCount" header="Values" :sortable="true" />
      <Column field="productsCount" header="Products" :sortable="true" />
      <Column header="" body-style="text-align: right; width: 6rem">
        <template #body="{ data }">
          <div class="flex justify-end gap-2">
            <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEdit(data.id)" />
            <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="selectedItems = [data]; confirmDelete()" />
          </div>
        </template>
      </Column>
      <template #empty>
        <div class="text-center py-8 text-muted-color">No option types found.</div>
      </template>
    </DataTable>
  </PageShell>
</template>
```

- [ ] **Step 2: Verify build compiles with the view**
```bash
cd app/Admin && pnpm run build-only 2>&1 | tail -1
```

- [ ] **Step 3: Commit**
```bash
git add app/Admin/src/features/catalog/option-types/views/OptionTypesList.vue
git commit -m "feat(catalog): implement OptionTypes list view with inline DataTable"
```

---

### Task 7: OptionTypeDetail View (form + tabbed OptionValues)

**Files:**
- Create: `app/Admin/src/features/catalog/option-types/views/OptionTypeDetail.vue`
- Create: `app/Admin/src/features/catalog/option-types/views/index.ts`

**Interfaces:**
- Consumes: `useRoute`, `useRouter`, `useConfirm`, `useNotify`, `useApiErrorHandler`, `usePagedQuery`, `Tabs`, `TabList`, `Tab`, `TabPanels`, `TabPanel`, `DataTable`, `Column`, `Toolbar`, `Card`, `PageShell`, `PageHeading`, `FormSection`, `FormField`, `OptionTypeApi` (Task 3), `OptionValueApi` (Task 3), `optionTypeSchema` (Task 2), `OptionTypeForm` (Task 2), `OptionValueListItem` (Task 1), `OPTION_VALUE_FILTER_FIELDS` (Task 1), `OPTION_VALUE_SORT_FIELDS` (Task 1), `OptionValueFormDialog` (Task 5)
- Produces: Lazy-loaded view at route `/catalog/option-types/:id`

- [ ] **Step 1: Write `views/OptionTypeDetail.vue`**

```vue
<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import TabPanels from 'primevue/tabpanels'
import TabPanel from 'primevue/tabpanel'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Toolbar from 'primevue/toolbar'
import Plus from '@primeicons/vue/plus'
import { PageShell, PageHeading } from '@panel'
import { FormSection, FormField } from '@form'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { OptionTypeApi } from '../services/optionTypeApi'
import { OptionValueApi } from '../services/optionValueApi'
import { optionTypeSchema } from '../validations/optionType'
import type { OptionTypeForm } from '../validations/optionType'
import type { OptionValueListItem } from '../types/optionValue'
import { OPTION_VALUE_FILTER_FIELDS, OPTION_VALUE_SORT_FIELDS } from '../types/optionValue'
import OptionValueFormDialog from '../components/OptionValueFormDialog.vue'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const confirm = useConfirm()
const { handleResult } = useApiErrorHandler()

const isEdit = computed(() => !!route.params.id)
const pageTitle = computed(() => isEdit.value ? 'Edit Option Type' : 'New Option Type')
const activeTab = ref('0')

const form = ref<OptionTypeForm>({
  name: '',
  presentation: '',
  position: 1,
  filterable: false,
})

const fieldErrors = ref<Record<string, string>>({})
const saving = ref(false)

const dialogVisible = ref(false)
const editingValue = ref<OptionValueListItem | null>(null)

const valueSearchFields = ['name', 'presentation']

const {
  items: optionValues,
  loading: valuesLoading,
  setSearch: setValueSearch,
  setFilter: setValueFilter,
  refresh: refreshValues,
} = usePagedQuery<OptionValueListItem>('api/catalog/option-types/option-values', {
  allowedFilterFields: OPTION_VALUE_FILTER_FIELDS,
  allowedSortFields: OPTION_VALUE_SORT_FIELDS,
  allowedSearchFields: valueSearchFields,
  defaultSearchFields: valueSearchFields,
  defaultSearchMode: 'any',
  defaultSort: ['position', 'name'],
  defaultPageSize: 20,
})

const valueSearchTerm = ref('')

onMounted(async () => {
  if (isEdit.value) {
    const id = route.params.id as string
    setValueFilter(`optionTypeId=${id}`)

    const result = await OptionTypeApi.getOptionType(id)
    if (result.isSuccess) {
      const ot = result.value
      form.value = {
        name: ot.name,
        presentation: ot.presentation,
        position: ot.position,
        filterable: ot.filterable,
      }
    } else {
      handleResult(result)
      router.push('/catalog/option-types')
    }
  }
})

async function onSave() {
  fieldErrors.value = {}
  const parsed = optionTypeSchema.safeParse(form.value)

  if (!parsed.success) {
    for (const issue of parsed.error.issues) {
      const field = String(issue.path[0])
      if (!fieldErrors.value[field]) {
        fieldErrors.value[field] = issue.message
      }
    }
    return
  }

  saving.value = true
  const data = parsed.data
  const request = {
    name: data.name,
    presentation: data.presentation,
    position: data.position,
    filterable: data.filterable,
  }

  const result = isEdit.value
    ? await OptionTypeApi.updateOptionType(route.params.id as string, request)
    : await OptionTypeApi.createOptionType(request)

  saving.value = false

  if (result.isSuccess) {
    notify.success(isEdit.value ? 'Option type updated' : 'Option type created')
    if (!isEdit.value && result.value) {
      router.push(`/catalog/option-types/${result.value.id}`)
    }
  } else {
    handleResult(result)
  }
}

function onCancel() {
  router.push('/catalog/option-types')
}

function openAddDialog() {
  editingValue.value = null
  dialogVisible.value = true
}

function openEditDialog(value: OptionValueListItem) {
  editingValue.value = value
  dialogVisible.value = true
}

function onDialogSaved() {
  refreshValues()
}

function confirmDeleteValue(value: OptionValueListItem) {
  confirm.require({
    message: `Are you sure you want to delete "${value.name}"?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const result = await OptionValueApi.deleteOptionValue(value.id)
      if (result.isSuccess) {
        notify.success('Option value deleted', `${value.name} has been removed.`)
        refreshValues()
      } else {
        notify.error('Delete failed', result.errors?.[0]?.message ?? 'Could not delete option value.')
      }
    },
  })
}

function onValueSearch(value: string) {
  valueSearchTerm.value = value
  setValueSearch(value)
}
</script>

<template>
  <PageShell :title="pageTitle">
    <PageHeading
      title=""
      :breadcrumbs="[
        { label: 'Home', to: '/' },
        { label: 'Option Types', to: '/catalog/option-types' },
        { label: pageTitle },
      ]"
      :actions="[
        { label: 'Save', icon: 'pi pi-check', severity: 'primary' },
        { label: 'Cancel', icon: 'pi pi-times' },
      ]"
      @action="(i: number) => i === 0 ? onSave() : onCancel()"
    />

    <Tabs v-model:value="activeTab">
      <TabList>
        <Tab value="0">General</Tab>
        <Tab v-if="isEdit" value="1">Option Values</Tab>
      </TabList>

      <TabPanels>
        <TabPanel value="0">
          <FormSection title="Option Type Details">
            <FormField label="Name" :required="true" :invalid="!!fieldErrors.name">
              <InputText v-model="form.name" fluid class="w-full" />
              <small v-if="fieldErrors.name" class="text-red-500">{{ fieldErrors.name }}</small>
            </FormField>
            <FormField label="Presentation" :required="true" :invalid="!!fieldErrors.presentation" help-text="Display text shown to customers">
              <InputText v-model="form.presentation" fluid class="w-full" />
              <small v-if="fieldErrors.presentation" class="text-red-500">{{ fieldErrors.presentation }}</small>
            </FormField>
            <FormField label="Position" :invalid="!!fieldErrors.position" help-text="Sort order (lower = first)">
              <InputNumber v-model="form.position" fluid :min="-1" class="w-full" />
              <small v-if="fieldErrors.position" class="text-red-500">{{ fieldErrors.position }}</small>
            </FormField>
            <FormField label="Filterable" help-text="Show in storefront filter panel">
              <ToggleSwitch v-model="form.filterable" />
            </FormField>
          </FormSection>
        </TabPanel>

        <TabPanel v-if="isEdit" value="1">
          <Card>
            <template #content>
              <Toolbar>
                <template #start>
                  <Button label="Add Value" severity="secondary" @click="openAddDialog">
                    <Plus />
                  </Button>
                </template>
              </Toolbar>
            </template>
          </Card>

          <DataTable
            :value="optionValues"
            :loading="valuesLoading"
            data-key="id"
            :global-filter-fields="valueSearchFields"
          >
            <template #header>
              <div class="flex justify-between items-center">
                <IconField>
                  <InputIcon><i class="pi pi-search" /></InputIcon>
                  <InputText
                    :model-value="valueSearchTerm"
                    placeholder="Search values..."
                    @update:model-value="onValueSearch($event ?? '')"
                  />
                </IconField>
              </div>
            </template>
            <Column field="name" header="Name" :sortable="true" />
            <Column field="presentation" header="Presentation" :sortable="true" />
            <Column field="position" header="Position" :sortable="true" />
            <Column header="" body-style="text-align: right; width: 8rem">
              <template #body="{ data }">
                <div class="flex justify-end gap-2">
                  <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="openEditDialog(data)" />
                  <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="confirmDeleteValue(data)" />
                </div>
              </template>
            </Column>
            <template #empty>
              <div class="text-center py-8 text-muted-color">No option values defined.</div>
            </template>
          </DataTable>
        </TabPanel>
      </TabPanels>
    </Tabs>

    <OptionValueFormDialog
      :visible="dialogVisible"
      :option-type-id="(route.params.id as string) || ''"
      :editing-value="editingValue"
      @update:visible="dialogVisible = $event"
      @saved="onDialogSaved"
    />
  </PageShell>
</template>
```

- [ ] **Step 2: Write `views/index.ts`**

```ts
export { default as OptionTypesList } from './OptionTypesList.vue'
export { default as OptionTypeDetail } from './OptionTypeDetail.vue'
```

- [ ] **Step 3: Run full build and test suite**
```bash
cd app/Admin && pnpm run build-only 2>&1 | tail -1 && pnpm run test:unit -- run 2>&1 | grep 'Test Files\|Tests'
```
Expected: Build passes, all 416+ tests pass.

- [ ] **Step 4: Run lint check**
```bash
cd app/Admin && pnpm run lint 2>&1 | tail -5
```
Fix any lint errors before committing.

- [ ] **Step 5: Commit**
```bash
git add app/Admin/src/features/catalog/option-types/views/
git commit -m "feat(catalog): implement OptionType detail view with tabbed OptionValues"
```
