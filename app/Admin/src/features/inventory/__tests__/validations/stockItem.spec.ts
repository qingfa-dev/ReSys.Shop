import { describe, it, expect } from 'vitest'
import {
  stockItemStockLocationId,
  stockItemVariantId,
  stockItemCountOnHand,
  stockItemBackorderable,
  stockItemSchema,
} from '../../validations/stockItem'

describe('stockItemStockLocationId', () => {
  it('accepts a valid stock location id', () => {
    expect(stockItemStockLocationId.safeParse('l-1').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(stockItemStockLocationId.safeParse('').success).toBe(false)
  })
})

describe('stockItemVariantId', () => {
  it('accepts a valid variant id', () => {
    expect(stockItemVariantId.safeParse('v-1').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(stockItemVariantId.safeParse('').success).toBe(false)
  })
})

describe('stockItemCountOnHand', () => {
  it('accepts 0', () => {
    expect(stockItemCountOnHand.safeParse(0).success).toBe(true)
  })

  it('accepts a positive integer', () => {
    expect(stockItemCountOnHand.safeParse(10).success).toBe(true)
  })

  it('rejects -1', () => {
    expect(stockItemCountOnHand.safeParse(-1).success).toBe(false)
  })

  it('rejects a non-integer', () => {
    expect(stockItemCountOnHand.safeParse(1.5).success).toBe(false)
  })
})

describe('stockItemBackorderable', () => {
  it('accepts true', () => {
    expect(stockItemBackorderable.safeParse(true).success).toBe(true)
  })

  it('accepts false', () => {
    expect(stockItemBackorderable.safeParse(false).success).toBe(true)
  })
})

describe('stockItemSchema', () => {
  it('accepts a valid stock item form', () => {
    const result = stockItemSchema.safeParse({
      stockLocationId: 'l-1',
      variantId: 'v-1',
      countOnHand: 10,
      backorderable: true,
    })
    expect(result.success).toBe(true)
  })

  it('rejects missing stockLocationId', () => {
    const result = stockItemSchema.safeParse({
      variantId: 'v-1',
      countOnHand: 10,
      backorderable: true,
    })
    expect(result.success).toBe(false)
  })

  it('returns per-field errors', () => {
    const result = stockItemSchema.safeParse({})
    expect(result.success).toBe(false)
    const fields = result.error!.issues.map(i => String(i.path[0]))
    expect(fields).toContain('stockLocationId')
    expect(fields).toContain('variantId')
    expect(fields).toContain('countOnHand')
    expect(fields).toContain('backorderable')
  })
})
