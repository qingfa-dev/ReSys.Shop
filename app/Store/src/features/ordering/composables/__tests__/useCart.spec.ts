import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useCart } from '../useCart'
import { CartApi } from '../../services/cartApi'
import { ok } from '@/shared/types/result'
import type { CartLineItem, CartResponse } from '../../types'

// Stub: CartApi so the composable does not make real HTTP calls.
vi.mock('../../services/cartApi', () => ({
  CartApi: {
    getCart: vi.fn<() => Promise<unknown>>(),
    addItem: vi.fn<() => Promise<unknown>>(),
    updateItem: vi.fn<() => Promise<unknown>>(),
    removeItem: vi.fn<() => Promise<unknown>>(),
    emptyCart: vi.fn<() => Promise<unknown>>(),
    associateCart: vi.fn<() => Promise<unknown>>(),
  },
}))

const mockedCartApi = vi.mocked(CartApi)

// Fixture: Line item matching the CartLineItem contract.
const lineItem: CartLineItem = {
  id: 'li-1',
  variantId: 'v-1', productId: null,
  variantName: 'Classic Tee / Red / M',
  sku: 'CT-001-R-M',
  productName: 'Classic Tee',
  productImageUrl: '/img/tee.jpg',
  quantity: 2,
  price: 45,
  total: 90,
}

// Fixture: Cart carrying non-zero server totals for the sync assertions.
const serverCart: CartResponse = {
  id: 'cart-1',
  items: [lineItem],
  itemTotal: 90,
  shipmentTotal: 9.99,
  adjustmentTotal: 5,
  total: 104.99,
  currency: 'USD',
  itemCount: 2,
  checkoutState: 'Address',
  shippingMethodId: 'sm-standard',
  shipAddressId: null,
  email: 'ada@example.com',
  shippingAdjustment: null, shippingCalculation: null, adjustments: [],
}

describe('useCart', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    // Reset: The singleton refs persist across tests in this module.
    useCart().reset()
  })

  it('syncs server totals from a cart response and resets them on clearCart', async () => {
    mockedCartApi.getCart.mockResolvedValue(ok(serverCart))
    mockedCartApi.emptyCart.mockResolvedValue(ok(null))
    const cart = useCart()

    await cart.fetchCart(true)

    expect(cart.shipping).toBe(9.99)
    expect(cart.adjustments).toBe(5)
    expect(cart.total).toBe(104.99)
    expect(cart.itemTotal).toBe(90)

    await cart.clearCart()

    expect(cart.shipping).toBe(0)
    expect(cart.adjustments).toBe(0)
    expect(cart.total).toBe(0)
    expect(cart.items).toHaveLength(0)
  })
})
