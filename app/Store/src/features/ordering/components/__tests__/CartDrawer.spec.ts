import { describe, it, expect, vi, beforeAll, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import { createTestingPinia } from '@pinia/testing'
import PrimeVue from 'primevue/config'
import InputNumber from 'primevue/inputnumber'
import CartDrawer from '../CartDrawer.vue'
import { useCartStore } from '../../stores/cartStore'
import type { CartLineItem } from '../../types'

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

// Mount: PrimeVue + stubbed pinia so the drawer renders without network calls.
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

// Seed: Populate the cart store with two line items.
function seedCart() {
  const cart = useCartStore()
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

  it('updates the quantity via cartStore.updateQuantity', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = await mountDrawer(router)
    const cart = seedCart()
    await wrapper.vm.$nextTick()

    const inputs = wrapper.findAllComponents(InputNumber)
    expect(inputs[0]?.props('min')).toBe(1)
    inputs[0]!.vm.$emit('update:modelValue', 3)
    await wrapper.vm.$nextTick()

    expect(cart.updateQuantity).toHaveBeenCalledWith('li-1', 3)
  })

  it('removes a line item via cartStore.removeItem', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = await mountDrawer(router)
    const cart = seedCart()
    await wrapper.vm.$nextTick()

    const removeButtons = wrapper.findAll('[aria-label="Remove item"]')
    expect(removeButtons).toHaveLength(2)
    await removeButtons[1]!.trigger('click')

    expect(cart.removeItem).toHaveBeenCalledWith('li-2')
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
})
