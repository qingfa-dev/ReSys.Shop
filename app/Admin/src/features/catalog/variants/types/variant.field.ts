import { z } from 'zod'

export function skuSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string().min(1, t('catalog.validation.sku.required')).max(100, t('catalog.validation.sku.max_length'))
}

export function barcodeSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string().max(50, t('catalog.validation.barcode.max_length')).optional()
}

export function priceSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.number().min(0, t('catalog.validation.price.min')).default(0)
}

export function compareAtPriceSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.number().min(0, t('catalog.validation.compare_at_price.min')).optional().nullable()
}

export function costPriceSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.number().min(0, t('catalog.validation.cost_price.min')).optional().nullable()
}

export function positionSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.number().int(t('catalog.validation.position.whole')).min(0, t('catalog.validation.position.min')).default(0)
}

export function trackInventorySchema() {
  return z.boolean().default(true)
}

export function weightSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.number().min(0, t('catalog.validation.weight.min')).optional().nullable()
}

export function heightSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.number().min(0, t('catalog.validation.height.min')).optional().nullable()
}

export function widthSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.number().min(0, t('catalog.validation.width.min')).optional().nullable()
}

export function depthSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.number().min(0, t('catalog.validation.depth.min')).optional().nullable()
}

export function optionValueIdsSchema() {
  return z.array(z.string().uuid()).optional()
}

export function createVariantSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
    sku: skuSchema(t),
    barcode: barcodeSchema(t),
    price: priceSchema(t),
    compareAtPrice: compareAtPriceSchema(t),
    costPrice: costPriceSchema(t),
    position: positionSchema(t),
    trackInventory: trackInventorySchema(),
    weight: weightSchema(t),
    height: heightSchema(t),
    width: widthSchema(t),
    depth: depthSchema(t),
    optionValueIds: optionValueIdsSchema(),
  })
}

export type VariantParameters = z.infer<ReturnType<typeof createVariantSchema>>
