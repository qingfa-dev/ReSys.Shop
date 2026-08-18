import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import type { DOMWrapper } from '@vue/test-utils'
import PrimeVue from 'primevue/config'
import OrderDetail from '../../views/OrderDetail.vue'
import { OrderApi } from '../../services/orderApi'
import type { OrderDetail as OrderDetailType } from '../../types/order'

if (!window.matchMedia) {
  window.matchMedia = (query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: () => {},
    removeListener: () => {},
    addEventListener: () => {},
    removeEventListener: () => {},
    dispatchEvent: () => false,
  })
}

if (!window.ResizeObserver) {
  window.ResizeObserver = class {
    observe() {}
    unobserve() {}
    disconnect() {}
  } as unknown as typeof ResizeObserver
}

const { confirmRequire, toastAdd } = vi.hoisted(() => ({
  confirmRequire: vi.fn<(...args: unknown[]) => unknown>(),
  toastAdd: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('primevue/usetoast', () => ({
  useToast: () => ({ add: toastAdd }),
}))

vi.mock('primevue/useconfirm', () => ({
  useConfirm: () => ({ require: confirmRequire }),
}))

vi.mock('../../services/orderApi', () => ({
  OrderApi: {
    getOrder: vi.fn<(...args: unknown[]) => unknown>(),
    getLineItems: vi.fn<(...args: unknown[]) => unknown>(),
    approveOrder: vi.fn<(...args: unknown[]) => unknown>(),
    cancelOrder: vi.fn<(...args: unknown[]) => unknown>(),
    completeOrder: vi.fn<(...args: unknown[]) => unknown>(),
    resumeOrder: vi.fn<(...args: unknown[]) => unknown>(),
    updateShipmentStatus: vi.fn<(...args: unknown[]) => unknown>(),
  },
}))

const orderApiMock = OrderApi as unknown as {
  getOrder: ReturnType<typeof vi.fn>
  getLineItems: ReturnType<typeof vi.fn>
  approveOrder: ReturnType<typeof vi.fn>
  cancelOrder: ReturnType<typeof vi.fn>
  completeOrder: ReturnType<typeof vi.fn>
  resumeOrder: ReturnType<typeof vi.fn>
  updateShipmentStatus: ReturnType<typeof vi.fn>
}

function makeOrder(): OrderDetailType {
  return {
    id: 'o-1',
    number: '1001',
    status: 'Placed',
    checkoutState: 'Complete',
    currency: 'USD',
    itemTotal: 200,
    adjustmentTotal: 0,
    shipmentTotal: 10,
    adjustments: [],
    total: 210,
    paymentTotal: 0,
    outstandingBalance: 210,
    itemCount: 2,
    createdAtUtc: '2026-01-01T00:00:00Z',
    lineItems: [],
    payments: [],
    shipments: [
      {
        id: 's-ready',
        orderId: 'o-1',
        shippingMethodId: 'sm-1',
        shippingMethodName: 'Standard',
        trackingNumber: '',
        status: 'Ready',
        shippedAtUtc: null,
        deliveredAtUtc: null,
        estimatedDeliveryAtUtc: null,
        createdAtUtc: '2026-01-01T00:00:00Z',
      },
      {
        id: 's-shipped',
        orderId: 'o-1',
        shippingMethodId: 'sm-2',
        shippingMethodName: 'Express',
        trackingNumber: 'TRK-9',
        status: 'Shipped',
        shippedAtUtc: '2026-01-02T00:00:00Z',
        deliveredAtUtc: null,
        estimatedDeliveryAtUtc: null,
        createdAtUtc: '2026-01-01T00:00:00Z',
      },
    ],
    timeline: [],
  }
}

function shipmentRows(wrapper: ReturnType<typeof mount>) {
  const tables = wrapper.findAll('table')
  const table = tables[0]
  return table.findAll('tbody tr')
}

function rowButtons(row: DOMWrapper<Element>) {
  return row.findAll('button').map(b => b.text().trim())
}

function rowForShippingMethod(wrapper: ReturnType<typeof mount>, shippingMethod: string) {
  const row = shipmentRows(wrapper).find(r => r.text().includes(shippingMethod))
  expect(row).toBeTruthy()
  return row!
}

async function mountWith(order: OrderDetailType) {
  orderApiMock.getOrder.mockResolvedValue({
    isSuccess: true,
    statusCode: 200,
    message: null,
    errors: [],
    metadata: null,
    value: order,
  })
  const router = createRouter({
    history: createMemoryHistory(),
    routes: [{ path: '/ordering/orders/:id', component: OrderDetail }],
  })
  router.push('/ordering/orders/o-1')
  await router.isReady()
  const wrapper = mount(OrderDetail, { global: { plugins: [PrimeVue, router] } })
  await flushPromises()
  return wrapper
}

describe('OrderDetail shipment quick actions', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('enables quick actions per shipment status', async () => {
    const wrapper = await mountWith(makeOrder())

    const readyRow = rowForShippingMethod(wrapper, 'Standard')
    expect(rowButtons(readyRow)).toEqual(['Save', 'Mark Shipped', 'Mark Delivered'])

    const shippedRow = rowForShippingMethod(wrapper, 'Express')
    expect(rowButtons(shippedRow)).toEqual(['Save', 'Mark Shipped', 'Mark Delivered'])

    expect(readyRow.findAll('button')[1].attributes('disabled')).toBeUndefined()
    expect(readyRow.findAll('button')[2].attributes('disabled')).toBeDefined()
    expect(shippedRow.findAll('button')[1].attributes('disabled')).toBeDefined()
    expect(shippedRow.findAll('button')[2].attributes('disabled')).toBeUndefined()
  })

  it('disables Mark Shipped for Backorder and Pending shipments', async () => {
    const order = makeOrder()
    order.shipments = [
      {
        id: 's-backorder',
        orderId: 'o-1',
        shippingMethodId: 'sm-3',
        shippingMethodName: 'Backorder Ship',
        trackingNumber: '',
        status: 'Backorder',
        shippedAtUtc: null,
        deliveredAtUtc: null,
        estimatedDeliveryAtUtc: null,
        createdAtUtc: '2026-01-01T00:00:00Z',
      },
      {
        id: 's-pending',
        orderId: 'o-1',
        shippingMethodId: 'sm-4',
        shippingMethodName: 'Pending Ship',
        trackingNumber: '',
        status: 'Pending',
        shippedAtUtc: null,
        deliveredAtUtc: null,
        estimatedDeliveryAtUtc: null,
        createdAtUtc: '2026-01-01T00:00:00Z',
      },
    ]
    const wrapper = await mountWith(order)

    const backorderRow = rowForShippingMethod(wrapper, 'Backorder Ship')
    const pendingRow = rowForShippingMethod(wrapper, 'Pending Ship')

    expect(backorderRow.findAll('button')[1].attributes('disabled')).toBeDefined()
    expect(pendingRow.findAll('button')[1].attributes('disabled')).toBeDefined()
  })

  it('marks a Ready shipment as Shipped with the tracking number', async () => {
    const wrapper = await mountWith(makeOrder())
    orderApiMock.updateShipmentStatus.mockResolvedValue({
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
      value: { id: 's-ready', status: 'Shipped' },
    })

    const readyRow = rowForShippingMethod(wrapper, 'Standard')
    await readyRow.find('input').setValue('TRK-123')
    await readyRow.findAll('button')[1].trigger('click')
    await flushPromises()

    expect(orderApiMock.updateShipmentStatus).toHaveBeenCalledWith('s-ready', {
      status: 'Shipped',
      trackingNumber: 'TRK-123',
    })
    expect(toastAdd).toHaveBeenCalledWith(expect.objectContaining({ severity: 'success' }))
    expect(orderApiMock.getOrder).toHaveBeenCalledTimes(2)
  })

  it('blocks Mark Shipped when the tracking number is empty', async () => {
    const wrapper = await mountWith(makeOrder())
    const readyRow = rowForShippingMethod(wrapper, 'Standard')
    await readyRow.findAll('button')[1].trigger('click')
    await flushPromises()

    expect(orderApiMock.updateShipmentStatus).not.toHaveBeenCalled()
    expect(toastAdd).toHaveBeenCalledWith(expect.objectContaining({ severity: 'error' }))
  })

  it('marks a Shipped shipment as Delivered without a tracking number', async () => {
    const wrapper = await mountWith(makeOrder())
    orderApiMock.updateShipmentStatus.mockResolvedValue({
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
      value: { id: 's-shipped', status: 'Delivered' },
    })

    const shippedRow = rowForShippingMethod(wrapper, 'Express')
    await shippedRow.findAll('button')[2].trigger('click')
    await flushPromises()

    expect(orderApiMock.updateShipmentStatus).toHaveBeenCalledWith('s-shipped', {
      status: 'Delivered',
    })
    expect(toastAdd).toHaveBeenCalledWith(expect.objectContaining({ severity: 'success' }))
  })
})