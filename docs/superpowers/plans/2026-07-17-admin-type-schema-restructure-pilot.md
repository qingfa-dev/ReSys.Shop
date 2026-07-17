# Admin Type/Schema Restructure — Taxonomies & Taxa Pilot

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restructure the Taxonomies and Taxa entity types and schemas into the canonical Parameters/Request/Response/Query pattern — one `{Entity}.Schema.ts` per entity, four `.Type.ts` files per entity — as a pilot for the full Admin SPA.

**Architecture:** Current `*.schema.ts` files (lowercase, sometimes multi-entity) are split into per-entity `*.Schema.ts` files. Current `*.domain.types.ts` (response models) become `*.Response.Type.ts`. Current `*.request.types.ts` split into `*.Parameters.Type.ts` (form fields from schema inference), `*.Request.Type.ts` (extends Parameters, adds DTO-specific fields), and `*.Query.Type.ts` (pagination/filter types). Old files deleted; all import sites updated.

**Tech Stack:** TypeScript 5.x, Zod 3.x, pnpm workspace

## Global Constraints

- File naming: `{Entity}.Schema.ts`, `{Entity}.Parameters.Type.ts`, `{Entity}.Request.Type.ts`, `{Entity}.Response.Type.ts`, `{Entity}.Query.Type.ts`
- Parameters must be derived from Zod schema via `z.infer<typeof Schema>`
- Request types must extend Parameters via intersection (`&`) or type alias, never duplicate fields
- Response types are defined independently from schemas (API shapes != form shapes)
- Child entities (Taxon under Taxonomy, TaxonRule under Taxon) have their own `schemas/` and `types/` dirs
- All imports must be updated — no stale `*.domain.types.ts` or `*.request.types.ts` references survive
- `pnpm run typecheck` must pass after each task; `pnpm run lint` and `pnpm run test:unit` after the final task

---

## File Structure

### Created files (17 total)

```
app/Admin/src/features/catalog/taxonomies/
  schemas/
    Taxonomy.Schema.ts              # Zod schema for Taxonomy form
    Taxon.Schema.ts                 # Zod schema for Taxon form (from split)
    TaxonRule.Schema.ts             # Zod schema for TaxonRule form (from split)
  types/
    Taxonomy.Parameters.Type.ts     # z.infer<typeof TaxonomySchema>
    Taxonomy.Request.Type.ts        # Create/Update request DTOs
    Taxonomy.Response.Type.ts       # TaxonomyListItem, TaxonomyDetail, TaxonNode
    Taxonomy.Query.Type.ts          # TaxonomyQuery = ServerQueryingParameters
    taxa/
      types/
        Taxon.Parameters.Type.ts    # z.infer<typeof TaxonSchema>
        Taxon.Request.Type.ts       # Create/Update request DTOs
        Taxon.Response.Type.ts      # TaxonListItem, TaxonTreeItem, TaxonDetail
        Taxon.Query.Type.ts         # TaxonQuery extends ServerQueryingParameters
        TaxonRule.Parameters.Type.ts # z.infer<typeof TaxonRuleSchema>
        TaxonRule.Request.Type.ts   # Create/Update rule DTOs
        TaxonRule.Response.Type.ts  # TaxonRuleListItem
```

### Deleted files (6 total)

```
app/Admin/src/features/catalog/taxonomies/
  schemas/taxonomy.schema.ts        # → replaced by Taxonomy.Schema.ts
  taxa/schemas/taxon.schema.ts       # → replaced by Taxon.Schema.ts + TaxonRule.Schema.ts
  types/taxonomy.domain.types.ts     # → replaced by Taxonomy.Response.Type.ts
  types/taxonomy.request.types.ts    # → replaced by Taxonomy.Parameters/Request/Query.Type.ts
  taxa/types/taxon.domain.types.ts   # → replaced by Taxon.Response.Type.ts + TaxonRule.Response.Type.ts
  taxa/types/taxon.request.types.ts  # → replaced by Taxon.Parameters/Request/Query.Type.ts + TaxonRule.Parameters/Request.Type.ts
```

### Modified files (4 import updates)

```
app/Admin/src/features/catalog/repository/taxon.repository.ts
app/Admin/src/features/catalog/repository/taxonomy.repository.ts
app/Admin/src/features/catalog/mapper/catalog.mapper.ts
app/Admin/src/features/catalog/catalog.routes.ts       (verify — may not need changes)
```

---

## Tasks

### Task 1: Split taxon.schema.ts into per-entity Schema files

**Files:**
- Create: `app/Admin/src/features/catalog/taxonomies/schemas/Taxon.Schema.ts`
- Create: `app/Admin/src/features/catalog/taxonomies/schemas/TaxonRule.Schema.ts`

**Interfaces:**
- Produces: `TaxonSchema` (ZodObject), `TaxonParameters` (inferred type alias), `TaxonRuleSchema` (ZodObject), `TaxonRuleParameters` (inferred type alias)

- [ ] **Step 1: Write typecheck assertion in a temp test**

```typescript
// File: app/Admin/src/features/catalog/taxonomies/taxa/tests/schema-typecheck.spec.ts
import { describe, it, expect } from 'vitest'
import { z } from 'zod'

describe('Taxon Schema shape', () => {
  it('TaxonSchema defines required fields', () => {
    // This test will fail until we create the file — that's the point
    expect(() => {
      // will fail: cannot find module
      require('../schemas/Taxon.Schema')
    }).toThrow()
  })
})
```

- [ ] **Step 2: Verify it fails**

Run: `pnpm vitest run --reporter verbose -t "Taxon Schema shape"`
Expected: FAIL — `Cannot find module '../schemas/Taxon.Schema'`

- [ ] **Step 3: Create `Taxon.Schema.ts`**

```typescript
// app/Admin/src/features/catalog/taxonomies/schemas/Taxon.Schema.ts
import { z } from 'zod'

export const TaxonSchema = z.object({
  taxonomyId: z.string().uuid('Taxonomy is required'),
  name: z.string().min(1, 'Name is required').max(100),
  presentation: z.string().min(1, 'Presentation is required').max(100),
  description: z.string().max(500).optional().nullable(),
  slug: z.string().min(1, 'Slug is required').max(100),
  position: z.number().int().min(0).default(0),
  hideFromNav: z.boolean().default(false),
  parentId: z.string().uuid().optional().nullable(),
  automatic: z.boolean().default(false),
  rulesMatchPolicy: z.enum(['all', 'any']).default('all'),
  sortOrder: z.string().default('manual'),
  metaTitle: z.string().max(100).optional().nullable(),
  metaDescription: z.string().max(255).optional().nullable(),
  metaKeywords: z.string().max(255).optional().nullable(),
})

export type TaxonParameters = z.infer<typeof TaxonSchema>
```

- [ ] **Step 4: Create `TaxonRule.Schema.ts`**

```typescript
// app/Admin/src/features/catalog/taxonomies/schemas/TaxonRule.Schema.ts
import { z } from 'zod'

export const TaxonRuleSchema = z.object({
  type: z.string().min(1, 'Rule type is required'),
  value: z.string().min(1, 'Value is required'),
  matchPolicy: z.string().min(1, 'Match policy is required'),
})

export type TaxonRuleParameters = z.infer<typeof TaxonRuleSchema>
```

- [ ] **Step 5: Update the temp test to verify schemas load**

```typescript
// File: app/Admin/src/features/catalog/taxonomies/taxa/tests/schema-typecheck.spec.ts
import { describe, it, expect } from 'vitest'
import { TaxonSchema } from '../schemas/Taxon.Schema'
import { TaxonRuleSchema } from '../schemas/TaxonRule.Schema'

describe('Taxon Schema shape', () => {
  it('TaxonSchema defines correct shape', () => {
    const result = TaxonSchema.safeParse({
      taxonomyId: '00000000-0000-0000-0000-000000000000',
      name: 'Test Taxón',
      presentation: 'Test',
      slug: 'test-taxon',
      position: 0,
      hideFromNav: false,
      automatic: false,
      rulesMatchPolicy: 'all',
      sortOrder: 'manual',
    })
    expect(result.success).toBe(true)
  })

  it('TaxonRuleSchema validates required fields', () => {
    const result = TaxonRuleSchema.safeParse({ type: '', value: '', matchPolicy: '' })
    expect(result.success).toBe(false)
  })
})
```

- [ ] **Step 6: Run test to verify it passes**

Run: `pnpm vitest run --reporter verbose -t "Taxon Schema shape"`
Expected: PASS

- [ ] **Step 7: Delete the temp test file**

```bash
rm app/Admin/src/features/catalog/taxonomies/taxa/tests/schema-typecheck.spec.ts
```

- [ ] **Step 8: Commit**

```bash
git add app/Admin/src/features/catalog/taxonomies/schemas/Taxon.Schema.ts
git add app/Admin/src/features/catalog/taxonomies/schemas/TaxonRule.Schema.ts
git commit -m "refactor(admin): split taxon schema into per-entity Schema files"
```

---

### Task 2: Create Taxonomy.Schema.ts (rename)

**Files:**
- Create: `app/Admin/src/features/catalog/taxonomies/schemas/Taxonomy.Schema.ts`
- Modify: `app/Admin/src/features/catalog/taxonomies/tests/taxonomy.schema.spec.ts`
- Delete: `app/Admin/src/features/catalog/taxonomies/schemas/taxonomy.schema.ts`

**Interfaces:**
- Produces: `TaxonomySchema` (ZodObject), `TaxonomyParameters` (inferred type alias)
- Consumes: none (new file, content derived from old)

- [ ] **Step 1: Create `Taxonomy.Schema.ts`**

```typescript
// app/Admin/src/features/catalog/taxonomies/schemas/Taxonomy.Schema.ts
import { z } from 'zod'

export const TaxonomySchema = z.object({
  name: z.string().min(1, 'Name is required').max(100),
  presentation: z.string().min(1, 'Presentation is required').max(100),
  position: z.number().int().min(0).default(0),
})

export type TaxonomyParameters = z.infer<typeof TaxonomySchema>
```

- [ ] **Step 2: Update spec test import before deleting old file**

```typescript
// app/Admin/src/features/catalog/taxonomies/tests/taxonomy.schema.spec.ts
// BEFORE:
// import { TaxonomySchema } from '../schemas/taxonomy.schema'
// AFTER:
import { TaxonomySchema } from '../schemas/Taxonomy.Schema'
```

- [ ] **Step 3: Delete old lowercase file**

```bash
git rm app/Admin/src/features/catalog/taxonomies/schemas/taxonomy.schema.ts
```

- [ ] **Step 4: Verify no stale imports**

Run: `rg "from.*['\"]taxonomies/schemas/taxonomy\.schema" app/Admin/src/`
Expected: no matches

- [ ] **Step 5: Run typecheck and spec test**

Run: `pnpm run typecheck`
Expected: PASS

Run: `pnpm vitest run --reporter verbose -t "Taxonomy"`
Expected: PASS (schema spec test uses new import path)

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/features/catalog/taxonomies/schemas/Taxonomy.Schema.ts
git add app/Admin/src/features/catalog/taxonomies/tests/taxonomy.schema.spec.ts
git commit -m "refactor(admin): rename taxonomy.schema.ts to Taxonomy.Schema.ts"
```

---

### Task 3: Create Taxonomy type files (Parameters, Request, Response, Query)

**Files:**
- Create: `app/Admin/src/features/catalog/taxonomies/types/Taxonomy.Parameters.Type.ts`
- Create: `app/Admin/src/features/catalog/taxonomies/types/Taxonomy.Request.Type.ts`
- Create: `app/Admin/src/features/catalog/taxonomies/types/Taxonomy.Response.Type.ts`
- Create: `app/Admin/src/features/catalog/taxonomies/types/Taxonomy.Query.Type.ts`

**Interfaces:**
- Produces: `TaxonomyParameters`, `CreateTaxonomyRequest`, `UpdateTaxonomyRequest`, `TaxonomyListItem`, `TaxonNode`, `TaxonomyDetail`, `TaxonomyQuery`

- [ ] **Step 1: Create `Taxonomy.Parameters.Type.ts`**

```typescript
// app/Admin/src/features/catalog/taxonomies/types/Taxonomy.Parameters.Type.ts
import type { TaxonomyParameters } from '../schemas/Taxonomy.Schema'

export type { TaxonomyParameters }
```

- [ ] **Step 2: Create `Taxonomy.Request.Type.ts`**

```typescript
// app/Admin/src/features/catalog/taxonomies/types/Taxonomy.Request.Type.ts
import type { TaxonomyParameters } from '../schemas/Taxonomy.Schema'

export type CreateTaxonomyRequest = TaxonomyParameters

export type UpdateTaxonomyRequest = Partial<TaxonomyParameters>
```

- [ ] **Step 3: Create `Taxonomy.Response.Type.ts`**

```typescript
// app/Admin/src/features/catalog/taxonomies/types/Taxonomy.Response.Type.ts
export interface TaxonomyListItem {
  id: string
  name: string
  presentation: string | null
  position: number
  taxonsCount: number
  createdAtUtc: string
  modifiedAtUtc: string
}

export interface TaxonNode {
  id: string
  name: string
  slug: string
  position: number
  child: TaxonNode[]
}

export interface TaxonomyDetail extends TaxonomyListItem {
  root: TaxonNode | null
}
```

- [ ] **Step 4: Create `Taxonomy.Query.Type.ts`**

```typescript
// app/Admin/src/features/catalog/taxonomies/types/Taxonomy.Query.Type.ts
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export type TaxonomyQuery = ServerQueryingParameters
```

- [ ] **Step 5: Typecheck**

Run: `pnpm run typecheck`
Expected: PASS (no consumers reference these yet; just verify no import errors in the new files)

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/features/catalog/taxonomies/types/Taxonomy.*.Type.ts
git commit -m "refactor(admin): create Taxonomy Parameters/Request/Response/Query Type files"
```

---

### Task 4: Create TaxonRule type files (Parameters, Request, Response)

**Files:**
- Create: `app/Admin/src/features/catalog/taxonomies/taxa/types/TaxonRule.Parameters.Type.ts`
- Create: `app/Admin/src/features/catalog/taxonomies/taxa/types/TaxonRule.Request.Type.ts`
- Create: `app/Admin/src/features/catalog/taxonomies/taxa/types/TaxonRule.Response.Type.ts`

**Interfaces:**
- Produces: `TaxonRuleParameters`, `CreateTaxonRuleRequest`, `UpdateTaxonRuleRequest`, `TaxonRuleListItem`
- Consumes: `TaxonRuleSchema` from `../../schemas/TaxonRule.Schema`

- [ ] **Step 1: Create `TaxonRule.Parameters.Type.ts`**

```typescript
// app/Admin/src/features/catalog/taxonomies/taxa/types/TaxonRule.Parameters.Type.ts
import type { TaxonRuleParameters } from '../../schemas/TaxonRule.Schema'

export type { TaxonRuleParameters }
```

- [ ] **Step 2: Create `TaxonRule.Request.Type.ts`**

```typescript
// app/Admin/src/features/catalog/taxonomies/taxa/types/TaxonRule.Request.Type.ts
import type { TaxonRuleParameters } from './TaxonRule.Parameters.Type'

export type CreateTaxonRuleRequest = TaxonRuleParameters

export type UpdateTaxonRuleRequest = CreateTaxonRuleRequest
```

- [ ] **Step 3: Create `TaxonRule.Response.Type.ts`**

```typescript
// app/Admin/src/features/catalog/taxonomies/taxa/types/TaxonRule.Response.Type.ts
export interface TaxonRuleListItem {
  id: string
  taxonId: string
  type: string
  value: string
  matchPolicy: string
}
```

- [ ] **Step 4: Typecheck**

Run: `pnpm run typecheck`
Expected: PASS (no external dependencies — only imports schemas and its own Parameters)

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/catalog/taxonomies/taxa/types/TaxonRule.*.Type.ts
git commit -m "refactor(admin): create TaxonRule Parameters/Request/Response Type files"
```

---

### Task 5: Create Taxon type files (Parameters, Request, Response, Query)

**Files:**
- Create: `app/Admin/src/features/catalog/taxonomies/taxa/types/Taxon.Parameters.Type.ts`
- Create: `app/Admin/src/features/catalog/taxonomies/taxa/types/Taxon.Request.Type.ts`
- Create: `app/Admin/src/features/catalog/taxonomies/taxa/types/Taxon.Response.Type.ts`
- Create: `app/Admin/src/features/catalog/taxonomies/taxa/types/Taxon.Query.Type.ts`

**Interfaces:**
- Produces: `TaxonParameters`, `CreateTaxonRequest`, `UpdateTaxonRequest`, `TaxonListItem`, `TaxonTreeItem`, `TaxonDetail`, `TaxonQuery`
- Consumes: `TaxonRuleParameters` from `./TaxonRule.Parameters.Type` (Task 4), `TaxonRuleListItem` from `./TaxonRule.Response.Type` (Task 4)

- [ ] **Step 1: Create `Taxon.Parameters.Type.ts`**

```typescript
// app/Admin/src/features/catalog/taxonomies/taxa/types/Taxon.Parameters.Type.ts
import type { TaxonParameters } from '../../schemas/Taxon.Schema'

export type { TaxonParameters }
```

- [ ] **Step 2: Create `Taxon.Request.Type.ts`**

```typescript
// app/Admin/src/features/catalog/taxonomies/taxa/types/Taxon.Request.Type.ts
import type { TaxonParameters } from '../../schemas/Taxon.Schema'
import type { TaxonRuleParameters } from './TaxonRule.Parameters.Type'

export type CreateTaxonRequest = TaxonParameters & {
  rules?: TaxonRuleParameters[]
}

export type UpdateTaxonRequest = CreateTaxonRequest
```

- [ ] **Step 3: Create `Taxon.Response.Type.ts`**

```typescript
// app/Admin/src/features/catalog/taxonomies/taxa/types/Taxon.Response.Type.ts
import type { TaxonRuleListItem } from './TaxonRule.Response.Type'

export interface TaxonListItem {
  id: string
  taxonomyId: string
  parentId?: string
  name: string
  presentation: string
  description?: string
  slug: string
  permalink: string
  prettyName: string
  position: number
  hideFromNav: boolean
  depth: number
  productCount: number
  childrenCount: number
  lft: number
  rgt: number
  hasChildren: boolean
  automatic: boolean
  createdAtUtc: string
  modifiedAtUtc: string
}

export interface TaxonTreeItem extends TaxonListItem {
  key: string
  isExpanded?: boolean
  children: TaxonTreeItem[]
}

export interface TaxonDetail extends TaxonListItem {
  rulesMatchPolicy: string
  sortOrder: string
  metaTitle?: string
  metaDescription?: string
  metaKeywords?: string
  taxonRuleCount: number
  rules?: TaxonRuleListItem[]
}
```

- [ ] **Step 4: Create `Taxon.Query.Type.ts`**

```typescript
// app/Admin/src/features/catalog/taxonomies/taxa/types/Taxon.Query.Type.ts
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export interface TaxonQuery extends ServerQueryingParameters {
  taxonomyId?: string[]
  focusedTaxonId?: string
  includeLeavesOnly?: boolean
  includeHidden?: boolean
  maxDepth?: number
}
```

- [ ] **Step 5: Typecheck**

Run: `pnpm run typecheck`
Expected: PASS (TaxonRule types already exist from Task 4)

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/features/catalog/taxonomies/taxa/types/Taxon.*.Type.ts
git commit -m "refactor(admin): create Taxon Parameters/Request/Response/Query Type files"
```

---

### Task 6: Delete old type files and update repository imports

**Files:**
- Delete: `app/Admin/src/features/catalog/taxonomies/types/taxonomy.domain.types.ts`
- Delete: `app/Admin/src/features/catalog/taxonomies/types/taxonomy.request.types.ts`
- Delete: `app/Admin/src/features/catalog/taxonomies/taxa/types/taxon.domain.types.ts`
- Delete: `app/Admin/src/features/catalog/taxonomies/taxa/types/taxon.request.types.ts`
- Modify: `app/Admin/src/features/catalog/repository/taxonomy.repository.ts`
- Modify: `app/Admin/src/features/catalog/repository/taxon.repository.ts`

**Interfaces:**
- Consumes: all new type exports from Tasks 3-5
- All import paths must resolve without errors

- [ ] **Step 1: Delete old type files**

```bash
git rm app/Admin/src/features/catalog/taxonomies/types/taxonomy.domain.types.ts
git rm app/Admin/src/features/catalog/taxonomies/types/taxonomy.request.types.ts
git rm app/Admin/src/features/catalog/taxonomies/taxa/types/taxon.domain.types.ts
git rm app/Admin/src/features/catalog/taxonomies/taxa/types/taxon.request.types.ts
```

- [ ] **Step 2: Update `taxonomy.repository.ts` imports**

```typescript
// app/Admin/src/features/catalog/repository/taxonomy.repository.ts
// BEFORE:
// import type { TaxonomyDetail, TaxonomyListItem } from '../taxonomies/types/taxonomy.domain.types'
// import type { CreateTaxonomyRequest, UpdateTaxonomyRequest } from '../taxonomies/types/taxonomy.request.types'

// AFTER:
import type { TaxonomyDetail, TaxonomyListItem } from '../taxonomies/types/Taxonomy.Response.Type'
import type { CreateTaxonomyRequest, UpdateTaxonomyRequest } from '../taxonomies/types/Taxonomy.Request.Type'
```

- [ ] **Step 3: Update `taxon.repository.ts` imports**

```typescript
// app/Admin/src/features/catalog/repository/taxon.repository.ts
// BEFORE:
// import type { TaxonDetail, TaxonListItem, TaxonTreeItem, TaxonRuleListItem } from "../taxonomies/taxa/types/taxon.domain.types";
// import type { CreateTaxonRequest, UpdateTaxonRequest, CreateTaxonRuleRequest, UpdateTaxonRuleRequest } from "../taxonomies/taxa/types/taxon.request.types";

// AFTER:
import type {
  TaxonDetail,
  TaxonListItem,
  TaxonTreeItem,
} from "../taxonomies/taxa/types/Taxon.Response.Type";
import type { TaxonRuleListItem } from "../taxonomies/taxa/types/TaxonRule.Response.Type";
import type { CreateTaxonRequest, UpdateTaxonRequest } from "../taxonomies/taxa/types/Taxon.Request.Type";
import type { CreateTaxonRuleRequest, UpdateTaxonRuleRequest } from "../taxonomies/taxa/types/TaxonRule.Request.Type";
```

- [ ] **Step 4: Run typecheck**

Run: `pnpm run typecheck`
Expected: PASS

- [ ] **Step 5: Run existing unit tests (schema specs)**

Run: `pnpm run test:unit -- --reporter verbose`
Expected: PASS (schema spec tests import from the old file paths — they will fail after deletion. Check the next step if they fail.)

- [ ] **Step 6: Fix schema spec test imports**

If test fails, update these test imports:

```typescript
// app/Admin/src/features/catalog/taxonomies/tests/taxonomy.schema.spec.ts
// BEFORE:
// import { TaxonomySchema } from '../schemas/taxonomy.schema'
// AFTER:
import { TaxonomySchema } from '../schemas/Taxonomy.Schema'
```

```typescript
// app/Admin/src/features/catalog/taxonomies/taxa/tests/taxon.schema.spec.ts
// BEFORE:
// import { TaxonSchema, TaxonRuleSchema } from '../schemas/taxon.schema'
// AFTER:
import { TaxonSchema } from '../schemas/Taxon.Schema'
import { TaxonRuleSchema } from '../schemas/TaxonRule.Schema'
```

- [ ] **Step 7: Re-run tests**

Run: `pnpm run test:unit -- --reporter verbose`
Expected: PASS

- [ ] **Step 8: Commit**

```bash
git add app/Admin/src/features/catalog/repository/taxon.repository.ts
git add app/Admin/src/features/catalog/repository/taxonomy.repository.ts
git add app/Admin/src/features/catalog/taxonomies/tests/taxonomy.schema.spec.ts
git add app/Admin/src/features/catalog/taxonomies/taxa/tests/taxon.schema.spec.ts
git commit -m "refactor(admin): delete old taxonomy taxon type files update repository imports"
```

---

### Task 7: Update mapper and remaining import sites

**Files:**
- Modify: `app/Admin/src/features/catalog/mapper/catalog.mapper.ts`
- Verify: `app/Admin/src/features/catalog/catalog.routes.ts`

- [ ] **Step 1: Update `catalog.mapper.ts` imports**

```typescript
// app/Admin/src/features/catalog/mapper/catalog.mapper.ts
// BEFORE:
// import type { TaxonomyDetail, TaxonomyListItem } from '../taxonomies/types/taxonomy.domain.types'
// import type { TaxonDetail, TaxonListItem, TaxonTreeItem } from '../taxonomies/taxa/types/taxon.domain.types'

// AFTER:
import type { TaxonomyDetail, TaxonomyListItem } from '../taxonomies/types/Taxonomy.Response.Type'
import type { TaxonDetail, TaxonListItem, TaxonTreeItem } from '../taxonomies/taxa/types/Taxon.Response.Type'
```

- [ ] **Step 2: Verify `catalog.routes.ts` has no stale imports**

Run: `rg "taxonomies/(types|schemas)" app/Admin/src/features/catalog/catalog.routes.ts`
Expected: no matches

- [ ] **Step 3: Typecheck**

Run: `pnpm run typecheck`
Expected: PASS

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/catalog/mapper/catalog.mapper.ts
git commit -m "refactor(admin): update mapper imports to new type file paths"
```

---

### Task 8: Full verification — typecheck, lint, test

**Files:** none changed — pure verification

- [ ] **Step 1: Typecheck**

Run: `pnpm run typecheck`
Expected: PASS (zero errors)

- [ ] **Step 2: Lint**

Run: `pnpm run lint`
Expected: PASS (zero warnings — `TreatWarningsAsErrors` equivalent for the TS/JS codebase)

- [ ] **Step 3: Full unit test suite**

Run: `pnpm run test:unit`
Expected: PASS (all tests, including taxonomy schema spec and taxon schema spec)

- [ ] **Step 4: Verify all old files are gone**

Run:
```bash
ls app/Admin/src/features/catalog/taxonomies/schemas/
ls app/Admin/src/features/catalog/taxonomies/types/
ls app/Admin/src/features/catalog/taxonomies/taxa/schemas/
ls app/Admin/src/features/catalog/taxonomies/taxa/types/
```

Expected output:
```
schemas/: Taxonomy.Schema.ts  Taxon.Schema.ts  TaxonRule.Schema.ts
types/:   Taxonomy.Parameters.Type.ts  Taxonomy.Request.Type.ts  Taxonomy.Response.Type.ts  Taxonomy.Query.Type.ts
taxa/schemas/:   (directory exists but empty — schemas moved to parent)
taxa/types/:     Taxon.Parameters.Type.ts  Taxon.Request.Type.ts  Taxon.Response.Type.ts  Taxon.Query.Type.ts
                 TaxonRule.Parameters.Type.ts  TaxonRule.Request.Type.ts  TaxonRule.Response.Type.ts
```

- [ ] **Step 5: Commit**

```bash
git commit --allow-empty -m "chore(admin): verify type-schema restructure pilot for taxonomies"
```

---

## 3. Alternatives

- **ALT-001**: Keep Taxa `schemas/` directory and place `Taxon.Schema.ts` and `TaxonRule.Schema.ts` inside it (under `taxa/schemas/`) instead of the parent `taxonomies/schemas/`. Rejected because the schemas are used by taxa views, and taxa is the canonical entity — putting schemas at the parent level implies they belong to the Taxonomy entity, which is incorrect.
- **ALT-002**: Keep `*.domain.types.ts` naming instead of `*.Response.Type.ts` — rejected because "domain" is ambiguous (it could mean domain model vs response model) and doesn't align with the Parameters/Request/Response/Query quadrant.
- **ALT-003**: Collapse `TaxonRule` type files into `Taxon.*.Type.ts` to reduce file count — rejected because the user explicitly requires one schema per entity (`TaxonRule.Schema.ts`), and having a separate entity with its own files is consistent even for sub-entities.

## 4. Dependencies

- **DEP-001**: Zod must be at `^3.22` or higher (already in `app/Admin/package.json`)
- **DEP-002**: `ServerQueryingParameters` must be exported from `@/shared/api/types/query.types` (verified — it is)
- **DEP-003**: `pnpm run typecheck` script must exist in `app/Admin/package.json` (uses `vue-tsc --noEmit` + `tsc --noEmit`)
- **DEP-004**: `pnpm run test:unit` script must exist (uses `vitest run`)

## 5. Testing

- **TEST-001**: `taxonomy.schema.spec.ts` — schema validation tests for TaxonomySchema (import path update only, no behavioral change)
- **TEST-002**: `taxon.schema.spec.ts` — schema validation tests for TaxonSchema and TaxonRuleSchema (import path update only)
- **TEST-003**: TypeScript compilation — all new `.Type.ts` files must compile without errors
- **TEST-004**: `pnpm run lint` — zero warnings across the modified files

## 6. Risks & Assumptions

- **RISK-001**: Vee-validate form bindings in `TaxonForm.View.vue` use `defineField('fieldName')` which references schema field names — these are unchanged since the Zod schema object shape is preserved identically
- **ASSUMPTION-001**: No Vue SFC imports type files directly in their `<script>` blocks (confirmed — `TaxonForm.View.vue` imports from service, which imports from repository, which imports from types; no direct import of `types/taxon.*`)
- **ASSUMPTION-002**: The `taxonomies/taxa/tests/` directory exists for the temp schema-typecheck spec (verified — it contains `taxon.schema.spec.ts`)

## 7. Related Specifications / Further Reading

- `plan/refactor-admin-type-schema-1.md` — full-scope plan (all 18 entities) that this pilot implements for the Taxonomies module
- `docs/codebase/CONVENTIONS.md` — Admin SPA coding conventions
- `app/Admin/src/features/catalog/taxonomies/schemas/taxon.schema.ts` — original combined schema file (will be deleted)
