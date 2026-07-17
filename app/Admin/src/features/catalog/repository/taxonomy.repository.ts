import apiClient from '@/shared/api/http/api.client'
import { CATALOG } from '@/shared/api/constants'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { TaxonomyDetail, TaxonomyListItem } from '../taxonomies/types/taxonomy.domain.types'
import type { CreateTaxonomyRequest, UpdateTaxonomyRequest } from '../taxonomies/types/taxonomy.request.types'

export const taxonomyRepository = {
  list: (params?: ServerQueryingParameters): Promise<ServerResult<TaxonomyListItem[]>> =>
    apiClient.get(`${CATALOG}/taxonomies`, { params }).then(res => res.data as ServerResult<TaxonomyListItem[]>),

  getById: (id: string): Promise<ServerResult<TaxonomyDetail>> =>
    apiClient.get(`${CATALOG}/taxonomies/${id}`).then(res => res.data as ServerResult<TaxonomyDetail>),

  create: (data: CreateTaxonomyRequest): Promise<ServerResult<TaxonomyDetail>> =>
    apiClient.post(`${CATALOG}/taxonomies`, data).then(res => res.data as ServerResult<TaxonomyDetail>),

  update: (id: string, data: UpdateTaxonomyRequest): Promise<ServerResult<TaxonomyDetail>> =>
    apiClient.put(`${CATALOG}/taxonomies/${id}`, data).then(res => res.data as ServerResult<TaxonomyDetail>),

  delete: (id: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/taxonomies/${id}`).then(res => res.data as ServerResult<void>),

  restore: (id: string): Promise<ServerResult<void>> =>
    apiClient.patch(`${CATALOG}/taxonomies/${id}/restore`).then(res => res.data as ServerResult<void>),
}
