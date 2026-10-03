import { z } from 'zod'

export function nameSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string().min(1, t('catalog.validation.name.required')).max(100)
}

export function presentationSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string().min(1, t('catalog.validation.presentation.required')).max(100)
}

export function positionSchema() {
  return z.number().int().min(0).default(0)
}

export function createTaxonomySchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
    name: nameSchema(t),
    presentation: presentationSchema(t),
    position: positionSchema(),
  })
}

export type TaxonomyParameters = z.infer<ReturnType<typeof createTaxonomySchema>>
