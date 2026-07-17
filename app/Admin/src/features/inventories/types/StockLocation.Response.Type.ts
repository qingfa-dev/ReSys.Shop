export interface StockLocation {
  id: string; name: string; code: string; active: boolean; isDefault: boolean
  type: string; city: string; countryCode: string; position?: number
  backorderableDefault?: boolean; propagateAllVariants?: boolean
  lowStockThreshold?: number; notifyOnLowStock?: boolean
}

export interface StockLocationDetail extends StockLocation {
  presentation: string | null
  address: { address1: string; address2: string | null; city: string; zipCode: string; countryCode: string; stateCode: string | null; phone: string | null; firstName: string | null; lastName: string | null; company: string | null }
  publicMetadata: Record<string, unknown>; privateMetadata: Record<string, unknown>
}
