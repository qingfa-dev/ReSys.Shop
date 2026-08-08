import { defineStore } from 'pinia'
import { ref } from 'vue'
import { WishlistApi } from '../services/wishlistApi'
import { on } from '@/shared/composables/useStoreEvents'
import type { WishlistListItem, WishlistDetail, CreateWishlistRequest, UpdateWishlistRequest, AddWishlistItemRequest } from '../types'

export const useWishlistStore = defineStore('wishlists', () => {
  const lists = ref<WishlistListItem[]>([])
  const loading = ref(false)
  const saving = ref(false)
  const error = ref<string | null>(null)
  const details = ref<Record<string, WishlistDetail>>({})
  const wishlistedVariantIds = ref<Set<string>>(new Set())

  // Fetch: Eagerly load all list details to build a flat variant-id lookup set
  async function fetchWishlists(): Promise<void> {
    loading.value = true
    error.value = null
    const result = await WishlistApi.getWishlists()
    if (result.isSuccess) {
      lists.value = result.items
      const ids = new Set<string>()
      for (const list of result.items) {
        const detail = await WishlistApi.getWishlist(list.id)
        if (detail.isSuccess) {
          details.value[list.id] = detail.value
          for (const item of detail.value.wishedItems) ids.add(item.variantId)
        }
      }
      wishlistedVariantIds.value = ids
    } else {
      error.value = result.message
    }
    loading.value = false
  }

  // Create: Prepend new list to front for immediate visibility
  async function createWishlist(req: CreateWishlistRequest): Promise<boolean> {
    saving.value = true
    const r = await WishlistApi.createWishlist(req)
    if (r.isSuccess) lists.value.unshift(r.value)
    else error.value = r.message
    saving.value = false
    return r.isSuccess
  }

  // Update: Merge server response into existing detail cache entry
  async function updateWishlist(id: string, req: UpdateWishlistRequest): Promise<boolean> {
    saving.value = true
    const r = await WishlistApi.updateWishlist(id, req)
    if (r.isSuccess && details.value[id]) Object.assign(details.value[id], r.value)
    else error.value = r.message
    saving.value = false
    return r.isSuccess
  }

  // Delete: Remove from list array and detail cache atomically
  async function deleteWishlist(id: string): Promise<boolean> {
    saving.value = true
    const r = await WishlistApi.deleteWishlist(id)
    if (r.isSuccess) {
      lists.value = lists.value.filter(l => l.id !== id)
      delete details.value[id]
    } else {
      error.value = r.message
    }
    saving.value = false
    return r.isSuccess
  }

  // Add: Update detail and track variant in global wishlisted set
  async function addItem(listId: string, req: AddWishlistItemRequest): Promise<boolean> {
    saving.value = true
    const r = await WishlistApi.addWishlistItem(listId, req)
    if (r.isSuccess) {
      details.value[listId] = r.value
      wishlistedVariantIds.value.add(req.variantId)
    } else {
      error.value = r.message
    }
    saving.value = false
    return r.isSuccess
  }

  // Remove: Refresh detail and re-sync variant tracking set
  async function removeItem(listId: string, itemId: string): Promise<boolean> {
    saving.value = true
    const r = await WishlistApi.removeWishlistItem(listId, itemId)
    if (r.isSuccess) {
      details.value[listId] = r.value
      await fetchWishlists()
    } else {
      error.value = r.message
    }
    saving.value = false
    return r.isSuccess
  }

  // Subscribe: Load wishlists after identity session initializes
  on('auth:init-done', () => fetchWishlists())

  return {
    lists, loading, saving, error, details, wishlistedVariantIds,
    fetchWishlists, createWishlist, updateWishlist, deleteWishlist, addItem, removeItem,
  }
})
