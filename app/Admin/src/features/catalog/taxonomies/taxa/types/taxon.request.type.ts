import type { TaxonParameters } from '../../schemas/Taxon.Schema'
import type { TaxonRuleParameters } from './TaxonRule.Parameters.Type'

export type CreateTaxonRequest = TaxonParameters & {
  rules?: TaxonRuleParameters[]
}

export type UpdateTaxonRequest = CreateTaxonRequest
