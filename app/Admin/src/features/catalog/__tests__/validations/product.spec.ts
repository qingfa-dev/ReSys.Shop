import { describe, it, expect } from 'vitest'
import {
  productName,
  productSlug,
  productDepartment,
  productGenderTarget,
  productSchema,
} from '../../validations/product'

const validProduct = {
  name: 'Cotton T-Shirt',
  slug: 'cotton-t-shirt',
  description: null,
  metaTitle: null,
  metaDescription: null,
  metaKeywords: null,
  availableOn: null,
  discontinueOn: null,
  trackInventory: true,
  styleCode: null,
  seasonName: null,
  materialComposition: null,
  careInstructions: null,
  fitNotes: null,
  department: null,
  genderTarget: null,
}

describe('productName', () => {
  it('accepts valid name', () => {
    expect(productName.safeParse('Cotton T-Shirt').success).toBe(true)
  })

  it('rejects empty', () => {
    expect(productName.safeParse('').success).toBe(false)
  })

  it('rejects over 255 chars', () => {
    expect(productName.safeParse('A'.repeat(256)).success).toBe(false)
  })
})

describe('productSlug', () => {
  it('accepts valid slug', () => {
    expect(productSlug.safeParse('cotton-t-shirt').success).toBe(true)
  })

  it('rejects uppercase', () => {
    expect(productSlug.safeParse('Cotton-T-Shirt').success).toBe(false)
  })

  it('rejects spaces', () => {
    expect(productSlug.safeParse('cotton t shirt').success).toBe(false)
  })

  it('rejects empty', () => {
    expect(productSlug.safeParse('').success).toBe(false)
  })
})

describe('productDepartment', () => {
  it('accepts valid department', () => {
    expect(productDepartment.safeParse('Mens').success).toBe(true)
  })

  it('rejects over 50 chars', () => {
    expect(productDepartment.safeParse('A'.repeat(51)).success).toBe(false)
  })

  it('accepts null', () => {
    expect(productDepartment.safeParse(null).success).toBe(true)
  })
})

describe('productGenderTarget', () => {
  it('accepts valid gender', () => {
    expect(productGenderTarget.safeParse('Unisex').success).toBe(true)
  })

  it('rejects over 20 chars', () => {
    expect(productGenderTarget.safeParse('A'.repeat(21)).success).toBe(false)
  })
})

describe('productSchema', () => {
  it('accepts valid form', () => {
    const result = productSchema.safeParse(validProduct)
    expect(result.success).toBe(true)
  })

  it('rejects empty name', () => {
    const result = productSchema.safeParse({ ...validProduct, name: '' })
    expect(result.success).toBe(false)
  })

  it('rejects invalid slug', () => {
    const result = productSchema.safeParse({ ...validProduct, slug: 'Invalid Slug' })
    expect(result.success).toBe(false)
  })

  it('accepts null optional fields', () => {
    const result = productSchema.safeParse(validProduct)
    expect(result.success).toBe(true)
  })
})
