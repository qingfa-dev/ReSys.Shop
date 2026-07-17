import type { ApiResult } from '@/shared/api/types/api.types';
import type { PaginationMeta } from '@/shared/api/types/result.types';
import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types';

export interface TaxonomyListItem {
  id: string;
  name: string;
  presentation: string | null;
  position: number;
  taxonsCount: number;
  createdAtUtc: string;
  modifiedAtUtc: string;
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
}

export interface CreateTaxonomyRequest {
  name: string;
  presentation?: string;
  position?: number;
}

export interface UpdateTaxonomyRequest {
  name?: string;
  presentation?: string;
  position?: number;
}

export type TaxonomyQuery = ServerQueryingParameters
