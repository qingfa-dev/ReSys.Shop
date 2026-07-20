import apiClient from "@/common/api/http/api.client";
import { CATALOG } from "@/common/api/constants";
import type { ServerPagedResult, ServerResult } from "@/common/api/types/result.types";
import type { ServerQueryingParameters } from "@/common/api/types/query.types";
import type {
  TaxonDetail,
  TaxonListItem,
  TaxonTreeItem,
} from "../types/taxon.response.type";
import type { TaxonRuleListItem } from "../types/taxon-rule.response.type";
import type { CreateTaxonRequest, UpdateTaxonRequest } from "../types/taxon.request.type";
import type { CreateTaxonRuleRequest, UpdateTaxonRuleRequest } from "../types/taxon-rule.request.type";
import type { ProductSummaryModel } from '../../../products/types/product.model.type'

export const taxonRepository = {
  listByTaxonomyId: async (
    taxonomyId: string,
    params?: ServerQueryingParameters & { includeLeavesOnly?: boolean },
  ): Promise<ServerPagedResult<TaxonListItem>> => {
    return apiClient
      .get(`${CATALOG}/taxonomies/${taxonomyId}/taxons`, { params })
      .then((res) => res.data as ServerPagedResult<TaxonListItem>);
  },

  getTree: async (taxonomyId: string): Promise<ServerResult<TaxonTreeItem | null>> => {
    return apiClient
      .get(`${CATALOG}/taxonomies/${taxonomyId}/taxons/tree`)
      .then((res) => res.data as ServerResult<TaxonTreeItem>);
  },

  getById: async (taxonomyId: string, taxonId: string): Promise<ServerResult<TaxonDetail | null>> => {
    return apiClient
      .get(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}`)
      .then((res) => res.data as ServerResult<TaxonDetail>);
  },

  create: async (taxonomyId: string, data: CreateTaxonRequest): Promise<ServerResult<TaxonDetail | null>> => {
    return apiClient
      .post(`${CATALOG}/taxonomies/${taxonomyId}/taxons`, data)
      .then((res) => res.data as ServerResult<TaxonDetail>);
  },

  update: async (
    taxonomyId: string,
    taxonId: string,
    data: UpdateTaxonRequest,
  ): Promise<ServerResult<TaxonDetail | null>> => {
    return apiClient
      .put(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}`, data)
      .then((res) => res.data as ServerResult<TaxonDetail>);
  },

  delete: (taxonomyId: string, taxonId: string): Promise<ServerResult<void>> =>
    apiClient
      .delete(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}`)
      .then((res) => res.data as ServerResult<void>),

  reposition: (
    taxonomyId: string,
    taxonId: string,
    position: number,
  ): Promise<ServerResult<void>> =>
    apiClient
      .post(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/reposition`, { position })
      .then((res) => res.data as ServerResult<void>),

  restore: (taxonomyId: string, taxonId: string): Promise<ServerResult<void>> =>
    apiClient
      .patch(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/restore`)
      .then((res) => res.data as ServerResult<void>),

  listRules: async (taxonomyId: string, taxonId: string): Promise<ServerPagedResult<TaxonRuleListItem>> => {
    return apiClient
      .get(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/rules`)
      .then((res) => res.data as ServerPagedResult<TaxonRuleListItem>);
  },

  createRule: (
    taxonomyId: string,
    taxonId: string,
    data: CreateTaxonRuleRequest,
  ): Promise<ServerResult<TaxonRuleListItem>> =>
    apiClient
      .post(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/rules`, data)
      .then((res) => res.data as ServerResult<TaxonRuleListItem>),

  updateRule: (
    taxonomyId: string,
    taxonId: string,
    ruleId: string,
    data: UpdateTaxonRuleRequest,
  ): Promise<ServerResult<TaxonRuleListItem>> =>
    apiClient
      .put(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/rules/${ruleId}`, data)
      .then((res) => res.data as ServerResult<TaxonRuleListItem>),

  deleteRule: (taxonomyId: string, taxonId: string, ruleId: string): Promise<ServerResult<void>> =>
    apiClient
      .delete(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/rules/${ruleId}`)
      .then((res) => res.data as ServerResult<void>),

  syncRules: (
    taxonomyId: string,
    taxonId: string,
    rules: CreateTaxonRuleRequest[],
  ): Promise<ServerResult<void>> =>
    apiClient
      .post(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/rules/sync`, rules)
      .then((res) => res.data as ServerResult<void>),

  regenerateProducts: (taxonomyId: string, taxonId: string): Promise<ServerResult<void>> =>
    apiClient
      .post(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/rules/regenerate`)
      .then((res) => res.data as ServerResult<void>),

  getProductPreview: async (_taxonId: string, _params: Record<string, unknown>): Promise<ServerResult<{ items: ProductSummaryModel[]; totalCount: number }>> => ({
    isSuccess: true,
    statusCode: 200,
    errors: [],
    message: null,
    metadata: null,
    value: { items: [], totalCount: 0 },
  }),
};
