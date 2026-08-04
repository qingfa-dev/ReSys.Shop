import { describe, it, expect } from 'vitest'
import {
  shippingMethodName,
  shippingMethodCalculatorType,
  shippingMethodCode,
  shippingMethodPosition,
  shippingMethodSchema,
} from '../../validations/shippingMethod'

describe('shippingMethodName', () => {
  it('accepts a valid name', () => {
    expect(shippingMethodName.safeParse('X').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(shippingMethodName.safeParse('').success).toBe(false)
  })

  it('accepts a 255-character name', () => {
    expect(shippingMethodName.safeParse('a'.repeat(255)).success).toBe(true)
  })

  it('rejects a 256-character name', () => {
    expect(shippingMethodName.safeParse('a'.repeat(256)).success).toBe(false)
  })
})

describe('shippingMethodCalculatorType', () => {
  it('accepts FlatRate', () => {
    expect(shippingMethodCalculatorType.safeParse('FlatRate').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(shippingMethodCalculatorType.safeParse('').success).toBe(false)
  })
})

describe('shippingMethodCode', () => {
  it('accepts a valid code', () => {
    expect(shippingMethodCode.safeParse('x').success).toBe(true)
  })

  it('accepts undefined', () => {
    expect(shippingMethodCode.safeParse(undefined).success).toBe(true)
  })
})

describe('shippingMethodPosition', () => {
  it('accepts 0', () => {
    expect(shippingMethodPosition.safeParse(0).success).toBe(true)
  })

  it('rejects -1', () => {
    expect(shippingMethodPosition.safeParse(-1).success).toBe(false)
  })
})

describe('shippingMethodSchema', () => {
  it('accepts a valid shipping method form', () => {
    const result = shippingMethodSchema.safeParse({
      name: 'Express',
      code: 'express',
      calculatorType: 'FlatRate',
      position: 1,
      availableToUsers: true,
    })
    expect(result.success).toBe(true)
  })

  it('rejects missing name', () => {
    const result = shippingMethodSchema.safeParse({
      code: 'express',
      calculatorType: 'FlatRate',
      position: 1,
      availableToUsers: true,
    })
    expect(result.success).toBe(false)
  })

  it('rejects missing calculatorType', () => {
    const result = shippingMethodSchema.safeParse({
      name: 'Express',
      code: 'express',
      position: 1,
      availableToUsers: true,
    })
    expect(result.success).toBe(false)
  })
})
