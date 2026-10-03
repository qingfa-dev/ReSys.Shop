import { describe, it, expect } from 'vitest'
import { mapStockItem, mapStockItemDetail } from '../models/stock-item.mapper'

describe('mapStockItem', () => {
  it('maps and computes countAvailable', () => {
    const dto = { id: 's1', stockLocationId: 'loc1', variantId: 'v1', countOnHand: 50, backorderable: true, sku: 'TST', variantName: 'Test', stockLocationName: 'Main' }
    const result = mapStockItem(dto)
    expect(result.countOnHand).toBe(50)
    expect(result.countAvailable).toBe(50)
  })
})

describe('mapStockItemDetail', () => {
  it('includes timestamps', () => {
    const dto = { id: 's1', stockLocationId: 'loc1', variantId: 'v1', countOnHand: 10, backorderable: false, sku: null, variantName: null, stockLocationName: null, createdAtUtc: '2025-01-01T00:00:00Z', modifiedAtUtc: null }
    const result = mapStockItemDetail(dto)
    expect(result.createdAtUtc).toBe('2025-01-01T00:00:00Z')
    expect(result.countAvailable).toBe(10)
  })
})
