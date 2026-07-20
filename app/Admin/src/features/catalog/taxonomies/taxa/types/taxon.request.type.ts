import type { TaxonParameters } from '../../schemas/taxon.schema'
import type { TaxonRuleParameters } from './taxon-rule.parameters.type'

export type CreateTaxonRequest = TaxonParameters & {
  rules?: TaxonRuleParameters[]
}

export type UpdateTaxonRequest = CreateTaxonRequest
