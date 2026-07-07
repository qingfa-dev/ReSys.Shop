import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types'
import type { VariantSummary } from './variant.types';

export type ProductStatus = 'Draft' | 'Active' | 'Archived';
export type VariantImageType = 'Default' | 'Thumbnail' | 'Square' | 'Gallery' | 'Search';

export interface ProductImage {
  id: string;
  productId: string;
  variantId: string | null;
  url: string;
  alt: string | null;
  position: number;
  role: number;
  status: VariantImageType | null;
  fileSize: number | null;
  width: number | null;
  height: number | null;
  isDefault: boolean;
}

export interface ProductClassification {
  id: string;
  productId: string;
  taxonId: string;
  position: number;
  isAutomatic: boolean;
  isMain: boolean;
  taxonName?: string;
  taxonomyName?: string;
}

export interface ProductProperty {
  id: string;
  property_type_id: string;
  property_type_name: string;
  property_type_presentation: string;
  value: string;
}

export interface ProductSummary {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  sku: string | null;
  price: number;
  status: ProductStatus;
  imageUrl: string | null;
  variantsCount: number;
  createdAtUtc: string;
  modifiedAtUtc: string | null;
}

export interface ProductDetail extends ProductSummary {
  metaTitle: string | null;
  metaDescription: string | null;
  metaKeywords: string | null;
  weight: number | null;
  height: number | null;
  width: number | null;
  depth: number | null;
  variants: VariantSummary[];
  classifications: ProductClassification[];
  properties: ProductProperty[];
  images: ProductImage[];
}

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
