# Gap 5: Related Products

## Summary

Wire the existing `GET /api/storefront/products/related` API to a new `RelatedProductsRow.vue` component on `ProductDetailView.vue`. The API endpoint exists but is not consumed.

## Current State

- `productApi.ts:26-31`: defines `getRelatedProducts(productId)` — not called anywhere
- `SimilarProductsRow.vue`: displays vector-similar products (pgvector cosine)
- `ProductDetailView.vue`: shows SimilarProductsRow + RecentlyViewedRow (homepage only)

## Design

### New Component: `RelatedProductsRow.vue`

**Location:** `app/Store/src/features/catalog/components/RelatedProductsRow.vue`

**Props:**
```ts
products: StoreProductListItemResponse[]
loading?: boolean
```

**UI:** Horizontal scrollable row of `ProductCard` components. Section header: "You Might Also Like". Matches `SimilarProductsRow.vue` pattern exactly.

### ProductDetailView Changes

**File:** `app/Store/src/features/catalog/views/ProductDetailView.vue`

- Import `getRelatedProducts` from `productApi.ts`
- Fetch related products on mount (after product loads)
- Render `RelatedProductsRow` below `SimilarProductsRow`
- Show loading skeleton while fetching

### Data Flow

```
ProductDetailView
  ├─ onMounted → getProductBySlug(slug)
  ├─ onMounted → getSimilarProducts(productId)  (existing)
  ├─ onMounted → getRelatedProducts(productId)   (NEW)
  ├─ SimilarProductsRow (vector similarity)
  ├─ RelatedProductsRow (taxon-based)            (NEW)
  └─ ProductCard (reused)
```

## Files to Create/Modify

| File | Action |
|------|--------|
| `features/catalog/components/RelatedProductsRow.vue` | CREATE |
| `features/catalog/views/ProductDetailView.vue` | MODIFY — add related products fetch + render |

## Acceptance Criteria

- [ ] Related products section appears on product detail page
- [ ] Products are fetched from existing API endpoint
- [ ] Horizontal scrollable row of ProductCard components
- [ ] Loading skeleton shown while fetching
- [ ] Empty state hidden if no related products
- [ ] Section appears below SimilarProductsRow
