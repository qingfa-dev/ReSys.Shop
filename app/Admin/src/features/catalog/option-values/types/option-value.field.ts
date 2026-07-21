import { z } from 'zod'

export function nameSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string()
    .min(1, t('catalog.validation.internal_name.required'))
    .max(100, t('catalog.validation.name.max_length'))
}

export function presentationSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.string()
    .min(1, t('catalog.validation.display_name.required'))
    .max(100, t('catalog.validation.display_name.max_length'))
}

export function positionSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.number()
    .int(t('catalog.validation.position.whole'))
    .min(0, t('catalog.validation.position.min'))
    .default(0)
}

export function createOptionValueSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
    name: nameSchema(t),
    presentation: presentationSchema(t),
    position: positionSchema(t),
  })
}
