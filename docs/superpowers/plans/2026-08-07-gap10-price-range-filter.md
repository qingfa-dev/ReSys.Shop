# Gap 10: Price Range Filter Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a price range filter (slider + min/max inputs) to the existing FilterSidebar.

**Architecture:** New `FilterPriceRange` component using PrimeVue Slider (range mode) + InputNumber fields. Added to FilterSidebar below option type filters. Store already has `minPrice`/`maxPrice` state.

**Tech Stack:** Vue 3, PrimeVue 5 Slider + InputNumber, Pinia

## Global Constraints

- Warnings-as-errors: any TypeScript/lint warning fails build
- PrimeVue components auto-import via `PrimeVueResolver`
- Store actions: `setPriceRange(min, max)` and `clearFilters()` already exist
- Backend already accepts `MinPrice`/`MaxPrice` query parameters

---

## File Structure

| File | Action | Purpose |
|------|--------|---------|
| `app/Store/src/features/catalog/components/FilterPriceRange.vue` | CREATE | Slider + min/max inputs |
| `app/Store/src/features/catalog/components/FilterSidebar.vue` | MODIFY | Add price range section |
| `app/Store/src/features/catalog/stores/catalogStore.ts` | READ | Verify `setPriceRange` exists |

---

## Tasks

### Task 1: Verify store has price range state

**Files:**
- Read: `app/Store/src/features/catalog/stores/catalogStore.ts`

**Interfaces:**
- Consumes: `minPrice`, `maxPrice` refs, `setPriceRange(min, max)` action
- Produces: No changes — verification only

- [ ] **Step 1: Read catalogStore.ts**

Verify `minPrice` and `maxPrice` refs exist (lines 8-9), `setPriceRange` action exists (lines 35-38), and `clearFilters` resets both to null (lines 40-46).

### Task 2: Create FilterPriceRange.vue

**Files:**
- Create: `app/Store/src/features/catalog/components/FilterPriceRange.vue`

**Interfaces:**
- Consumes: `min`, `max` (bounds), `modelValue` (current range)
- Produces: `update:modelValue` emit with `{ min: number | null, max: number | null }`

- [ ] **Step 1: Create the component**

Create `app/Store/src/features/catalog/components/FilterPriceRange.vue`:

```vue
<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  min?: number
  max?: number
  modelValue: { min: number | null; max: number | null }
}>(), {
  min: 0,
  max: 1_000_000,
})

const emit = defineEmits<{ 'update:modelValue': [value: { min: number | null; max: number | null }] }>()

const range = computed({
  get: () => [props.modelValue.min ?? props.min, props.modelValue.max ?? props.max] as [number, number],
  set: ([lo, hi]: [number, number]) => {
    emit('update:modelValue', {
      min: lo > props.min ? lo : null,
      max: hi < props.max ? hi : null,
    })
  },
})

const localMin = computed({
  get: () => props.modelValue.min ?? props.min,
  set: (v: number | null) => {
    const hi = props.modelValue.max ?? props.max
    emit('update:modelValue', { min: v && v > props.min ? v : null, max: hi })
  },
})

const localMax = computed({
  get: () => props.modelValue.max ?? props.max,
  set: (v: number | null) => {
    const lo = props.modelValue.min ?? props.min
    emit('update:modelValue', { min: lo, max: v && v < props.max ? v : null })
  },
})
</script>
<template>
  <!-- Section: Price Range Filter -->
  <section class="space-y-3">
    <h3 class="text-sm font-semibold text-stone-900">Price Range</h3>
    <Slider v-model="range" :min="min" :max="max" :step="10_000" range class="w-full" />
    <div class="flex gap-2">
      <InputNumber
        v-model="localMin"
        :min="min"
        :max="max"
        :step="10_000"
        mode="currency"
        currency="VND"
        locale="vi-VN"
        fluid
        size="small"
      />
      <InputNumber
        v-model="localMax"
        :min="min"
        :max="max"
        :step="10_000"
        mode="currency"
        currency="VND"
        locale="vi-VN"
        fluid
        size="small"
      />
    </div>
  </section>
</template>
```

- [ ] **Step 2: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

### Task 3: Add price range to FilterSidebar

**Files:**
- Modify: `app/Store/src/features/catalog/components/FilterSidebar.vue:7-13,19,84`

**Interfaces:**
- Consumes: `minPrice`, `maxPrice` from catalogStore
- Produces: Calls `catalog.setPriceRange(min, max)` on change

- [ ] **Step 1: Add import for FilterPriceRange**

After line 5 (`import TaxonTreeNodes from './TaxonTreeNodes.vue'`), add:

```typescript
import FilterPriceRange from './FilterPriceRange.vue'
import { useCatalogStore } from '../stores/catalogStore'
```

- [ ] **Step 2: Add catalog store reference**

After line 13 (`const emit = ...`), add:

```typescript
const catalog = useCatalogStore()
```

- [ ] **Step 3: Add price range computed for v-model**

After line 22 (`const filterableTypes = computed(...)`), add:

```typescript
const priceRange = computed({
  get: () => ({ min: catalog.minPrice, max: catalog.maxPrice }),
  set: (v: { min: number | null; max: number | null }) => catalog.setPriceRange(v.min, v.max),
})
```

- [ ] **Step 4: Add price range section in template**

After line 84 (closing `</section>` for option type groups), before the closing `</div>`, add:

```vue
    <!-- Section: Price Range -->
    <FilterPriceRange v-model="priceRange" />
```

- [ ] **Step 5: Update hasSelection to include price**

Replace line 19:

```typescript
const hasSelection = computed(() => props.selectedTaxonIds.length > 0 || props.selectedOptionValueIds.length > 0)
```

With:

```typescript
const hasSelection = computed(() =>
  props.selectedTaxonIds.length > 0
  || props.selectedOptionValueIds.length > 0
  || catalog.minPrice != null
  || catalog.maxPrice != null
)
```

- [ ] **Step 6: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 7: Run unit tests**

```bash
cd app/Store && pnpm run test:unit
```

Expected: PASS

- [ ] **Step 8: Commit**

```bash
cd app/Store && git add src/features/catalog/components/FilterPriceRange.vue src/features/catalog/components/FilterSidebar.vue
git commit -m "feat(catalog): add price range filter to sidebar"
```
