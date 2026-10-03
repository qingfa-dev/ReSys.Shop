import { describe, it, expect } from 'vitest'
import {
  taxonRuleType,
  taxonRuleValue,
  taxonRuleSchema,
} from '../../validations/taxonRule'

describe('taxonRuleType', () => {
  it('accepts valid type', () => {
    expect(taxonRuleType.safeParse('product_name').success).toBe(true)
  })

  it('rejects empty', () => {
    expect(taxonRuleType.safeParse('').success).toBe(false)
  })
})

describe('taxonRuleValue', () => {
  it('accepts valid value', () => {
    expect(taxonRuleValue.safeParse('Nike').success).toBe(true)
  })

  it('rejects empty', () => {
    expect(taxonRuleValue.safeParse('').success).toBe(false)
  })

  it('rejects over 255 characters', () => {
    expect(taxonRuleValue.safeParse('A'.repeat(256)).success).toBe(false)
  })
})

describe('taxonRuleSchema', () => {
  it('accepts valid rule', () => {
    const result = taxonRuleSchema.safeParse({
      type: 'product_name',
      matchPolicy: 'contains',
      value: 'Nike',
    })
    expect(result.success).toBe(true)
  })

  it('rejects empty type', () => {
    const result = taxonRuleSchema.safeParse({
      type: '',
      matchPolicy: 'contains',
      value: 'Nike',
    })
    expect(result.success).toBe(false)
  })
})
