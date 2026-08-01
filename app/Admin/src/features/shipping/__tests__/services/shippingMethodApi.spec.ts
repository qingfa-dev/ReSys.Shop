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

import { ShippingMethodApi } from '../../services/shippingMethodApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('ShippingMethodApi.getShippingMethods', () => {
  it('calls getPaged with shipping method query params and allowed fields', async () => {
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

    await ShippingMethodApi.getShippingMethods({
      availableToUsers: true,
      calculatorType: 'FlatRate',
      search: 'express',
      sortBy: 'name',
      sortDirection: 'desc',
      page: 1,
      pageSize: 10,
    })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/shipping/shipping-methods',
      {
        filter: 'availableToUsers=true,calculatorType=FlatRate',
        search: 'express',
        sort: ['-name'],
        pageNumber: 1,
        pageSize: 10,
      },
      expect.objectContaining({
        allowedFilterFields: ['availableToUsers', 'calculatorType', 'taxCategoryId', 'isDeleted'],
        allowedSortFields: ['name', 'code', 'position', 'createdAtUtc'],
        allowedSearchFields: ['name', 'code', 'adminName'],
      }),
    )
  })
})

describe('ShippingMethodApi.getShippingMethod', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: 'sm-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await ShippingMethodApi.getShippingMethod('sm-1')
    expect(mockGet).toHaveBeenCalledWith('api/shipping/shipping-methods/sm-1')
  })
})

describe('ShippingMethodApi.createShippingMethod', () => {
  it('calls POST with request body', async () => {
    const req = {
      name: 'Express',
      code: 'express',
      position: 1,
      availableToUsers: true,
      calculatorType: 'FlatRate',
    }
    mockPost.mockResolvedValue({ value: { id: 'sm-1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await ShippingMethodApi.createShippingMethod(req)
    expect(mockPost).toHaveBeenCalledWith('api/shipping/shipping-methods', req)
  })
})

describe('ShippingMethodApi.updateShippingMethod', () => {
  it('calls PUT with request body', async () => {
    const req = { name: 'Express Updated' }
    mockPut.mockResolvedValue({ value: { id: 'sm-1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await ShippingMethodApi.updateShippingMethod('sm-1', req)
    expect(mockPut).toHaveBeenCalledWith('api/shipping/shipping-methods/sm-1', req)
  })
})

describe('ShippingMethodApi.deleteShippingMethod', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: null, isSuccess: true, statusCode: 204, message: null, errors: [], metadata: null })
    await ShippingMethodApi.deleteShippingMethod('sm-1')
    expect(mockDel).toHaveBeenCalledWith('api/shipping/shipping-methods/sm-1')
  })
})

describe('ShippingMethodApi.activateShippingMethod', () => {
  it('calls PATCH with activate URL', async () => {
    mockPatch.mockResolvedValue({ value: null, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await ShippingMethodApi.activateShippingMethod('sm-1')
    expect(mockPatch).toHaveBeenCalledWith('api/shipping/shipping-methods/sm-1/activate')
  })
})

describe('ShippingMethodApi.deactivateShippingMethod', () => {
  it('calls PATCH with deactivate URL', async () => {
    mockPatch.mockResolvedValue({ value: null, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await ShippingMethodApi.deactivateShippingMethod('sm-1')
    expect(mockPatch).toHaveBeenCalledWith('api/shipping/shipping-methods/sm-1/deactivate')
  })
})
