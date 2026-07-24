export interface VariantParameters {
  sku: string
  barcode?: string
  price: number
  compareAtPrice?: number | null
  costPrice?: number | null
  position: number
  trackInventory: boolean
  weight?: number | null
  height?: number | null
  width?: number | null
  depth?: number | null
}
