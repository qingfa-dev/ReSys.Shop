import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { StockTransferApi } from '../stock-transfer.api'

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

describe('StockTransferApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  describe('getMany', () => {
    it('calls GET /inventory/stock-transfers with serialized query params', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: pagedEmpty })
      await StockTransferApi.getMany(defaultQuery)
      expect(apiClient.get).toHaveBeenCalledWith('/inventory/stock-transfers', {
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
    it('calls GET /inventory/stock-transfers/:id', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ id: '1', status: 'Draft' }) })
      await StockTransferApi.get('1')
      expect(apiClient.get).toHaveBeenCalledWith('/inventory/stock-transfers/1')
    })
  })

  describe('create', () => {
    it('calls POST /inventory/stock-transfers with body', async () => {
      const data = { fromLocationId: 'loc-1', toLocationId: 'loc-2', items: [{ stockItemId: 'item-1', quantity: 5 }] }
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '1', ...data, status: 'Draft' }) })
      await StockTransferApi.create(data)
      expect(apiClient.post).toHaveBeenCalledWith('/inventory/stock-transfers', data)
    })
  })

  describe('transfer', () => {
    it('calls POST /inventory/stock-transfers/:id/transfer', async () => {
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '1', status: 'InTransit' }) })
      await StockTransferApi.transfer('1')
      expect(apiClient.post).toHaveBeenCalledWith('/inventory/stock-transfers/1/transfer')
    })
  })

  describe('receive', () => {
    it('calls POST /inventory/stock-transfers/:id/receive', async () => {
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '1', status: 'Completed' }) })
      await StockTransferApi.receive('1')
      expect(apiClient.post).toHaveBeenCalledWith('/inventory/stock-transfers/1/receive')
    })
  })

  describe('cancel', () => {
    it('calls POST /inventory/stock-transfers/:id/cancel', async () => {
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '1', status: 'Cancelled' }) })
      await StockTransferApi.cancel('1')
      expect(apiClient.post).toHaveBeenCalledWith('/inventory/stock-transfers/1/cancel')
    })
  })

  describe('lifecycle', () => {
    it('supports full transfer lifecycle: create, transfer, receive, cancel', async () => {
      const transferData = { fromLocationId: 'loc-1', toLocationId: 'loc-2', items: [{ stockItemId: 'item-1', quantity: 5 }] }

      vi.mocked(apiClient.post).mockResolvedValueOnce({ data: singleOk({ id: '1', ...transferData, status: 'Draft' }) })
      await StockTransferApi.create(transferData)
      expect(apiClient.post).toHaveBeenNthCalledWith(1, '/inventory/stock-transfers', transferData)

      vi.mocked(apiClient.post).mockResolvedValueOnce({ data: singleOk({ id: '1', status: 'InTransit' }) })
      await StockTransferApi.transfer('1')
      expect(apiClient.post).toHaveBeenNthCalledWith(2, '/inventory/stock-transfers/1/transfer')

      vi.mocked(apiClient.post).mockResolvedValueOnce({ data: singleOk({ id: '1', status: 'Completed' }) })
      await StockTransferApi.receive('1')
      expect(apiClient.post).toHaveBeenNthCalledWith(3, '/inventory/stock-transfers/1/receive')

      vi.mocked(apiClient.post).mockResolvedValueOnce({ data: singleOk({ id: '1', status: 'Cancelled' }) })
      await StockTransferApi.cancel('1')
      expect(apiClient.post).toHaveBeenNthCalledWith(4, '/inventory/stock-transfers/1/cancel')
    })
  })
})
