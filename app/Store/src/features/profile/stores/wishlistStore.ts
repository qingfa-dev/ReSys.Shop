import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as wishlistApi from '../services/wishlistApi'
import type {
  WishlistListItem,
  WishlistDetail,
  CreateWishlistRequest,
  UpdateWishlistRequest,
  AddWishlistItemRequest,
} from '../types/wishlist'

export const useWishlistStore = defineStore('wishlists', () => {
  const items = ref<WishlistListItem[]>([])
  const loading = ref(false)
  const saving = ref(false)
  const error = ref<string | null>(null)
  const details = ref<Record<string, WishlistDetail>>({})
  const detailLoadingId = ref<string | null>(null)
  const wishlistedVariantIds = ref<Set<string>>(new Set())

  function upsertDetail(detail: WishlistDetail): void {
    details.value[detail.id] = detail
    // Keep the list card's itemCount/privacy/name in sync with the detail payload.
    const entry = items.value.find((w) => w.id === detail.id)
    if (entry) {
      entry.itemCount = detail.itemCount
      entry.name = detail.name
      entry.isPrivate = detail.isPrivate
    }
  }

  async function fetchWishlists(): Promise<boolean> {
    loading.value = true
    error.value = null
    try {
      const result = await wishlistApi.getWishlists()
      if (result.isSuccess) {
        items.value = result.items
        return true
      }
      error.value = result.message ?? result.errors[0]?.message ?? 'Failed to load wishlists'
      return false
    } catch {
      error.value = 'Failed to load wishlists'
      return false
    } finally {
      loading.value = false
    }
  }

  async function fetchWishlist(id: string): Promise<WishlistDetail | null> {
    detailLoadingId.value = id
    error.value = null
    try {
      const result = await wishlistApi.getWishlist(id)
      if (result.isSuccess) {
        details.value[id] = result.value
        return result.value
      }
      error.value = result.message ?? result.errors[0]?.message ?? 'Failed to load wishlist'
      return null
    } catch {
      error.value = 'Failed to load wishlist'
      return null
    } finally {
      detailLoadingId.value = null
    }
  }

  async function createWishlist(req: CreateWishlistRequest): Promise<boolean> {
    saving.value = true
    error.value = null
    try {
      const result = await wishlistApi.createWishlist(req)
      if (result.isSuccess) {
        upsertDetail(result.value)
        items.value.unshift({
          id: result.value.id,
          name: result.value.name,
          isPrivate: result.value.isPrivate,
          itemCount: result.value.itemCount,
        })
        return true
      }
      error.value = result.message ?? result.errors[0]?.message ?? 'Failed to create wishlist'
      return false
    } catch {
      error.value = 'Failed to create wishlist'
      return false
    } finally {
      saving.value = false
    }
  }

  async function updateWishlist(id: string, req: UpdateWishlistRequest): Promise<boolean> {
    saving.value = true
    error.value = null
    try {
      const result = await wishlistApi.updateWishlist(id, req)
      if (result.isSuccess) {
        upsertDetail(result.value)
        return true
      }
      error.value = result.message ?? result.errors[0]?.message ?? 'Failed to update wishlist'
      return false
    } catch {
      error.value = 'Failed to update wishlist'
      return false
    } finally {
      saving.value = false
    }
  }

  async function deleteWishlist(id: string): Promise<boolean> {
    saving.value = true
    error.value = null
    try {
      const result = await wishlistApi.deleteWishlist(id)
      if (result.isSuccess) {
        items.value = items.value.filter((w) => w.id !== id)
        delete details.value[id]
        return true
      }
      error.value = result.message ?? result.errors[0]?.message ?? 'Failed to delete wishlist'
      return false
    } catch {
      error.value = 'Failed to delete wishlist'
      return false
    } finally {
      saving.value = false
    }
  }

  async function addItem(id: string, req: AddWishlistItemRequest): Promise<boolean> {
    saving.value = true
    error.value = null
    try {
      const result = await wishlistApi.addWishlistItem(id, req)
      if (result.isSuccess) {
        upsertDetail(result.value)
        return true
      }
      error.value = result.message ?? result.errors[0]?.message ?? 'Failed to add item'
      return false
    } catch {
      error.value = 'Failed to add item'
      return false
    } finally {
      saving.value = false
    }
  }

  async function removeItem(listId: string, itemId: string): Promise<boolean> {
    saving.value = true
    error.value = null
    try {
      const result = await wishlistApi.removeWishlistItem(listId, itemId)
      if (result.isSuccess) {
        upsertDetail(result.value)
        return true
      }
      error.value = result.message ?? result.errors[0]?.message ?? 'Failed to remove item'
      return false
    } catch {
      error.value = 'Failed to remove item'
      return false
    } finally {
      saving.value = false
    }
  }

  async function fetchWishlistedIds(): Promise<void> {
    try {
      const result = await wishlistApi.getWishlists()
      if (!result.isSuccess) return
      const ids = new Set<string>()
      for (const wl of result.items) {
        const detail = await wishlistApi.getWishlist(wl.id)
        if (detail.isSuccess) {
          for (const item of detail.value.wishedItems) {
            ids.add(item.variantId)
          }
        }
      }
      wishlistedVariantIds.value = ids
    } catch {
      // Silently ignore — wishlist state is best-effort
    }
  }

  async function toggleWishlist(variantId: string): Promise<boolean> {
    if (wishlistedVariantIds.value.has(variantId)) {
      // Remove: find the wishlist and item to remove
      const wishlists = await wishlistApi.getWishlists()
      if (wishlists.isSuccess) {
        for (const wl of wishlists.items) {
          const detail = await wishlistApi.getWishlist(wl.id)
          if (detail.isSuccess) {
            const item = detail.value.wishedItems.find(i => i.variantId === variantId)
            if (item) {
              await wishlistApi.removeWishlistItem(wl.id, item.id)
              const next = new Set(wishlistedVariantIds.value)
              next.delete(variantId)
              wishlistedVariantIds.value = next
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
        const created = await wishlistApi.createWishlist({ name: 'My Wishlist', isPrivate: false })
        if (created.isSuccess) targetId = created.value.id
      }
      if (targetId) {
        await wishlistApi.addWishlistItem(targetId, { variantId, quantity: 1 })
        const next = new Set(wishlistedVariantIds.value)
        next.add(variantId)
        wishlistedVariantIds.value = next
        return true
      }
    }
    return wishlistedVariantIds.value.has(variantId)
  }

  function isWishlisted(variantId: string): boolean {
    return wishlistedVariantIds.value.has(variantId)
  }

  return {
    items,
    loading,
    saving,
    error,
    details,
    detailLoadingId,
    fetchWishlists,
    fetchWishlist,
    createWishlist,
    updateWishlist,
    deleteWishlist,
    addItem,
    removeItem,
    wishlistedVariantIds,
    fetchWishlistedIds,
    toggleWishlist,
    isWishlisted,
  }
})
