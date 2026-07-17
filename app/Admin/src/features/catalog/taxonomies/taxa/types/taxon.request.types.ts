import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

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
