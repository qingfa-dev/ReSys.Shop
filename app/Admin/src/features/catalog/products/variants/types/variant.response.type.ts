export interface VariantOption { name: string; value: string }

export interface VariantSummary {
  id: string; productId: string; sku: string | null; barcode: string | null
  price: number; compareAtPrice: number | null; costPrice: number | null
  costCurrency?: string; isMaster: boolean; position: number; trackInventory: boolean
  weightUnit?: string; dimensionsUnit?: string; options: VariantOption[]
}

export interface VariantDetail extends VariantSummary {
  weight: number | null; height: number | null; width: number | null; depth: number | null
  optionValueIds: string[]
}
