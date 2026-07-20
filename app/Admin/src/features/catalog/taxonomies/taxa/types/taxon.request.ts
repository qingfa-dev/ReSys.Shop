import type { TaxonParameters } from '../../types/taxon.field'
import type { TaxonRuleParameters } from './taxon-rule.parameters'

export type CreateTaxonRequest = TaxonParameters & {
  rules?: TaxonRuleParameters[]
}

export type UpdateTaxonRequest = CreateTaxonRequest
