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

  async function createWishlist(req: CreateWishlistRequest): Promise<boolean> {
    saving.value = true
    const r = await WishlistApi.createWishlist(req)
    if (r.isSuccess) lists.value.unshift(r.value)
    else error.value = r.message
    saving.value = false
    return r.isSuccess
  }

  async function updateWishlist(id: string, req: UpdateWishlistRequest): Promise<boolean> {
    saving.value = true
    const r = await WishlistApi.updateWishlist(id, req)
    if (r.isSuccess && details.value[id]) Object.assign(details.value[id], r.value)
    else error.value = r.message
    saving.value = false
    return r.isSuccess
  }

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

  on('auth:init-done', () => fetchWishlists())

  return {
    lists, loading, saving, error, details, wishlistedVariantIds,
    fetchWishlists, createWishlist, updateWishlist, deleteWishlist, addItem, removeItem,
  }
})
