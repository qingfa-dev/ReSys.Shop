export interface VariantRequest {
  sku: string
  position: number
  trackInventory: boolean
  weight?: number | null
  weightUnit?: string | null
  height?: number | null
  width?: number | null
  depth?: number | null
  dimensionsUnit?: string | null
  price?: number | null
  costPrice?: number | null
  costCurrency?: string | null
  isMaster: boolean
  optionValueIds?: string[]
}
