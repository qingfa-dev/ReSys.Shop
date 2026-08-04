export interface TaxonRuleRequest {
  type: string
  matchPolicy: string
  value: string
}

export interface SyncRuleItem {
  id?: string
  type: string
  matchPolicy: string
  value: string
}

export interface SyncTaxonRulesRequest {
  rules: SyncRuleItem[]
}
