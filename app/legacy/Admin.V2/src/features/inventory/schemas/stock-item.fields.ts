import { z } from 'zod'
import type { TFunction } from './stock-location.fields'

export class StockItemFields {
  constructor(private t: TFunction) {}
  variantId() { return z.string().min(1, 'Variant is required') }
  locationId() { return z.string().min(1, 'Location is required') }
  quantity() { return z.coerce.number().int().min(0, 'Quantity must be 0 or more') }
  lowStockThreshold() { return z.coerce.number().int().min(0).optional() }
}
