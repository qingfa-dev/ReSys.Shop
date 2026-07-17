import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export interface CreateProductRequest {
  name: string;
  slug?: string;
  description?: string;
  price: number;
  sku?: string;
  availableOn?: string;
  discontinueOn?: string;
  trackInventory?: boolean;
  weight?: number | null;
  height?: number | null;
  width?: number | null;
  depth?: number | null;
  metaTitle?: string | null;
  metaDescription?: string | null;
  metaKeywords?: string | null;
}

export interface UpdateProductRequest {
  name?: string;
  slug?: string;
  description?: string;
  price?: number;
  sku?: string;
  availableOn?: string;
  discontinueOn?: string;
  trackInventory?: boolean;
  weight?: number | null;
  height?: number | null;
  width?: number | null;
  depth?: number | null;
  metaTitle?: string | null;
  metaDescription?: string | null;
  metaKeywords?: string | null;
}

export interface ProductSearchParams extends ServerQueryingParameters {
  status?: string
  taxonId?: string
  season?: string
}
