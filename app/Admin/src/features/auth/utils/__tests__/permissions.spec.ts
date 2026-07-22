import { describe, it, expect } from 'vitest'
import { hasPermission, hasAnyPermission, hasAllPermissions } from '../permissions'

describe('hasPermission', () => {
  it('should match exact permission', () => {
    expect(hasPermission('catalog.view', ['catalog.view', 'catalog.edit'])).toBe(true)
  })

  it('should match wildcard permission', () => {
    expect(hasPermission('catalog.create', ['*'])).toBe(true)
  })

  it('should reject missing permission', () => {
    expect(hasPermission('catalog.delete', ['catalog.view'])).toBe(false)
  })
})

describe('hasAnyPermission', () => {
  it('should return true if any matches', () => {
    expect(hasAnyPermission(['catalog.view', 'catalog.delete'], ['catalog.view'])).toBe(true)
  })
})

describe('hasAllPermissions', () => {
  it('should return true if all match', () => {
    expect(hasAllPermissions(['catalog.view', 'catalog.edit'], ['catalog.view', 'catalog.edit', 'catalog.create'])).toBe(true)
  })

  it('should return false if any missing', () => {
    expect(hasAllPermissions(['catalog.view', 'catalog.delete'], ['catalog.view'])).toBe(false)
  })
})
