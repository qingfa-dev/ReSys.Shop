import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import PrimeVue from 'primevue/config'
import PaymentsList from '../../views/PaymentsList.vue'
import { PaymentApi } from '../../services/paymentApi'
import type { PaymentListItem } from '../../types/payment'

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

vi.mock('../../services/paymentApi', () => ({
  PaymentApi: {
    getPayments: vi.fn<(...args: unknown[]) => unknown>(),
    capturePayment: vi.fn<(...args: unknown[]) => unknown>(),
    refundPayment: vi.fn<(...args: unknown[]) => unknown>(),
    voidPayment: vi.fn<(...args: unknown[]) => unknown>(),
  },
}))

const paymentApiMock = PaymentApi as unknown as {
  getPayments: ReturnType<typeof vi.fn>
  capturePayment: ReturnType<typeof vi.fn>
  refundPayment: ReturnType<typeof vi.fn>
  voidPayment: ReturnType<typeof vi.fn>
}

function pagedResult(items: PaymentListItem[]) {
  return {
    isSuccess: true,
    statusCode: 200,
    message: null,
    errors: [],
    metadata: null,
    items,
    page: 1,
    pageSize: 10,
    totalCount: items.length,
    totalPages: 1,
  }
}

function payment(id: string, state: PaymentListItem['state']): PaymentListItem {
  return { id, amount: 100, currency: 'USD', orderId: 'o-1', paymentMethodId: 'm-1', state }
}

function findRowButtons(wrapper: ReturnType<typeof mount>, id: string) {
  const rows = wrapper.findAll('tbody tr')
  const row = rows.find(r => r.text().includes(id))
  expect(row).toBeTruthy()
  return row!.findAll('button').map(b => b.text().trim())
}

async function mountWith(items: PaymentListItem[]) {
  paymentApiMock.getPayments.mockResolvedValue(pagedResult(items))
  const wrapper = mount(PaymentsList, { global: { plugins: [PrimeVue] } })
  await flushPromises()
  return wrapper
}

describe('PaymentsList', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders action buttons gated by payment state', async () => {
    const wrapper = await mountWith([
      payment('p-pending', 'Pending'),
      payment('p-processing', 'Processing'),
      payment('p-completed', 'Completed'),
      payment('p-failed', 'Failed'),
    ])

    expect(findRowButtons(wrapper, 'p-pending')).toEqual(['Capture', 'Void'])
    expect(findRowButtons(wrapper, 'p-processing')).toEqual(['Capture', 'Void'])
    expect(findRowButtons(wrapper, 'p-completed')).toEqual(['Refund'])
    expect(findRowButtons(wrapper, 'p-failed')).toEqual([])
  })

  it('calls PaymentApi.capturePayment on confirmed capture', async () => {
    const wrapper = await mountWith([payment('p-pending', 'Pending')])
    paymentApiMock.capturePayment.mockResolvedValue({
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
      value: { id: 'p-pending', capturedAmount: 100, message: 'ok' },
    })

    await wrapper.findAll('tbody tr')[0].findAll('button')[0].trigger('click')
    await flushPromises()

    expect(confirmRequire).toHaveBeenCalledTimes(1)
    const opts = confirmRequire.mock.calls[0][0]
    await opts.accept()
    await flushPromises()

    expect(paymentApiMock.capturePayment).toHaveBeenCalledWith('p-pending')
    expect(toastAdd).toHaveBeenCalledWith(expect.objectContaining({ severity: 'success' }))
    expect(paymentApiMock.getPayments).toHaveBeenCalledTimes(2)
  })

  it('calls PaymentApi.refundPayment with full amount on confirmed refund', async () => {
    const wrapper = await mountWith([payment('p-completed', 'Completed')])
    paymentApiMock.refundPayment.mockResolvedValue({
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
      value: { id: 'p-completed', refundedAmount: 100, message: 'ok' },
    })

    await wrapper.findAll('tbody tr')[0].findAll('button')[0].trigger('click')
    await flushPromises()

    const opts = confirmRequire.mock.calls[0][0]
    await opts.accept()
    await flushPromises()

    expect(paymentApiMock.refundPayment).toHaveBeenCalledWith('p-completed', { amount: 100 })
    expect(paymentApiMock.getPayments).toHaveBeenCalledTimes(2)
  })

  it('calls PaymentApi.voidPayment on confirmed void', async () => {
    const wrapper = await mountWith([payment('p-processing', 'Processing')])
    paymentApiMock.voidPayment.mockResolvedValue({
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
      value: { id: 'p-processing', message: 'voided' },
    })

    await wrapper.findAll('tbody tr')[0].findAll('button')[1].trigger('click')
    await flushPromises()

    const opts = confirmRequire.mock.calls[0][0]
    await opts.accept()
    await flushPromises()

    expect(paymentApiMock.voidPayment).toHaveBeenCalledWith('p-processing')
    expect(paymentApiMock.getPayments).toHaveBeenCalledTimes(2)
  })

  it('renders a state tag with the payment severity map', async () => {
    const wrapper = await mountWith([payment('p-completed', 'Completed')])
    expect(wrapper.text()).toContain('Completed')
    const tag = wrapper.find('span.p-tag')
    expect(tag.exists()).toBe(true)
  })
})