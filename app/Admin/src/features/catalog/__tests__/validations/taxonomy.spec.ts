import { describe, it, expect } from 'vitest'
import {
  taxonomyName,
  taxonomyPresentation,
  taxonomyPosition,
  taxonomySchema,
} from '../../validations/taxonomy'

describe('taxonomyName', () => {
  it('accepts a valid name', () => {
    expect(taxonomyName.safeParse('Categories').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(taxonomyName.safeParse('').success).toBe(false)
  })

  it('rejects string over 100 characters', () => {
    expect(taxonomyName.safeParse('A'.repeat(101)).success).toBe(false)
  })
})

describe('taxonomyPresentation', () => {
  it('accepts a valid presentation', () => {
    expect(taxonomyPresentation.safeParse('Categories').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(taxonomyPresentation.safeParse('').success).toBe(false)
  })
})

describe('taxonomyPosition', () => {
  it('accepts position 0', () => {
    expect(taxonomyPosition.safeParse(0).success).toBe(true)
  })

  it('accepts position -1', () => {
    expect(taxonomyPosition.safeParse(-1).success).toBe(true)
  })

  it('rejects position -2', () => {
    expect(taxonomyPosition.safeParse(-2).success).toBe(false)
  })
})

describe('taxonomySchema', () => {
  it('accepts valid form', () => {
    const result = taxonomySchema.safeParse({
      name: 'Categories',
      presentation: 'Categories',
      position: 1,
    })
    expect(result.success).toBe(true)
  })

  it('rejects empty name', () => {
    const result = taxonomySchema.safeParse({
      name: '',
      presentation: 'Categories',
      position: 1,
    })
    expect(result.success).toBe(false)
  })
})
