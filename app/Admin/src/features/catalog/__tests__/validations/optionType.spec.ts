import { describe, it, expect } from 'vitest'
import {
  optionTypeName,
  optionTypePresentation,
  optionTypePosition,
  optionTypeFilterable,
  optionTypeSchema,
} from '../../validations/optionType'

describe('optionTypeName', () => {
  it('accepts a valid name', () => {
    expect(optionTypeName.safeParse('Size').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(optionTypeName.safeParse('').success).toBe(false)
  })

  it('rejects string over 100 characters', () => {
    expect(optionTypeName.safeParse('A'.repeat(101)).success).toBe(false)
  })

  it('accepts string of exactly 100 characters', () => {
    expect(optionTypeName.safeParse('A'.repeat(100)).success).toBe(true)
  })
})

describe('optionTypePresentation', () => {
  it('accepts a valid presentation', () => {
    expect(optionTypePresentation.safeParse('Select a size').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(optionTypePresentation.safeParse('').success).toBe(false)
  })

  it('rejects string over 100 characters', () => {
    expect(optionTypePresentation.safeParse('A'.repeat(101)).success).toBe(false)
  })
})

describe('optionTypePosition', () => {
  it('accepts position 0', () => {
    expect(optionTypePosition.safeParse(0).success).toBe(true)
  })

  it('accepts position -1', () => {
    expect(optionTypePosition.safeParse(-1).success).toBe(true)
  })

  it('rejects position -2', () => {
    expect(optionTypePosition.safeParse(-2).success).toBe(false)
  })

  it('rejects non-integer', () => {
    expect(optionTypePosition.safeParse(1.5).success).toBe(false)
  })
})

describe('optionTypeFilterable', () => {
  it('accepts true', () => {
    expect(optionTypeFilterable.safeParse(true).success).toBe(true)
  })

  it('accepts false', () => {
    expect(optionTypeFilterable.safeParse(false).success).toBe(true)
  })
})

describe('optionTypeSchema', () => {
  it('accepts valid form', () => {
    const result = optionTypeSchema.safeParse({
      name: 'Size',
      presentation: 'Select a size',
      position: 1,
      filterable: true,
    })
    expect(result.success).toBe(true)
  })

  it('rejects missing required name', () => {
    const result = optionTypeSchema.safeParse({
      name: '',
      presentation: 'Select a size',
      position: 1,
      filterable: true,
    })
    expect(result.success).toBe(false)
  })

  it('returns per-field errors', () => {
    const result = optionTypeSchema.safeParse({
      name: '',
      presentation: '',
      position: -2,
      filterable: true,
    })
    expect(result.success).toBe(false)
    const fields = result.error!.issues.map(i => String(i.path[0]))
    expect(fields).toContain('name')
    expect(fields).toContain('presentation')
    expect(fields).toContain('position')
  })
})
