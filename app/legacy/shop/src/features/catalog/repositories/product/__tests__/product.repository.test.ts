import { describe, it, expect } from 'vitest'
import { mockProductRepository } from '../product.mock.repository'

describe('ProductRepository', () => {
  describe('getAll', () => {
    it('should return paginated products', async () => {
      const result = await mockProductRepository.getAll({ paging: { page: 1, pageSize: 10 } })
      expect(result.isSuccess).toBe(true)
      expect(result.items).toBeDefined()
      expect(result.page).toBe(1)
    })

    it('should apply filters', async () => {
      const result = await mockProductRepository.getAll({
        filter: { filter: JSON.stringify({ category: 'electronics' }) }
      })
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('getById', () => {
    it('should return product by id', async () => {
      const result = await mockProductRepository.getById('prod-1')
      expect(result.isSuccess).toBe(true)
      expect(result.data).toBeDefined()
    })

    it('should return error for non-existent product', async () => {
      const result = await mockProductRepository.getById('non-existent')
      expect(result.isFailure).toBe(true)
      expect(result.statusCode).toBe(404)
    })
  })

  describe('getProductBySlug', () => {
    it('should return product by slug', async () => {
      const result = await mockProductRepository.getProductBySlug('classic-cotton-tshirt')
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('searchProducts', () => {
    it('should search products by query', async () => {
      const result = await mockProductRepository.searchProducts('headphones')
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('getFeaturedProducts', () => {
    it('should return featured products', async () => {
      const result = await mockProductRepository.getFeaturedProducts(4)
      expect(result.isSuccess).toBe(true)
      expect(result.items.length).toBeLessThanOrEqual(4)
    })
  })

  describe('getNewArrivals', () => {
    it('should return new arrival products', async () => {
      const result = await mockProductRepository.getNewArrivals(8)
      expect(result.isSuccess).toBe(true)
    })
  })
})