import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { Wishlist } from '../types/entity/wishlist.entity'
import type { CreateWishlistRequest, AddWishlistItemRequest } from '../types/request/wishlist.request'
import { wishlistService } from '../services/wishlist/wishlist.service'

export const useWishlistStore = defineStore('wishlist', () => {
  const wishlists = ref<Wishlist[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchWishlists() {
    loading.value = true
    error.value = null
    const result = await wishlistService.getWishlists()
    loading.value = false
    if (result.isSuccess && result.data) {
      wishlists.value = result.data
    } else {
      error.value = result.message || 'Failed to load wishlists'
    }
  }

  async function createWishlist(data: CreateWishlistRequest) {
    const result = await wishlistService.createWishlist(data)
    if (result.isSuccess && result.data) {
      wishlists.value.push(result.data)
    }
    return result
  }

  async function deleteWishlist(id: string) {
    const result = await wishlistService.deleteWishlist(id)
    if (result.isSuccess) {
      wishlists.value = wishlists.value.filter(w => w.id !== id)
    }
    return result
  }

  async function addItem(wishlistId: string, data: AddWishlistItemRequest) {
    const result = await wishlistService.addItem(wishlistId, data)
    if (result.isSuccess) {
      await fetchWishlists() // Refresh to get updated item list
    }
    return result
  }

  async function removeItem(wishlistId: string, itemId: string) {
    const result = await wishlistService.removeItem(wishlistId, itemId)
    if (result.isSuccess) {
      await fetchWishlists()
    }
    return result
  }

  return { wishlists, loading, error, fetchWishlists, createWishlist, deleteWishlist, addItem, removeItem }
})
