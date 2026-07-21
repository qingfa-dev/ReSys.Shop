# Admin SPA Catalog Module Refactor — Design Spec

**Date:** 2026-07-21
**Scope:** Catalog module (option-types, option-values, products, variants, taxonomies, and all sub-entities)
**Context:** Refactoring the Admin SPA to mirror backend patterns: per-field validation, parameter models, explicit mapping, flattened entity structure. Catalog is the pilot; all 8 modules follow this pattern afterward.

---

## §G — Goal

- Per-field Zod validation schemas mirroring C# domain validation (`ApplyNameRules`, etc.)
- Parameter/Request/Response model interfaces separated from validation
- Explicit API response mappers (not type casting)
- Flattened entity folders (no nested parent-child directories)
- Every entity gets 9 standard slots: `pages/`, `components/`, `api/`, `store/`, `composables/`, `types/`, `models/`, `routes.ts`, `index.ts`

---

## §I — Invariants

1. **No `as Type` casts** — all API responses flow through explicit mapper functions
2. **Types/ validation only** — `types/` contains Zod schemas and query types; never data interfaces
3. **Models/ data interfaces only** — `models/` contains parameter/request/response interfaces; never validation logic
4. **Per-field schemas are reusable** — `nameSchema`, `presentationSchema`, `positionSchema` exported independently so other entities can compose them
5. **Module route file aggregates, entity route file defines** — `catalog.routes.ts` imports from each entity's `routes.ts`
6. **Child entities are flattened** — e.g., `option-values/` at catalog level, not nested inside `option-types/`

---

## §T — Tasks

### T.1 Define per-field validation schemas for each entity

Each entity defines its own per-field Zod schemas in its `types/` folder. Common fields (name, presentation, position) are defined per-entity since constraints and i18n keys may differ (just as backend constants differ between `OptionTypeConstant.Constraints` and `OptionValueConstant.Constraints`).

**Success:** Each entity's field schemas are independently exported and composable into a full form schema.

### T.2 Refactor option-types entity

- **types/**: Per-field schemas (`nameSchema`, `presentationSchema`, `positionSchema`, `filterableSchema`) + composed `createOptionTypeSchema`
- **models/**: `OptionTypeParameters`, `CreateOptionTypeRequest`, `UpdateOptionTypeRequest`, `OptionTypeListItem`, `OptionTypeDetail`
- **api/**: `option-type.api.ts` (existing, update imports), `option-type.mapper.ts` (new — `mapToListItem`, `mapToDetail`)
- **store/**: Wire mapper into store — no `as` casts
- **routes.ts**: Extract from `catalog.routes.ts`
- **index.ts**: Updated barrel exports

### T.3 Flatten option-values out of option-types/

Move `option-types/option-values/` → `option-values/` at catalog level.

- **types/**: Per-field schemas + query types
- **models/**: `OptionValueParameters`, `CreateOptionValueRequest`, `UpdateOptionValueRequest`, `OptionValueListItem`
- **api/**: `option-value.api.ts` + `option-value.mapper.ts`
- **store/**: Wire mapper; keep `optionTypeId` as param, not folder-derived
- **routes.ts**: Extract routes, still nested under option types URL path

### T.4 Refactor products entity

- **types/**: Per-field schemas for product fields
- **models/**: Product parameter/request/response interfaces
- **api/**: `product.api.ts` (existing) + `product.mapper.ts` (new)
- **store/**: Wire mapper
- **routes.ts**: Extract from `catalog.routes.ts`

### T.5 Flatten product sub-entities

Move out of `products/`:
- `products/classifications/` → `classifications/`
- `products/option-types/` → `product-option-types/`
- `products/variants/` → `variants/`
- `products/variants/images/` → `variant-images/`
- `products/variants/prices/` → `variant-prices/`

Each gets the full 9-slot structure with mapper, per-field schemas, models, routes.

### T.6 Refactor taxonomies + flatten taxa

- `taxonomies/` — per-field schemas, models, mapper, routes
- `taxonomies/taxa/` → `taxa/` at catalog level

### T.7 Update catalog.routes.ts

Aggregate all entity route files. Remove inline route definitions.

### T.8 Run lint + tests after each entity

```bash
cd app/Admin && pnpm run lint && pnpm run test:unit
```

### T.9 Update other modules (future phase)

Inventory, Ordering, Payment, Shipping, Location, Users, Profile — same pattern applied per module in subsequent phases.

---

## §C — Constraints

- **Backend endpoints do not change** — API routes, request/response shapes remain identical
- **Existing tests must stay green** — store tests and schema tests updated to new imports only
- **Pinia store IDs unchanged** — `'option-type'`, `'option-value'`, etc. stay the same
- **i18n keys unchanged** — translation keys in field schemas preserved

---

## §V — Verification

1. `cd app/Admin && pnpm run lint` passes with zero warnings
2. `cd app/Admin && pnpm run test:unit` passes — existing option-type, option-value tests updated and green
3. Manual smoke test: navigate to option types list → create → edit → delete
4. Manual smoke test: navigate to option values for a type → CRUD
5. Verify no `as Type` casts exist in any new mapper-wired store
6. Verify `types/` contains only Zod schemas and query types (no data interfaces)
7. Verify `models/` contains only data interfaces (no Zod)

---

## Design Details

### Folder Structure (flat)

```
features/catalog/
├── catalog.routes.ts
├── option-types/                # was catalog/option-types/
│   ├── api/    (option-type.api.ts, option-type.mapper.ts)
│   ├── components/              (reusable UI pieces)
│   ├── composables/             (.gitkeep if empty)
│   ├── index.ts
│   ├── models/  (option-type.parameters.ts, .request.ts, .response.ts)
│   ├── pages/   (*Page.vue)
│   ├── routes.ts
│   ├── store/   (option-type.store.ts)
│   └── types/   (option-type.field.ts, .query.ts)
├── option-values/               # was catalog/option-types/option-values/
│   ├── api/    (option-value.api.ts, option-value.mapper.ts)
│   ├── components/
│   ├── composables/
│   ├── index.ts
│   ├── models/  (option-value.parameters.ts, .request.ts, .response.ts)
│   ├── pages/
│   ├── routes.ts
│   ├── store/   (option-value.store.ts)
│   └── types/   (option-value.field.ts, .query.ts)
├── products/
│   ├── api/    (product.api.ts, product.mapper.ts)
│   ├── components/
│   ├── composables/
│   ├── index.ts
│   ├── models/  (product.parameters.ts, .request.ts, .response.ts)
│   ├── pages/
│   ├── routes.ts
│   ├── store/   (product.store.ts)
│   └── types/   (product.field.ts, .query.ts)
├── variants/                    # was products/variants/
│   └── (9 slots)
├── variant-images/              # was products/variants/images/
│   └── (9 slots)
├── variant-prices/              # was products/variants/prices/
│   └── (9 slots)
├── classifications/             # was products/classifications/
│   └── (9 slots)
├── product-option-types/        # was products/option-types/
│   └── (9 slots)
├── taxonomies/
│   └── (9 slots)
├── taxa/                        # was taxonomies/taxa/
│   └── (9 slots)
└── dashboard/
    └── (9 slots)
```

### Per-Field Validation Pattern

```typescript
// types/option-type.field.ts — each field is an independent exported schema
export function nameSchema(t: TFunc)    { return z.string().min(1)...max(100) }
export function presentationSchema(t: TFunc) { return z.string().min(1)...max(100) }
export function positionSchema(t: TFunc) { return z.number().int().min(0).default(0) }
export function filterableSchema()     { return z.boolean().default(false) }

// Composed via z.object (mirrors OptionTypeParametersValidator):
export function createOptionTypeSchema(t: TFunc) {
  return z.object({ name: nameSchema(t), presentation: presentationSchema(t), ... })
}
```

### Mapping Pattern

```typescript
// api/option-type.mapper.ts — explicit field-by-field, no as casts
export function mapToListItem(dto: Record<string, unknown>): OptionTypeListItem {
  return {
    id: String(dto.id ?? ''),
    name: String(dto.name ?? ''),
    // ... every field explicitly mapped
  }
}
```

### Store → Mapper Integration

```typescript
// store/option-type.store.ts — store calls mapper after repository returns
async (p) => {
  const result = await optionTypeRepository.list(p)
  return { ...result, items: result.items?.map(mapToListItem) ?? [] }
}
```

### Routing

- Entity `routes.ts` defines routes for that entity
- `catalog.routes.ts` imports and spreads all entity routes
- URL paths unchanged (e.g., option-values still at `:optionTypeId/values`)
