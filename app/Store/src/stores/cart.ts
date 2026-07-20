import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

/** An item in the shopping cart. */
export interface CartItem {
  /** Unique product identifier. */
  id: string
  /** Display name. */
  name: string
  /** Unit price in cents or base currency unit. */
  price: number
  /** Product image URL. */
  image: string
  /** Number of units in the cart. */
  quantity: number
}

/** Manage the shopping cart: items, totals, and drawer visibility. */
export const useCartStore = defineStore('cart', () => {
  /** Cart line items. */
  const items = ref<CartItem[]>([])
  /** Whether the cart drawer is visible. */
  const isOpen = ref(false)

  /** Total number of units across all items. */
  const itemCount = computed(() =>
    items.value.reduce((sum, item) => sum + item.quantity, 0),
  )

  /** Sum of price × quantity for all items before adjustments. */
  const subtotal = computed(() =>
    items.value.reduce((sum, item) => sum + item.price * item.quantity, 0),
  )

  /** Grand total (equals subtotal when no discounts or shipping apply). */
  const total = computed(() => subtotal.value)

  /** Add an item. Increments quantity if it already exists. */
  function addItem(item: CartItem) {
    const existing = items.value.find((i) => i.id === item.id)
    if (existing) {
      existing.quantity += item.quantity
    } else {
      items.value.push({ ...item })
    }
  }

  /** Remove an item by ID. */
  function removeItem(id: string) {
    items.value = items.value.filter((i) => i.id !== id)
  }

  /** Update quantity for an item. Removes the item when quantity reaches 0. */
  function updateQuantity(id: string, quantity: number) {
    const item = items.value.find((i) => i.id === id)
    if (item) {
      item.quantity = Math.max(0, quantity)
      if (item.quantity === 0) {
        removeItem(id)
      }
    }
  }

  /** Remove all items from the cart. */
  function clearCart() {
    items.value = []
  }

  /** Toggle the cart drawer open/closed. */
  function toggleCart() {
    isOpen.value = !isOpen.value
  }

  return {
    items,
    isOpen,
    itemCount,
    subtotal,
    total,
    addItem,
    removeItem,
    updateQuantity,
    clearCart,
    toggleCart,
  }
})
