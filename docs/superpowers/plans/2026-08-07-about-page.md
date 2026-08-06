# About Page Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add static About page to the Storefront. Pure marketing content, no backend.

**Architecture:** New view component with static content. Route added to catalog routes. Footer link added.

**Tech Stack:** Vue 3, PrimeVue 5, Tailwind CSS v4

## Global Constraints

- Warnings-as-errors: any TypeScript/lint warning fails build
- Follow existing page patterns (HeroSection, FeaturesStrip)
- Use PrimeVue Card for content sections
- Use Playfair Display font for headings (already configured)

---

## File Structure

| File | Action | Purpose |
|------|--------|---------|
| `features/catalog/views/AboutView.vue` | CREATE | Static about page |
| `features/catalog/routes/index.ts` | MODIFY | Add /about route |
| `app/components/layout/AppFooter.vue` | MODIFY | Add About link |

---

## Tasks

### Task 1: Create AboutView.vue

**Files:**
- Create: `app/Store/src/features/catalog/views/AboutView.vue`

**Interfaces:**
- Consumes: None (static content)
- Produces: No exports — page component only

- [ ] **Step 1: Create the view**

Create `app/Store/src/features/catalog/views/AboutView.vue`:

```vue
<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()
</script>
<template>
  <!-- Section: About Page -->
  <div>
    <!-- Section: Hero -->
    <section class="bg-gradient-to-br from-teal-800 via-teal-700 to-teal-900 text-white py-24">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
        <h1 class="text-4xl md:text-6xl font-bold font-['Playfair_Display'] mb-4">About ReSys</h1>
        <p class="text-xl text-teal-100 max-w-2xl mx-auto">Fashion meets technology</p>
      </div>
    </section>

    <!-- Section: Our Story -->
    <section class="py-16">
      <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
        <h2 class="text-3xl font-bold text-stone-900 font-['Playfair_Display'] mb-6">Our Story</h2>
        <p class="text-stone-600 leading-relaxed mb-4">
          ReSys was born from a simple idea: make fashion discovery effortless. We combine cutting-edge AI technology with curated fashion to help you find exactly what you're looking for.
        </p>
        <p class="text-stone-600 leading-relaxed">
          Our visual search technology lets you upload any fashion image and instantly find similar products from our collection. No more endless scrolling — just upload, discover, and shop.
        </p>
      </div>
    </section>

    <!-- Section: Our Mission -->
    <section class="py-16 bg-stone-50">
      <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
        <h2 class="text-3xl font-bold text-stone-900 font-['Playfair_Display'] mb-6">Our Mission</h2>
        <p class="text-stone-600 leading-relaxed">
          We believe technology should serve style, not replace it. Our mission is to bridge the gap between inspiration and purchase, making every fashion find just one upload away.
        </p>
      </div>
    </section>

    <!-- Section: Values -->
    <section class="py-16">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <h2 class="text-3xl font-bold text-stone-900 font-['Playfair_Display'] mb-8 text-center">Our Values</h2>
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <div class="bg-white rounded-xl border border-stone-200 p-6 text-center">
            <i class="pi pi-star text-3xl text-teal-600 mb-4" />
            <h3 class="font-semibold text-stone-900 mb-2">Quality</h3>
            <p class="text-sm text-stone-600">Every product meets our high standards</p>
          </div>
          <div class="bg-white rounded-xl border border-stone-200 p-6 text-center">
            <i class="pi pi-heart text-3xl text-teal-600 mb-4" />
            <h3 class="font-semibold text-stone-900 mb-2">Sustainability</h3>
            <p class="text-sm text-stone-600">Responsible fashion for a better future</p>
          </div>
          <div class="bg-white rounded-xl border border-stone-200 p-6 text-center">
            <i class="pi pi-bolt text-3xl text-teal-600 mb-4" />
            <h3 class="font-semibold text-stone-900 mb-2">Innovation</h3>
            <p class="text-sm text-stone-600">AI-powered discovery for effortless shopping</p>
          </div>
          <div class="bg-white rounded-xl border border-stone-200 p-6 text-center">
            <i class="pi pi-users text-3xl text-teal-600 mb-4" />
            <h3 class="font-semibold text-stone-900 mb-2">Customer First</h3>
            <p class="text-sm text-stone-600">Your satisfaction drives everything we do</p>
          </div>
        </div>
      </div>
    </section>

    <!-- Section: CTA -->
    <section class="py-16 bg-stone-900 text-white">
      <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
        <h2 class="text-3xl font-bold font-['Playfair_Display'] mb-4">Ready to Discover?</h2>
        <p class="text-stone-300 mb-8">Explore our collection and find your next favorite piece.</p>
        <Button label="Shop Now" icon="pi pi-arrow-right" iconPos="right" size="large" @click="router.push('/shop')" />
      </div>
    </section>
  </div>
</template>
```

- [ ] **Step 2: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 3: Commit**

```bash
cd app/Store && git add src/features/catalog/views/AboutView.vue
git commit -m "feat(catalog): create About page"
```

### Task 2: Add route and footer link

**Files:**
- Modify: `app/Store/src/features/catalog/routes/index.ts`
- Modify: `app/Store/src/app/components/layout/AppFooter.vue`

**Interfaces:**
- Consumes: None
- Produces: Route + footer link

- [ ] **Step 1: Read routes/index.ts**

Read `app/Store/src/features/catalog/routes/index.ts`.

- [ ] **Step 2: Add about route**

Add to routes array:

```typescript
{
  path: 'about',
  name: 'about',
  component: () => import('../views/AboutView.vue'),
  meta: { title: 'About Us' },
},
```

- [ ] **Step 3: Read AppFooter.vue**

Read `app/Store/src/app/components/layout/AppFooter.vue`. Find the Company links section.

- [ ] **Step 4: Add About link**

In the Company column, add:

```vue
<li><router-link to="/about" class="text-stone-400 hover:text-white transition-colors">About Us</router-link></li>
```

- [ ] **Step 5: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 6: Commit**

```bash
cd app/Store && git add src/features/catalog/routes/index.ts src/app/components/layout/AppFooter.vue
git commit -m "feat(catalog): add About route and footer link"
```
