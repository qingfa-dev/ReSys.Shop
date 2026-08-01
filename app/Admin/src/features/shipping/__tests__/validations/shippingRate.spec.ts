import { describe, it, expect } from 'vitest'
import {
  shippingRateName,
  shippingRateCost,
  shippingRateShippingMethodId,
  shippingRateMinWeight,
  shippingRateSchema,
} from '../../validations/shippingRate'

describe('shippingRateName', () => {
  it('accepts a valid name', () => {
    expect(shippingRateName.safeParse('X').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(shippingRateName.safeParse('').success).toBe(false)
  })
})

describe('shippingRateCost', () => {
  it('accepts 1', () => {
    expect(shippingRateCost.safeParse(1).success).toBe(true)
  })

  it('rejects 0', () => {
    expect(shippingRateCost.safeParse(0).success).toBe(false)
  })

  it('rejects -1', () => {
    expect(shippingRateCost.safeParse(-1).success).toBe(false)
  })
})

describe('shippingRateShippingMethodId', () => {
  it('accepts a valid id', () => {
    expect(shippingRateShippingMethodId.safeParse('sm-1').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(shippingRateShippingMethodId.safeParse('').success).toBe(false)
  })
})

describe('shippingRateMinWeight', () => {
  it('accepts 0', () => {
    expect(shippingRateMinWeight.safeParse(0).success).toBe(true)
  })

  it('accepts 5', () => {
    expect(shippingRateMinWeight.safeParse(5).success).toBe(true)
  })

  it('rejects -1', () => {
    expect(shippingRateMinWeight.safeParse(-1).success).toBe(false)
  })
})

describe('shippingRateSchema', () => {
  it('accepts a valid shipping rate form', () => {
    const result = shippingRateSchema.safeParse({
      name: 'Standard',
      cost: 5,
      shippingMethodId: 'sm-1',
      minWeight: 0,
    })
    expect(result.success).toBe(true)
  })

  it('rejects missing cost', () => {
    const result = shippingRateSchema.safeParse({
      name: 'Standard',
      shippingMethodId: 'sm-1',
      minWeight: 0,
    })
    expect(result.success).toBe(false)
  })

  it('rejects missing shippingMethodId', () => {
    const result = shippingRateSchema.safeParse({
      name: 'Standard',
      cost: 5,
      minWeight: 0,
    })
    expect(result.success).toBe(false)
  })
})
