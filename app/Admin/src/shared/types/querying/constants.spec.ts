import { describe, it, expect } from 'vitest'
import {
  tryParseOperator,
  toDslToken,
  isCaseSensitiveOperator,
  isNegationOperator,
  isStringOnlyOperator,
  OPERATOR_TO_TOKEN,
  DSL_DELIMITERS,
  JSON_KEYS,
  NULL_SENTINEL,
  BOOLEAN_ALIASES,
  NAVIGATION_SEPARATOR,
  CASE_INSENSITIVE_SUFFIX,
} from './constants'
import { FilterOperator } from './enums'

describe('OPERATOR_TO_TOKEN', () => {
  it('maps every FilterOperator to a DSL token', () => {
    expect(OPERATOR_TO_TOKEN[FilterOperator.Equal]).toBe('=')
    expect(OPERATOR_TO_TOKEN[FilterOperator.Contains]).toBe('*')
    expect(OPERATOR_TO_TOKEN[FilterOperator.GreaterThan]).toBe('>')
    expect(OPERATOR_TO_TOKEN[FilterOperator.NotContains]).toBe('!*')
  })

  it('covers all 16 operators', () => {
    expect(Object.keys(OPERATOR_TO_TOKEN)).toHaveLength(16)
  })
})

describe('tryParseOperator', () => {
  it('resolves DSL tokens to operators', () => {
    expect(tryParseOperator('=')).toBe(FilterOperator.Equal)
    expect(tryParseOperator('==')).toBe(FilterOperator.EqualCaseSensitive)
    expect(tryParseOperator('!=')).toBe(FilterOperator.NotEqual)
    expect(tryParseOperator('*')).toBe(FilterOperator.Contains)
    expect(tryParseOperator('*~')).toBe(FilterOperator.ContainsCaseSensitive)
    expect(tryParseOperator('!*')).toBe(FilterOperator.NotContains)
    expect(tryParseOperator('^')).toBe(FilterOperator.StartsWith)
    expect(tryParseOperator('!$')).toBe(FilterOperator.NotEndsWith)
  })

  it('resolves JSON alias tokens', () => {
    expect(tryParseOperator('eq')).toBe(FilterOperator.Equal)
    expect(tryParseOperator('neq')).toBe(FilterOperator.NotEqual)
    expect(tryParseOperator('contains')).toBe(FilterOperator.Contains)
    expect(tryParseOperator('ncontains')).toBe(FilterOperator.NotContains)
  })

  it('returns null for unknown tokens', () => {
    expect(tryParseOperator('???')).toBeNull()
    expect(tryParseOperator('')).toBeNull()
  })
})

describe('toDslToken', () => {
  it('converts operator to DSL token', () => {
    expect(toDslToken(FilterOperator.GreaterThanOrEqual)).toBe('>=')
    expect(toDslToken(FilterOperator.EndsWith)).toBe('$')
  })

  it('falls back to = for unknown', () => {
    expect(toDslToken('BogusOperator')).toBe('=')
  })
})

describe('operator classifiers', () => {
  it('identifies case-sensitive operators', () => {
    expect(isCaseSensitiveOperator(FilterOperator.EqualCaseSensitive)).toBe(true)
    expect(isCaseSensitiveOperator(FilterOperator.ContainsCaseSensitive)).toBe(true)
    expect(isCaseSensitiveOperator(FilterOperator.Equal)).toBe(false)
  })

  it('identifies negation operators', () => {
    expect(isNegationOperator(FilterOperator.NotEqual)).toBe(true)
    expect(isNegationOperator(FilterOperator.NotContains)).toBe(true)
    expect(isNegationOperator(FilterOperator.Equal)).toBe(false)
  })

  it('identifies string-only operators', () => {
    expect(isStringOnlyOperator(FilterOperator.Contains)).toBe(true)
    expect(isStringOnlyOperator(FilterOperator.StartsWithCaseSensitive)).toBe(true)
    expect(isStringOnlyOperator(FilterOperator.NotEndsWith)).toBe(true)
    expect(isStringOnlyOperator(FilterOperator.Equal)).toBe(false)
    expect(isStringOnlyOperator(FilterOperator.GreaterThan)).toBe(false)
  })
})

describe('constants', () => {
  it('defines DSL delimiters', () => {
    expect(DSL_DELIMITERS.AND).toBe(',')
    expect(DSL_DELIMITERS.OR).toBe('|')
    expect(DSL_DELIMITERS.WILDCARD).toBe('*')
  })

  it('defines JSON keys', () => {
    expect(JSON_KEYS.FIELD).toBe('field')
    expect(JSON_KEYS.OP).toBe('op')
  })

  it('defines special values', () => {
    expect(NULL_SENTINEL).toBe('null')
    expect(CASE_INSENSITIVE_SUFFIX).toBe('~')
    expect(NAVIGATION_SEPARATOR).toBe('.')
  })

  it('defines boolean aliases', () => {
    expect(BOOLEAN_ALIASES['1']).toBe(true)
    expect(BOOLEAN_ALIASES['0']).toBe(false)
    expect(BOOLEAN_ALIASES.yes).toBe(true)
    expect(BOOLEAN_ALIASES.no).toBe(false)
  })
})
