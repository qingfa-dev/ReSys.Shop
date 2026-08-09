import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockGet, mockGetPaged } = vi.hoisted(() => ({
  mockGet: vi.fn<(...args: unknown[]) => unknown>(),
  mockGetPaged: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('@/shared/api/client', () => ({
  get: mockGet,
}))

vi.mock('@/shared/api', () => ({
  getPaged: mockGetPaged,
}))

import { StockMovementApi } from '../../services/stockMovementApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('StockMovementApi.getStockMovements', () => {
  it('calls getPaged with dedicated params in URL and nulled dedicated fields', async () => {
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

    await StockMovementApi.getStockMovements({
      fromUtc: '2026-01-01T08:30:00Z',
      toUtc: '2026-01-31',
      variantId: 'v-1',
      stockLocationId: 'l-1',
      page: 1,
      pageSize: 10,
    })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/admin/inventory/stock-movements?fromUtc=2026-01-01T08%3A30%3A00Z&toUtc=2026-01-31&variantId=v-1&stockLocationId=l-1',
      expect.objectContaining({ pageNumber: 1, pageSize: 10 }),
      expect.objectContaining({
        allowedFilterFields: ['stockItemId', 'originatorType'],
        allowedSortFields: ['quantity', 'createdAtUtc'],
        allowedSearchFields: ['reason'],
      }),
    )
  })

  it('omits query string when no dedicated params are provided', async () => {
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

    await StockMovementApi.getStockMovements({ page: 1, pageSize: 10 })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/admin/inventory/stock-movements',
      expect.objectContaining({ pageNumber: 1, pageSize: 10 }),
      expect.any(Object),
    )
  })

  it('does not pass dedicated params into query params object', async () => {
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

    await StockMovementApi.getStockMovements({
      fromUtc: '2026-01-01',
      toUtc: '2026-01-31',
      variantId: 'v-1',
      stockLocationId: 'l-1',
      page: 1,
      pageSize: 10,
    })

    const params = mockGetPaged.mock.calls[0]?.[1]
    expect(params).toMatchObject({
      pageNumber: 1,
      pageSize: 10,
      fromUtc: null,
      toUtc: null,
      variantId: null,
      stockLocationId: null,
    })
  })
})

describe('StockMovementApi.getStockMovement', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: 'm-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await StockMovementApi.getStockMovement('m-1')
    expect(mockGet).toHaveBeenCalledWith('api/admin/inventory/stock-movements/m-1')
  })
})
