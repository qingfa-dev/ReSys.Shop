import type { TaxonParameters } from './taxon.parameters'
import type { TaxonRuleParameters } from './taxon-rule.parameters'

export type CreateTaxonRequest = TaxonParameters & { rules?: TaxonRuleParameters[] }
export type UpdateTaxonRequest = CreateTaxonRequest
