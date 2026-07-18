export interface VariantSummary {
  id: string; productId: string; sku: string | null
  price: number; costPrice: number | null; costCurrency: string
  isMaster: boolean; position: number; trackInventory: boolean
  weightUnit: string; dimensionsUnit: string
}

export interface VariantDetail extends VariantSummary {
  weight: number | null; height: number | null; width: number | null; depth: number | null
  pricesCount: number; discontinuedOn: string | null
}
