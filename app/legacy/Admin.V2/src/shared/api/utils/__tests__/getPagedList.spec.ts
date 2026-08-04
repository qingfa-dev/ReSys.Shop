import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { getPagedList } from '../query-serializer'
import type { ListQuery } from '@/shared/models'

vi.mock('@/shared/api/client', () => ({
  default: {
    get: vi.fn<(...args: unknown[]) => unknown>(),
  },
}))

interface FakeResponse { name: string }
const pagedResult = { isSuccess: true, items: [], page: 1, pageSize: 20, totalCount: 0, statusCode: 200 }

describe('getPagedList', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('calls apiClient.get with the URL and serialized params', async () => {
    const mockGet = vi.mocked(apiClient.get)
    mockGet.mockResolvedValue({ data: pagedResult })

    await getPagedList<FakeResponse>('/test', { page: 1, pageSize: 20 })

    expect(mockGet).toHaveBeenCalledWith('/test', {
      params: { 'page.page': 1, 'page.pageSize': 20 },
    })
  })

  it('returns res.data', async () => {
    const mockGet = vi.mocked(apiClient.get)
    mockGet.mockResolvedValue({ data: pagedResult })

    const result = await getPagedList<FakeResponse>('/test', { page: 1, pageSize: 20 })

    expect(result).toEqual(pagedResult)
  })

  it('serializes page.page and page.pageSize from query', async () => {
    const mockGet = vi.mocked(apiClient.get)
    mockGet.mockResolvedValue({ data: pagedResult })

    await getPagedList<FakeResponse>('/test', { page: 2, pageSize: 50 })

    expect(mockGet).toHaveBeenCalledWith('/test', {
      params: { 'page.page': 2, 'page.pageSize': 50 },
    })
  })
})
