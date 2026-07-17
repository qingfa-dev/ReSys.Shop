import type { ApiResult, PaginationMeta } from '@/shared/api/api.types';

export interface TaxonomyListItem {
  id: string;
  name: string;
  presentation: string | null;
  position: number;
  taxon_count: number;
}

export interface TaxonNode {
  id: string;
  name: string;
  slug: string;
  position: number;
  child: TaxonNode[];
}

export interface TaxonomyDetail extends TaxonomyListItem {
  root: TaxonNode | null;
  public_metadata: Record<string, any>;
  private_metadata: Record<string, any>;
}

export interface CreateTaxonomyRequest {
  name: string;
  presentation?: string;
  position?: number;
  public_metadata?: Record<string, any>;
  private_metadata?: Record<string, any>;
}

export interface UpdateTaxonomyRequest {
  name?: string;
  presentation?: string;
  position?: number;
  public_metadata?: Record<string, any>;
  private_metadata?: Record<string, any>;
}

export interface TaxonomyQuery {
  page?: number;
  page_size?: number;
  sort?: string;
  sort_by?: string;
  is_descending?: boolean;
  search?: string;
  filter?: string;
}
