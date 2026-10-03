import { computed } from 'vue'
import { useWishlistStore, type WishlistItem } from '../store/wishlist'
import { useAuthStore } from '@/features/identity/store/auth'

export function useWishlist() {
  const wishlistStore = useWishlistStore()
  const authStore = useAuthStore()

  const items = computed(() => wishlistStore.items)
  const count = computed(() => wishlistStore.count)
  const isEmpty = computed(() => wishlistStore.isEmpty)
  const isLoading = computed(() => wishlistStore.isLoading)

  function isWishlisted(productId: string): boolean {
    return wishlistStore.isWishlisted(productId)
  }

  function toggle(item: WishlistItem): boolean {
    if (!authStore.isAuthenticated) {
      return false
    }
    wishlistStore.toggle(item)
    return true
  }

  function add(item: WishlistItem): boolean {
    if (!authStore.isAuthenticated) {
      return false
    }
    wishlistStore.add(item)
    return true
  }

  function remove(productId: string) {
    wishlistStore.remove(productId)
  }

  function clear() {
    wishlistStore.clear()
  }

  return {
    items,
    count,
    isEmpty,
    isLoading,
    isWishlisted,
    toggle,
    add,
    remove,
    clear,
  }
}
