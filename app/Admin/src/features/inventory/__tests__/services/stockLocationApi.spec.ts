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

import { StockLocationApi } from '../../services/stockLocationApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('StockLocationApi.getStockLocations', () => {
  it('calls getPaged with stock location query params and allowed fields', async () => {
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

    await StockLocationApi.getStockLocations({
      filter: 'active=true',
      search: 'main',
      sort: ['name'],
      pageNumber: 1,
      pageSize: 10,
    })

    expect(mockGetPaged).toHaveBeenCalledWith(
      '/api/admin/inventory/stock-locations',
      {
        filter: 'active=true',
        search: 'main',
        sort: ['name'],
        pageNumber: 1,
        pageSize: 10,
      },
      expect.objectContaining({
        allowedFilterFields: ['active', 'default', 'backorderableDefault', 'isDeleted', 'countryId', 'stateId'],
        allowedSortFields: ['name', 'code', 'position', 'createdAtUtc'],
        allowedSearchFields: ['name', 'code', 'city', 'adminName'],
      }),
    )
  })
})

describe('StockLocationApi.getStockLocation', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: 'l-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await StockLocationApi.getStockLocation('l-1')
    expect(mockGet).toHaveBeenCalledWith('/api/admin/inventory/stock-locations/l-1')
  })
})

describe('StockLocationApi.createStockLocation', () => {
  it('calls POST with request body', async () => {
    const req = { name: 'Main', active: true, default: false, backorderableDefault: true, propagateAllVariants: true, position: 0 }
    mockPost.mockResolvedValue({ value: { id: 'l-1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await StockLocationApi.createStockLocation(req)
    expect(mockPost).toHaveBeenCalledWith('/api/admin/inventory/stock-locations', req)
  })
})

describe('StockLocationApi.updateStockLocation', () => {
  it('calls PUT with request body', async () => {
    const req = { name: 'Main Store', active: true, default: false, backorderableDefault: true, propagateAllVariants: true, position: 1 }
    mockPut.mockResolvedValue({ value: { id: 'l-1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await StockLocationApi.updateStockLocation('l-1', req)
    expect(mockPut).toHaveBeenCalledWith('/api/admin/inventory/stock-locations/l-1', req)
  })
})

describe('StockLocationApi.deleteStockLocation', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: null, isSuccess: true, statusCode: 204, message: null, errors: [], metadata: null })
    await StockLocationApi.deleteStockLocation('l-1')
    expect(mockDel).toHaveBeenCalledWith('/api/admin/inventory/stock-locations/l-1')
  })
})

describe('StockLocationApi.setDefaultStockLocation', () => {
  it('calls PUT with default URL and no body', async () => {
    mockPut.mockResolvedValue({ value: { id: 'l-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await StockLocationApi.setDefaultStockLocation('l-1')
    expect(mockPut).toHaveBeenCalledWith('/api/admin/inventory/stock-locations/l-1/default')
  })
})
