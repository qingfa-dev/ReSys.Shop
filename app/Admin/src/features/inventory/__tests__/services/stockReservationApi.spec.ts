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

import { StockReservationApi } from '../../services/stockReservationApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('StockReservationApi.getStockReservations', () => {
  it('calls getPaged with reservation query params and allowed fields', async () => {
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

    await StockReservationApi.getStockReservations({
      state: 'Reserved',
      sortBy: 'createdAtUtc',
      sortDirection: 'desc',
      page: 1,
      pageSize: 10,
    })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/admin/inventory/stock-reservations',
      {
        filter: 'state=Reserved',
        search: null,
        sort: ['-createdAtUtc'],
        pageNumber: 1,
        pageSize: 10,
      },
      expect.objectContaining({
        allowedFilterFields: ['variantId', 'orderId', 'state'],
        allowedSortFields: ['expiresAtUtc', 'createdAtUtc'],
        allowedSearchFields: [],
      }),
    )
  })
})

describe('StockReservationApi.getStockReservation', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: 'r-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await StockReservationApi.getStockReservation('r-1')
    expect(mockGet).toHaveBeenCalledWith('api/admin/inventory/stock-reservations/r-1')
  })
})

describe('StockReservationApi.cancelStockReservation', () => {
  it('calls POST with cancel URL and no body', async () => {
    mockPost.mockResolvedValue({ value: { id: 'r-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await StockReservationApi.cancelStockReservation('r-1')
    expect(mockPost).toHaveBeenCalledWith('api/admin/inventory/stock-reservations/r-1/cancel')
  })
})
