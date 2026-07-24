import { beforeEach, describe, expect, it, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useProductStore } from '../product'
import type { Product, ProductFilter } from '../../types'

vi.mock('../../services/product/product.service', () => ({
  productService: {
    getProducts: vi.fn(),
    getProduct: vi.fn(),
    getProductBySlug: vi.fn(),
    searchProducts: vi.fn(),
    getFeaturedProducts: vi.fn(),
    getNewArrivals: vi.fn(),
  },
}))

import { productService } from '../../services/product/product.service'

describe('useProductStore', () => {
  const mockProduct: Product = {
    id: 'prod-1',
    name: 'Test Product',
    slug: 'test-product',
    description: 'A test product',
    price: 99.99,
    compareAtPrice: 129.99,
    images: ['/image1.jpg'],
    category: { id: 'cat-1', name: 'Test Category', slug: 'test-category', parentId: undefined, image: '/cat.jpg' },
    tags: ['test'],
    variants: [],
    inventory: { quantity: 10, trackQuantity: true, allowBackorder: false, lowStockThreshold: 5 },
    createdAt: '2024-01-01',
    updatedAt: '2024-01-01',
  }

  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('state', () => {
    it('should initialize with default values', () => {
      const store = useProductStore()
      expect(store.products).toEqual([])
      expect(store.currentProduct).toBeNull()
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
      expect(store.pagination).toEqual({ page: 1, pageSize: 12, total: 0, totalPages: 0 })
      expect(store.filter).toEqual({})
    })
  })

  describe('computed', () => {
    it('should compute hasProducts correctly', () => {
      const store = useProductStore()
      expect(store.hasProducts).toBe(false)
      store.products = [mockProduct]
      expect(store.hasProducts).toBe(true)
    })

    it('should compute productCount correctly', () => {
      const store = useProductStore()
      expect(store.productCount).toBe(0)
      store.pagination.total = 100
      expect(store.productCount).toBe(100)
    })
  })

  describe('fetchProducts', () => {
    it('should fetch products successfully', async () => {
      const mockResult = {
        isSuccess: true,
        isFailure: false,
        statusCode: 200,
        items: [mockProduct],
        page: 1,
        pageSize: 12,
        totalCount: 1,
        totalPages: 1,
        hasNextPage: false,
        hasPreviousPage: false,
      }
      vi.mocked(productService.getProducts).mockResolvedValue(mockResult)

      const store = useProductStore()
      await store.fetchProducts()

      expect(productService.getProducts).toHaveBeenCalledWith({}, 1, 12)
      expect(store.products).toEqual([mockProduct])
      expect(store.pagination).toEqual({ page: 1, pageSize: 12, total: 1, totalPages: 1 })
      expect(store.loading).toBe(false)
    })

    it('should handle fetch error', async () => {
      vi.mocked(productService.getProducts).mockRejectedValue(new Error('Network error'))

      const store = useProductStore()
      await store.fetchProducts()

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
        items: [],
        page: 1,
        pageSize: 12,
        totalCount: 0,
        totalPages: 0,
        hasNextPage: false,
        hasPreviousPage: false,
      }
      vi.mocked(productService.getProducts).mockResolvedValue(mockResult)

      const store = useProductStore()
      await store.fetchProducts()

      expect(store.error).toBe('Service error')
      expect(store.loading).toBe(false)
    })

    it('should merge filters when fetching', async () => {
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

      const store = useProductStore()
      store.filter = { priceMin: 50 }
      await store.fetchProducts({ priceMax: 100 }, 2)

      expect(productService.getProducts).toHaveBeenCalledWith({ priceMin: 50, priceMax: 100 }, 2, 12)
    })
  })

  describe('fetchProduct', () => {
    it('should fetch single product successfully', async () => {
      const mockResult = {
        isSuccess: true,
        isFailure: false,
        statusCode: 200,
        data: mockProduct,
      }
      vi.mocked(productService.getProduct).mockResolvedValue(mockResult)

      const store = useProductStore()
      await store.fetchProduct('prod-1')

      expect(productService.getProduct).toHaveBeenCalledWith('prod-1')
      expect(store.currentProduct).toEqual(mockProduct)
      expect(store.loading).toBe(false)
    })

    it('should handle fetch error', async () => {
      const mockResult = {
        isSuccess: false,
        isFailure: true,
        statusCode: 404,
        message: 'Product not found',
        errors: [],
      }
      vi.mocked(productService.getProduct).mockResolvedValue(mockResult)

      const store = useProductStore()
      await store.fetchProduct('prod-1')

      expect(store.error).toBe('Product not found')
      expect(store.loading).toBe(false)
    })

    it('should clear currentProduct when no data', async () => {
      const mockResult = {
        isSuccess: true,
        isFailure: false,
        statusCode: 200,
        data: undefined,
      }
      vi.mocked(productService.getProduct).mockResolvedValue(mockResult)

      const store = useProductStore()
      await store.fetchProduct('prod-1')

      expect(store.currentProduct).toBeNull()
    })
  })

  describe('getFeaturedProducts', () => {
    it('should fetch featured products successfully', async () => {
      const mockResult = {
        isSuccess: true,
        isFailure: false,
        statusCode: 200,
        data: [mockProduct],
      }
      vi.mocked(productService.getFeaturedProducts).mockResolvedValue(mockResult)

      const store = useProductStore()
      await store.getFeaturedProducts(4)

      expect(productService.getFeaturedProducts).toHaveBeenCalledWith(4)
      expect(store.products).toEqual([mockProduct])
      expect(store.loading).toBe(false)
    })

    it('should handle featured products error', async () => {
      const mockResult = {
        isSuccess: false,
        isFailure: true,
        statusCode: 500,
        message: 'Failed to fetch featured',
        errors: [],
      }
      vi.mocked(productService.getFeaturedProducts).mockResolvedValue(mockResult)

      const store = useProductStore()
      await store.getFeaturedProducts()

      expect(store.error).toBe('Failed to fetch featured')
      expect(store.loading).toBe(false)
    })
  })

  describe('getNewArrivals', () => {
    it('should fetch new arrivals successfully', async () => {
      const mockResult = {
        isSuccess: true,
        isFailure: false,
        statusCode: 200,
        data: [mockProduct],
      }
      vi.mocked(productService.getNewArrivals).mockResolvedValue(mockResult)

      const store = useProductStore()
      await store.getNewArrivals(8)

      expect(productService.getNewArrivals).toHaveBeenCalledWith(8)
      expect(store.products).toEqual([mockProduct])
      expect(store.loading).toBe(false)
    })

    it('should handle new arrivals error', async () => {
      const mockResult = {
        isSuccess: false,
        isFailure: true,
        statusCode: 500,
        message: 'Failed to fetch arrivals',
        errors: [],
      }
      vi.mocked(productService.getNewArrivals).mockResolvedValue(mockResult)

      const store = useProductStore()
      await store.getNewArrivals()

      expect(store.error).toBe('Failed to fetch arrivals')
      expect(store.loading).toBe(false)
    })
  })

  describe('clearCurrentProduct', () => {
    it('should clear current product', async () => {
      const mockResult = {
        isSuccess: true,
        isFailure: false,
        statusCode: 200,
        data: mockProduct,
      }
      vi.mocked(productService.getProduct).mockResolvedValue(mockResult)

      const store = useProductStore()
      await store.fetchProduct('prod-1')
      expect(store.currentProduct).not.toBeNull()

      store.clearCurrentProduct()
      expect(store.currentProduct).toBeNull()
    })
  })

  describe('pagination methods', () => {
    it('should set page and fetch products', async () => {
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

      const store = useProductStore()
      await store.setPage(2)

      expect(productService.getProducts).toHaveBeenCalledWith({}, 2, 12)
    })

    it('should set filter and fetch products', async () => {
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

      const store = useProductStore()
      await store.setFilter({ priceMin: 50 })

      expect(store.filter).toEqual({ priceMin: 50 })
    })

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

      const store = useProductStore()
      store.filter = { priceMin: 50 }
      await store.clearFilter()

      expect(store.filter).toEqual({})
    })
  })
})
