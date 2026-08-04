import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockGet, mockPost, mockGetPaged } = vi.hoisted(() => ({
  mockGet: vi.fn<(...args: unknown[]) => unknown>(),
  mockPost: vi.fn<(...args: unknown[]) => unknown>(),
  mockGetPaged: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('@/shared/api/client', () => ({
  get: mockGet,
  post: mockPost,
}))

vi.mock('@/shared/api', () => ({
  getPaged: mockGetPaged,
}))

import { StockTransferApi } from '../../services/stockTransferApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('StockTransferApi.getStockTransfers', () => {
  it('calls getPaged with transfer query params and allowed fields', async () => {
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

    await StockTransferApi.getStockTransfers({
      state: 'InTransit',
      sourceLocationId: 'l-1',
      destinationLocationId: 'l-2',
      sortBy: 'createdAtUtc',
      sortDirection: 'desc',
      page: 1,
      pageSize: 10,
    })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/inventory/stock-transfers',
      {
        filter: 'state=InTransit,sourceLocationId=l-1,destinationLocationId=l-2',
        search: null,
        sort: ['-createdAtUtc'],
        pageNumber: 1,
        pageSize: 10,
      },
      expect.objectContaining({
        allowedFilterFields: ['state', 'sourceLocationId', 'destinationLocationId'],
        allowedSortFields: ['number', 'state', 'createdAtUtc'],
        allowedSearchFields: [],
      }),
    )
  })
})

describe('StockTransferApi.getStockTransfer', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: 't-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await StockTransferApi.getStockTransfer('t-1')
    expect(mockGet).toHaveBeenCalledWith('api/inventory/stock-transfers/t-1')
  })
})

describe('StockTransferApi.createStockTransfer', () => {
  it('calls POST with request body', async () => {
    const req = { reference: 'TR-1', sourceLocationId: 'l-1', destinationLocationId: 'l-2', items: [{ variantId: 'v-1', quantity: 5 }] }
    mockPost.mockResolvedValue({ value: { id: 't-1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await StockTransferApi.createStockTransfer(req)
    expect(mockPost).toHaveBeenCalledWith('api/inventory/stock-transfers', req)
  })
})

describe('StockTransferApi.transferStockTransfer', () => {
  it('calls POST with transfer URL and no body', async () => {
    mockPost.mockResolvedValue({ value: null, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await StockTransferApi.transferStockTransfer('t-1')
    expect(mockPost).toHaveBeenCalledWith('api/inventory/stock-transfers/t-1/transfer')
  })
})

describe('StockTransferApi.receiveStockTransfer', () => {
  it('calls POST with receive URL and request body', async () => {
    const req = { items: [{ variantId: 'v-1', quantity: 5 }] }
    mockPost.mockResolvedValue({ value: null, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await StockTransferApi.receiveStockTransfer('t-1', req)
    expect(mockPost).toHaveBeenCalledWith('api/inventory/stock-transfers/t-1/receive', req)
  })
})

describe('StockTransferApi.cancelStockTransfer', () => {
  it('calls POST with cancel URL and no body', async () => {
    mockPost.mockResolvedValue({ value: null, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await StockTransferApi.cancelStockTransfer('t-1')
    expect(mockPost).toHaveBeenCalledWith('api/inventory/stock-transfers/t-1/cancel')
  })
})
