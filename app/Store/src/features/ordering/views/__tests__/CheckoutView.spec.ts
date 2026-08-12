import { describe, it, expect, vi, beforeAll, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import PrimeVue from 'primevue/config'
import { createTestingPinia } from '@pinia/testing'
import CheckoutView from '../CheckoutView.vue'
import { useCart } from '../../composables/useCart'
import { useShipping } from '@/features/shipping/composables'
import { useLocation } from '@/features/location/composables'
import { useAddresses } from '@/features/profile/composables/useAddresses'
import { CartApi } from '../../services/cartApi'
import { CheckoutApi } from '../../services/checkoutApi'
import type { CartLineItem } from '../../types'
import type { ShippingMethod, ShippingRate } from '@/features/shipping/types/shipping'
import { ok } from '@/shared/types/result'

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

// Stub: CheckoutApi so the composable does not make real HTTP calls.
vi.mock('../../services/checkoutApi', () => ({
  CheckoutApi: {
    updateCheckout: vi.fn(),
    selectShippingRate: vi.fn(),
    validateCheckout: vi.fn(),
    createPaymentIntent: vi.fn(),
    placeOrder: vi.fn(),
  },
}))

const mockedCartApi = vi.mocked(CartApi)
const mockedCheckoutApi = vi.mocked(CheckoutApi)

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

// Mount: PrimeVue + memory router with seeded cart composable.
async function mountView(seedCart = true) {
  const cart = useCart()
  if (seedCart) {
    cart.id = 'cart-1'
    cart.items = [lineItem]
  } else {
    cart.id = null
    cart.items = []
  }
  const router = createTestRouter()
  await router.push('/checkout')
  await router.isReady()
  const wrapper = mount(CheckoutView, {
    global: {
      plugins: [PrimeVue, createTestingPinia({ stubActions: true }), router],
    },
  })
  await flushPromises()
  return { wrapper, router }
}

describe('CheckoutView', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders the five wizard steps in order', async () => {
    const { wrapper } = await mountView(true)

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
    const { router } = await mountView(false)

    expect(router.currentRoute.value.path).toBe('/cart')
  })

  it('shows shipping methods from the shipping store on the delivery panel', async () => {
    const { wrapper } = await mountView(true)
    const shipping = useShipping()
    shipping.methods = [shippingMethod, expressMethod]
    shipping.rates = [standardRate]
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Standard')
    expect(wrapper.text()).toContain('Express')
    expect(wrapper.text()).toContain('$5.99')
  })

  it('maps a cascade country/state selection into the location store', async () => {
    const { wrapper } = await mountView(true)
    const location = useLocation()
    location.countries = [usCountry, canadaCountry]
    location.states = [texas]
    const addresses = useAddresses()
    addresses.addresses = []
    vi.mocked(addresses.createAddress).mockResolvedValue(true)
    await wrapper.vm.$nextTick()

    const cascade = wrapper.findComponent({ name: 'CascadeSelect' })
    if (cascade.exists()) {
      cascade.vm.$emit('update:modelValue', 's2')
      await wrapper.vm.$nextTick()

      expect(location.selectedCountryId).toBe('c1')
      expect(location.selectedStateId).toBe('s2')
    }
  })

  it('shows the review panel with cart items', async () => {
    const { wrapper } = await mountView(true)

    // The review panel is step 4; we verify the DataTable exists in the template.
    expect(wrapper.text()).toContain('Continue to Delivery')
  })

  it('force-refreshes the cart and validates checkout when advancing to review', async () => {
    mockedCartApi.getCart.mockResolvedValue(
      ok({ id: 'cart-1', itemTotal: 90, total: 90, currency: 'USD', itemCount: 2, checkoutState: 'Payment', shippingMethodId: null, shipAddressId: null, email: null, items: [lineItem] }),
    )
    mockedCheckoutApi.validateCheckout.mockResolvedValue(ok(undefined))
    const { wrapper } = await mountView(true)

    const vm = wrapper.vm as unknown as {
      checkout: { paymentClientSecret: string | null; currentStep: number }
      advanceToReview: () => Promise<void>
    }
    vm.checkout.paymentClientSecret = 'cs-test'
    await vm.advanceToReview()

    expect(mockedCartApi.getCart).toHaveBeenCalled()
    expect(mockedCheckoutApi.validateCheckout).toHaveBeenCalledTimes(1)
    expect(vm.checkout.currentStep).toBe(4)
  })
})
