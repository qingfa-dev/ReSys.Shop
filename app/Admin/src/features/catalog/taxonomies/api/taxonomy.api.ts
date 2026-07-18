import apiClient from "@/shared/api/http/api.client";
import { CATALOG } from "@/shared/api/constants";
import type { ServerPagedResult, ServerResult } from "@/shared/api/types/result.types";
import type { ServerQueryingParameters } from "@/shared/api/types/query.types";
import type { TaxonomyDetail, TaxonomyListItem } from "../types/Taxonomy.Response.Type";
import type {
  CreateTaxonomyRequest,
  UpdateTaxonomyRequest,
} from "../types/Taxonomy.Request.Type";

export const taxonomyRepository = {
  list: (params?: ServerQueryingParameters): Promise<ServerPagedResult<TaxonomyListItem>> =>
    apiClient
      .get(`${CATALOG}/taxonomies`, { params })
      .then((res) => res.data as ServerPagedResult<TaxonomyListItem>),

  getById: (id: string): Promise<ServerResult<TaxonomyDetail>> =>
    apiClient
      .get(`${CATALOG}/taxonomies/${id}`)
      .then((res) => res.data as ServerResult<TaxonomyDetail>),

  create: (data: CreateTaxonomyRequest): Promise<ServerResult<TaxonomyDetail>> =>
    apiClient
      .post(`${CATALOG}/taxonomies`, data)
      .then((res) => res.data as ServerResult<TaxonomyDetail>),

  update: (id: string, data: UpdateTaxonomyRequest): Promise<ServerResult<TaxonomyDetail>> =>
    apiClient
      .put(`${CATALOG}/taxonomies/${id}`, data)
      .then((res) => res.data as ServerResult<TaxonomyDetail>),

  delete: (id: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/taxonomies/${id}`).then((res) => res.data as ServerResult<void>),

  restore: (id: string): Promise<ServerResult<void>> =>
    apiClient
      .patch(`${CATALOG}/taxonomies/${id}/restore`)
      .then((res) => res.data as ServerResult<void>),
};
