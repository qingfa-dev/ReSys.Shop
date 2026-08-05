# Storefront Feature Restoration — 13 Features

**Date**: 2026-08-05
**Scope**: Restore dropped legacy storefront features using real backend API endpoints
**Depends on**: Spec A (Design System foundation — teal tokens + fonts must be in place)
**Status**: Approved

## Goal

Restore 13 features from the legacy `app/legacy/Storefront/` that were dropped
in the PrimeVue 5 plan (`2026-08-04-storefront-primevue5.md`). All new
components consume Spec A's teal design tokens via Tailwind utility classes.
All data comes from real backend API endpoints. No mock data or mock
repositories.

## Feature List

| # | Feature | Thesis UC | Backend API | Batch |
|---|---------|-----------|-------------|-------|
| 1 | Rich footer (4-column) | N/A | N/A (static) | 1 |
| 2 | Breadcrumbs on shop/collections/checkout | UC-STR-BRW | N/A (router) | 1 |
| 3 | Hero section component | N/A | N/A (static) | 2 |
| 4 | Features strip (4 icons) | N/A | N/A (static) | 2 |
| 5 | Category grid on Home | UC-STR-BRW | `GET /api/storefront/taxons` | 2 |
| 6 | Featured products row on Home | UC-STR-BRW | `GET /api/storefront/products` | 2 |
| 7 | Recently viewed products | N/A | localStorage | 5 |
| 8 | Search overlay with autocomplete | UC-STR-BRW alt flow | `GET /api/storefront/products?search=` | 3 |
| 9 | Product card badges (New/Sale) | CAT-FR-16 | `status` + `availableOn` in list API | 5 |
| 10 | Size guide modal on PDP | UC-STR-BRW | `variants[].optionValue1/2` in detail API | 4 |
| 11 | Product details info on PDP | UC-STR-BRW | All fields in detail API response | 4 |
| 12 | Notification bell in header | UC-STR-PRF | `GET /api/store/profiles/notification-preferences` | 6 |
| 13 | Terms + Privacy views | N/A | N/A (static) | 6 |

## Architecture

### Data Flow Convention

All features follow the existing `app/Store` pattern:
- API functions in `features/*/services/*Api.ts` (already exist — reuse)
- Store state in `features/*/stores/*Store.ts` (already exist — reuse)
- New composables in `features/*/composables/use*.ts` or `shared/composables/use*.ts`
- View components in `features/*/views/*View.vue` (modified)
- Presentation components in `features/*/components/*.vue` (new)

No mock data. No mock repositories. No `USE_MOCK` flags.

### Styling Convention

All new components use **Tailwind utility classes exclusively** for layout
and spacing. Color tokens come from PrimeVue `--p-*` CSS variables set by
Spec A. No scoped SCSS. No component-level `<style>` blocks except for
animation/transition utilities.

Color classes: `teal-*` for primary elements, `stone-*` for neutral
surfaces/text. See Spec A token map for the full palette.

## Batch 1 — App Shell

### Feature 1: Rich Footer

**File**: `src/app/components/layout/AppFooter.vue` — rewrite from 14 to ~100 lines

**Current**: Copyright text + Terms/Privacy links. 14 lines.
**Target**: 4-column responsive grid with brand, link groups, social icons, bottom bar.

**Layout**:
```
Desktop: 2fr | 1fr | 1fr | 1fr
Tablet:  full | 1fr | 1fr      (brand full-width)
Mobile:  full                   (all stacked)
```

**Column 1 — Brand (2fr)**:
- Logo: "ReSys.Shop" (Playfair Display, text-xl, font-bold)
- Tagline: "Discover your style with visual search."
- Social icons row: Instagram, Twitter, Facebook, GitHub (pi icons, circle on hover, teal)

**Column 2 — Shop**:
- Heading: "Shop" (uppercase, tracking-wider, text-sm, stone-900, font-semibold)
- Links: All Products, New Arrivals, Collections, Sale

**Column 3 — Support**:
- Heading: "Support"
- Links: Help Center, Shipping Info, Returns, Size Guide

**Column 4 — Company**:
- Heading: "Company"
- Links: About Us, Careers, Privacy, Terms

**Bottom bar**: Copyright text + payment method icons (Visa, Mastercard, Stripe — pi-icons). Border-top separator.

**Note on placeholder links**: Links to `/help`, `/shipping`, `/returns`,
`/size-guide`, `/about`, `/careers` are intentional placeholders — they will
404 until content pages are created. Only `/privacy` and `/terms` have real
views (see Feature 13). This is acceptable for MVP.

### Feature 2: Breadcrumbs

**Files modified**:
- `src/features/catalog/views/ShopView.vue`
- `src/features/catalog/views/CollectionsView.vue`
- `src/features/ordering/views/CheckoutView.vue`

Add `<Breadcrumb>` as first element inside the page container div (after the
`max-w-7xl mx-auto px-4` wrapper). Use PrimeVue's `Breadcrumb` component
with `:model` bound to computed breadcrumb items.

**ProductDetailView**: Already has manual breadcrumbs. Upgrade to
`<Breadcrumb>` for consistency.

**Breadcrumb paths**:
- `/shop` -> Home > Shop
- `/collections` -> Home > Collections
- `/checkout` -> Home > Cart > Checkout
- `/products/:slug` -> Home > Shop > {product.name} (already exists)

Each view defines `breadcrumbItems` as a local `computed()`. No shared
breadcrumb utility needed — 3-4 views, each with different logic.

## Batch 2 — Home Page

### Feature 3: Hero Section

**File**: `src/features/catalog/components/HeroSection.vue` — new, ~40 lines

Extracted from inline code in HomeView. Reusable in future landing pages.

**Visual**: Full-width section, teal gradient background (`bg-gradient-to-br
from-teal-800 via-teal-700 to-teal-900`), min-height 70vh desktop, 50vh mobile.
Content centered vertically and horizontally.

**Content**:
- Badge pill: "New Collection" (bg-teal-100/20, text-white, rounded-full)
- Heading: "Discover Your Style" (Playfair Display, text-4xl md:text-6xl, white)
- Subtitle: "Shop the latest fashion trends. Upload an image, find your look."
  (DM Sans, text-lg, white/70)
- CTAs: "Shop All" (Button, primary) + "Visual Search" (Button, outlined, white)
- Scroll indicator: animated chevron-down at bottom

### Feature 4: Features Strip

**File**: `src/features/catalog/components/FeaturesStrip.vue` — new, ~50 lines

**Visual**: White background, responsive grid (4 cols desktop, 2 cols mobile),
border-bottom separator. Each item: centered icon in teal circle (w-12 h-12,
bg-teal-50, text-teal-700), title (font-semibold), description (text-stone-500,
text-sm).

**Icons**:
1. `pi pi-truck` — Free Shipping — "On orders over 500.000 d"
2. `pi pi-undo` — Easy Returns — "30-day return policy"
3. `pi pi-shield` — Secure Payment — "SSL encrypted checkout"
4. `pi pi-headphones` — 24/7 Support — "Dedicated customer service"

**Data**: Static — no API. Text strings are hardcoded (no i18n yet).

### Feature 5: Category Grid

**File**: `src/features/catalog/components/CategoryGrid.vue` — new, ~60 lines

**Data source**: `GET /api/storefront/taxons?pageSize=8` on mount.
Uses `taxonApi.getTaxons()` (already exists).

**Visual**: Responsive grid (2 cols mobile, 4 cols desktop). Each category
card: aspect-[4/3], image (`taxon.imageUrl`) as background with gradient overlay,
category name (Playfair Display, white, text-xl), product count badge.

**States**: Loading (SkeletonGrid 8 cards), error (Message + retry), empty
(hide section entirely — not an error).

**Link**: Each card links to `/shop?taxonId={id}`, applying the taxon filter
in the shop page.

### Feature 6: Featured Products Row

**File**: `src/features/catalog/components/FeaturedProductsRow.vue` — new, ~50 lines

**Data source**: `getPagedProducts({ pageNumber:1, pageSize:12, sort:['-createdAtUtc'] })`.
Uses `productApi.getPagedProducts()` (already exists).

**Visual**: Section heading "Featured" with "View All ->" link. Horizontal
scroll container (`overflow-x-auto`, `flex gap-4 px-1 pb-4`) with
`<ProductCard>` for each item.

**States**: Loading (SkeletonCard[] in horizontal row), error (Message),
empty (hide section).

### Feature 7: Recently Viewed Row (reuses Batch 5 composable)

Same horizontal scroll pattern as Featured Products. Heading "Recently
Viewed" with "Clear" link (calls `useRecentlyViewed().clear()`). Only
renders when items.length > 0.

### HomeView Integration

**File**: `src/features/catalog/views/HomeView.vue` — rewrite from 60 to ~120 lines

New section order (top to bottom):
1. `<HeroSection />`
2. `<FeaturesStrip />`
3. `<CategoryGrid />`
4. `<FeaturedProductsRow />`
5. `<RecentlyViewedRow />` (conditional)
6. "New Arrivals" `<ProductGrid />` (existing code, unchanged)

Each section is independently rendered with its own loading/error state.
No global page-level loading — sections appear incrementally as data loads.

## Batch 3 — Search Overlay

### Feature 8: Search Overlay

**Files**:
- `src/features/catalog/composables/useSearch.ts` — new, ~60 lines
- `src/features/catalog/components/SearchOverlay.vue` — new, ~120 lines
- `src/app/components/layout/AppHeader.vue` — modify (replace search form with icon trigger)

### useSearch Composable API

```ts
export function useSearch() {
  const isOpen: Ref<boolean>
  const query: Ref<string>
  const results: Ref<StoreProductListItemResponse[]>
  const loading: Ref<boolean>
  const selectedIndex: Ref<number>
  const error: Ref<string | null>

  const open(): void
  const close(): void
  const search(): Promise<void>  // debounced 300ms
  const clear(): void
  const navigateToResult(index: number): void
}
```

Debounce: 300ms after last keystroke. API call: `getPagedProducts({ search:
query, pageSize:5 })`. Keyboard: ArrowUp/Down moves `selectedIndex`, Enter
calls `navigateToResult`, Escape closes overlay.

Module-level singleton — one SearchOverlay instance shared across all pages
via App.vue mount.

### SearchOverlay Component

PrimeVue `Dialog` component:
- Desktop: modal, 600px width, centered
- Mobile: full-screen (Dialog handles this via `breakpoints` option)

**Content**:
1. Auto-focused `<InputText>` with search icon prefix, placeholder "Search products..."
2. Results list (or loading skeleton, or empty state)
3. Footer: "View all results for '{query}' ->" link to `/shop?search={query}`

**Result item**: Thumbnail (48x48, rounded), product name (truncated),
price (font-semibold). Hover/active: bg-teal-50. Click: navigate to
`/products/{slug}`.

**Keyboard shortcut**: Ctrl+K / Cmd+K opens search. Global keydown listener
registered in App.vue (not the composable, to avoid multiple listeners).

### AppHeader Integration

Replace current `<form>` (lines 33-42) with:
```vue
<button @click="search.open()" class="p-2 text-stone-500 hover:text-teal-700" aria-label="Search">
  <i class="pi pi-search text-xl" />
</button>
```

SearchOverlay mounted once in `App.vue`:
```vue
<SearchOverlay />
```

## Batch 4 — Product Detail Enhancements

### Feature 10: Size Guide Modal

**File**: `src/features/catalog/components/SizeGuideModal.vue` — new, ~50 lines

**Trigger**: Text link "Size Guide" with ruler icon (`pi pi-ruler`), placed
next to the variant picker heading in ProductDetailView.

**Dialog**: PrimeVue `Dialog`, header "Size Guide — {product.name}". Content:
2-column table if variant data has structured measurements, otherwise chip
list of available sizes.

**Data source**: `product.variants[].optionValue1/optionValue2` — already
loaded in ProductDetailView. No additional API call. Passed as prop:
`<SizeGuideModal :variants="product.variants" />`.

**Fallback**: If no option values with measurements exist, display "Size
information not available for this product."

**ProductDetailView integration**: Add trigger button after the `<h1>`
heading, before the price section.

### Feature 11: Product Details Info

**File**: `src/features/catalog/components/ProductDetailsInfo.vue` — new, ~50 lines

**Visual**: Property list with icons, placed between the fashion metadata
badges and the accordion panels.

**Fields** (only shown when non-null):
| Icon | Label | Source field |
|------|-------|-------------|
| `pi pi-hashtag` | Style Code | `product.styleCode` |
| `pi pi-leaf` | Material | `product.materialComposition` |
| `pi pi-droplet` | Care | `product.careInstructions` |
| `pi pi-ruler` | Fit | `product.fitNotes` |
| `pi pi-calendar` | Season | `product.seasonName` |
| `pi pi-tag` | Department | `product.department` |
| `pi pi-users` | Gender | `product.genderTarget` |

**Layout**: 2-column grid desktop, 1-column mobile. Each row: icon
(text-teal-600), label (text-stone-500, text-sm), value (text-stone-900,
font-medium). Gap between rows: `mb-2`.

**ProductDetailView integration**: Insert after the fashion metadata badges
div (lines 116-122), before the variant options section (line 125).

### Recently Viewed Tracking

In ProductDetailView `loadProduct()` success path, after product is set:
```ts
useRecentlyViewed().add({
  productId: product.value.id,
  productName: product.value.name,
  slug: product.value.slug,
  thumbnailUrl: product.value.thumbnailUrl,
  minPrice: product.value.minPrice,
  viewedAt: Date.now()
})
```

Import: `import { useRecentlyViewed } from '@/shared/composables/useRecentlyViewed'`

## Batch 5 — Shared Components

### Feature 7 (cont): useRecentlyViewed

**File**: `src/shared/composables/useRecentlyViewed.ts` — new, ~50 lines

```ts
interface RecentlyViewedItem {
  productId: string
  productName: string
  slug: string
  thumbnailUrl: string | null
  minPrice: number | null
  viewedAt: number
}

export function useRecentlyViewed(maxItems = 10) {
  const items: Ref<RecentlyViewedItem[]>
  const add(product: RecentlyViewedItem): void
  const clear(): void
}
```

Storage key: `recentlyViewed`. JSON array in localStorage. LRU eviction:
when `maxItems` reached, shift oldest entry (lowest `viewedAt`). Dedup:
if same `productId` exists, remove old entry, push new at end. Syncs to
localStorage on every mutation. Try/catch on JSON.parse — fallback to `[]`.

### Feature 9: Product Badges

**File**: `src/features/catalog/components/ProductBadge.vue` — new, ~20 lines

**Props**: `variant: 'new' | 'sale'`

**Visual**: Absolute positioned `top-3 left-3` inside product card thumbnail
area. Pill shape: `rounded-full px-2.5 py-1 text-xs font-semibold uppercase
tracking-wide`. Colors: new -> `bg-teal-600 text-white`, sale ->
`bg-red-500 text-white`. Text: "New" or "Sale".

**Integration in ProductCard.vue**: Wrap thumbnail `<router-link>` in
`<div class="relative">`. Add `<ProductBadge>` as first child:
```vue
<div class="relative">
  <ProductBadge v-if="isNew" variant="'new'" />
  <router-link :to="`/products/${product.slug}`" ...>
    ...
  </router-link>
</div>
```

**isNew computed**:
```ts
const isNew = computed(() => {
  if (!props.product.availableOn) return false
  const diff = Date.now() - new Date(props.product.availableOn).getTime()
  return diff >= 0 && diff <= 14 * 24 * 60 * 60 * 1000
})
```

## Batch 6 — Notification Bell + Legal Pages

### Feature 12: Notification Bell

**File**: `src/features/catalog/components/NotificationBell.vue` — new, ~60 lines

**Visual**: Bell icon button (`pi pi-bell`) in AppHeader action bar, after
search icon, before cart. Badge overlay with unread count (red, only shown
when count > 0).

**Data source**: `GET /api/store/profiles/notification-preferences` on mount.
Uses `notificationApi.getPreferences()` (already exists).

**Behavior on click**: Opens PrimeVue `Popover` anchored to the bell. Shows
3 toggle-style items: Email notifications, SMS notifications, Newsfeeds.
Each with current state icon (check-circle / x-circle). "Manage Preferences"
link at bottom -> `/account/notifications`.

**States**: Unauthenticated (hide bell entirely — `v-if="auth.isAuthenticated"`),
authenticated (show bell with fetched preferences).

**Integration**: Add `<NotificationBell />` in AppHeader action div, after
search trigger, before theme toggle:
```
Search -> NotificationBell -> ThemeToggle -> Cart -> User
```

### Feature 13: Terms + Privacy Views

**Files**:
- `src/features/catalog/views/TermsView.vue` — new, ~80 lines
- `src/features/catalog/views/PrivacyView.vue` — new, ~80 lines
- `src/features/catalog/routes/index.ts` — modify (add 2 routes)

**Visual**: Prose container (`max-w-3xl mx-auto px-4 py-16`). Semantic HTML
with h1, h2, p tags. Placeholder legal text. Uses DefaultLayout.

**Routes added**:
```ts
{ path: '/terms', name: 'terms', component: () => import('../views/TermsView.vue'), meta: { title: 'Terms of Service' } },
{ path: '/privacy', name: 'privacy', component: () => import('../views/PrivacyView.vue'), meta: { title: 'Privacy Policy' } },
```

**Note**: These routes are added in the `catalogRoutes` array which is
registered under `DefaultLayout` in `routes.ts`. No special auth required.

## File Inventory

### New Files (14)

| File | Batch |
|------|-------|
| `src/features/catalog/components/HeroSection.vue` | 2 |
| `src/features/catalog/components/FeaturesStrip.vue` | 2 |
| `src/features/catalog/components/CategoryGrid.vue` | 2 |
| `src/features/catalog/components/FeaturedProductsRow.vue` | 2 |
| `src/features/catalog/components/RecentlyViewedRow.vue` | 2 |
| `src/features/catalog/components/ProductBadge.vue` | 5 |
| `src/features/catalog/components/SizeGuideModal.vue` | 4 |
| `src/features/catalog/components/ProductDetailsInfo.vue` | 4 |
| `src/features/catalog/components/NotificationBell.vue` | 6 |
| `src/features/catalog/components/SearchOverlay.vue` | 3 |
| `src/features/catalog/composables/useSearch.ts` | 3 |
| `src/shared/composables/useRecentlyViewed.ts` | 5 |
| `src/features/catalog/views/TermsView.vue` | 6 |
| `src/features/catalog/views/PrivacyView.vue` | 6 |

### Modified Files (7)

| File | Batch | Change |
|------|-------|--------|
| `src/app/components/layout/AppFooter.vue` | 1 | Full rewrite to 4-column |
| `src/app/components/layout/AppHeader.vue` | 1,3,6 | Search trigger + ThemeToggle + NotificationBell + nav links |
| `src/features/catalog/views/HomeView.vue` | 2 | Add 5 new sections |
| `src/features/catalog/views/ProductDetailView.vue` | 4 | Size guide trigger + ProductDetailsInfo + recently viewed tracking |
| `src/features/catalog/views/ShopView.vue` | 1 | Add Breadcrumb |
| `src/features/catalog/views/CollectionsView.vue` | 1 | Add Breadcrumb |
| `src/features/catalog/components/ProductCard.vue` | 5 | Add ProductBadge + relative wrapper |
| `src/features/ordering/views/CheckoutView.vue` | 1 | Add Breadcrumb |
| `src/features/catalog/routes/index.ts` | 6 | Add Terms + Privacy routes |
| `src/App.vue` | 3 | Mount SearchOverlay + Ctrl+K listener |

## Batch Dependency Graph

```
Batch 1 (App Shell)   -> independent
Batch 5 (Shared)       -> independent
Batch 2 (Home Page)   -> depends on Batch 5 (useRecentlyViewed)
Batch 3 (Search)       -> independent
Batch 4 (PDP)          -> depends on Batch 5 (useRecentlyViewed)
Batch 6 (Bell + Legal) -> independent
```

Batches can be implemented in any order except 2/4 depend on 5.

## Risk Matrix

| Risk | Impact | Mitigation |
|------|--------|------------|
| Search overlay conflicts with PrimeVue Dialog z-index | Medium | Dialog manages z-index via Aura tokens; no custom z-index |
| RecentlyViewed localStorage corrupted by bad data | Low | Try/catch JSON.parse, fallback to `[]`, validate item shape |
| CategoryGrid API returns empty taxons | Low | Hide section entirely, not error message |
| Placeholder footer links 404 on click | Low | Acceptable for MVP; only `/terms`/`/privacy` have real views |
| ProductBadge fires on products older than 14 days | Low | Test with seeded data: product with `availableOn` 3 days ago vs 30 days ago |
| SizeGuideModal has no data for a product | Medium | Show "Size information not available" message |
| Notification bell renders when not authenticated | Low | Guard with `v-if="auth.isAuthenticated"` |
| Multiple SearchOverlay instances if mounted in multiple places | Low | Singleton composable; mount once in App.vue |

## Verification

1. `pnpm run type-check` — 0 errors after all new files added
2. `pnpm run lint` — 0 violations
3. `pnpm run test:unit -- --run` — existing tests still pass
4. Home page: 6 sections render in order, no layout break on mobile viewport
5. Ctrl+K / Cmd+K: search overlay opens, type query -> results appear, Enter -> navigates
6. Product detail: Size Guide link opens modal with data. ProductDetailsInfo shows non-null fields with correct icons
7. Product card: new products (< 14 days) show "New" badge, older products don't
8. Footer: 4 columns desktop, stacked mobile. Links to /terms and /privacy work
9. Recently viewed: visit 3 products -> Home page shows "Recently Viewed" row with 3 product cards
10. Notification bell: shown in header when authenticated, popover opens with preferences
11. /terms, /privacy: render content (not 404)
12. Breadcrumbs present on /shop, /collections, /checkout
13. All new components render correctly in dark mode (toggled via ThemeToggle from Spec A)

## Out of Scope

- Reviews (no backend endpoint)
- Newsletter signup (MVP disabled in legacy, no backend endpoint)
- Grid/List view toggle (nice-to-have, not in legacy thesis UCs)
- Product quality badges (Free shipping/returns/payment — static marketing, not product data)
- Infinite scroll (pagination is fine)
- i18n/multi-language (separate project)
- E2E tests (separate phase)
- Backend API changes (all features use existing endpoints)

## Related Specs

- **Spec A**: `2026-08-05-storefront-design-system-design.md` — provides teal tokens and fonts
- **Spec C**: `2026-08-05-storefront-api-fixes-checkout-design.md` — checkout wiring, route fixes
