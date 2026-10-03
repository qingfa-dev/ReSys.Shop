import apiClient from "@/common/api/http/api.client";
import { CATALOG } from "@/common/api/constants";
import type { ServerPagedResult, ServerResult } from "@/common/api/types/result.types";
import type { ServerQueryingParameters } from "@/common/api/types/query.types";
import type { TaxonomyDetail, TaxonomyListItem } from '../models/taxonomy.response'
import type { CreateTaxonomyRequest, UpdateTaxonomyRequest } from '../models/taxonomy.request'
import { TaxonomyMapper } from "./taxonomy.mapper";

export const taxonomyRepository = {
  list: async (params?: ServerQueryingParameters): Promise<ServerPagedResult<TaxonomyListItem>> => {
    const res = await apiClient.get(`${CATALOG}/taxonomies`, { params });
    const result = res.data as ServerPagedResult<TaxonomyListItem>;
    return { ...result, items: result.items?.map(TaxonomyMapper.toListItem) ?? [] };
  },

  getById: async (id: string): Promise<ServerResult<TaxonomyDetail | null>> => {
    const res = await apiClient.get(`${CATALOG}/taxonomies/${id}`);
    const result = res.data as ServerResult<TaxonomyDetail>;
    if (result.value) result.value = TaxonomyMapper.toDetail(result.value)
    return result;
  },

  create: async (data: CreateTaxonomyRequest): Promise<ServerResult<TaxonomyDetail | null>> => {
    const res = await apiClient.post(`${CATALOG}/taxonomies`, data);
    const result = res.data as ServerResult<TaxonomyDetail>;
    if (result.value) result.value = TaxonomyMapper.toDetail(result.value)
    return result;
  },

  update: async (id: string, data: UpdateTaxonomyRequest): Promise<ServerResult<TaxonomyDetail | null>> => {
    const res = await apiClient.put(`${CATALOG}/taxonomies/${id}`, data);
    const result = res.data as ServerResult<TaxonomyDetail>;
    if (result.value) result.value = TaxonomyMapper.toDetail(result.value)
    return result;
  },

  delete: (id: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/taxonomies/${id}`).then((res) => res.data as ServerResult<void>),

  restore: (id: string): Promise<ServerResult<void>> =>
    apiClient.patch(`${CATALOG}/taxonomies/${id}/restore`).then((res) => res.data as ServerResult<void>),
};
