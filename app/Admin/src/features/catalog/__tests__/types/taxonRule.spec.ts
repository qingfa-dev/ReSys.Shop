import { describe, it, expect } from 'vitest'
import { toTaxonRuleQueryParams, TAXON_RULE_TYPES, TAXON_RULE_MATCH_POLICIES } from '../../types/taxonRule'

describe('toTaxonRuleQueryParams', () => {
  it('returns null filter when query is empty', () => {
    const result = toTaxonRuleQueryParams({})
    expect(result.filter).toBeNull()
  })

  it('builds filter for taxonId', () => {
    const result = toTaxonRuleQueryParams({ taxonId: 'abc-123' })
    expect(result.filter).toBe('taxonId=abc-123')
  })
})

describe('TAXON_RULE_TYPES', () => {
  it('contains all 10 rule types', () => {
    expect(TAXON_RULE_TYPES).toHaveLength(10)
    expect(TAXON_RULE_TYPES[0]).toBe('product_name')
    expect(TAXON_RULE_TYPES).toContain('variant_sku')
  })
})

describe('TAXON_RULE_MATCH_POLICIES', () => {
  it('contains all 14 match policies', () => {
    expect(TAXON_RULE_MATCH_POLICIES).toHaveLength(14)
    expect(TAXON_RULE_MATCH_POLICIES[0]).toBe('is_equal_to')
    expect(TAXON_RULE_MATCH_POLICIES).toContain('is_not_null')
  })
})
