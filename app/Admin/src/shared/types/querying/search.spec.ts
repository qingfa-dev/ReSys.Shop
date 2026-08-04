import { describe, it, expect } from 'vitest'
import { defaultSearchTerm, emptySearchModel } from './search'

describe('defaultSearchTerm', () => {
  it('has empty value and case-insensitive', () => {
    expect(defaultSearchTerm.value).toBe('')
    expect(defaultSearchTerm.caseSensitive).toBe(false)
  })
})

describe('emptySearchModel', () => {
  it('is empty and valid', () => {
    expect(emptySearchModel.isEmpty).toBe(true)
    expect(emptySearchModel.isValid).toBe(true)
    expect(emptySearchModel.fields).toHaveLength(0)
  })
})
