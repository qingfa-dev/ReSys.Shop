import { describe, it, expect } from 'vitest'
import { resolvePostLoginRedirect } from '@/shared/utils/postLoginRedirect'

describe('resolvePostLoginRedirect', () => {
  it('returns a safe internal path unchanged', () => {
    expect(resolvePostLoginRedirect('/catalog/products')).toBe('/catalog/products')
  })

  it('returns the default root for an absolute external URL', () => {
    expect(resolvePostLoginRedirect('https://evil.com')).toBe('/')
  })

  it('rejects protocol-relative URLs', () => {
    expect(resolvePostLoginRedirect('//evil.com')).toBe('/')
  })

  it('rejects backslash-prefixed URLs', () => {
    expect(resolvePostLoginRedirect('/\\evil.com')).toBe('/')
  })

  it('returns the fallback for non-string values', () => {
    expect(resolvePostLoginRedirect(undefined)).toBe('/')
    expect(resolvePostLoginRedirect(['/catalog/products'])).toBe('/')
  })

  it('honors a custom fallback', () => {
    expect(resolvePostLoginRedirect('https://evil.com', '/dashboard')).toBe('/dashboard')
  })
})
