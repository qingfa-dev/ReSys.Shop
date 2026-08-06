# Implementation Plan: Gap 10 — Price Range Filter

**Spec:** `docs/superpowers/specs/2026-08-07-gap10-price-range-filter-design.md`
**Estimated effort:** Small (1-2 hours)
**Dependencies:** None

## Tasks

### T1: Create FilterPriceRange.vue
- [ ] Create `app/Store/src/features/catalog/components/FilterPriceRange.vue`
- [ ] Props: `min`, `max`, `modelValue: { min, max }`
- [ ] Emit: `update:modelValue`
- [ ] PrimeVue Slider with `:range="true"`, step 10000
- [ ] Two PrimeVue InputNumber fields (Min/Max) with `mode="currency" currency="VND"`
- [ ] Price labels update reactively
- [ ] Enforce min <= max constraint

### T2: Add to FilterSidebar
- [ ] Edit `app/Store/src/features/catalog/components/FilterSidebar.vue`
- [ ] Import FilterPriceRange
- [ ] Add price range section below option type filters
- [ ] Bind to `catalog.minPrice` / `catalog.maxPrice` via v-model
- [ ] Emit setPriceRange on change

### T3: Add clearPriceRange to catalogStore
- [ ] Edit `app/Store/src/features/catalog/stores/catalogStore.ts`
- [ ] Add `clearPriceRange()` action that sets both to null
- [ ] Call from `clearFilters()` action

### T4: Verify
- [ ] Price range slider renders in sidebar
- [ ] Min/Max inputs sync with slider
- [ ] Products filter via API query params
- [ ] "Clear All" resets price range
- [ ] Mobile responsive

## Verification

```bash
cd app/Store && pnpm run lint && pnpm run test:unit
```
