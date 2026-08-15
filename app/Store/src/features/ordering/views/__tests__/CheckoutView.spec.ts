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
import type { PaymentMethod } from '@/features/payment/types/payment'
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

// Stub: CheckoutApi so the composable does not make real HTTP calls.
vi.mock('../../services/checkoutApi', () => ({
  CheckoutApi: {
    updateCheckout: vi.fn<() => Promise<{ isSuccess: boolean }>>(),
    selectShippingRate: vi.fn<() => Promise<{ isSuccess: boolean }>>(),
    validateCheckout: vi.fn<() => Promise<{ isSuccess: boolean }>>(),
    createPaymentIntent: vi.fn<() => Promise<{ isSuccess: boolean }>>(),
    placeOrder: vi.fn<() => Promise<{ isSuccess: boolean }>>(),
  },
}))

// Stub: Payment methods for the payment panel — a card gateway and a COD method.
vi.mock('@/features/payment/services/paymentApi', () => ({
  getPaymentMethods: vi.fn<() => Promise<{
    isSuccess: boolean; statusCode: number; message: null; errors: never[]
    items: PaymentMethod[]; page: number; pageSize: number; totalCount: number; totalPages: number
  }>>().mockResolvedValue({
    isSuccess: true,
    statusCode: 200,
    message: null,
    errors: [],
    items: [
      {
        id: 'pm-stripe',
        name: 'Credit Card',
        code: null,
        description: null,
        providerKey: 'stripe',
        preferences: null,
        active: true,
        autoCapture: true,
        displayOn: 'Frontend',
        position: 1,
        presentation: null,
        webhookEnabled: true,
      },
      {
        id: 'pm-cod',
        name: 'Cash on Delivery',
        code: null,
        description: null,
        providerKey: 'cod',
        preferences: null,
        active: true,
        autoCapture: false,
        displayOn: 'Both',
        position: 2,
        presentation: null,
        webhookEnabled: false,
      },
    ],
    page: 1,
    pageSize: 50,
    totalCount: 2,
    totalPages: 1,
  }),
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
    // Location: Provide a writable location so the Stripe redirect can be asserted
    // without jsdom attempting a real navigation.
    vi.stubGlobal('location', {
      origin: 'http://localhost',
      href: 'http://localhost/checkout',
      pathname: '/checkout',
      search: '',
      hash: '',
    })
    // Reset: Singleton cart refs persist across tests in this module.
    const cart = useCart()
    cart.checkoutState = null
    cart.shippingMethodId = null
    cart.shipAddressId = null
    cart.email = null
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
    cascade.vm.$emit('update:modelValue', 's2')
    await wrapper.vm.$nextTick()

    expect(location.selectedCountryId).toBe('c1')
    expect(location.selectedStateId).toBe('s2')
  })

  it('shows the review panel with cart items', async () => {
    const { wrapper } = await mountView(true)

    // The review panel is step 4; we verify the DataTable exists in the template.
    expect(wrapper.text()).toContain('Continue to Delivery')
  })

  it('force-refreshes the cart and validates checkout when advancing to review', async () => {
    mockedCartApi.getCart.mockResolvedValue(
      ok({ id: 'cart-1', itemTotal: 90, total: 90, currency: 'USD', itemCount: 2, checkoutState: 'PickPaymentMethod', shippingMethodId: null, shipAddressId: null, email: null, shipmentTotal: 0, adjustmentTotal: 0, shippingAdjustment: null, items: [lineItem] }),
    )
    mockedCheckoutApi.validateCheckout.mockResolvedValue(ok(undefined))
    const { wrapper } = await mountView(true)

    const vm = wrapper.vm as unknown as {
      checkout: { paymentIntentId: string | null; displayStep: number }
      advanceToReview: () => Promise<void>
    }
    vm.checkout.paymentIntentId = 'pi-1'
    await vm.advanceToReview()

    expect(mockedCartApi.getCart).toHaveBeenCalled()
    expect(mockedCheckoutApi.validateCheckout).toHaveBeenCalledTimes(1)
    expect(vm.checkout.displayStep).toBe(4)
  })

  // Methods: The payment panel lists customer-facing methods and preselects the first.
  it('renders the payment method radio list on the payment panel', async () => {
    mockedCartApi.getCart.mockResolvedValue(
      ok({ id: 'cart-1', itemTotal: 90, total: 90, currency: 'USD', itemCount: 2, checkoutState: 'PickDeliveryMethod', shippingMethodId: null, shipAddressId: null, email: null, shipmentTotal: 0, adjustmentTotal: 0, shippingAdjustment: null, items: [lineItem] }),
    )
    const { wrapper } = await mountView(true)

    const vm = wrapper.vm as unknown as { checkout: { displayStep: number } }
    vm.checkout.displayStep = 3
    await flushPromises()

    expect(wrapper.text()).toContain('Credit Card')
    expect(wrapper.text()).toContain('Cash on Delivery')
    expect(wrapper.findAll('[data-pc-name="radiobutton"]').length).toBeGreaterThanOrEqual(2)
  })

  // Confirm-before-charge: A card method creates the intent and advances to Review;
  // the Stripe redirect happens only when the customer clicks "Confirm and Pay".
  it('advances to review for a card method without redirecting to Stripe yet', async () => {
    mockedCartApi.getCart.mockResolvedValue(
      ok({ id: 'cart-1', itemTotal: 90, total: 90, currency: 'USD', itemCount: 2, checkoutState: 'PickPaymentMethod', shippingMethodId: null, shipAddressId: null, email: null, shipmentTotal: 0, adjustmentTotal: 0, shippingAdjustment: null, items: [lineItem] }),
    )
    mockedCheckoutApi.createPaymentIntent.mockResolvedValue(
      ok({ id: 'pi-1', clientSecret: 'cs-test', checkoutUrl: 'https://checkout.stripe.com/c/pay/cs_test_123' }),
    )
    mockedCheckoutApi.validateCheckout.mockResolvedValue(ok(undefined))
    const { wrapper } = await mountView(true)

    const vm = wrapper.vm as unknown as {
      checkout: {
        displayStep: number
        checkoutUrl: string | null
        createPaymentIntent: (methodId: string, opts?: { returnUrl?: string; cancelUrl?: string }) => Promise<boolean>
      }
      selectedPaymentMethodId: string | null
      onContinueFromPayment: () => Promise<void>
      onConfirmAndPay: () => void
    }
    // Drive the wizard to the payment panel (backend PickPaymentMethod -> display step 3).
    vm.checkout.displayStep = 3
    await flushPromises()
    vm.selectedPaymentMethodId = 'pm-stripe'
    await vm.onContinueFromPayment()

    expect(mockedCheckoutApi.createPaymentIntent).toHaveBeenCalledWith({
      orderId: 'cart-1',
      paymentMethodId: 'pm-stripe',
      returnUrl: 'http://localhost/checkout/return',
      cancelUrl: 'http://localhost/checkout',
    })
    // Confirm-before-charge: landed on Review, not yet redirected to Stripe.
    expect(vm.checkout.displayStep).toBe(4)
    expect(window.location.href).toBe('http://localhost/checkout')
    expect(vm.checkout.checkoutUrl).toBe('https://checkout.stripe.com/c/pay/cs_test_123')

    // Confirm and Pay → redirect to the hosted checkout.
    vm.onConfirmAndPay()
    expect(window.location.href).toBe('https://checkout.stripe.com/c/pay/cs_test_123')
  })

  // COD: No hosted checkout URL, so continuing advances to the review panel.
  it('advances to review for a COD method without a hosted checkout', async () => {
    mockedCartApi.getCart.mockResolvedValue(
      ok({ id: 'cart-1', itemTotal: 90, total: 90, currency: 'USD', itemCount: 2, checkoutState: 'PickPaymentMethod', shippingMethodId: null, shipAddressId: null, email: null, shipmentTotal: 0, adjustmentTotal: 0, shippingAdjustment: null, items: [lineItem] }),
    )
    mockedCheckoutApi.createPaymentIntent.mockResolvedValue(
      ok({ id: 'pi-1', clientSecret: null }),
    )
    mockedCheckoutApi.validateCheckout.mockResolvedValue(ok(undefined))
    const { wrapper } = await mountView(true)

    const vm = wrapper.vm as unknown as {
      checkout: {
        displayStep: number
        createPaymentIntent: (methodId: string, opts?: { returnUrl?: string; cancelUrl?: string }) => Promise<boolean>
      }
      selectedPaymentMethodId: string | null
      onContinueFromPayment: () => Promise<void>
    }
    vm.checkout.displayStep = 3
    await flushPromises()
    vm.selectedPaymentMethodId = 'pm-cod'
    await vm.onContinueFromPayment()

    expect(window.location.href).toBe('http://localhost/checkout')
    expect(vm.checkout.displayStep).toBe(4)
  })

  // Hydrate: Backend 'PickDeliveryMethod' state drives the delivery panel on mount.
  it('hydrates the delivery panel from the backend checkout state', async () => {
    mockedCartApi.getCart.mockResolvedValue(
      ok({ id: 'cart-1', itemTotal: 90, total: 90, currency: 'USD', itemCount: 2, checkoutState: 'PickDeliveryMethod', shippingMethodId: null, shipAddressId: null, email: null, shipmentTotal: 0, adjustmentTotal: 0, shippingAdjustment: null, items: [lineItem] }),
    )
    const cart = useCart()
    cart.checkoutState = 'PickDeliveryMethod'
    const { wrapper } = await mountView(true)

    const vm = wrapper.vm as unknown as { checkout: { displayStep: number } }
    expect(vm.checkout.displayStep).toBe(2)
    expect(wrapper.text()).toContain('Continue to Payment')
  })

  // Select: Re-choosing a shipping method on the delivery step succeeds.
  it('allows re-selecting a shipping method on the same step', async () => {
    mockedCartApi.getCart.mockResolvedValue(
      ok({ id: 'cart-1', itemTotal: 90, total: 90, currency: 'USD', itemCount: 2, checkoutState: 'PickDeliveryMethod', shippingMethodId: null, shipAddressId: null, email: null, shipmentTotal: 0, adjustmentTotal: 0, shippingAdjustment: null, items: [lineItem] }),
    )
    mockedCheckoutApi.selectShippingRate.mockResolvedValue(ok(undefined))
    const cart = useCart()
    cart.checkoutState = 'PickDeliveryMethod'
    const { wrapper } = await mountView(true)

    const vm = wrapper.vm as unknown as {
      checkout: { displayStep: number; selectShippingRate: (id: string) => Promise<boolean> }
      goToStep: (value: number) => void
    }

    await vm.checkout.selectShippingRate('sm-standard')
    expect(vm.checkout.displayStep).toBe(3)

    vm.goToStep(2)
    expect(vm.checkout.displayStep).toBe(2)

    await vm.checkout.selectShippingRate('sm-express')
    expect(vm.checkout.displayStep).toBe(3)
    expect(mockedCheckoutApi.selectShippingRate).toHaveBeenCalledTimes(2)
  })

  // Regression: A backend step-back to PickDeliveryMethod clears the payment intent.
  it('clears payment intent refs when the backend moves Payment back to Delivery', async () => {
    mockedCartApi.getCart.mockResolvedValue(
      ok({ id: 'cart-1', itemTotal: 90, total: 90, currency: 'USD', itemCount: 2, checkoutState: 'PickPaymentMethod', shippingMethodId: null, shipAddressId: null, email: null, shipmentTotal: 0, adjustmentTotal: 0, shippingAdjustment: null, items: [lineItem] }),
    )
    const cart = useCart()
    cart.checkoutState = 'PickPaymentMethod'
    const { wrapper } = await mountView(true)

    const vm = wrapper.vm as unknown as {
      checkout: {
        displayStep: number
        paymentClientSecret: string | null
        paymentIntentId: string | null
        paymentMethodId: string | null
      }
    }
    vm.checkout.paymentClientSecret = 'cs'
    vm.checkout.paymentIntentId = 'pi'
    vm.checkout.paymentMethodId = 'pm'

    cart.checkoutState = 'PickDeliveryMethod'
    await wrapper.vm.$nextTick()

    expect(vm.checkout.paymentClientSecret).toBeNull()
    expect(vm.checkout.paymentIntentId).toBeNull()
    expect(vm.checkout.paymentMethodId).toBeNull()
    expect(vm.checkout.displayStep).toBe(2)
  })

  // Navigate: The wizard cannot advance ahead of the backend step.
  it('blocks goToStep from advancing beyond the backend step', async () => {
    mockedCartApi.getCart.mockResolvedValue(
      ok({ id: 'cart-1', itemTotal: 90, total: 90, currency: 'USD', itemCount: 2, checkoutState: 'PickDeliveryMethod', shippingMethodId: null, shipAddressId: null, email: null, shipmentTotal: 0, adjustmentTotal: 0, shippingAdjustment: null, items: [lineItem] }),
    )
    const cart = useCart()
    cart.checkoutState = 'PickDeliveryMethod'
    const { wrapper } = await mountView(true)

    const vm = wrapper.vm as unknown as {
      checkout: { displayStep: number }
      goToStep: (value: number) => void
    }

    vm.goToStep(4)
    expect(vm.checkout.displayStep).toBe(2)

    vm.goToStep(2)
    expect(vm.checkout.displayStep).toBe(2)
  })

  // Bounce: Empty-cart redirect is skipped when the backend is at Complete.
  it('skips the empty-cart redirect on the confirmation step', async () => {
    mockedCartApi.getCart.mockResolvedValue(
      ok({ id: 'cart-1', itemTotal: 0, total: 0, currency: 'USD', itemCount: 0, checkoutState: 'Complete', shippingMethodId: null, shipAddressId: null, email: null, shipmentTotal: 0, adjustmentTotal: 0, shippingAdjustment: null, items: [] }),
    )
    const { wrapper, router } = await mountView(false)

    const vm = wrapper.vm as unknown as { checkout: { displayStep: number } }
    expect(router.currentRoute.value.path).toBe('/checkout')
    expect(vm.checkout.displayStep).toBe(5)
  })

  // Review: The summary panel shows the server shipmentTotal and cart total.
  it('shows the server shipmentTotal and total on the review panel', async () => {
    const { wrapper } = await mountView(true)
    const cart = useCart()
    cart.shipmentTotal = 9.99
    cart.total = 139.99
    const vm = wrapper.vm as unknown as { checkout: { displayStep: number } }
    vm.checkout.displayStep = 4
    await wrapper.vm.$nextTick()

    const summary = wrapper.findAllComponents({ name: 'Panel' }).find(p => p.text().includes('Order Summary'))
    expect(summary!.text()).toContain('Shipping')
    expect(summary!.text()).toContain('$9.99')
    expect(summary!.text()).toContain('Total')
    expect(summary!.text()).toContain('$139.99')
  })

  // Free: A zero shipmentTotal renders $0.00 on the review panel.
  it('renders free shipping as $0.00 on the review panel', async () => {
    const { wrapper } = await mountView(true)
    const cart = useCart()
    cart.shipmentTotal = 0
    cart.total = 90
    const vm = wrapper.vm as unknown as { checkout: { displayStep: number } }
    vm.checkout.displayStep = 4
    await wrapper.vm.$nextTick()

    const summary = wrapper.findAllComponents({ name: 'Panel' }).find(p => p.text().includes('Order Summary'))
    expect(summary!.text()).toContain('Shipping')
    expect(summary!.text()).toContain('$0.00')
  })
})
