# Fix Broken Quick Add Design Spec

## Summary

Add `@add-to-cart` event listeners to 3 row components that define the emit but never receive it. Trivial fix — 3 one-line changes.

## Findings

### 1. FeaturedProductsRow on HomeView

**File:** `HomeView.vue:34`

`FeaturedProductsRow` defines `addToCart` emit (bubbled from ProductCard). HomeView renders it without `@add-to-cart`. Clicking Quick Add on featured products does nothing.

**Fix:** Add `@add-to-cart="(id) => cart.addItem(id)"` to `<FeaturedProductsRow>`.

### 2. RecentlyViewedRow on HomeView

**File:** `HomeView.vue:35`

Same issue. `RecentlyViewedRow` defines `addToCart` emit. HomeView ignores it.

**Fix:** Add `@add-to-cart="(id) => cart.addItem(id)"` to `<RecentlyViewedRow>`.

### 3. RelatedProductsRow on ProductDetailView

**File:** `ProductDetailView.vue:206`

Same issue. `RelatedProductsRow` defines `addToCart` emit. ProductDetailView ignores it.

**Fix:** Add `@add-to-cart="(id) => cart.addItem(id)"` to `<RelatedProductsRow>`.

## Prerequisites

- `cartStore` must be imported in `HomeView.vue` and `ProductDetailView.vue`
- `cart.addItem(variantId)` must work (verify in Sub-Project 2)

## Verification

- [ ] Quick Add works on featured products (homepage)
- [ ] Quick Add works on recently viewed products (homepage)
- [ ] Quick Add works on related products (product detail)
- [ ] Toast notification shows on successful add
- [ ] Cart badge updates
