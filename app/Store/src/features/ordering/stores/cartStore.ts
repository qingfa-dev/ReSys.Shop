import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { STORAGE_KEYS } from '@/shared/constants/storage'
import type { CartLineItem, CartResponse } from '../types/cart'
import * as cartApi from '../services/cartApi'

export const useCartStore = defineStore('cart', () => {
  const id = ref<string | null>(null)
  const items = ref<CartLineItem[]>([])
  const currency = ref('VND')
  const loading = ref(false)
  const error = ref<string | null>(null)

  const itemCount = computed(() => items.value.reduce((sum, i) => sum + i.quantity, 0))
  const subtotal = computed(() => items.value.reduce((sum, i) => sum + i.unitPrice * i.quantity, 0))

  function getCartToken(): string {
    let token = localStorage.getItem(STORAGE_KEYS.CART_TOKEN)
    if (!token) {
      token = crypto.randomUUID()
      localStorage.setItem(STORAGE_KEYS.CART_TOKEN, token)
    }
    return token
  }

  async function fetchCart(): Promise<void> {
    loading.value = true
    error.value = null
    const result = await cartApi.getCart()
    if (result.isSuccess) {
      applyCart(result.value)
    } else {
      error.value = result.message ?? 'Failed to load cart'
    }
    loading.value = false
  }

  function applyCart(cart: CartResponse): void {
    id.value = cart.id
    items.value = cart.items
    currency.value = cart.currency
  }

  async function addItem(variantId: string, quantity = 1): Promise<boolean> {
    const result = await cartApi.addItem({ variantId, quantity })
    if (result.isSuccess) { applyCart(result.value); return true }
    error.value = result.message ?? 'Failed to add item'
    return false
  }

  async function updateQuantity(lineItemId: string, quantity: number): Promise<boolean> {
    const result = await cartApi.updateItem(lineItemId, { quantity })
    if (result.isSuccess) { applyCart(result.value); return true }
    error.value = result.message ?? 'Failed to update quantity'
    return false
  }

  async function removeItem(lineItemId: string): Promise<boolean> {
    const result = await cartApi.removeItem(lineItemId)
    if (result.isSuccess) { applyCart(result.value); return true }
    error.value = result.message ?? 'Failed to remove item'
    return false
  }

  async function clearCart(): Promise<void> {
    await cartApi.emptyCart()
    items.value = []
  }

  async function associate(): Promise<void> {
    const result = await cartApi.associateCart()
    if (result.isSuccess) applyCart(result.value)
  }

  function reset(): void {
    id.value = null
    items.value = []
    error.value = null
  }

  return { id, items, currency, loading, error, itemCount, subtotal, getCartToken, fetchCart, addItem, updateQuantity, removeItem, clearCart, associate, reset }
})
