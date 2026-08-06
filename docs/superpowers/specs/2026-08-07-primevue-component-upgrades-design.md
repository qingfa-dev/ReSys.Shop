# PrimeVue Component Upgrades Design Spec

## Summary

Replace custom implementations with PrimeVue v5 components for better UX and less maintenance. 10 component upgrades.

## Upgrades

### 1. Product Gallery → Galleria

**Current:** `ProductGallery.vue` uses raw `<Image>` with thumbnail strip.

**New:** PrimeVue `Galleria` with:
- Thumbnail navigation (circular thumbnails)
- Fullscreen preview on click
- Keyboard navigation (arrow keys)
- Responsive design
- NumVisible for visible thumbnails

**Files:** `features/catalog/components/ProductGallery.vue`

### 2. Product Scrollers → Carousel

**Current:** `FeaturedProductsRow`, `SimilarProductsRow`, `RelatedProductsRow` use custom horizontal scroll.

**New:** PrimeVue `Carousel` with:
- `numVisible` responsive (2 mobile, 3 tablet, 4 desktop)
- `numScroll` = 1
- Circular navigation
- Responsive breakpolets

**Files:** 3 component files

### 3. Checkout Stepper → Steps

**Current:** `CheckoutStepper.vue` manual numbered circles.

**New:** PrimeVue `Steps` with:
- Model-based step definition
- Active/completed/pending states
- Click navigation (with guard)
- Responsive (labels hide on mobile)

**Files:** `features/ordering/components/CheckoutStepper.vue`, `CheckoutView.vue`

### 4. Product Badges → Tag

**Current:** `ProductBadge.vue` custom positioned badge.

**New:** PrimeVue `Tag` with:
- `severity="success"` for New
- `severity="danger"` for Sale
- Consistent with status tags

**Files:** `features/catalog/components/ProductBadge.vue`

### 5. Status Tags → Tag

**Current:** `StatusTag.vue` custom colored pill.

**New:** PrimeVue `Tag` with severity mapping:
- Placed → `info`
- Shipped → `warn`
- Delivered → `success`
- Canceled → `danger`

**Files:** `shared/components/StatusTag.vue`

### 6. Scroll-to-Top → ScrollTop

**Current:** `ScrollToTop.vue` custom button with scroll listener.

**New:** PrimeVue `ScrollTop` with:
- `threshold` = 500
- Built-in smooth scroll
- Icon customization

**Files:** `shared/components/ScrollToTop.vue`

### 7. Cart Badge → Badge

**Current:** Cart count rendered as custom `<span>` in header.

**New:** PrimeVue `Badge` with:
- `severity="danger"` for count
- Positioned on cart icon
- Auto-hide when empty

**Files:** `app/components/layout/AppHeader.vue`

### 8. Skeleton Loading → Keep Current

**Decision:** Keep custom `SkeletonCard`/`SkeletonGrid` — better for card layouts than ProgressSpinner.

### 9. Filter Chips → Chip

**Current:** Active filters shown as plain text.

**New:** PrimeVue `Chip` for active filter display with remove button.

**Files:** `features/catalog/components/FilterSidebar.vue`

### 10. Product Detail Tabs → TabView

**Current:** Description uses `Accordion`.

**New:** PrimeVue `TabView` with tabs:
- Description
- Specifications (from ProductDetailsInfo)
- Reviews (placeholder for future)

**Files:** `features/catalog/views/ProductDetailView.vue`

## Verification

- [ ] Galleria renders with thumbnails and fullscreen
- [ ] Carousels scroll products with responsive count
- [ ] Steps show correct active/completed states
- [ ] Tags display with correct severities
- [ ] ScrollTop appears after 500px
- [ ] Cart badge shows count
- [ ] Filter chips removable
- [ ] Tabs switch content
- [ ] All 257 unit tests pass
