export interface ProductSummary {
  id: string
  name: string
  slug: string
  description: string | null
  masterVariantId: string
  status: number
  availableOn: string | null
  discontinueOn: string | null
  trackInventory: boolean
  variantsCount: number
  createdAtUtc: string
  modifiedAtUtc: string | null
}

export interface ProductDetail extends ProductSummary {
  metaTitle: string | null
  metaDescription: string | null
  metaKeywords: string | null
}
