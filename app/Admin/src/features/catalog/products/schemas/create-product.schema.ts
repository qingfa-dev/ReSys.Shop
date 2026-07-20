import { z } from 'zod'

export function createCreateProductSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  name: z.string().min(1, t('catalog.validation.name.required')).max(200, t('catalog.validation.name.max_length')),
  slug: z.string().min(1, t('catalog.validation.slug.required')).max(200, t('catalog.validation.slug.max_length')).regex(/^[a-z0-9-]+$/, t('catalog.validation.slug.format')),
  description: z.string().optional(),
  price: z.number().min(0, t('catalog.validation.price.min')),
  sku: z.string().max(100, t('catalog.validation.sku.max_length')).optional(),
  availableOn: z.string().optional(),
  discontinueOn: z.string().optional(),
  trackInventory: z.boolean().default(true),
  weight: z.number().min(0, t('catalog.validation.weight.min')).optional().nullable(),
  height: z.number().min(0, t('catalog.validation.height.min')).optional().nullable(),
  width: z.number().min(0, t('catalog.validation.width.min')).optional().nullable(),
  depth: z.number().min(0, t('catalog.validation.depth.min')).optional().nullable(),
  metaTitle: z.string().max(60, t('catalog.validation.meta_title.max_length')).optional().nullable(),
  metaDescription: z.string().max(160, t('catalog.validation.meta_description.max_length')).optional().nullable(),
  metaKeywords: z.string().max(255, t('catalog.validation.meta_keywords.max_length')).optional().nullable(),
})
}

export type CreateProductParameters = z.infer<ReturnType<typeof createCreateProductSchema>>
