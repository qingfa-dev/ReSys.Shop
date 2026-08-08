import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createTestingPinia } from '@pinia/testing'
import { setActivePinia } from 'pinia'
import { ok, failure, pagedOk } from '@/shared/types/result'
import { useCartStore } from '../cartStore'
import * as cartApi from '../../services/cartApi'
import * as cartReservationApi from '@/features/inventory/services/cartReservationApi'
import type { CartLineItem, CartResponse } from '../../types/cart'

const mockedCartApi = vi.mocked(cartApi) as any
const mockedReservationApi = vi.mocked(cartReservationApi) as any

vi.mock('@/features/ordering/services/cartApi', () => ({
  CartApi: {
    getCart: vi.fn<() => Promise<void>>(),
    addItem: vi.fn<() => Promise<void>>(),
    updateItem: vi.fn<() => Promise<void>>(),
    removeItem: vi.fn<() => Promise<void>>(),
    emptyCart: vi.fn<() => Promise<void>>(),
    associateCart: vi.fn<() => Promise<void>>(),
  },
}))

vi.mock('@/features/inventory/services/cartReservationApi', () => ({
  reserveStock: vi.fn<() => Promise<void>>(),
  releaseReservation: vi.fn<() => Promise<void>>(),
  getCartReservations: vi.fn<() => Promise<void>>(),
}))

const baseItem: CartLineItem = {
  id: 'li-1',
  variantId: 'v-1',
  variantName: 'SKU-1',
  sku: 'SKU-1',
  productName: 'Hex Bolt',
  productImageUrl: null,
  quantity: 2,
  price: 50000,
  total: 100000,
}

function item(overrides: Partial<CartLineItem>): CartLineItem {
  const merged = { ...baseItem, ...overrides }
  return { ...merged, total: merged.price * merged.quantity }
}

function cart(overrides: Partial<CartResponse>): CartResponse {
  return {
    id: 'cart-1',
    itemTotal: 100000,
    total: 100000,
    currency: 'VND',
    itemCount: 2,
    checkoutState: 'address',
    items: [baseItem],
    ...overrides,
  }
}

describe('cartStore', () => {
  beforeEach(() => {
    setActivePinia(createTestingPinia({ stubActions: false, createSpy: vi.fn }))
    vi.clearAllMocks()
    mockedReservationApi.reserveStock.mockResolvedValue(ok({ id: 'res-1', variantId: '', stockLocationId: null, orderId: null, quantity: 1, state: 'Reserved', expiresAtUtc: '', reason: null, createdAtUtc: '', modifiedAtUtc: null }))
    mockedReservationApi.releaseReservation.mockResolvedValue(ok(null))
    mockedReservationApi.getCartReservations.mockResolvedValue(pagedOk([], 1, 20, 0))
  })

  it('fetchCart applies id and items on success', async () => {
    const store = useCartStore()
    const payload = cart({})
    mockedCartApi.CartApi.getCart.mockResolvedValue(ok(payload))

    await store.fetchCart()

    expect(store.id).toBe('cart-1')
    expect(store.items).toEqual([baseItem])
    expect(store.loading).toBe(false)
    expect(store.error).toBeNull()
  })

  it('fetchCart sets an error on failure', async () => {
    const store = useCartStore()
    mockedCartApi.CartApi.getCart.mockResolvedValue(
      failure({ code: 'Cart.LoadFailed', message: 'Cart unavailable', type: 500 }),
    )

    await store.fetchCart()

    expect(store.error).toBe('Cart unavailable')
    expect(store.loading).toBe(false)
  })

  it('fetchCart sets an error and clears loading when the request throws', async () => {
    const store = useCartStore()
    mockedCartApi.CartApi.getCart.mockRejectedValue(new Error('network down'))

    await store.fetchCart()

    expect(store.error).toBe('Failed to load cart')
    expect(store.loading).toBe(false)
  })

  it('addItem sets an error and returns false when the request throws', async () => {
    const store = useCartStore()
    mockedCartApi.CartApi.addItem.mockRejectedValue(new Error('network down'))

    const success = await store.addItem('v-1')

    expect(success).toBe(false)
    expect(store.error).toBe('Failed to add item')
  })

  it('computes subtotal as the sum of line totals', async () => {
    const store = useCartStore()
    mockedCartApi.CartApi.getCart.mockResolvedValue(
      ok(
        cart({
          items: [
            item({ id: 'a', quantity: 1, price: 10000 }),
            item({ id: 'b', quantity: 3, price: 20000 }),
          ],
        }),
      ),
    )

    await store.fetchCart()

    expect(store.subtotal).toBe(70000)
  })

  it('computes itemCount as the sum of quantities', async () => {
    const store = useCartStore()
    mockedCartApi.CartApi.getCart.mockResolvedValue(
      ok(
        cart({
          items: [
            item({ id: 'a', quantity: 2 }),
            item({ id: 'b', quantity: 5 }),
          ],
        }),
      ),
    )

    await store.fetchCart()

    expect(store.itemCount).toBe(7)
  })

  it('addItem calls the api with the request and applies the returned cart on success', async () => {
    const store = useCartStore()
    const updated = cart({ items: [item({ quantity: 3 })], itemCount: 3, itemTotal: 150000, total: 150000 })
    mockedCartApi.CartApi.addItem.mockResolvedValue(ok(updated))

    const success = await store.addItem('v-1', 1)

    expect(success).toBe(true)
    expect(mockedCartApi.CartApi.addItem).toHaveBeenCalledWith({ variantId: 'v-1', quantity: 1 })
    expect(store.id).toBe('cart-1')
    expect(store.items).toEqual([item({ quantity: 3 })])
    expect(store.error).toBeNull()
  })

  it('addItem sets error and returns false on failure', async () => {
    const store = useCartStore()
    mockedCartApi.CartApi.addItem.mockResolvedValue(
      failure({ code: 'Cart.AddFailed', message: 'Cannot add item', type: 400 }),
    )

    const success = await store.addItem('v-1')

    expect(success).toBe(false)
    expect(store.error).toBe('Cannot add item')
  })

  it('updateQuantity calls the api and applies the returned cart', async () => {
    const store = useCartStore()
    mockedCartApi.CartApi.updateItem.mockResolvedValue(
      ok(cart({ items: [item({ quantity: 4 })], itemCount: 4, itemTotal: 200000, total: 200000 })),
    )

    const success = await store.updateQuantity('li-1', 4)

    expect(success).toBe(true)
    expect(mockedCartApi.CartApi.updateItem).toHaveBeenCalledWith('li-1', { quantity: 4 })
    expect(store.itemCount).toBe(4)
    expect(store.subtotal).toBe(200000)
  })

  it('updateQuantity sets error and returns false on failure', async () => {
    const store = useCartStore()
    mockedCartApi.CartApi.getCart.mockResolvedValue(ok(cart({})))
    await store.fetchCart()
    mockedCartApi.CartApi.updateItem.mockResolvedValue(
      failure({ code: 'Cart.UpdateFailed', message: 'Cannot update', type: 400 }),
    )

    const success = await store.updateQuantity('li-1', 4)

    expect(success).toBe(false)
    expect(store.error).toBe('Cannot update')
  })

  it('removeItem calls the api and applies the returned cart', async () => {
    const store = useCartStore()
    mockedCartApi.CartApi.removeItem.mockResolvedValue(
      ok(cart({ items: [], itemCount: 0, itemTotal: 0, total: 0 })),
    )

    const success = await store.removeItem('li-1')

    expect(success).toBe(true)
    expect(mockedCartApi.CartApi.removeItem).toHaveBeenCalledWith('li-1')
    expect(store.items).toHaveLength(0)
    expect(store.itemCount).toBe(0)
    expect(store.subtotal).toBe(0)
  })

  it('removeItem sets error and returns false on failure', async () => {
    const store = useCartStore()
    mockedCartApi.CartApi.getCart.mockResolvedValue(ok(cart({})))
    await store.fetchCart()
    mockedCartApi.CartApi.removeItem.mockResolvedValue(
      failure({ code: 'Cart.RemoveFailed', message: 'Cannot remove', type: 400 }),
    )
    mockedCartApi.CartApi.getCart.mockResolvedValue(ok(cart({})))

    const success = await store.removeItem('li-1')

    expect(success).toBe(false)
    expect(mockedCartApi.CartApi.getCart).toHaveBeenCalled()
  })

  it('associateGuestCart sends the guest cart id and applies the merged cart on success', async () => {
    const store = useCartStore()
    store.id = 'guest-cart-1'
    const merged = cart({ id: 'user-cart-1', items: [item({ quantity: 3 })], itemCount: 3, itemTotal: 150000, total: 150000 })
    mockedCartApi.CartApi.associateCart.mockResolvedValue(ok(merged))
    mockedCartApi.CartApi.getCart.mockResolvedValue(ok(merged))

    await store.associateGuestCart()

    expect(mockedCartApi.CartApi.associateCart).toHaveBeenCalledTimes(1)
    expect(mockedCartApi.CartApi.associateCart).toHaveBeenCalledWith('guest-cart-1')
  })

  it('associateGuestCart skips cleanly when there is no guest cart id', async () => {
    const store = useCartStore()
    store.id = null

    await store.associateGuestCart()

    expect(mockedCartApi.CartApi.getCart).not.toHaveBeenCalled()
    expect(mockedCartApi.CartApi.associateCart).not.toHaveBeenCalled()
    expect(store.error).toBeNull()
  })

  it('associateGuestCart treats Guid.Empty as "no guest cart" and skips', async () => {
    const store = useCartStore()
    store.id = '00000000-0000-0000-0000-000000000000'

    await store.associateGuestCart()

    expect(mockedCartApi.CartApi.associateCart).not.toHaveBeenCalled()
  })
})
