import { describe, it, expect } from 'vitest'
import {
  paymentMethodName,
  paymentMethodCode,
  paymentMethodProviderKey,
  paymentMethodDisplayOn,
  paymentMethodPosition,
  paymentMethodSchema,
  paymentMethodUpdateSchema,
} from '../../validations/paymentMethod'

describe('paymentMethodName', () => {
  it('accepts a valid name', () => {
    expect(paymentMethodName.safeParse('Card').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(paymentMethodName.safeParse('').success).toBe(false)
  })

  it('accepts a 255-character name', () => {
    expect(paymentMethodName.safeParse('a'.repeat(255)).success).toBe(true)
  })

  it('rejects a 256-character name', () => {
    expect(paymentMethodName.safeParse('a'.repeat(256)).success).toBe(false)
  })
})

describe('paymentMethodCode', () => {
  it('accepts a valid code', () => {
    expect(paymentMethodCode.safeParse('card_1').success).toBe(true)
  })

  it('rejects a code with invalid characters', () => {
    expect(paymentMethodCode.safeParse('bad code!').success).toBe(false)
  })

  it('rejects a 51-character code', () => {
    expect(paymentMethodCode.safeParse('a'.repeat(51)).success).toBe(false)
  })
})

describe('paymentMethodProviderKey', () => {
  it('accepts a valid provider key', () => {
    expect(paymentMethodProviderKey.safeParse('stripe').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(paymentMethodProviderKey.safeParse('').success).toBe(false)
  })
})

describe('paymentMethodDisplayOn', () => {
  it('accepts Both', () => {
    expect(paymentMethodDisplayOn.safeParse('Both').success).toBe(true)
  })

  it('rejects an invalid value', () => {
    expect(paymentMethodDisplayOn.safeParse('Bogus').success).toBe(false)
  })
})

describe('paymentMethodPosition', () => {
  it('accepts 0', () => {
    expect(paymentMethodPosition.safeParse(0).success).toBe(true)
  })

  it('accepts 9999', () => {
    expect(paymentMethodPosition.safeParse(9999).success).toBe(true)
  })

  it('rejects -1', () => {
    expect(paymentMethodPosition.safeParse(-1).success).toBe(false)
  })

  it('rejects 10000', () => {
    expect(paymentMethodPosition.safeParse(10000).success).toBe(false)
  })
})

describe('paymentMethodSchema', () => {
  it('accepts a valid payment method form', () => {
    const result = paymentMethodSchema.safeParse({
      name: 'Card',
      code: 'card_1',
      providerKey: 'stripe',
      displayOn: 'Both',
      position: 1,
      active: true,
      webhookEnabled: true,
      autoCapture: true,
    })
    expect(result.success).toBe(true)
  })

  it('rejects missing name', () => {
    const result = paymentMethodSchema.safeParse({
      code: 'card_1',
      providerKey: 'stripe',
      displayOn: 'Both',
      position: 1,
      active: true,
      webhookEnabled: true,
      autoCapture: true,
    })
    expect(result.success).toBe(false)
  })

  it('returns per-field errors', () => {
    const result = paymentMethodSchema.safeParse({})
    expect(result.success).toBe(false)
    const fields = result.error!.issues.map(i => String(i.path[0]))
    expect(fields).toContain('name')
    expect(fields).toContain('code')
    expect(fields).toContain('providerKey')
    expect(fields).toContain('displayOn')
    expect(fields).toContain('position')
    expect(fields).toContain('active')
    expect(fields).toContain('webhookEnabled')
    expect(fields).toContain('autoCapture')
  })
})

describe('paymentMethodUpdateSchema', () => {
  it('accepts a partial update with only name', () => {
    const result = paymentMethodUpdateSchema.safeParse({ name: 'Card Updated' })
    expect(result.success).toBe(true)
  })
})
