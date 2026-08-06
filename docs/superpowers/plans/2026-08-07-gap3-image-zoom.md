# Gap 3: Image Zoom / Lightbox Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace raw `<img>` in ProductGallery with PrimeVue Image component for built-in zoom, lightbox, and keyboard navigation.

**Architecture:** Single component swap. PrimeVue `Image` with `preview="true"` replaces the raw `<img>` tag. Zero custom code — all interactions handled by PrimeVue.

**Tech Stack:** Vue 3, PrimeVue 5 `Image` component (auto-imported via PrimeVueResolver)

## Global Constraints

- Warnings-as-errors: any TypeScript/lint warning fails build
- PrimeVue components auto-import via `unplugin-vue-components` + `PrimeVueResolver`
- No new dependencies — PrimeVue Image is already available in primevue v5

---

## File Structure

| File | Action | Purpose |
|------|--------|---------|
| `app/Store/src/features/catalog/components/ProductGallery.vue` | MODIFY | Replace `<img>` with PrimeVue `<Image>` |

---

## Tasks

### Task 1: Replace img with PrimeVue Image

**Files:**
- Modify: `app/Store/src/features/catalog/components/ProductGallery.vue:17-27`

**Interfaces:**
- Consumes: `images: StoreProductImageResponse[]`, `alt: string` (existing props)
- Produces: No new exports — component interface unchanged

- [ ] **Step 1: Read the current ProductGallery.vue**

Verify the file at `app/Store/src/features/catalog/components/ProductGallery.vue` has the raw `<img>` tag on lines 18-23.

- [ ] **Step 2: Replace the main image `<img>` with PrimeVue `<Image>`**

Edit `app/Store/src/features/catalog/components/ProductGallery.vue`. Replace lines 17-27:

```vue
    <!-- Section: Main Image -->
    <div class="aspect-square bg-stone-100 rounded-xl overflow-hidden">
      <img
        v-if="images.length > 0"
        :src="images[activeIndex]?.url"
        :alt="images[activeIndex]?.alt ?? alt"
        class="w-full h-full object-cover"
      />
      <div v-else class="w-full h-full flex items-center justify-center text-stone-400">
        <i class="pi pi-image text-6xl" />
      </div>
    </div>
```

With:

```vue
    <!-- Section: Main Image -->
    <div class="aspect-square bg-stone-100 rounded-xl overflow-hidden">
      <Image
        v-if="images.length > 0"
        :src="images[activeIndex]?.url"
        :alt="images[activeIndex]?.alt ?? alt"
        :preview="true"
        image-class="w-full h-full object-cover"
        class="w-full h-full"
      />
      <div v-else class="w-full h-full flex items-center justify-center text-stone-400">
        <i class="pi pi-image text-6xl" />
      </div>
    </div>
```

- [ ] **Step 3: Verify no import needed**

PrimeVue `Image` auto-imports via `PrimeVueResolver`. No `<script>` change needed.

- [ ] **Step 4: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS (no new warnings)

- [ ] **Step 5: Run unit tests**

```bash
cd app/Store && pnpm run test:unit
```

Expected: PASS

- [ ] **Step 6: Commit**

```bash
cd app/Store && git add src/features/catalog/components/ProductGallery.vue
git commit -m "feat(catalog): add image zoom and lightbox via PrimeVue Image"
```
