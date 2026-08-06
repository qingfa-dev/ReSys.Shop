# Gap 7: Wishlist Button on Product Cards

## Summary

Add a heart icon button to `ProductCard.vue` for quick add-to-wishlist. Event-based: card emits event, parent handles API call. Keeps card presentational.

## Current State

- `ProductCard.vue`: has "Quick Add" overlay on hover (bottom of image)
- `wishlistApi.ts`: full CRUD API exists (`getWishlists`, `addWishlistItem`, `removeWishlistItem`)
- `WishlistsView.vue`: manages wishlists (create, delete, items)
- No inline wishlist action on product cards

## Design

### ProductCard Changes

**File:** `app/Store/src/features/catalog/components/ProductCard.vue`

**New emit:**
```ts
toggleWishlist: [productId: string]
```

**New prop:**
```ts
isWishlisted?: boolean
```

**UI:** Heart button (44x44px circular) at absolute bottom-left on image area. Appears on hover alongside Quick Add. Toggle between `pi-heart` (outline) and `pi-heart-fill` (filled, primary color) based on `isWishlisted` prop.

```
Hover state:
┌──────────┐
│ [image]  │
│  ♡  [+]  │  ← heart (left) + Quick Add (right)
│ NEW      │
├──────────┤
```

### ShopView Changes

**File:** `app/Store/src/features/catalog/views/ShopView.vue`

- Import `wishlistApi` functions
- Fetch wishlisted product IDs on mount (if authenticated)
- Pass `isWishlisted` prop to each `ProductCard`
- Handle `toggleWishlist` event: call `addWishlistItem` or `removeWishlistItem`
- Update local wishlisted state

### Auth Guard

- Heart button visible only when user is authenticated
- If not authenticated, clicking shows login prompt (toast or redirect)

### wishlistStore Addition

**File:** `app/Store/src/features/profile/stores/wishlistStore.ts` (existing)

- Add `wishlistedProductIds: ref<Set<string>>` state
- Add `toggleWishlist(productId)` action
- Add `isWishlisted(productId)` getter
- Fetch wishlisted IDs on auth login

## Files to Create/Modify

| File | Action |
|------|--------|
| `features/catalog/components/ProductCard.vue` | MODIFY — add heart button + emit |
| `features/catalog/views/ShopView.vue` | MODIFY — handle wishlist toggle |
| `features/profile/stores/wishlistStore.ts` | MODIFY — add wishlisted IDs state |

## Acceptance Criteria

- [ ] Heart button appears on product card hover
- [ ] Heart toggles between outline and filled
- [ ] Only visible for authenticated users
- [ ] Toggle calls wishlist API (add/remove item)
- [ ] State persists across page navigation
- [ ] Works on both grid and list card layouts
