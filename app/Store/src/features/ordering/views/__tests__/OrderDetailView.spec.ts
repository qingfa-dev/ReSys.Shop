import { describe, it, expect, vi, beforeAll, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import PrimeVue from 'primevue/config'
import { createTestingPinia } from '@pinia/testing'
import OrderDetailView from '../OrderDetailView.vue'
import { useOrders } from '../../composables/useOrders'
import { useAddresses } from '@/features/profile/composables/useAddresses'
import { OrderApi } from '../../services/orderApi'
import { ok } from '@/shared/types/result'
import type { OrderDetail, OrderTrackingResponse } from '../../types'
import type { Address } from '@/features/profile/types'

// Polyfill: Dialog calls matchMedia on mount; jsdom does not provide it.
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

// Stub: The view fetches tracking directly (the store discards it), so the API
// module is mocked to return seeded tracking without a network call.
vi.mock('../../services/orderApi', () => ({
  OrderApi: {
    getOrders: vi.fn<() => Promise<unknown>>(),
    getOrder: vi.fn<() => Promise<unknown>>(),
    getOrderTracking: vi.fn<() => Promise<unknown>>(),
    cancelOrder: vi.fn<() => Promise<unknown>>(),
  },
}))

const mockedApi = vi.mocked(OrderApi)

// Fixture: Order detail matching the OrderDetail contract with two line items.
const orderDetail: OrderDetail = {
  id: 'o1',
  number: 'ORD-1001',
  status: 'Placed',
  total: 130,
  createdAtUtc: '2026-08-01T10:00:00Z',
  checkoutState: 'Complete',
  currency: 'USD',
  email: 'ada@example.com',
  shipAddressId: 'addr-1',
  billAddressId: 'addr-1',
  shippingMethodId: 'sm-standard',
  itemTotal: 120,
  adjustmentTotal: 0,
  shipmentTotal: 10,
  paymentTotal: 130,
  outstandingBalance: 0,
  paymentState: 'Paid',
  fulfillmentState: null,
  userId: 'u1',
  approvedById: null,
  approvedAtUtc: null,
  completedAtUtc: null,
  canceledAtUtc: null,
  modifiedAtUtc: null,
  lineItems: [
    { id: 'li-1', variantId: 'v-1', quantity: 2, price: 50, total: 100, currency: 'USD', createdAtUtc: '2026-08-01T09:00:00Z' },
    { id: 'li-2', variantId: 'v-2', quantity: 1, price: 20, total: 20, currency: 'USD', createdAtUtc: '2026-08-01T09:01:00Z' },
  ],
}

// Fixture: Tracking with placed + approved events and an estimated delivery.
const tracking: OrderTrackingResponse = {
  orderId: 'o1',
  orderCreatedAt: '2026-08-01T10:00:00Z',
  orderApprovedAt: '2026-08-01T10:05:00Z',
  orderCompletedAt: null,
  orderCanceledAt: null,
  shippedAt: null,
  deliveredAt: null,
  estimatedDeliveryAt: '2026-08-04T00:00:00Z',
}

// Fixture: Shipping address resolving the order's shipAddressId.
const shippingAddress: Address = {
  id: 'addr-1',
  userId: 'u1',
  addressType: 'Shipping',
  firstName: 'Ada',
  lastName: 'Lovelace',
  address1: '12 Analytical Engine Way',
  address2: null,
  city: 'London',
  zipCode: 'SW1A 1AA',
  phone: '+44 20 7946 0958',
  label: 'Home',
  isDefault: true,
  countryName: 'United Kingdom',
  stateProvince: null,
  countryCode: 'GB',
  stateCode: null,
}

// Router: Memory-history router with the order detail route under /account/orders.
function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: { template: '<div />' } },
      { path: '/account/orders/:id', component: OrderDetailView },
    ],
  })
}

// Mount: PrimeVue + memory router; tracking comes from the mocked API module.
async function mountView(router = createTestRouter()) {
  mockedApi.getOrderTracking.mockResolvedValue(ok(tracking))
  await router.push('/account/orders/o1')
  await router.isReady()
  const wrapper = mount(OrderDetailView, {
    global: {
      plugins: [PrimeVue, createTestingPinia({ stubActions: true }), router],
      stubs: { teleport: true },
    },
  })
  await flushPromises()
  return { wrapper, router }
}

// Seed: Populate the orders composable and address store with the detail fixtures.
function seedDetail() {
  const orders = useOrders()
  orders.currentOrder = orderDetail
  const addresses = useAddresses()
  addresses.addresses = [shippingAddress]
  return { orders, addresses }
}

describe('OrderDetailView', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders the order number, status tag and header actions', async () => {
    const { wrapper } = await mountView()
    seedDetail()
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('ORD-1001')
    expect(wrapper.find('[data-pc-name="tag"]').text()).toBe('Placed')
    const buttons = wrapper.findAll('button')
    expect(buttons.some(b => b.text() === 'Track')).toBe(true)
    expect(buttons.some(b => b.text() === 'Reorder')).toBe(true)
  })

  it('renders the order line items with variant ids, quantities and totals', async () => {
    const { wrapper } = await mountView()
    seedDetail()
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('v-1')
    expect(wrapper.text()).toContain('v-2')
    expect(wrapper.text()).toContain('$100.00')
    expect(wrapper.text()).toContain('$20.00')
  })

  it('renders summary totals and the shipping address', async () => {
    const { wrapper } = await mountView()
    seedDetail()
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('$120.00')
    expect(wrapper.text()).toContain('$10.00')
    expect(wrapper.text()).toContain('$130.00')
    expect(wrapper.text()).toContain('Ada Lovelace')
    expect(wrapper.text()).toContain('12 Analytical Engine Way')
    expect(wrapper.text()).toContain('London, SW1A 1AA')
  })

  it('opens the tracking dialog with timeline events from the tracking API', async () => {
    const { wrapper } = await mountView()
    seedDetail()
    await wrapper.vm.$nextTick()

    const track = wrapper.findAll('button').find(b => b.text() === 'Track')
    await track!.trigger('click')
    await wrapper.vm.$nextTick()

    const dialog = wrapper.find('[data-pc-name="dialog"]')
    expect(dialog.exists()).toBe(true)
    expect(wrapper.text()).toContain('Estimated delivery')
    expect(wrapper.text()).toContain('Order placed')
    expect(wrapper.text()).toContain('Order approved')
    expect(mockedApi.getOrderTracking).toHaveBeenCalledWith('o1')
  })

  it('keeps the Reorder button disabled because no line items are available', async () => {
    const { wrapper } = await mountView()
    seedDetail()
    await wrapper.vm.$nextTick()

    const reorder = wrapper.findAll('button').find(b => b.text() === 'Reorder')
    expect(reorder?.attributes('disabled')).toBeDefined()
  })

  it('shows the error state with retry when the detail fetch fails', async () => {
    const { wrapper } = await mountView()
    const orders = useOrders()
    orders.error = 'Failed to load order'
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-pc-name="message"]').text()).toContain('Failed to load order')
    const retry = wrapper.findAll('button').find(b => b.text() === 'Retry')
    expect(retry).toBeDefined()
    await retry!.trigger('click')
    expect(mockedApi.getOrder).toHaveBeenCalledWith('o1')
  })

  it('adds no native interactive elements of its own', async () => {
    const { wrapper } = await mountView()
    seedDetail()
    await wrapper.vm.$nextTick()

    expect(wrapper.findAll('input')).toHaveLength(0)
    expect(wrapper.findAll('select')).toHaveLength(0)
    expect(wrapper.findAll('textarea')).toHaveLength(0)
    expect(wrapper.findAll('button').length).toBeGreaterThan(0)
  })
})
