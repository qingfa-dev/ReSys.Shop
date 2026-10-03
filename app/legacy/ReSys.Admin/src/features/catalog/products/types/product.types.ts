import type { VariantSummary } from './variant.types';

export interface ProductImage {
  id: string;
  product_id: string;
  variant_id: string | null;
  url: string;
  alt: string | null;
  position: number;
  role: number;
  status: 'Pending' | 'Processing' | 'Processed' | 'Failed';
  file_size: number | null;
  width: number | null;
  height: number | null;
  is_default: boolean; 
}

export interface ProductClassification {
  id: string;
  product_id: string;
  taxon_id: string;
  position: number;
  is_automatic: boolean;
  is_main: boolean;
  taxon_name?: string;
  taxonomy_name?: string;
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
  is_active: boolean;
  is_visible: boolean;
  is_digital: boolean;
  image_url: string | null;
  variant_count: number;
  created_at: string;
  updated_at: string | null;
}

export interface ProductDetail extends ProductSummary {
  meta_title: string | null;
  meta_description: string | null;
  meta_keywords: string | null;
  weight: number | null;
  height: number | null;
  width: number | null;
  depth: number | null;
  brand: string | null;
  public_metadata: Record<string, any>;
  private_metadata: Record<string, any>;
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
  is_active: boolean;
  is_visible: boolean;
  weight?: number | null;
  height?: number | null;
  width?: number | null;
  depth?: number | null;
  brand?: string | null;
  meta_title?: string | null;
  meta_description?: string | null;
  meta_keywords?: string | null;
}

export interface UpdateProductRequest {
  name?: string;
  slug?: string;
  description?: string;
  price?: number;
  sku?: string;
  is_active?: boolean;
  is_visible?: boolean;
  weight?: number | null;
  height?: number | null;
  width?: number | null;
  depth?: number | null;
  brand?: string | null;
  meta_title?: string | null;
  meta_description?: string | null;
  meta_keywords?: string | null;
}

export interface ProductSearchParams {
  page?: number;
  page_size?: number;
  search?: string;
  is_active?: boolean;
  sort_by?: string;
  is_descending?: boolean;
  filter?: string;
}
