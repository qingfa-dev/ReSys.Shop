export interface TaxonRuleDetailResponse {
  id: string
  taxonId: string
  type: string
  matchPolicy: string
  value: string
}

export interface TaxonRuleListItem {
  id: string
  type: string
  matchPolicy: string
  value: string
}

export interface SyncTaxonRulesResponse {
  rules: TaxonRuleListItem[]
}
