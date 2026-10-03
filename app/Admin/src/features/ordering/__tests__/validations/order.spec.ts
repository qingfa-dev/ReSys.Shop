import { describe, it, expect } from 'vitest'
import {
  orderCurrency,
  orderEmail,
  orderLineItemQuantity,
  orderLineItemPrice,
  orderStatus,
  orderSchema,
  addLineItemSchema,
} from '../../validations/order'

describe('orderCurrency', () => {
  it('accepts a valid 3-letter currency code', () => {
    expect(orderCurrency.safeParse('USD').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(orderCurrency.safeParse('').success).toBe(false)
  })

  it('rejects a 4-character currency code', () => {
    expect(orderCurrency.safeParse('USDD').success).toBe(false)
  })
})

describe('orderEmail', () => {
  it('accepts a valid email', () => {
    expect(orderEmail.safeParse('a@b.com').success).toBe(true)
  })

  it('accepts undefined', () => {
    expect(orderEmail.safeParse(undefined).success).toBe(true)
  })

  it('rejects a non-email string', () => {
    expect(orderEmail.safeParse('not-email').success).toBe(false)
  })
})

describe('orderLineItemQuantity', () => {
  it('accepts 1', () => {
    expect(orderLineItemQuantity.safeParse(1).success).toBe(true)
  })

  it('accepts 999', () => {
    expect(orderLineItemQuantity.safeParse(999).success).toBe(true)
  })

  it('rejects 0', () => {
    expect(orderLineItemQuantity.safeParse(0).success).toBe(false)
  })

  it('rejects 1000', () => {
    expect(orderLineItemQuantity.safeParse(1000).success).toBe(false)
  })

  it('rejects a non-integer', () => {
    expect(orderLineItemQuantity.safeParse(1.5).success).toBe(false)
  })
})

describe('orderLineItemPrice', () => {
  it('accepts 0', () => {
    expect(orderLineItemPrice.safeParse(0).success).toBe(true)
  })

  it('rejects -1', () => {
    expect(orderLineItemPrice.safeParse(-1).success).toBe(false)
  })
})

describe('orderStatus', () => {
  it('accepts a valid order status', () => {
    expect(orderStatus.safeParse('Placed').success).toBe(true)
  })

  it('rejects an invalid order status', () => {
    expect(orderStatus.safeParse('Bogus').success).toBe(false)
  })
})

describe('orderSchema', () => {
  it('accepts a valid order form', () => {
    const result = orderSchema.safeParse({
      currency: 'USD',
      email: 'a@b.com',
    })
    expect(result.success).toBe(true)
  })

  it('rejects missing currency', () => {
    const result = orderSchema.safeParse({
      email: 'a@b.com',
    })
    expect(result.success).toBe(false)
  })
})

describe('addLineItemSchema', () => {
  it('accepts a valid line item form', () => {
    const result = addLineItemSchema.safeParse({
      variantId: 'v-1',
      quantity: 2,
      price: 19.99,
    })
    expect(result.success).toBe(true)
  })

  it('rejects missing variantId', () => {
    const result = addLineItemSchema.safeParse({
      quantity: 2,
      price: 19.99,
    })
    expect(result.success).toBe(false)
  })
})
