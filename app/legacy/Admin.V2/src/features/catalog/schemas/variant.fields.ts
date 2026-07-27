import { z } from 'zod'
export type TFunction = (key: string) => string
export class VariantFields {
  constructor(private t: TFunction) {}
  sku() { return z.string().min(1, 'SKU is required') }
  position() { return z.number().min(0) }
  trackInventory() { return z.boolean().optional() }
  weight() { return z.number().optional() }
  weightUnit() { return z.string().optional() }
  height() { return z.number().optional() }
  width() { return z.number().optional() }
  depth() { return z.number().optional() }
  dimensionsUnit() { return z.string().optional() }
  price() { return z.number().optional() }
  costPrice() { return z.number().optional() }
  costCurrency() { return z.string().optional() }
  isMaster() { return z.boolean().optional() }
}
