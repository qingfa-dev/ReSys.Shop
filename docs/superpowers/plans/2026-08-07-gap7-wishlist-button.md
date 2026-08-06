# Gap 7: Wishlist Button on Product Cards Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add heart icon button to ProductCard for quick add-to-wishlist. Event-based: card emits event, parent handles API call.

**Architecture:** Heart button added to ProductCard (absolute bottom-left on hover). Parent ShopView manages wishlist state via existing `wishlistStore`. Auth guard: button hidden for unauthenticated users.

**Tech Stack:** Vue 3, Pinia, existing `wishlistApi.ts` endpoints

## Global Constraints

- Warnings-as-errors: any TypeScript/lint warning fails build
- Backend: `POST /api/store/profiles/wishlists/{id}/items` (add item), `DELETE /api/store/profiles/wishlists/{id}/items/{itemId}` (remove)
- ProductCard stays presentational — emits event, no direct API calls
- Auth state from `authStore`

---

## File Structure

| File | Action | Purpose |
|------|--------|---------|
| `app/Store/src/features/profile/stores/wishlistStore.ts` | MODIFY | Add wishlisted IDs state |
| `app/Store/src/features/catalog/components/ProductCard.vue` | MODIFY | Add heart button |
| `app/Store/src/features/catalog/views/ShopView.vue` | MODIFY | Handle wishlist toggle |

---

## Tasks

### Task 1: Add wishlisted state to wishlistStore

**Files:**
- Modify: `app/Store/src/features/profile/stores/wishlistStore.ts`

**Interfaces:**
- Consumes: `wishlistApi.getWishlists()`, `wishlistApi.addWishlistItem()`, `wishlistApi.removeWishlistItem()`
- Produces: `wishlistedProductIds`, `toggleWishlist(productId)`, `isWishlisted(productId)`

- [ ] **Step 1: Read wishlistStore.ts**

Read `app/Store/src/features/profile/stores/wishlistStore.ts` to understand existing structure.

- [ ] **Step 2: Add wishlistedProductIds state**

Add a new ref for tracking wishlisted product IDs:

```typescript
const wishlistedProductIds = ref<Set<string>>(new Set())
```

- [ ] **Step 3: Add fetchWishlistedIds action**

```typescript
async function fetchWishlistedIds(): Promise<void> {
  const result = await wishlistApi.getWishlists()
  if (result.isSuccess) {
    const ids = new Set<string>()
    for (const wl of result.items) {
      const detail = await wishlistApi.getWishlist(wl.id)
      if (detail.isSuccess) {
        for (const item of detail.value.items) {
          ids.add(item.productId)
        }
      }
    }
    wishlistedProductIds.value = ids
  }
}
```

- [ ] **Step 4: Add toggleWishlist action**

```typescript
async function toggleWishlist(productId: string): Promise<boolean> {
  if (wishlistedProductIds.value.has(productId)) {
    // Remove: find the wishlist and item to remove
    const wishlists = await wishlistApi.getWishlists()
    if (wishlists.isSuccess) {
      for (const wl of wishlists.items) {
        const detail = await wishlistApi.getWishlist(wl.id)
        if (detail.isSuccess) {
          const item = detail.value.items.find(i => i.productId === productId)
          if (item) {
            await wishlistApi.removeWishlistItem(wl.id, item.id)
            wishlistedProductIds.value.delete(productId)
            return false
          }
        }
      }
    }
  } else {
    // Add: use first wishlist or create one
    const wishlists = await wishlistApi.getWishlists()
    let targetId: string | null = null
    if (wishlists.isSuccess && wishlists.items.length > 0) {
      targetId = wishlists.items[0].id
    } else {
      const created = await wishlistApi.createWishlist({ name: 'My Wishlist', isPublic: false })
      if (created.isSuccess) targetId = created.value.id
    }
    if (targetId) {
      await wishlistApi.addWishlistItem(targetId, { productId })
      wishlistedProductIds.value.add(productId)
      return true
    }
  }
  return wishlistedProductIds.value.has(productId)
}
```

- [ ] **Step 5: Add isWishlisted getter**

```typescript
function isWishlisted(productId: string): boolean {
  return wishlistedProductIds.value.has(productId)
}
```

- [ ] **Step 6: Export new members**

Add `wishlistedProductIds`, `toggleWishlist`, `isWishlisted`, `fetchWishlistedIds` to the return object.

- [ ] **Step 7: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

### Task 2: Add heart button to ProductCard

**Files:**
- Modify: `app/Store/src/features/catalog/components/ProductCard.vue:7-9,44-73`

**Interfaces:**
- Consumes: `isWishlisted` prop (boolean)
- Produces: `toggleWishlist` emit (productId)

- [ ] **Step 1: Add props and emits**

Edit `app/Store/src/features/catalog/components/ProductCard.vue`. Update the props and emits:

```typescript
const props = defineProps<{ product: StoreProductListItemResponse; loading?: boolean; isWishlisted?: boolean }>()
const emit = defineEmits<{ addToCart: [variantId: string]; toggleWishlist: [productId: string] }>()
```

- [ ] **Step 2: Add heart button to template**

Inside the `<div class="relative">` (line 48), after the `<ProductBadge>` (line 49), add the heart button:

```vue
      <button
        v-if="isWishlisted !== undefined"
        class="absolute top-3 left-3 z-10 w-9 h-9 rounded-full flex items-center justify-center transition-colors"
        :class="isWishlisted ? 'bg-stone-900 text-white' : 'bg-white/80 text-stone-600 hover:bg-white hover:text-stone-900'"
        :aria-label="isWishlisted ? 'Remove from wishlist' : 'Add to wishlist'"
        @click.prevent="emit('toggleWishlist', product.id)"
      >
        <i :class="isWishlisted ? 'pi pi-heart-fill' : 'pi pi-heart'" />
      </button>
```

- [ ] **Step 3: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

### Task 3: Wire in ShopView

**Files:**
- Modify: `app/Store/src/features/catalog/views/ShopView.vue`

**Interfaces:**
- Consumes: `useWishlistStore()` from `wishlistStore.ts`
- Produces: Passes `isWishlisted` to ProductCard, handles `toggleWishlist` event

- [ ] **Step 1: Read ShopView.vue**

Read `app/Store/src/features/catalog/views/ShopView.vue` to understand the product grid structure.

- [ ] **Step 2: Add wishlistStore import**

```typescript
import { useWishlistStore } from '@/features/profile/stores/wishlistStore'
```

- [ ] **Step 3: Initialize store**

```typescript
const wishlist = useWishlistStore()
```

- [ ] **Step 4: Fetch wishlisted IDs on mount**

In the `onMounted` or after products load, call:

```typescript
wishlist.fetchWishlistedIds()
```

- [ ] **Step 5: Pass isWishlisted to ProductCard**

On each `<ProductCard>`, add:

```vue
:is-wishlisted="wishlist.isWishlisted(product.id)"
```

- [ ] **Step 6: Handle toggleWishlist event**

```vue
@toggle-wishlist="(id) => wishlist.toggleWishlist(id)"
```

- [ ] **Step 7: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 8: Run unit tests**

```bash
cd app/Store && pnpm run test:unit
```

Expected: PASS

- [ ] **Step 9: Commit**

```bash
cd app/Store && git add src/features/profile/stores/wishlistStore.ts src/features/catalog/components/ProductCard.vue src/features/catalog/views/ShopView.vue
git commit -m "feat(catalog): add wishlist heart button to product cards"
```
