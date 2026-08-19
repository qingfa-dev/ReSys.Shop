import { describe, it, expect } from 'vitest'
import { mockCategoryRepository } from '../category.mock.repository'

describe('CategoryRepository', () => {
  describe('getAll', () => {
    it('should return paginated categories', async () => {
      const result = await mockCategoryRepository.getAll({ paging: { page: 1, pageSize: 10 } })
      expect(result.isSuccess).toBe(true)
      expect(result.items).toBeDefined()
    })
  })

  describe('getById', () => {
    it('should return category by id', async () => {
      const result = await mockCategoryRepository.getById('cat-1')
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('getBySlug', () => {
    it('should return category by slug', async () => {
      const result = await mockCategoryRepository.getBySlug('clothing')
      expect(result.isSuccess).toBe(true)
    })
  })
})