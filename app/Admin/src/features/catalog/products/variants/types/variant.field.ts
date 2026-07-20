import { z } from 'zod'

export function createVariantSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  sku: z.string().min(1, t('catalog.validation.sku.required')).max(100, t('catalog.validation.sku.max_length')),
  barcode: z.string().max(50, t('catalog.validation.barcode.max_length')).optional(),
  price: z.number().min(0, t('catalog.validation.price.min')).default(0),
  compareAtPrice: z.number().min(0, t('catalog.validation.compare_at_price.min')).optional().nullable(),
  costPrice: z.number().min(0, t('catalog.validation.cost_price.min')).optional().nullable(),
  position: z.number().int(t('catalog.validation.position.whole')).min(0, t('catalog.validation.position.min')).default(0),
  trackInventory: z.boolean().default(true),
  weight: z.number().min(0, t('catalog.validation.weight.min')).optional().nullable(),
  height: z.number().min(0, t('catalog.validation.height.min')).optional().nullable(),
  width: z.number().min(0, t('catalog.validation.width.min')).optional().nullable(),
  depth: z.number().min(0, t('catalog.validation.depth.min')).optional().nullable(),
  optionValueIds: z.array(z.string().uuid()).optional(),
})
}

export type VariantParameters = z.infer<ReturnType<typeof createVariantSchema>>
