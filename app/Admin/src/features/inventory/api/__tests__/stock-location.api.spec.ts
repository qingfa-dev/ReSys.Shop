import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { StockLocationApi } from '../stock-location.api'

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

describe('StockLocationApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  describe('getMany', () => {
    it('calls GET /inventory/stock-locations with serialized query params', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: pagedEmpty })
      await StockLocationApi.getMany(defaultQuery)
      expect(apiClient.get).toHaveBeenCalledWith('/inventory/stock-locations', {
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
    it('calls GET /inventory/stock-locations/:id', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ id: '1', name: 'Warehouse A' }) })
      await StockLocationApi.get('1')
      expect(apiClient.get).toHaveBeenCalledWith('/inventory/stock-locations/1')
    })
  })

  describe('create', () => {
    it('calls POST /inventory/stock-locations with body', async () => {
      const data = { name: 'Warehouse B', code: 'WH-B' }
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '1', ...data }) })
      await StockLocationApi.create(data)
      expect(apiClient.post).toHaveBeenCalledWith('/inventory/stock-locations', data)
    })
  })

  describe('update', () => {
    it('calls PUT /inventory/stock-locations/:id with body', async () => {
      vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: '1', name: 'Updated' }) })
      await StockLocationApi.update('1', { name: 'Updated' })
      expect(apiClient.put).toHaveBeenCalledWith('/inventory/stock-locations/1', { name: 'Updated' })
    })
  })

  describe('delete', () => {
    it('calls DELETE /inventory/stock-locations/:id', async () => {
      vi.mocked(apiClient.delete).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await StockLocationApi.delete('1')
      expect(apiClient.delete).toHaveBeenCalledWith('/inventory/stock-locations/1')
    })
  })

  describe('setDefault', () => {
    it('calls PUT /inventory/stock-locations/:id/default', async () => {
      vi.mocked(apiClient.put).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await StockLocationApi.setDefault('1')
      expect(apiClient.put).toHaveBeenCalledWith('/inventory/stock-locations/1/default')
    })
  })
})
