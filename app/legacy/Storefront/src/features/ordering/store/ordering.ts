import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Cart, Order, Address, CheckoutRequest, ShippingMethod, PaymentMethod } from '../types'
import { cartService } from '../services/cart/cart.service'
import { orderService } from '../services/order/order.service'
import { addressService } from '../services/address/address.service'
import { shippingMethodService } from '../services/shipping-method/shipping-method.service'
import { paymentMethodService } from '../services/payment-method/payment-method.service'

const CART_TOKEN_KEY = 'cartToken'

function ensureCartToken(): string {
  let token = localStorage.getItem(CART_TOKEN_KEY)
  if (!token) {
    token = crypto.randomUUID()
    localStorage.setItem(CART_TOKEN_KEY, token)
  }
  return token
}

export const useCartStore = defineStore('cart', () => {
  const cart = ref<Cart | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  const items = computed(() => cart.value?.items ?? [])
  const itemCount = computed(() => items.value.reduce((sum, item) => sum + item.quantity, 0))
  const subtotal = computed(() => cart.value?.subtotal ?? 0)
  const tax = computed(() => cart.value?.tax ?? 0)
  const shipping = computed(() => cart.value?.shipping ?? 0)
  const discount = computed(() => cart.value?.discount ?? 0)
  const total = computed(() => cart.value?.total ?? 0)
  const isEmpty = computed(() => items.value.length === 0)

  async function fetchCart() {
    loading.value = true
    error.value = null
    try {
      const result = await cartService.getCart()
      if (result.isSuccess && result.data) {
        cart.value = result.data
      } else {
        error.value = result.message || 'Failed to fetch cart'
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to fetch cart'
    } finally {
      loading.value = false
    }
  }

  async function addItem(variantId: string, quantity = 1) {
    loading.value = true
    error.value = null
    try {
      const result = await cartService.addToCart(variantId, quantity)
      if (result.isSuccess && result.data) {
        cart.value = result.data
      } else {
        throw new Error(result.message || 'Failed to add item')
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to add item'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function updateItem(itemId: string, quantity: number) {
    try {
      const result = await cartService.updateCartItem(itemId, quantity)
      if (result.isSuccess && result.data) {
        cart.value = result.data
      } else {
        throw new Error(result.message || 'Failed to update item')
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to update item'
      throw e
    }
  }

  async function removeItem(itemId: string) {
    try {
      const result = await cartService.removeCartItem(itemId)
      if (result.isSuccess && result.data) {
        cart.value = result.data
      } else {
        throw new Error(result.message || 'Failed to remove item')
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to remove item'
      throw e
    }
  }

  async function clear() {
    try {
      const result = await cartService.clearCart()
      if (result.isSuccess && result.data) {
        cart.value = result.data
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to clear cart'
    }
  }

  return {
    cart,
    items,
    itemCount,
    subtotal,
    tax,
    shipping,
    discount,
    total,
    isEmpty,
    loading,
    error,
    ensureCartToken: () => ensureCartToken(),
    fetchCart,
    addItem,
    updateItem,
    removeItem,
    clear,
  }
})

export const useOrderStore = defineStore('order', () => {
  const orders = ref<Order[]>([])
  const currentOrder = ref<Order | null>(null)
  const addresses = ref<Address[]>([])
  const shippingMethods = ref<ShippingMethod[]>([])
  const paymentMethods = ref<PaymentMethod[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const pagination = ref({ page: 1, pageSize: 10, total: 0, totalPages: 0 })

  async function fetchOrders(page = 1, pageSize = 10) {
    loading.value = true
    try {
      const result = await orderService.getOrders({ page, pageSize })
      if (result.isSuccess) {
        orders.value = result.items
        pagination.value = {
          page: result.page,
          pageSize: result.pageSize,
          total: result.totalCount,
          totalPages: result.totalPages,
        }
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to fetch orders'
    } finally {
      loading.value = false
    }
  }

  async function fetchOrder(id: string) {
    loading.value = true
    try {
      const result = await orderService.getOrder(id)
      if (result.isSuccess && result.data) {
        currentOrder.value = result.data
      } else {
        error.value = result.message || 'Failed to fetch order'
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to fetch order'
    } finally {
      loading.value = false
    }
  }

  async function fetchCheckoutData() {
    loading.value = true
    try {
      const [addrResult, shippingResult, paymentResult] = await Promise.all([
        addressService.getAddresses(),
        shippingMethodService.getShippingMethods(),
        paymentMethodService.getPaymentMethods(),
      ])
      if (addrResult.isSuccess && addrResult.data) addresses.value = addrResult.data
      if (shippingResult.isSuccess && shippingResult.data) shippingMethods.value = shippingResult.data
      if (paymentResult.isSuccess && paymentResult.data) paymentMethods.value = paymentResult.data
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to fetch checkout data'
    } finally {
      loading.value = false
    }
  }

  async function checkout(request: CheckoutRequest): Promise<Order> {
    loading.value = true
    error.value = null
    try {
      const result = await orderService.checkout(request)
      if (result.isSuccess && result.data) {
        currentOrder.value = result.data
        return result.data
      } else {
        throw new Error(result.message || 'Checkout failed')
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Checkout failed'
      throw e
    } finally {
      loading.value = false
    }
  }

  return {
    orders,
    currentOrder,
    addresses,
    shippingMethods,
    paymentMethods,
    loading,
    error,
    pagination,
    fetchOrders,
    fetchOrder,
    fetchCheckoutData,
    checkout,
  }
})