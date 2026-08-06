# Gap 10: Price Range Filter

## Summary

Add a price range filter (slider + min/max inputs) to the existing `FilterSidebar.vue`. The store already has `minPrice`/`maxPrice` state and `setPriceRange()` action. Backend already accepts `MinPrice`/`MaxPrice` query params.

## Current State

- `catalogStore.ts`: has `minPrice`, `maxPrice` refs + `setPriceRange(min, max)` action
- `FilterSidebar.vue`: renders taxonomy tree + option type checkboxes, no price UI
- `ShopView.vue`: passes `minPrice`/`maxPrice` in URL params to API
- Backend: accepts `MinPrice`/`MaxPrice` query parameters on product listing

## Design

### New Component: `FilterPriceRange.vue`

**Location:** `app/Store/src/features/catalog/components/FilterPriceRange.vue`

**Props:**
```ts
min?: number        // default 0
max?: number        // default 1000000 (VND)
modelValue?: { min: number | null; max: number | null }
```

**Emits:**
```ts
'update:modelValue': [value: { min: number | null; max: number | null }]
```

**UI:**
```
Price Range
  $0          $5,000,000
  ├────────●────●───────┤
  [Min: 250,000] [Max: 3,500,000]
```

- PrimeVue `Slider` with `:range="true"`, step 10000
- Two PrimeVue `InputNumber` fields (Min/Max) with `mode="currency" currency="VND"`
- Price labels update reactively with slider position
- Enforce min <= max constraint

### FilterSidebar Changes

**File:** `app/Store/src/features/catalog/components/FilterSidebar.vue`

- Import `FilterPriceRange`
- Add price range section below option type filters, before "Clear All" button
- Bind to `catalog.minPrice` / `catalog.maxPrice` via v-model
- Emit `setPriceRange` on change

### catalogStore Changes

**File:** `app/Store/src/features/catalog/stores/catalogStore.ts`

- `setPriceRange` action already exists (lines 35-38) — no changes needed
- Add `clearPriceRange` action that sets both to null

### ShopView Changes

**File:** `app/Store/src/features/catalog/views/ShopView.vue`

- Already passes `minPrice`/`maxPrice` in URL params (lines 31-32) — no changes needed

## Files to Create/Modify

| File | Action |
|------|--------|
| `features/catalog/components/FilterPriceRange.vue` | CREATE |
| `features/catalog/components/FilterSidebar.vue` | MODIFY — add price range section |
| `features/catalog/stores/catalogStore.ts` | MODIFY — add `clearPriceRange` action |

## Acceptance Criteria

- [ ] Price range slider renders in filter sidebar
- [ ] Min/Max inputs sync with slider
- [ ] Price range filters products via API query params
- [ ] "Clear All" button resets price range
- [ ] Price range persists when other filters change
- [ ] Mobile responsive (slider + inputs stack vertically)
