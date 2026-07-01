import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export interface CartItem {
  id: string
  name: string
  price: number
  image: string
  quantity: number
}

export const useCartStore = defineStore('cart', () => {
  const items = ref<CartItem[]>([])
  const isOpen = ref(false)

  const itemCount = computed(() =>
    items.value.reduce((sum, item) => sum + item.quantity, 0),
  )

  const subtotal = computed(() =>
    items.value.reduce((sum, item) => sum + item.price * item.quantity, 0),
  )

  const total = computed(() => subtotal.value)

  function addItem(item: CartItem) {
    const existing = items.value.find((i) => i.id === item.id)
    if (existing) {
      existing.quantity += item.quantity
    } else {
      items.value.push({ ...item })
    }
  }

  function removeItem(id: string) {
    items.value = items.value.filter((i) => i.id !== id)
  }

  function updateQuantity(id: string, quantity: number) {
    const item = items.value.find((i) => i.id === id)
    if (item) {
      item.quantity = Math.max(0, quantity)
      if (item.quantity === 0) {
        removeItem(id)
      }
    }
  }

  function clearCart() {
    items.value = []
  }

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
