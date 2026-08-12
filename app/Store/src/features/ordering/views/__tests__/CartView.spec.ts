import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import PrimeVue from 'primevue/config'
import { createTestingPinia } from '@pinia/testing'
import ToastService from 'primevue/toastservice'
import InputNumber from 'primevue/inputnumber'
import Chip from 'primevue/chip'
import CartView from '../CartView.vue'
import { useCart } from '../../composables/useCart'
import { CartApi } from '../../services/cartApi'
import { ok } from '@/shared/types/result'
import type { CartLineItem, CartResponse } from '../../types'

// Stub: CartApi so the composable does not make real HTTP calls.
vi.mock('../../services/cartApi', () => ({
  CartApi: {
    getCart: vi.fn(),
    addItem: vi.fn(),
    updateItem: vi.fn(),
    removeItem: vi.fn(),
    emptyCart: vi.fn(),
    associateCart: vi.fn(),
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

// Router: Memory-history router with the cart's navigation targets.
function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: { template: '<div />' } },
      { path: '/cart', component: CartView },
      { path: '/shop', component: { template: '<div />' } },
      { path: '/checkout', component: { template: '<div />' } },
    ],
  })
}

// Mount: PrimeVue + ToastService + memory router so mounted loads are no-ops.
async function mountView() {
  const router = createTestRouter()
  await router.push('/cart')
  await router.isReady()
  const wrapper = mount(CartView, {
    global: {
      plugins: [PrimeVue, createTestingPinia({ stubActions: true }), ToastService, router],
    },
  })
  await flushPromises()
  return wrapper
}

// Seed: Populate the cart composable singleton with two line items.
function seedCart() {
  const cart = useCart()
  cart.items = [lineItem, lineItem2]
  return cart
}

describe('CartView', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('shows the empty state with a Continue Shopping action when the cart is empty', async () => {
    const wrapper = await mountView()

    expect(wrapper.text()).toContain('Your cart is empty.')
    expect(wrapper.text()).toContain('Continue Shopping')
    expect(wrapper.text()).not.toContain('Order Summary')
  })

  it('renders line items with unit price, line totals and order summary', async () => {
    const wrapper = await mountView()
    seedCart()
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Classic Tee')
    expect(wrapper.text()).toContain('CT-001-R-M')
    expect(wrapper.text()).toContain('Denim Jacket')
    expect(wrapper.text()).toContain('Order Summary')
    expect(wrapper.text()).toContain('$45.00 each')
    expect(wrapper.text()).toContain('Items (3)')
    expect(wrapper.text()).toContain('$170.00')
    expect(wrapper.text()).toContain('Proceed to Checkout')
  })

  it('updates the quantity via cart.updateQuantity', async () => {
    const wrapper = await mountView()
    seedCart()
    mockedCartApi.updateItem.mockResolvedValue(ok<CartResponse>({ id: 'cart-1', items: [{ ...lineItem, quantity: 4 }] as CartLineItem[], itemTotal: 180, total: 180, currency: 'USD', itemCount: 5, checkoutState: 'address', shippingMethodId: null, shipAddressId: null, email: null }))
    await wrapper.vm.$nextTick()

    const inputs = wrapper.findAllComponents(InputNumber)
    expect(inputs[0]?.props('min')).toBe(1)
    inputs[0]!.vm.$emit('update:modelValue', 4)
    await wrapper.vm.$nextTick()

    expect(mockedCartApi.updateItem).toHaveBeenCalledWith('li-1', { quantity: 4 })
  })

  it('removes a line item via cart.removeItem', async () => {
    const wrapper = await mountView()
    seedCart()
    mockedCartApi.removeItem.mockResolvedValue(ok<CartResponse>({ id: 'cart-1', items: [lineItem2], itemTotal: 80, total: 80, currency: 'USD', itemCount: 1, checkoutState: 'address', shippingMethodId: null, shipAddressId: null, email: null }))
    await wrapper.vm.$nextTick()

    const removeButtons = wrapper.findAll('[aria-label="Remove item"]')
    expect(removeButtons).toHaveLength(2)
    await removeButtons[0]!.trigger('click')

    expect(mockedCartApi.removeItem).toHaveBeenCalledWith('li-1')
  })

  it('applies a promo code and shows the removable chip', async () => {
    const wrapper = await mountView()
    seedCart()
    await wrapper.vm.$nextTick()

    await wrapper.find('[aria-label="Promo code"]').setValue('WELCOME10')
    const applyButton = wrapper.findAll('button').find(b => b.text() === 'Apply')
    await applyButton!.trigger('click')
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('WELCOME10')
    const chip = wrapper.find('[data-pc-name="chip"]')
    expect(chip.exists()).toBe(true)
  })

  it('clears the applied coupon when the chip is removed', async () => {
    const wrapper = await mountView()
    seedCart()
    await wrapper.vm.$nextTick()

    await wrapper.find('[aria-label="Promo code"]').setValue('WELCOME10')
    const applyButton = wrapper.findAll('button').find(b => b.text() === 'Apply')
    await applyButton!.trigger('click')
    await wrapper.vm.$nextTick()
    const chip = wrapper.findComponent(Chip)
    expect(chip.exists()).toBe(true)
    chip.vm.$emit('remove')
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-pc-name="chip"]').exists()).toBe(false)
  })
})
