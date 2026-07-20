import { describe, it, expect } from 'vitest'
import type {
  PagingParams,
  PageMeta,
} from '../models/paging.model'
import {
  createPagingParams,
  createPaginationParams,
  calculatePageMeta,
} from '../models/paging.model'

describe('Paging Model', () => {
  describe('PagingParams', () => {
    it('should allow optional page and pageSize', () => {
      const params: PagingParams = {}
      expect(params.page).toBeUndefined()
      expect(params.pageSize).toBeUndefined()
    })

    it('should allow setting page and pageSize', () => {
      const params: PagingParams = { page: 2, pageSize: 25 }
      expect(params.page).toBe(2)
      expect(params.pageSize).toBe(25)
    })
  })

  describe('PageMeta', () => {
    it('should have correct structure', () => {
      const meta: PageMeta = {
        page: 1,
        pageSize: 10,
        totalCount: 100,
        totalPages: 10,
        hasNextPage: true,
        hasPreviousPage: false,
      }
      expect(meta.page).toBe(1)
      expect(meta.totalCount).toBe(100)
      expect(meta.hasNextPage).toBe(true)
    })
  })

  describe('createPagingParams', () => {
    it('should use defaults when called with no args', () => {
      const params = createPagingParams()
      expect(params.page).toBe(1)
      expect(params.pageSize).toBe(10)
    })

    it('should use provided values', () => {
      const params = createPagingParams(3, 50)
      expect(params.page).toBe(3)
      expect(params.pageSize).toBe(50)
    })
  })

  describe('createPaginationParams', () => {
    it('should be an alias for createPagingParams', () => {
      const params = createPaginationParams(2, 20)
      expect(params.page).toBe(2)
      expect(params.pageSize).toBe(20)
    })
  })

  describe('calculatePageMeta', () => {
    it('should calculate total pages correctly', () => {
      const meta = calculatePageMeta(100, 1, 10)
      expect(meta.totalPages).toBe(10)
      expect(meta.hasNextPage).toBe(true)
      expect(meta.hasPreviousPage).toBe(false)
    })

    it('should handle last page', () => {
      const meta = calculatePageMeta(100, 10, 10)
      expect(meta.totalPages).toBe(10)
      expect(meta.hasNextPage).toBe(false)
      expect(meta.hasPreviousPage).toBe(true)
    })

    it('should handle middle page', () => {
      const meta = calculatePageMeta(100, 5, 10)
      expect(meta.hasNextPage).toBe(true)
      expect(meta.hasPreviousPage).toBe(true)
    })

    it('should handle zero pageSize', () => {
      const meta = calculatePageMeta(100, 1, 0)
      expect(meta.totalPages).toBe(0)
      expect(meta.hasNextPage).toBe(false)
    })

    it('should handle empty results', () => {
      const meta = calculatePageMeta(0, 1, 10)
      expect(meta.totalPages).toBe(0)
      expect(meta.hasNextPage).toBe(false)
      expect(meta.hasPreviousPage).toBe(false)
    })

    it('should handle non-even division', () => {
      const meta = calculatePageMeta(25, 1, 10)
      expect(meta.totalPages).toBe(3)
      expect(meta.hasNextPage).toBe(true)
    })
  })
})