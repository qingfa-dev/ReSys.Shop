import { beforeEach, describe, expect, it, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useCategoryStore } from '../category'
import type { Category } from '../../types'

vi.mock('../../services/category/category.service', () => ({
  categoryService: {
    getCategories: vi.fn(),
  },
}))

import { categoryService } from '../../services/category/category.service'

describe('useCategoryStore', () => {
  const mockCategory: Category = {
    id: 'cat-1',
    name: 'Test Category',
    slug: 'test-category',
    parentId: undefined,
    image: '/cat.jpg',
  }

  const mockCategory2: Category = {
    id: 'cat-2',
    name: 'Another Category',
    slug: 'another-category',
    parentId: 'cat-1',
    image: '/cat2.jpg',
  }

  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('state', () => {
    it('should initialize with default values', () => {
      const store = useCategoryStore()
      expect(store.categories).toEqual([])
      expect(store.currentCategory).toBeNull()
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
    })
  })

  describe('computed', () => {
    it('should compute categoryCount correctly', () => {
      const store = useCategoryStore()
      expect(store.categoryCount).toBe(0)
      store.categories = [mockCategory, mockCategory2]
      expect(store.categoryCount).toBe(2)
    })
  })

  describe('fetchCategories', () => {
    it('should fetch categories successfully', async () => {
      const mockResult = {
        isSuccess: true,
        isFailure: false,
        statusCode: 200,
        data: [mockCategory, mockCategory2],
      }
      vi.mocked(categoryService.getCategories).mockResolvedValue(mockResult)

      const store = useCategoryStore()
      await store.fetchCategories()

      expect(categoryService.getCategories).toHaveBeenCalled()
      expect(store.categories).toEqual([mockCategory, mockCategory2])
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
    })

    it('should handle fetch error', async () => {
      vi.mocked(categoryService.getCategories).mockRejectedValue(new Error('Network error'))

      const store = useCategoryStore()
      await store.fetchCategories()

      expect(store.error).toBe('Network error')
      expect(store.loading).toBe(false)
    })

    it('should handle service failure result', async () => {
      const mockResult = {
        isSuccess: false,
        isFailure: true,
        statusCode: 500,
        message: 'Service error',
        errors: [],
      }
      vi.mocked(categoryService.getCategories).mockResolvedValue(mockResult)

      const store = useCategoryStore()
      await store.fetchCategories()

      expect(store.error).toBe('Service error')
      expect(store.categories).toEqual([])
      expect(store.loading).toBe(false)
    })

    it('should clear error on successful fetch', async () => {
      const store = useCategoryStore()
      store.error = 'Previous error'

      const mockResult = {
        isSuccess: true,
        isFailure: false,
        statusCode: 200,
        data: [mockCategory],
      }
      vi.mocked(categoryService.getCategories).mockResolvedValue(mockResult)

      await store.fetchCategories()
      expect(store.error).toBeNull()
    })
  })

  describe('setCurrentCategory', () => {
    it('should set current category', () => {
      const store = useCategoryStore()
      store.setCurrentCategory(mockCategory)
      expect(store.currentCategory).toEqual(mockCategory)
    })

    it('should clear current category when null passed', () => {
      const store = useCategoryStore()
      store.setCurrentCategory(mockCategory)
      store.setCurrentCategory(null)
      expect(store.currentCategory).toBeNull()
    })
  })

  describe('getCategoryBySlug', () => {
    it('should find category by slug', () => {
      const store = useCategoryStore()
      store.categories = [mockCategory, mockCategory2]

      const result = store.getCategoryBySlug('test-category')
      expect(result).toEqual(mockCategory)
    })

    it('should return undefined for non-existent slug', () => {
      const store = useCategoryStore()
      store.categories = [mockCategory]

      const result = store.getCategoryBySlug('non-existent')
      expect(result).toBeUndefined()
    })
  })

  describe('getCategoryById', () => {
    it('should find category by id', () => {
      const store = useCategoryStore()
      store.categories = [mockCategory, mockCategory2]

      const result = store.getCategoryById('cat-1')
      expect(result).toEqual(mockCategory)
    })

    it('should return undefined for non-existent id', () => {
      const store = useCategoryStore()
      store.categories = [mockCategory]

      const result = store.getCategoryById('non-existent')
      expect(result).toBeUndefined()
    })
  })

  describe('loading state', () => {
    it('should be true while fetching', async () => {
      let resolveFn: (value: any) => void
      const promise = new Promise((resolve) => {
        resolveFn = resolve
      })
      vi.mocked(categoryService.getCategories).mockReturnValue(promise as any)

      const store = useCategoryStore()
      const fetchPromise = store.fetchCategories()

      expect(store.loading).toBe(true)

      resolveFn!({
        isSuccess: true,
        isFailure: false,
        statusCode: 200,
        data: [],
      })
      await fetchPromise

      expect(store.loading).toBe(false)
    })
  })
})
