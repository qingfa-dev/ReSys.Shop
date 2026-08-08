import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { CartApi } from '../services/cartApi'
import { emit, on } from '@/shared/composables/useStoreEvents'
import type { CartLineItem } from '../types'

// Store: Client-side cart state with optimistic updates and server sync.
export const useCartStore = defineStore('cart', () => {
  const id = ref<string | null>(null)
  const items = ref<CartLineItem[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const lastFetchedAt = ref(0)
  // Context: Unique token per cart instance for idempotent guest-to-auth association.
  const cartToken = crypto.randomUUID()

  const itemCount = computed(() => items.value.reduce((s, i) => s + i.quantity, 0))
  const subtotal = computed(() => items.value.reduce((s, i) => s + i.total, 0))
  const isEmpty = computed(() => items.value.length === 0)

  async function fetchCart(): Promise<boolean> {
    // Guard: Skip if already loading or cache is fresh (< 30 s with items).
    if (loading.value) return false
    if (Date.now() - lastFetchedAt.value < 30_000 && items.value.length > 0) return true
    loading.value = true
    error.value = null
    try {
      const result = await CartApi.getCart()
      if (result.isSuccess) {
        id.value = result.value.id
        items.value = result.value.items
        lastFetchedAt.value = Date.now()
        emit({ type: 'cart:updated', itemCount: itemCount.value })
      } else {
        error.value = result.message ?? 'Failed to load cart'
      }
      loading.value = false
      return result.isSuccess
    } catch {
      error.value = 'Failed to load cart'
      loading.value = false
      return false
    }
  }

  async function addItem(variantId: string, quantity = 1): Promise<boolean> {
    loading.value = true
    error.value = null
    try {
      const result = await CartApi.addItem({ variantId, quantity })
      if (result.isSuccess) {
        id.value = result.value.id
        items.value = result.value.items
        lastFetchedAt.value = Date.now()
        emit({ type: 'cart:updated', itemCount: itemCount.value })
      } else {
        error.value = result.message ?? 'Failed to add item'
      }
      loading.value = false
      return result.isSuccess
    } catch {
      error.value = 'Failed to add item'
      loading.value = false
      return false
    }
  }

  async function updateQuantity(lineItemId: string, quantity: number): Promise<boolean> {
    // Update: Optimistically mutate local state before API round-trip for instant UI feedback.
    const prev = items.value.find(i => i.id === lineItemId)
    if (prev) prev.quantity = quantity
    try {
      const result = await CartApi.updateItem(lineItemId, { quantity })
      if (result.isSuccess) {
        items.value = result.value.items
        emit({ type: 'cart:updated', itemCount: itemCount.value })
      } else if (prev) {
        // Fallback: Re-fetch server state to restore consistent data on failure.
        const refresh = await CartApi.getCart()
        if (refresh.isSuccess) {
          prev.quantity = refresh.value.items.find(i => i.id === lineItemId)?.quantity ?? prev.quantity
        }
        error.value = result.message
      }
      return result.isSuccess
    } catch {
      error.value = 'Failed to update quantity'
      return false
    }
  }

  async function removeItem(lineItemId: string): Promise<boolean> {
    // Update: Optimistically remove from local list before API call.
    const removed = items.value.filter(i => i.id !== lineItemId)
    items.value = removed
    try {
      const result = await CartApi.removeItem(lineItemId)
      if (!result.isSuccess) {
        error.value = result.message
        // Fallback: Re-fetch cart to reconcile local state with server.
        await fetchCart()
      } else {
        emit({ type: 'cart:updated', itemCount: itemCount.value })
      }
      return result.isSuccess
    } catch {
      error.value = 'Failed to remove item'
      await fetchCart()
      return false
    }
  }

  async function clearCart(): Promise<void> {
    // Update: Empty cart on server and reset local state.
    await CartApi.emptyCart()
    items.value = []
    id.value = null
    emit({ type: 'cart:updated', itemCount: 0 })
  }

  async function associateGuestCart(): Promise<void> {
    // Guard: Skip association for empty or zeroed guest cart IDs.
    if (!id.value || id.value === '00000000-0000-0000-0000-000000000000') return
    // Call: Link anonymous cart to authenticated user account on login.
    await CartApi.associateCart(id.value)
    await fetchCart()
  }

  function reset(): void {
    // Reset: Clear all local cart state on logout.
    items.value = []
    id.value = null
    error.value = null
  }

  // Subscribe: Re-associate guest cart on login, clear on logout.
  on('auth:login', () => associateGuestCart())
  on('auth:logout', () => reset())

  return {
    id, items, loading, error, itemCount, subtotal, isEmpty, cartToken,
    fetchCart, addItem, updateQuantity, removeItem, clearCart, associateGuestCart, reset,
  }
})
