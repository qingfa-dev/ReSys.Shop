import type { CreateTaxonomyForm, UpdateTaxonomyForm } from '../schemas'

export type CreateTaxonomyRequest = CreateTaxonomyForm
export type UpdateTaxonomyRequest = UpdateTaxonomyForm

export interface TaxonomyListParams {
  page?: number
  pageSize?: number
  search?: string
}
