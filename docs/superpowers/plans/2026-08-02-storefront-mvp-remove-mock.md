# Storefront MVP — Remove USE_MOCK & Enable Real API

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove all `USE_MOCK` flags from 18 Storefront services, hide 15 dropped MVP features from UI, and enable real API calls.

**Architecture:** Replace the `USE_MOCK ? mockRepo : apiRepo` ternary in every service with a direct `apiRepo` reference. No constructor injection, no fallback logic — one line change per service. Dropped features are hidden via route comment-out or `v-if="false"`, not deleted.

**Tech Stack:** Vue 3 + TypeScript + Pinia + axios (no new dependencies)

## Global Constraints

- Don't delete any `.ts` file — mock repos, types, services all stay on disk
- Don't refactor unrelated code
- Don't add new features
- Don't change the router structure or add new routes
- Run `pnpm run build` after each phase — must pass with zero errors
- Existing unit tests must still pass (26 pre-existing failures in payment-intent cancel tests are expected)
- Use `<!-- MVP: dropped — <reason> -->` comments for all UI hiding
- Use `// MVP: dropped — <reason>` comments for all route commenting

---

### Task 1: Phase 1 — Flip Catalog services (product, category, search)

**Files:**
- Modify: `app/Storefront/src/features/catalog/services/product/product.service.ts`
- Modify: `app/Storefront/src/features/catalog/services/category/category.service.ts`
- Modify: `app/Storefront/src/features/search/services/search.service.ts`

**Interfaces:**
- Consumes: `productApiRepository`, `categoryApiRepository`, `searchApiRepository` (already imported)
- Produces: Same `productService`, `categoryService`, `searchService` singletons — consumers unchanged

- [ ] **Step 1: Flip product.service.ts**

Remove the `USE_MOCK` constant and the mock import. Change the repository to use only the API repo.

```ts
// Remove line 1: import { mockProductRepository } from '../../repositories/product/product.mock.repository'
// Remove line 9: const USE_MOCK = true
// Change line 12 from:
//   private readonly productRepository = USE_MOCK ? mockProductRepository : productApiRepository
// to:
//   private readonly productRepository = productApiRepository
```

- [ ] **Step 2: Flip category.service.ts**

Same pattern as product.service.ts.

- [ ] **Step 3: Flip search.service.ts**

The search service uses a constructor parameter pattern. Change the default value from ternary to API repo only. Remove `USE_MOCK` constant and mock import.

```ts
// Remove: import { searchMockRepository } from '../repositories/search.mock.repository'
// Remove: const USE_MOCK = true
// Change constructor from:
//   constructor(private readonly repository: ISearchRepository = USE_MOCK ? searchMockRepository : searchApiRepository) {}
// to:
//   constructor(private readonly repository: ISearchRepository = searchApiRepository) {}
```

- [ ] **Step 4: Build and verify**

Run: `cd app/Storefront && pnpm run build`
Expected: Zero TypeScript errors, build succeeds.

- [ ] **Step 5: Commit**

```bash
git add app/Storefront/src/features/catalog/services/product/product.service.ts app/Storefront/src/features/catalog/services/category/category.service.ts app/Storefront/src/features/search/services/search.service.ts
git commit -m "feat(mvp): flip catalog services to real API (P1)

Remove USE_MOCK from product, category, and search services.
All three now use only their ApiRepository.

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

### Task 2: Phase 2 — Flip Identity services (auth, user)

**Files:**
- Modify: `app/Storefront/src/features/identity/services/auth/auth.service.ts`
- Modify: `app/Storefront/src/features/identity/services/user/user.service.ts`

**Interfaces:**
- Consumes: `authApiRepository`, `userApiRepository` (already imported)
- Produces: Same `authService`, `userService` singletons

- [ ] **Step 1: Flip auth.service.ts**

Remove `USE_MOCK` constant, mock import. Change `USE_MOCK ? mockAuthRepository : authApiRepository` to `authApiRepository`.

- [ ] **Step 2: Flip user.service.ts**

Same pattern.

- [ ] **Step 3: Build and verify**

Run: `cd app/Storefront && pnpm run build`
Expected: Zero errors.

- [ ] **Step 4: Commit**

```bash
git add app/Storefront/src/features/identity/services/auth/auth.service.ts app/Storefront/src/features/identity/services/user/user.service.ts
git commit -m "feat(mvp): flip identity services to real API (P2)

Remove USE_MOCK from auth and user services.

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

### Task 3: Phase 3 — Flip Cart + Order services

**Files:**
- Modify: `app/Storefront/src/features/ordering/services/cart/cart.service.ts`
- Modify: `app/Storefront/src/features/ordering/services/order/order.service.ts`

**Interfaces:**
- Consumes: `cartApiRepository`, `orderApiRepository` (already imported)
- Produces: Same `cartService`, `orderService` singletons

- [ ] **Step 1: Flip cart.service.ts**

Remove `USE_MOCK` constant (line 9), mock import (line 2). Change `USE_MOCK ? mockCartRepository : cartApiRepository` to `cartApiRepository` (line 12).

- [ ] **Step 2: Flip order.service.ts**

Same pattern.

- [ ] **Step 3: Build and verify**

Run: `cd app/Storefront && pnpm run build`
Expected: Zero errors.

- [ ] **Step 4: Commit**

```bash
git add app/Storefront/src/features/ordering/services/cart/cart.service.ts app/Storefront/src/features/ordering/services/order/order.service.ts
git commit -m "feat(mvp): flip cart and order services to real API (P3)

Remove USE_MOCK from cart and order services.

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

### Task 4: Phase 4 — Flip Payment + Shipping services

**Files:**
- Modify: `app/Storefront/src/features/payment/services/payment-intent/payment-intent.service.ts`
- Modify: `app/Storefront/src/features/ordering/services/payment-method/payment-method.service.ts`
- Modify: `app/Storefront/src/features/ordering/services/shipping-method/shipping-method.service.ts`
- Modify: `app/Storefront/src/features/shipping/services/shipping-rate/shipping-rate.service.ts`

**Interfaces:**
- Consumes: `paymentIntentApiRepository`, `paymentMethodApiRepository`, `shippingMethodApiRepository`, `shippingRateApiRepository` (already imported)
- Produces: Same singletons

- [ ] **Step 1: Flip payment-intent.service.ts**

Remove `USE_MOCK` constant and mock import. Change ternary to `paymentIntentApiRepository`.

- [ ] **Step 2: Flip payment-method.service.ts**

Same pattern.

- [ ] **Step 3: Flip shipping-method.service.ts**

Same pattern.

- [ ] **Step 4: Flip shipping-rate.service.ts**

Same pattern.

- [ ] **Step 5: Build and verify**

Run: `cd app/Storefront && pnpm run build`
Expected: Zero errors.

- [ ] **Step 6: Commit**

```bash
git add app/Storefront/src/features/payment/services/payment-intent/payment-intent.service.ts app/Storefront/src/features/ordering/services/payment-method/payment-method.service.ts app/Storefront/src/features/ordering/services/shipping-method/shipping-method.service.ts app/Storefront/src/features/shipping/services/shipping-rate/shipping-rate.service.ts
git commit -m "feat(mvp): flip payment and shipping services to real API (P4)

Remove USE_MOCK from payment-intent, payment-method, shipping-method,
and shipping-rate services.

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

### Task 5: Phase 5 — Flip Profile + Remaining services

**Files:**
- Modify: `app/Storefront/src/features/profile/services/profile.service.ts`
- Modify: `app/Storefront/src/features/ordering/services/address/address.service.ts`
- Modify: `app/Storefront/src/features/locations/services/address/address.service.ts`
- Modify: `app/Storefront/src/features/inventory/services/inventory-item/inventory-item.service.ts`
- Modify: `app/Storefront/src/features/inventory/services/stock-status/stock-status.service.ts`
- Modify: `app/Storefront/src/features/notifications/services/notifications.service.ts`
- Modify: `app/Storefront/src/features/recommendations/services/recommendations.service.ts`

**Interfaces:**
- Consumes: Respective `*ApiRepository` (already imported in each file)
- Produces: Same singletons

- [ ] **Step 1: Flip profile.service.ts**

Remove `USE_MOCK` constant and mock import. Change ternary to `profileApiRepository`.

- [ ] **Step 2: Flip ordering/address/address.service.ts**

Same pattern.

- [ ] **Step 3: Flip locations/address/address.service.ts**

Same pattern.

- [ ] **Step 4: Flip inventory-item.service.ts**

Same pattern.

- [ ] **Step 5: Flip stock-status.service.ts**

Same pattern.

- [ ] **Step 6: Flip notifications.service.ts**

Same pattern.

- [ ] **Step 7: Flip recommendations.service.ts**

Same pattern.

- [ ] **Step 8: Build and verify — confirm all 18 services done**

Run: `cd app/Storefront && pnpm run build`
Expected: Zero errors.
Verify: `grep -rn "USE_MOCK" app/Storefront/src/ --include="*.ts" | grep -v __tests__ | grep -v node_modules`
Expected: No output — zero remaining `USE_MOCK` in production code.

- [ ] **Step 9: Commit**

```bash
git add app/Storefront/src/features/profile/services/profile.service.ts app/Storefront/src/features/ordering/services/address/address.service.ts app/Storefront/src/features/locations/services/address/address.service.ts app/Storefront/src/features/inventory/services/inventory-item/inventory-item.service.ts app/Storefront/src/features/inventory/services/stock-status/stock-status.service.ts app/Storefront/src/features/notifications/services/notifications.service.ts app/Storefront/src/features/recommendations/services/recommendations.service.ts
git commit -m "feat(mvp): flip remaining 7 services to real API (P5)

Remove USE_MOCK from profile, addresses (×2), inventory (×2),
notifications, and recommendations services. All 18 services now
use real API repositories — zero USE_MOCK remaining.

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

### Task 6: Hide dropped features — Router

**Files:**
- Modify: `app/Storefront/src/app/router/index.ts`

**Interfaces:**
- Consumes: Current router with all routes active
- Produces: Same router with `/recommendations` route commented out

- [ ] **Step 1: Comment out the recommendations route**

In `src/app/router/index.ts`, wrap the recommendations route block (lines 54-59) in a block comment:

```ts
  // MVP: dropped — backend endpoint /api/storefront/recommendations/personalized does not exist
  /*
  {
    path: '/recommendations',
    name: 'recommendations',
    component: () => import('@/features/recommendations/views/RecommendationsView.vue'),
    meta: { title: 'Image Search & Recommendations', breadcrumb: 'Recommendations' },
  },
  */
```

Also hide any navigation links to `/recommendations` in the header. Check `src/app/components/layout/AppHeader.vue` for a link to recommendations and wrap it with `<!-- MVP: dropped -->` + `v-if="false"`.

- [ ] **Step 2: Build and verify**

Run: `cd app/Storefront && pnpm run build`
Expected: Zero errors (lazy-loaded component import is fine inside comment).

- [ ] **Step 3: Commit**

```bash
git add app/Storefront/src/app/router/index.ts
git commit -m "feat(mvp): hide recommendations route (dropped for MVP)

Backend endpoint /api/storefront/recommendations/personalized does
not exist. Comment out the /recommendations route.

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

### Task 7: Hide dropped features — UI components

**Files:**
- Modify: `app/Storefront/src/features/identity/views/LoginView.vue`
- Modify: `app/Storefront/src/features/catalog/views/ProductDetailView.vue`
- Modify: `app/Storefront/src/features/catalog/components/search/SearchBar.vue`
- Modify: `app/Storefront/src/features/ordering/views/CheckoutView.vue`
- Modify: `app/Storefront/src/features/profile/views/ProfileView.vue`
- Modify: `app/Storefront/src/app/components/layout/AppFooter.vue`
- Modify: `app/Storefront/src/app/components/layout/AppHeader.vue`

**Interfaces:**
- Consumes: Current views with all features visible
- Produces: Same views with dropped features hidden behind `v-if="false"`

- [ ] **Step 1: Hide external login button in LoginView.vue**

Find the "Sign in with Google" or external login button/section. Wrap it:
```html
<!-- MVP: dropped — no frontend implementation for external login -->
<div v-if="false">
  <!-- existing external login button -->
</div>
```

If there's no visible external login UI yet (the frontend never implemented it), skip this — nothing to hide.

- [ ] **Step 2: Hide reviews section in ProductDetailView.vue**

Find the `<ReviewList>` component or reviews section. Wrap it:
```html
<!-- MVP: dropped — no storefront API for reviews -->
<div v-if="false">
  <ReviewList ... />
</div>
```

- [ ] **Step 3: Hide search suggestions in SearchBar.vue**

Find the suggestions dropdown/autocomplete section. Wrap it:
```html
<!-- MVP: dropped — backend endpoint /api/storefront/search/suggestions does not exist -->
<div v-if="false">
  <!-- suggestions dropdown -->
</div>
```

- [ ] **Step 4: Hide save-payment checkbox in CheckoutView.vue**

Find "Save payment method" checkbox/section. Wrap it:
```html
<!-- MVP: dropped — non-essential for MVP checkout -->
<div v-if="false">
  <!-- save payment method checkbox -->
</div>
```

- [ ] **Step 5: Hide avatar upload in ProfileView.vue**

Find the avatar upload section. Wrap it:
```html
<!-- MVP: dropped — backend returns 501 for avatar upload -->
<div v-if="false">
  <!-- avatar upload UI -->
</div>
```

- [ ] **Step 6: Hide newsletter form in AppFooter.vue**

Find the `<NewsletterForm>` component. Wrap it:
```html
<!-- MVP: dropped — no backend for newsletter -->
<div v-if="false">
  <NewsletterForm />
</div>
```

- [ ] **Step 7: Hide recommendations link in AppHeader.vue**

Find any navigation link pointing to `/recommendations`. Wrap it:
```html
<!-- MVP: dropped — recommendations page removed from MVP -->
<router-link v-if="false" to="/recommendations">...</router-link>
```

- [ ] **Step 8: Build and verify**

Run: `cd app/Storefront && pnpm run build`
Expected: Zero errors. All `v-if="false"` wrappers preserve imports.

- [ ] **Step 9: Commit**

```bash
git add app/Storefront/src/features/identity/views/LoginView.vue app/Storefront/src/features/catalog/views/ProductDetailView.vue app/Storefront/src/features/catalog/components/search/SearchBar.vue app/Storefront/src/features/ordering/views/CheckoutView.vue app/Storefront/src/features/profile/views/ProfileView.vue app/Storefront/src/app/components/layout/AppFooter.vue app/Storefront/src/app/components/layout/AppHeader.vue
git commit -m "feat(mvp): hide dropped features from UI

Hide with v-if='false': reviews, search suggestions, external login,
save-payment checkbox, avatar upload, newsletter form, recommendations link.
All code preserved — only visibility toggled.

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

### Task 8: Final verification

- [ ] **Step 1: Confirm zero USE_MOCK remaining**

```bash
grep -rn "USE_MOCK" app/Storefront/src/ --include="*.ts" | grep -v __tests__ | grep -v node_modules
```
Expected: No output.

- [ ] **Step 2: Full build**

```bash
cd app/Storefront && pnpm run build
```
Expected: Zero errors, dist/ produced.

- [ ] **Step 3: Run unit tests**

```bash
cd app/Storefront && pnpm run test:unit
```
Expected: 274 pass, 26 pre-existing failures (payment-intent cancel mock). Same numbers as before — no new failures.

- [ ] **Step 4: Verify all 18 service files changed, mock files untouched**

```bash
# Services changed (18 files — adjust base ref to first MVP commit if needed)
git diff --name-only $(git log --oneline --reverse | grep "flip catalog" | head -1 | awk '{print $1}')^..HEAD -- 'app/Storefront/src/features/*/services/'

# Mock repos still exist (should list all .mock.repository.ts files)
find app/Storefront/src -name "*.mock.repository.ts" | wc -l
```
Expected: All mock files still on disk.

- [ ] **Step 5: Commit verification**

```bash
git add -A
git commit -m "chore(mvp): verification — zero USE_MOCK, build passes, tests pass

All 18 services flipped to real API. 15 features hidden from UI.
Zero USE_MOCK remaining in production code. Mock repos preserved on disk.

Co-Authored-By: Claude <noreply@anthropic.com>"
```
