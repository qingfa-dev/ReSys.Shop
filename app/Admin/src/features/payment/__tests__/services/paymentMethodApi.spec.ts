import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockGet, mockPut, mockDel, mockPatch, mockGetPaged } = vi.hoisted(() => ({
  mockPost: vi.fn<any>(),
  mockGet: vi.fn<any>(),
  mockPut: vi.fn<any>(),
  mockDel: vi.fn<any>(),
  mockPatch: vi.fn<any>(),
  mockGetPaged: vi.fn<any>(),
}))

vi.mock('@/shared/api/client', () => ({
  post: mockPost,
  get: mockGet,
  put: mockPut,
  del: mockDel,
  patch: mockPatch,
}))

vi.mock('@/shared/api', () => ({
  getPaged: mockGetPaged,
}))

import { PaymentMethodApi } from '../../services/paymentMethodApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('PaymentMethodApi.getPaymentMethods', () => {
  it('calls getPaged with payment method query params and allowed fields', async () => {
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

    await PaymentMethodApi.getPaymentMethods({
      active: true,
      providerKey: 'stripe',
      autoCapture: true,
      sortBy: 'name',
      sortDirection: 'desc',
      page: 1,
      pageSize: 10,
    })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/payment/payment-methods',
      {
        filter: 'Active=true,ProviderKey=stripe,AutoCapture=true',
        search: null,
        searchFields: ['Name', 'Code', 'Description'],
        sort: ['-name'],
        pageNumber: 1,
        pageSize: 10,
      },
      expect.objectContaining({
        allowedFilterFields: ['Active', 'ProviderKey', 'AutoCapture', 'DisplayOn', 'IsDeleted'],
        allowedSortFields: ['Name', 'Position', 'CreatedAtUtc'],
        allowedSearchFields: ['Name', 'Code', 'Description'],
      }),
    )
  })
})

describe('PaymentMethodApi.getPaymentMethod', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: 'pm-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await PaymentMethodApi.getPaymentMethod('pm-1')
    expect(mockGet).toHaveBeenCalledWith('api/payment/payment-methods/pm-1')
  })
})

describe('PaymentMethodApi.createPaymentMethod', () => {
  it('calls POST with request body', async () => {
    const req = {
      name: 'Card',
      providerKey: 'stripe',
      webhookEnabled: true,
      autoCapture: true,
      displayOn: 'Both',
      position: 1,
      active: true,
    }
    mockPost.mockResolvedValue({ value: { id: 'pm-1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await PaymentMethodApi.createPaymentMethod(req)
    expect(mockPost).toHaveBeenCalledWith('api/payment/payment-methods', req)
  })
})

describe('PaymentMethodApi.updatePaymentMethod', () => {
  it('calls PUT with request body', async () => {
    const req = { name: 'Card Updated' }
    mockPut.mockResolvedValue({ value: { id: 'pm-1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await PaymentMethodApi.updatePaymentMethod('pm-1', req)
    expect(mockPut).toHaveBeenCalledWith('api/payment/payment-methods/pm-1', req)
  })
})

describe('PaymentMethodApi.deletePaymentMethod', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: null, isSuccess: true, statusCode: 204, message: null, errors: [], metadata: null })
    await PaymentMethodApi.deletePaymentMethod('pm-1')
    expect(mockDel).toHaveBeenCalledWith('api/payment/payment-methods/pm-1')
  })
})

describe('PaymentMethodApi.activatePaymentMethod', () => {
  it('calls PATCH with activate URL', async () => {
    mockPatch.mockResolvedValue({ value: null, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await PaymentMethodApi.activatePaymentMethod('pm-1')
    expect(mockPatch).toHaveBeenCalledWith('api/payment/payment-methods/pm-1/activate')
  })
})

describe('PaymentMethodApi.deactivatePaymentMethod', () => {
  it('calls PATCH with deactivate URL', async () => {
    mockPatch.mockResolvedValue({ value: null, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await PaymentMethodApi.deactivatePaymentMethod('pm-1')
    expect(mockPatch).toHaveBeenCalledWith('api/payment/payment-methods/pm-1/deactivate')
  })
})
