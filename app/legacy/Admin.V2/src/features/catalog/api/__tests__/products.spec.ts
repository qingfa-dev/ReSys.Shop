import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { ProductApi } from '../product.api'

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
const defaultQuery = { page: 1, pageSize: 20, sort: [{ field: 'createdAt', direction: 'Descending' as const }] }

describe('ProductApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('getMany: GET /catalog/products with serialized query params', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: pagedEmpty })
    await ProductApi.getMany(defaultQuery)
    expect(apiClient.get).toHaveBeenCalledWith('/catalog/products', {
      params: {
        'page.page': 1,
        'page.pageSize': 20,
        'sort.clauses[0].field': 'createdAt',
        'sort.clauses[0].direction': 'Descending',
      },
    })
  })

  it('get: GET /catalog/products/:id', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ id: '1', name: 'Test' }) })
    await ProductApi.get('1')
    expect(apiClient.get).toHaveBeenCalledWith('/catalog/products/1')
  })

  it('create: POST /catalog/products', async () => {
    vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: 'new', name: 'New' }) })
    await ProductApi.create({ name: 'New', slug: 'new' })
    expect(apiClient.post).toHaveBeenCalledWith('/catalog/products', { name: 'New', slug: 'new' })
  })

  it('update: PUT /catalog/products/:id', async () => {
    vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: '1', name: 'Updated' }) })
    await ProductApi.update('1', { name: 'Updated', slug: 'updated' })
    expect(apiClient.put).toHaveBeenCalledWith('/catalog/products/1', { name: 'Updated', slug: 'updated' })
  })

  it('delete: DELETE /catalog/products/:id', async () => {
    vi.mocked(apiClient.delete).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
    await ProductApi.delete('1')
    expect(apiClient.delete).toHaveBeenCalledWith('/catalog/products/1')
  })
})
