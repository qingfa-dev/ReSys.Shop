import { describe, it, expect } from 'vitest'
import { stockStatusService } from '../stock-status.service'

describe('StockStatusService', () => {
  describe('getStockStatus', () => {
    it('should return stock status', async () => {
      const result = await stockStatusService.getStockStatus('prod-1')
      expect(result).toBeDefined()
    })
  })
})