import { describe, it, expect } from 'vitest'
import { productService } from '../product.service'

import type { ProductFilter } from '../../../types'

describe('ProductService', () => {
  describe('getProducts', () => {
    it('should return paginated products', async () => {
      const result = await productService.getProducts(undefined, 1, 12)
      expect(result.isSuccess).toBe(true)
      expect(result.items).toBeDefined()
      expect(result.page).toBe(1)
    })

    it('should apply filters', async () => {
      const result = await productService.getProducts({ category: 'electronics' } as ProductFilter, 1, 10)
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('getProduct', () => {
    it('should return product by id', async () => {
      const result = await productService.getProduct('prod-1')
      expect(result.isSuccess).toBe(true)
      expect(result.data).toBeDefined()
    })

    it('should return error for non-existent product', async () => {
      const result = await productService.getProduct('non-existent')
      expect(result.isFailure).toBe(true)
      expect(result.statusCode).toBe(404)
    })
  })

  describe('getProductBySlug', () => {
    it('should return product by slug', async () => {
      const result = await productService.getProductBySlug('classic-cotton-tshirt')
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('searchProducts', () => {
    it('should search products by query', async () => {
      const result = await productService.searchProducts('headphones')
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('getFeaturedProducts', () => {
    it('should return featured products', async () => {
      const result = await productService.getFeaturedProducts(4)
      expect(result.isSuccess).toBe(true)
      expect(result.data?.length).toBeLessThanOrEqual(4)
    })
  })

  describe('getNewArrivals', () => {
    it('should return new arrival products', async () => {
      const result = await productService.getNewArrivals(8)
      expect(result.isSuccess).toBe(true)
    })
  })
})