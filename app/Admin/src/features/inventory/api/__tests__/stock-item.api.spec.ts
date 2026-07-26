import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { StockItemApi } from '../stock-item.api'

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

describe('StockItemApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  describe('getMany', () => {
    it('calls GET /inventory/stock-items with serialized query params', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: pagedEmpty })
      await StockItemApi.getMany(defaultQuery)
      expect(apiClient.get).toHaveBeenCalledWith('/inventory/stock-items', {
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
      await StockItemApi.getMany({ ...defaultQuery, search: { value: 'SKU-1' } })
      expect(apiClient.get).toHaveBeenCalledWith('/inventory/stock-items', {
        params: expect.objectContaining({ 'search.term.value': 'SKU-1' }),
      })
    })
  })

  describe('get', () => {
    it('calls GET /inventory/stock-items/:id', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ id: '1', sku: 'SKU-1' }) })
      await StockItemApi.get('1')
      expect(apiClient.get).toHaveBeenCalledWith('/inventory/stock-items/1')
    })
  })

  describe('getLowStock', () => {
    it('calls GET /inventory/stock-items/low-stock', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk([]) })
      await StockItemApi.getLowStock()
      expect(apiClient.get).toHaveBeenCalledWith('/inventory/stock-items/low-stock')
    })
  })

  describe('getSummary', () => {
    it('calls GET /inventory/stock-items/summary', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ totalStock: 100, totalValue: 5000 }) })
      await StockItemApi.getSummary()
      expect(apiClient.get).toHaveBeenCalledWith('/inventory/stock-items/summary')
    })
  })

  describe('create', () => {
    it('calls POST /inventory/stock-items with body', async () => {
      const data = { sku: 'SKU-1', quantity: 10 }
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '1', ...data }) })
      await StockItemApi.create(data)
      expect(apiClient.post).toHaveBeenCalledWith('/inventory/stock-items', data)
    })
  })

  describe('bulkAdjust', () => {
    it('calls POST /inventory/stock-items/bulk-adjust with body', async () => {
      const data = { adjustments: [{ stockItemId: '1', quantity: 5 }] }
      vi.mocked(apiClient.post).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await StockItemApi.bulkAdjust(data)
      expect(apiClient.post).toHaveBeenCalledWith('/inventory/stock-items/bulk-adjust', data)
    })
  })

  describe('importFile', () => {
    it('calls POST /inventory/stock-items/import with multipart header', async () => {
      const formData = new FormData()
      vi.mocked(apiClient.post).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await StockItemApi.importFile(formData)
      expect(apiClient.post).toHaveBeenCalledWith('/inventory/stock-items/import', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
    })
  })

  describe('restock', () => {
    it('calls POST /inventory/stock-items/:id/restock with body', async () => {
      const data = { quantity: 20 }
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '1', quantity: 30 }) })
      await StockItemApi.restock('1', data)
      expect(apiClient.post).toHaveBeenCalledWith('/inventory/stock-items/1/restock', data)
    })
  })

  describe('update', () => {
    it('calls PUT /inventory/stock-items/:id with body', async () => {
      vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: '1', quantity: 20 }) })
      await StockItemApi.update('1', { quantity: 20 })
      expect(apiClient.put).toHaveBeenCalledWith('/inventory/stock-items/1', { quantity: 20 })
    })
  })

  describe('delete', () => {
    it('calls DELETE /inventory/stock-items/:id', async () => {
      vi.mocked(apiClient.delete).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await StockItemApi.delete('1')
      expect(apiClient.delete).toHaveBeenCalledWith('/inventory/stock-items/1')
    })
  })
})
