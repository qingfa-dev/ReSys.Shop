import type { TaxonomyParameters } from '../schemas/taxonomy.schema'

export type CreateTaxonomyRequest = TaxonomyParameters

export type UpdateTaxonomyRequest = Partial<TaxonomyParameters>
