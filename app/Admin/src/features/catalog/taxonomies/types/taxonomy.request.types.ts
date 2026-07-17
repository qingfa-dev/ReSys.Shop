import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

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
