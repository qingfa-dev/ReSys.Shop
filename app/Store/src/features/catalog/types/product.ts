export interface StoreProductListItemResponse {
  id: string
  masterVariantId: string
  name: string
  status: string
  description: string | null
  slug: string
  styleCode: string | null
  seasonName: string | null
  materialComposition: string | null
  careInstructions: string | null
  fitNotes: string | null
  department: string | null
  genderTarget: string | null
  variantsCount: number
  availableOn: string | null
  masterVariant: StoreProductVariantResponse | null
  taxons: StoreProductTaxonResponse[]
}

export interface StoreProductDetailResponse extends StoreProductListItemResponse {
  masterVariant: StoreProductVariantResponse | null
  variants: StoreProductVariantResponse[]
  taxons: StoreProductTaxonResponse[]
}

export interface StoreProductTaxonResponse {
  id: string
  name: string
  permalink: string
  depth: number
  breadcrumb?: Array<{ id: string; name: string; permalink: string }>
}

export interface StoreVariantStockInfo {
  availableQuantity: number
  backorderable: boolean
}

export interface StoreVariantOptionValueResponse {
  variantOptionValueId: string
  optionValueId: string
  name: string
  presentation: string | null
  position: number
  optionTypeId: string
  optionTypeName: string | null
}

export interface StoreProductVariantResponse {
  id: string
  sku: string | null
  isMaster: boolean
  price: number | null
  currency: string | null
  optionValues: StoreVariantOptionValueResponse[]
  images: StoreProductImageResponse[]
  stock: StoreVariantStockInfo
}

export interface StoreProductImageResponse {
  id: string
  url: string
  alt: string | null
  position: number
}
