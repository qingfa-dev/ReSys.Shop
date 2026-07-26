import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { ProfileApi } from '../profile.api'

vi.mock('@/shared/api/client', () => ({
  default: {
    get: vi.fn<(...args: unknown[]) => unknown>(),
    post: vi.fn<(...args: unknown[]) => unknown>(),
    put: vi.fn<(...args: unknown[]) => unknown>(),
    delete: vi.fn<(...args: unknown[]) => unknown>(),
    patch: vi.fn<(...args: unknown[]) => unknown>(),
  },
}))

const singleOk = (value: unknown) => ({ isSuccess: true, value, statusCode: 200 })

describe('ProfileApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  describe('get', () => {
    it('calls GET /profiles/profiles', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ id: '1', firstName: 'John', lastName: 'Doe' }) })
      await ProfileApi.get()
      expect(apiClient.get).toHaveBeenCalledWith('/profiles/profiles')
    })
  })

  describe('create', () => {
    it('calls POST /profiles/profiles with body', async () => {
      const data = { firstName: 'Jane', lastName: 'Doe' }
      vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: '2', ...data }) })
      await ProfileApi.create(data)
      expect(apiClient.post).toHaveBeenCalledWith('/profiles/profiles', data)
    })
  })

  describe('update', () => {
    it('calls PUT /profiles/profiles with body', async () => {
      const data = { firstName: 'Updated' }
      vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: '1', firstName: 'Updated' }) })
      await ProfileApi.update(data)
      expect(apiClient.put).toHaveBeenCalledWith('/profiles/profiles', data)
    })
  })

  describe('delete', () => {
    it('calls DELETE /profiles/profiles', async () => {
      vi.mocked(apiClient.delete).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
      await ProfileApi.delete()
      expect(apiClient.delete).toHaveBeenCalledWith('/profiles/profiles')
    })
  })
})
