import { describe, it, expect, vi, beforeAll, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import PrimeVue from 'primevue/config'
import { createTestingPinia } from '@pinia/testing'
import InputNumber from 'primevue/inputnumber'
import CartDrawer from '../CartDrawer.vue'
import { useCart } from '../../composables/useCart'
import { CartApi } from '../../services/cartApi'
import { ok } from '@/shared/types/result'
import type { CartLineItem, CartResponse } from '../../types'

// Polyfill: Drawer calls matchMedia on mount; jsdom does not provide it.
function createMatchMediaStub(query: string) {
  return {
    matches: false,
    media: query,
    onchange: null,
    addEventListener: vi.fn<() => void>(),
    removeEventListener: vi.fn<() => void>(),
    addListener: vi.fn<() => void>(),
    removeListener: vi.fn<() => void>(),
    dispatchEvent: vi.fn<() => void>(),
  }
}

beforeAll(() => {
  vi.stubGlobal('matchMedia', vi.fn<typeof createMatchMediaStub>(createMatchMediaStub))
})

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
  variantId: 'v-1',
  variantName: 'Classic Tee / Red / M',
  sku: 'CT-001-R-M',
  productName: 'Classic Tee',
  productImageUrl: '/img/tee.jpg',
  quantity: 2,
  price: 45,
  total: 90,
}

// Fixture: Second line item for multi-row assertions.
const lineItem2: CartLineItem = {
  id: 'li-2',
  variantId: 'v-2',
  variantName: 'Denim Jacket / Blue / L',
  sku: 'DJ-002-B-L',
  productName: 'Denim Jacket',
  productImageUrl: null,
  quantity: 1,
  price: 80,
  total: 80,
}

// Router: Memory-history router with the drawer's navigation targets.
function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: { template: '<div />' } },
      { path: '/shop', component: { template: '<div />' } },
      { path: '/cart', component: { template: '<div />' } },
      { path: '/checkout', component: { template: '<div />' } },
    ],
  })
}

// Mount: PrimeVue + memory router; the composable singleton holds shared state.
// Teleport: Keep the drawer DOM inside the wrapper so assertions stay scoped.
async function mountDrawer(router = createTestRouter(), visible = true) {
  const wrapper = mount(CartDrawer, {
    props: { visible },
    global: {
      plugins: [PrimeVue, createTestingPinia({ stubActions: true }), router],
      stubs: { teleport: true },
    },
  })
  await flushPromises()
  await wrapper.vm.$nextTick()
  return wrapper
}

// Seed: Populate the cart composable singleton with two line items.
function seedCart() {
  const cart = useCart()
  cart.items = [lineItem, lineItem2]
  return cart
}

describe('CartDrawer', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('shows the empty state with a Continue Shopping action when the cart is empty', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = await mountDrawer(router)

    expect(wrapper.text()).toContain('Your cart is empty.')
    expect(wrapper.text()).toContain('Continue Shopping')
  })

  it('renders line items with subtotal and free-shipping progress', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = await mountDrawer(router)
    seedCart()
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Classic Tee')
    expect(wrapper.text()).toContain('Denim Jacket')
    expect(wrapper.text()).toContain('Subtotal')
    expect(wrapper.text()).toContain('$170.00')
    expect(wrapper.find('[data-pc-name="progressbar"]').exists()).toBe(true)
    expect(wrapper.text()).not.toContain('Your cart is empty.')
  })

  it('updates the quantity via cart.updateQuantity', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = await mountDrawer(router)
    seedCart()
    mockedCartApi.updateItem.mockResolvedValue(ok<CartResponse>({ id: 'cart-1', items: [{ ...lineItem, quantity: 3 }] as CartLineItem[], itemTotal: 135, total: 135, currency: 'USD', itemCount: 4, checkoutState: 'Address', shippingMethodId: null, shipAddressId: null, email: null, shipmentTotal: 0, adjustmentTotal: 0, shippingAdjustment: null, shippingCalculation: null, adjustments: [] }))
    await wrapper.vm.$nextTick()

    const inputs = wrapper.findAllComponents(InputNumber)
    expect(inputs[0]?.props('min')).toBe(1)
    inputs[0]!.vm.$emit('update:modelValue', 3)
    await wrapper.vm.$nextTick()

    expect(mockedCartApi.updateItem).toHaveBeenCalledWith('li-1', { quantity: 3 })
  })

  it('removes a line item via cart.removeItem', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = await mountDrawer(router)
    seedCart()
    mockedCartApi.removeItem.mockResolvedValue(ok<CartResponse>({ id: 'cart-1', items: [lineItem], itemTotal: 90, total: 90, currency: 'USD', itemCount: 2, checkoutState: 'Address', shippingMethodId: null, shipAddressId: null, email: null, shipmentTotal: 0, adjustmentTotal: 0, shippingAdjustment: null, shippingCalculation: null, adjustments: [] }))
    await wrapper.vm.$nextTick()

    const removeButtons = wrapper.findAll('[aria-label="Remove item"]')
    expect(removeButtons).toHaveLength(2)
    await removeButtons[1]!.trigger('click')

    expect(mockedCartApi.removeItem).toHaveBeenCalledWith('li-2')
  })

  it('shows the checkout and view cart actions in the footer', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = await mountDrawer(router)
    seedCart()
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Checkout')
    expect(wrapper.text()).toContain('View Cart')
  })

  it('emits update:visible when the drawer closes', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = await mountDrawer(router)

    wrapper.findComponent({ name: 'Drawer' }).vm.$emit('update:visible', false)

    expect(wrapper.emitted('update:visible')?.at(-1)).toEqual([false])
  })

  it('shows server shipping and total lines in the totals zone', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = await mountDrawer(router)
    seedCart()
    const cart = useCart()
    cart.shipmentTotal = 9.99
    cart.total = 179.99
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Shipping')
    expect(wrapper.text()).toContain('$9.99')
    expect(wrapper.text()).toContain('Total')
    expect(wrapper.text()).toContain('$179.99')
  })
})
