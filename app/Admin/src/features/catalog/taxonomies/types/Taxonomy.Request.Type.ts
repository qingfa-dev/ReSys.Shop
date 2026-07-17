import type { TaxonomyParameters } from '../schemas/Taxonomy.Schema'

export type CreateTaxonomyRequest = TaxonomyParameters

export type UpdateTaxonomyRequest = Partial<TaxonomyParameters>
