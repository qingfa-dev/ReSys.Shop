import { describe, it, expect } from 'vitest'
import { categoryService } from '../category.service'

describe('CategoryService', () => {
  describe('getCategories', () => {
    it('should return all categories', async () => {
      const result = await categoryService.getCategories()
      expect(result.isSuccess).toBe(true)
      expect(result.data).toBeDefined()
      expect(Array.isArray(result.data)).toBe(true)
    })

    it('should map category fields correctly', async () => {
      const result = await categoryService.getCategories()
      if (result.isSuccess && result.data && result.data.length > 0) {
        const category = result.data[0]
        expect(category).toHaveProperty('id')
        expect(category).toHaveProperty('name')
        expect(category).toHaveProperty('slug')
        expect(category).toHaveProperty('parentId')
        expect(category).toHaveProperty('image')
      }
    })
  })
})