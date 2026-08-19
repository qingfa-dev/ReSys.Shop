import { describe, it, expect, vi, beforeEach } from 'vitest'
import { getPaged } from '@/shared/api/paged'
import * as client from '@/shared/api/client'

vi.mock('@/shared/api/client', () => ({
  get: vi.fn<(url: string, signal?: AbortSignal) => Promise<unknown>>(),
  HttpError: class HttpError extends Error {
    statusCode: number
    errors: Array<{ code: string; message: string; type: number }>
    constructor(statusCode: number, errors: Array<{ code: string; message: string; type: number }>) {
      super(errors[0]?.message ?? 'HTTP Error')
      this.statusCode = statusCode
      this.errors = errors
    }
  },
}))

describe('getPaged', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('returns paged result on success', async () => {
    const mockResult = {
      isSuccess: true,
      statusCode: 200,
      items: [{ id: '1' }],
      page: 1,
      pageSize: 20,
      totalCount: 1,
      totalPages: 1,
    }
    vi.mocked(client.get).mockResolvedValue(mockResult)

    const result = await getPaged('/api/storefront/products', {
      pageNumber: 1,
      pageSize: 20,
    })

    expect(result.isSuccess).toBe(true)
    expect(result.items).toEqual([{ id: '1' }])
  })

  it('returns paged failure on HttpError', async () => {
    const httpError = new client.HttpError(500, [{ code: 'Server.Error', message: 'Boom', type: 500 }])
    vi.mocked(client.get).mockRejectedValue(httpError)

    const result = await getPaged('/api/test', { pageNumber: 1, pageSize: 20 })

    expect(result.isSuccess).toBe(false)
    expect(result.errors[0]?.code).toBe('Server.Error')
  })
})
