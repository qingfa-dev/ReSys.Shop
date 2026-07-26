import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { StateApi } from '../state.api'

vi.mock('@/shared/api/client', () => ({
  default: {
    get: vi.fn<(...args: unknown[]) => unknown>(),
    post: vi.fn<(...args: unknown[]) => unknown>(),
    put: vi.fn<(...args: unknown[]) => unknown>(),
    delete: vi.fn<(...args: unknown[]) => unknown>(),
    patch: vi.fn<(...args: unknown[]) => unknown>(),
  },
}))

const pagedEmpty = { isSuccess: true, items: [], page: 1, pageSize: 20, totalCount: 0, statusCode: 200 }
const singleOk = (value: unknown) => ({ isSuccess: true, value, statusCode: 200 })
const defaultQuery = { page: 1, pageSize: 20, sort: [{ field: 'createdAt' as const, direction: 'Descending' as const }] }

describe('StateApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  describe('getMany', () => {
    it('calls GET /locations/states with serialized query params', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: pagedEmpty })
      await StateApi.getMany(defaultQuery)
      expect(apiClient.get).toHaveBeenCalledWith('/locations/states', {
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
    it('calls GET /locations/states/:id', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ id: '1', name: 'California' }) })
      await StateApi.get('1')
      expect(apiClient.get).toHaveBeenCalledWith('/locations/states/1')
    })
  })

  describe('getByIso', () => {
    it('calls GET /locations/states/by-iso/:isoCode', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ id: '1', name: 'California' }) })
      await StateApi.getByIso('US-CA')
      expect(apiClient.get).toHaveBeenCalledWith('/locations/states/by-iso/US-CA')
    })
  })

  describe('create', () => {
    it('calls POST /locations/states with body', async () => {
      const data = { name: 'Texas', isoCode: 'US-TX' }
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '2', ...data }) })
      await StateApi.create(data)
      expect(apiClient.post).toHaveBeenCalledWith('/locations/states', data)
    })
  })

  describe('update', () => {
    it('calls PUT /locations/states/:id with body', async () => {
      vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: '1', name: 'Updated' }) })
      await StateApi.update('1', { name: 'Updated' })
      expect(apiClient.put).toHaveBeenCalledWith('/locations/states/1', { name: 'Updated' })
    })
  })

  describe('delete', () => {
    it('calls DELETE /locations/states/:id', async () => {
      vi.mocked(apiClient.delete).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await StateApi.delete('1')
      expect(apiClient.delete).toHaveBeenCalledWith('/locations/states/1')
    })
  })
})
