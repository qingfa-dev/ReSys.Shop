import apiClient from "@/common/api/http/api.client";
import { CATALOG } from "@/common/api/constants";
import type { ServerPagedResult, ServerResult } from "@/common/api/types/result.types";
import type { ServerQueryingParameters } from "@/common/api/types/query.types";
import type { TaxonomyDetail, TaxonomyListItem } from '../models/taxonomy.response'
import type {
  CreateTaxonomyRequest,
  UpdateTaxonomyRequest,
} from '../models/taxonomy.request'
export const taxonomyRepository = {
  list: async (params?: ServerQueryingParameters): Promise<ServerPagedResult<TaxonomyListItem>> => {
    return apiClient
      .get(`${CATALOG}/taxonomies`, { params })
      .then((res) => res.data as ServerPagedResult<TaxonomyListItem>);
  },

  getById: async (id: string): Promise<ServerResult<TaxonomyDetail | null>> => {
    return apiClient
      .get(`${CATALOG}/taxonomies/${id}`)
      .then((res) => res.data as ServerResult<TaxonomyDetail>);
  },

  create: async (data: CreateTaxonomyRequest): Promise<ServerResult<TaxonomyDetail | null>> => {
    return apiClient
      .post(`${CATALOG}/taxonomies`, data)
      .then((res) => res.data as ServerResult<TaxonomyDetail>);
  },

  update: async (id: string, data: UpdateTaxonomyRequest): Promise<ServerResult<TaxonomyDetail | null>> => {
    return apiClient
      .put(`${CATALOG}/taxonomies/${id}`, data)
      .then((res) => res.data as ServerResult<TaxonomyDetail>);
  },

  delete: (id: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/taxonomies/${id}`).then((res) => res.data as ServerResult<void>),

  restore: (id: string): Promise<ServerResult<void>> =>
    apiClient
      .patch(`${CATALOG}/taxonomies/${id}/restore`)
      .then((res) => res.data as ServerResult<void>),
};
