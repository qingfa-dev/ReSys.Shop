import { z } from 'zod'

export function createTaxonSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  taxonomyId: z.string().uuid(t('catalog.validation.taxonomy.required')),
  name: z.string().min(1, t('catalog.validation.name.required')).max(100),
  presentation: z.string().min(1, t('catalog.validation.presentation.required')).max(100),
  description: z.string().max(500).optional().nullable(),
  slug: z.string().min(1, t('catalog.validation.slug.required')).max(100),
  position: z.number().int().min(0).default(0),
  hideFromNav: z.boolean().default(false),
  parentId: z.string().uuid().optional().nullable(),
  automatic: z.boolean().default(false),
  rulesMatchPolicy: z.enum(['all', 'any']).default('all'),
  sortOrder: z.string().default('manual'),
  metaTitle: z.string().max(100).optional().nullable(),
  metaDescription: z.string().max(255).optional().nullable(),
  metaKeywords: z.string().max(255).optional().nullable(),
})
}

export type TaxonParameters = z.infer<ReturnType<typeof createTaxonSchema>>
