# Implementation Plan: Gap 5 — Related Products

**Spec:** `docs/superpowers/specs/2026-08-07-gap5-related-products-design.md`
**Estimated effort:** Small (1 hour)
**Dependencies:** None

## Tasks

### T1: Create RelatedProductsRow.vue
- [ ] Create `app/Store/src/features/catalog/components/RelatedProductsRow.vue`
- [ ] Props: `products: StoreProductListItemResponse[]`, `loading?: boolean`
- [ ] Horizontal scrollable row of ProductCard components
- [ ] Section header: "You Might Also Like"
- [ ] Match SimilarProductsRow.vue pattern exactly

### T2: Wire to ProductDetailView
- [ ] Edit `app/Store/src/features/catalog/views/ProductDetailView.vue`
- [ ] Import `getRelatedProducts` from `productApi.ts`
- [ ] Add `relatedProducts` ref + `relatedLoading` ref
- [ ] Fetch on mount after product loads
- [ ] Render RelatedProductsRow below SimilarProductsRow
- [ ] Show SkeletonGrid while loading

### T3: Verify
- [ ] Related products section appears on product detail
- [ ] Products fetched from existing API
- [ ] Loading skeleton shown
- [ ] Empty state hidden if no related products

## Verification

```bash
cd app/Store && pnpm run lint && pnpm run test:unit
```
