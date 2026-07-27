import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { StockMovementApi } from '../stock-movement.api'

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

describe('StockMovementApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  describe('getMany', () => {
    it('calls GET /inventory/stock-movements with serialized query params', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: pagedEmpty })
      await StockMovementApi.getMany(defaultQuery)
      expect(apiClient.get).toHaveBeenCalledWith('/inventory/stock-movements', {
        params: {
          'page.page': 1,
          'page.pageSize': 20,
          'sort.clauses[0].field': 'createdAt',
          'sort.clauses[0].direction': 'Descending',
        },
      })
    })

    it('includes search params when query has search', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: pagedEmpty })
      await StockMovementApi.getMany({ ...defaultQuery, search: { value: 'IN-1' } })
      expect(apiClient.get).toHaveBeenCalledWith('/inventory/stock-movements', {
        params: expect.objectContaining({ 'search.term.value': 'IN-1' }),
      })
    })
  })

  describe('get', () => {
    it('calls GET /inventory/stock-movements/:id', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ id: '1', type: 'In' }) })
      await StockMovementApi.get('1')
      expect(apiClient.get).toHaveBeenCalledWith('/inventory/stock-movements/1')
    })
  })
})
