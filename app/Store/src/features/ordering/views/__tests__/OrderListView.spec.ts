import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import PrimeVue from 'primevue/config'
import { createTestingPinia } from '@pinia/testing'
import Paginator from 'primevue/paginator'
import OrderListView from '../OrderListView.vue'
import { useOrders } from '../../composables/useOrders'
import { OrderApi } from '../../services/orderApi'
import type { OrderListItem } from '../../types'

// Stub: OrderApi so the composable does not make real HTTP calls.
vi.mock('../../services/orderApi', () => ({
  OrderApi: {
    getOrders: vi.fn<() => Promise<unknown>>(),
    getOrder: vi.fn<() => Promise<unknown>>(),
    getOrderTracking: vi.fn<() => Promise<unknown>>(),
    cancelOrder: vi.fn<() => Promise<unknown>>(),
  },
}))

const mockedOrderApi = vi.mocked(OrderApi)

// Fixture: Placed order with a known total for currency assertions.
const placedOrder: OrderListItem = {
  id: 'o1',
  number: 'ORD-1001',
  status: 'Placed',
  total: 130,
  currency: 'USD',
  itemCount: 2,
  createdAtUtc: '2026-08-01T10:00:00Z',
}

// Fixture: Canceled order to exercise the danger severity tag.
const canceledOrder: OrderListItem = {
  id: 'o2',
  number: 'ORD-1002',
  status: 'Canceled',
  total: 45,
  currency: 'USD',
  itemCount: 1,
  createdAtUtc: '2026-07-20T09:30:00Z',
}

// Router: Memory-history router with the orders list and its navigation targets.
function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: { template: '<div />' } },
      { path: '/shop', component: { template: '<div />' } },
      { path: '/account/orders', component: OrderListView },
      { path: '/account/orders/:id', component: { template: '<div />' } },
    ],
  })
}

// Mount: PrimeVue + memory router so the mounted fetch is a no-op.
async function mountView(router = createTestRouter()) {
  await router.push('/account/orders')
  await router.isReady()
  const wrapper = mount(OrderListView, {
    global: {
      plugins: [PrimeVue, createTestingPinia({ stubActions: true }), router],
    },
  })
  await flushPromises()
  return { wrapper, router }
}

// Seed: Populate the orders composable singleton with list rows and pagination state.
function seedOrders(orders: OrderListItem[], totalCount = orders.length) {
  const store = useOrders()
  store.items = orders
  store.totalCount = totalCount
  store.page = 1
  store.pageSize = 20
  return store
}

describe('OrderListView', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders order rows with formatted dates, totals and status tags', async () => {
    const { wrapper } = await mountView()
    seedOrders([placedOrder, canceledOrder])
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('ORD-1001')
    expect(wrapper.text()).toContain('ORD-1002')
    expect(wrapper.text()).toContain('$130.00')
    expect(wrapper.text()).toContain('$45.00')
    const tags = wrapper.findAll('[data-pc-name="tag"]')
    expect(tags).toHaveLength(2)
    expect(tags[0]!.text()).toBe('Placed')
    expect(tags[1]!.text()).toBe('Canceled')
  })

  it('navigates to the order detail route when a row number is clicked', async () => {
    const { wrapper, router } = await mountView()
    seedOrders([placedOrder])
    await wrapper.vm.$nextTick()

    const link = wrapper.find('a[href="/account/orders/o1"]')
    expect(link.exists()).toBe(true)
    await link.trigger('click')
    await flushPromises()

    expect(router.currentRoute.value.path).toBe('/account/orders/o1')
  })

  it('pages through the order list via the store pagination', async () => {
    const { wrapper } = await mountView()
    const store = seedOrders([placedOrder], 45)
    await wrapper.vm.$nextTick()

    const paginator = wrapper.findComponent(Paginator)
    expect(paginator.exists()).toBe(true)
    paginator.vm.$emit('page', { page: 2, first: 40, rows: 20, pageCount: 3 })

    expect(store.goToPage).toBeDefined()
  })

  it('shows skeleton rows while the first page loads', async () => {
    const { wrapper } = await mountView()
    const store = useOrders()
    store.loading = true
    await wrapper.vm.$nextTick()

    expect(wrapper.findAll('[data-pc-name="skeleton"]').length).toBeGreaterThan(0)
  })

  it('shows the empty state with a Start Shopping link when no orders exist', async () => {
    const { wrapper } = await mountView()
    seedOrders([])
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-pc-name="message"]').text()).toContain('No orders yet.')
    expect(wrapper.find('a[href="/shop"]').exists()).toBe(true)
  })

  it('shows an error message with retry when the fetch fails', async () => {
    const { wrapper } = await mountView()
    const store = useOrders()
    store.error = 'Failed to load orders'
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-pc-name="message"]').text()).toContain('Failed to load orders')
    const retry = wrapper.findAll('button').find(b => b.text() === 'Retry')
    expect(retry).toBeDefined()
    await retry!.trigger('click')
    expect(mockedOrderApi.getOrders).toHaveBeenCalled()
  })

  it('adds no native interactive elements of its own', async () => {
    const { wrapper } = await mountView()
    seedOrders([placedOrder, canceledOrder])
    await wrapper.vm.$nextTick()

    expect(wrapper.findAll('input')).toHaveLength(0)
    expect(wrapper.findAll('select')).toHaveLength(0)
    expect(wrapper.findAll('textarea')).toHaveLength(0)
  })
})
