import { describe, it, expect } from 'vitest'
import {
  toInventoryItem,
  fromInventoryItem,
  toStockStatus,
  fromStockStatus,
  isLowStock,
  isOutOfStock,
  calculateAvailableQuantity,
} from '../mapping/inventory.mapping'
import { InventoryItemSchema, StockStatusSchema } from '../types/schemas'

describe('Inventory Mapping', () => {
  describe('toInventoryItem', () => {
    it('should convert schema to entity', () => {
      const schema = InventoryItemSchema.parse({
        id: 'inv-1',
        productId: 'prod-1',
        quantity: 100,
        reserved: 10,
        available: 90,
        warehouse: 'Main',
        lowStockThreshold: 20,
      })
      const result = toInventoryItem(schema)
      expect(result.id).toBe('inv-1')
      expect(result.productId).toBe('prod-1')
      expect(result.quantity).toBe(100)
    })
  })

  describe('fromInventoryItem', () => {
    it('should convert entity to schema', () => {
      const item = {
        id: 'inv-1',
        productId: 'prod-1',
        quantity: 100,
        reserved: 10,
        available: 90,
        warehouse: 'Main',
        lowStockThreshold: 20,
      }
      const result = fromInventoryItem(item)
      expect(result.id).toBe('inv-1')
    })
  })

  describe('toStockStatus', () => {
    it('should convert schema to entity', () => {
      const schema = StockStatusSchema.parse({
        inStock: true,
        lowStock: false,
        outOfStock: false,
        quantity: 100,
      })
      const result = toStockStatus(schema)
      expect(result.inStock).toBe(true)
      expect(result.quantity).toBe(100)
    })
  })

  describe('isLowStock', () => {
    it('should return true when available <= threshold', () => {
      const item = { available: 10, lowStockThreshold: 20, quantity: 50, reserved: 40, id: '', productId: '', warehouse: '' }
      expect(isLowStock(item)).toBe(true)
    })

    it('should return false when available > threshold', () => {
      const item = { available: 30, lowStockThreshold: 20, quantity: 50, reserved: 20, id: '', productId: '', warehouse: '' }
      expect(isLowStock(item)).toBe(false)
    })
  })

  describe('isOutOfStock', () => {
    it('should return true when quantity is 0', () => {
      const item = { quantity: 0, available: 0, reserved: 0, id: '', productId: '', warehouse: '', lowStockThreshold: 0 }
      expect(isOutOfStock(item)).toBe(true)
    })

    it('should return false when quantity > 0', () => {
      const item = { quantity: 100, available: 90, reserved: 10, id: '', productId: '', warehouse: '', lowStockThreshold: 20 }
      expect(isOutOfStock(item)).toBe(false)
    })
  })

  describe('calculateAvailableQuantity', () => {
    it('should calculate available = quantity - reserved', () => {
      const item = { quantity: 100, reserved: 30, available: 70, id: '', productId: '', warehouse: '', lowStockThreshold: 0 }
      expect(calculateAvailableQuantity(item)).toBe(70)
    })

    it('should return 0 when reserved > quantity', () => {
      const item = { quantity: 20, reserved: 50, available: -30, id: '', productId: '', warehouse: '', lowStockThreshold: 0 }
      expect(calculateAvailableQuantity(item)).toBe(0)
    })
  })
})