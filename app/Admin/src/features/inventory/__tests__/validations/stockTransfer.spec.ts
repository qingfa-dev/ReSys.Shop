import { describe, it, expect } from 'vitest'
import {
  stockTransferQuantity,
  stockTransferItems,
  stockTransferSchema,
} from '../../validations/stockTransfer'

describe('stockTransferQuantity', () => {
  it('accepts 1', () => {
    expect(stockTransferQuantity.safeParse(1).success).toBe(true)
  })

  it('accepts a positive integer', () => {
    expect(stockTransferQuantity.safeParse(5).success).toBe(true)
  })

  it('rejects 0', () => {
    expect(stockTransferQuantity.safeParse(0).success).toBe(false)
  })

  it('rejects -1', () => {
    expect(stockTransferQuantity.safeParse(-1).success).toBe(false)
  })
})

describe('stockTransferItems', () => {
  it('accepts a non-empty array', () => {
    expect(stockTransferItems.safeParse([{ variantId: 'v-1', quantity: 1 }]).success).toBe(true)
  })

  it('rejects an empty array', () => {
    expect(stockTransferItems.safeParse([]).success).toBe(false)
  })
})

describe('stockTransferSchema', () => {
  it('accepts a valid transfer form', () => {
    const result = stockTransferSchema.safeParse({
      sourceLocationId: 'l-1',
      destinationLocationId: 'l-2',
      items: [{ variantId: 'v-1', quantity: 5 }],
    })
    expect(result.success).toBe(true)
  })

  it('rejects same source and destination locations', () => {
    const result = stockTransferSchema.safeParse({
      sourceLocationId: 'l-1',
      destinationLocationId: 'l-1',
      items: [{ variantId: 'v-1', quantity: 5 }],
    })
    expect(result.success).toBe(false)
    const fields = result.error!.issues.map(i => String(i.path[0]))
    expect(fields).toContain('destinationLocationId')
  })

  it('rejects empty items', () => {
    const result = stockTransferSchema.safeParse({
      sourceLocationId: 'l-1',
      destinationLocationId: 'l-2',
      items: [],
    })
    expect(result.success).toBe(false)
  })

  it('rejects a missing source location', () => {
    const result = stockTransferSchema.safeParse({
      destinationLocationId: 'l-2',
      items: [{ variantId: 'v-1', quantity: 5 }],
    })
    expect(result.success).toBe(false)
  })
})
