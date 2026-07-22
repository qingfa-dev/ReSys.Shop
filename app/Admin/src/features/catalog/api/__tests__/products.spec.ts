import { describe, it, expect, vi, beforeEach } from 'vitest'
import { getProducts, getProduct, createProduct, updateProduct, deleteProduct } from '../products'
import type { FailureResult } from '@/shared/api/utils/result.mapper'

const mockGet = vi.fn()
const mockPost = vi.fn()
const mockPut = vi.fn()
const mockDelete = vi.fn()

vi.mock('@/shared/api/client', () => ({
  default: {
    get: (...args: unknown[]) => mockGet(...args),
    post: (...args: unknown[]) => mockPost(...args),
    put: (...args: unknown[]) => mockPut(...args),
    delete: (...args: unknown[]) => mockDelete(...args),
  },
}))

describe('products API', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('getProducts', () => {
    it('calls GET /catalog/products with pagination params', async () => {
      mockGet.mockResolvedValue({
        data: { isSuccess: true, items: [], page: 1, pageSize: 20, totalCount: 0, statusCode: 200 },
      })
      await getProducts({ page: 1, pageSize: 20, search: 'test' })
      expect(mockGet).toHaveBeenCalledWith('/catalog/products', {
        params: { page: 1, pageSize: 20, search: 'test' },
      })
    })

    it('maps paged success response', async () => {
      mockGet.mockResolvedValue({
        data: {
          isSuccess: true,
          value: null,
          items: [{ id: '1', name: 'Test', slug: 'test', status: 'Draft' }],
          page: 1,
          pageSize: 20,
          totalCount: 1,
          statusCode: 200,
        },
      })
      const result = await getProducts()
      expect(result.success).toBe(true)
      expect(result.data).toHaveLength(1)
      expect(result.meta?.totalCount).toBe(1)
    })

    it('maps failure response', async () => {
      mockGet.mockResolvedValue({
        data: { isSuccess: false, value: null, items: [], message: 'fail', errors: [{ code: 'ERR', message: 'fail' }], statusCode: 400 },
      })
      const result = await getProducts()
      expect(result.success).toBe(false)
      expect((result as FailureResult).error.message).toBeDefined()
    })
  })

  describe('getProduct', () => {
    it('calls GET /catalog/products/:id', async () => {
      mockGet.mockResolvedValue({
        data: { isSuccess: true, value: { id: 'abc', name: 'Test', slug: 'test', status: 'Draft' }, statusCode: 200 },
      })
      await getProduct('abc')
      expect(mockGet).toHaveBeenCalledWith('/catalog/products/abc')
    })
  })

  describe('createProduct', () => {
    it('calls POST /catalog/products with body', async () => {
      mockPost.mockResolvedValue({
        data: { isSuccess: true, value: { id: 'new', name: 'New', slug: 'new', status: 'Draft' }, statusCode: 201 },
      })
      await createProduct({ name: 'New', slug: 'new' })
      expect(mockPost).toHaveBeenCalledWith('/catalog/products', { name: 'New', slug: 'new' })
    })
  })

  describe('updateProduct', () => {
    it('calls PUT /catalog/products/:id with body', async () => {
      mockPut.mockResolvedValue({
        data: { isSuccess: true, value: { id: 'abc', name: 'Updated', slug: 'updated', status: 'Active' }, statusCode: 200 },
      })
      await updateProduct('abc', { name: 'Updated', slug: 'updated' })
      expect(mockPut).toHaveBeenCalledWith('/catalog/products/abc', { name: 'Updated', slug: 'updated' })
    })
  })

  describe('deleteProduct', () => {
    it('calls DELETE /catalog/products/:id', async () => {
      mockDelete.mockResolvedValue({
        data: { isSuccess: true, value: null, statusCode: 200 },
      })
      await deleteProduct('abc')
      expect(mockDelete).toHaveBeenCalledWith('/catalog/products/abc')
    })
  })
})
