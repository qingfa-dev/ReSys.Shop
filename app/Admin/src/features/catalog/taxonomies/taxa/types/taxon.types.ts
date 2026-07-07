import type { ApiResult } from '@/shared/api/types/api.types';
import type { PaginationMeta } from '@/shared/api/types/result.types';
import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types';
import type { TaxonFormData, TaxonRuleFormData } from '../schemas/taxon.schema';

export interface TaxonRuleListItem {
  id: string;
  taxonId: string;
  type: string;
  value: string;
  matchPolicy: string;
}

export interface TaxonListItem {
  id: string;
  taxonomyId: string;
  parentId?: string;
  name: string;
  presentation: string;
  description?: string;
  slug: string;
  permalink: string;
  prettyName: string;
  position: number;
  hideFromNav: boolean;
  depth: number;
  productCount: number;
  childrenCount: number;
  lft: number;
  rgt: number;
  hasChildren: boolean;
  automatic: boolean;
  createdAtUtc: string;
  modifiedAtUtc: string;
}

export interface TaxonTreeItem extends TaxonListItem {
  key: string;
  isExpanded?: boolean;
  children: TaxonTreeItem[];
}

export interface TaxonDetail extends TaxonListItem {
  rulesMatchPolicy: string;
  sortOrder: string;
  metaTitle?: string;
  metaDescription?: string;
  metaKeywords?: string;
  taxonRuleCount: number;
  rules?: TaxonRuleListItem[];
}

export type CreateTaxonRuleRequest = {
  type: string;
  value: string;
  matchPolicy: string;
};

export type UpdateTaxonRuleRequest = CreateTaxonRuleRequest;

export type CreateTaxonRequest = {
  taxonomyId: string;
  name: string;
  presentation: string;
  description?: string | null;
  slug: string;
  position?: number;
  hideFromNav?: boolean;
  parentId?: string | null;
  automatic?: boolean;
  rulesMatchPolicy?: string;
  sortOrder?: string;
  metaTitle?: string | null;
  metaDescription?: string | null;
  metaKeywords?: string | null;
};

export type UpdateTaxonRequest = CreateTaxonRequest;

export interface TaxonQuery extends ServerQueryingParameters {
  taxonomyId?: string[]
  focusedTaxonId?: string
  includeLeavesOnly?: boolean
  includeHidden?: boolean
  maxDepth?: number
}

export type { ApiResult, PaginationMeta };