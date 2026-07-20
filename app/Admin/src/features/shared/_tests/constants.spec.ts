import { describe, it, expect } from 'vitest'
import { CATALOG, IDENTITY, LOCATIONS, PROFILES, INVENTORY, ORDERS, PAYMENTS, SHIPPING } from '@/common/api/constants'

describe('API constants', () => {
  it('CATALOG matches backend', () => {
    expect(CATALOG).toBe('catalog')
  })
  it('IDENTITY matches backend', () => {
    expect(IDENTITY).toBe('identity')
  })
  it('LOCATIONS matches backend', () => {
    expect(LOCATIONS).toBe('locations')
  })
  it('PROFILES matches backend', () => {
    expect(PROFILES).toBe('profiles')
  })
  it('INVENTORY matches backend', () => {
    expect(INVENTORY).toBe('inventory')
  })
  it('ORDERS matches backend', () => {
    expect(ORDERS).toBe('ordering')
  })
  it('PAYMENTS matches backend', () => {
    expect(PAYMENTS).toBe('payment')
  })
  it('SHIPPING matches backend', () => {
    expect(SHIPPING).toBe('shipping')
  })
})
