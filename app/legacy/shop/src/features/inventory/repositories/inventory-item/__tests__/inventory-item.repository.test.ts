import { describe, it, expect, beforeEach } from 'vitest'
import { mockInventoryItemRepository, MockInventoryItemRepository } from '../inventory-item.mock.repository'

describe('InventoryItemRepository', () => {
  beforeEach(() => {
    MockInventoryItemRepository.reset()
  })

  describe('getAll', () => {
    it('should return low stock items with default threshold', async () => {
      const result = await mockInventoryItemRepository.getAll()
      expect(result.isSuccess).toBe(true)
      expect(result.data?.length).toBeGreaterThan(0)
    })

    it('should accept custom threshold', async () => {
      const result = await mockInventoryItemRepository.getAll(5)
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('getById', () => {
    it('should return inventory item by id', async () => {
      const result = await mockInventoryItemRepository.getById('inv-1')
      expect(result.isSuccess).toBe(true)
      expect(result.data?.id).toBe('inv-1')
    })

    it('should return error for non-existent id', async () => {
      const result = await mockInventoryItemRepository.getById('invalid-id')
      expect(result.isFailure).toBe(true)
      expect(result.statusCode).toBe(404)
    })

    it('should support generic type', async () => {
      const result = await mockInventoryItemRepository.getById('inv-1')
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('getStockStatus', () => {
    it('should return stock status for product', async () => {
      const result = await mockInventoryItemRepository.getStockStatus('prod-1')
      expect(result.isSuccess).toBe(true)
      expect(result.data?.inStock).toBe(true)
      expect(result.data?.outOfStock).toBe(false)
    })

    it('should return low stock status', async () => {
      const result = await mockInventoryItemRepository.getStockStatus('prod-3')
      expect(result.isSuccess).toBe(true)
      expect(result.data?.lowStock).toBe(true)
    })

    it('should return error for non-existent product', async () => {
      const result = await mockInventoryItemRepository.getStockStatus('invalid-prod')
      expect(result.isFailure).toBe(true)
    })
  })

  describe('updateQuantity', () => {
    it('should update inventory quantity', async () => {
      const result = await mockInventoryItemRepository.updateQuantity('prod-1', 150)
      expect(result.isSuccess).toBe(true)
      expect(result.data?.quantity).toBe(150)
    })

    it('should return error for non-existent product', async () => {
      const result = await mockInventoryItemRepository.updateQuantity('invalid-prod', 100)
      expect(result.isFailure).toBe(true)
    })
  })

  describe('reserveStock', () => {
    it('should reserve stock', async () => {
      const result = await mockInventoryItemRepository.reserveStock('prod-1', 10)
      expect(result.isSuccess).toBe(true)
      expect(result.data?.reserved).toBe(20)
    })

    it('should fail when insufficient stock', async () => {
      const result = await mockInventoryItemRepository.reserveStock('prod-1', 1000)
      expect(result.isFailure).toBe(true)
    })
  })

  describe('releaseStock', () => {
    it('should release reserved stock', async () => {
      await mockInventoryItemRepository.reserveStock('prod-1', 10)
      const result = await mockInventoryItemRepository.releaseStock('prod-1', 5)
      expect(result.isSuccess).toBe(true)
      expect(result.data?.reserved).toBe(15)
    })
  })
})