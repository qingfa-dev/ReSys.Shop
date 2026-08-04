import { describe, it, expect } from 'vitest'
import { API_MODULES, CATALOG, DASHBOARD } from './api'

describe('API_MODULES', () => {
  it('exports all module routes', () => {
    expect(API_MODULES).toEqual({
      CATALOG: 'api/catalog',
      IDENTITY: 'api/identity',
      INVENTORY: 'api/inventory',
      LOCATION: 'api/locations',
      ORDERING: 'api/ordering',
      PAYMENT: 'api/payment',
      PROFILE: 'api/profiles',
      SHIPPING: 'api/shipping',
      DASHBOARD: 'api/dashboard',
    })
  })

  it('exports individual constants that match', () => {
    expect(CATALOG).toBe('api/catalog')
    expect(DASHBOARD).toBe('api/dashboard')
  })
})
