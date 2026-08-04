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
  lineItemId: 'li-1',
  variantId: 'v-1',
  productId: 'p-1',
  productName: 'Hex Bolt',
  productSlug: 'hex-bolt',
  sku: 'SKU-1',
  quantity: 2,
  unitPrice: 50000,
  currency: 'VND',
  thumbnailUrl: null,
  optionDescription: null,
  maxQuantity: 10,
}

function item(overrides: Partial<CartLineItem>): CartLineItem {
  return { ...baseItem, ...overrides }
}

function cart(overrides: Partial<CartResponse>): CartResponse {
  return {
    id: 'cart-1',
    items: [baseItem],
    itemCount: 2,
    subtotal: 100000,
    currency: 'VND',
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

  it('computes subtotal as the sum of unitPrice * quantity', async () => {
    const store = useCartStore()
    mockedCartApi.getCart.mockResolvedValue(
      ok(
        cart({
          items: [
            item({ lineItemId: 'a', quantity: 1, unitPrice: 10000 }),
            item({ lineItemId: 'b', quantity: 3, unitPrice: 20000 }),
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
            item({ lineItemId: 'a', quantity: 2 }),
            item({ lineItemId: 'b', quantity: 5 }),
          ],
        }),
      ),
    )

    await store.fetchCart()

    expect(store.itemCount).toBe(7)
  })

  it('addItem calls the api with the request and applies the returned cart on success', async () => {
    const store = useCartStore()
    const updated = cart({ items: [item({ quantity: 3 })], itemCount: 3, subtotal: 150000 })
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
      ok(cart({ items: [item({ quantity: 4 })], itemCount: 4, subtotal: 200000 })),
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
      ok(cart({ items: [], itemCount: 0, subtotal: 0 })),
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
})
