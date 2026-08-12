# Visual Search UI — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add model selection, search tuning sliders, image preview, and action buttons to the Visual Search page.

**Architecture:** Extend `useVisualSearch` composable with search parameter refs and a `filteredResults` computed. Rebuild `VisualSearchView.vue` with two-card layout (upload + parameters) using PrimeVue auto-imported components.

**Tech Stack:** Vue 3 Composition API, PrimeVue 5.0.0 (auto-imported), TypeScript

## Global Constraints

- PrimeVue components are auto-imported via `PrimeVueResolver()` — no explicit imports needed except `Label`
- `Label` must be imported explicitly: `import Label from 'primevue/label'`
- Comments follow Store AGENTS.md standard: `// Label: Sentence.` in script, `<!-- Section: Title — purpose -->` in template
- `TreatWarningsAsErrors=true` — no warnings allowed

---

## File Map

| File | Change |
|------|--------|
| `app/Store/src/features/catalog/composables/useVisualSearch.ts` | Add `topK`, `minSimilarity`, `scoreWeight` refs; add `filteredResults` computed |
| `app/Store/src/features/catalog/views/VisualSearchView.vue` | Rebuild with two-card layout, parameter controls, image preview, actions |

---

### Task 1: Extend the composable with search parameter state

**Files:**
- Modify: `app/Store/src/features/catalog/composables/useVisualSearch.ts`
- Test: `app/Store/src/features/catalog/composables/__tests__/useVisualSearch.spec.ts`

**Interfaces:**
- Consumes: existing `useVisualSearch()` return shape
- Produces: new refs `topK`, `minSimilarity`, `scoreWeight`; new computed `filteredResults`

- [ ] **Step 1: Add search parameter refs**

After the existing `validationError` ref (line 21), add:

```typescript
const topK = ref(20)
const minSimilarity = ref(0)
const scoreWeight = ref(1.0)
```

- [ ] **Step 2: Add filteredResults computed**

After the `search` function (after line 70), add:

```typescript
import { computed } from 'vue'
```

(Add `computed` to the existing import from `'vue'` on line 1.)

Then add the computed:

```typescript
const filteredResults = computed(() => {
  const weight = scoreWeight.value
  const threshold = minSimilarity.value / 100
  return results.value
    .map(item => ({
      ...item,
      adjustedScore: Math.min(item.similarityScore * weight, 1),
    }))
    .filter(item => item.adjustedScore >= threshold)
})
```

- [ ] **Step 3: Update the reactive return**

Replace the return block (lines 88-92) with:

```typescript
return reactive({
  state, selectedFile, previewUrl, selectedModelId, availableModels, results,
  topK, minSimilarity, scoreWeight, filteredResults,
  loading, error, validationError,
  validateFile, selectFile, search, loadModels, reset,
})
```

- [ ] **Step 4: Reset search params in reset()**

Inside the `reset()` function (line 77-86), add before `state.value = 'empty'`:

```typescript
topK.value = 20
minSimilarity.value = 0
scoreWeight.value = 1.0
```

- [ ] **Step 5: Run unit tests**

Run: `cd app/Store && pnpm run test:unit -- --run composables/__tests__/useVisualSearch`
Expected: PASS (existing tests should still pass)

- [ ] **Step 6: Commit**

```bash
git add app/Store/src/features/catalog/composables/useVisualSearch.ts
git commit -m "feat(catalog): add search param state and filteredResults to useVisualSearch"
```

---

### Task 2: Rebuild VisualSearchView.vue with two-card layout

**Files:**
- Modify: `app/Store/src/features/catalog/views/VisualSearchView.vue`

**Interfaces:**
- Consumes: `useVisualSearch()` with new `topK`, `minSimilarity`, `scoreWeight`, `filteredResults`, `loadModels()`
- Produces: complete visual search page UI

- [ ] **Step 1: Update script setup imports**

Replace the entire `<script setup>` block with:

```vue
<script setup lang="ts">
import { onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useVisualSearch } from '../composables/useVisualSearch'
import type { FileUploadSelectEvent } from 'primevue/fileupload'
import ProductGridCard from '../components/ProductGridCard.vue'

usePageTitle('Visual Search')

const vs = useVisualSearch()

// Load: Fetch available ML models on mount
onMounted(() => { vs.loadModels() })

// Upload: Route the chosen file into the composable
function onSelect(event: FileUploadSelectEvent): void {
  const file = event.files[0] as File | undefined
  if (!file) return
  vs.selectFile(file)
  if (!vs.validationError) void vs.search(vs.topK)
}

// Search: Re-run search with current parameters
function onSearch(): void {
  if (!vs.selectedFile) return
  void vs.search(vs.topK)
}
</script>
```

- [ ] **Step 2: Replace template with two-card layout**

Replace the entire `<template>` block with:

```vue
<template>
  <div class="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
    <!-- Section: Page Header — breadcrumb, heading and subtitle -->
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Visual Search' }]" class="mb-6" />
    <h1 class="mb-2 text-2xl font-semibold tracking-tight text-heading">
      Visual Search
    </h1>
    <p class="mb-8 text-sm text-muted">
      Find visually similar products by uploading an image
    </p>

    <!-- Section: Upload Panel — file picker with image preview and clear action -->
    <Card class="mb-6">
      <template #content>
        <div class="flex flex-col items-center gap-4 py-4">
          <div v-if="vs.previewUrl" class="relative">
            <img
              :src="vs.previewUrl"
              alt="Preview"
              class="max-h-48 rounded-lg border border-border object-contain"
            />
            <Button
              icon="pi pi-times"
              rounded
              size="small"
              severity="danger"
              class="absolute -top-2 -right-2"
              @click="vs.reset()"
              aria-label="Clear image"
            />
          </div>
          <div v-else class="flex flex-col items-center gap-3">
            <FileUpload
              mode="basic"
              accept="image/*"
              chooseLabel="Choose an image"
              :auto="false"
              :customUpload="true"
              @select="onSelect"
            />
            <p class="text-sm text-muted">JPEG, PNG, WebP — max 10 MB</p>
          </div>
        </div>
      </template>
    </Card>

    <!-- Section: Search Parameters — model, result count, threshold, and weight controls -->
    <Card v-if="vs.selectedFile" class="mb-6">
      <template #content>
        <div class="grid gap-6 sm:grid-cols-2">
          <div class="flex flex-col gap-2">
            <Label for="model">Model</Label>
            <Select
              id="model"
              v-model="vs.selectedModelId"
              :options="vs.availableModels"
              optionLabel="name"
              optionValue="id"
              placeholder="Select model"
              class="w-full"
            />
          </div>

          <div class="flex flex-col gap-2">
            <Label for="topK">Results</Label>
            <div class="flex items-center gap-3">
              <Slider
                id="topK"
                v-model="vs.topK"
                :min="1"
                :max="50"
                class="flex-1"
              />
              <InputNumber
                v-model="vs.topK"
                :min="1"
                :max="50"
                showButtons
                buttonLayout="horizontal"
                :inputStyle="{ width: '3rem', textAlign: 'center' }"
              />
            </div>
          </div>

          <div class="flex flex-col gap-2">
            <Label for="threshold">Min Match %</Label>
            <div class="flex items-center gap-3">
              <Slider
                id="threshold"
                v-model="vs.minSimilarity"
                :min="0"
                :max="100"
                class="flex-1"
              />
              <InputNumber
                v-model="vs.minSimilarity"
                :min="0"
                :max="100"
                suffix="%"
                :inputStyle="{ width: '4rem', textAlign: 'center' }"
              />
            </div>
          </div>

          <div class="flex flex-col gap-2">
            <Label for="weight">Score Weight</Label>
            <div class="flex items-center gap-3">
              <Slider
                id="weight"
                v-model="vs.scoreWeight"
                :min="0.1"
                :max="3"
                :step="0.1"
                class="flex-1"
              />
              <InputNumber
                v-model="vs.scoreWeight"
                :min="0.1"
                :max="3"
                :step="0.1"
                :inputStyle="{ width: '4rem', textAlign: 'center' }"
              />
            </div>
          </div>
        </div>

        <!-- Section: Parameter Actions — search and reset buttons -->
        <div class="mt-6 flex gap-3">
          <Button
            label="Search"
            icon="pi pi-search"
            :loading="vs.loading"
            @click="onSearch"
          />
          <Button
            label="Reset"
            icon="pi pi-refresh"
            severity="secondary"
            @click="vs.reset()"
          />
        </div>
      </template>
    </Card>

    <!-- Section: Validation Error — file rejected by the store -->
    <Message v-if="vs.validationError" severity="error" :closable="false" class="mb-6">
      {{ vs.validationError }}
    </Message>

    <!-- Section: Loading State — spinner while the image is embedded -->
    <div v-if="vs.state === 'loading'" class="flex flex-col items-center gap-4 py-16">
      <ProgressSpinner style="width: 3rem; height: 3rem" :strokeWidth="4" />
      <p class="text-sm text-muted">
        Embedding image and searching the catalog…
      </p>
    </div>

    <!-- Section: Results Grid — visually similar products with match tags -->
    <div v-if="vs.state === 'results'">
      <h2 class="mb-6 text-sm font-medium uppercase tracking-wide text-muted">
        Results ({{ vs.filteredResults.length }})
      </h2>
      <div class="grid grid-cols-2 gap-6 lg:grid-cols-4">
        <ProductGridCard
          v-for="item in vs.filteredResults"
          :key="item.id"
          :product="item"
          :show-similarity="true"
          :similarity-score="item.adjustedScore"
        />
      </div>
      <p v-if="vs.filteredResults.length === 0" class="py-12 text-center text-sm text-muted">
        No results above {{ vs.minSimilarity }}% similarity threshold.
      </p>
    </div>

    <!-- Section: Error State — search failure message -->
    <Message v-if="vs.error" severity="error" :closable="false" class="mt-6">
      {{ vs.error }}
    </Message>
  </div>
</template>
```

- [ ] **Step 3: Run build**

Run: `cd app/Store && pnpm run build-only`
Expected: PASS, no errors

- [ ] **Step 4: Run lint**

Run: `cd app/Store && pnpm run lint`
Expected: PASS, no errors

- [ ] **Step 5: Commit**

```bash
git add app/Store/src/features/catalog/views/VisualSearchView.vue
git commit -m "feat(catalog): add search parameters and image actions to visual search UI"
```

---

### Task 3: Final verification

**Files:** None (verification only)

- [ ] **Step 1: Full build**

Run: `cd app/Store && pnpm run build-only`
Expected: PASS

- [ ] **Step 2: Full lint**

Run: `cd app/Store && pnpm run lint`
Expected: PASS

- [ ] **Step 3: Run all unit tests**

Run: `cd app/Store && pnpm run test:unit -- --run`
Expected: PASS

- [ ] **Step 4: Verify no regressions in composable tests**

Run: `cd app/Store && pnpm run test:unit -- --run composables/__tests__/useVisualSearch`
Expected: PASS
