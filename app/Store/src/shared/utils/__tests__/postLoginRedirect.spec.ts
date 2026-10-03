import { describe, it, expect } from 'vitest'
import { validateRedirect } from '@/shared/utils/postLoginRedirect'

describe('validateRedirect', () => {
  it('returns root for a null path', () => {
    expect(validateRedirect(null)).toBe('/')
  })

  it('returns root for an empty path', () => {
    expect(validateRedirect('')).toBe('/')
  })

  it('returns a safe internal path unchanged', () => {
    expect(validateRedirect('/products')).toBe('/products')
  })

  it('returns root for an absolute external URL', () => {
    expect(validateRedirect('https://evil.com')).toBe('/')
  })

  it('rejects protocol-relative URLs', () => {
    expect(validateRedirect('//evil.com')).toBe('/')
  })

  it('returns root for a non-slash-prefixed relative path', () => {
    expect(validateRedirect('products')).toBe('/')
  })
})
