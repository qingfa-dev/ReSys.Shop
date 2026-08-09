import { ref, computed, reactive } from 'vue'
import { CartApi } from '../services/cartApi'
import { emit, on } from '@/shared/composables/useStoreEvents'
import type { CartLineItem } from '../types'

// Module-level singleton state
const id = ref<string | null>(null)
const items = ref<CartLineItem[]>([])
const loading = ref(false)
const error = ref<string | null>(null)
const lastFetchedAt = ref(0)
const cartToken = crypto.randomUUID()

const itemCount = computed(() => items.value.reduce((s, i) => s + i.quantity, 0))
const subtotal = computed(() => items.value.reduce((s, i) => s + i.total, 0))
const isEmpty = computed(() => items.value.length === 0)

async function fetchCart(): Promise<boolean> {
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
  const prev = items.value.find(i => i.id === lineItemId)
  if (prev) prev.quantity = quantity
  try {
    const result = await CartApi.updateItem(lineItemId, { quantity })
    if (result.isSuccess) {
      items.value = result.value.items
      emit({ type: 'cart:updated', itemCount: itemCount.value })
    } else if (prev) {
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
  const removed = items.value.filter(i => i.id !== lineItemId)
  items.value = removed
  try {
    const result = await CartApi.removeItem(lineItemId)
    if (!result.isSuccess) {
      error.value = result.message
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
  await CartApi.emptyCart()
  items.value = []
  id.value = null
  emit({ type: 'cart:updated', itemCount: 0 })
}

async function associateGuestCart(): Promise<void> {
  if (!id.value || id.value === '00000000-0000-0000-0000-000000000000') return
  await CartApi.associateCart(id.value)
  await fetchCart()
}

function reset(): void {
  items.value = []
  id.value = null
  error.value = null
}

// Subscribe: Re-associate guest cart on login, clear on logout.
on('auth:login', () => associateGuestCart())
on('auth:logout', () => reset())

export function useCart() {
  return reactive({
    id, items, loading, error, itemCount, subtotal, isEmpty, cartToken,
    fetchCart, addItem, updateQuantity, removeItem, clearCart, associateGuestCart, reset,
  })
}
