import { describe, it, expect } from 'vitest'
import { CATALOG, IDENTITY, LOCATIONS, PROFILES, INVENTORY, ORDERS, PAYMENTS, SHIPPING } from '@/shared/api/constants'

describe('API constants', () => {
  it('CATALOG matches backend', () => {
    expect(CATALOG).toBe('api/catalog')
  })
  it('IDENTITY matches backend', () => {
    expect(IDENTITY).toBe('api/identity')
  })
  it('LOCATIONS matches backend', () => {
    expect(LOCATIONS).toBe('api/locations')
  })
  it('PROFILES matches backend', () => {
    expect(PROFILES).toBe('api/profiles')
  })
  it('INVENTORY matches backend', () => {
    expect(INVENTORY).toBe('api/inventory')
  })
  it('ORDERS matches backend', () => {
    expect(ORDERS).toBe('api/ordering')
  })
  it('PAYMENTS matches backend', () => {
    expect(PAYMENTS).toBe('api/payment')
  })
  it('SHIPPING matches backend', () => {
    expect(SHIPPING).toBe('api/shipping')
  })
})
