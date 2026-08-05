# Catalog Taxonomies, Taxons & Taxon Rules — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build Admin SPA management UI for Catalog Taxonomy/Taxon/TaxonRule CRUD with TreeTable, dual-view Taxons list, and 5-tab Taxon detail form.

**Architecture:** Full Location-module replication (types → services → stores → validations → views). Three-layer hierarchy: Taxonomies list, standalone Taxons list with DataTable/TreeTable toggle, Taxon detail with 5-tab form + embedded Rules table with dialog CRUD.

**Tech Stack:** Vue 3 + TypeScript, PrimeVue v5 (TreeTable, DataTable, Tabs, Dialog), Zod, Pinia, Vitest, @primeicons/vue

## Global Constraints

- Must pass `pnpm run lint` and `pnpm run build-only` with zero errors
- Must pass all existing 472 tests (no regressions)
- No new npm dependencies
- `catalog/taxons` and `catalog/taxons/:id` routes must appear BEFORE `catalog/taxonomies/:id` in routes array to avoid `:id` capturing `"taxons"`
- `isEdit` must exclude `route.params.id === 'new'` (same fix as OptionTypeDetail)
- `getTree()` response shape: `Result<{ tree: TaxonTreeItem[], breadcrumbs: TaxonTreeItem[], focusedNode: null, focusedSubtree: null }>` — extract via `result.value?.tree`
- Follow existing conventions: no comments, static API classes, Zod individual + combined schema, Pinia with loaded guard, inline PrimeVue components
- Views are lazy-loaded via dynamic import in routes

## File Structure

```
catalog/
├── types/
│   ├── taxonomy.ts              (new)
│   ├── taxon.ts                 (new)
│   ├── taxonRule.ts             (new)
│   └── index.ts                 (modify — add 3 new barrels + merge existing)
├── services/
│   ├── taxonomyApi.ts           (new)
│   ├── taxonApi.ts              (new)
│   ├── taxonRuleApi.ts          (new)
│   └── index.ts                 (modify — add new exports)
├── stores/
│   ├── taxonomyStore.ts         (new)
│   └── index.ts                 (modify — add new export)
├── validations/
│   ├── taxonomy.ts              (new)
│   ├── taxon.ts                 (new)
│   ├── taxonRule.ts             (new)
│   └── index.ts                 (modify — add new exports)
├── components/
│   ├── TaxonRuleFormDialog.vue  (new)
│   └── index.ts                 (modify — add new export)
├── views/
│   ├── TaxonomiesList.vue       (modify — replace stub)
│   ├── TaxonomyDetail.vue       (modify — replace stub)
│   ├── TaxonsList.vue           (new — create file)
│   ├── TaxonDetail.vue          (new — create file)
│   └── index.ts                 (modify — add new exports)
├── __tests__/
│   ├── types/       (3 new spec files)
│   ├── services/    (3 new spec files)
│   └── validations/ (3 new spec files)
└── routes/
    └── index.ts                 (modify — add 2 routes + 1 menu item)
```

---
### Task 1: Taxonomy/Taxon/TaxonRule Types Layer + Tests

**Files:**
- Create: `app/Admin/src/features/catalog/types/taxonomy.ts`
- Create: `app/Admin/src/features/catalog/types/taxon.ts`
- Create: `app/Admin/src/features/catalog/types/taxonRule.ts`
- Modify: `app/Admin/src/features/catalog/types/index.ts`
- Create: `app/Admin/src/features/catalog/__tests__/types/taxonomy.spec.ts`
- Create: `app/Admin/src/features/catalog/__tests__/types/taxon.spec.ts`
- Create: `app/Admin/src/features/catalog/__tests__/types/taxonRule.spec.ts`

**Interfaces:**
- Consumes: `QueryingParameters` from `@/shared/types/querying`
- Produces: All taxonomy/taxon/taxonRule interfaces, const arrays, and converter functions used by Tasks 2-10

**All code is verbatim. Write each file exactly as shown.**

- [ ] **Step 1: Write `types/taxonomy.ts`**

```ts
import type { QueryingParameters } from '@/shared/types/querying'

export interface TaxonomyRequest {
  name: string
  presentation: string
  position: number
}

export interface TaxonomyListItem extends TaxonomyRequest {
  id: string
  taxonsCount: number
  createdAtUtc: string
  modifiedAtUtc: string | null
}

export interface TaxonomyDetail extends TaxonomyListItem {
  createdBy: string | null
  modifiedBy: string | null
}

export interface TaxonomyQuery {
  name?: string
  search?: string
  sortBy?: 'name' | 'presentation' | 'position' | 'taxonsCount' | 'createdAtUtc' | 'modifiedAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const TAXONOMY_FILTER_FIELDS = [
  'name',
  'taxonsCount',
  'createdAtUtc',
  'modifiedAtUtc',
]

export const TAXONOMY_SORT_FIELDS = [
  'name',
  'presentation',
  'position',
  'taxonsCount',
  'createdAtUtc',
  'modifiedAtUtc',
]

export function toTaxonomyQueryParams(query: TaxonomyQuery): QueryingParameters {
  const filters: string[] = []

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

- [ ] **Step 2: Write `types/taxon.ts`**

```ts
import type { QueryingParameters } from '@/shared/types/querying'

export interface TaxonRequest {
  taxonomyId: string
  parentId: string | null
  name: string
  presentation: string
  description: string | null
  slug: string
  position: number
  metaTitle: string | null
  metaDescription: string | null
  metaKeywords: string | null
  imageUrl: string | null
  squareImageUrl: string | null
  automatic: boolean
  rulesMatchPolicy: 'All' | 'Any'
  sortOrder: string
  hideFromNav: boolean
}

export interface TaxonListItem extends TaxonRequest {
  id: string
  parentName: string | null
  taxonomyName: string | null
  lft: number
  rgt: number
  depth: number
  childrenCount: number
  taxonRuleCount: number
  productCount: number
  permalink: string
  prettyName: string
  createdAtUtc: string
  modifiedAtUtc: string | null
}

export type TaxonDetail = TaxonListItem

export interface TaxonTreeItem extends TaxonListItem {
  children: TaxonTreeItem[]
}

export interface TaxonQuery {
  taxonomyId?: string
  name?: string
  search?: string
  sortBy?: 'name' | 'slug' | 'position' | 'depth' | 'createdAtUtc' | 'modifiedAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const TAXON_FILTER_FIELDS = [
  'taxonomyId',
  'name',
  'slug',
  'depth',
  'createdAtUtc',
  'modifiedAtUtc',
]

export const TAXON_SORT_FIELDS = [
  'name',
  'slug',
  'position',
  'depth',
  'createdAtUtc',
  'modifiedAtUtc',
]

export const TAXON_SORT_ORDERS = [
  'Manual',
  'BestSelling',
  'AlphabeticallyAZ',
  'AlphabeticallyZA',
  'PriceHigh2Low',
  'PriceLow2High',
  'Newest',
  'Oldest',
]

export const TAXON_MATCH_POLICIES = ['All', 'Any']

export function toTaxonQueryParams(query: TaxonQuery): QueryingParameters {
  const filters: string[] = []

  if (query.taxonomyId !== undefined && query.taxonomyId !== '') {
    filters.push(`taxonomyId=${query.taxonomyId}`)
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

- [ ] **Step 3: Write `types/taxonRule.ts`**

```ts
import type { QueryingParameters } from '@/shared/types/querying'

export interface TaxonRuleRequest {
  type: string
  matchPolicy: string
  value: string
}

export interface TaxonRuleListItem extends TaxonRuleRequest {
  id: string
  taxonId: string
}

export type TaxonRuleDetail = TaxonRuleListItem

export interface TaxonRuleQuery {
  taxonId?: string
}

export const TAXON_RULE_TYPES = [
  'product_name',
  'product_sku',
  'product_description',
  'product_price',
  'product_weight',
  'product_available',
  'product_archived',
  'variant_price',
  'variant_sku',
  'product_status',
]

export const TAXON_RULE_MATCH_POLICIES = [
  'is_equal_to',
  'is_not_equal_to',
  'contains',
  'does_not_contain',
  'starts_with',
  'ends_with',
  'greater_than',
  'less_than',
  'greater_than_or_equal',
  'less_than_or_equal',
  'in',
  'not_in',
  'is_null',
  'is_not_null',
]

export function toTaxonRuleQueryParams(query: TaxonRuleQuery): QueryingParameters {
  const filters: string[] = []

  if (query.taxonId !== undefined && query.taxonId !== '') {
    filters.push(`taxonId=${query.taxonId}`)
  }

  return {
    filter: filters.length > 0 ? filters.join(',') : null,
    search: null,
    sort: null,
    pageNumber: null,
    pageSize: null,
  }
}
```

- [ ] **Step 4: Modify `types/index.ts`** (read current, append new exports)

Current file content:
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

Append below the OptionValue block:
```ts
export type {
  TaxonomyRequest,
  TaxonomyListItem,
  TaxonomyDetail,
  TaxonomyQuery,
} from './taxonomy'
export {
  TAXONOMY_FILTER_FIELDS,
  TAXONOMY_SORT_FIELDS,
  toTaxonomyQueryParams,
} from './taxonomy'
export type {
  TaxonRequest,
  TaxonListItem,
  TaxonDetail,
  TaxonTreeItem,
  TaxonQuery,
} from './taxon'
export {
  TAXON_FILTER_FIELDS,
  TAXON_SORT_FIELDS,
  TAXON_SORT_ORDERS,
  TAXON_MATCH_POLICIES,
  toTaxonQueryParams,
} from './taxon'
export type {
  TaxonRuleRequest,
  TaxonRuleListItem,
  TaxonRuleDetail,
  TaxonRuleQuery,
} from './taxonRule'
export {
  TAXON_RULE_TYPES,
  TAXON_RULE_MATCH_POLICIES,
  toTaxonRuleQueryParams,
} from './taxonRule'
```

- [ ] **Step 5: Write `__tests__/types/taxonomy.spec.ts`**

```ts
import { describe, it, expect } from 'vitest'
import { toTaxonomyQueryParams, TAXONOMY_FILTER_FIELDS, TAXONOMY_SORT_FIELDS } from '../../types/taxonomy'

describe('toTaxonomyQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toTaxonomyQueryParams({})
    expect(result.filter).toBeNull()
    expect(result.search).toBeNull()
    expect(result.sort).toBeNull()
  })

  it('builds filter DSL for name (contains operator)', () => {
    const result = toTaxonomyQueryParams({ name: 'Categories' })
    expect(result.filter).toBe('name*=Categories')
  })

  it('builds sort ascending', () => {
    const result = toTaxonomyQueryParams({ sortBy: 'name', sortDirection: 'asc' })
    expect(result.sort).toEqual(['name'])
  })

  it('builds sort descending', () => {
    const result = toTaxonomyQueryParams({ sortBy: 'createdAtUtc', sortDirection: 'desc' })
    expect(result.sort).toEqual(['-createdAtUtc'])
  })

  it('skips empty string values in filters', () => {
    const result = toTaxonomyQueryParams({ name: '' })
    expect(result.filter).toBeNull()
  })

  it('passes search and pagination', () => {
    const result = toTaxonomyQueryParams({ search: 'test', page: 2, pageSize: 10 })
    expect(result.search).toBe('test')
    expect(result.pageNumber).toBe(2)
    expect(result.pageSize).toBe(10)
  })
})

describe('TAXONOMY_FILTER_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(TAXONOMY_FILTER_FIELDS).toEqual([
      'name',
      'taxonsCount',
      'createdAtUtc',
      'modifiedAtUtc',
    ])
  })
})

describe('TAXONOMY_SORT_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(TAXONOMY_SORT_FIELDS).toEqual([
      'name',
      'presentation',
      'position',
      'taxonsCount',
      'createdAtUtc',
      'modifiedAtUtc',
    ])
  })
})
```

- [ ] **Step 6: Write `__tests__/types/taxon.spec.ts`**

```ts
import { describe, it, expect } from 'vitest'
import { toTaxonQueryParams, TAXON_FILTER_FIELDS, TAXON_SORT_FIELDS, TAXON_SORT_ORDERS, TAXON_MATCH_POLICIES } from '../../types/taxon'

describe('toTaxonQueryParams', () => {
  it('returns null filter/search/sort when query is empty', () => {
    const result = toTaxonQueryParams({})
    expect(result.filter).toBeNull()
  })

  it('builds filter for taxonomyId', () => {
    const result = toTaxonQueryParams({ taxonomyId: 'abc-123' })
    expect(result.filter).toBe('taxonomyId=abc-123')
  })

  it('builds filter for name contains', () => {
    const result = toTaxonQueryParams({ name: 'Shoes' })
    expect(result.filter).toBe('name*=Shoes')
  })

  it('combines taxonomyId and name filters', () => {
    const result = toTaxonQueryParams({ taxonomyId: 'abc-123', name: 'Shoes' })
    expect(result.filter).toBe('taxonomyId=abc-123,name*=Shoes')
  })

  it('builds sort ascending', () => {
    const result = toTaxonQueryParams({ sortBy: 'name', sortDirection: 'asc' })
    expect(result.sort).toEqual(['name'])
  })
})

describe('TAXON_FILTER_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(TAXON_FILTER_FIELDS).toEqual([
      'taxonomyId',
      'name',
      'slug',
      'depth',
      'createdAtUtc',
      'modifiedAtUtc',
    ])
  })
})

describe('TAXON_SORT_FIELDS', () => {
  it('contains all expected fields', () => {
    expect(TAXON_SORT_FIELDS).toEqual([
      'name',
      'slug',
      'position',
      'depth',
      'createdAtUtc',
      'modifiedAtUtc',
    ])
  })
})

describe('TAXON_SORT_ORDERS', () => {
  it('contains all 8 sort orders', () => {
    expect(TAXON_SORT_ORDERS).toHaveLength(8)
    expect(TAXON_SORT_ORDERS[0]).toBe('Manual')
  })
})

describe('TAXON_MATCH_POLICIES', () => {
  it('contains All and Any', () => {
    expect(TAXON_MATCH_POLICIES).toEqual(['All', 'Any'])
  })
})
```

- [ ] **Step 7: Write `__tests__/types/taxonRule.spec.ts`**

```ts
import { describe, it, expect } from 'vitest'
import { toTaxonRuleQueryParams, TAXON_RULE_TYPES, TAXON_RULE_MATCH_POLICIES } from '../../types/taxonRule'

describe('toTaxonRuleQueryParams', () => {
  it('returns null filter when query is empty', () => {
    const result = toTaxonRuleQueryParams({})
    expect(result.filter).toBeNull()
  })

  it('builds filter for taxonId', () => {
    const result = toTaxonRuleQueryParams({ taxonId: 'abc-123' })
    expect(result.filter).toBe('taxonId=abc-123')
  })
})

describe('TAXON_RULE_TYPES', () => {
  it('contains all 10 rule types', () => {
    expect(TAXON_RULE_TYPES).toHaveLength(10)
    expect(TAXON_RULE_TYPES[0]).toBe('product_name')
    expect(TAXON_RULE_TYPES).toContain('variant_sku')
  })
})

describe('TAXON_RULE_MATCH_POLICIES', () => {
  it('contains all 14 match policies', () => {
    expect(TAXON_RULE_MATCH_POLICIES).toHaveLength(14)
    expect(TAXON_RULE_MATCH_POLICIES[0]).toBe('is_equal_to')
    expect(TAXON_RULE_MATCH_POLICIES).toContain('is_not_null')
  })
})
```

- [ ] **Step 8: Run tests and commit**
```bash
cd app/Admin && pnpm run test:unit -- run 2>&1 | grep 'Test Files\|Tests'
```
Expected: All tests pass (~490+ total).

```bash
git add app/Admin/src/features/catalog/types/
git add app/Admin/src/features/catalog/__tests__/types/
git commit -m "feat(catalog): add taxonomy, taxon and taxon rule type definitions"
```

---

### Task 2: Taxonomy/Taxon/TaxonRule Validations Layer + Tests

**Files:**
- Create: `app/Admin/src/features/catalog/validations/taxonomy.ts`
- Create: `app/Admin/src/features/catalog/validations/taxon.ts`
- Create: `app/Admin/src/features/catalog/validations/taxonRule.ts`
- Modify: `app/Admin/src/features/catalog/validations/index.ts`
- Create: `app/Admin/src/features/catalog/__tests__/validations/taxonomy.spec.ts`
- Create: `app/Admin/src/features/catalog/__tests__/validations/taxon.spec.ts`
- Create: `app/Admin/src/features/catalog/__tests__/validations/taxonRule.spec.ts`

**Interfaces:**
- Consumes: `z` from `zod`
- Produces: All Zod field validators, schemas, and inferred form types for Tasks 5, 8, 9

- [ ] **Step 1: Write `validations/taxonomy.ts`**

```ts
import { z } from 'zod'

export const taxonomyName = z.string()
  .min(1, 'Taxonomy name is required.')
  .max(100, 'Taxonomy name must not exceed 100 characters.')

export const taxonomyPresentation = z.string()
  .min(1, 'Presentation is required.')
  .max(100, 'Presentation must not exceed 100 characters.')

export const taxonomyPosition = z.number()
  .int()
  .min(-1, 'Position must be at least -1.')

export const taxonomySchema = z.object({
  name: taxonomyName,
  presentation: taxonomyPresentation,
  position: taxonomyPosition,
})

export type TaxonomyForm = z.infer<typeof taxonomySchema>
```

- [ ] **Step 2: Write `validations/taxon.ts`**

```ts
import { z } from 'zod'

export const taxonTaxonomyId = z.string()
  .min(1, 'Taxonomy is required.')

export const taxonParentId = z.string()
  .nullable()
  .optional()

export const taxonName = z.string()
  .min(1, 'Taxon name is required.')
  .max(255, 'Taxon name must not exceed 255 characters.')

export const taxonPresentation = z.string()
  .min(1, 'Presentation is required.')
  .max(255, 'Presentation must not exceed 255 characters.')

export const taxonSlug = z.string()
  .min(1, 'Slug is required.')
  .max(255, 'Slug must not exceed 255 characters.')
  .regex(/^[a-z0-9]+(?:-[a-z0-9]+)*$/, 'Slug must be lowercase alphanumeric with hyphens.')

export const taxonDescription = z.string()
  .max(2000, 'Description must not exceed 2000 characters.')
  .nullable()
  .optional()

export const taxonPosition = z.number()
  .int()
  .min(-1, 'Position must be at least -1.')

export const taxonMetaTitle = z.string()
  .max(100, 'Meta title must not exceed 100 characters.')
  .nullable()
  .optional()

export const taxonMetaDescription = z.string()
  .max(255, 'Meta description must not exceed 255 characters.')
  .nullable()
  .optional()

export const taxonMetaKeywords = z.string()
  .max(255, 'Meta keywords must not exceed 255 characters.')
  .nullable()
  .optional()

export const taxonImageUrl = z.string()
  .nullable()
  .optional()

export const taxonSquareImageUrl = z.string()
  .nullable()
  .optional()

export const taxonAutomatic = z.boolean()

export const taxonRulesMatchPolicy = z.string()
  .min(1, 'Rules match policy is required.')

export const taxonSortOrder = z.string()
  .min(1, 'Sort order is required.')

export const taxonHideFromNav = z.boolean()

export const taxonSchema = z.object({
  taxonomyId: taxonTaxonomyId,
  parentId: taxonParentId,
  name: taxonName,
  presentation: taxonPresentation,
  slug: taxonSlug,
  description: taxonDescription,
  position: taxonPosition,
  metaTitle: taxonMetaTitle,
  metaDescription: taxonMetaDescription,
  metaKeywords: taxonMetaKeywords,
  imageUrl: taxonImageUrl,
  squareImageUrl: taxonSquareImageUrl,
  automatic: taxonAutomatic,
  rulesMatchPolicy: taxonRulesMatchPolicy,
  sortOrder: taxonSortOrder,
  hideFromNav: taxonHideFromNav,
})

export type TaxonForm = z.infer<typeof taxonSchema>
```

- [ ] **Step 3: Write `validations/taxonRule.ts`**

```ts
import { z } from 'zod'

export const taxonRuleType = z.string()
  .min(1, 'Rule type is required.')

export const taxonRuleMatchPolicy = z.string()
  .min(1, 'Match policy is required.')

export const taxonRuleValue = z.string()
  .min(1, 'Value is required.')
  .max(255, 'Value must not exceed 255 characters.')

export const taxonRuleSchema = z.object({
  type: taxonRuleType,
  matchPolicy: taxonRuleMatchPolicy,
  value: taxonRuleValue,
})

export type TaxonRuleForm = z.infer<typeof taxonRuleSchema>
```

- [ ] **Step 4: Modify `validations/index.ts`** — read current, append:

```ts
export {
  taxonomyName,
  taxonomyPresentation,
  taxonomyPosition,
  taxonomySchema,
} from './taxonomy'
export type { TaxonomyForm } from './taxonomy'
export {
  taxonTaxonomyId,
  taxonName,
  taxonPresentation,
  taxonSlug,
  taxonPosition,
  taxonSchema,
} from './taxon'
export type { TaxonForm } from './taxon'
export {
  taxonRuleType,
  taxonRuleMatchPolicy,
  taxonRuleValue,
  taxonRuleSchema,
} from './taxonRule'
export type { TaxonRuleForm } from './taxonRule'
```

- [ ] **Step 5: Write `__tests__/validations/taxonomy.spec.ts`**

```ts
import { describe, it, expect } from 'vitest'
import {
  taxonomyName,
  taxonomyPresentation,
  taxonomyPosition,
  taxonomySchema,
} from '../../validations/taxonomy'

describe('taxonomyName', () => {
  it('accepts a valid name', () => {
    expect(taxonomyName.safeParse('Categories').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(taxonomyName.safeParse('').success).toBe(false)
  })

  it('rejects string over 100 characters', () => {
    expect(taxonomyName.safeParse('A'.repeat(101)).success).toBe(false)
  })
})

describe('taxonomyPresentation', () => {
  it('accepts a valid presentation', () => {
    expect(taxonomyPresentation.safeParse('Categories').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(taxonomyPresentation.safeParse('').success).toBe(false)
  })
})

describe('taxonomyPosition', () => {
  it('accepts position 0', () => {
    expect(taxonomyPosition.safeParse(0).success).toBe(true)
  })

  it('accepts position -1', () => {
    expect(taxonomyPosition.safeParse(-1).success).toBe(true)
  })

  it('rejects position -2', () => {
    expect(taxonomyPosition.safeParse(-2).success).toBe(false)
  })
})

describe('taxonomySchema', () => {
  it('accepts valid form', () => {
    const result = taxonomySchema.safeParse({
      name: 'Categories',
      presentation: 'Categories',
      position: 1,
    })
    expect(result.success).toBe(true)
  })

  it('rejects empty name', () => {
    const result = taxonomySchema.safeParse({
      name: '',
      presentation: 'Categories',
      position: 1,
    })
    expect(result.success).toBe(false)
  })
})
```

- [ ] **Step 6: Write `__tests__/validations/taxon.spec.ts`**

```ts
import { describe, it, expect } from 'vitest'
import {
  taxonName,
  taxonSlug,
  taxonPosition,
  taxonSchema,
} from '../../validations/taxon'

describe('taxonName', () => {
  it('accepts a valid name', () => {
    expect(taxonName.safeParse('Shoes').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(taxonName.safeParse('').success).toBe(false)
  })

  it('rejects string over 255 characters', () => {
    expect(taxonName.safeParse('A'.repeat(256)).success).toBe(false)
  })
})

describe('taxonSlug', () => {
  it('accepts valid slug', () => {
    expect(taxonSlug.safeParse('running-shoes').success).toBe(true)
  })

  it('rejects uppercase', () => {
    expect(taxonSlug.safeParse('Running-Shoes').success).toBe(false)
  })

  it('rejects spaces', () => {
    expect(taxonSlug.safeParse('running shoes').success).toBe(false)
  })

  it('rejects empty', () => {
    expect(taxonSlug.safeParse('').success).toBe(false)
  })
})

describe('taxonPosition', () => {
  it('accepts position 0', () => {
    expect(taxonPosition.safeParse(0).success).toBe(true)
  })

  it('rejects position -2', () => {
    expect(taxonPosition.safeParse(-2).success).toBe(false)
  })
})

describe('taxonSchema', () => {
  const validTaxon = {
    taxonomyId: 'abc-123',
    parentId: null,
    name: 'Shoes',
    presentation: 'Shoes',
    slug: 'shoes',
    description: null,
    position: 0,
    metaTitle: null,
    metaDescription: null,
    metaKeywords: null,
    imageUrl: null,
    squareImageUrl: null,
    automatic: false,
    rulesMatchPolicy: 'All',
    sortOrder: 'Manual',
    hideFromNav: false,
  }

  it('accepts valid form', () => {
    const result = taxonSchema.safeParse(validTaxon)
    expect(result.success).toBe(true)
  })

  it('rejects empty name', () => {
    const result = taxonSchema.safeParse({ ...validTaxon, name: '' })
    expect(result.success).toBe(false)
  })

  it('accepts parentId as null', () => {
    const result = taxonSchema.safeParse(validTaxon)
    expect(result.success).toBe(true)
  })
})
```

- [ ] **Step 7: Write `__tests__/validations/taxonRule.spec.ts`**

```ts
import { describe, it, expect } from 'vitest'
import {
  taxonRuleType,
  taxonRuleValue,
  taxonRuleSchema,
} from '../../validations/taxonRule'

describe('taxonRuleType', () => {
  it('accepts valid type', () => {
    expect(taxonRuleType.safeParse('product_name').success).toBe(true)
  })

  it('rejects empty', () => {
    expect(taxonRuleType.safeParse('').success).toBe(false)
  })
})

describe('taxonRuleValue', () => {
  it('accepts valid value', () => {
    expect(taxonRuleValue.safeParse('Nike').success).toBe(true)
  })

  it('rejects empty', () => {
    expect(taxonRuleValue.safeParse('').success).toBe(false)
  })

  it('rejects over 255 characters', () => {
    expect(taxonRuleValue.safeParse('A'.repeat(256)).success).toBe(false)
  })
})

describe('taxonRuleSchema', () => {
  it('accepts valid rule', () => {
    const result = taxonRuleSchema.safeParse({
      type: 'product_name',
      matchPolicy: 'contains',
      value: 'Nike',
    })
    expect(result.success).toBe(true)
  })

  it('rejects empty type', () => {
    const result = taxonRuleSchema.safeParse({
      type: '',
      matchPolicy: 'contains',
      value: 'Nike',
    })
    expect(result.success).toBe(false)
  })
})
```

- [ ] **Step 8: Run tests and commit**
```bash
cd app/Admin && pnpm run test:unit -- run 2>&1 | grep 'Test Files\|Tests'
git add app/Admin/src/features/catalog/validations/
git add app/Admin/src/features/catalog/__tests__/validations/
git commit -m "feat(catalog): add taxonomy, taxon and taxon rule Zod validations"
```

---

### Task 3: Taxonomy/Taxon/TaxonRule Services Layer + Tests

**Files:**
- Create: `app/Admin/src/features/catalog/services/taxonomyApi.ts`
- Create: `app/Admin/src/features/catalog/services/taxonApi.ts`
- Create: `app/Admin/src/features/catalog/services/taxonRuleApi.ts`
- Modify: `app/Admin/src/features/catalog/services/index.ts`
- Create: `app/Admin/src/features/catalog/__tests__/services/taxonomyApi.spec.ts`
- Create: `app/Admin/src/features/catalog/__tests__/services/taxonApi.spec.ts`
- Create: `app/Admin/src/features/catalog/__tests__/services/taxonRuleApi.spec.ts`

**Interfaces:**
- Consumes: `post/get/put/del` from `@/shared/api/client`, `getPaged` from `@/shared/api`, `CATALOG` from `@/shared/constants/api`, types from Task 1
- Produces: `TaxonomyApi`, `TaxonApi`, `TaxonRuleApi` static classes for Tasks 4, 6, 7, 8, 9

- [ ] **Step 1: Write `services/taxonomyApi.ts`**

```ts
import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type {
  TaxonomyRequest,
  TaxonomyListItem,
  TaxonomyDetail,
  TaxonomyQuery,
} from '../types/taxonomy'
import {
  toTaxonomyQueryParams,
  TAXONOMY_FILTER_FIELDS,
  TAXONOMY_SORT_FIELDS,
} from '../types/taxonomy'

export class TaxonomyApi {
  private static readonly BASE = `${CATALOG}/taxonomies`

  static getTaxonomies(query: TaxonomyQuery): Promise<PagedResult<TaxonomyListItem>> {
    return getPaged<TaxonomyListItem>(TaxonomyApi.BASE, toTaxonomyQueryParams(query), {
      allowedFilterFields: TAXONOMY_FILTER_FIELDS,
      allowedSortFields: TAXONOMY_SORT_FIELDS,
    })
  }

  static getTaxonomy(id: string): Promise<Result<TaxonomyDetail>> {
    return get<Result<TaxonomyDetail>>(`${TaxonomyApi.BASE}/${id}`)
  }

  static createTaxonomy(request: TaxonomyRequest): Promise<Result<TaxonomyDetail>> {
    return post<Result<TaxonomyDetail>>(TaxonomyApi.BASE, request)
  }

  static updateTaxonomy(id: string, request: TaxonomyRequest): Promise<Result<TaxonomyDetail>> {
    return put<Result<TaxonomyDetail>>(`${TaxonomyApi.BASE}/${id}`, request)
  }

  static deleteTaxonomy(id: string): Promise<Result<TaxonomyListItem>> {
    return del<Result<TaxonomyListItem>>(`${TaxonomyApi.BASE}/${id}`)
  }
}
```

- [ ] **Step 2: Write `services/taxonApi.ts`**

```ts
import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type {
  TaxonRequest,
  TaxonListItem,
  TaxonDetail,
  TaxonTreeItem,
  TaxonQuery,
} from '../types/taxon'
import {
  toTaxonQueryParams,
  TAXON_FILTER_FIELDS,
  TAXON_SORT_FIELDS,
} from '../types/taxon'

export class TaxonApi {
  private static readonly BASE = `${CATALOG}/taxonomies/taxons`

  static getTaxons(query: TaxonQuery): Promise<PagedResult<TaxonListItem>> {
    return getPaged<TaxonListItem>(TaxonApi.BASE, toTaxonQueryParams(query), {
      allowedFilterFields: TAXON_FILTER_FIELDS,
      allowedSortFields: TAXON_SORT_FIELDS,
    })
  }

  static getTaxon(id: string): Promise<Result<TaxonDetail>> {
    return get<Result<TaxonDetail>>(`${TaxonApi.BASE}/${id}`)
  }

  static getTree(): Promise<Result<{ tree: TaxonTreeItem[] }>> {
    return get<Result<{ tree: TaxonTreeItem[] }>>(`${TaxonApi.BASE}/tree`)
  }

  static createTaxon(request: TaxonRequest): Promise<Result<TaxonDetail>> {
    return post<Result<TaxonDetail>>(TaxonApi.BASE, request)
  }

  static updateTaxon(id: string, request: TaxonRequest): Promise<Result<TaxonDetail>> {
    return put<Result<TaxonDetail>>(`${TaxonApi.BASE}/${id}`, request)
  }

  static deleteTaxon(id: string): Promise<Result<TaxonListItem>> {
    return del<Result<TaxonListItem>>(`${TaxonApi.BASE}/${id}`)
  }
}
```

- [ ] **Step 3: Write `services/taxonRuleApi.ts`**

```ts
import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type {
  TaxonRuleRequest,
  TaxonRuleListItem,
  TaxonRuleDetail,
  TaxonRuleQuery,
} from '../types/taxonRule'
import {
  toTaxonRuleQueryParams,
} from '../types/taxonRule'

export class TaxonRuleApi {
  private static readonly BASE = `${CATALOG}/taxonomies/taxons`

  static getRules(taxonId: string): Promise<PagedResult<TaxonRuleListItem>> {
    const query: TaxonRuleQuery = { taxonId }
    return getPaged<TaxonRuleListItem>(`${TaxonRuleApi.BASE}/${taxonId}/rules`, toTaxonRuleQueryParams(query))
  }

  static createRule(taxonId: string, request: TaxonRuleRequest): Promise<Result<TaxonRuleDetail>> {
    return post<Result<TaxonRuleDetail>>(`${TaxonRuleApi.BASE}/${taxonId}/rules`, request)
  }

  static updateRule(taxonId: string, ruleId: string, request: TaxonRuleRequest): Promise<Result<TaxonRuleDetail>> {
    return put<Result<TaxonRuleDetail>>(`${TaxonRuleApi.BASE}/${taxonId}/rules/${ruleId}`, request)
  }

  static deleteRule(taxonId: string, ruleId: string): Promise<Result<TaxonRuleListItem>> {
    return del<Result<TaxonRuleListItem>>(`${TaxonRuleApi.BASE}/${taxonId}/rules/${ruleId}`)
  }
}
```

- [ ] **Step 4: Modify `services/index.ts`** — read current, append:

```ts
export { TaxonomyApi } from './taxonomyApi'
export { TaxonApi } from './taxonApi'
export { TaxonRuleApi } from './taxonRuleApi'
```

- [ ] **Step 5: Write `__tests__/services/taxonomyApi.spec.ts`**

Same pattern as optionTypeApi.spec.ts — mock post/get/put/del/getPaged, test all 5 methods. Use `'api/catalog/taxonomies'` as BASE. Each method test: one `.toHaveBeenCalledWith` assertion.

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

import { TaxonomyApi } from '../../services/taxonomyApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('TaxonomyApi.getTaxonomies', () => {
  it('calls getPaged with query params', async () => {
    mockGetPaged.mockResolvedValue({
      items: [],
      page: 1, pageSize: 20, totalCount: 0, totalPages: 0,
      isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    })

    await TaxonomyApi.getTaxonomies({ name: 'Categories', page: 1, pageSize: 10 })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/catalog/taxonomies',
      { filter: 'name*=Categories', search: null, sort: null, pageNumber: 1, pageSize: 10 },
      expect.objectContaining({ allowedFilterFields: expect.any(Array) }),
    )
  })
})

describe('TaxonomyApi.getTaxonomy', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: '1', name: 'Categories' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonomyApi.getTaxonomy('abc-123')
    expect(mockGet).toHaveBeenCalledWith('api/catalog/taxonomies/abc-123')
  })
})

describe('TaxonomyApi.createTaxonomy', () => {
  it('calls POST with request body', async () => {
    const req = { name: 'Categories', presentation: 'Categories', position: 1 }
    mockPost.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await TaxonomyApi.createTaxonomy(req)
    expect(mockPost).toHaveBeenCalledWith('api/catalog/taxonomies', req)
  })
})

describe('TaxonomyApi.updateTaxonomy', () => {
  it('calls PUT with request body', async () => {
    const req = { name: 'Categories', presentation: 'Categories', position: 2 }
    mockPut.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonomyApi.updateTaxonomy('abc-123', req)
    expect(mockPut).toHaveBeenCalledWith('api/catalog/taxonomies/abc-123', req)
  })
})

describe('TaxonomyApi.deleteTaxonomy', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: { id: '1', name: 'Categories' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonomyApi.deleteTaxonomy('abc-123')
    expect(mockDel).toHaveBeenCalledWith('api/catalog/taxonomies/abc-123')
  })
})
```

- [ ] **Step 6: Write `__tests__/services/taxonApi.spec.ts`**

Same pattern, test 6 methods: getTaxons (getPaged), getTaxon (GET), getTree (GET), createTaxon (POST), updateTaxon (PUT), deleteTaxon (DELETE). Use `'api/catalog/taxonomies/taxons'` as BASE.

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

import { TaxonApi } from '../../services/taxonApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('TaxonApi.getTaxons', () => {
  it('calls getPaged with query params', async () => {
    mockGetPaged.mockResolvedValue({
      items: [],
      page: 1, pageSize: 20, totalCount: 0, totalPages: 0,
      isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    })

    await TaxonApi.getTaxons({ taxonomyId: 'abc-123' })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/catalog/taxonomies/taxons',
      expect.objectContaining({ filter: 'taxonomyId=abc-123' }),
      expect.any(Object),
    )
  })
})

describe('TaxonApi.getTaxon', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: '1', name: 'Shoes' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonApi.getTaxon('abc-123')
    expect(mockGet).toHaveBeenCalledWith('api/catalog/taxonomies/taxons/abc-123')
  })
})

describe('TaxonApi.getTree', () => {
  it('calls GET with tree URL', async () => {
    mockGet.mockResolvedValue({ value: { tree: [] }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonApi.getTree()
    expect(mockGet).toHaveBeenCalledWith('api/catalog/taxonomies/taxons/tree')
  })
})

describe('TaxonApi.createTaxon', () => {
  it('calls POST with request body', async () => {
    const req = {
      taxonomyId: 'tax-1', parentId: null, name: 'Shoes', presentation: 'Shoes', description: null,
      slug: 'shoes', position: 0, metaTitle: null, metaDescription: null, metaKeywords: null,
      imageUrl: null, squareImageUrl: null, automatic: false, rulesMatchPolicy: 'All',
      sortOrder: 'Manual', hideFromNav: false,
    } as any
    mockPost.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await TaxonApi.createTaxon(req)
    expect(mockPost).toHaveBeenCalledWith('api/catalog/taxonomies/taxons', req)
  })
})

describe('TaxonApi.updateTaxon', () => {
  it('calls PUT with request body', async () => {
    const req = {
      taxonomyId: 'tax-1', parentId: null, name: 'Shoes', presentation: 'Shoes', description: null,
      slug: 'shoes', position: 1, metaTitle: null, metaDescription: null, metaKeywords: null,
      imageUrl: null, squareImageUrl: null, automatic: false, rulesMatchPolicy: 'Any',
      sortOrder: 'BestSelling', hideFromNav: true,
    } as any
    mockPut.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonApi.updateTaxon('abc-123', req)
    expect(mockPut).toHaveBeenCalledWith('api/catalog/taxonomies/taxons/abc-123', req)
  })
})

describe('TaxonApi.deleteTaxon', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: { id: '1', name: 'Shoes' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonApi.deleteTaxon('abc-123')
    expect(mockDel).toHaveBeenCalledWith('api/catalog/taxonomies/taxons/abc-123')
  })
})
```

- [ ] **Step 7: Write `__tests__/services/taxonRuleApi.spec.ts`**

Same pattern, test 4 methods. Use URL `${BASE}/${taxonId}/rules`.

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

import { TaxonRuleApi } from '../../services/taxonRuleApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('TaxonRuleApi.getRules', () => {
  it('calls getPaged with taxonId in URL', async () => {
    mockGetPaged.mockResolvedValue({
      items: [],
      page: 1, pageSize: 9999, totalCount: 0, totalPages: 0,
      isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null,
    })

    await TaxonRuleApi.getRules('taxon-123')

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/catalog/taxonomies/taxons/taxon-123/rules',
      expect.objectContaining({ filter: 'taxonId=taxon-123' }),
    )
  })
})

describe('TaxonRuleApi.createRule', () => {
  it('calls POST with correct URL and body', async () => {
    const req = { type: 'product_name', matchPolicy: 'contains', value: 'Nike' }
    mockPost.mockResolvedValue({ value: { id: '1', taxonId: 'taxon-123', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await TaxonRuleApi.createRule('taxon-123', req)
    expect(mockPost).toHaveBeenCalledWith('api/catalog/taxonomies/taxons/taxon-123/rules', req)
  })
})

describe('TaxonRuleApi.updateRule', () => {
  it('calls PUT with correct URL and body', async () => {
    const req = { type: 'product_name', matchPolicy: 'is_equal_to', value: 'Adidas' }
    mockPut.mockResolvedValue({ value: { id: 'rule-456', taxonId: 'taxon-123', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonRuleApi.updateRule('taxon-123', 'rule-456', req)
    expect(mockPut).toHaveBeenCalledWith('api/catalog/taxonomies/taxons/taxon-123/rules/rule-456', req)
  })
})

describe('TaxonRuleApi.deleteRule', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: { id: 'rule-456', taxonId: 'taxon-123' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await TaxonRuleApi.deleteRule('taxon-123', 'rule-456')
    expect(mockDel).toHaveBeenCalledWith('api/catalog/taxonomies/taxons/taxon-123/rules/rule-456')
  })
})
```

- [ ] **Step 8: Run tests and commit**
```bash
cd app/Admin && pnpm run test:unit -- run 2>&1 | grep 'Test Files\|Tests'
git add app/Admin/src/features/catalog/services/
git add app/Admin/src/features/catalog/__tests__/services/
git commit -m "feat(catalog): add taxonomy, taxon and taxon rule API services"
```

---

### Task 4: Taxonomy Store Layer

**Files:**
- Create: `app/Admin/src/features/catalog/stores/taxonomyStore.ts`
- Modify: `app/Admin/src/features/catalog/stores/index.ts`

**Interfaces:**
- Consumes: `defineStore` from `pinia`, `ref` from `vue`, `TaxonomyListItem` from Task 1, `TaxonomyApi` from Task 3
- Produces: `useTaxonomyStore` — Pinia store with lazy-once pattern for dropdown reuse

- [ ] **Step 1: Write `stores/taxonomyStore.ts`**

```ts
import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { TaxonomyListItem } from '../types/taxonomy'
import { TaxonomyApi } from '../services/taxonomyApi'

export const useTaxonomyStore = defineStore('taxonomies', () => {
  const activeTaxonomies = ref<TaxonomyListItem[]>([])
  const loaded = ref(false)

  async function fetchActive(): Promise<void> {
    if (loaded.value) return

    const result = await TaxonomyApi.getTaxonomies({
      sortBy: 'name',
      sortDirection: 'asc',
    })

    if (result.isSuccess) {
      activeTaxonomies.value = result.items
      loaded.value = true
    }
  }

  return { activeTaxonomies, loaded, fetchActive }
})
```

(Note: no `pageSize` parameter — backend returns all items when pageSize is omitted.)

- [ ] **Step 2: Modify `stores/index.ts`** — append:

```ts
export { useTaxonomyStore } from './taxonomyStore'
```

- [ ] **Step 3: Run build and commit**
```bash
cd app/Admin && pnpm run build-only 2>&1 | tail -1
git add app/Admin/src/features/catalog/stores/
git commit -m "feat(catalog): add taxonomy store for dropdown caching"
```

---

### Task 5: TaxonRuleFormDialog Component

**Files:**
- Create: `app/Admin/src/features/catalog/components/TaxonRuleFormDialog.vue`
- Modify: `app/Admin/src/features/catalog/components/index.ts`

**Interfaces:**
- Consumes: `Dialog`, `useNotify`, `useApiErrorHandler`, `TaxonRuleApi` (Task 3), `taxonRuleSchema` (Task 2), `TaxonRuleForm` (Task 2), `TaxonRuleListItem` (Task 1), `TAXON_RULE_TYPES`, `TAXON_RULE_MATCH_POLICIES` (Task 1)
- Produces: `TaxonRuleFormDialog` with props `visible`, `taxonId`, `editingRule`; emits `update:visible`, `saved`

- [ ] **Step 1: Write `components/TaxonRuleFormDialog.vue`** — pattern identical to OptionValueFormDialog, 3 fields: Type (Select from TAXON_RULE_TYPES), MatchPolicy (Select from TAXON_RULE_MATCH_POLICIES), Value (InputText)

```vue
<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import Dialog from 'primevue/dialog'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { TaxonRuleApi } from '../services/taxonRuleApi'
import { taxonRuleSchema } from '../validations/taxonRule'
import type { TaxonRuleForm } from '../validations/taxonRule'
import type { TaxonRuleListItem } from '../types/taxonRule'
import { TAXON_RULE_TYPES, TAXON_RULE_MATCH_POLICIES } from '../types/taxonRule'

interface Props {
  visible: boolean
  taxonId: string
  editingRule: TaxonRuleListItem | null
}

const props = defineProps<Props>()

const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void
  (e: 'saved'): void
}>()

const notify = useNotify()
const { handleResult } = useApiErrorHandler()

const isEdit = computed(() => !!props.editingRule)
const dialogTitle = computed(() => isEdit.value ? 'Edit Rule' : 'Add Rule')

const form = ref<TaxonRuleForm>({
  type: 'product_name',
  matchPolicy: 'contains',
  value: '',
})

const fieldErrors = ref<Record<string, string>>({})
const saving = ref(false)

watch(
  () => props.visible,
  (v) => {
    if (v) {
      fieldErrors.value = {}
      if (props.editingRule) {
        form.value = {
          type: props.editingRule.type,
          matchPolicy: props.editingRule.matchPolicy,
          value: props.editingRule.value,
        }
      } else {
        form.value = {
          type: 'product_name',
          matchPolicy: 'contains',
          value: '',
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
  const parsed = taxonRuleSchema.safeParse(form.value)

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
    type: data.type,
    matchPolicy: data.matchPolicy,
    value: data.value,
  }

  const result = isEdit.value
    ? await TaxonRuleApi.updateRule(props.taxonId, props.editingRule!.id, request)
    : await TaxonRuleApi.createRule(props.taxonId, request)

  saving.value = false

  if (result.isSuccess) {
    notify.success(isEdit.value ? 'Rule updated' : 'Rule created')
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
    :style="{ width: '500px' }"
    @update:visible="close"
  >
    <div class="flex flex-col gap-4">
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium">Type</label>
        <Select v-model="form.type" :options="TAXON_RULE_TYPES" option-label="type" fluid :invalid="!!fieldErrors.type" />
        <small v-if="fieldErrors.type" class="text-red-500">{{ fieldErrors.type }}</small>
      </div>
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium">Match Policy</label>
        <Select v-model="form.matchPolicy" :options="TAXON_RULE_MATCH_POLICIES" option-label="matchPolicy" fluid :invalid="!!fieldErrors.matchPolicy" />
        <small v-if="fieldErrors.matchPolicy" class="text-red-500">{{ fieldErrors.matchPolicy }}</small>
      </div>
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium">Value</label>
        <InputText v-model="form.value" fluid :invalid="!!fieldErrors.value" />
        <small v-if="fieldErrors.value" class="text-red-500">{{ fieldErrors.value }}</small>
      </div>
    </div>
    <template #footer>
      <Button label="Cancel" severity="secondary" @click="close" />
      <Button label="Save" severity="primary" :loading="saving" @click="onSave" />
    </template>
  </Dialog>
</template>
```

- [ ] **Step 2: Modify `components/index.ts`** — append:

```ts
export { default as TaxonRuleFormDialog } from './TaxonRuleFormDialog.vue'
```

- [ ] **Step 3: Run build and commit**
```bash
cd app/Admin && pnpm run build-only 2>&1 | tail -1
git add app/Admin/src/features/catalog/components/
git commit -m "feat(catalog): add TaxonRule form dialog component"
```
### Task 6: TaxonomiesList View

**Files:**
- Modify: `app/Admin/src/features/catalog/views/TaxonomiesList.vue` (replace stub)

**Interfaces:**
- Consumes: `useRouter`, `useConfirm`, `useNotify`, `useDataTableExport`, `usePagedQuery`, `PageShell`, `TaxonomyApi` (Task 3), `TaxonomyListItem` (Task 1), `TAXONOMY_FILTER_FIELDS`, `TAXONOMY_SORT_FIELDS` (Task 1)
- Produces: View at route `/catalog/taxonomies`

- [ ] **Step 1: Replace `views/TaxonomiesList.vue`** — same pattern as OptionTypesList

```vue
<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Toolbar from 'primevue/toolbar'
import Plus from '@primeicons/vue/plus'
import Trash from '@primeicons/vue/trash'
import Upload from '@primeicons/vue/upload'
import { PageShell } from '@panel'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useNotify } from '@/shared/composables/useNotify'
import { TaxonomyApi } from '../services/taxonomyApi'
import type { TaxonomyListItem } from '../types/taxonomy'
import { TAXONOMY_FILTER_FIELDS, TAXONOMY_SORT_FIELDS } from '../types/taxonomy'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()

const { dt, exportCSV } = useDataTableExport()
const selectedItems = ref<TaxonomyListItem[]>([])
const searchTerm = ref('')
const allowedSearchFields = ['name', 'presentation']

const {
  items,
  loading,
  setSearch,
  refresh,
} = usePagedQuery<TaxonomyListItem>('api/catalog/taxonomies', {
  allowedFilterFields: TAXONOMY_FILTER_FIELDS,
  allowedSortFields: TAXONOMY_SORT_FIELDS,
  allowedSearchFields,
  defaultSearchFields: allowedSearchFields,
  defaultSearchMode: 'any',
  defaultSort: ['name'],
  defaultPageSize: 20,
})

function navigateToNew() {
  router.push('/catalog/taxonomies/new')
}

function navigateToEdit(id: string) {
  router.push(`/catalog/taxonomies/${id}`)
}

function navigateToTaxons(id: string) {
  router.push(`/catalog/taxons?taxonomyId=${id}`)
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
    message: `Are you sure you want to delete ${selectedItems.value.length > 1 ? 'these taxonomies' : 'this taxonomy'}?`,
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
        const result = await TaxonomyApi.deleteTaxonomy(id)
        if (!result.isSuccess) failed++
      }
      selectedItems.value = []
      refresh()
      if (failed === 0) {
        notify.success(
          ids.length > 1 ? 'Taxonomies deleted' : 'Taxonomy deleted',
          ids.length > 1
            ? `${ids.length} taxonomies have been removed.`
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
  <PageShell title="Taxonomies" description="Manage product classification taxonomies">
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
      :rows="20"
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
              placeholder="Search taxonomies..."
              @update:model-value="onSearch($event ?? '')"
            />
          </IconField>
          <Button label="Clear" outlined @click="clearSearch" />
        </div>
      </template>
      <Column field="name" header="Name" :sortable="true" :filter="true" filter-field="name" />
      <Column field="presentation" header="Presentation" :sortable="true" />
      <Column field="position" header="Position" :sortable="true" />
      <Column field="taxonsCount" header="Taxons" :sortable="true" />
      <Column header="" body-style="text-align: right; width: 10rem">
        <template #body="{ data }">
          <div class="flex justify-end gap-2">
            <Button icon="pi pi-sitemap" severity="secondary" text rounded aria-label="Taxons" @click="navigateToTaxons(data.id)" />
            <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEdit(data.id)" />
            <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="selectedItems = [data]; confirmDelete()" />
          </div>
        </template>
      </Column>
      <template #empty>
        <div class="text-center py-8 text-muted-color">No taxonomies found.</div>
      </template>
    </DataTable>
  </PageShell>
</template>
```

- [ ] **Step 2: Run build and commit**
```bash
cd app/Admin && pnpm run build-only 2>&1 | tail -1
git add app/Admin/src/features/catalog/views/TaxonomiesList.vue
git commit -m "feat(catalog): implement Taxonomies list view"
```

---

### Task 7: TaxonomyDetail View (form + inline TreeTable)

**Files:**
- Modify: `app/Admin/src/features/catalog/views/TaxonomyDetail.vue` (replace stub)

**Interfaces:**
- Consumes: `useRoute`, `useRouter`, `useConfirm`, `useNotify`, `useApiErrorHandler`, `TreeTable`, `Column`, `Toolbar`, `Card`, `PageShell`, `PageHeading`, `FormSection`, `FormField`, `TaxonomyApi` (Task 3), `TaxonApi` (Task 3), `taxonomySchema` (Task 2), `TaxonomyForm` (Task 2), `TaxonTreeItem` (Task 1)
- Produces: View at route `/catalog/taxonomies/:id`

- [ ] **Step 1: Replace `views/TaxonomyDetail.vue`** — form with inline TreeTable of taxons

```vue
<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import TreeTable from 'primevue/treetable'
import Column from 'primevue/column'
import Toolbar from 'primevue/toolbar'
import Card from 'primevue/card'
import Plus from '@primeicons/vue/plus'
import { PageShell, PageHeading } from '@panel'
import { FormSection, FormField } from '@form'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { TaxonomyApi } from '../services/taxonomyApi'
import { TaxonApi } from '../services/taxonApi'
import { taxonomySchema } from '../validations/taxonomy'
import type { TaxonomyForm } from '../validations/taxonomy'
import type { TaxonTreeItem } from '../types/taxon'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const confirm = useConfirm()
const { handleResult } = useApiErrorHandler()

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => isEdit.value ? 'Edit Taxonomy' : 'New Taxonomy')

const form = ref<TaxonomyForm>({
  name: '',
  presentation: '',
  position: 1,
})

const fieldErrors = ref<Record<string, string>>({})
const saving = ref(false)

const treeNodes = ref<TaxonTreeItem[]>([])
const treeLoading = ref(false)

async function initEditMode(id: string) {
  const result = await TaxonomyApi.getTaxonomy(id)
  if (result.isSuccess) {
    const t = result.value
    form.value = {
      name: t.name,
      presentation: t.presentation,
      position: t.position,
    }
  } else {
    handleResult(result)
    router.push('/catalog/taxonomies')
  }

  await loadTree(id)
}

async function loadTree(taxonomyId: string) {
  treeLoading.value = true
  const result = await TaxonApi.getTree()
  if (result.isSuccess && result.value?.tree) {
    treeNodes.value = result.value.tree
  }
  treeLoading.value = false
}

onMounted(() => {
  if (isEdit.value) {
    initEditMode(route.params.id as string)
  }
})

watch(() => route.params.id, (newId) => {
  if (newId && newId !== 'new') {
    initEditMode(newId as string)
  }
})

async function onSave() {
  fieldErrors.value = {}
  const parsed = taxonomySchema.safeParse(form.value)

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
  }

  const result = isEdit.value
    ? await TaxonomyApi.updateTaxonomy(route.params.id as string, request)
    : await TaxonomyApi.createTaxonomy(request)

  saving.value = false

  if (result.isSuccess) {
    notify.success(isEdit.value ? 'Taxonomy updated' : 'Taxonomy created')
    if (!isEdit.value && result.value) {
      const created = result.value
      form.value = {
        name: created.name,
        presentation: created.presentation,
        position: created.position,
      }
      router.replace(`/catalog/taxonomies/${created.id}`)
    }
  } else {
    handleResult(result)
  }
}

function onCancel() {
  router.push('/catalog/taxonomies')
}

function navigateToCreateTaxon(parentId: string | null = null) {
  const base = `/catalog/taxons/new?taxonomyId=${route.params.id}`
  router.push(parentId ? `${base}&parentId=${parentId}` : base)
}

function navigateToEditTaxon(id: string) {
  router.push(`/catalog/taxons/${id}`)
}

function confirmDeleteTaxon(node: TaxonTreeItem) {
  confirm.require({
    message: `Are you sure you want to delete "${node.name}"?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const result = await TaxonApi.deleteTaxon(node.id)
      if (result.isSuccess) {
        notify.success('Taxon deleted', `${node.name} has been removed.`)
        if (isEdit.value) {
          await loadTree(route.params.id as string)
        }
      } else {
        notify.error('Delete failed', result.errors?.[0]?.message ?? 'Could not delete taxon.')
      }
    },
  })
}
</script>

<template>
  <PageShell :title="pageTitle">
    <PageHeading
      title=""
      :breadcrumbs="[
        { label: 'Home', to: '/' },
        { label: 'Taxonomies', to: '/catalog/taxonomies' },
        { label: pageTitle },
      ]"
      :actions="[
        { label: 'Save', icon: 'pi pi-check', severity: 'primary' },
        { label: 'Cancel', icon: 'pi pi-times' },
      ]"
      @action="(i: number) => i === 0 ? onSave() : onCancel()"
    />

    <FormSection title="Taxonomy Details">
      <FormField label="Name" :required="true" :invalid="!!fieldErrors.name">
        <InputText v-model="form.name" fluid class="w-full" />
        <small v-if="fieldErrors.name" class="text-red-500">{{ fieldErrors.name }}</small>
      </FormField>
      <FormField label="Presentation" :required="true" :invalid="!!fieldErrors.presentation">
        <InputText v-model="form.presentation" fluid class="w-full" />
        <small v-if="fieldErrors.presentation" class="text-red-500">{{ fieldErrors.presentation }}</small>
      </FormField>
      <FormField label="Position" :invalid="!!fieldErrors.position" help-text="Sort order (lower = first)">
        <InputNumber v-model="form.position" fluid :min="-1" class="w-full" />
        <small v-if="fieldErrors.position" class="text-red-500">{{ fieldErrors.position }}</small>
      </FormField>
    </FormSection>

    <Card v-if="isEdit">
      <template #content>
        <Toolbar>
          <template #start>
            <Button label="Add Taxon" severity="secondary" @click="navigateToCreateTaxon()">
              <Plus />
            </Button>
          </template>
        </Toolbar>
      </template>
    </Card>

    <TreeTable
      v-if="isEdit"
      :value="treeNodes"
      :loading="treeLoading"
      class="mt-0"
    >
      <Column field="name" header="Name" :expander="true" />
      <Column field="slug" header="Slug" />
      <Column field="position" header="Position" />
      <Column field="childrenCount" header="Children" />
      <Column field="taxonRuleCount" header="Rules" />
      <Column field="productCount" header="Products" />
      <Column header="" body-style="text-align: right; width: 6rem">
        <template #body="{ node }">
          <div class="flex justify-end gap-2">
            <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEditTaxon(node.data.id)" />
            <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="confirmDeleteTaxon(node.data)" />
          </div>
        </template>
      </Column>
      <template #empty>
        <div class="text-center py-8 text-muted-color">No taxons defined. Add one to start building your category tree.</div>
      </template>
    </TreeTable>
  </PageShell>
</template>
```

- [ ] **Step 2: Run build and commit**
```bash
cd app/Admin && pnpm run build-only 2>&1 | tail -1
git add app/Admin/src/features/catalog/views/TaxonomyDetail.vue
git commit -m "feat(catalog): implement Taxonomy detail view with TreeTable"
```

---

### Task 8: TaxonsList View (dual DataTable/TreeTable toggle)

**Files:**
- Create: `app/Admin/src/features/catalog/views/TaxonsList.vue`

**Interfaces:**
- Consumes: `useRouter`, `useConfirm`, `useNotify`, `useDataTableExport`, `usePagedQuery`, `PageShell`, `DataTable`, `TreeTable`, `Column`, `Toolbar`, `Card`, `TaxonApi` (Task 3), `TaxonListItem` (Task 1), `TaxonTreeItem` (Task 1), `TAXON_FILTER_FIELDS`, `TAXON_SORT_FIELDS` (Task 1)
- Produces: View at route `/catalog/taxons`

- [ ] **Step 1: Write `views/TaxonsList.vue`** — dual-mode page with toolbar toggle

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import TreeTable from 'primevue/treetable'
import Column from 'primevue/column'
import Toolbar from 'primevue/toolbar'
import Plus from '@primeicons/vue/plus'
import Trash from '@primeicons/vue/trash'
import Upload from '@primeicons/vue/upload'
import { PageShell } from '@panel'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useNotify } from '@/shared/composables/useNotify'
import { TaxonApi } from '../services/taxonApi'
import type { TaxonListItem, TaxonTreeItem } from '../types/taxon'
import { TAXON_FILTER_FIELDS, TAXON_SORT_FIELDS } from '../types/taxon'

const route = useRoute()
const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()

const { dt, exportCSV } = useDataTableExport()
const selectedItems = ref<TaxonListItem[]>([])
const searchTerm = ref('')
const viewMode = ref<'table' | 'tree'>('table')
const allowedSearchFields = ['name', 'slug']

const {
  items,
  loading,
  setSearch,
  setFilter,
  refresh,
} = usePagedQuery<TaxonListItem>('api/catalog/taxonomies/taxons', {
  allowedFilterFields: TAXON_FILTER_FIELDS,
  allowedSortFields: TAXON_SORT_FIELDS,
  allowedSearchFields,
  defaultSearchFields: allowedSearchFields,
  defaultSearchMode: 'any',
  defaultSort: ['lft'],
  defaultPageSize: 20,
})

const treeData = ref<TaxonTreeItem[]>([])
const treeLoading = ref(false)
const treeFilter = ref('')

onMounted(() => {
  const taxonomyId = route.query.taxonomyId as string | undefined
  if (taxonomyId) {
    setFilter(`taxonomyId=${taxonomyId}`)
  }
})

async function loadTree() {
  treeLoading.value = true
  const result = await TaxonApi.getTree()
  if (result.isSuccess && result.value?.tree) {
    treeData.value = result.value.tree
  }
  treeLoading.value = false
}

function toggleViewMode() {
  if (viewMode.value === 'table') {
    viewMode.value = 'tree'
    if (treeData.value.length === 0) {
      loadTree()
    }
  } else {
    viewMode.value = 'table'
  }
}

function navigateToNew() {
  const taxonomyId = route.query.taxonomyId as string | undefined
  const query = taxonomyId ? `?taxonomyId=${taxonomyId}` : ''
  router.push(`/catalog/taxons/new${query}`)
}

function navigateToEdit(id: string) {
  router.push(`/catalog/taxons/${id}`)
}

function onSearch(value: string) {
  searchTerm.value = value
  setSearch(value)
}

function clearSearch() {
  searchTerm.value = ''
  setSearch('')
}

function filterTree(name: string) {
  treeFilter.value = name ? name.toLowerCase() : ''
}

function isNodeVisible(node: TaxonTreeItem): boolean {
  if (!treeFilter.value) return true
  return node.name.toLowerCase().includes(treeFilter.value)
    || node.slug.toLowerCase().includes(treeFilter.value)
    || node.children.some(c => isNodeVisible(c))
}

function confirmDelete() {
  if (selectedItems.value.length === 0) return

  confirm.require({
    message: `Are you sure you want to delete ${selectedItems.value.length > 1 ? 'these taxons' : 'this taxon'}?`,
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
        const result = await TaxonApi.deleteTaxon(id)
        if (!result.isSuccess) failed++
      }
      selectedItems.value = []
      refresh()
      if (failed === 0) {
        notify.success(
          ids.length > 1 ? 'Taxons deleted' : 'Taxon deleted',
          ids.length > 1
            ? `${ids.length} taxons have been removed.`
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
  <PageShell title="Taxons" description="Manage product classification taxons">
    <Card>
      <template #content>
        <Toolbar>
          <template #start>
            <Button label="New" severity="secondary" class="mr-2" @click="navigateToNew">
              <Plus />
            </Button>
            <Button label="Delete" severity="secondary" :disabled="selectedItems.length === 0" class="mr-2" @click="confirmDelete">
              <Trash />
            </Button>
          </template>
          <template #end>
            <Button
              :label="viewMode === 'table' ? 'Tree' : 'Table'"
              severity="secondary"
              class="mr-2"
              :icon="viewMode === 'table' ? 'pi pi-sitemap' : 'pi pi-list'"
              @click="toggleViewMode"
            />
            <Button v-if="viewMode === 'table'" label="Export" severity="secondary" @click="exportCSV">
              <Upload />
            </Button>
          </template>
        </Toolbar>
      </template>
    </Card>

    <DataTable
      v-if="viewMode === 'table'"
      ref="dt"
      v-model:selection="selectedItems"
      :value="items"
      :loading="loading"
      :paginator="true"
      :rows="20"
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
              placeholder="Search taxons..."
              @update:model-value="onSearch($event ?? '')"
            />
          </IconField>
          <Button label="Clear" outlined @click="clearSearch" />
        </div>
      </template>
      <Column field="name" header="Name" :sortable="true" :filter="true" filter-field="name" />
      <Column field="slug" header="Slug" :sortable="true" />
      <Column field="taxonomyName" header="Taxonomy" :sortable="true" />
      <Column field="parentName" header="Parent" :sortable="true" />
      <Column field="depth" header="Depth" :sortable="true" body-style="text-align: center" />
      <Column field="position" header="Position" :sortable="true" />
      <Column field="taxonRuleCount" header="Rules" :sortable="true" body-style="text-align: center" />
      <Column field="productCount" header="Products" :sortable="true" body-style="text-align: center" />
      <Column header="" body-style="text-align: right; width: 6rem">
        <template #body="{ data }">
          <div class="flex justify-end gap-2">
            <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEdit(data.id)" />
            <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="selectedItems = [data]; confirmDelete()" />
          </div>
        </template>
      </Column>
      <template #empty>
        <div class="text-center py-8 text-muted-color">No taxons found.</div>
      </template>
    </DataTable>

    <div v-if="viewMode === 'tree'">
      <div class="flex justify-between items-center mb-3">
        <IconField>
          <InputIcon><i class="pi pi-search" /></InputIcon>
          <InputText
            v-model="treeFilter"
            placeholder="Filter tree..."
            @update:model-value="filterTree($event ?? '')"
          />
        </IconField>
      </div>

      <TreeTable
        :value="treeData"
        :loading="treeLoading"
        filter-mode="lenient"
      >
        <Column field="name" header="Name" :expander="true" />
        <Column field="slug" header="Slug" />
        <Column field="position" header="Position" />
        <Column field="taxonRuleCount" header="Rules" />
        <Column field="productCount" header="Products" />
        <Column header="" body-style="text-align: right; width: 6rem">
          <template #body="{ node }">
            <div class="flex justify-end gap-2">
              <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEdit(node.data.id)" />
              <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="selectedItems = [{ ...node.data, ...node.data }] as any; confirmDelete()" />
            </div>
          </template>
        </Column>
        <template #empty>
          <div class="text-center py-8 text-muted-color">No taxons in tree.</div>
        </template>
      </TreeTable>
    </div>
  </PageShell>
</template>
```

- [ ] **Step 2: Run build and commit**
```bash
cd app/Admin && pnpm run build-only 2>&1 | tail -1
git add app/Admin/src/features/catalog/views/TaxonsList.vue
git commit -m "feat(catalog): implement Taxons list view with DataTable/TreeTable toggle"
```

---

### Task 9: TaxonDetail View (5-tab form + Rules)

**Files:**
- Create: `app/Admin/src/features/catalog/views/TaxonDetail.vue`

**Interfaces:**
- Consumes: `useRoute`, `useRouter`, `useConfirm`, `useNotify`, `useApiErrorHandler`, `usePagedQuery`, `Tabs`, `TabList`, `Tab`, `TabPanels`, `TabPanel`, `DataTable`, `Column`, `Toolbar`, `Card`, `PageShell`, `PageHeading`, `FormSection`, `FormField`, `useTaxonomyStore` (Task 4), `TaxonApi` (Task 3), `TaxonRuleApi` (Task 3), `taxonSchema` (Task 2), `TaxonForm` (Task 2), `TaxonRuleListItem` (Task 1), `TAXON_SORT_ORDERS`, `TAXON_MATCH_POLICIES`, `TaxonTreeItem` (Task 1), `TaxonRuleFormDialog` (Task 5)
- Produces: View at route `/catalog/taxons/:id`

- [ ] **Step 1: Write `views/TaxonDetail.vue`** — 5-tab form with embedded Rules DataTable + dialog

```vue
<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
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
import { useTaxonomyStore } from '../stores/taxonomyStore'
import { TaxonApi } from '../services/taxonApi'
import { TaxonRuleApi } from '../services/taxonRuleApi'
import { taxonSchema } from '../validations/taxon'
import type { TaxonForm } from '../validations/taxon'
import type { TaxonRuleListItem } from '../types/taxonRule'
import { TAXON_SORT_ORDERS, TAXON_MATCH_POLICIES } from '../types/taxon'
import TaxonRuleFormDialog from '../components/TaxonRuleFormDialog.vue'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const confirm = useConfirm()
const { handleResult } = useApiErrorHandler()
const taxonomyStore = useTaxonomyStore()

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => isEdit.value ? 'Edit Taxon' : 'New Taxon')
const activeTab = ref('0')

const form = ref<TaxonForm>({
  taxonomyId: (route.query.taxonomyId as string) || '',
  parentId: (route.query.parentId as string) || null,
  name: '',
  presentation: '',
  slug: '',
  description: null,
  position: 0,
  metaTitle: null,
  metaDescription: null,
  metaKeywords: null,
  imageUrl: null,
  squareImageUrl: null,
  automatic: false,
  rulesMatchPolicy: 'All',
  sortOrder: 'Manual',
  hideFromNav: false,
})

const fieldErrors = ref<Record<string, string>>({})
const saving = ref(false)

const parentOptions = ref<{ label: string; value: string }[]>([])
const dialogVisible = ref(false)
const editingRule = ref<TaxonRuleListItem | null>(null)

const {
  items: rules,
  loading: rulesLoading,
  refresh: refreshRules,
} = usePagedQuery<TaxonRuleListItem>('', {
  allowedFilterFields: [],
  allowedSortFields: [],
  defaultPageSize: 100,
})

async function initEditMode(id: string) {
  const result = await TaxonApi.getTaxon(id)
  if (result.isSuccess) {
    const t = result.value
    form.value = {
      taxonomyId: t.taxonomyId,
      parentId: t.parentId,
      name: t.name,
      presentation: t.presentation,
      slug: t.slug,
      description: t.description,
      position: t.position,
      metaTitle: t.metaTitle,
      metaDescription: t.metaDescription,
      metaKeywords: t.metaKeywords,
      imageUrl: t.imageUrl,
      squareImageUrl: t.squareImageUrl,
      automatic: t.automatic,
      rulesMatchPolicy: t.rulesMatchPolicy,
      sortOrder: t.sortOrder,
      hideFromNav: t.hideFromNav,
    }

    await Promise.all([loadParents(result.value.taxonomyId), loadRules(id)])
  } else {
    handleResult(result)
    router.push('/catalog/taxons')
  }
}

async function loadParents(taxonomyId: string) {
  const result = await TaxonApi.getTree()
  if (result.isSuccess && result.value?.tree) {
    const flat: { label: string; value: string }[] = [{ label: '(None — root level)', value: '' }]
    function walk(nodes: any[], depth: number) {
      for (const n of nodes) {
        flat.push({ label: '  '.repeat(depth) + '|-- ' + n.name, value: n.id })
        if (n.children?.length) walk(n.children, depth + 1)
      }
    }
    walk(result.value.tree, 1)
    parentOptions.value = flat
  }
}

async function loadRules(taxonId: string) {
  const result = await TaxonRuleApi.getRules(taxonId)
  if (result.isSuccess) {
    rules.value = result.items
  }
}

onMounted(async () => {
  await taxonomyStore.fetchActive()
  if (isEdit.value) {
    await initEditMode(route.params.id as string)
  } else if (form.value.taxonomyId) {
    await loadParents(form.value.taxonomyId)
  }
})

watch(() => route.params.id, (newId) => {
  if (newId && newId !== 'new') {
    initEditMode(newId as string)
  }
})

async function onSave() {
  fieldErrors.value = {}
  const parsed = taxonSchema.safeParse({
    ...form.value,
    parentId: form.value.parentId || null,
    description: form.value.description || null,
  })

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
    taxonomyId: data.taxonomyId,
    parentId: data.parentId || null,
    name: data.name,
    presentation: data.presentation,
    slug: data.slug,
    description: data.description ?? null,
    position: data.position,
    metaTitle: data.metaTitle ?? null,
    metaDescription: data.metaDescription ?? null,
    metaKeywords: data.metaKeywords ?? null,
    imageUrl: data.imageUrl ?? null,
    squareImageUrl: data.squareImageUrl ?? null,
    automatic: data.automatic,
    rulesMatchPolicy: data.rulesMatchPolicy,
    sortOrder: data.sortOrder,
    hideFromNav: data.hideFromNav,
  }

  const result = isEdit.value
    ? await TaxonApi.updateTaxon(route.params.id as string, request)
    : await TaxonApi.createTaxon(request)

  saving.value = false

  if (result.isSuccess) {
    notify.success(isEdit.value ? 'Taxon updated' : 'Taxon created')
    if (!isEdit.value && result.value) {
      const created = result.value
      form.value = {
        ...form.value,
        taxonomyId: created.taxonomyId,
        parentId: created.parentId,
        name: created.name,
        presentation: created.presentation,
        slug: created.slug,
        description: created.description,
        position: created.position,
        metaTitle: created.metaTitle,
        metaDescription: created.metaDescription,
        metaKeywords: created.metaKeywords,
        imageUrl: created.imageUrl,
        squareImageUrl: created.squareImageUrl,
        automatic: created.automatic,
        rulesMatchPolicy: created.rulesMatchPolicy,
        sortOrder: created.sortOrder,
        hideFromNav: created.hideFromNav,
      }
      router.replace(`/catalog/taxons/${created.id}`)
    }
  } else {
    handleResult(result)
  }
}

function onCancel() {
  router.push('/catalog/taxons')
}

function openAddRule() {
  editingRule.value = null
  dialogVisible.value = true
}

function openEditRule(rule: TaxonRuleListItem) {
  editingRule.value = rule
  dialogVisible.value = true
}

function onRuleSaved() {
  refreshRules()
  loadRules(route.params.id as string)
}

function confirmDeleteRule(rule: TaxonRuleListItem) {
  confirm.require({
    message: `Are you sure you want to delete this rule?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const result = await TaxonRuleApi.deleteRule(route.params.id as string, rule.id)
      if (result.isSuccess) {
        notify.success('Rule deleted')
        loadRules(route.params.id as string)
      } else {
        notify.error('Delete failed', result.errors?.[0]?.message ?? 'Could not delete rule.')
      }
    },
  })
}
</script>

<template>
  <PageShell :title="pageTitle">
    <PageHeading
      title=""
      :breadcrumbs="[
        { label: 'Home', to: '/' },
        { label: 'Taxons', to: '/catalog/taxons' },
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
        <Tab value="1">Settings</Tab>
        <Tab value="2">SEO</Tab>
        <Tab value="3">Images</Tab>
        <Tab v-if="isEdit" value="4">Rules</Tab>
      </TabList>

      <TabPanels>
        <TabPanel value="0">
          <FormSection title="General">
            <FormField label="Taxonomy" :required="true" :invalid="!!fieldErrors.taxonomyId">
              <Select v-model="form.taxonomyId" :options="taxonomyStore.activeTaxonomies" option-label="name" option-value="id" fluid :disabled="!isEdit && !!route.query.taxonomyId" />
              <small v-if="fieldErrors.taxonomyId" class="text-red-500">{{ fieldErrors.taxonomyId }}</small>
            </FormField>
            <FormField label="Parent" help-text="Leave empty for root-level taxon">
              <Select v-model="form.parentId" :options="parentOptions" option-label="label" option-value="value" fluid show-clear />
            </FormField>
            <FormField label="Name" :required="true" :invalid="!!fieldErrors.name">
              <InputText v-model="form.name" fluid class="w-full" />
              <small v-if="fieldErrors.name" class="text-red-500">{{ fieldErrors.name }}</small>
            </FormField>
            <FormField label="Presentation" :required="true" :invalid="!!fieldErrors.presentation">
              <InputText v-model="form.presentation" fluid class="w-full" />
              <small v-if="fieldErrors.presentation" class="text-red-500">{{ fieldErrors.presentation }}</small>
            </FormField>
            <FormField label="Slug" :required="true" :invalid="!!fieldErrors.slug" help-text="Lowercase alphanumeric with hyphens (e.g. running-shoes)">
              <InputText v-model="form.slug" fluid class="w-full" />
              <small v-if="fieldErrors.slug" class="text-red-500">{{ fieldErrors.slug }}</small>
            </FormField>
            <FormField label="Description" :invalid="!!fieldErrors.description">
              <Textarea v-model="form.description" fluid class="w-full" rows="3" />
              <small v-if="fieldErrors.description" class="text-red-500">{{ fieldErrors.description }}</small>
            </FormField>
            <FormField label="Position" :invalid="!!fieldErrors.position" help-text="Sort order">
              <InputNumber v-model="form.position" fluid :min="-1" class="w-full" />
              <small v-if="fieldErrors.position" class="text-red-500">{{ fieldErrors.position }}</small>
            </FormField>
          </FormSection>
        </TabPanel>

        <TabPanel value="1">
          <FormSection title="Settings">
            <FormField label="Sort Order">
              <Select v-model="form.sortOrder" :options="TAXON_SORT_ORDERS" fluid />
            </FormField>
            <FormField label="Hide from Navigation">
              <ToggleSwitch v-model="form.hideFromNav" />
            </FormField>
            <FormField label="Automatic Classification" help-text="Use rules to auto-assign products">
              <ToggleSwitch v-model="form.automatic" />
            </FormField>
            <FormField label="Rules Match Policy" help-text="How multiple rules are combined">
              <Select v-model="form.rulesMatchPolicy" :options="TAXON_MATCH_POLICIES" fluid />
            </FormField>
          </FormSection>
        </TabPanel>

        <TabPanel value="2">
          <FormSection title="SEO">
            <FormField label="Meta Title" :invalid="!!fieldErrors.metaTitle">
              <InputText v-model="form.metaTitle" fluid class="w-full" />
              <small v-if="fieldErrors.metaTitle" class="text-red-500">{{ fieldErrors.metaTitle }}</small>
            </FormField>
            <FormField label="Meta Description" :invalid="!!fieldErrors.metaDescription">
              <Textarea v-model="form.metaDescription" fluid class="w-full" rows="3" />
              <small v-if="fieldErrors.metaDescription" class="text-red-500">{{ fieldErrors.metaDescription }}</small>
            </FormField>
            <FormField label="Meta Keywords" :invalid="!!fieldErrors.metaKeywords">
              <InputText v-model="form.metaKeywords" fluid class="w-full" />
              <small v-if="fieldErrors.metaKeywords" class="text-red-500">{{ fieldErrors.metaKeywords }}</small>
            </FormField>
          </FormSection>
        </TabPanel>

        <TabPanel value="3">
          <FormSection title="Images">
            <FormField label="Image URL" :invalid="!!fieldErrors.imageUrl">
              <InputText v-model="form.imageUrl" fluid class="w-full" />
              <small v-if="fieldErrors.imageUrl" class="text-red-500">{{ fieldErrors.imageUrl }}</small>
            </FormField>
            <FormField label="Square Image URL" :invalid="!!fieldErrors.squareImageUrl">
              <InputText v-model="form.squareImageUrl" fluid class="w-full" />
              <small v-if="fieldErrors.squareImageUrl" class="text-red-500">{{ fieldErrors.squareImageUrl }}</small>
            </FormField>
          </FormSection>
        </TabPanel>

        <TabPanel v-if="isEdit" value="4">
          <Card>
            <template #content>
              <Toolbar>
                <template #start>
                  <Button label="Add Rule" severity="secondary" @click="openAddRule">
                    <Plus />
                  </Button>
                </template>
              </Toolbar>
            </template>
          </Card>

          <DataTable :value="rules" :loading="rulesLoading" data-key="id">
            <Column field="type" header="Type" />
            <Column field="matchPolicy" header="Match Policy" />
            <Column field="value" header="Value" />
            <Column header="" body-style="text-align: right; width: 6rem">
              <template #body="{ data }">
                <div class="flex justify-end gap-2">
                  <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="openEditRule(data)" />
                  <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="confirmDeleteRule(data)" />
                </div>
              </template>
            </Column>
            <template #empty>
              <div class="text-center py-8 text-muted-color">No rules defined.</div>
            </template>
          </DataTable>
        </TabPanel>
      </TabPanels>
    </Tabs>

    <TaxonRuleFormDialog
      v-if="isEdit"
      :visible="dialogVisible"
      :taxon-id="(route.params.id as string) || ''"
      :editing-rule="editingRule"
      @update:visible="dialogVisible = $event"
      @saved="onRuleSaved"
    />
  </PageShell>
</template>
```

- [ ] **Step 2: Run build and commit**
```bash
cd app/Admin && pnpm run build-only 2>&1 | tail -1
git add app/Admin/src/features/catalog/views/TaxonDetail.vue
git commit -m "feat(catalog): implement Taxon detail view with 5-tab form and Rules management"
```

---

### Task 10: Routes & Barrels

**Files:**
- Modify: `app/Admin/src/features/catalog/views/index.ts` — add new exports
- Modify: `app/Admin/src/features/catalog/routes/index.ts` — add 2 routes + 1 menu item

**Interfaces:**
- Consumes: All view components from Tasks 6-9
- Produces: Updated route definitions and barrel exports

- [ ] **Step 1: Modify `views/index.ts`** — append:

```ts
export { default as TaxonsList } from './TaxonsList.vue'
export { default as TaxonDetail } from './TaxonDetail.vue'
```

(Keep existing 6 exports for ProductsList, ProductDetail, TaxonomiesList, TaxonomyDetail, OptionTypesList, OptionTypeDetail.)

- [ ] **Step 2: Modify `routes/index.ts`** — add 2 routes BEFORE `catalog/taxonomies/:id` AND add taxon lazy imports AND add "Taxons" menu item:

**Add lazy imports** (after the existing 6):
```ts
const TaxonsList = () => import('../views/TaxonsList.vue')
const TaxonDetail = () => import('../views/TaxonDetail.vue')
```

**Add routes** (insert after `catalog/taxonomies` and BEFORE `catalog/taxonomies/:id`):
```ts
  {
    path: 'catalog/taxons',
    name: 'catalog-taxons',
    component: TaxonsList,
    meta: { title: 'Taxons' },
  },
  {
    path: 'catalog/taxons/:id',
    name: 'catalog-taxon-detail',
    component: TaxonDetail,
    meta: { title: 'Taxon Detail' },
  },
```

**Add menu item** under the "Catalog" menu items:
```ts
{ label: 'Taxons', icon: 'pi pi-fw pi-sitemap', to: '/catalog/taxons' },
```

- [ ] **Step 3: Run full build + tests + lint**
```bash
cd app/Admin && pnpm run build-only 2>&1 | tail -1
cd app/Admin && pnpm run test:unit -- run 2>&1 | grep 'Test Files\|Tests'
cd app/Admin && pnpm run lint 2>&1 | tail -3
```
Expected: Build passes, all 472+ tests pass, no new lint errors.

- [ ] **Step 4: Commit**
```bash
git add app/Admin/src/features/catalog/views/index.ts
git add app/Admin/src/features/catalog/routes/index.ts
git commit -m "feat(catalog): wire taxonomy routes and menu items"
```
