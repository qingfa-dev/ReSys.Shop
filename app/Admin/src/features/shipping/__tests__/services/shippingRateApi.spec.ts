import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockGet, mockPut, mockDel, mockGetPaged } = vi.hoisted(() => ({
  mockPost: vi.fn<(...args: unknown[]) => unknown>(),
  mockGet: vi.fn<(...args: unknown[]) => unknown>(),
  mockPut: vi.fn<(...args: unknown[]) => unknown>(),
  mockDel: vi.fn<(...args: unknown[]) => unknown>(),
  mockGetPaged: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('@/shared/api/client', () => ({
  post: mockPost,
  get: mockGet,
  put: mockPut,
  del: mockDel,
}))

vi.mock('@/shared/api', () => ({
  getPaged: mockGetPaged,
}))

import { ShippingRateApi } from '../../services/shippingRateApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('ShippingRateApi.getShippingRates', () => {
  it('calls getPaged with shipping rate query params and allowed fields', async () => {
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

    await ShippingRateApi.getShippingRates({
      shippingMethodId: 'sm-1',
      selected: true,
      sortBy: 'cost',
      sortDirection: 'asc',
      page: 1,
      pageSize: 10,
    })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/shipping/shipping-rates',
      {
        filter: 'shippingMethodId=sm-1,selected=true',
        search: null,
        sort: ['cost'],
        pageNumber: 1,
        pageSize: 10,
      },
      expect.objectContaining({
        allowedFilterFields: ['selected', 'shippingMethodId'],
        allowedSortFields: ['name', 'cost', 'finalPrice', 'selected', 'createdAtUtc'],
        allowedSearchFields: ['name', 'deliveryRange'],
      }),
    )
  })
})

describe('ShippingRateApi.getShippingRate', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: 'sr-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await ShippingRateApi.getShippingRate('sr-1')
    expect(mockGet).toHaveBeenCalledWith('api/shipping/shipping-rates/sr-1')
  })
})

describe('ShippingRateApi.createShippingRate', () => {
  it('calls POST with request body', async () => {
    const req = {
      name: 'Standard',
      cost: 5,
      minWeight: 0,
      shippingMethodId: 'sm-1',
    }
    mockPost.mockResolvedValue({ value: { id: 'sr-1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await ShippingRateApi.createShippingRate(req)
    expect(mockPost).toHaveBeenCalledWith('api/shipping/shipping-rates', req)
  })
})

describe('ShippingRateApi.updateShippingRate', () => {
  it('calls PUT with request body', async () => {
    const req = { name: 'Standard Updated' }
    mockPut.mockResolvedValue({ value: { id: 'sr-1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await ShippingRateApi.updateShippingRate('sr-1', req)
    expect(mockPut).toHaveBeenCalledWith('api/shipping/shipping-rates/sr-1', req)
  })
})

describe('ShippingRateApi.deleteShippingRate', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: null, isSuccess: true, statusCode: 204, message: null, errors: [], metadata: null })
    await ShippingRateApi.deleteShippingRate('sr-1')
    expect(mockDel).toHaveBeenCalledWith('api/shipping/shipping-rates/sr-1')
  })
})
