import { z } from 'zod'

export function createTaxonomySchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  name: z.string().min(1, t('catalog.validation.name.required')).max(100),
  presentation: z.string().min(1, t('catalog.validation.presentation.required')).max(100),
  position: z.number().int().min(0).default(0),
})
}

export type TaxonomyParameters = z.infer<ReturnType<typeof createTaxonomySchema>>
