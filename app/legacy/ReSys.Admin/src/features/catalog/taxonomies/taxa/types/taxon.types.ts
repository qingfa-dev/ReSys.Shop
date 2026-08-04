import type { ApiResult, PaginationMeta } from '@/shared/api/api.types';
import type { TaxonFormData, TaxonRuleFormData } from '../schemas/taxon.schema';

export interface TaxonRuleListItem {
  id: string;
  type: string;
  value: string;
  match_policy: string;
  property_name?: string;
}

export interface TaxonListItem {
  id: string;
  taxonomy_id: string;
  parent_id?: string;
  name: string;
  presentation: string;
  description?: string;
  slug: string;
  permalink: string;
  pretty_name: string;
  position: number;
  hide_from_nav: boolean;
  image_url?: string;
  square_image_url?: string;
  depth: number;
  product_count: number;
  child_count: number;
  has_children: boolean;
  automatic: boolean;
}

export interface TaxonTreeItem extends TaxonListItem {
  key: string;
  isExpanded?: boolean;
  children: TaxonTreeItem[];
}

export interface TaxonDetail extends TaxonListItem {
  rules_match_policy: string;
  sort_order: string;
  meta_title?: string;
  meta_description?: string;
  meta_keywords?: string;
  public_metadata: Record<string, any>;
  private_metadata: Record<string, any>;
  rules?: TaxonRuleListItem[];
}

export type CreateTaxonRuleRequest = {
  type: string;
  value: string;
  match_policy: string;
  property_name?: string | null;
};

export type UpdateTaxonRuleRequest = CreateTaxonRuleRequest;

export type CreateTaxonRequest = {
  taxonomy_id: string;
  name: string;
  presentation: string;
  description?: string | null;
  slug: string;
  position?: number;
  hide_from_nav?: boolean;
  image_url?: string | null;
  square_image_url?: string | null;
  parent_id?: string | null;
  automatic?: boolean;
  rules_match_policy?: string;
  sort_order?: string;
  meta_title?: string | null;
  meta_description?: string | null;
  meta_keywords?: string | null;
  public_metadata?: Record<string, any>;
  private_metadata?: Record<string, any>;
};

export type UpdateTaxonRequest = CreateTaxonRequest;

export interface TaxonQuery {
    taxonomy_id?: string[];
    focused_taxon_id?: string;
    include_leaves_only?: boolean;
    include_hidden?: boolean;
    max_depth?: number;
    page?: number;
    page_size?: number;
    search?: string;
}

export type { ApiResult, PaginationMeta };