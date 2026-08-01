import { describe, it, expect } from 'vitest'
import {
  stockLocationName,
  stockLocationCode,
  stockLocationCity,
  stockLocationPostalCode,
  stockLocationPhone,
  stockLocationPosition,
  stockLocationActive,
  stockLocationSchema,
} from '../../validations/stockLocation'

describe('stockLocationName', () => {
  it('accepts a valid name', () => {
    expect(stockLocationName.safeParse('X').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(stockLocationName.safeParse('').success).toBe(false)
  })

  it('accepts a name of exactly 255 characters', () => {
    expect(stockLocationName.safeParse('A'.repeat(255)).success).toBe(true)
  })

  it('rejects a name over 255 characters', () => {
    expect(stockLocationName.safeParse('A'.repeat(256)).success).toBe(false)
  })
})

describe('optional stock location fields', () => {
  it('accepts a value for code', () => {
    expect(stockLocationCode.safeParse('MAIN').success).toBe(true)
  })

  it('accepts undefined for code', () => {
    expect(stockLocationCode.safeParse(undefined).success).toBe(true)
  })

  it('accepts a value for city', () => {
    expect(stockLocationCity.safeParse('Hanoi').success).toBe(true)
  })

  it('accepts undefined for city', () => {
    expect(stockLocationCity.safeParse(undefined).success).toBe(true)
  })

  it('accepts a value for postalCode', () => {
    expect(stockLocationPostalCode.safeParse('10000').success).toBe(true)
  })

  it('accepts undefined for postalCode', () => {
    expect(stockLocationPostalCode.safeParse(undefined).success).toBe(true)
  })

  it('accepts a value for phone', () => {
    expect(stockLocationPhone.safeParse('123').success).toBe(true)
  })

  it('accepts undefined for phone', () => {
    expect(stockLocationPhone.safeParse(undefined).success).toBe(true)
  })
})

describe('stockLocationPosition', () => {
  it('accepts 0', () => {
    expect(stockLocationPosition.safeParse(0).success).toBe(true)
  })

  it('rejects -1', () => {
    expect(stockLocationPosition.safeParse(-1).success).toBe(false)
  })

  it('rejects a non-integer', () => {
    expect(stockLocationPosition.safeParse(1.5).success).toBe(false)
  })
})

describe('stockLocationActive', () => {
  it('accepts true', () => {
    expect(stockLocationActive.safeParse(true).success).toBe(true)
  })

  it('accepts false', () => {
    expect(stockLocationActive.safeParse(false).success).toBe(true)
  })
})

describe('stockLocationSchema', () => {
  it('accepts a valid stock location form', () => {
    const result = stockLocationSchema.safeParse({
      name: 'Main',
      code: 'MAIN',
      city: 'Hanoi',
      postalCode: '10000',
      phone: '123',
      position: 0,
      active: true,
    })
    expect(result.success).toBe(true)
  })

  it('accepts a minimal valid form', () => {
    const result = stockLocationSchema.safeParse({
      name: 'Main',
      position: 0,
      active: true,
    })
    expect(result.success).toBe(true)
  })

  it('rejects an empty name', () => {
    const result = stockLocationSchema.safeParse({
      name: '',
      position: 0,
      active: true,
    })
    expect(result.success).toBe(false)
  })
})
