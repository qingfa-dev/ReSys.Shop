import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export interface WishlistItem {
  id: string
  productId: string
  name: string
  slug: string
  price: number
  compareAtPrice?: number
  image: string
  brand: {
    id: string
    name: string
    slug: string
  }
  addedAt: string
}

const STORAGE_KEY = 'shop_wishlist'

export const useWishlistStore = defineStore('wishlist', () => {
  const items = ref<WishlistItem[]>([])
  const isLoading = ref(false)

  const count = computed(() => items.value.length)
  const isEmpty = computed(() => items.value.length === 0)
  const productIds = computed(() => new Set(items.value.map(i => i.productId)))

  function isWishlisted(productId: string): boolean {
    return productIds.value.has(productId)
  }

  function toggle(item: WishlistItem) {
    const existing = items.value.find(i => i.productId === item.productId)
    if (existing) {
      remove(item.productId)
    } else {
      add(item)
    }
  }

  function add(item: WishlistItem) {
    if (!isWishlisted(item.productId)) {
      items.value.push({
        ...item,
        addedAt: new Date().toISOString(),
      })
      persist()
    }
  }

  function remove(productId: string) {
    items.value = items.value.filter(i => i.productId !== productId)
    persist()
  }

  function clear() {
    items.value = []
    persist()
  }

  function persist() {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(items.value))
    } catch (e) {
      console.error('Failed to persist wishlist:', e)
    }
  }

  function hydrate() {
    try {
      const saved = localStorage.getItem(STORAGE_KEY)
      if (saved) {
        items.value = JSON.parse(saved) as WishlistItem[]
      }
    } catch (e) {
      console.error('Failed to hydrate wishlist:', e)
      items.value = []
    }
  }

  return {
    items,
    isLoading,
    count,
    isEmpty,
    productIds,
    isWishlisted,
    toggle,
    add,
    remove,
    clear,
    hydrate,
  }
})
