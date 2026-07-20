import type { TaxonomyParameters } from '../types/taxonomy.field'

export type CreateTaxonomyRequest = TaxonomyParameters

export type UpdateTaxonomyRequest = Partial<TaxonomyParameters>
