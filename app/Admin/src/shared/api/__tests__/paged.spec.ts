import { describe, it, expect, vi, beforeEach } from 'vitest'
import { getPaged } from '../paged'
import type { PagedResult } from '@/shared/types/result'

const { mockGet, mockHttpError } = vi.hoisted(() => ({
  mockGet: vi.fn(),
  mockHttpError: class extends Error {
    statusCode: number
    errors: Array<{ code: string; message: string; type: number }>
    constructor(statusCode: number, errors: Array<{ code: string; message: string; type: number }>) {
      super(errors[0]?.message ?? '')
      this.name = 'HttpError'
      this.statusCode = statusCode
      this.errors = errors
    }
  },
}))

vi.mock('../client', () => ({
  get: mockGet,
  HttpError: mockHttpError,
}))

beforeEach(() => {
  vi.clearAllMocks()
})

function okResponse(overrides: Partial<PagedResult<unknown>> = {}): PagedResult<unknown> {
  return {
    isSuccess: true,
    statusCode: 200,
    items: [{ id: '1', name: 'Product A' }, { id: '2', name: 'Product B' }],
    page: 1,
    pageSize: 10,
    totalCount: 25,
    totalPages: 3,
    errors: [],
    message: null,
    metadata: null,
    ...overrides,
  }
}

describe('getPaged', () => {
  it('returns paged result on success', async () => {
    mockGet.mockResolvedValue(okResponse())

    const result = await getPaged<{ id: string; name: string }>('/api/products', {
      pageNumber: 1,
      pageSize: 10,
    })

    expect(result.isSuccess).toBe(true)
    expect(result.items).toHaveLength(2)
    expect(result.totalCount).toBe(25)
    expect(result.totalPages).toBe(3)
  })

  it('sends filter, sort, search, page params as query string', async () => {
    mockGet.mockImplementation((url: string) => {
      expect(url).toContain('filter=name%3Dbolt')
      expect(url).toContain('sort=-createdAt')
      expect(url).toContain('sort=name')
      expect(url).toContain('search=electronics')
      expect(url).toContain('page=2')
      expect(url).toContain('pageSize=50')
      return Promise.resolve(okResponse())
    })

    await getPaged('/api/products', {
      filter: 'name=bolt',
      sort: ['-createdAt', 'name'],
      search: 'electronics',
      pageNumber: 2,
      pageSize: 50,
    })
  })

  it('returns failure when parseAll fails', async () => {
    const result = await getPaged<unknown>('/api/products', {
      sort: ['-'],
    })

    expect(result.isSuccess).toBe(false)
    expect(result.errors.length).toBeGreaterThan(0)
    expect(mockGet).not.toHaveBeenCalled()
  })

  it('returns failure on HTTP error', async () => {
    mockGet.mockRejectedValue(new mockHttpError(404, [{ code: 'NotFound', message: 'Not found', type: 404 }]))

    const result = await getPaged<unknown>('/api/products', { pageNumber: 1, pageSize: 20 })

    expect(result.isSuccess).toBe(false)
    expect(result.errors[0]?.code).toBe('NotFound')
  })

  it('returns failure on network error', async () => {
    mockGet.mockRejectedValue(new Error('Failed to fetch'))

    const result = await getPaged<unknown>('/api/products', { pageNumber: 1, pageSize: 20 })

    expect(result.isSuccess).toBe(false)
    expect(result.errors[0]?.code).toBe('NetworkError')
  })

  it('passes signal to get', async () => {
    mockGet.mockResolvedValue(okResponse())
    const controller = new AbortController()

    await getPaged<unknown>('/api/products', { pageNumber: 1, pageSize: 20 }, { signal: controller.signal })

    expect(mockGet).toHaveBeenCalledWith(
      '/api/products?page=1&pageSize=20',
      controller.signal,
    )
  })

  it('applies allowed fields filter', async () => {
    const result = await getPaged<unknown>('/api/products', {
      filter: 'secret=value',
    }, {
      allowedFilterFields: ['name'],
    })

    expect(result.isSuccess).toBe(false)
    expect(result.errors[0]?.code).toBe('Filter.Field.Disallowed')
    expect(mockGet).not.toHaveBeenCalled()
  })

  it('enforces allowed search fields whitelist', async () => {
    const result = await getPaged<unknown>('/api/products', {
      search: 'test',
      searchFields: ['secret'],
    }, {
      allowedSearchFields: ['name'],
    })

    expect(result.isSuccess).toBe(false)
    expect(result.errors[0]?.code).toBe('Search.Parsing.InvalidJson')
    expect(mockGet).not.toHaveBeenCalled()
  })
})
