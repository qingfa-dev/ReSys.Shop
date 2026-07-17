export type ProductStatus = 'Draft' | 'Active' | 'Archived'

export interface ProductImage {
  id: string; productId: string; variantId: string | null; url: string
  alt: string | null; position: number; role: number; fileSize: number | null
  width: number | null; height: number | null; isDefault: boolean
}

export interface ProductClassification {
  id: string; productId: string; taxonId: string; position: number
  isAutomatic: boolean; isMain: boolean; taxonName?: string; taxonomyName?: string
}

export interface ProductProperty {
  id: string; propertyTypeId: string; propertyTypeName: string
  propertyTypePresentation: string; value: string
}

export interface ProductSummary {
  id: string; name: string; slug: string; description: string | null
  sku: string | null; price: number; status: ProductStatus
  imageUrl: string | null; variantsCount: number
  createdAtUtc: string; modifiedAtUtc: string | null
}

export interface ProductDetail extends ProductSummary {
  metaTitle: string | null; metaDescription: string | null; metaKeywords: string | null
  weight: number | null; height: number | null; width: number | null; depth: number | null
  variants: VariantSummary[]; classifications: ProductClassification[]
  properties: ProductProperty[]; images: ProductImage[]
}

import type { VariantSummary } from './Variant.Response.Type'
