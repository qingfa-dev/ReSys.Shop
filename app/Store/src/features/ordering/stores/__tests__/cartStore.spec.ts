import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createTestingPinia } from '@pinia/testing'
import { setActivePinia } from 'pinia'
import { ok, failure } from '@/shared/types/result'
import { useCartStore } from '../cartStore'
import * as cartApi from '../../services/cartApi'
import type { CartLineItem, CartResponse } from '../../types/cart'

const mockedCartApi = vi.mocked(cartApi)

vi.mock('@/features/ordering/services/cartApi', () => ({
  getCart: vi.fn<(...args: unknown[]) => unknown>(),
  addItem: vi.fn<(...args: unknown[]) => unknown>(),
  updateItem: vi.fn<(...args: unknown[]) => unknown>(),
  removeItem: vi.fn<(...args: unknown[]) => unknown>(),
  emptyCart: vi.fn<(...args: unknown[]) => unknown>(),
  associateCart: vi.fn<(...args: unknown[]) => unknown>(),
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
  // Keep total consistent with price × quantity so subtotal assertions stay coherent.
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
  })

  it('fetchCart applies id, items, and currency on success', async () => {
    const store = useCartStore()
    const payload = cart({})
    mockedCartApi.getCart.mockResolvedValue(ok(payload))

    await store.fetchCart()

    expect(store.id).toBe('cart-1')
    expect(store.items).toEqual([baseItem])
    expect(store.currency).toBe('VND')
    expect(store.loading).toBe(false)
    expect(store.error).toBeNull()
  })

  it('fetchCart sets an error on failure', async () => {
    const store = useCartStore()
    mockedCartApi.getCart.mockResolvedValue(
      failure({ code: 'Cart.LoadFailed', message: 'Cart unavailable', type: 500 }),
    )

    await store.fetchCart()

    expect(store.error).toBe('Cart unavailable')
    expect(store.loading).toBe(false)
  })

  it('computes subtotal as the sum of line totals', async () => {
    const store = useCartStore()
    mockedCartApi.getCart.mockResolvedValue(
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
    mockedCartApi.getCart.mockResolvedValue(
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
    mockedCartApi.addItem.mockResolvedValue(ok(updated))

    const success = await store.addItem('v-1', 1)

    expect(success).toBe(true)
    expect(mockedCartApi.addItem).toHaveBeenCalledWith({ variantId: 'v-1', quantity: 1 })
    expect(store.id).toBe('cart-1')
    expect(store.items).toEqual([item({ quantity: 3 })])
    expect(store.error).toBeNull()
  })

  it('addItem sets error and returns false on failure', async () => {
    const store = useCartStore()
    mockedCartApi.addItem.mockResolvedValue(
      failure({ code: 'Cart.AddFailed', message: 'Cannot add item', type: 400 }),
    )

    const success = await store.addItem('v-1')

    expect(success).toBe(false)
    expect(store.error).toBe('Cannot add item')
  })

  it('updateQuantity calls the api and applies the returned cart', async () => {
    const store = useCartStore()
    mockedCartApi.updateItem.mockResolvedValue(
      ok(cart({ items: [item({ quantity: 4 })], itemCount: 4, itemTotal: 200000, total: 200000 })),
    )

    const success = await store.updateQuantity('li-1', 4)

    expect(success).toBe(true)
    expect(mockedCartApi.updateItem).toHaveBeenCalledWith('li-1', { quantity: 4 })
    expect(store.itemCount).toBe(4)
    expect(store.subtotal).toBe(200000)
  })

  it('updateQuantity sets error and returns false on failure', async () => {
    const store = useCartStore()
    mockedCartApi.updateItem.mockResolvedValue(
      failure({ code: 'Cart.UpdateFailed', message: 'Cannot update', type: 400 }),
    )

    const success = await store.updateQuantity('li-1', 4)

    expect(success).toBe(false)
    expect(store.error).toBe('Cannot update')
  })

  it('removeItem calls the api and applies the returned cart', async () => {
    const store = useCartStore()
    mockedCartApi.removeItem.mockResolvedValue(
      ok(cart({ items: [], itemCount: 0, itemTotal: 0, total: 0 })),
    )

    const success = await store.removeItem('li-1')

    expect(success).toBe(true)
    expect(mockedCartApi.removeItem).toHaveBeenCalledWith('li-1')
    expect(store.items).toHaveLength(0)
    expect(store.itemCount).toBe(0)
    expect(store.subtotal).toBe(0)
  })

  it('removeItem sets error and returns false on failure', async () => {
    const store = useCartStore()
    mockedCartApi.removeItem.mockResolvedValue(
      failure({ code: 'Cart.RemoveFailed', message: 'Cannot remove', type: 400 }),
    )

    const success = await store.removeItem('li-1')

    expect(success).toBe(false)
    expect(store.error).toBe('Cannot remove')
  })

  it('associate sends the guest cart id and applies the merged cart on success', async () => {
    const store = useCartStore()
    store.id = 'guest-cart-1'
    const merged = cart({ id: 'user-cart-1', items: [item({ quantity: 3 })], itemCount: 3, itemTotal: 150000, total: 150000 })
    mockedCartApi.associateCart.mockResolvedValue(ok(merged))

    await store.associate()

    expect(mockedCartApi.associateCart).toHaveBeenCalledTimes(1)
    expect(mockedCartApi.associateCart).toHaveBeenCalledWith('guest-cart-1')
    expect(store.id).toBe('user-cart-1')
    expect(store.items).toEqual([item({ quantity: 3 })])
  })

  it('associate skips cleanly when there is no guest cart id', async () => {
    const store = useCartStore()
    store.id = null

    await store.associate()

    // No pre-login guest cart id — skip entirely; never fetch the (now user's) cart.
    expect(mockedCartApi.getCart).not.toHaveBeenCalled()
    expect(mockedCartApi.associateCart).not.toHaveBeenCalled()
    expect(store.error).toBeNull()
  })

  it('associate treats Guid.Empty as "no guest cart" and skips', async () => {
    const store = useCartStore()
    store.id = '00000000-0000-0000-0000-000000000000'

    await store.associate()

    expect(mockedCartApi.associateCart).not.toHaveBeenCalled()
  })
})
