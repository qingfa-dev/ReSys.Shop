import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockGet, mockPut, mockDel, mockGetPaged } = vi.hoisted(() => ({
  mockPost: vi.fn<any>(),
  mockGet: vi.fn<any>(),
  mockPut: vi.fn<any>(),
  mockDel: vi.fn<any>(),
  mockGetPaged: vi.fn<any>(),
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

import { StockItemApi } from '../../services/stockItemApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('StockItemApi.getStockItems', () => {
  it('calls getPaged with stock item query params and allowed fields', async () => {
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

    await StockItemApi.getStockItems({
      stockLocationId: 'l-1',
      variantId: 'v-1',
      backorderable: true,
      sortBy: 'countOnHand',
      sortDirection: 'desc',
      page: 1,
      pageSize: 10,
    })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/inventory/stock-items',
      {
        filter: 'StockLocationId=l-1,VariantId=v-1,Backorderable=true',
        search: null,
        sort: ['-countOnHand'],
        pageNumber: 1,
        pageSize: 10,
      },
      expect.objectContaining({
        allowedFilterFields: ['StockLocationId', 'VariantId', 'Backorderable'],
        allowedSortFields: ['CountOnHand', 'CreatedAtUtc'],
        allowedSearchFields: [],
      }),
    )
  })
})

describe('StockItemApi.getStockItem', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: 's-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await StockItemApi.getStockItem('s-1')
    expect(mockGet).toHaveBeenCalledWith('api/inventory/stock-items/s-1')
  })
})

describe('StockItemApi.createStockItem', () => {
  it('calls POST with request body', async () => {
    const req = { stockLocationId: 'l-1', variantId: 'v-1', countOnHand: 10, backorderable: true }
    mockPost.mockResolvedValue({ value: { id: 's-1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await StockItemApi.createStockItem(req)
    expect(mockPost).toHaveBeenCalledWith('api/inventory/stock-items', req)
  })
})

describe('StockItemApi.updateStockItem', () => {
  it('calls PUT with request body', async () => {
    const req = { stockLocationId: 'l-1', variantId: 'v-1', countOnHand: 15, backorderable: false }
    mockPut.mockResolvedValue({ value: { id: 's-1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await StockItemApi.updateStockItem('s-1', req)
    expect(mockPut).toHaveBeenCalledWith('api/inventory/stock-items/s-1', req)
  })
})

describe('StockItemApi.deleteStockItem', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: null, isSuccess: true, statusCode: 204, message: null, errors: [], metadata: null })
    await StockItemApi.deleteStockItem('s-1')
    expect(mockDel).toHaveBeenCalledWith('api/inventory/stock-items/s-1')
  })
})

describe('StockItemApi.bulkAdjustStockItems', () => {
  it('calls POST with bulk-adjust URL and request body', async () => {
    const req = {
      stockLocationId: 'l-1',
      variantId: 'v-1',
      countOnHand: 0,
      backorderable: false,
      items: [{ stockItemId: 's-1', quantity: 5 }],
      reason: 'recount',
    }
    mockPost.mockResolvedValue({ value: null, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await StockItemApi.bulkAdjustStockItems(req)
    expect(mockPost).toHaveBeenCalledWith('api/inventory/stock-items/bulk-adjust', req)
  })
})

describe('StockItemApi.restockStockItem', () => {
  it('calls POST with restock URL and request body', async () => {
    const req = { quantity: 10, reference: 'po-1', reason: 'received' }
    mockPost.mockResolvedValue({ value: { stockItemId: 's-1', newCountOnHand: 10 }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await StockItemApi.restockStockItem('s-1', req)
    expect(mockPost).toHaveBeenCalledWith('api/inventory/stock-items/s-1/restock', req)
  })
})

describe('StockItemApi.getLowStockItems', () => {
  it('calls getPaged with low-stock URL including dedicated params', async () => {
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

    await StockItemApi.getLowStockItems({ locationId: 'l-1', threshold: 5, page: 1, pageSize: 10 })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/inventory/stock-items/low-stock?locationId=l-1&threshold=5',
      { pageNumber: 1, pageSize: 10 },
    )
  })
})

describe('StockItemApi.getStockSummary', () => {
  it('calls getPaged with summary URL and page params', async () => {
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

    await StockItemApi.getStockSummary({ page: 1, pageSize: 10 })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/inventory/stock-items/summary',
      { pageNumber: 1, pageSize: 10 },
    )
  })
})

describe('StockItemApi.importStockItems', () => {
  it('calls POST with import URL and FormData body', async () => {
    mockPost.mockResolvedValue({ value: { created: 1, updated: 0, failed: 0, errors: [] }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await StockItemApi.importStockItems(new File(['csv'], 'stock.csv', { type: 'text/csv' }))
    expect(mockPost).toHaveBeenCalledWith('api/inventory/stock-items/import', expect.any(FormData))
  })
})
