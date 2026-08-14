import { ref, computed, reactive } from 'vue'
import { CartApi } from '../services/cartApi'
import { emit, on } from '@/shared/composables/useStoreEvents'
import type { CartLineItem, CheckoutState } from '../types'

// Module-level singleton state
const id = ref<string | null>(null)
const items = ref<CartLineItem[]>([])
const loading = ref(false)
const error = ref<string | null>(null)
const lastFetchedAt = ref(0)
// State: Checkout prefill fields persisted from the cart response.
const checkoutState = ref<CheckoutState | null>(null)
const shippingMethodId = ref<string | null>(null)
const shipAddressId = ref<string | null>(null)
const email = ref<string | null>(null)
const cartToken = crypto.randomUUID()

const itemCount = computed(() => items.value.reduce((s, i) => s + i.quantity, 0))
const subtotal = computed(() => items.value.reduce((s, i) => s + i.total, 0))
const isEmpty = computed(() => items.value.length === 0)

async function fetchCart(force = false): Promise<boolean> {
  if (loading.value) return false
  if (!force && Date.now() - lastFetchedAt.value < 30_000 && items.value.length > 0) return true
  loading.value = true
  error.value = null
  try {
    const result = await CartApi.getCart()
    if (result.isSuccess) {
      id.value = result.value.id
      items.value = result.value.items
      checkoutState.value = result.value.checkoutState
      shippingMethodId.value = result.value.shippingMethodId
      shipAddressId.value = result.value.shipAddressId
      email.value = result.value.email
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
      // State: Sync the checkout prefill fields from the updated cart response.
      checkoutState.value = result.value.checkoutState
      shippingMethodId.value = result.value.shippingMethodId
      shipAddressId.value = result.value.shipAddressId
      email.value = result.value.email
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
      // State: Sync the checkout prefill fields from the updated cart response.
      checkoutState.value = result.value.checkoutState
      shippingMethodId.value = result.value.shippingMethodId
      shipAddressId.value = result.value.shipAddressId
      email.value = result.value.email
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
      // State: Sync the checkout prefill fields from the updated cart response.
      checkoutState.value = result.value.checkoutState
      shippingMethodId.value = result.value.shippingMethodId
      shipAddressId.value = result.value.shipAddressId
      email.value = result.value.email
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
  try {
    await CartApi.emptyCart()
  } catch {
    // Swallow: the cart is cleared client-side regardless of the network outcome.
  }
  items.value = []
  id.value = null
  // State: Reset the checkout prefill fields with the cleared cart.
  checkoutState.value = null
  shippingMethodId.value = null
  shipAddressId.value = null
  email.value = null
  emit({ type: 'cart:updated', itemCount: 0 })
}

async function associateGuestCart(): Promise<void> {
  if (!id.value || id.value === '00000000-0000-0000-0000-000000000000') return
  try {
    await CartApi.associateCart(id.value)
    // Force: association may merge into a different user cart id, so bypass the cache.
    await fetchCart(true)
  } catch {
    // Swallow: guest-cart association is best-effort on login.
  }
}

function reset(): void {
  items.value = []
  id.value = null
  error.value = null
  // State: Reset the checkout prefill fields with the cart.
  checkoutState.value = null
  shippingMethodId.value = null
  shipAddressId.value = null
  email.value = null
}

// Subscribe: Re-associate guest cart on login, clear on logout.
on('auth:login', () => associateGuestCart())
on('auth:logout', () => reset())

let cartInstance: ReturnType<typeof createCart> | null = null

function createCart() {
  return reactive({
    id, items, loading, error, itemCount, subtotal, isEmpty, cartToken,
    checkoutState, shippingMethodId, shipAddressId, email,
    fetchCart, addItem, updateQuantity, removeItem, clearCart, associateGuestCart, reset,
  })
}

export function useCart() {
  if (!cartInstance) cartInstance = createCart()
  return cartInstance
}
