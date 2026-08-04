import { beforeEach, describe, expect, it, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useCartStore, useOrderStore } from '../ordering'
import type { Cart, Order, Address, CheckoutRequest, ShippingMethod, PaymentMethod } from '../../types'

vi.mock('../../services/cart/cart.service', () => ({
  cartService: {
    getCart: vi.fn(),
    addToCart: vi.fn(),
    updateCartItem: vi.fn(),
    removeCartItem: vi.fn(),
    clearCart: vi.fn(),
  },
}))

vi.mock('../../services/order/order.service', () => ({
  orderService: {
    getOrders: vi.fn(),
    getOrder: vi.fn(),
    checkout: vi.fn(),
    cancelOrder: vi.fn(),
  },
}))

vi.mock('../../services/address/address.service', () => ({
  addressService: {
    getAddresses: vi.fn(),
  },
}))

vi.mock('../../services/shipping-method/shipping-method.service', () => ({
  shippingMethodService: {
    getShippingMethods: vi.fn(),
  },
}))

vi.mock('../../services/payment-method/payment-method.service', () => ({
  paymentMethodService: {
    getPaymentMethods: vi.fn(),
  },
}))

import { cartService } from '../../services/cart/cart.service'
import { orderService } from '../../services/order/order.service'
import { addressService } from '../../services/address/address.service'
import { shippingMethodService } from '../../services/shipping-method/shipping-method.service'
import { paymentMethodService } from '../../services/payment-method/payment-method.service'

describe('useCartStore', () => {
  const mockCart = {
    items: [
      { id: 'item-1', productId: 'prod-1', productName: 'Product 1', productImage: '/img1.jpg', quantity: 2, price: 29.99 },
      { id: 'item-2', productId: 'prod-2', productName: 'Product 2', productImage: '/img2.jpg', quantity: 1, price: 49.99 },
    ],
    subtotal: 109.97,
    tax: 8.80,
    shipping: 10.00,
    discount: 0,
    total: 128.77,
  }

  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('state', () => {
    it('should initialize with default values', () => {
      const store = useCartStore()
      expect(store.cart).toBeNull()
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
    })
  })

  describe('computed', () => {
    it('should compute items', () => {
      const store = useCartStore()
      store.cart = mockCart
      expect(store.items).toEqual(mockCart.items)
    })

    it('should compute itemCount', () => {
      const store = useCartStore()
      store.cart = mockCart
      expect(store.itemCount).toBe(3)
    })

    it('should compute subtotal', () => {
      const store = useCartStore()
      store.cart = mockCart
      expect(store.subtotal).toBe(109.97)
    })

    it('should compute tax', () => {
      const store = useCartStore()
      store.cart = mockCart
      expect(store.tax).toBe(8.80)
    })

    it('should compute shipping', () => {
      const store = useCartStore()
      store.cart = mockCart
      expect(store.shipping).toBe(10)
    })

    it('should compute discount', () => {
      const store = useCartStore()
      store.cart = mockCart
      expect(store.discount).toBe(0)
    })

    it('should compute total', () => {
      const store = useCartStore()
      store.cart = mockCart
      expect(store.total).toBe(128.77)
    })

    it('should compute isEmpty', () => {
      const store = useCartStore()
      expect(store.isEmpty).toBe(true)
      store.cart = mockCart
      expect(store.isEmpty).toBe(false)
    })
  })

  describe('fetchCart', () => {
    it('should fetch cart successfully', async () => {
      const mockResult = { isSuccess: true, isFailure: false, statusCode: 200, data: mockCart }
      vi.mocked(cartService.getCart).mockResolvedValue(mockResult)

      const store = useCartStore()
      await store.fetchCart()

      expect(store.cart).toEqual(mockCart)
      expect(store.loading).toBe(false)
    })

    it('should handle error', async () => {
      const mockResult = { isSuccess: false, isFailure: true, statusCode: 500, message: 'Failed', errors: [] }
      vi.mocked(cartService.getCart).mockResolvedValue(mockResult)

      const store = useCartStore()
      await store.fetchCart()

      expect(store.error).toBe('Failed')
    })
  })

  describe('addItem', () => {
    it('should add item successfully', async () => {
      const mockResult = { isSuccess: true, isFailure: false, statusCode: 200, data: mockCart }
      vi.mocked(cartService.addToCart).mockResolvedValue(mockResult)

      const store = useCartStore()
      await store.addItem('prod-1', 'Product 1', '/img1.jpg', 2, 29.99)

      expect(store.cart).toEqual(mockCart)
    })

    it('should throw on error', async () => {
      const mockResult = { isSuccess: false, isFailure: true, statusCode: 400, message: 'Out of stock', errors: [] }
      vi.mocked(cartService.addToCart).mockResolvedValue(mockResult)

      const store = useCartStore()
      const fn = () => store.addItem('prod-1', 'Product 1', '/img1.jpg', 2, 29.99)

      await expect(fn).rejects.toThrow()
    })
  })

  describe('updateItem', () => {
    it('should update item successfully', async () => {
      const mockResult = { isSuccess: true, isFailure: false, statusCode: 200, data: mockCart }
      vi.mocked(cartService.updateCartItem).mockResolvedValue(mockResult)

      const store = useCartStore()
      await store.updateItem('item-1', 5)

      expect(store.cart).toEqual(mockCart)
    })

    it('should throw on error', async () => {
      const mockResult = { isSuccess: false, isFailure: true, statusCode: 400, message: 'Invalid quantity', errors: [] }
      vi.mocked(cartService.updateCartItem).mockResolvedValue(mockResult)

      const store = useCartStore()
      const fn = () => store.updateItem('item-1', 0)

      await expect(fn).rejects.toThrow()
    })
  })

  describe('removeItem', () => {
    it('should remove item successfully', async () => {
      const mockResult = { isSuccess: true, isFailure: false, statusCode: 200, data: mockCart }
      vi.mocked(cartService.removeCartItem).mockResolvedValue(mockResult)

      const store = useCartStore()
      await store.removeItem('item-1')

      expect(store.cart).toEqual(mockCart)
    })

    it('should throw on error', async () => {
      const mockResult = { isSuccess: false, isFailure: true, statusCode: 400, message: 'Not found', errors: [] }
      vi.mocked(cartService.removeCartItem).mockResolvedValue(mockResult)

      const store = useCartStore()
      const fn = () => store.removeItem('invalid')

      await expect(fn).rejects.toThrow()
    })
  })

  describe('clear', () => {
    it('should clear cart', async () => {
      const emptyCart = { items: [], subtotal: 0, tax: 0, shipping: 0, discount: 0, total: 0 }
      const mockResult = { isSuccess: true, isFailure: false, statusCode: 200, data: emptyCart }
      vi.mocked(cartService.clearCart).mockResolvedValue(mockResult)

      const store = useCartStore()
      await store.clear()

      expect(store.cart).toEqual(emptyCart)
    })
  })

})

describe('useOrderStore', () => {
  const mockOrder = {
    id: 'order-1',
    orderNumber: 'ORD-001',
    status: 'pending' as const,
    items: [],
    shippingAddress: { id: 'addr-1', firstName: 'John', lastName: 'Doe', address1: '123 Main St', city: 'NYC', state: 'NY', postalCode: '10001', country: 'US' },
    billingAddress: { id: 'addr-1', firstName: 'John', lastName: 'Doe', address1: '123 Main St', city: 'NYC', state: 'NY', postalCode: '10001', country: 'US' },
    subtotal: 100,
    tax: 10,
    shipping: 5,
    discount: 0,
    total: 115,
    currency: 'USD',
    createdAt: '2024-01-01',
    updatedAt: '2024-01-01',
  }

  const mockAddress = {
    id: 'addr-1',
    firstName: 'John',
    lastName: 'Doe',
    address1: '123 Main St',
    city: 'NYC',
    state: 'NY',
    postalCode: '10001',
    country: 'US',
    phone: '+1234567890',
    isDefault: true,
  }

  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('state', () => {
    it('should initialize with default values', () => {
      const store = useOrderStore()
      expect(store.orders).toEqual([])
      expect(store.currentOrder).toBeNull()
      expect(store.addresses).toEqual([])
      expect(store.shippingMethods).toEqual([])
      expect(store.paymentMethods).toEqual([])
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
      expect(store.pagination).toEqual({ page: 1, pageSize: 10, total: 0, totalPages: 0 })
    })
  })

  describe('fetchOrders', () => {
    it('should fetch orders successfully', async () => {
      const mockResult = {
        isSuccess: true,
        isFailure: false,
        statusCode: 200,
        items: [mockOrder],
        page: 1,
        pageSize: 10,
        totalCount: 1,
        totalPages: 1,
        hasNextPage: false,
        hasPreviousPage: false,
      }
      vi.mocked(orderService.getOrders).mockResolvedValue(mockResult)

      const store = useOrderStore()
      await store.fetchOrders()

      expect(store.orders).toEqual([mockOrder])
      expect(store.pagination).toEqual({ page: 1, pageSize: 10, total: 1, totalPages: 1 })
    })

    it('should handle error from exception', async () => {
      vi.mocked(orderService.getOrders).mockRejectedValue(new Error('Network error'))

      const store = useOrderStore()
      await store.fetchOrders()

      expect(store.error).toBe('Network error')
    })
  })

  describe('fetchOrder', () => {
    it('should fetch single order successfully', async () => {
      const mockResult = { isSuccess: true, isFailure: false, statusCode: 200, data: mockOrder } as const
      vi.mocked(orderService.getOrder).mockResolvedValue(mockResult)

      const store = useOrderStore()
      await store.fetchOrder('order-1')

      expect(store.currentOrder).toEqual(mockOrder)
    })

    it('should handle error', async () => {
      const mockResult = { isSuccess: false, isFailure: true, statusCode: 404, message: 'Not found', errors: [] }
      vi.mocked(orderService.getOrder).mockResolvedValue(mockResult)

      const store = useOrderStore()
      await store.fetchOrder('order-1')

      expect(store.error).toBe('Not found')
    })
  })

  describe('fetchCheckoutData', () => {
    it('should fetch all checkout data', async () => {
      const addrResult = { isSuccess: true, isFailure: false, statusCode: 200, data: [mockAddress] }
      const shippingResult = { isSuccess: true, isFailure: false, statusCode: 200, data: [{ id: 'ship-1', name: 'Standard', description: 'Standard shipping', price: 5, estimatedDays: 3 }] }
      const paymentResult = { isSuccess: true, isFailure: false, statusCode: 200, data: [{ id: 'pm-1', name: 'Credit Card', type: 'card' as const, last4: '4242', brand: 'Visa', expiryMonth: 12, expiryYear: 2025 }] }

      vi.mocked(addressService.getAddresses).mockResolvedValue(addrResult)
      vi.mocked(shippingMethodService.getShippingMethods).mockResolvedValue(shippingResult)
      vi.mocked(paymentMethodService.getPaymentMethods).mockResolvedValue(paymentResult)

      const store = useOrderStore()
      await store.fetchCheckoutData()

      expect(store.addresses).toEqual([mockAddress])
      expect(store.shippingMethods).toHaveLength(1)
      expect(store.paymentMethods).toHaveLength(1)
    })
  })

  describe('checkout', () => {
    it('should checkout successfully', async () => {
      const mockResult = { isSuccess: true, isFailure: false, statusCode: 200, data: mockOrder } as const
      vi.mocked(orderService.checkout).mockResolvedValue(mockResult)

      const store = useOrderStore()
      const result = await store.checkout({} as CheckoutRequest)

      expect(result).toEqual(mockOrder)
      expect(store.currentOrder).toEqual(mockOrder)
    })

    it('should throw on error', async () => {
      const mockResult = { isSuccess: false, isFailure: true, statusCode: 400, message: 'Checkout failed', errors: [] }
      vi.mocked(orderService.checkout).mockResolvedValue(mockResult)

      const store = useOrderStore()
      const fn = () => store.checkout({} as CheckoutRequest)

      await expect(fn).rejects.toThrow()
      expect(store.error).toBe('Checkout failed')
    })
  })

  describe('cancelOrder', () => {
    it('should cancel order successfully', async () => {
      const mockResult = { isSuccess: true, isFailure: false, statusCode: 200, data: { ...mockOrder, status: 'cancelled' as const } } as const
      vi.mocked(orderService.cancelOrder).mockResolvedValue(mockResult)

      const store = useOrderStore()
      await store.cancelOrder('order-1')

      expect(store.currentOrder?.status).toBe('cancelled')
    })

    it('should throw on error', async () => {
      const mockResult = { isSuccess: false, isFailure: true, statusCode: 400, message: 'Cannot cancel', errors: [] }
      vi.mocked(orderService.cancelOrder).mockResolvedValue(mockResult)

      const store = useOrderStore()
      const fn = () => store.cancelOrder('order-1')

      await expect(fn).rejects.toThrow()
    })
  })
})