import { describe, it, expect } from 'vitest'
import { variantsListUrl } from '../../utils/variantListUrl'

describe('variantsListUrl', () => {
  it('includes productId when present', () => {
    expect(variantsListUrl('abc')).toBe('api/catalog/variants?productId=abc')
  })
  it('omits productId when absent', () => {
    expect(variantsListUrl(null)).toBe('api/catalog/variants')
    expect(variantsListUrl(undefined)).toBe('api/catalog/variants')
  })
})
