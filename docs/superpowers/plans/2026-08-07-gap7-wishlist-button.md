# Implementation Plan: Gap 7 — Wishlist Button on Product Cards

**Spec:** `docs/superpowers/specs/2026-08-07-gap7-wishlist-button-design.md`
**Estimated effort:** Medium (2-3 hours)
**Dependencies:** None

## Tasks

### T1: Add wishlist state to wishlistStore
- [ ] Edit `app/Store/src/features/profile/stores/wishlistStore.ts`
- [ ] Add `wishlistedProductIds: ref<Set<string>>` state
- [ ] Add `toggleWishlist(productId)` action (calls API)
- [ ] Add `isWishlisted(productId)` getter
- [ ] Add `fetchWishlistedIds()` action (fetches all wishlist items)
- [ ] Call `fetchWishlistedIds()` on auth login (in authStore)

### T2: Add heart button to ProductCard
- [ ] Edit `app/Store/src/features/catalog/components/ProductCard.vue`
- [ ] Add `isWishlisted` prop (boolean)
- [ ] Add `toggleWishlist` emit (productId)
- [ ] Add heart button (44x44px circular) at absolute bottom-left
- [ ] Toggle pi-heart / pi-heart-fill based on prop
- [ ] Show only when authenticated (use authStore)
- [ ] @click.prevent to stop navigation

### T3: Wire in ShopView
- [ ] Edit `app/Store/src/features/catalog/views/ShopView.vue`
- [ ] Import wishlistStore
- [ ] Pass `isWishlisted` prop to each ProductCard
- [ ] Handle `toggleWishlist` event: call wishlistStore.toggleWishlist
- [ ] Handle unauthenticated click: toast "Please log in"

### T4: Verify
- [ ] Heart button appears on hover
- [ ] Heart toggles outline/filled
- [ ] Only visible for authenticated users
- [ ] Toggle calls wishlist API
- [ ] State persists across navigation

## Verification

```bash
cd app/Store && pnpm run lint && pnpm run test:unit
```
