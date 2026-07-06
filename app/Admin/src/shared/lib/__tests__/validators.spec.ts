import { describe, it, expect } from 'vitest'
import { isEmail, isUrl, isGuid } from '../validators'

describe('validators', () => {
  it('isEmail', () => {
    expect(isEmail('a@b.c')).toBe(true)
    expect(isEmail('nope')).toBe(false)
  })
  it('isUrl', () => {
    expect(isUrl('https://x.y')).toBe(true)
    expect(isUrl('x')).toBe(false)
  })
  it('isGuid', () => {
    expect(isGuid('11111111-2222-3333-4444-555555555555')).toBe(true)
    expect(isGuid('not-a-guid')).toBe(false)
  })
})
