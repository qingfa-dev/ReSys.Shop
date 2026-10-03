import { describe, it, expect } from 'vitest'
import { mockStockStatusRepository } from '../stock-status.mock.repository'

describe('StockStatusRepository', () => {
  describe('getByProductId', () => {
    it('should return in-stock status', async () => {
      const result = await mockStockStatusRepository.getByProductId('prod-1')
      expect(result.isSuccess).toBe(true)
      expect(result.data?.inStock).toBe(true)
      expect(result.data?.outOfStock).toBe(false)
      expect(result.data?.lowStock).toBe(false)
    })

    it('should return low-stock status', async () => {
      const result = await mockStockStatusRepository.getByProductId('prod-2')
      expect(result.isSuccess).toBe(true)
      expect(result.data?.lowStock).toBe(true)
    })

    it('should return out-of-stock status', async () => {
      const result = await mockStockStatusRepository.getByProductId('prod-3')
      expect(result.isSuccess).toBe(true)
      expect(result.data?.outOfStock).toBe(true)
    })

    it('should return error for non-existent product', async () => {
      const result = await mockStockStatusRepository.getByProductId('invalid-prod')
      expect(result.isFailure).toBe(true)
      expect(result.statusCode).toBe(404)
    })
  })
})