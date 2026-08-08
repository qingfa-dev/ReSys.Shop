import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import { createTestingPinia } from '@pinia/testing'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import InputNumber from 'primevue/inputnumber'
import Chip from 'primevue/chip'
import CartView from '../CartView.vue'
import { useCartStore } from '../../stores/cartStore'
import type { CartLineItem } from '../../types'

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

// Mount: PrimeVue + ToastService + stubbed pinia so mounted loads are no-ops.
async function mountView() {
  const router = createTestRouter()
  await router.push('/cart')
  await router.isReady()
  const wrapper = mount(CartView, {
    global: {
      plugins: [PrimeVue, ToastService, createTestingPinia({ stubActions: true }), router],
    },
  })
  await flushPromises()
  return wrapper
}

// Seed: Populate the cart store with two line items.
function seedCart() {
  const cart = useCartStore()
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

  it('updates the quantity via cartStore.updateQuantity', async () => {
    const wrapper = await mountView()
    const cart = seedCart()
    await wrapper.vm.$nextTick()

    const inputs = wrapper.findAllComponents(InputNumber)
    expect(inputs[0]?.props('min')).toBe(1)
    inputs[0]!.vm.$emit('update:modelValue', 4)
    await wrapper.vm.$nextTick()

    expect(cart.updateQuantity).toHaveBeenCalledWith('li-1', 4)
  })

  it('removes a line item via cartStore.removeItem', async () => {
    const wrapper = await mountView()
    const cart = seedCart()
    await wrapper.vm.$nextTick()

    const removeButtons = wrapper.findAll('[aria-label="Remove item"]')
    expect(removeButtons).toHaveLength(2)
    await removeButtons[0]!.trigger('click')

    expect(cart.removeItem).toHaveBeenCalledWith('li-1')
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
