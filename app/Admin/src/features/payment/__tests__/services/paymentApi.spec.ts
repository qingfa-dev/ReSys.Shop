import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockGet, mockPost, mockGetPaged } = vi.hoisted(() => ({
  mockGet: vi.fn<any>(),
  mockPost: vi.fn<any>(),
  mockGetPaged: vi.fn<any>(),
}))

vi.mock('@/shared/api/client', () => ({
  get: mockGet,
  post: mockPost,
}))

vi.mock('@/shared/api', () => ({
  getPaged: mockGetPaged,
}))

import { PaymentApi } from '../../services/paymentApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('PaymentApi.getPayments', () => {
  it('calls getPaged with payment query params and allowed fields', async () => {
    mockGetPaged.mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 10,
      totalCount: 0,
      totalPages: 0,
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
    })

    await PaymentApi.getPayments({
      state: 'Captured',
      orderId: 'o-1',
      sortBy: 'createdAtUtc',
      sortDirection: 'desc',
      page: 1,
      pageSize: 10,
    })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/payment/payments',
      {
        filter: 'state=Captured,orderId=o-1',
        search: null,
        searchFields: ['number'],
        sort: ['-createdAtUtc'],
        pageNumber: 1,
        pageSize: 10,
      },
      expect.objectContaining({
        allowedFilterFields: ['state', 'paymentMethodId', 'orderId'],
        allowedSortFields: ['number', 'amount', 'state', 'createdAtUtc'],
        allowedSearchFields: ['number'],
      }),
    )
  })
})

describe('PaymentApi.getPayment', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: 'p-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await PaymentApi.getPayment('p-1')
    expect(mockGet).toHaveBeenCalledWith('api/payment/payments/p-1')
  })
})

describe('PaymentApi.capturePayment', () => {
  it('calls POST with capture URL and amount body', async () => {
    mockPost.mockResolvedValue({ value: { id: 'p-1', capturedAmount: 10, message: 'ok' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await PaymentApi.capturePayment('p-1', { amount: 10 })
    expect(mockPost).toHaveBeenCalledWith('api/payment/payments/p-1/capture', { amount: 10 })
  })

  it('calls POST with empty body when no amount given', async () => {
    mockPost.mockResolvedValue({ value: { id: 'p-1', capturedAmount: 0, message: 'ok' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await PaymentApi.capturePayment('p-1')
    expect(mockPost).toHaveBeenCalledWith('api/payment/payments/p-1/capture', {})
  })
})

describe('PaymentApi.refundPayment', () => {
  it('calls POST with refund URL and request body', async () => {
    mockPost.mockResolvedValue({ value: { id: 'p-1', refundedAmount: 5, message: 'ok' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await PaymentApi.refundPayment('p-1', { amount: 5, reason: 'x' })
    expect(mockPost).toHaveBeenCalledWith('api/payment/payments/p-1/refund', { amount: 5, reason: 'x' })
  })
})

describe('PaymentApi.voidPayment', () => {
  it('calls POST with void URL', async () => {
    mockPost.mockResolvedValue({ value: { id: 'p-1', message: 'voided' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await PaymentApi.voidPayment('p-1')
    expect(mockPost).toHaveBeenCalledWith('api/payment/payments/p-1/void')
  })
})
