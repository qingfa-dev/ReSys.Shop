import apiClient from "@/shared/api/http/api.client";
import { CATALOG } from "@/shared/api/constants";
import type { ServerPagedResult, ServerResult } from "@/shared/api/types/result.types";
import type { ServerQueryingParameters } from "@/shared/api/types/query.types";
import type {
  TaxonDetail,
  TaxonListItem,
  TaxonTreeItem,
} from "../types/Taxon.Response.Type";
import type { TaxonRuleListItem } from "../types/TaxonRule.Response.Type";
import type { CreateTaxonRequest, UpdateTaxonRequest } from "../types/Taxon.Request.Type";
import type { CreateTaxonRuleRequest, UpdateTaxonRuleRequest } from "../types/TaxonRule.Request.Type";

export const taxonRepository = {
  listByTaxonomyId: (
    taxonomyId: string,
    params?: ServerQueryingParameters & { includeLeavesOnly?: boolean },
  ): Promise<ServerPagedResult<TaxonListItem>> =>
    apiClient
      .get(`${CATALOG}/taxonomies/${taxonomyId}/taxons`, { params })
      .then((res) => res.data as ServerPagedResult<TaxonListItem>),

  getTree: (taxonomyId: string): Promise<ServerResult<TaxonTreeItem[]>> =>
    apiClient
      .get(`${CATALOG}/taxonomies/${taxonomyId}/taxons/tree`)
      .then((res) => res.data as ServerResult<TaxonTreeItem[]>),

  getById: (taxonomyId: string, taxonId: string): Promise<ServerResult<TaxonDetail>> =>
    apiClient
      .get(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}`)
      .then((res) => res.data as ServerResult<TaxonDetail>),

  create: (taxonomyId: string, data: CreateTaxonRequest): Promise<ServerResult<TaxonDetail>> =>
    apiClient
      .post(`${CATALOG}/taxonomies/${taxonomyId}/taxons`, data)
      .then((res) => res.data as ServerResult<TaxonDetail>),

  update: (
    taxonomyId: string,
    taxonId: string,
    data: UpdateTaxonRequest,
  ): Promise<ServerResult<TaxonDetail>> =>
    apiClient
      .put(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}`, data)
      .then((res) => res.data as ServerResult<TaxonDetail>),

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

  listRules: (taxonomyId: string, taxonId: string): Promise<ServerPagedResult<TaxonRuleListItem>> =>
    apiClient
      .get(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/rules`)
      .then((res) => res.data as ServerPagedResult<TaxonRuleListItem>),

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
};
