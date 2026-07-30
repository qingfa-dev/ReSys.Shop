import { describe, it, expect } from 'vitest'
import {
  optionValueOptionTypeId,
  optionValueName,
  optionValuePresentation,
  optionValuePosition,
  optionValueSchema,
} from '../../validations/optionValue'

describe('optionValueOptionTypeId', () => {
  it('accepts a valid GUID', () => {
    expect(optionValueOptionTypeId.safeParse('abc-123-def').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(optionValueOptionTypeId.safeParse('').success).toBe(false)
  })
})

describe('optionValueName', () => {
  it('accepts a valid name', () => {
    expect(optionValueName.safeParse('Medium').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(optionValueName.safeParse('').success).toBe(false)
  })

  it('rejects string over 100 characters', () => {
    expect(optionValueName.safeParse('A'.repeat(101)).success).toBe(false)
  })
})

describe('optionValuePresentation', () => {
  it('accepts a valid presentation', () => {
    expect(optionValuePresentation.safeParse('Medium').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(optionValuePresentation.safeParse('').success).toBe(false)
  })
})

describe('optionValuePosition', () => {
  it('accepts position 0', () => {
    expect(optionValuePosition.safeParse(0).success).toBe(true)
  })

  it('rejects position -2', () => {
    expect(optionValuePosition.safeParse(-2).success).toBe(false)
  })

  it('rejects non-integer', () => {
    expect(optionValuePosition.safeParse(1.5).success).toBe(false)
  })
})

describe('optionValueSchema', () => {
  it('accepts valid form', () => {
    const result = optionValueSchema.safeParse({
      optionTypeId: 'abc-123',
      name: 'Medium',
      presentation: 'Medium',
      position: 2,
    })
    expect(result.success).toBe(true)
  })

  it('rejects missing required fields', () => {
    const result = optionValueSchema.safeParse({
      optionTypeId: '',
      name: '',
      presentation: '',
      position: 1,
    })
    expect(result.success).toBe(false)
    const fields = result.error!.issues.map(i => String(i.path[0]))
    expect(fields).toContain('optionTypeId')
    expect(fields).toContain('name')
    expect(fields).toContain('presentation')
  })
})
