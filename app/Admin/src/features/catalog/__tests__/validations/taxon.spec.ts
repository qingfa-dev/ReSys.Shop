import { describe, it, expect } from 'vitest'
import {
  taxonName,
  taxonSlug,
  taxonPosition,
  taxonSchema,
} from '../../validations/taxon'

describe('taxonName', () => {
  it('accepts a valid name', () => {
    expect(taxonName.safeParse('Shoes').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(taxonName.safeParse('').success).toBe(false)
  })

  it('rejects string over 255 characters', () => {
    expect(taxonName.safeParse('A'.repeat(256)).success).toBe(false)
  })
})

describe('taxonSlug', () => {
  it('accepts valid slug', () => {
    expect(taxonSlug.safeParse('running-shoes').success).toBe(true)
  })

  it('rejects uppercase', () => {
    expect(taxonSlug.safeParse('Running-Shoes').success).toBe(false)
  })

  it('rejects spaces', () => {
    expect(taxonSlug.safeParse('running shoes').success).toBe(false)
  })

  it('rejects empty', () => {
    expect(taxonSlug.safeParse('').success).toBe(false)
  })
})

describe('taxonPosition', () => {
  it('accepts position 0', () => {
    expect(taxonPosition.safeParse(0).success).toBe(true)
  })

  it('rejects position -2', () => {
    expect(taxonPosition.safeParse(-2).success).toBe(false)
  })
})

describe('taxonSchema', () => {
  const validTaxon = {
    taxonomyId: 'abc-123',
    parentId: null,
    name: 'Shoes',
    presentation: 'Shoes',
    slug: 'shoes',
    description: null,
    position: 0,
    metaTitle: null,
    metaDescription: null,
    metaKeywords: null,
    imageUrl: null,
    squareImageUrl: null,
    automatic: false,
    rulesMatchPolicy: 'All',
    sortOrder: 'Manual',
    hideFromNav: false,
  }

  it('accepts valid form', () => {
    const result = taxonSchema.safeParse(validTaxon)
    expect(result.success).toBe(true)
  })

  it('rejects empty name', () => {
    const result = taxonSchema.safeParse({ ...validTaxon, name: '' })
    expect(result.success).toBe(false)
  })

  it('accepts parentId as null', () => {
    const result = taxonSchema.safeParse(validTaxon)
    expect(result.success).toBe(true)
  })
})
