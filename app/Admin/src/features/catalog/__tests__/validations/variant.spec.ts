import { describe, it, expect } from 'vitest'
import {
  variantSku,
  variantPosition,
  variantPrice,
  variantCostCurrency,
  variantSchema,
} from '../../validations/variant'

const validVariant = {
  sku: 'SHIRT-M',
  position: 0,
  isMaster: false,
  trackInventory: true,
  weight: null,
  weightUnit: null,
  height: null,
  width: null,
  depth: null,
  dimensionsUnit: null,
  price: null,
  costPrice: null,
  costCurrency: null,
}

describe('variantSku', () => {
  it('accepts valid sku', () => {
    expect(variantSku.safeParse('SHIRT-M').success).toBe(true)
  })

  it('rejects empty', () => {
    expect(variantSku.safeParse('').success).toBe(false)
  })

  it('rejects whitespace-only', () => {
    expect(variantSku.safeParse('   ').success).toBe(false)
  })

  it('rejects over 255 chars', () => {
    expect(variantSku.safeParse('A'.repeat(256)).success).toBe(false)
  })
})

describe('variantPosition', () => {
  it('accepts 0 and -1', () => {
    expect(variantPosition.safeParse(0).success).toBe(true)
    expect(variantPosition.safeParse(-1).success).toBe(true)
  })

  it('rejects below -1', () => {
    expect(variantPosition.safeParse(-2).success).toBe(false)
  })
})

describe('variantPrice', () => {
  it('accepts null and non-negative', () => {
    expect(variantPrice.safeParse(null).success).toBe(true)
    expect(variantPrice.safeParse(12.5).success).toBe(true)
  })

  it('rejects negative', () => {
    expect(variantPrice.safeParse(-1).success).toBe(false)
  })
})

describe('variantCostCurrency', () => {
  it('accepts 3-letter code', () => {
    expect(variantCostCurrency.safeParse('USD').success).toBe(true)
  })

  it('rejects longer than 3 chars', () => {
    expect(variantCostCurrency.safeParse('USDT').success).toBe(false)
  })
})

describe('variantSchema', () => {
  it('accepts valid form', () => {
    const result = variantSchema.safeParse(validVariant)
    expect(result.success).toBe(true)
  })

  it('rejects empty sku', () => {
    const result = variantSchema.safeParse({ ...validVariant, sku: '' })
    expect(result.success).toBe(false)
  })

  it('accepts null optional fields', () => {
    const result = variantSchema.safeParse(validVariant)
    expect(result.success).toBe(true)
  })
})
