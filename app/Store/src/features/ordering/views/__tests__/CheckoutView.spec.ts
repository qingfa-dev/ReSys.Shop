import { describe, it, expect, vi, beforeAll, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import { createTestingPinia } from '@pinia/testing'
import PrimeVue from 'primevue/config'
import type { Pinia } from 'pinia'
import CheckoutView from '../CheckoutView.vue'
import CascadeSelect from 'primevue/cascadeselect'
import { useCartStore } from '../../stores/cartStore'
import { useCheckoutStore } from '../../stores/checkoutStore'
import { useShippingStore } from '@/features/shipping/stores/shippingStore'
import { useLocationStore } from '@/features/location/stores/locationStore'
import { useAddressStore } from '@/features/profile/stores/addressStore'
import type { CartLineItem } from '../../types'
import type { ShippingMethod, ShippingRate } from '@/features/shipping/types/shipping'
import type { Address } from '@/features/profile/types'

// Polyfill: Select calls matchMedia on mount; jsdom does not provide it.
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

// Stub: Stripe Elements needs browser APIs and a publishable key, so the payment
// composable is replaced with a no-op harness for the checkout tests.
vi.mock('@/features/payment/composables/usePayment', () => ({
  usePayment: () => ({
    loading: { value: false },
    error: { value: null },
    stripePromise: { value: null },
    init: vi.fn<() => void>(),
    mount: vi.fn<() => Promise<null>>().mockResolvedValue(null),
    unmount: vi.fn<() => void>(),
  }),
}))

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

// Fixture: Shipping methods matching the ShippingMethod contract.
const shippingMethod: ShippingMethod = {
  id: 'sm-standard',
  name: 'Standard',
  adminName: null,
  code: 'standard',
  calculatorType: 'weight',
  position: 1,
}

const expressMethod: ShippingMethod = {
  id: 'sm-express',
  name: 'Express',
  adminName: null,
  code: 'express',
  calculatorType: 'weight',
  position: 2,
}

// Fixture: Rate for the standard method to surface a customer-facing price.
const standardRate: ShippingRate = {
  id: 'rate-1',
  shippingMethodId: 'sm-standard',
  name: 'Standard Rate',
  cost: 5.99,
  finalPrice: 5.99,
  deliveryRange: '5-7 business days',
  minWeight: null,
  maxWeight: null,
  freeShippingThreshold: null,
}

// Fixture: Country with a state cascade level, its state, and a state-free country.
const usCountry = {
  id: 'c1',
  name: 'United States',
  isoCode: 'US',
  callingCode: '1',
  statesRequired: true,
  isActive: true,
}

const texas = {
  id: 's2',
  name: 'Texas',
  abbreviation: 'TX',
  countryId: 'c1',
  isActive: true,
  countryName: 'United States',
}

const canadaCountry = {
  id: 'c9',
  name: 'Canada',
  isoCode: 'CA',
  callingCode: '1',
  statesRequired: false,
  isActive: true,
}

// Fixture: Existing saved address used to prove the created address flows into checkout.
const savedAddress: Address = {
  id: 'addr-1',
  userId: 'u-1',
  addressType: 'Shipping',
  firstName: 'Ada',
  lastName: null,
  address1: '1 Main St',
  address2: null,
  city: 'Austin',
  zipCode: '73301',
  phone: null,
  label: null,
  isDefault: true,
  countryName: 'United States',
  stateProvince: 'Texas',
  countryCode: 'US',
  stateCode: 'TX',
}

// Router: Memory-history router with the checkout's navigation targets.
function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: { template: '<div />' } },
      { path: '/cart', component: { template: '<div />' } },
      { path: '/shop', component: { template: '<div />' } },
      { path: '/checkout', component: CheckoutView },
      { path: '/account/orders', component: { template: '<div />' } },
    ],
  })
}

// Mount: PrimeVue + stubbed pinia + memory router; seeds depend on options.
async function mountView(options: { seedCart?: boolean; step?: 1 | 2 | 3 | 4 | 5; orderId?: string | null } = {}) {
  const pinia: Pinia = createTestingPinia({ stubActions: true })
  const checkout = useCheckoutStore(pinia)
  checkout.currentStep = options.step ?? 1
  checkout.orderId = options.orderId ?? null
  if (options.seedCart) {
    const cart = useCartStore(pinia)
    cart.id = 'cart-1'
    cart.items = [lineItem]
  }
  const router = createTestRouter()
  await router.push('/checkout')
  await router.isReady()
  const wrapper = mount(CheckoutView, {
    global: {
      plugins: [PrimeVue, pinia, router],
    },
  })
  await flushPromises()
  return { wrapper, pinia, router }
}

describe('CheckoutView', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders the five wizard steps in order', async () => {
    const { wrapper } = await mountView({ seedCart: true })

    const steps = wrapper.findAll('[data-pc-name="step"]')
    expect(steps).toHaveLength(5)
    expect(wrapper.text()).toContain('Shipping')
    expect(wrapper.text()).toContain('Delivery')
    expect(wrapper.text()).toContain('Payment')
    expect(wrapper.text()).toContain('Review')
    expect(wrapper.text()).toContain('Confirmation')
    expect(wrapper.text()).toContain('Continue to Delivery')
  })

  it('redirects to /cart when the cart is empty', async () => {
    const { router } = await mountView({ seedCart: false })

    expect(router.currentRoute.value.path).toBe('/cart')
  })

  it('does not redirect when an order was just confirmed', async () => {
    const { router, wrapper } = await mountView({ step: 5, orderId: 'order-9' })

    expect(router.currentRoute.value.path).toBe('/checkout')
    expect(wrapper.text()).toContain('order-9')
  })

  it('shows shipping methods from the shipping store on the delivery panel', async () => {
    const { wrapper, pinia } = await mountView({ seedCart: true })
    const shipping = useShippingStore(pinia)
    shipping.methods = [shippingMethod, expressMethod]
    shipping.rates = [standardRate]
    const checkout = useCheckoutStore(pinia)
    checkout.currentStep = 2
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Standard')
    expect(wrapper.text()).toContain('Express')
    expect(wrapper.text()).toContain('$5.99')
    expect(wrapper.find('[data-pc-name="radiobuttongroup"]').exists()).toBe(true)
  })

  it('maps a cascade country/state selection into the store and the new address payload', async () => {
    const { wrapper, pinia } = await mountView({ seedCart: true })
    const location = useLocationStore(pinia)
    location.countries = [usCountry, canadaCountry]
    location.states = [texas]
    const addresses = useAddressStore(pinia)
    addresses.addresses = [savedAddress]
    // Stub: Force the createAddress action to succeed so the created id flows onward.
    vi.mocked(addresses.createAddress).mockResolvedValue(true)
    const checkout = useCheckoutStore(pinia)

    // Simulate: Select United States → Texas through the cascade control; PrimeVue v5
    // emits the leaf optionValue (the state id) as modelValue.
    const cascade = wrapper.findComponent(CascadeSelect)
    cascade.vm.$emit('update:modelValue', 's2')
    await wrapper.vm.$nextTick()

    expect(cascade.props('modelValue')).toBe('s2')
    expect(location.selectedCountryId).toBe('c1')
    expect(location.selectedStateId).toBe('s2')
    expect(wrapper.text()).toContain('United States / Texas')

    await wrapper.find('#checkout-first-name').setValue('Ada')
    await wrapper.find('#checkout-address1').setValue('1 Main St')
    await wrapper.find('#checkout-city').setValue('Austin')
    await wrapper.find('#checkout-email').setValue('ada@example.com')
    const continueButton = wrapper.findAll('button').find((b) => b.text() === 'Continue to Delivery')
    await continueButton!.trigger('click')
    await flushPromises()

    expect(addresses.createAddress).toHaveBeenCalledWith(
      expect.objectContaining({
        countryName: 'United States',
        stateProvince: 'Texas',
        countryCode: 'US',
        stateCode: 'TX',
      }),
    )
    expect(checkout.saveAddress).toHaveBeenCalledWith('addr-1', 'ada@example.com')
  })

  it('selects a state-free country as a bare leaf and clears the state field', async () => {
    const { wrapper, pinia } = await mountView({ seedCart: true })
    const location = useLocationStore(pinia)
    location.countries = [usCountry, canadaCountry]
    location.states = [texas]

    const cascade = wrapper.findComponent(CascadeSelect)
    cascade.vm.$emit('update:modelValue', 'c9')
    await wrapper.vm.$nextTick()

    expect(location.selectedCountryId).toBe('c9')
    expect(location.selectedStateId).toBeNull()
    expect(wrapper.text()).toContain('Canada')
  })

  it('places the order via checkoutStore.placeOrder from the review panel', async () => {
    const { wrapper, pinia } = await mountView({ seedCart: true })
    const checkout = useCheckoutStore(pinia)
    checkout.currentStep = 4
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Classic Tee')
    const placeOrderButton = wrapper.findAll('button').find((b) => b.text() === 'Place Order')
    expect(placeOrderButton).toBeDefined()
    await placeOrderButton!.trigger('click')
    await wrapper.vm.$nextTick()

    expect(checkout.placeOrder).toHaveBeenCalled()
  })

  it('shows the order number and orders link on the confirmation panel', async () => {
    const { wrapper } = await mountView({ step: 5, orderId: 'order-123' })

    expect(wrapper.text()).toContain('Order confirmed!')
    expect(wrapper.text()).toContain('Order number: order-123')
    const ordersLink = wrapper.find('a[href="/account/orders"]')
    expect(ordersLink.exists()).toBe(true)
  })
})
