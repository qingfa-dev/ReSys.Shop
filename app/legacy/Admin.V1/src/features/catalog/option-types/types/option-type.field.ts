import { z } from 'zod'

export function nameSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string()
    .min(1, t('catalog.validation.name.required'))
    .max(100, t('catalog.validation.name.max_length'))
}

export function presentationSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string()
    .min(1, t('catalog.validation.presentation.required'))
    .max(100, t('catalog.validation.presentation.max_length'))
}

export function positionSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.number()
    .int(t('catalog.validation.position.whole'))
    .min(-1, t('catalog.validation.position.min'))
    .default(1)
}

export function filterableSchema() {
  return z.boolean().default(false)
}

export function createOptionTypeSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
    name: nameSchema(t),
    presentation: presentationSchema(t),
    filterable: filterableSchema(),
    position: positionSchema(t),
  })
}
