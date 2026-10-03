import { beforeEach, describe, expect, it, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useCatalogStore } from '../catalog'
import { useProductStore } from '../product'

vi.mock('../../services/product/product.service', () => ({
  productService: {
    getProducts: vi.fn(),
  },
}))

vi.mock('../../services/category/category.service', () => ({
  categoryService: {
    getCategories: vi.fn(),
  },
}))

import { productService } from '../../services/product/product.service'

describe('useCatalogStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('state', () => {
    it('should initialize with default filter', () => {
      const store = useCatalogStore()
      expect(store.filter).toEqual({})
    })
  })

  describe('computed proxies', () => {
    it('should proxy hasProducts from productStore', () => {
      const productStore = useProductStore()
      const catalogStore = useCatalogStore()
      
      expect(catalogStore.hasProducts).toBe(false)
      productStore.products = [{}] as any
      expect(catalogStore.hasProducts).toBe(true)
    })

    it('should proxy productCount from productStore', () => {
      const productStore = useProductStore()
      const catalogStore = useCatalogStore()
      
      expect(catalogStore.productCount).toBe(0)
      productStore.pagination.total = 100
      expect(catalogStore.productCount).toBe(100)
    })

    it('should proxy products from productStore', () => {
      const productStore = useProductStore()
      const catalogStore = useCatalogStore()
      
      expect(catalogStore.products).toEqual([])
      productStore.products = [{ id: '1' }] as any
      expect(catalogStore.products).length(1)
    })

    it('should proxy loading from productStore', () => {
      const productStore = useProductStore()
      const catalogStore = useCatalogStore()
      
      expect(catalogStore.loading).toBe(false)
      productStore.loading = true
      expect(catalogStore.loading).toBe(true)
    })

    it('should proxy error from productStore', () => {
      const productStore = useProductStore()
      const catalogStore = useCatalogStore()
      
      expect(catalogStore.error).toBeNull()
      productStore.error = 'Test error'
      expect(catalogStore.error).toBe('Test error')
    })

    it('should proxy pagination from productStore', () => {
      const productStore = useProductStore()
      const catalogStore = useCatalogStore()
      
      expect(catalogStore.pagination).toEqual({ page: 1, pageSize: 12, total: 0, totalPages: 0 })
      productStore.pagination = { page: 2, pageSize: 24, total: 48, totalPages: 2 }
      expect(catalogStore.pagination).toEqual({ page: 2, pageSize: 24, total: 48, totalPages: 2 })
    })
  })

  describe('setPage', () => {
    it('should call productStore.fetchProducts with page', async () => {
      const mockResult = {
        isSuccess: true,
        isFailure: false,
        statusCode: 200,
        items: [],
        page: 2,
        pageSize: 12,
        totalCount: 0,
        totalPages: 0,
        hasNextPage: false,
        hasPreviousPage: true,
      }
      vi.mocked(productService.getProducts).mockResolvedValue(mockResult)

      const catalogStore = useCatalogStore()
      await catalogStore.setPage(2)

      expect(productService.getProducts).toHaveBeenCalledWith({}, 2, 12)
    })
  })

  describe('setFilter', () => {
    it('should update filter and fetch products', async () => {
      const mockResult = {
        isSuccess: true,
        isFailure: false,
        statusCode: 200,
        items: [],
        page: 1,
        pageSize: 12,
        totalCount: 0,
        totalPages: 0,
        hasNextPage: false,
        hasPreviousPage: false,
      }
      vi.mocked(productService.getProducts).mockResolvedValue(mockResult)

      const catalogStore = useCatalogStore()
      await catalogStore.setFilter({ priceMin: 50 })

      expect(catalogStore.filter).toEqual({ priceMin: 50 })
    })
  })

  describe('clearFilter', () => {
    it('should clear filter and fetch products', async () => {
      const mockResult = {
        isSuccess: true,
        isFailure: false,
        statusCode: 200,
        items: [],
        page: 1,
        pageSize: 12,
        totalCount: 0,
        totalPages: 0,
        hasNextPage: false,
        hasPreviousPage: false,
      }
      vi.mocked(productService.getProducts).mockResolvedValue(mockResult)

      const catalogStore = useCatalogStore()
      catalogStore.filter = { priceMin: 50 }
      await catalogStore.clearFilter()

      expect(catalogStore.filter).toEqual({})
    })
  })
})
