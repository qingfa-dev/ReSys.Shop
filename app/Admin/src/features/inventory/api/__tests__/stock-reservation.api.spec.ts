import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { StockReservationApi } from '../stock-reservation.api'

vi.mock('@/shared/api/client', () => ({
  default: {
    get: vi.fn<(...args: unknown[]) => unknown>(),
    post: vi.fn<(...args: unknown[]) => unknown>(),
    put: vi.fn<(...args: unknown[]) => unknown>(),
    delete: vi.fn<(...args: unknown[]) => unknown>(),
  },
}))

const pagedEmpty = { isSuccess: true, items: [], page: 1, pageSize: 20, totalCount: 0, statusCode: 200 }
const singleOk = (value: unknown) => ({ isSuccess: true, value, statusCode: 200 })
const defaultQuery = { page: 1, pageSize: 20, sort: [{ field: 'createdAt' as const, direction: 'Descending' as const }] }

describe('StockReservationApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  describe('getMany', () => {
    it('calls GET /inventory/stock-reservations with serialized query params', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: pagedEmpty })
      await StockReservationApi.getMany(defaultQuery)
      expect(apiClient.get).toHaveBeenCalledWith('/inventory/stock-reservations', {
        params: {
          'page.page': 1,
          'page.pageSize': 20,
          'sort.clauses[0].field': 'createdAt',
          'sort.clauses[0].direction': 'Descending',
        },
      })
    })
  })

  describe('get', () => {
    it('calls GET /inventory/stock-reservations/:id', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ id: '1', status: 'Active' }) })
      await StockReservationApi.get('1')
      expect(apiClient.get).toHaveBeenCalledWith('/inventory/stock-reservations/1')
    })
  })

  describe('cancel', () => {
    it('calls POST /inventory/stock-reservations/:id/cancel', async () => {
      vi.mocked(apiClient.post).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await StockReservationApi.cancel('1')
      expect(apiClient.post).toHaveBeenCalledWith('/inventory/stock-reservations/1/cancel')
    })
  })
})
