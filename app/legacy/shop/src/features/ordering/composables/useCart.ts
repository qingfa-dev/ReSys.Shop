import { computed } from 'vue'
import { useCartStore, useOrderStore } from '@/features/ordering/store/ordering'

export function useCart() {
  const cartStore = useCartStore()

  const items = computed(() => cartStore.items)
  const itemCount = computed(() => cartStore.itemCount)
  const isEmpty = computed(() => cartStore.isEmpty)
  const subtotal = computed(() => cartStore.subtotal)
  const total = computed(() => cartStore.total)
  const isLoading = computed(() => cartStore.loading)

  async function loadCart() {
    await cartStore.fetchCart()
  }

  async function addToCart(productId: string, productName: string, productImage: string, quantity = 1, price: number) {
    await cartStore.addItem(productId, productName, productImage, quantity, price)
  }

  async function updateQuantity(itemId: string, quantity: number) {
    await cartStore.updateItem(itemId, quantity)
  }

  async function removeFromCart(itemId: string) {
    await cartStore.removeItem(itemId)
  }

  async function clearCart() {
    await cartStore.clear()
  }

  async function applyCoupon(code: string) {
    await cartStore.applyCoupon(code)
  }

  return {
    items,
    itemCount,
    isEmpty,
    subtotal,
    total,
    isLoading,
    loadCart,
    addToCart,
    updateQuantity,
    removeFromCart,
    clearCart,
    applyCoupon,
  }
}

export function useOrder() {
  const store = useOrderStore()

  const orders = computed(() => store.orders)
  const currentOrder = computed(() => store.currentOrder)
  const addresses = computed(() => store.addresses)
  const shippingMethods = computed(() => store.shippingMethods)
  const paymentMethods = computed(() => store.paymentMethods)
  const isLoading = computed(() => store.loading)

  async function loadOrders(page = 1) {
    await store.fetchOrders(page)
  }

  async function loadOrder(id: string) {
    await store.fetchOrder(id)
  }

  async function loadCheckoutData() {
    await store.fetchCheckoutData()
  }

  async function placeOrder(request: Parameters<typeof store.checkout>[0]) {
    return await store.checkout(request)
  }

  async function cancelOrder(id: string) {
    await store.cancelOrder(id)
  }

  return {
    orders,
    currentOrder,
    addresses,
    shippingMethods,
    paymentMethods,
    isLoading,
    loadOrders,
    loadOrder,
    loadCheckoutData,
    placeOrder,
    cancelOrder,
  }
}
