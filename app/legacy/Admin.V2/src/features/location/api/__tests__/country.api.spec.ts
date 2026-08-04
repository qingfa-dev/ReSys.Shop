import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { CountryApi } from '../country.api'

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

describe('CountryApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  describe('getMany', () => {
    it('calls GET /locations/countries with serialized query params', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: pagedEmpty })
      await CountryApi.getMany(defaultQuery)
      expect(apiClient.get).toHaveBeenCalledWith('/locations/countries', {
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
    it('calls GET /locations/countries/:id', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ id: '1', name: 'US', isoCode: 'US' }) })
      await CountryApi.get('1')
      expect(apiClient.get).toHaveBeenCalledWith('/locations/countries/1')
    })
  })

  describe('getByIso', () => {
    it('calls GET /locations/countries/by-iso/:isoCode', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ id: '1', name: 'US', isoCode: 'US' }) })
      await CountryApi.getByIso('US')
      expect(apiClient.get).toHaveBeenCalledWith('/locations/countries/by-iso/US')
    })
  })

  describe('create', () => {
    it('calls POST /locations/countries with body', async () => {
      const data = { name: 'Canada', isoCode: 'CA' }
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '2', ...data }) })
      await CountryApi.create(data)
      expect(apiClient.post).toHaveBeenCalledWith('/locations/countries', data)
    })
  })

  describe('update', () => {
    it('calls PUT /locations/countries/:id with body', async () => {
      vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: '1', name: 'Updated' }) })
      await CountryApi.update('1', { name: 'Updated', isoCode: 'US' })
      expect(apiClient.put).toHaveBeenCalledWith('/locations/countries/1', { name: 'Updated', isoCode: 'US' })
    })
  })

  describe('delete', () => {
    it('calls DELETE /locations/countries/:id', async () => {
      vi.mocked(apiClient.delete).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await CountryApi.delete('1')
      expect(apiClient.delete).toHaveBeenCalledWith('/locations/countries/1')
    })
  })
})
